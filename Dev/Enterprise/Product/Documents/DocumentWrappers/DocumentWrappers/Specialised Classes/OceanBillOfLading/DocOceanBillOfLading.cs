using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class DocOceanBillOfLading : NonPersistentBusinessObject, IObsoleteValidation, IFormedPagesSupporter
	{
		public DocOceanBillOfLading(AgencyShipment shipment, DocAgencyShipment shipmentWrapper)
			: base(shipmentWrapper.Factory)
		{
			if (shipment == null)
			{
				throw new ArgumentNullException(nameof(shipment));
			}

			if (shipmentWrapper == null)
			{
				throw new ArgumentNullException(nameof(shipmentWrapper));
			}

			this.shipmentWrapper = shipmentWrapper;
			this.shipment = shipment;
		}

		#region Fields

		public ZString Weight
		{
			get
			{
				if (this.weight.IsEmpty)
				{
					ZDecimal newWeight = CalculateWeight(false);

					if (newWeight != 0M)
					{
						this.weight = string.Format("{0} {1}", FormatWeightNumber(newWeight), TargetWeightUnitCode);
					}
				}

				return this.weight;
			}
		}
		ZString weight;

		public ZString GrossWeight
		{
			get
			{
				if (this.grossWeight.IsEmpty)
				{
					ZDecimal weight = CalculateWeight(true);

					if (weight != 0m)
					{
						this.grossWeight = string.Format("{0} {1}", FormatWeightNumber(weight), TargetWeightUnitCode);
					}
				}

				return this.grossWeight;
			}
		}
		ZString grossWeight;

		public ZString Volume
		{
			get
			{
				if (this.volume.IsEmpty && shipmentWrapper.ActualVolume != 0M)
				{
					decimal newVolume;
					ZString newVolumeUnitCode;

					if (shipmentWrapper.ConvertUnits)
					{
						newVolumeUnitCode = shipmentWrapper.BOLVolumeUnit;
						newVolume = Constants.Volume.ConvertSafe(shipmentWrapper.ActualVolume, shipmentWrapper.UnitOfVolume, newVolumeUnitCode);
					}
					else
					{
						newVolume = shipmentWrapper.ActualVolume;
						newVolumeUnitCode = shipmentWrapper.UnitOfVolume;
					}

					this.volume = string.Format("{0} {1}", FormatVolumeNumber(newVolume), newVolumeUnitCode);
				}

				return this.volume;
			}
		}
		ZString volume;

		public ZString Title
		{
			get { return (IsSeaWaybill) ? DocConstants.Resources.BillTitles.SWB : DocConstants.Resources.BillTitles.HBL; }
		}

		public ZBool IsSeaWaybill
		{
			get { return shipmentWrapper.ReleaseTypeCode == Core.Constants.ShipmentReleaseTypes.SeaWaybill; }
		}

		public ZString OceanBillNumber
		{
			get { return shipmentWrapper.HouseBill; }
		}

		public ZString VesselName
		{
			get { return shipmentWrapper.VesselName; }
		}

		public ZString VoyageNumber
		{
			get { return shipmentWrapper.VoyageNo; }
		}

		public ZString NotifyParty
		{
			get
			{
				if (shipmentWrapper.NotifyParty != null)
				{
					return shipmentWrapper.NotifyParty.PostalAddress;
				}
				return Env.Registry.NotifyPartyDefaultText;
			}
		}

		public DocUNLOCO OriginPort
		{
			get { return shipmentWrapper.OriginLoco; }
		}

		public DocUNLOCO LoadPort
		{
			get { return shipmentWrapper.LoadPort; }
		}

		public DocUNLOCO DischargePort
		{
			get { return shipmentWrapper.DischargePort; }
		}

		public DocUNLOCO DestinationPort
		{
			get { return shipmentWrapper.DestinationLoco; }
		}

		public DocUNLOCO FreightPayableAtPort
		{
			get
			{
				if (shipmentWrapper.IsPrepaid)
				{
					return OriginPort;
				}
				else if (shipmentWrapper.IsCollect)
				{
					return DestinationPort;
				}
				else
				{
					return null;
				}
			}
		}

		public ZInt NumberOfContainers
		{
			get { return Containers.Count; }
		}

		public ZString BillClause
		{
			get { return shipmentWrapper.GetDocDataValue((NoResString)"Bill Clause", DefaultBillClause); }
		}

		public ZString DefaultBillClause
		{
			get
			{
				OrgHeader principal = shipment.Principal;
				return principal == null ? "" : AgencyRegistry.Instance.BillOfLadingClause(principal).Value ?? string.Empty;
			}
		}

		#region Country specific

		public ZString PuertoRicoTaxReleaseStatus
		{
			get
			{
				if (shipment.RealContainers.All(x => ((AgencyShipmentContainer)x).JC_ContainerImportDORelease.StartsWith("RELEASED", System.StringComparison.OrdinalIgnoreCase)))
				{
					return "RELEASED";
				}
				else if (shipment.RealContainers.All(x => ((AgencyShipmentContainer)x).JC_ContainerImportDORelease.StartsWith((NoResString)"NOT RLSD", System.StringComparison.OrdinalIgnoreCase)))
				{
					return (NoResString)"NOT RELEASED";
				}

				return ZString.Empty;
			}
		}

		#endregion

		#endregion

		#region IFormedPagesSupporter

		public DocFormedPagesShipmentCollection Shipments
		{
			get
			{
				if (shipments == null)
				{
					DocFormedPagesShipment docFormedPageShipment = new DocFormedPagesShipment()
					{
						MarksAndNumbers = shipmentWrapper.MarksAndNumbers.ToUpper(),
						GoodsDescription = shipmentWrapper.DescriptionForGoods.ToUpper(),
						PackageCount = shipment.IsTopLevelPacksMode
							? shipment.TopLevelPacks.Count.ToString()
							: shipment.OuterPackLines.Count.ToString(),
						Weight = Weight,
						Volume = Volume,
					};

					shipments = new DocFormedPagesShipmentCollection(Factory);
					shipments.Add(docFormedPageShipment);
				}

				return shipments;
			}
		}
		DocFormedPagesShipmentCollection shipments;

		public DocFormedPagesContainerCollection Containers
		{
			get
			{
				if (containers == null)
				{
					containers = new DocFormedPagesContainerCollection(Factory);

					if (shipmentWrapper.PackingMode == "FCL")
					{
						foreach (IDocContainer container in shipmentWrapper.Containers)
						{
							containers.Add(new DocFormedPagesContainer(container));
						}

						containers.Sort("ContainerNumber");
					}
				}

				return containers;
			}
		}
		DocFormedPagesContainerCollection containers;

		bool IFormedPagesSupporter.DisplayContainers
		{
			get { return shipmentWrapper.PackingMode == Constants.ContainerModes.FCL; }
		}

		bool IFormedPagesSupporter.HideContainerGrossWeight
		{
			get { return false; }
		}

		bool IFormedPagesSupporter.HideContainerTareWeight
		{
			get { return false; }
		}

		bool IFormedPagesSupporter.HidePackLinesInContainersSection
		{
			get { return false; }
		}

		public DocFormedPagesTopLevelPackCollection TopLevelPacks
		{
			get
			{
				if (topLevelPacks == null)
				{
					topLevelPacks = new DocFormedPagesTopLevelPackCollection(Factory);

					if (shipment.IsTopLevelPacksMode)
					{
						foreach (AgencyShipmentContainer container in shipment.ShippingContainers)
						{
							topLevelPacks.Add(new DocFormedPagesTopLevelPack(container));
						}
					}
				}

				return topLevelPacks;
			}
		}
		DocFormedPagesTopLevelPackCollection topLevelPacks;

		public DocPackLinesCollection PackLines
		{
			get { return shipmentWrapper.OuterPackLineCollection; }
		}

		public DocJobChargeCollection AllCharges => shipmentWrapper.JobHeader?.JobCharges ?? DocJobChargeCollection.GetCollection(shipmentWrapper, (NoResString)"Empty");

		public DocJobChargeCollection CollectCharges =>
			DocJobChargeCollection.GetCollection(shipmentWrapper, nameof(CollectCharges),
				(collectCharges) =>
				{
					foreach (DocJobCharge charge in AllCharges)
					{
						if (IsCollect(charge))
						{
							collectCharges.Add(charge);
						}
					}
				});

		ZBool IsCollect(DocJobCharge charge)
		{
			ZString agencyInvoiceType = ((JobCharge)charge.WrappedObject).JR_InvoiceType;
			return (agencyInvoiceType == AgencyInvoiceTypesList.Codes.ForeignCollect || agencyInvoiceType == AgencyInvoiceTypesList.Codes.LocalCollect ||
					agencyInvoiceType == AgencyInvoiceTypesList.Codes.ForeignCollect_Batching || agencyInvoiceType == AgencyInvoiceTypesList.Codes.LocalCollect_Batching ||
					agencyInvoiceType == AgencyInvoiceTypesList.Codes.Misc ||
					agencyInvoiceType == AgencyInvoiceTypesList.Codes.Misc_Batching);
		}

		public ZString BOLClause
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				builder.AppendIfNotEmpty(BillClause);
				builder.AppendIfNotEmpty(shipmentWrapper.AdditionalBillClauses);

				return builder.ToStringWithDelimiterBetweenAppends("\r\n\r\n");
			}
		}

		public DocBillOfLadingFormedPageCollection FormedPages
		{
			get
			{
				if (formedPages == null)
				{
					formedPages = new DocBillOfLadingFormedPageCollection(shipmentWrapper, this, Factory);
				}

				return formedPages;
			}
		}
		DocBillOfLadingFormedPageCollection formedPages;

		public ZString[] FollowOnSection
		{
			get
			{
				return FormedPages.FollowOnSection;
			}
		}

		public ZBool HasFollowOnSection
		{
			get
			{
				return FormedPages.HasFollowOnSection;
			}
		}

		public bool IsOriginal
		{
			get { return false; }
		}

		public bool IsCopy
		{
			get { return false; }
		}

		public bool ShouldPrintChargesAsLumpSum
		{
			get { return false; }
		}

		public bool ShouldPrintTotalCharges
		{
			get { return false; }
		}

		#endregion

		#region Section Headers

		public ZString DetailsSectionHeader
		{
			get { return FormedPages.DetailsSectionHeader; }
		}

		public ZString ContainersSectionHeader
		{
			get { return FormedPages.ContainersSectionHeader; }
		}

		public ZString ExtraSectionHeader
		{
			get { return FormedPages.ExtraSectionHeader; }
		}

		public ZString ChargesSectionHeader
		{
			get { return FormedPages.ChargesSectionHeader; }
		}

		public ZString PackRORSectionHeader
		{
			get { return FormedPages.PackRORSectionHeader; }
		}

		#endregion

		#region Implementation

		ZString TargetWeightUnitCode
		{
			get
			{
				ZString targetUnitCode = shipmentWrapper.BOLWeightUnit;

				if (shipmentWrapper.ContainerMode == Constants.ContainerModes.FCL)
				{
					if (!shipmentWrapper.ConvertUnits)
					{
						ZString enteredWeightUnit = targetUnitCode;
						if (shipmentWrapper.Containers.Count > 0)
						{
							enteredWeightUnit = shipmentWrapper.Containers[0].WeightUQ;
						}
						else if (shipmentWrapper.OuterPackLineCollection.Count > 0)
						{
							enteredWeightUnit = shipmentWrapper.OuterPackLineCollection[0].ActualWeightUQ;
						}

						if (targetUnitCode != enteredWeightUnit
								&& shipmentWrapper.Containers.All(x => ((IDocContainer)x).WeightUQ == enteredWeightUnit)
								&& shipmentWrapper.OuterPackLineCollection.Cast<DocPackLines>().Where(x => x.Container == null).All(x => x.ActualWeightUQ == enteredWeightUnit))
						{
							targetUnitCode = enteredWeightUnit;
						}
					}
				}
				else
				{
					if (!shipmentWrapper.ConvertUnits)
					{
						targetUnitCode = shipmentWrapper.UnitOfWeight;
					}
				}

				return targetUnitCode;
			}
		}

		ZDecimal CalculateWeight(bool includeTare)
		{
			if (shipmentWrapper.ContainerMode == Constants.ContainerModes.FCL)
			{
				ZDecimal result = 0M;

				foreach (IDocContainer container in shipmentWrapper.Containers)
				{
					ZDecimal containerWeight = container.GrossWeight;

					if (!includeTare)
					{
						containerWeight -= container.TareWeight;
					}

					result += Constants.Weight.ConvertSafe(containerWeight, container.WeightUQ, TargetWeightUnitCode);
				}

				foreach (DocPackLines pack in shipmentWrapper.OuterPackLineCollection)
				{
					if (pack.Container == null)
					{
						result += Constants.Weight.ConvertSafe(pack.ActualWeight, pack.ActualWeightUQ, TargetWeightUnitCode);
					}
				}

				return result;
			}
			else
			{
				return Constants.Weight.ConvertSafe(shipmentWrapper.ActualWeight, shipmentWrapper.UnitOfWeight, TargetWeightUnitCode);
			}
		}

		ZString FormatWeightNumber(ZDecimal value)
		{
			return shipmentWrapper.FormatNumber(value, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
		}

		ZString FormatVolumeNumber(ZDecimal value)
		{
			if (value.IsEmpty)
			{
				return ZString.Empty;
			}
			else
			{
				return shipmentWrapper.FormatNumber(value, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);
			}
		}

		readonly DocAgencyShipment shipmentWrapper;
		readonly AgencyShipment shipment;

		#endregion
	}
}
