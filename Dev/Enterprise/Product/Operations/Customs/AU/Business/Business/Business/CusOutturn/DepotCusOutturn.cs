using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[DependentBusinessObject(typeof(CusOutturnHeader), "Outturns")]
	public class DepotCusOutturn : CusOutturn
		, ISeaCargoEstablishmentQueryInformation
		, ISendersMessageReferenceProvider
	{
		public DepotCusOutturn(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			MessageStatusCalculator = new DepotCusOutturnUnderbondStatusCalculator(this);
			if (C5_MessageStatus == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		public new abstract class Schema : CusOutturn.Schema
		{
			public const string RawCombinedStatus = "RawCombinedStatus";
			public const string LoadList = "LoadList";
			public const string ShipmentOrContainerNumber = "ShipmentOrContainerNumber";
			public const string OutturnStatus = "OutturnStatus";
		}

		#region Business Object Overrides

		#region Validation

		public new DepotCusOutturnValidation Validation
		{
			get { return (DepotCusOutturnValidation)base.Validation; }
		}

		protected override Customs.Business.CusOutturnValidation GetNewValidation()
		{
			return new DepotCusOutturnValidation(this);
		}

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public new DepotCusOutturn Clone()
		{
			return (DepotCusOutturn)base.Clone();
		}

		#endregion

		public override void OnSaved(bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				_outturnStatus = null;
				OutturnStatusInfo.RefreshBinding();
			}

			base.OnSaved(saveSucceeded);
		}

		protected override bool ShouldLogCustomsStatusChangedEvent => true;

		protected override bool ShouldPublishCustomStatusChangeEvent => Header != null;

		#endregion

		#region Properties

		CMRUBMREQRMessage URRMessage
		{
			get
			{
				if (_uRRMessage == null)
				{
					_uRRMessage = (CMRUBMREQRMessage)Messages.GetLastMessage(EDIMessage.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.UBMREQR);
				}
				return _uRRMessage;
			}
		}
		CMRUBMREQRMessage _uRRMessage;

		public ZString UBMRequestReason
		{
			get { return URRMessage != null ? URRMessage.RequestReason : ZString.Empty; }
		}
		public ZPropertyInfo UBMRequestReasonInfo
		{
			get { return GetZPropertyInfo(nameof(UBMRequestReason)); }
		}

		public ZString UBMOriginID
		{
			get { return URRMessage != null ? URRMessage.OriginID : ZString.Empty; }
		}
		public ZPropertyInfo UBMOriginIDInfo
		{
			get { return GetZPropertyInfo(nameof(UBMOriginID)); }
		}

		public ZString UBMDestinationID
		{
			get { return URRMessage != null ? URRMessage.DestinationID : ZString.Empty; }
		}
		public ZPropertyInfo UBMDestinationIDInfo
		{
			get { return GetZPropertyInfo(nameof(UBMDestinationID)); }
		}

		public CMRSEIMessageLine SEIMessageLine
		{
			get
			{
				if (_sEIMessageLine == null && Header != null)
				{
					ZString key = C5_ContainerNumber + "/" + C5_MasterBill + "/" + C5_HouseBill;
					Header.SEIMessageLines.TryGetValue(key, out _sEIMessageLine);
				}
				return _sEIMessageLine;
			}
		}
		CMRSEIMessageLine _sEIMessageLine;

		public ZBool SEIMatched
		{
			get { return SEIMessageLine != null; }
		}
		public ZPropertyInfo SEIMatchedInfo
		{
			get { return GetZPropertyInfo(nameof(SEIMatched)); }
		}

		public ZString SEIDetails
		{
			get
			{
				if (_sEIDetails.IsEmpty)
				{
					ZStringBuilder builder = new ZStringBuilder();
					if (SEIMatched)
					{
						builder.Append("SEI Details:");
						builder.Append(SEIMessageLine.ToString());
					}
					if (URRMessage != null)
					{
						builder.Append("UBM Details:");
						builder.Append(URRMessage.UnderbondMovementDetails());
					}
					_sEIDetails = builder.ToStringWithNewLineBetweenAppends();
				}

				return _sEIDetails;
			}
		}
		ZString _sEIDetails;

		public ZDateTime SEIProcessingDate
		{
			get
			{
				if (!_sEIProcessingDate.HasValue)
				{
					_sEIProcessingDate = SEIMatched ? SEIMessageLine.SEIMessage.ProcessingDate : ZDateTime.Empty;
				}
				return _sEIProcessingDate.Value;
			}
		}
		ZDateTime? _sEIProcessingDate;

		public ZPropertyInfo SEIProcessingDateInfo
		{
			get { return GetZPropertyInfo(nameof(SEIProcessingDate)); }
		}

		public ZString InlandMovementMode
		{
			get
			{
				if (!_inlandMovementMode.HasValue)
				{
					_inlandMovementMode = SEIMatched ? SEIMessageLine.SEIMessage.InlandMovementMode : ZString.Empty;
				}
				return _inlandMovementMode.Value;
			}
		}
		ZString? _inlandMovementMode;

		public ZPropertyInfo InlandMovementModeInfo
		{
			get { return GetZPropertyInfo(nameof(InlandMovementMode)); }
		}

		public ZString UBMResponsibleID
		{
			get
			{
				if (!_uBMResponsibleID.HasValue)
				{
					_uBMResponsibleID = SEIMatched ? SEIMessageLine.SEIMessage.UBMResponsiblePartyID : ZString.Empty;
				}
				return _uBMResponsibleID.Value;
			}
		}
		ZString? _uBMResponsibleID;

		public ZPropertyInfo UBMResponsibleIDInfo
		{
			get { return GetZPropertyInfo(nameof(UBMResponsibleID)); }
		}

		public ZString UBMResponsibleIDName
		{
			get
			{
				ZString result = ZString.Empty;
				if (!UBMResponsibleID.IsEmpty)
				{
					ZQuery query = new ZQuery();
					query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, UBMResponsibleID);
					query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Australia);
					query.AddToFilter(OrgCusCodeSchema.OK_CodeType, new ZString[] { OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber,
																																			OrgCusCode.CodeTypes.CustomsClientID });
					OrgCusCode orgCusCode = Factory.LoadTop1<OrgCusCode>(query);
					if (orgCusCode != null)
					{
						var org = Factory.Load<OrgHeader>(orgCusCode.OK_OH);
						if (org != null)
						{
							result = org.OH_FullNameTruncated;
						}
					}
				}
				return result;
			}
		}
		public ZPropertyInfo UBMResponsibleIDNameInfo
		{
			get { return GetZPropertyInfo(nameof(UBMResponsibleIDName)); }
		}

		public ZString RecipientSiteID
		{
			get
			{
				if (!_recipientSiteID.HasValue)
				{
					_recipientSiteID = SEIMatched ? SEIMessageLine.SEIMessage.RecipientSiteID : ZString.Empty;
				}
				return _recipientSiteID.Value;
			}
		}
		ZString? _recipientSiteID;

		public ZPropertyInfo RecipientSiteIDInfo
		{
			get { return GetZPropertyInfo(nameof(RecipientSiteID)); }
		}

		public ZString FreightForwarderIndicator
		{
			get
			{
				if (!_freightForwarderIndicator.HasValue)
				{
					_freightForwarderIndicator = SEIMatched ? SEIMessageLine.FreightForwarderIndicator.ToString() : "N/A";
				}
				return _freightForwarderIndicator.Value;
			}
		}
		ZString? _freightForwarderIndicator;

		public ZPropertyInfo FreightForwarderIndicatorInfo
		{
			get { return GetZPropertyInfo(nameof(FreightForwarderIndicator)); }
		}

		public ZString ConsigneeName
		{
			get { return SEIMatched ? SEIMessageLine.ConsigneeName : ZString.Empty; }
		}
		public ZPropertyInfo ConsigneeNameInfo
		{
			get { return GetZPropertyInfo(nameof(ConsigneeName)); }
		}

		public ZString NetWeight
		{
			get { return SEIMatched ? SEIMessageLine.NetWeightString : ZString.Empty; }
		}
		public ZPropertyInfo NetWeightInfo
		{
			get { return GetZPropertyInfo(nameof(NetWeight)); }
		}

		public ZString GrossWeight
		{
			get { return SEIMatched ? SEIMessageLine.GrossWeightString : ZString.Empty; }
		}
		public ZPropertyInfo GrossWeightInfo
		{
			get { return GetZPropertyInfo(nameof(GrossWeight)); }
		}

		public ZDecimal GrossWeightInDecimal
		{
			get { return SEIMatched ? SEIMessageLine.GrossWeight.ResultDecimal : ZDecimal.Zero; }
		}
		public ZString GrossWeightUQ
		{
			get { return SEIMatched ? SEIMessageLine.GrossWeight.ResultString : ZString.Empty; }
		}

		public ZDecimal VolumeInDecimal
		{
			get { return SEIMatched ? SEIMessageLine.Volume.ResultDecimal : ZDecimal.Zero; }
		}
		public ZString VolumeUQ
		{
			get { return SEIMatched ? SEIMessageLine.Volume.ResultString : ZString.Empty; }
		}

		public ZString Volume
		{
			get { return SEIMatched ? SEIMessageLine.VolumeString : ZString.Empty; }
		}
		public ZPropertyInfo VolumeInfo
		{
			get { return GetZPropertyInfo(nameof(Volume)); }
		}

		public CFSLoadListConsol LoadList
		{
			get
			{
				CFSLoadListConsol result = null;
				if (Container != null)
				{
					result = Container.Consol;
				}

				if (Shipment != null && Shipment.Consols.Count > 0)
				{
					result = Shipment.Consols[0];
				}

				return result;
			}
		}

		public CFSShipment Shipment
		{
			get
			{
				CFSShipment result = null;
				CFSShipmentWrapper wrapper = Parent as CFSShipmentWrapper;
				if (wrapper != null)
				{
					result = wrapper.Shipment;
				}

				return result;
			}
		}

		public CFSContainer Container
		{
			get { return GetContainerCore(); }
		}

		protected virtual CFSContainer GetContainerCore()
		{
			CFSContainer result = null;
			CFSContainerWrapper wrapper = Parent as CFSContainerWrapper;
			if (wrapper != null)
			{
				result = wrapper.Container;
			}

			return result;
		}

		public ZString ShipmentOrContainerNumber
		{
			get
			{
				ZString result = ZString.Empty;

				if (Container != null)
				{
					result = Container.JC_ContainerJobID;
				}
				else if (Shipment != null)
				{
					result = Shipment.JS_UniqueConsignRef;
				}

				return result;
			}
		}

		public ZPropertyInfo ShipmentOrContainerNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ShipmentOrContainerNumber); }
		}

		public ZBool IsBulk
		{
			get { return C5_CargoType == CMRImportCargoTypes.Codes.Bulk; }
		}

		public ZBool IsBreakBulk
		{
			get { return C5_CargoType == CMRImportCargoTypes.Codes.BreakBulk; }
		}

		public ZBool IsLCL
		{
			get { return C5_CargoType == CMRImportCargoTypes.Codes.LessThanContainerLoad; }
		}

		public ZBool IsFCL
		{
			get { return C5_CargoType == CMRImportCargoTypes.Codes.FullContainerLoad; }
		}

		public ZBool IsFCX
		{
			get { return C5_CargoType == CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills; }
		}

		public ZBool IsSurplus
		{
			get { return C5_OutturnResultType == CMROutturnResultType.Codes.SurplusConsignment || C5_OutturnResultType == CMROutturnResultType.Codes.SurplusPackages; }
		}

		#endregion

		#region Property Overrides

		#region Parent

		public override IOutturnableLine Parent
		{
			get
			{
				return base.Parent;
			}
			set
			{
				base.Parent = value;

				if (value != null)
				{
					CFSContainerWrapper containerWrapper = value as CFSContainerWrapper;

					if (containerWrapper != null)
					{
						if (C5_ContainerNumber.IsEmpty)
						{
							C5_ContainerNumber = containerWrapper.Container.JC_ContainerNum;
						}

						if (C5_CargoType.IsEmpty)
						{
							C5_CargoType = CMRCargoType(Enterprise.Core.Constants.ContainerModes.FCL);
						}

						if (C5_OuterPacks.IsEmpty)
						{
							C5_OuterPacks = 1;
						}

						if (C5_OuterPackUnits.IsEmpty)
						{
							C5_OuterPackUnits = CMRPackageTypes.Codes.UnpackedOrPacked;
						}
					}

					CFSShipmentWrapper shipmentWrapper = value as CFSShipmentWrapper;

					if (shipmentWrapper != null)
					{
						CFSLoadListConsol consol = shipmentWrapper.Shipment.ArrivalConsol as CFSLoadListConsol;

						if (consol == null && shipmentWrapper.Shipment.Consols.Count == 1)
						{
							consol = shipmentWrapper.Shipment.Consols[0];
						}

						if (consol != null && shipmentWrapper.Shipment.OuterPackLines.Count > 0)
						{
							CFSContainer container = shipmentWrapper.Shipment.OuterPackLines[0].GetContainer(consol) as CFSContainer;
							if (container != null && C5_ContainerNumber.IsEmpty)
							{
								C5_ContainerNumber = container.JC_ContainerNum;
							}
						}

						if (C5_HouseBill.IsEmpty)
						{
							if (!IsFCX && !IsFCL)
							{
								C5_HouseBill = shipmentWrapper.HouseBill;
							}
						}

						if (C5_OuterPacks.IsEmpty)
						{
							C5_OuterPacks = shipmentWrapper.Shipment.JS_OuterPacks;
						}

						if (C5_OuterPackUnits.IsEmpty)
						{
							C5_OuterPackUnits = SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(shipmentWrapper.Shipment.JS_F3_NKPackType);
						}

						if (consol != null)
						{
							if (C5_CargoType.IsEmpty)
							{
								C5_CargoType = CMRCargoType(consol.JK_ConsolMode);
							}

							if (IsFCX || IsFCL)
							{
								C5_MasterBill = C5_HouseBill = ZString.Empty;
							}
							else if (C5_MasterBill.IsEmpty)
							{
								C5_MasterBill = consol.JK_MasterBillNum;
							}
						}
					}
				}
			}
		}

		protected ZString CMRCargoType(ZString enterpriseCode)
		{
			ZString result = ZString.Empty;
			switch (enterpriseCode)
			{
				case Enterprise.Core.Constants.ContainerModes.FCL:
					result = CMRImportCargoTypes.Codes.FullContainerLoad;
					break;
				case Enterprise.Core.Constants.ContainerModes.FCLMixedShipper:
					result = CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills;
					break;
				case Enterprise.Core.Constants.ContainerModes.BreakBulk:
					result = CMRImportCargoTypes.Codes.BreakBulk;
					break;
				case Enterprise.Core.Constants.ContainerModes.Bulk:
					result = CMRImportCargoTypes.Codes.Bulk;
					break;
				default:
					result = CMRImportCargoTypes.Codes.LessThanContainerLoad;
					break;
			}
			return result;
		}

		public new CusUnderbond Underbond
		{
			get { return (CusUnderbond)base.Underbond; }
		}

		#endregion

		#region C5_C6
		public override ZGuid C5_C6
		{
			get => base.C5_C6;
			set
			{
				if (value != base.C5_C6)
				{
					base.C5_C6 = value;
					Header?.MarkAsNeedingValidation();
				}
			}
		}
		#endregion

		#region C5_CargoUnpackDate

		public override ZDateTime C5_CargoUnpackDate
		{
			get { return base.C5_CargoUnpackDate; }
			set
			{
				base.C5_CargoUnpackDate = value;
				if (!value.IsEmpty && value.IsValid && C5_CargoReceiptDate.IsEmpty)
				{
					C5_CargoReceiptDate = value;
				}

				if (MessageStatus.Code == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived)
				{
					string messageText = "The outturn [" + C5_ContainerNumber + "] - [" +
						C5_HouseBill + "/" + C5_MasterBill + "] has been rescinded!";

					Header.ShowPopupRescindMessageReceived(messageText);
				}
			}
		}

		#endregion

		#region C5_CargoReceiptDate

		public override ZDateTime C5_CargoReceiptDate
		{
			get { return base.C5_CargoReceiptDate; }
			set
			{
				base.C5_CargoReceiptDate = value;
				if (value.IsEmpty && !C5_CargoUnpackDate.IsEmpty)
				{
					C5_CargoUnpackDate = value;
				}

				if (MessageStatus.Code == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived)
				{
					string messageText = "The outturn [" + C5_ContainerNumber + "] - [" +
						C5_HouseBill + "/" + C5_MasterBill + "] has been rescinded!";

					Header.ShowPopupRescindMessageReceived(messageText);
				}
			}
		}

		#endregion

		#region C5_PackagesOutturned

		public override ZInt C5_PackagesOutturned
		{
			get
			{
				return base.C5_PackagesOutturned;
			}
			set
			{
				if (C5_PackagesUnits.IsEmpty && !value.IsEmpty)
				{
					C5_PackagesUnits = C5_OuterPackUnits;
				}

				base.C5_PackagesOutturned = value;
				CalculateOutturnResult();
			}
		}

		void CalculateOutturnResult()
		{
			if (!C5_OuterPackUnits.IsEmpty)
			{
				if (C5_OuterPacks == C5_PackagesOutturned)
				{
					C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
				}
				else if (C5_OuterPacks == 0)
				{
					C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
				}
				else if (C5_PackagesOutturned < C5_OuterPacks)
				{
					C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
				}
				else if (C5_PackagesOutturned > C5_OuterPacks)
				{
					C5_OutturnResultType = CMROutturnResultType.Codes.SurplusPackages;
				}
			}
		}

		#endregion

		#region C5_OuterPacks

		public override ZInt C5_OuterPacks
		{
			get
			{
				return base.C5_OuterPacks;
			}
			set
			{
				base.C5_OuterPacks = value;
				CalculateOutturnResult();
			}
		}

		#endregion

		#region C5_OutturnResultType

		public override ZString C5_OutturnResultType
		{
			get
			{
				return base.C5_OutturnResultType;
			}
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.C5_OutturnResultType))
				{
					base.C5_OutturnResultType = value;
				}
			}
		}

		#endregion

		#region C5_MessageStatus

		[ReadOnly(true)]
		public override ZString C5_MessageStatus
		{
			get { return base.C5_MessageStatus; }
			set
			{
				if (base.C5_MessageStatus != value && !IsCopying)
				{
					base.C5_MessageStatus = value;
					Header?.MarkAsNeedingValidation();
				}

				if (value == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived)
				{
					SetReadOnlyIncludingChildren(true);
				}
			}
		}

		#endregion

		#endregion

		#region Underbond Type

		protected override Type CusUnderbondType
		{
			get { return typeof(CusUnderbond); }
		}

		#endregion

		#region Message Status

		public readonly DepotCusOutturnUnderbondStatusCalculator MessageStatusCalculator;

		protected override Customs.Business.CusStatus MessageStatusCore
		{
			get { return new UnderbondCusStatus(C5_MessageStatusInfo, MessageStatusCalculator); }
		}

		#endregion

		protected override ZString GetCustomsOutlinedStatus(ZString cS_CustomsStatus)
		{
			return new CMRConsolidatedCargoStatuses().ConvertCustomsStatusReasonCodeToGenericCustomsStatus(cS_CustomsStatus);
		}

		#region Combined Status

		public CalculatedCusStatus CombinedStatus
		{
			get
			{
				if (combinedStatus == null)
				{
					combinedStatus = new CargoAndUnderbondCusStatus(RawCombinedStatusInfo, CombinedStatusCalculators);
				}
				return combinedStatus;
			}
		}
		protected CalculatedCusStatus combinedStatus;

		protected StatusCalculatorCombiner CombinedStatusCalculators
		{
			get
			{
				if (combinedStatusCalculators == null)
				{
					combinedStatusCalculators = new StatusCalculatorCombiner(new ICalculatedCusStatusCalculator[] { StatusCalculator, MessageStatusCalculator });
				}
				return combinedStatusCalculators;
			}
		}
		StatusCalculatorCombiner combinedStatusCalculators;

		#region RawCombinedStatus

		public ZPropertyInfo RawCombinedStatusInfo
		{
			get { return GetZPropertyInfo(Schema.RawCombinedStatus); }
		}

		public ZString RawCombinedStatus
		{
			get { return Combiner.GetCombinedStatus(CustomsStatus.Code, MessageStatus.Code); }
		}

		protected CARSTAndUnderbondStatusCombiner Combiner
		{
			get
			{
				if (combiner == null)
				{
					combiner = new CARSTAndUnderbondStatusCombiner();
				}
				return combiner;
			}
		}
		CARSTAndUnderbondStatusCombiner combiner;

		#endregion

		#endregion

#if DEBUG

		public void ResetCachedValuesForTesting()
		{
			_outturnStatus = null;
			_freightForwarderIndicator = null;
		}

#endif

		#region Outturn Status
		public ZPropertyInfo OutturnStatusInfo
		{
			get { return GetZPropertyInfo(Schema.OutturnStatus); }
		}

		public ZString OutturnStatus
		{
			get
			{
				if (!_outturnStatus.HasValue)
				{
					_outturnStatus = ZString.Empty;
					var header = Header;
					if (header != null && Messages.OfType<CMRSEAOUTRMessage>().Any())
					{
						if (header.HasSplitMessageOriginalRejectedLog)
						{
							_outturnStatus = CMRMessage.CMRMessageStatusDescription.ERROR;
						}
						else
						{
							var seaLine = new SEAOUTMessageLine(new DepotCusOutturnOutturnReportLineInformation(this));
							var customsLine = header.CustomsLines.LastOrDefault(x => x.UniqueIdentifier == seaLine.UniqueIdentifier);
							if (customsLine != null)
							{
								if (C5_CargoReceiptDate.IsEmpty && C5_CargoUnpackDate.IsEmpty)
								{
									_outturnStatus = CMRMessage.CMRMessageStatusDescription.WITHDRAWN;
								}
								else
								{
									_outturnStatus = customsLine.StringValue == seaLine.StringValue ? CMRMessage.CMRMessageStatusDescription.ACCEPTED : CMRMessage.CMRMessageStatusDescription.AMENDMENTDETECTED;
								}
							}
							else if (!ResponseIsPending)
							{
								_outturnStatus = header.CustomsLines.Length == 0 ? CMRMessage.CMRMessageStatusDescription.WITHDRAWN : CMRMessage.CMRMessageStatusDescription.ERROR;
							}
						}
					}
				}

				return _outturnStatus.Value;
			}
		}
		ZString? _outturnStatus;

		bool ResponseIsPending
		{
			get
			{
				return Header.C6_MessageStatus == CMRBaseStatuses.Codes.AwaitingResponseToOriginal
					|| Header.C6_MessageStatus == CMRBaseStatuses.Codes.AwaitingResponseToAmendment
					|| Header.C6_MessageStatus == CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;
			}
		}

		#endregion

		#region Use Parent Status

		protected override bool UseParentStatus
		{
			get { return false; }
		}

		#endregion

		#region Related Business Objects

		public CusOutturnHeader Header
		{
			get { return GetHeaderCore(); }
		}

		protected virtual CusOutturnHeader GetHeaderCore()
		{
			return Factory.Load<CusOutturnHeader>(C5_C6);
		}

		#endregion

		#region ISeaCargoEstablishmentQueryInformation members

		ZString ISeaCargoEstablishmentQueryInformation.ContainerNumber
		{
			get { return C5_ContainerNumber; }
		}

		ZString ISeaCargoEstablishmentQueryInformation.HouseBill
		{
			get { return C5_HouseBill; }
		}

		ZString ISeaCargoEstablishmentQueryInformation.OceanBill
		{
			get { return C5_MasterBill; }
		}

		ZString ISeaCargoEstablishmentQueryInformation.ResponsiblePartyID
		{
			get { return Header == null ? ZString.Empty : Header.C6_ResponsiblePartyID; }
		}

		ZString ISeaCargoEstablishmentQueryInformation.VesselID
		{
			get { return Header == null ? ZString.Empty : Header.C6_LloydsIMO; }
		}

		ZString ISeaCargoEstablishmentQueryInformation.VoyageNumber
		{
			get { return Header == null ? ZString.Empty : Header.C6_VoyageNum; }
		}

		ZString ISeaCargoEstablishmentQueryInformation.EstablishmentID
		{
			get { return Header == null ? ZString.Empty : Header.C6_OutturningPremiseID; }
		}

		#endregion

		#region ISendersMessageReferenceProvider Members

		public void PopulateSendersReferenceIfNeeded()
		{
			if (Header != null)
			{
				((ISendersMessageReferenceProvider)Header).PopulateSendersReferenceIfNeeded();
			}
		}

		public ZString SendersReference
		{
			get { return Header == null ? ZString.Empty : ((ISendersMessageReferenceProvider)Header).SendersReference; }
		}

		#endregion
	}
}
