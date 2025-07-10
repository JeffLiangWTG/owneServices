using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class StmALogValueObjectDataAdapterForBatchImport : StmALogValueObjectDataAdapter
	{
		public StmALogValueObjectDataAdapterForBatchImport()
			: base(null, "", null)
		{
		}

		#region Reference Getters

		protected delegate BusinessObject GetBusinessObjectByReference(ZString reference, IValueObjectImportContext context);

		Dictionary<Xsd.ReferenceType, GetBusinessObjectByReference> ReferenceGetters
		{
			get
			{
				if (referenceGetters == null)
				{
					referenceGetters = new Dictionary<Xsd.ReferenceType, GetBusinessObjectByReference>();
					CreateReferenceTypesMap(referenceGetters);
				}

				return referenceGetters;
			}
		}
		Dictionary<Xsd.ReferenceType, GetBusinessObjectByReference> referenceGetters;

		protected virtual void CreateReferenceTypesMap(Dictionary<Xsd.ReferenceType, GetBusinessObjectByReference> referenceGetters)
		{
			referenceGetters.Add(Xsd.ReferenceType.ShipmentJobNumber, GetShipmentByReference);
			referenceGetters.Add(Xsd.ReferenceType.DeclarationJobNumber, GetDeclarationByReference);
			referenceGetters.Add(Xsd.ReferenceType.ConsolNumber, GetConsolByUniqueConsignRef);
			referenceGetters.Add(Xsd.ReferenceType.MasterBill, GetConsolByMasterBillNum);
			referenceGetters.Add(Xsd.ReferenceType.OrderNumber, GetOrderByOrderNumber);
		}

		BusinessObject GetShipmentByReference(ZString reference, IValueObjectImportContext context)
		{
			ZQuery query = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, reference);
			return context.Factory.LoadTop1<CommonShipment>(query);
		}

		BusinessObject GetDeclarationByReference(ZString reference, IValueObjectImportContext context)
		{
			ZQuery query = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, reference);
			return context.Factory.LoadTop1<BaseJobDeclaration>(query);
		}

		BusinessObject GetConsolByUniqueConsignRef(ZString reference, IValueObjectImportContext context)
		{
			ZQuery query = new ZQuery(JobConsolSchema.JK_UniqueConsignRef, reference);
			return context.Factory.LoadTop1<ForwardingConsol>(query);
		}

		BusinessObject GetConsolByMasterBillNum(ZString reference, IValueObjectImportContext context)
		{
			ZQuery query = new ZQuery(JobConsolSchema.JK_MasterBillNum, reference);
			return context.Factory.LoadTop1<ForwardingConsol>(query);
		}

		BusinessObject GetOrderByOrderNumber(ZString reference, IValueObjectImportContext context)
		{
			ZQuery query = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, reference);
			query.OrderBy = JobOrderHeaderSchema.JD_OrderNumberSplit.Name + " DESC";
			var result = context.Factory.LoadTop1<Order>(query);

			if (result == null)
			{
				Regex reg = new Regex(@"^(?<OrderNumber>\S+)-(?<Split>\d+)$");
				Match match = reg.Match(reference);
				if (match.Success)
				{
					ZString newOrderReference = match.Groups["OrderNumber"].Value;
					ZByte newSplitReference;
					if (ZByte.TryParse(match.Groups["Split"].Value, out newSplitReference))
					{
						ZQuery querywithSplits = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, newOrderReference);
						querywithSplits.AddToFilter(JobOrderHeaderSchema.JD_OrderNumberSplit, newSplitReference);
						result = context.Factory.LoadTop1<Order>(querywithSplits);
					}
				}
			}

			return result;
		}

		#endregion

		public override StmALog CreateOrUpdateFromValueObject(Xsd.Event value, IValueObjectImportContext context)
		{
			if (value != null)
			{
				Event eventType = GetEventTypeByCode(value.Code, context);
				if (eventType == null)
				{
					context.Notify(new ErrorNotification(ErrorType.UnknownCode, Res.GetString("e46b88a7-23b1-4f94-9eba-5b78711e21e8", "Event code '{0}'", value.Code)));
				}
				else
				{
					value.Code = eventType.Code;
					ZString referenceKeyValue = ZString.Empty;
					foreach (Xsd.EventReferenceKey refKey in value.ReferenceKeys)
					{
						if (ReferenceGetters.ContainsKey(refKey.ReferenceKeyName))
						{
							referenceKeyValue = refKey.Value;
							LogParent = referenceGetters[refKey.ReferenceKeyName](referenceKeyValue, context);
							break;
						}
					}

					if (LogParent != null)
					{
						return base.CreateOrUpdateFromValueObject(value, context);
					}
					else
					{
						ZString errorMessage = Res.GetString("32e2ec9c-005d-49ef-81e3-13e590a566a9", "Event '{0}' -", value.Code) + " ";
						if (referenceKeyValue.IsEmpty)
						{
							errorMessage += Res.GetString("c2687a25-ac94-4380-9823-8fc3a315e847", "Referenced record number is not specified");
						}
						else
						{
							errorMessage += Res.GetString("1e59fae2-6ac2-47ff-b1fc-6e3b757d8933", "Unable to find referenced record '{0}'", referenceKeyValue);
						}

						context.Notify(new WarningNotification(WarningType.Warning, errorMessage));
					}
				}
			}

			return null;
		}

		protected override void ImportFromValueObjectCore(StmALog log, Xsd.Event value, IValueObjectImportContext context)
		{
			base.ImportFromValueObjectCore(log, value, context);

			foreach (Xsd.EventReferenceKey refKey in value.ReferenceKeys)
			{
				if (refKey.ReferenceKeyName == Xsd.ReferenceType.AdditionalReferenceNumber)
				{
					if (CheckNumberValueObjectForErrors(refKey, Res.GetString("d1b9c024-c07b-453a-ab27-6a7fbec99eb9", "additional reference number"), context))
					{
						IAdditionalReferenceNumberSupporter refNumbersSupporter = LogParent as IAdditionalReferenceNumberSupporter;
						if (refNumbersSupporter != null)
						{
							refNumbersSupporter.CreateOrUpdate(refKey.ReferenceKeyType, refKey.ReferenceKeyCountry, refKey.Value, context);
						}
						else
						{
							context.Notify(new WarningNotification(WarningType.Warning, Res.GetString("db63f65c-4435-49fa-922b-525f486d69c3", "Event code '{0}'. No support for additional reference numbers - {1}", value.Code, LogParent.GetType().Name)));
						}
					}
				}
				else if (refKey.ReferenceKeyName == Xsd.ReferenceType.CustomsEntryNumber)
				{
					if (CheckNumberValueObjectForErrors(refKey, Res.GetString("0277ed97-cdf3-4521-a61c-1822205d0109", "customs entry number"), context))
					{
						ICusEntryNumberSupporter cusNumbersSupporter = LogParent as ICusEntryNumberSupporter;
						if (cusNumbersSupporter != null)
						{
							cusNumbersSupporter.CreateOrUpdate(refKey.ReferenceKeyType, refKey.ReferenceKeyCountry, refKey.Value, context);

							cusNumbersSupporter.UpdateCustomsEntryIssueDate(refKey.ReferenceKeyType, refKey.ReferenceKeyCountry, refKey.ReferenceKeyDateTime);
						}
						else
						{
							context.Notify(new WarningNotification(WarningType.Warning, Res.GetString("2012d6c7-2a0c-4c3a-85b6-1d723672aa6b", "Event code '{0}'. No support for customs entry numbers - {1}", value.Code, LogParent.GetType().Name)));
						}
					}
				}
			}
		}

		protected override StmALog NewBusinessObject(Xsd.Event value, IValueObjectImportContext context)
		{
			return LogParent.GetLogs().AddNew();
		}

		#region Validation

		bool CheckNumberValueObjectForErrors(Xsd.EventReferenceKey referenceKey, ZString name, IValueObjectImportContext context)
		{
			bool hasErrors = false;
			if (referenceKey.ReferenceKeyType.IsEmpty)
			{
				context.Notify(new WarningNotification(WarningType.Warning, Res.GetString("cf569405-a87d-4dae-9585-562334c9eb38", "'Type' of {0} with value '{1}' must be provided", name, referenceKey.Value)));
				hasErrors = true;
			}

			if (referenceKey.ReferenceKeyCountry.IsEmpty)
			{
				context.Notify(new WarningNotification(WarningType.Warning, Res.GetString("d58445df-1b41-4161-bfdc-e0ca696a305a", "'Country/Region' of {0} with value '{1}' must be provided", name, referenceKey.Value)));
				hasErrors = true;
			}

			if (referenceKey.Value.Length > CusEntryNumSchema.CE_EntryNum.MaxLength)
			{
				context.Notify(new WarningNotification(WarningType.MaxLengthExceeded, Res.GetString("a70792d4-d851-4056-be61-5a57879bcce4", "{0} with value '{1}'", name, referenceKey.Value)));
				hasErrors = true;
			}

			if (referenceKey.ReferenceKeyType.Length > CusEntryNumSchema.CE_EntryType.MaxLength)
			{
				context.Notify(new WarningNotification(WarningType.MaxLengthExceeded, Res.GetString("a70792d4-d851-4056-be61-5a57879bcce4", "{0} with value '{1}'", name, referenceKey.ReferenceKeyType)));
				hasErrors = true;
			}

			return !hasErrors;
		}

		#endregion
	}
}
