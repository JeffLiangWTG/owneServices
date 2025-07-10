using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class FreightShipmentManifestLineWrapper : IManifestLineWrapper
	{
		public FreightShipmentManifestLineWrapper(CommonShipment shipment, IEManifestLine consignmentLine, FreightConsolWrapper consolWrapper)
		{
			Shipment = shipment;
			Consignment = consignmentLine;
			ConsolWrapper = consolWrapper;
		}

		public static FreightShipmentManifestLineWrapper NewWhenShipmentIsDeleted()
		{
			return new FreightShipmentManifestLineWrapper(null, null, null) { fLineActionCode = LineAction.Delete };
		}

		public int PackCount => Consignment?.PackCount ?? Shipment?.JS_OuterPacks ?? ZInt.Zero;

		public int ContainerCount => Shipment?.Containers.Count() ?? 0;

		public ZString CAN => Shipment.Factory.GetValue(ref cachedCAN, GetCAN);

		CachedProperty<ZString> cachedCAN;

		public ZString GetCAN()
		{
			var canEntryNum = CANEntryNum;
			return canEntryNum == null ? ZString.Empty : canEntryNum.CE_EntryNum;
		}

		public ZString CCAN => Shipment.Factory.GetValue(ref cachedCCAN, GetCCAN);

		CachedProperty<ZString> cachedCCAN;

		public ZString GetCCAN()
		{
			var contingencyEntryNum = ContingencyCAN;
			return contingencyEntryNum == null ? ZString.Empty : contingencyEntryNum.CE_EntryNum;
		}

		public ZString CountryOfDestination => Consignment?.CountryOfDestination ?? Shipment?.Destination?.RL_RN_NKCountryCode ?? ZString.Empty;

		public ZString AirWaybillNumber => ZString.Empty;

		public ZString GoodsOwner => Consignment?.GoodsOwner ?? Shipment?.ConsignorDocumentaryAddress?.E2_CompanyNameTruncated ?? ZString.Empty;

		public ZString GoodsOwnerPartyID
		{
			get
			{
				if (Shipment == null || Shipment.Consignor == null)
				{
					return ZString.Empty;
				}
				else
				{
					return (!Shipment.Consignor.LocalBusinessRegNo.IsEmpty) ? Shipment.Consignor.LocalBusinessRegNo : Shipment.Consignor.LocalCustomsSupplierCode;
				}
			}
		}

		public ZString GoodsDescription
		{
			get
			{
				var result = ZString.Empty;
				if (Consignment != null)
				{
					result = Consignment.GoodsDescription;
				}

				if (result.IsEmpty && Shipment != null)
				{
					result = Shipment.JS_GoodsDescription;
				}

				return result;
			}
		}

		public ZString ExemptionCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (Consignment != null)
				{
					result = Common.AU.CMR.CMRExportExemptionCodes.EXLV.Code;
				}
				else
				{
					AUCusEntryNumber entryNum = this.EntryNum;
					if (entryNum != null)
					{
						if (entryNum.CE_EntryType == CusEntryNumber.EntryType.ImportManifestStatus && entryNum.CE_EntryNum == AirCargoStatus.TranshipmentCargo.Code)
						{
							result = CMRExportExemptionCodes.EXTI.Code;
						}
						else if (entryNum.IsValidExemption)
						{
							result = CMRExportExemptionCodes.GetFromExit2Exemption(entryNum.CE_EntryType);
						}
					}
				}

				return result;
			}
		}

		public int LineNumber { get; set; }

		public ZString LineActionCode
		{
			get
			{
				if (fLineActionCode == null)
				{
					fLineActionCode = ZString.Empty;
					if (Consignment != null)
					{
						if (ConsolWrapper != null && ConsolWrapper.ExportManifestMessageType == Customs.Common.MessageBuilders.MessageSubTypes.Change)
						{
							fLineActionCode = ConsolWrapper != null ? ConsolWrapper.GetLineActionForConsignment(Consignment.PK) : LineAction.Insert;
						}
					}
					else if (Shipment != null)
					{
						if (ConsolWrapper != null && ConsolWrapper.ExportManifestMessageType == Customs.Common.MessageBuilders.MessageSubTypes.Change)
						{
							fLineActionCode = ConsolWrapper != null ? ConsolWrapper.GetLineActionFor(Shipment.PK) : LineAction.Insert;
						}
					}
				}

				return fLineActionCode;
			}
		}
		protected string fLineActionCode;

		public ZString Reference
		{
			get
			{
				var result = ZString.Empty;
				if (Consignment != null)
				{
					var consignRef = Consignment.Reference;
					result = ReferenceIsUnique(consignRef) ? consignRef : new ZString(Shipment.JS_HouseBill + "-" + consignRef);
				}
				else if (Shipment != null)
				{
					result = Shipment.JS_UniqueConsignRef;
				}

				return result;
			}
		}

		public bool HasCANOrExemption
		{
			get
			{
				return !CAN.IsEmpty || !CCAN.IsEmpty || !ExemptionCode.IsEmpty;
			}
		}

		#region HVLV ESM processing

		public int ESMLineNumber    // for HVLV consignment lines to be included in Export Sub Manifest
		{
			get
			{
				int result = 0;
				if (ESMMainfestLineSequence != null)
				{
					result = ESMMainfestLineSequence.CY_Order;
				}

				return result;
			}
			set
			{
				var esmManifestLineData = ESMMainfestLineSequence;
				if (esmManifestLineData == null)
				{
					var consol = ConsolWrapper.Consol;
					esmManifestLineData = LineSequenceCollection.AddNew();
					esmManifestLineData.CY_Code = CustomsManifestLineSequence.DataCodes.ManifestLineNumber;
					esmManifestLineData.CY_Data = consol != null ? consol.JK_UniqueConsignRef : Shipment.JS_JK_ConsolID;
				}

				esmManifestLineData.CY_Order = (ZShort)value;
			}
		}

		public CustomsManifestLineSequence ESMMainfestLineSequence
		{
			get
			{
				CustomsManifestLineSequence result = null;
				var consol = ConsolWrapper.Consol;
				foreach (CustomsManifestLineSequence lineSequence in LineSequenceCollection)
				{
					if (lineSequence.CY_Code == CustomsManifestLineSequence.DataCodes.ManifestLineNumber)
					{
						if (consol != null)
						{
							if (lineSequence.CY_Data == consol.JK_UniqueConsignRef)
							{
								result = lineSequence;
								break;
							}
						}
						else
						{
							result = lineSequence;
							break;
						}
					}
				}

				return result;
			}
		}

		public bool HasSubManifestLineNumber
		{
			get { return ESMMainfestLineSequence != null & ESMLineNumber > 0; }
		}

		[ChildEditable(true)]
		public CustomsManifestLineSequenceCollection LineSequenceCollection
		{
			get
			{
				if (esmLineSequenceData == null)
				{
					esmLineSequenceData = Consignment is IHVLVConsignment hvlvConsignment
						? new CustomsManifestLineSequenceCollection(hvlvConsignment)
						: new CustomsManifestLineSequenceCollection(Shipment);

					esmLineSequenceData.Load();
					Shipment.RegisterEditableChildObject(esmLineSequenceData);
				}
				return esmLineSequenceData;
			}
		}
		CustomsManifestLineSequenceCollection esmLineSequenceData;

		bool ReferenceIsUnique(ZString referenceToCheck)
		{
			return ShipmentReferenceUnique(referenceToCheck) &&
				   HLSShipmentConsignmentUnique(referenceToCheck) &&
				   ConsignmentReferenceUnique(referenceToCheck);
		}

		bool ShipmentReferenceUnique(ZString referenceToCheck)
		{
			var result = true;
			var consol = ConsolWrapper?.Consol;
			if (consol != null)
			{
				bool hasSameShipmentReference = consol.Shipments.Cast<ForwardingShipment>().Any(x => x.JS_HouseBill == referenceToCheck);
				result = !hasSameShipmentReference;
			}

			return result;
		}

		bool HLSShipmentConsignmentUnique(ZString referenceToCheck)
		{
			var result = true;
			var consol = ConsolWrapper?.Consol;
			if (consol != null)
			{
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					if (shipment.PK != Shipment.PK)
					{
						if (shipment.IsHighVolumeLowValueLegacy || shipment.IsHighVolumeLowValue)
						{
							var hasSameConsignmentReference = shipment.GetHVLVConsignmentLines().Any(x => x.Reference == referenceToCheck);
							result = !hasSameConsignmentReference;
							if (!result)
							{
								break;
							}
						}
					}
				}
			}

			return result;
		}

		bool ConsignmentReferenceUnique(ZString referenceToCheck)
		{
			var result = true;
			if (Shipment is ForwardingShipment forwardingShipment)
			{
				var manifestLines = forwardingShipment.GetHVLVConsignmentLines();
				result = !manifestLines.Where(x => x.Reference == referenceToCheck).Skip(1).Any();
			}
			return result;
		}

		#endregion

		#region Implementation

		protected bool IsExemption
		{
			get
			{
				return EntryNum != null && EntryNum.IsValidExemption;
			}
		}

		protected AUCusEntryNumber EntryNum
		{
			get
			{
				FreightShipmentWrapper wrapper = new FreightShipmentWrapper(Shipment, ConsolWrapper.Consol);
				return wrapper.GetExistingPermit();
			}
		}

		protected AUCusEntryNumber CANEntryNum
		{
			get
			{
				ICollection<string> canPermitCode = new List<string>(new string[] { CANType.CustomsAuthorityNumber.Code });
				var wrapper = new FreightShipmentWrapper(Shipment, ConsolWrapper.Consol);
				return wrapper.GetSpecificPermitType(canPermitCode);
			}
		}

		protected AUCusEntryNumber ContingencyCAN
		{
			get
			{
				ICollection<string> contingencyPermitCodes = new List<string>(new string[] { CANType.ContingencyCustomsAuthorityNumber.Code, CusEntryNumberTypes.Australia.ECN });
				var wrapper = new FreightShipmentWrapper(Shipment, ConsolWrapper.Consol);
				return wrapper.GetSpecificPermitType(contingencyPermitCodes);
			}
		}

		protected CommonShipment Shipment { get; }
		protected FreightConsolWrapper ConsolWrapper { get; }
		protected IEManifestLine Consignment { get; }

		#endregion

		public ZString HouseBillNumber
		{
			get { return Consignment != null ? Reference : Shipment != null ? Shipment.JS_HouseBill : ZString.Empty; }
		}
	}
}
