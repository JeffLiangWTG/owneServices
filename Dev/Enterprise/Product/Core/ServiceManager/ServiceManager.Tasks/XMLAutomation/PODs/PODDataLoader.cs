using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class PODDataLoader : DataLoadWithFlexibleColumns
	{
		public void ImportPODData(string dataLocation)
		{
			ImportData(dataLocation, "POD");
		}

		class FieldNames
		{
			public const string JobNumber = "JOBNUMBER";
			public const string DeliveryDateTime = "DELIVERYDATETIME";
			public const string Signature = "SIGNATURE";
			public const string DeliveredPacks = "DELIVEREDPACKS";
			public const string DeliveredWeight = "DELIVEREDWEIGHT";
			public const string DeliveredVolume = "DELIVEREDVOLUME";

			public const string Seperator = ",";

			public static string[] MandatoryFields { get { return new[] { JobNumber, DeliveryDateTime }; } }
			public static string[] OptionalFields { get { return new[] { Signature, DeliveredPacks, DeliveredWeight, DeliveredVolume }; } }
		}

		public override string CSVTemplateHeading
		{
			get
			{
				return
					string.Join(FieldNames.Seperator, FieldNames.MandatoryFields) +
					FieldNames.Seperator +
					string.Join(FieldNames.Seperator, FieldNames.OptionalFields);
			}
		}

		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			try
			{
				ResetCounter();
				CheckLine(line);
				ProcessLine(line);
			}
			catch (ArgumentException argumentException)
			{
				RunCounters.RecsExcluded++;
				DisplayLogMessage("Row " + RunCounters.CurrentRow.ToString(CultureInfo.InvariantCulture) + " ignored: " + argumentException.Message);
			}
			catch (ZSaveException saveException)
			{
				RunCounters.RecsExcluded++;
				DisplayLogMessage("Row " + RunCounters.CurrentRow.ToString(CultureInfo.InvariantCulture) + " ignored: " + saveException.Message);
			}

			OnProgressChanged();
		}

		#region Validation and Checks

		protected override bool CheckMandatoryFields(Dictionary<string, bool> headings, string[] headerLineValues)
		{
			bool result = true;
			foreach (string mandatoryField in FieldNames.MandatoryFields)
			{
				if (!headings.ContainsKey(mandatoryField.Trim()))
				{
					DisplayLogMessage("Does not contain mandatory column heading: " + mandatoryField);
					result = false;
				}
			}
			return result;
		}

		void CheckLine(OCsvLine line)
		{
			CheckEmptyLine(line);
			CheckElementCount(line);
		}

		void CheckEmptyLine(OCsvLine line)
		{
			if (line.FieldValues.Length == 0)
			{
				throw new ArgumentException("Empty line.");
			}
		}

		ZString GetAndValidateJobNumber(ZString value)
		{
			if (!value.StartsWith("S") && !value.StartsWith("B"))
			{
				throw new ArgumentException("Job Number '" + value + "' is invalid. The Job Number should begin with 'S' or 'B'.");
			}
			return value;
		}

		ZDateTime GetAndValidateDeliveryDateTime(ZString value)
		{
			ZDateTime result = ZDateTime.Empty;
			if (value.IsEmpty || !ZDateTime.TryParseExact(value, out result, "yyyyMMddHHmmss") || !result.IsValidSmallDateTime)
			{
				throw new ArgumentException("Delivery Date Time '" + value + "' is invalid. The Delivery Date Time is mandatory and needs to be provided in the format 'yyyyMMddHHmmss'.");
			}

			return result;
		}

		#endregion

		#region Update

		void ProcessLine(OCsvLine line)
		{
			var jobNumber = GetAndValidateJobNumber(TryGetStringValue(line, FieldNames.JobNumber));
			var deliveryDateTime = GetAndValidateDeliveryDateTime(TryGetStringValue(line, FieldNames.DeliveryDateTime));
			var signature = TryGetStringValue(line, FieldNames.Signature);
			TryGetValue(line, FieldNames.DeliveredPacks, out ZInt deliveredPacks);
			var shipment = GetShipment(jobNumber);
			var declaration = GetDeclaration(jobNumber);

			if (shipment != null)
			{
				if (shipment.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Delivery))
				{
					UpdateContainerizedConfirms(shipment, signature, deliveryDateTime);
				}
				else
				{
					bool splitSpecified = !deliveredPacks.IsEmpty;

					if (shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual))
					{
						throw new ArgumentException("Shipment '" + jobNumber + "' is already complete.");
					}
					else if (!splitSpecified)
					{
						UpdateSplitSpecified(shipment, signature, deliveryDateTime);
					}
					else
					{
						var deliveredWeight = TryGetDecimalValue(line, FieldNames.DeliveredWeight, JobTransportLegPackLineDivotSchema.J8_DeliveryWeight.Precision, JobTransportLegPackLineDivotSchema.J8_DeliveryWeight.Scale);
						var deliveredVolume = TryGetDecimalValue(line, FieldNames.DeliveredVolume, JobTransportLegPackLineDivotSchema.J8_DeliveryVolume.Precision, JobTransportLegPackLineDivotSchema.J8_DeliveryVolume.Scale);
						UpdateRemainingPacks(shipment, signature, deliveryDateTime, deliveredPacks, deliveredWeight, deliveredVolume);
					}
				}

				AddDeliveryDateUpdatedEventTo(shipment.Logs);
				PerformSave();
			}
			else if (declaration != null)
			{
				UpdateDeclarationConfirms(deliveryDateTime, declaration);
			}
			else
			{
				throw new ArgumentException("Job # '" + jobNumber + "' could not be found in " + Core.Constants.ProductName + ".");
			}
		}

		void UpdateRemainingPacks(ForwardingShipment shipment, ZString signature, ZDateTime deliveryDateTime, ZInt deliveredPacks, ZDecimal deliveredWeight, ZDecimal deliveredVolume)
		{
			var jobNumber = shipment.JS_UniqueConsignRef;
			CommonPickupDeliveryConfirm matchingConfirm = null;
			foreach (CommonPickupDeliveryConfirm confirm in shipment.DeliveryConfirms)
			{
				if (confirm.EU_PickupDeliveryTime.IsEmpty
					&& !deliveredPacks.IsEmpty && deliveredPacks == confirm.TotalDeliveredPackages
					&& !deliveredWeight.IsEmpty && deliveredWeight == confirm.TotalDeliveredWeight
					&& !deliveredVolume.IsEmpty && deliveredVolume == confirm.TotalDeliveredVolume)
				{
					matchingConfirm = confirm;
					UpdateConfirm(matchingConfirm, shipment, deliveryDateTime, signature);
					PODsUpdated++;
					break;
				}
			}

			if (matchingConfirm == null && shipment.OuterPackLines.Count == 0)
			{
				throw new ArgumentException("Shipment '" + jobNumber + "' doesn't have any Outer Packlines.");
			}
			else if (matchingConfirm == null)
			{
				var remainingPacks = deliveredPacks;
				var remainingWeight = deliveredWeight;
				var remainingVolume = deliveredVolume;

				var shipmentDeliveryConfirms = shipment.DeliveryConfirms.ToArray();
				var sortedDeliveryConfirms = shipmentDeliveryConfirms.OrderBy(x => x.TotalDeliveredPackages);

				foreach (CommonPickupDeliveryConfirm confirm in sortedDeliveryConfirms)
				{
					if (confirm.EU_PickupDeliveryTime.IsEmpty && confirm.TotalDeliveredPackages > 0)
					{
						if (confirm.TotalDeliveredPackages > remainingPacks)
						{
							UpdateConfirm(confirm, shipment, deliveryDateTime, signature, confirm.TotalDeliveredPackages, confirm.TotalDeliveredWeight, confirm.TotalDeliveredVolume);
							PODsCreated++;
							CommonPickupDeliveryConfirm splitConfirm = shipment.DeliveryConfirms.AddNew();
							splitConfirm.TotalDeliveredPackages = confirm.TotalDeliveredPackages - remainingPacks;
							splitConfirm.TotalDeliveredWeight = confirm.TotalDeliveredWeight - remainingWeight;
							splitConfirm.TotalDeliveredVolume = confirm.TotalDeliveredVolume - remainingVolume;
							PODsUpdated++;

							remainingPacks = 0;
							remainingWeight = 0;
							remainingVolume = 0;
						}
						else
						{
							remainingPacks = remainingPacks - confirm.TotalDeliveredPackages;
							remainingWeight = remainingWeight - confirm.TotalDeliveredWeight;
							remainingVolume = remainingVolume - confirm.TotalDeliveredVolume;

							UpdateConfirm(confirm, shipment, deliveryDateTime, signature);
							PODsUpdated++;
						}

						if (remainingPacks.IsEmpty)
						{
							break;
						}
					}
				}

				if (!remainingPacks.IsEmpty)
				{
					if (!shipment.HasUndeliveredPackages(ConfirmTimesSyncHelper.ConfirmType.Delivery))
					{
						throw new ArgumentException("There were " + remainingPacks + " remaining Packages to be updated, but no remaining packages could be delivered.");
					}
					else
					{
						CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
						if (remainingPacks > confirm.TotalDeliveredPackages)
						{
							throw new ArgumentException("There were " + remainingPacks + " remaining packages to be updated, but only " + confirm.TotalDeliveredPackages + " remaining packages could be delivered.");
						}
						UpdateConfirm(confirm, shipment, deliveryDateTime, signature, remainingPacks, remainingWeight, remainingVolume);
						PODsCreated++;
					}
				}
			}
		}

		void UpdateSplitSpecified(ForwardingShipment shipment, ZString signature, ZDateTime deliveryDateTime)
		{
			var jobNumber = shipment.JS_UniqueConsignRef;

			foreach (CommonPickupDeliveryConfirm confirm in shipment.DeliveryConfirms)
			{
				if (confirm.EU_PickupDeliveryTime.IsEmpty)
				{
					UpdateConfirm(confirm, shipment, deliveryDateTime, signature);
					PODsUpdated++;
				}
			}

			if (shipment.OuterPackLines.Count == 0)
			{
				throw new ArgumentException("Shipment '" + jobNumber + "' doesn't have any Outer Packlines.");
			}
			else if (shipment.DeliveryConfirms.Count == 0 || !shipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual))
			{
				CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
				UpdateConfirm(confirm, shipment, deliveryDateTime, signature);
				PODsCreated++;
			}
		}

		void UpdateDeclarationConfirms(ZDateTime deliveryDateTime, BaseJobDeclaration declaration)
		{
			declaration.JE_CartageCompleted = deliveryDateTime;
			PODsUpdated++;
			AddDeliveryDateUpdatedEventTo(declaration.Logs);
			PerformSave();
		}

		void UpdateContainerizedConfirms(ForwardingShipment shipment, ZString signature, ZDateTime deliveryDateTime)
		{
			if (shipment.ArrivalContainers.Count == 0)
			{
				shipment.DocsAndCartage.JP_DeliveryCartageCompleted = deliveryDateTime;
				PODsUpdated++;
			}
			else
			{
				foreach (CommonContainer container in shipment.ArrivalContainers)
				{
					CommonPickupDeliveryConfirm confirm = container.DestinationConfirm;

					UpdateConfirm(confirm, shipment, deliveryDateTime, signature);
					if (confirm.IsInDatabase)
					{
						PODsUpdated++;
					}
					else
					{
						PODsCreated++;
					}
				}
			}
		}

		void UpdateConfirm(CommonPickupDeliveryConfirm confirm, CommonShipment parent, ZDateTime deliveryDateTime, ZString signature)
		{
			UpdateConfirm(confirm, parent, deliveryDateTime, signature, 0, 0, 0);
		}

		void UpdateConfirm(CommonPickupDeliveryConfirm confirm, CommonShipment parent, ZDateTime deliveryDateTime, ZString signature, ZInt deliveredPacks, ZDecimal deliveredWeight, ZDecimal deliveredVolume)
		{
			if (!deliveredPacks.IsEmpty)
			{
				confirm.TotalDeliveredPackages = deliveredPacks;
			}

			if (!deliveredWeight.IsEmpty)
			{
				confirm.TotalDeliveredWeight = deliveredWeight;
			}

			if (!deliveredVolume.IsEmpty)
			{
				confirm.TotalDeliveredVolume = deliveredVolume;
			}

			if (!signature.IsEmpty)
			{
				var maxLengthOfEU_GoodsSignForBy = confirm.EU_GoodsSignForByInfo.MaxLength;
				if (signature.Length > maxLengthOfEU_GoodsSignForBy)
				{
					confirm.EU_GoodsSignForBy = signature.Left(maxLengthOfEU_GoodsSignForBy);
					DisplayLogMessage(Res.GetString("cf30b211-741e-4e11-b715-d742f5806f31", "[{0}] Goods Signature has been truncated to fit shipment limit of {1} characters.", parent.JS_UniqueConsignRef, maxLengthOfEU_GoodsSignForBy) + " ");
				}
				else
				{
					confirm.EU_GoodsSignForBy = signature;
				}
			}
			if (!deliveryDateTime.IsEmpty)
			{
				confirm.EU_PickupDeliveryTime = deliveryDateTime;
			}
		}

		ForwardingShipment GetShipment(ZString jobNumber)
		{
			return Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, jobNumber));
		}

		BaseJobDeclaration GetDeclaration(ZString jobNumber)
		{
			return Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, jobNumber));
		}

		void AddDeliveryDateUpdatedEventTo(Logs log)
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log.AddNew(Events.DataImport, "POD date updated");
		}

		void PerformSave()
		{
			Factory.Save();
			RunCounters.RecsCreated += PODsCreated;
			RunCounters.RecsUpdated += PODsUpdated;
		}

		void ResetCounter()
		{
			PODsCreated = 0;
			PODsUpdated = 0;
		}
		int PODsCreated;
		int PODsUpdated;

		public ZBool HasErrors
		{
			get { return RunCounters.RecsExcluded > 0; }
		}

		#endregion
	}
}
