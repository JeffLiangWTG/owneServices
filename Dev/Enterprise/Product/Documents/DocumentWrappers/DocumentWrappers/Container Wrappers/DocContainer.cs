using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocContainer : DocFreightBaseContainer, IDocIMO, IDocCartageAdvice, IDocServicesParent
	{
		protected DocContainer(CommonContainer container, CommonShipment shipment, BusinessObjectFactory factoryToWrap)
			: base(container, factoryToWrap)
		{
			fShipment = shipment;
		}

		public static DocContainer New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<CommonContainer>(pK), factory);
		}

		public static DocContainer New(CommonContainer container, BusinessObjectFactory factoryToWrap)
		{
			DocContainer result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(container, null, factoryToWrap);
			}
			else
			{
				result = NewCore(container, null, factoryToWrap);
			}

			return result;
		}

		public static DocContainer New(CommonContainer container, CommonShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			DocContainer result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(container, shipment, factoryToWrap);
			}
			else
			{
				result = NewCore(container, shipment, factoryToWrap);
			}

			return result;
		}

		public static DocContainer NewCore(CommonContainer container, CommonShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			DocContainer result = null;

			if (container != null)
			{
				result = new DocContainer(container, shipment, factoryToWrap);
			}

			return result;
		}

		#region Overrides

		#region IDocContainer Members

		#region ZString Fields

		public override ZString ForwardingInstructionWeight
		{
			get { return DisplayAmount(TotalPackLineWeight); }
		}

		public override ZString ForwardingInstructionVolume
		{
			get { return DisplayAmount(TotalPackLineVolume); }
		}

		public override ZString ForwardingInstructionPackages
		{
			get { return DisplayAmount(ZDecimal.Parse(TotalPackLinePackages.ToString())); }
		}

		public override ZString DescriptionAndStatus
		{
			get
			{
				ZString result = ZString.Empty;
				if (AllocatedShipmentPackLine != null & AllocatedShipmentPackLine.Count > 0)
				{
					result = AllocatedShipmentPackLine[0].GoodsDescription;
					result += (AllocatedShipmentPackLine[0].Commodity == null) ? "" : "/" + AllocatedShipmentPackLine[0].Commodity.Description;
				}
				result += (IsEmptyContainer) ? "/MT" : "/" + ContainerMode;
				return result;
			}
		}

		#endregion

		#region ZDecimal Fields

		public override ZDecimal TotalAllocatedShipmentWeight
		{
			get
			{
				ZDecimal total = 0;

				foreach (DocPackLines packLine in AllocatedShipmentPackLine)
				{
					ZDecimal weight = packLine.ActualWeight;
					weight = Core.Constants.Weight.ConvertSafe(weight, packLine.ActualWeightUQ, GrossWeightUQ);
					total += weight;
				}

				total = decimal.Round(total, 3);
				return total;
			}
		}

		public override ZDecimal TotalPackLineWeight
		{
			get
			{
				ZDecimal total = 0;
				foreach (DocPackLines packLine in PackLines)
				{
					ZDecimal weight = packLine.ActualWeight;
					weight = Core.Constants.Weight.ConvertSafe(weight, packLine.ActualWeightUQ, GrossWeightUQ);
					total += weight;
				}

				total = decimal.Round(total, 3);
				return total;
			}
		}

		public override ZDecimal TotalAllocatedShipmentVolume
		{
			get
			{
				ZDecimal total = 0;
				foreach (DocPackLines packLine in AllocatedShipmentPackLine)
				{
					ZDecimal volume = packLine.ActualVolume;
					volume = Core.Constants.Volume.ConvertSafe(volume, packLine.ActualVolumeUQ, VolumeUQ);
					total += volume;
				}

				total = decimal.Round(total, 3);
				return total;
			}
		}

		public override ZString TotalAllocatedShipmentVolumeUQ
		{
			get { return VolumeUQ; }
		}

		public override ZDecimal TotalPackLineVolume
		{
			get
			{
				ZDecimal total = 0;
				foreach (DocPackLines packLine in PackLines)
				{
					ZDecimal volume = packLine.ActualVolume;
					volume = Core.Constants.Volume.ConvertSafe(volume, packLine.ActualVolumeUQ, VolumeUQ);
					total += volume;
				}

				total = decimal.Round(total, 3);
				return total;
			}
		}

		public ZString VolumeUQ
		{
			get { return Shipment == null || Shipment.UnitOfVolume.IsEmpty ? Env.Registry.FreightVolumeUnit : Shipment.UnitOfVolume.ToString(); }
		}

		#endregion

		#region ZInt Fields

		public override ZInt TotalPackLinePackages
		{
			get
			{
				ZInt total = 0;
				foreach (DocPackLines packLine in PackLines)
				{
					total += packLine.PackageCount;
				}

				return total;
			}
		}

		public override ZInt TotalAllocatedShipmentPackages
		{
			get
			{
				ZInt total = 0;
				foreach (DocPackLines packLine in AllocatedShipmentPackLine)
				{
					total += packLine.PackageCount;
				}

				return total;
			}
		}

		public override ZString TotalAllocatedShipmentPackagesPackType
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocPackLines line in AllocatedShipmentPackLine)
				{
					if (result.IsEmpty)
					{
						result = line.PackType;
					}
					else if (line.PackType != result)
					{
						result = Core.Constants.PkgUnit.Piece;
						break;
					}
				}
				return (!result.IsEmpty) ? result : (ZString)Core.Constants.PkgUnit.Piece;
			}
		}
		#endregion

		#endregion

		public override DocOrganisation DepartureTransport
		{
			get
			{
				DocOrganisation result = base.DepartureTransport;

				if (result == null)
				{
					if (Consol != null && Consol.DepartureTransportCompany != null)
					{
						result = Consol.DepartureTransportCompany;
					}
					else if (Shipments.Count > 0)
					{
						DocOrganisation shipmentDepartureTransport = Shipments[0].DocsAndCartage.PickupCartageCo;

						if (Shipments.Cast<DocShipment>().All(shipment => shipment.DocsAndCartage.PickupCartageCo == shipmentDepartureTransport))
						{
							result = shipmentDepartureTransport;
						}
					}
				}

				return result;
			}
		}

		#endregion

		public override ZString IMOClass
		{
			get
			{
				ZString result = ZString.Empty;
				System.Collections.ArrayList iMOCodes = new System.Collections.ArrayList();
				foreach (DocPackLines line in PackLines)
				{
					foreach (UNDGSubstanceWrapper docDG in line.UNDGs)
					{
						if (!iMOCodes.Contains(docDG.IMOClass))
						{
							iMOCodes.Add(docDG.IMOClass);
						}
					}
				}

				foreach (ZString codes in iMOCodes)
				{
					result += codes + ", ";
				}
				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		public override ZString UNDG_Num
		{
			get
			{
				ZString result = ZString.Empty;
				List<ZString> uNNOCodes = new List<ZString>();
				foreach (DocPackLines line in PackLines)
				{
					foreach (UNDGSubstanceWrapper docDG in line.UNDGs)
					{
						if (!uNNOCodes.Contains(docDG.UNNumberWithVariant))
						{
							uNNOCodes.Add(docDG.UNNumberWithVariant);
						}
					}
				}

				foreach (ZString codes in uNNOCodes)
				{
					result += codes + ", ";
				}
				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		#region Collections

		public DocPackLinesCollection AllocatedShipmentPackLine
		{
			get
			{
				DocPackLinesCollection packLineCollection = new DocPackLinesCollection(FreightContainer.Factory);
				if (fShipment != null)
				{
					foreach (PackLine packLine in fShipment.OuterPackLines)
					{
						if (packLine.Containers.Contains(CommonContainer.PK))
						{
							packLineCollection.Add(DocPackLines.New(packLine, Factory));
						}
					}

					packLineCollection.SortByHazardous();
				}

				return packLineCollection;
			}
		}

		public DocPackLinesCollection PackLines
		{
			get
			{
				DocPackLinesCollection packLineCollection = new DocPackLinesCollection(FreightContainer.Factory);

				foreach (PackLine packLine in FreightContainer.PackLines)
				{
					packLineCollection.Add(DocPackLines.New(packLine, Factory));
				}

				packLineCollection.SortByHazardous();

				return packLineCollection;
			}
		}

		public DocShipmentCollection Shipments
		{
			get
			{
				DocShipmentCollection result = new DocShipmentCollection(Factory);

				foreach (DocPackLines packLine in PackLines)
				{
					if (packLine.Shipment != null && !result.ContainsWrappedObject(((BusinessObject)packLine.Shipment.WrappedObject).PK))
					{
						result.Add(packLine.Shipment);
					}
				}

				return result;
			}
		}
		#endregion

		#region Wrapper Fields

		public DocShipment Shipment
		{
			get { return (fShipment != null) ? DocShipment.New(fShipment, Factory) : null; }
		}

		public DocForwardingConsol ArrivalConsol
		{
			get { return Consol; }
		}

		public DocERA ERA
		{
			get { return DocERA.New(this.FreightContainer, Factory); }
		}

		public DocPickupDeliveryConfirm OriginConfirm
		{
			get { return Shipment != null ? DocPickupDeliveryConfirm.New(CommonContainer.OriginConfirm, Factory) : null; }
		}

		public DocPickupDeliveryConfirm DestinationConfirm
		{
			get { return Shipment != null ? DocPickupDeliveryConfirm.New(CommonContainer.DestinationConfirm, Factory) : null; }
		}

		#endregion

		#region ZGuid Fields

		public ZGuid DeparturePackAddressOrg
		{
			get { return FreightContainer.JC_Calc_DeparturePackAddressOrg; }
		}

		public ZGuid DepartureCTOAddressOrg
		{
			get { return FreightContainer.JC_Calc_DepartureCTOAddressOrg; }
		}

		public ZGuid DepartureContainerParkAddressOrg
		{
			get { return FreightContainer.JC_Calc_DepartureContainerYardAddressOrg; }
		}

		public ZGuid ArrivalUnpackAddressOrg
		{
			get { return FreightContainer.JC_Calc_ArrivalUnpackAddressOrg; }
		}

		public ZGuid ArrivalCTOAddressOrg
		{
			get { return FreightContainer.JC_Calc_ArrivalCTOAddressOrg; }
		}

		public ZGuid ArrivalContainerParkAddressOrg
		{
			get { return FreightContainer.JC_Calc_ArrivalContainerYardAddressOrg; }
		}

		#endregion

		#region ZString Fields

		public ZString PackingMode
		{
			get { return (Shipment != null) ? Shipment.PackingMode : ZString.Empty; }
		}

		public ZString DeparturePackAddressCode
		{
			get { return FreightContainer.JC_Calc_DeparturePackAddressCode; }
		}

		public ZString DepartureCTOAddressCode
		{
			get { return FreightContainer.JC_Calc_DepartureCTOAddressCode; }
		}

		public ZString DepartureContainerParkAddressCode
		{
			get { return FreightContainer.JC_Calc_DepartureContainerYardAddressCode; }
		}

		public ZString ArrivalUnpackAddressCode
		{
			get { return FreightContainer.JC_Calc_ArrivalUnpackAddressCode; }
		}

		public ZString ArrivalCTOAddressCode
		{
			get { return FreightContainer.JC_Calc_ArrivalCTOAddressCode; }
		}

		public ZString ArrivalContainerParkAddressCode
		{
			get { return FreightContainer.JC_Calc_ArrivalContainerYardAddressCode; }
		}

		public ZString ContainerTempSign
		{
			get { return (SetPointTemp < 0) ? "-" : "+"; }
		}

		public ZString ContainerTemp
		{
			get
			{
				ZString result = SetPointTemp.ToStringTrimZeros();
				result = result.TrimStart(' ');
				result = result.TrimStart('+');
				result = result.TrimStart('-');
				result = result.TrimStart(' ');
				return result;
			}
		}

		public ZString AuthorisationReleaseClause
		{
			get { return Env.Registry.AuthorisationReleaseClause; }
		}

		public ZString Haulier
		{
			get
			{
				DocOrganisation haulier = null;
				if (Shipment != null)
				{
					haulier = Shipment.ImportCartage;
				}

				if (haulier == null && Consol != null)
				{
					haulier = Consol.ArrivalTransportCompany;
				}

				ZString result = ZString.Empty;
				ZString cCD = ZString.Empty;

				if (haulier != null)
				{
					result = haulier.Name;
					cCD = haulier.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientCode, null);
					if (!cCD.IsEmpty)
					{
						result += (NoResString)" CR#: " + cCD;
					}
				}

				return result;
			}
		}

		class ForwardingInstructionPackingKey
		{
			public ZString packType;
			public ZString description;
			public ZString hazardous;
			public ZString actualWeightUQ;

			public ForwardingInstructionPackingKey(ZString packType, ZString description, ZString hazardous, ZString actualWeightUQ)
			{
				this.packType = packType;
				this.description = description;
				this.hazardous = hazardous;
				this.actualWeightUQ = actualWeightUQ;
			}

			public override bool Equals(object obj)
			{
				if (obj == null || GetType() != obj.GetType())
				{
					return false;
				}

				ForwardingInstructionPackingKey p = (ForwardingInstructionPackingKey)obj;
				return packType == p.packType &&
						description == p.description &&
						hazardous == p.hazardous &&
						actualWeightUQ == p.actualWeightUQ;
			}

			public override int GetHashCode()
			{
				return packType.GetHashCode() ^
						description.GetHashCode() ^
						hazardous.GetHashCode() ^
						actualWeightUQ.GetHashCode();
			}
		}

		public ZString ForwardingInstructionPackingDetails
		{
			get
			{
				// Group pack lines by packType, description, commodity, weight unit
				Dictionary<ForwardingInstructionPackingKey, DocPackLinesCollection> packLinesDict = new Dictionary<ForwardingInstructionPackingKey, DocPackLinesCollection>();
				foreach (DocPackLines packLine in PackLines)
				{
					ForwardingInstructionPackingKey key = new ForwardingInstructionPackingKey(packLine.PackType, packLine.Description, packLine.HazardousDescriptionWithoutWeight, packLine.ActualWeightUQ);

					DocPackLinesCollection currentList = null;
					if (!packLinesDict.ContainsKey(key))
					{
						currentList = new DocPackLinesCollection(Factory);
						packLinesDict.Add(key, currentList);
					}
					else
					{
						currentList = packLinesDict[key];
					}

					currentList.Add(packLine);
				}

				const int approxWidth = 90;
				const string separator = " - ";

				ZStringBuilder result = new ZStringBuilder();
				foreach (DocPackLinesCollection mergedPackLines in packLinesDict.Values)
				{
					// Calculating totals of merged pack lines
					ZInt packCount = 0;
					ZDecimal actualWeight = 0;
					foreach (DocPackLines packLine in mergedPackLines)
					{
						packCount += packLine.PackageCount;
						actualWeight += packLine.ActualWeight;
					}

					DocPackLines firstPakLine = mergedPackLines[0];
					ZString packType = packCount.ToString().PadLeft(2) + " " + firstPakLine.PackType;
					ZString description = firstPakLine.Description;
					ZString hazardous = firstPakLine.HazardousDescriptionWithoutWeight + " - " + actualWeight.ToStringTrimZeros() + " " + firstPakLine.ActualWeightUQ;

					int width = packType.Length + description.Length + hazardous.Length + (2 * separator.Length);
					if (width > approxWidth)
					{
						int subStringLength = description.Length - (width - approxWidth);
						if (subStringLength >= 0)
						{
							description = description.SubstringSafe(0, subStringLength);
						}
					}

					List<string> lines = new List<string>();
					lines.Add(packType);
					if (!description.IsEmpty)
					{
						lines.Add(description);
					}

					if (!hazardous.IsEmpty)
					{
						lines.Add(hazardous);
					}

					ZString line = string.Join(separator, lines.ToArray());

					result.Append(line.SubstringSafe(0, approxWidth));
				}

				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString NotClearedByAgentNumber
		{
			get { return Shipment != null ? Shipment.NotClearedByAgentNumber : ZString.Empty; }
		}

		#endregion

		#region ZDateTime Fields

		public virtual ZDateTime BookingCutOffDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (Consol != null)
				{
					result = (PackingMode == Core.Constants.ContainerModes.FCL) ? Consol.FCLCutOff : Consol.LCLCutOff;
				}
				else if (Shipment != null && Shipment.Sailing != null) //Booking
				{
					result = (PackingMode == Core.Constants.ContainerModes.FCL) ? Shipment.Sailing.FCLCutOff : Shipment.Sailing.LCLCutOff;
				}

				return result;
			}
		}

		public virtual ZDateTime PickUpDate
		{
			get { return (Shipment != null && PackingMode == Core.Constants.ContainerModes.FCL) ? Shipment.PickupDate : ZDateTime.Empty; }
		}

		public ZDateTime AvailableDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (Shipment == null && Consol == null)
				{
					result = LCLAvailable;
				}
				else if (Shipment != null)
				{
					if (Shipment.PackingMode == Core.Constants.ContainerModes.FCL)
					{
						result = ContainerAvailable;
						if (result.IsEmpty)
						{
							result = Shipment.DocsAndCartage.FCLAvailable;
						}
					}
					else
					{
						result = LCLAvailable;
						if (result.IsEmpty)
						{
							result = Shipment.DocsAndCartage.LCLAvailable;
						}
					}
				}
				else if (Consol != null)
				{
					if (Consol.PackingMode == Core.Constants.ContainerModes.FCL)
					{
						result = ContainerAvailable;
					}
					else
					{
						result = LCLAvailable;
					}
				}

				return result;
			}
		}

		public ZDateTime StorageCommenceDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (Shipment == null && Consol == null)
				{
					result = LCLStorageCommences;
				}
				else if (Shipment != null)
				{
					if (Shipment.PackingMode == Core.Constants.ContainerModes.FCL)
					{
						result = StorageCommences;
						if (result.IsEmpty)
						{
							result = Shipment.DocsAndCartage.FCLStorageCommences;
						}
					}
					else
					{
						result = LCLStorageCommences;
						if (result.IsEmpty)
						{
							result = Shipment.DocsAndCartage.LCLStorageCommences;
						}
					}
				}
				else if (Consol != null)
				{
					if (Consol.PackingMode == Core.Constants.ContainerModes.FCL)
					{
						result = StorageCommences;
					}
					else
					{
						result = LCLStorageCommences;
					}
				}

				return result;
			}
		}

		public ZDateTime CartageCutOffDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (Consol != null)
				{
					result = Consol.CartageCutOffDate;
				}
				else if (Shipment != null)
				{
					result = Shipment.CartageCutOffDate;
				}
				return result;
			}
		}

		public ZDateTime CartageAvailableDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (Consol != null)
				{
					result = Consol.CartageAvailableDate;
				}
				else if (Shipment != null)
				{
					result = Shipment.CartageAvailableDate;
				}
				return result;
			}
		}

		public ZDateTime CutOffOrAvailableDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (IsExportDocument)
				{
					result = BookingCutOffDate;
				}
				else if (IsImportDocument)
				{
					result = AvailableDate;
				}
				return result;
			}
		}

		public ZDateTime CartageReceivalDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (Consol != null)
				{
					result = Consol.CartageReceivalDate;
				}
				else if (Shipment != null)
				{
					result = Shipment.CartageReceivalDate;
				}
				return result;
			}
		}

		public ZDateTime CartageStorageCommenceDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (Consol != null)
				{
					result = Consol.CartageStorageCommenceDate;
				}
				else if (Shipment != null)
				{
					result = Shipment.CartageStorageCommenceDate;
				}
				return result;
			}
		}

		public ZDateTime PickupOrStorageCommenceDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (IsExportDocument)
				{
					result = PickUpDate;
				}
				else if (IsImportDocument)
				{
					result = StorageCommenceDate;
				}
				return result;
			}
		}

		public ZString PickupOrStorageCommenceDateHeading
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsExportDocument)
				{
					result = CartageAdvice.PickupDateHeading;
				}
				else if (IsImportDocument)
				{
					result = CartageAdvice.StorageCommencesHeading;
				}

				return result;
			}
		}

		public ZDateTime NotClearedByAgentIssueDate
		{
			get { return Shipment != null ? Shipment.NotClearedByAgentIssueDate : ZDateTime.Empty; }
		}

		public ZDateTime NotClearedByAgentExpiryDate
		{
			get { return Shipment != null ? Shipment.NotClearedByAgentExpiryDate : ZDateTime.Empty; }
		}

		#endregion

		#region ZDecimal

		public ZDecimal TEU
		{
			get { return (FreightContainer.RefContainer != null) ? FreightContainer.RefContainer.RC_TEU : ZDecimal.Zero; }
		}

		#endregion

		#region IDocIMO Members

		public DocOrganisation IMOSender
		{
			get
			{
				if (ReportName == ShipmentIMO)
				{
					return Shipment != null ? Shipment.Consignor : null;
				}
				else
				{
					return Consol != null ? Consol.SendingForwarder : null;
				}
			}
		}

		public DocOrganisation IMOConsignee
		{
			get
			{
				if (ReportName == ShipmentIMO)
				{
					return Shipment != null ? Shipment.Consignee : null;
				}
				else
				{
					return Consol != null ? Consol.ReceivingForwarder : null;
				}
			}
		}

		public DocOrganisation IMOCarrier
		{
			get { return Consol != null ? Consol.ShippingLine : null; }
		}

		public ZString IMOConsolNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (Consol != null)
				{
					result = Res.GetString("51d333fd-094e-4880-8cd6-33db6987e2ff", "Consol: {0}", Consol.ConsolNumber) + "\r\n";
					if (!Consol.MasterBillNum.IsEmpty)
					{
						result += Consol.MasterBillHeading + ": " + Consol.MasterBillNum;
					}
				}
				return result.TrimEnd();
			}
		}

		public virtual ZString IMOShippersRef
		{
			get { return Consol != null ? Consol.BookingReference : ZString.Empty; }
		}

		public virtual ZString IMOForwardersRef
		{
			get { return Consol != null ? Consol.AgentsReference : ZString.Empty; }
		}

		public ZString IMOVesselVoyage
		{
			get
			{
				ZString result = ZString.Empty;
				if (Consol != null)
				{
					if (Consol.TransportMode == Core.Constants.TransportModes.Air)
					{
						result = Consol.VoyageNumber;
					}
					else
					{
						result = Consol.VesselName;
						if (!Consol.VoyageNumber.IsEmpty)
						{
							result += result.IsEmpty ? Consol.VoyageNumber : new ZString(" / " + Consol.VoyageNumber);
						}
					}
				}
				return result;
			}
		}

		public ZString IMOETD
		{
			get
			{
				ZString result = ZString.Empty;
				if (ReportName == ShipmentIMO)
				{
					if (Shipment != null)
					{
						result = Shipment.ETDString;
					}
				}
				if (result.IsEmpty)
				{
					return (Consol != null) ? Consol.ETDString : ZString.Empty;
				}
				return result;
			}
		}

		public ZString IMOPortOfLoading
		{
			get { return Consol != null && Consol.PortOfLoading != null ? Consol.PortOfLoading.Code + ", " + Consol.PortOfLoading.PortName : ""; }
		}

		public ZString IMOPortOfDischarge
		{
			get { return Consol != null && Consol.PortOfDischarge != null ? Consol.PortOfDischarge.Code + ", " + Consol.PortOfDischarge.PortName : ""; }
		}

		public ZString IMODestination
		{
			get
			{
				if (ReportName == ShipmentIMO)
				{
					return Shipment.DestinationLoco != null ? Shipment.DestinationLoco.Code + ", " + Shipment.DestinationLoco.PortName : "";
				}
				else
				{
					if (Consol != null)
					{
						ZInt count = 0;
						DocShipment hazShipment = null;
						foreach (DocShipment shipment in Consol.MasterShipments)
						{
							if (shipment.Commodity.ContainsHazardous)
							{
								hazShipment = shipment;
								count++;
							}
						}
						if (hazShipment != null && count == 1)
						{
							if (hazShipment.DestinationLoco != null)
							{
								return hazShipment.DestinationLoco.Code + ", " + hazShipment.DestinationLoco.PortName;
							}
						}
					}
					return ZString.Empty;
				}
			}
		}

		public ZString IMOHandlingInstructions
		{
			get
			{
				ZString handlingInstNote = ZString.Empty;
				if (ReportName == ShipmentIMO)
				{
					handlingInstNote = Shipment.DangerousGoodsHandlingInstruction;
				}
				if (handlingInstNote.IsEmpty)
				{
					if (Consol != null)
					{
						handlingInstNote = Consol.DangerousGoodsHandlingInstruction;
					}
				}
				return handlingInstNote;
			}
		}

		public ZString IMOContainerNum
		{
			get { return this.ContainerNumber; }
		}

		public ZString IMOSealNum
		{
			get { return this.SealNumber; }
		}

		public ZString IMOContainerType
		{
			get
			{
				ZString result = Container != null ? Container.Code : ZString.Empty;
				result += result.IsEmpty ? "" : " ";
				result += ContainerMode;
				return result;
			}
		}

		public ZString IMOContainerTare
		{
			get { return Container != null ? FormatNumber(Container.TareWeight) : ZString.Empty; }
		}

		public ZString IMOTotalGrossMassAndTare
		{
			get { return FormatNumber(Core.Constants.Weight.ConvertSafe(GrossWeight, GrossWeightUQ, Core.Constants.Weight.Kilograms)).ToString(); }
		}

		public DocIMOBodyCollection IMOShipments
		{
			get
			{
				DocIMOBodyCollection coll = new DocIMOBodyCollection(FreightContainer.Factory);
				if (ReportName == ShipmentIMO)
				{
					if (Shipment != null)
					{
						coll = GetHazPackDetailsForShipmentIMO();
					}
				}
				else
				{
					DocLoadListPackLineCollection hAZPackLines = new DocLoadListPackLineCollection(Consol, HazardousPackLines, FreightContainer.Factory);

					foreach (DocLoadListPackLine lines in hAZPackLines)
					{
						if (lines.Shipment != null)
						{
							ZString descriptionLine = Res.GetString("e1e10077-d0e4-4afa-9de9-9ffe5628d720", "Job Reference: {0}", lines.Shipment.ShipmentNumber) + "\n";
							descriptionLine += lines.PackageDetailsWithHazCat + "\n";
							descriptionLine += Res.GetString("bd03aeda-0380-4932-83e5-db48dbdcc269", "Description: {0}", lines.Shipment.DetailedDescriptionOfGoods);
							DocIMOBody iMOBody = new DocIMOBody(lines.Shipment.MarksAndNumbers, descriptionLine,
									FormatNumber(lines.Weight), "", FormatNumber(lines.Volume));
							coll.Add(iMOBody);
						}
					}

					if (coll.Count == 0)
					{
						coll.Add(GetEmptyShipmentIMO());
					}
				}
				return coll;
			}
		}

		#region IMO Fields

		public DocPackLinesCollection HazardousPackLines
		{
			get
			{
				DocPackLinesCollection hAZLines = new DocPackLinesCollection(FreightContainer.Factory);
				foreach (DocPackLines line in PackLines)
				{
					if (line.IsHazardous)
					{
						hAZLines.Add(line);
					}
				}

				return hAZLines;
			}
		}

		protected DocIMOBodyCollection GetHazPackDetailsForShipmentIMO()
		{
			DocIMOBodyCollection coll = new DocIMOBodyCollection(FreightContainer.Factory);

			ZString descriptionLine = Res.GetString("e1e10077-d0e4-4afa-9de9-9ffe5628d720", "Job Reference: {0}", Shipment.ShipmentNumber) + "\n";
			ZDecimal totalWeight = 0M;
			ZDecimal totalVolume = 0M;

			DocPackLinesCollection hazPacks = new DocPackLinesCollection(FreightContainer.Factory);
			foreach (DocPackLines line in AllocatedShipmentPackLine)
			{
				if (line.IsHazardous)
				{
					hazPacks.Add(line);
				}
			}

			DocLoadListPackLineCollection allocatedHazPackLines = new DocLoadListPackLineCollection(Consol, hazPacks, FreightContainer.Factory);
			foreach (DocLoadListPackLine line in allocatedHazPackLines)
			{
				descriptionLine += line.PackageDetailsWithHazCat + "\n";
				totalWeight += line.Weight;
				totalVolume += line.Volume;
			}

			if (hazPacks.Count > 0)
			{
				descriptionLine += Res.GetString("926a3bbb-9ebb-40d8-afd4-d7d6bf246eb3", "Description: {0}", Shipment.DetailedDescriptionOfGoods);
				DocIMOBody iMOBody = new DocIMOBody(Shipment.MarksAndNumbers, descriptionLine, FormatNumber(totalWeight), "", FormatNumber(totalVolume));
				coll.Add(iMOBody);
			}
			else
			{
				coll.Add(GetEmptyShipmentIMO());
			}

			return coll;
		}
		protected DocIMOBody GetEmptyShipmentIMO()
		{
			return new DocIMOBody(Res.GetString("976e5e23-5da8-41b0-9c93-9b17e527e90b", "No Hazardous packlines available for this container"), "", "", "", "");
		}
		#endregion

		#endregion

		#region Implementation

		protected delegate DocContainer NewDelegate(CommonContainer container, CommonShipment shipment, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected CommonShipment fShipment;

		CommonContainer FreightContainer
		{
			get { return (CommonContainer)WrappedObject; }
		}

		protected ZString DisplayAmount(ZDecimal amount)
		{
			if (amount.IsEmpty)
			{
				return (Consol == null || Consol.ShowBlankTotals) ? ZString.Empty : new ZString("-");
			}
			else
			{
				return FormatNumber(amount);
			}
		}

		#endregion

		#region IDoc Cartage Advice

		public CartageAdviceHelper CartageAdvice
		{
			get
			{
				if (fCartageAdvice == null)
				{
					fCartageAdvice = new CartageAdviceHelper(this, Factory);
				}
				return fCartageAdvice;
			}
		}
		CartageAdviceHelper fCartageAdvice;

		#region Addresses

		public virtual DocPickupDeliveryConfirm OriginJourneyOneDeliveryConfirm
		{
			get { return OriginConfirm; }
		}

		public virtual DocPickupDeliveryConfirm OriginJourneyTwoPickupConfirm
		{
			get { return OriginConfirm; }
		}

		public virtual DocPickupDeliveryConfirm DestinationJourneyOneDeliveryConfirm
		{
			get { return DestinationConfirm; }
		}

		public virtual DocPickupDeliveryConfirm DestinationJourneyTwoPickupConfirm
		{
			get { return DestinationConfirm; }
		}

		#region JourneyOnePickUpAddress

		public override DocDocAddress JourneyOnePickUpAddress
		{
			get
			{
				DocDocAddress result = null;
				if (IsExport)
				{
					if (DepartureContainerParkAddress != null)
					{
						result = DepartureContainerParkAddress;
					}
					else if (Consol != null)
					{
						result = Consol.ContainerParkEmptyPickupAddress;
					}
				}
				else if (IsImport)
				{
					if (ArrivalCTOAddress != null)
					{
						result = ArrivalCTOAddress;
					}
					else if (Consol != null)
					{
						result = Consol.ArrivalCTOAddress;
					}
				}

				return result;
			}
		}

		#endregion

		#region JourneyOneDeliverTo Addresses

		#region JourneyOneDeliverToAddressForExport

		public override DocDocAddress JourneyOneDeliverToAddressForExport
		{
			get
			{
				DocDocAddress result = null;
				if (OriginJourneyOneDeliveryConfirm != null && !DocDocAddress.IsNullOrEmpty(OriginJourneyOneDeliveryConfirm.ConfirmAddress))
				{
					result = OriginJourneyOneDeliveryConfirm.ConfirmAddress;
				}
				else if (Shipment != null)
				{
					result = Shipment.JourneyOneDeliverToAddressForExport;
				}
				else if (DeparturePackAddress != null)
				{
					result = DeparturePackAddress;
				}
				return result;
			}
		}

		#endregion

		#region JourneyOneDeliverToAddressForImport

		public override DocDocAddress JourneyOneDeliverToAddressForImport
		{
			get
			{
				DocDocAddress result = null;
				if (DestinationJourneyOneDeliveryConfirm != null && !DocDocAddress.IsNullOrEmpty(DestinationJourneyOneDeliveryConfirm.ConfirmAddress))
				{
					result = DestinationJourneyOneDeliveryConfirm.ConfirmAddress;
				}
				else if (Shipment != null)
				{
					result = Shipment.JourneyOneDeliverToAddressForImport;
				}
				else if (ArrivalUnpackAddress != null)
				{
					result = ArrivalUnpackAddress;
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region JourneyTwoPickUp Addresses

		#region JourneyTwoPickUpAddressForExport

		public override DocDocAddress JourneyTwoPickUpAddressForExport
		{
			get
			{
				DocDocAddress result = null;
				if (OriginJourneyTwoPickupConfirm != null && !DocDocAddress.IsNullOrEmpty(OriginJourneyTwoPickupConfirm.ConfirmAddress))
				{
					result = OriginJourneyTwoPickupConfirm.ConfirmAddress;
				}
				else if (Shipment != null)
				{
					result = Shipment.JourneyTwoPickUpAddressForExport;
				}
				else if (DeparturePackAddress != null)
				{
					result = DeparturePackAddress;
				}
				return result;
			}
		}

		#endregion

		#region JourneyTwoPickUpAddressForImport

		public override DocDocAddress JourneyTwoPickUpAddressForImport
		{
			get
			{
				DocDocAddress result = null;
				if (DestinationJourneyTwoPickupConfirm != null && !DocDocAddress.IsNullOrEmpty(DestinationJourneyTwoPickupConfirm.ConfirmAddress))
				{
					result = DestinationJourneyTwoPickupConfirm.ConfirmAddress;
				}
				else if (Shipment != null)
				{
					result = Shipment.JourneyTwoPickUpAddressForImport;
				}
				else if (ArrivalUnpackAddress != null)
				{
					result = ArrivalUnpackAddress;
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region JourneyTwoDeliverToAddress

		public override DocDocAddress JourneyTwoDeliverToAddress
		{
			get
			{
				DocDocAddress result = null;

				if (IsExport)
				{
					if (DepartureCTOAddress != null)
					{
						result = DepartureCTOAddress;
					}
					else if (Consol != null)
					{
						result = Consol.DepartureCTOAddress;
					}
					else if (Shipment != null && Shipment.IsBooking)
					{
						result = Shipment.ExportReceivingDepot;
					}
				}
				else if (IsImport)
				{
					if (ArrivalContainerParkAddress != null)
					{
						result = ArrivalContainerParkAddress;
					}
					else if (Consol != null)
					{
						result = Consol.ContainerParkEmptyReturnAddress;
					}
				}

				return result;
			}
		}

		#endregion

		#endregion

		public ZString EquipmentType
		{
			get
			{
				ZString result = "";

				if (result.IsEmpty && Shipment != null)
				{
					result = Shipment.EquipmentType;
				}

				return result;
			}
		}

		public virtual ZString FullHandlingInstructions
		{
			get
			{
				ZString result = "";

				if (Shipment != null)
				{
					result = Shipment.ShipmentAndOrgHandlingInstructions;
				}
				else if (Consol != null)
				{
					result = Consol.HandlingInstructions;
				}

				return result;
			}
		}

		public virtual ZString FullCartageInstructions
		{
			get
			{
				ZString result = "";

				if (Shipment != null)
				{
					result = Shipment.ShipmentAndOrgCartageInstruction;
				}
				else if (Consol != null)
				{
					result = Consol.CartageInstructions;
				}

				return result;
			}
		}

		public override DocOrganisation Consignee
		{
			get
			{
				DocOrganisation result = base.Consignee;

				if (result == null && Shipment != null)
				{
					result = Shipment.Consignee;
				}

				return result;
			}
		}

		public override DocOrganisation Consignor
		{
			get
			{
				DocOrganisation result = base.Consignor;

				if (result == null && Shipment != null)
				{
					result = Shipment.Consignor;
				}

				return result;
			}
		}

		public DocOrganisation ConsignorForERA
		{
			get
			{
				DocOrganisation result = null;
				if (Consol != null && Consol.ConsolMode == Constants.ContainerModes.Groupage)
				{
					result = Consol.SendingForwarder;
				}
				else if (Shipment != null)
				{
					result = Shipment.Consignor;
				}
				else if (Shipments.Count > 0)
				{
					result = Shipments[0].Consignor;
				}

				return result;
			}
		}

		public ZBool IsDomesticAndFreightCOD
		{
			get { return Shipment != null ? Shipment.IsDomesticAndFreightCOD : ZBool.False; }
		}

		public ZDecimal FreightCODAmount
		{
			get { return Shipment != null ? Shipment.FreightCODAmount : ZDecimal.Zero; }
		}

		public ZString FreightCODAmountWithCurrency
		{
			get { return Shipment != null ? Shipment.FreightCODAmountWithCurrency : ZString.Empty; }
		}

		#endregion

		#region IDocServiceParent Members

		ZString IDocServicesParent.ConsolNumber
		{
			get { return (Consol != null) ? Consol.ConsolNumber : ZString.Empty; }
		}

		ZString IDocServicesParent.GoodsDescription
		{
			get { return ""; }
		}

		ZString IDocServicesParent.Packages
		{
			get { return TotalPackLinePackages.ToString(); }
		}

		ZString IDocServicesParent.Weight
		{
			get { return FreightContainer.JC_GrossWeight.ToString(); }
		}

		ZString IDocServicesParent.Volume
		{
			get { return ""; }
		}

		ZString IDocServicesParent.WeightUnit
		{
			get { return FreightContainer.JC_GrossWeightUQ; }
		}

		ZString IDocServicesParent.VolumeUnit
		{
			get { return ""; }
		}

		ZString IDocServicesParent.MasterBillNum
		{
			get { return FreightContainer.JC_Calc_MasterBillNum; }
		}

		ZString IDocServicesParent.MasterBillHeading
		{
			get { return (Consol != null) ? Consol.MasterBillHeading : ZString.Empty; }
		}

		ZString IDocServicesParent.HouseBill
		{
			get { return (Shipment != null) ? Shipment.HouseBill : ZString.Empty; }
		}

		ZString IDocServicesParent.HouseBillHeading
		{
			get { return (Shipment != null) ? Shipment.HouseBillHeading : ZString.Empty; }
		}

		ZString IDocServicesParent.TransportInfo
		{
			get { return (Consol != null) ? Consol.VesselAndVoyage : ZString.Empty; }
		}

		ZDateTime IDocServicesParent.ETD
		{
			get { return (Consol != null) ? Consol.ETD : ZDateTime.Empty; }
		}

		ZDateTime IDocServicesParent.ETA
		{
			get { return (Consol != null) ? Consol.ETA : ZDateTime.Empty; }
		}

		ZString IDocServicesParent.ContainerNumbers
		{
			get { return ContainerNumber; }
		}

		ZString IDocServicesParent.Context
		{
			get { return "CONTAINER"; }
		}

		ZString IDocServicesParent.OwnerRefAndOrderRef
		{
			get { return (Consol != null) ? Consol.OwnerRefAndOrderRef : ZString.Empty; }
		}

		ZString IDocServicesParent.OwnerRefAndOrderRefHeading
		{
			get { return (Consol != null) ? Consol.OwnerRefAndOrderRefHeading : ZString.Empty; }
		}

		#endregion

		#region GoodsDescriptionForERA

		public ZString GoodsDescriptionForERA
		{
			get
			{
				var result = ZString.Empty;

				if (PackLines.Any())
				{
					var firstDescription = PackLines[0].Description;
					if (!firstDescription.IsEmpty && PackLines.Cast<DocPackLines>().Skip(1).All(x => x.Description == firstDescription))
					{
						result = firstDescription.Replace("\n", " ");
					}
				}

				if (result.IsEmpty && Shipments.Any())
				{
					var firstGoodsDescription = Shipments[0].GoodsDescription;
					if (!firstGoodsDescription.IsEmpty && Shipments.Cast<DocShipment>().Skip(1).All(x => x.GoodsDescription == firstGoodsDescription))
					{
						result = firstGoodsDescription.Replace("\n", " ");
					}
				}

				if (result.IsEmpty)
				{
					result = Res.GetString("5459763a-2d0b-42d7-8311-7bafcd38e306", "Freight All Kinds");
				}

				return result;
			}
		}

		#endregion
	}
}
