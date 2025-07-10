using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using IContainerMessagingData = Enterprise.Integration.Customs.AU.IContainerMessagingData;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class ContainerMessagingData : IPRAMessagingData, IContainerMessagingData
	{
		#region Interface

		public ContainerMessagingData(BusinessObject dataSource)
		{
			Argument.NotNull(dataSource, "dataSource");
			SavedFactory = dataSource.Factory;
		}

		public abstract EDIMessage GetEDIMessage();

		public void Save()
		{
			CreatePRAEventLogs();

			SavedFactory.Save();

			var logger = ObjectFactory.Get<ILicenceConsumptionLogCreator>();
			logger.CreateLog(Env.Licence.PRAMessagingPerTransaction, true);
		}

		#endregion

		#region CreatePRAEventLogs

		void CreatePRAEventLogs()
		{
			if (JobContainer.Consol != null && JobContainer.PRAMessages != null)
			{
				EDIMessage lastPRAMessageSent = JobContainer.GetLastPRAMessageSent();

				if (lastPRAMessageSent != null)
				{
					if (lastPRAMessageSent.EM_MessageSubType == "SSM")
					{
						JobContainer.Logs.AddNew(Events.MessageSent, PRAMessageEvent.GetEventParameters(Events.MessageSent));
					}
					else if (lastPRAMessageSent.EM_MessageSubType == "SCN")
					{
						JobContainer.Logs.AddNew(Events.MessageWithdrawCancelRequest, PRAMessageEvent.GetEventParameters(Events.MessageWithdrawCancelRequest));
					}
				}
			}
		}

		#endregion

		#region abstract properties

		protected abstract string GetPortOfLoading();
		protected abstract string GetPortOfDischarge();
		protected abstract string GetVesselName();
		protected abstract string GetVoyage();
		protected abstract string GetLloydsNumber();
		protected abstract string GetECNorCRN();
		protected abstract string GetConsignorName();
		protected abstract string GetShippingLine1StopCode();
		protected abstract string GetLoadTerminal1StopCode();
		protected abstract string GetPortOfFinalDischarge();
		protected abstract string GetContainerNumber();
		protected abstract string GetSealNumber();
		protected abstract decimal GetContainerGrossWeight();
		protected abstract decimal GetContainerTareWeight();
		protected abstract decimal GetContainerNetWeight();
		protected abstract bool GetIsTempControlled();
		protected abstract string GetFlatRackID();
		protected abstract string GetTruckRegoNumber();
		protected abstract string GetRoadOrig1StopCode();
		protected abstract string GetRoadDest1StopCode();
		protected abstract ZDateTime GetRoadScheduledDeparture();
		protected abstract ZDateTime GetRoadScheduledArrival();
		protected abstract string GetCartageCompanyABN();
		protected abstract string GetCartageBookingReference();
		protected abstract string GetMessageReference();
		protected abstract string GetGoodsDescription();
		protected abstract string GetDateTimeStringForMessage();
		protected abstract DangerousGoodsCollection GetDangerousGoodsList();
		protected abstract CommonContainer GetJobContainer();
		protected abstract RefContainer GetRefContainer();
		protected abstract string GetGrossWeightVerifiedDeclarantSignature();
		protected abstract string GetGrossWeightVerifiedDeclarantContact();
		protected abstract string GetGrossWeightDeclarantCompanyName();

		protected abstract string ShippingLineBookingReferenceGUILocation { get; }
		protected abstract string ECNorCRNGUILocation { get; }
		protected abstract string ShippingLine1StopCodeGUILocation { get; }
		protected abstract string VesselNameGUILocation { get; }
		protected abstract string VoyageGUILocation { get; }
		protected abstract string LloydsNumberGUILocation { get; }
		protected abstract string PortOfLoadingGUILocation { get; }
		protected abstract string Terminal1StopCodeMissingGUILocation { get; }
		protected abstract string Terminal1StopCodeInvalidGUILocation { get; }
		protected abstract string PortOfDischargeGUILocation { get; }
		protected abstract string ContainerNumberGUILocation { get; }
		protected abstract string ISOContainerTypeGUILocation { get; }
		protected abstract string Commodity1StopCodeGUILocation { get; }
		protected abstract string ContainerGrossWeightGUILocation { get; }
		protected abstract string ContainerGrossWeightVerificationGUILocation { get; }
		protected abstract string SealNumberGUILocation { get; }
		protected abstract string GrossWeightVerifiedDeclarantLocation { get; }

		public abstract bool ContainerIsWaitingForResponse { get; }

		#endregion

		#region Consol Level FieldMappers

		public string PortOfLoading
		{
			get
			{
				if (portOfLoading == null)
				{
					portOfLoading = GetPortOfLoading();
				}
				return portOfLoading;
			}
			set
			{
				portOfLoading = value;
			}
		}
		string portOfLoading;

		public string PortOfDischarge
		{
			get
			{
				if (portOfDischarge == null)
				{
					portOfDischarge = GetPortOfDischarge();
				}
				return portOfDischarge;
			}
			set
			{
				portOfDischarge = value;
			}
		}
		string portOfDischarge;

		public string VesselName
		{
			get
			{
				if (vesselName == null)
				{
					vesselName = GetVesselName();
				}
				return vesselName;
			}
			set
			{
				vesselName = value;
			}
		}
		string vesselName;

		public string Voyage
		{
			get
			{
				if (voyage == null)
				{
					voyage = GetVoyage();
				}
				return voyage;
			}
			set
			{
				voyage = value;
			}
		}
		string voyage;

		public string LloydsNumber
		{
			get
			{
				if (lloydsNumber == null)
				{
					lloydsNumber = GetLloydsNumber();
				}
				return lloydsNumber;
			}
			set
			{
				lloydsNumber = value;
			}
		}
		string lloydsNumber;

		public string ECNorCRN
		{
			get
			{
				if (eCNorCRN == null)
				{
					eCNorCRN = GetECNorCRN();
				}
				return eCNorCRN;
			}
			set
			{
				eCNorCRN = value;
			}
		}
		string eCNorCRN;

		public string ShippingLineBookingReference
		{
			get
			{
				if (shippingLineBookingReference == null)
				{
					shippingLineBookingReference = GetShippingLineBookingReference();
				}
				return shippingLineBookingReference;
			}
			set
			{
				shippingLineBookingReference = value;
			}
		}
		string shippingLineBookingReference;

		public string ConsignorName
		{
			get
			{
				if (consignor.IsEmpty)
				{
					consignor = GetConsignorName();
				}
				return consignor;
			}
			set
			{
				consignor = value;
			}
		}
		ZString consignor;

		public string ShippingLine1StopCode
		{
			get
			{
				if (shippingLine1StopCode == null)
				{
					shippingLine1StopCode = GetShippingLine1StopCode();
				}
				return shippingLine1StopCode;
			}
			set
			{
				shippingLine1StopCode = value;
			}
		}
		string shippingLine1StopCode;

		public string LoadTerminal1StopCode
		{
			get
			{
				if (loadTerminal1StopCode == null)
				{
					loadTerminal1StopCode = GetLoadTerminal1StopCode();
				}
				return loadTerminal1StopCode;
			}
			set
			{
				loadTerminal1StopCode = value;
			}
		}
		string loadTerminal1StopCode;

		public string PortOfFinalDischarge
		{
			get
			{
				if (portOfFinalDischarge == null)
				{
					portOfFinalDischarge = GetPortOfFinalDischarge();
				}
				return portOfFinalDischarge;
			}
			set
			{
				portOfFinalDischarge = value;
			}
		}

		string portOfFinalDischarge;

		#endregion

		#region GlbStaff Field Mappers

		public string SenderContactName
		{
			get
			{
				if (senderContactName == null)
				{
					senderContactName = GlbStaff.CurrentUser.GS_FullName;
				}
				return senderContactName;
			}
			set
			{
				senderContactName = value;
			}
		}
		string senderContactName;

		public string SenderPhone
		{
			get
			{
				if (senderPhone == null)
				{
					senderPhone = GlbStaff.CurrentUser.GS_WorkPhone;
				}
				return senderPhone;
			}
			set
			{
				senderPhone = value;
			}
		}
		string senderPhone;

		public string SenderFax
		{
			get
			{
				if (senderFax == null)
				{
					senderFax = GlbStaff.CurrentUser.GS_FaxNum;
				}
				return senderFax;
			}
			set
			{
				senderFax = value;
			}
		}
		string senderFax;

		public string SenderCompanyName
		{
			get
			{
				if (senderCompanyName == null)
				{
					if (GlbStaff.CurrentUser.HomeBranch?.OrgProxy != null)
					{
						senderCompanyName = GlbStaff.CurrentUser.HomeBranch.OrgProxy.OH_FullName;
					}
					else if (GlbCompany.CurrentCompany?.OrgProxy != null)
					{
						senderCompanyName = GlbCompany.CurrentCompany.OrgProxy.OH_FullName;
					}
				}
				return senderCompanyName ?? string.Empty;
			}
			set
			{
				senderCompanyName = value;
			}
		}
		string senderCompanyName;

		public string SenderEmail
		{
			get
			{
				if (senderEmail == null)
				{
					senderEmail = GlbStaff.CurrentUser.GS_EmailAddress;
				}
				return senderEmail;
			}
			set
			{
				senderEmail = value;
			}
		}
		string senderEmail;

		#endregion

		#region RefContainer FieldMappers

		public bool HasTynes
		{
			get
			{
				if (!hasTynes)
				{
					hasTynes = GetHasTynes();
				}
				return (hasTynes == ZBool.True);
			}
			set { hasTynes = value; }
		}
		bool hasTynes;

		public string ISOContainerType
		{
			get
			{
				if (isoContainerType == null)
				{
					isoContainerType = GetISOContainerType();
				}
				return isoContainerType;
			}
			set
			{
				isoContainerType = value;
			}
		}
		string isoContainerType;

		public ContainerISOType ISOType
		{
			get
			{
				if (isoType == null)
				{
					isoType = new ContainerISOType();
				}
				isoType.ISOCode = ISOContainerType;
				return isoType;
			}
		}
		protected ContainerISOType isoType;

		#endregion

		#region Container FieldMappers

		public bool ArrivingAtCTOByRail
		{
			get
			{
				if (!arrivingAtCTOByRail)
				{
					arrivingAtCTOByRail = GetArrivingAtCTOByRail();
				}
				return (arrivingAtCTOByRail == ZBool.True);
			}
			set { arrivingAtCTOByRail = value; }
		}
		bool arrivingAtCTOByRail;

		public string ContainerNumber
		{
			get
			{
				if (containerNumber == null)
				{
					containerNumber = GetContainerNumber();
				}
				return containerNumber;
			}
			set
			{
				containerNumber = value;
			}
		}
		string containerNumber;

		public string SealNumber
		{
			get
			{
				if (sealNumber == null)
				{
					sealNumber = GetSealNumber();
				}
				return sealNumber;
			}
			set
			{
				sealNumber = value;
			}
		}
		string sealNumber;

		public bool IsEmptyContainer
		{
			get
			{
				if (!isEmptyContainer)
				{
					isEmptyContainer = GetIsEmptyContainer();
				}
				return (isEmptyContainer == ZBool.True);
			}
			set { isEmptyContainer = value; }
		}
		bool isEmptyContainer;

		public string Commodity1StopCode
		{
			get
			{
				if (commodity1StopCode == null)
				{
					commodity1StopCode = GetCommodity1StopCode();
				}
				return commodity1StopCode;
			}
			set
			{
				commodity1StopCode = value;
			}
		}
		string commodity1StopCode;

		public decimal ContainerGrossWeight
		{
			get
			{
				if (containerGrossWeight == decimal.MaxValue)
				{
					containerGrossWeight = GetContainerGrossWeight();
				}
				return containerGrossWeight;
			}
			set
			{
				containerGrossWeight = value;
			}
		}
		decimal containerGrossWeight = decimal.MaxValue;

		public decimal ContainerTareWeight
		{
			get
			{
				if (containerTareWeight == decimal.MaxValue)
				{
					containerTareWeight = GetContainerTareWeight();
				}
				return containerTareWeight;
			}
			set
			{
				containerTareWeight = value;
			}
		}
		decimal containerTareWeight = decimal.MaxValue;

		public decimal ContainerNetWeight
		{
			get
			{
				if (containerNetWeight == decimal.MaxValue)
				{
					containerNetWeight = GetContainerNetWeight();
				}
				return containerNetWeight;
			}
			set
			{
				containerNetWeight = value;
			}
		}
		decimal containerNetWeight = decimal.MaxValue;

		public int AirVentSetting
		{
			get
			{
				if (airVentSetting == int.MaxValue)
				{
					airVentSetting = GetAirVentSetting();
				}
				return airVentSetting;
			}
			set
			{
				airVentSetting = value;
			}
		}
		int airVentSetting = int.MaxValue;

		public int HumidityPercentage
		{
			get
			{
				if (humidityPercentage == int.MaxValue)
				{
					humidityPercentage = GetHumidityPercentage();
				}
				return humidityPercentage;
			}
			set
			{
				humidityPercentage = value;
			}
		}
		int humidityPercentage = int.MaxValue;

		public string AirVentSettingUnit
		{
			get
			{
				if (airVentSettingUnit == null)
				{
					airVentSettingUnit = GetAirVentSettingUnit();
				}
				return airVentSettingUnit;
			}
			set
			{
				airVentSettingUnit = value;
			}
		}
		protected string airVentSettingUnit;

		public bool IsTempControlled
		{
			get
			{
				if (!isTempControlled)
				{
					isTempControlled = GetIsTempControlled();
				}
				return (isTempControlled == ZBool.True);
			}
			set { isTempControlled = value; }
		}
		bool isTempControlled;

		public string TemperatureSettingFormatted
		{
			get
			{
				if (temperatureSetting == null)
				{
					TemperatureSettingFormatted = GetTemperatureSettingFormatted();
				}
				return temperatureSetting;
			}
			set
			{
				temperatureSetting = TemperatureFormatter.FormatTemperatureString(value);
			}
		}
		string temperatureSetting;

		public int Temperature
		{
			get
			{
				if (temperature == 0)
				{
					Temperature = GetTemperature();
				}
				return temperature;
			}
			set
			{
				temperature = value;
			}
		}
		int temperature;

		public int OverhangFrontInCM
		{
			get
			{
				if (overhangFrontInCM == int.MaxValue)
				{
					overhangFrontInCM = GetOverhangFrontInCM();
				}
				return overhangFrontInCM;
			}
			set
			{
				overhangFrontInCM = value;
			}
		}
		int overhangFrontInCM = int.MaxValue;

		public int OverhangBackInCM
		{
			get
			{
				if (overhangBackInCM == int.MaxValue)
				{
					overhangBackInCM = GetOverhangBackInCM();
				}
				return overhangBackInCM;
			}
			set
			{
				overhangBackInCM = value;
			}
		}
		int overhangBackInCM = int.MaxValue;

		public int OverhangLeftInCM
		{
			get
			{
				if (overhangLeftInCM == int.MaxValue)
				{
					overhangLeftInCM = GetOverhangLeftInCM();
				}
				return overhangLeftInCM;
			}
			set
			{
				overhangLeftInCM = value;
			}
		}
		int overhangLeftInCM = int.MaxValue;

		public int OverhangRightInCM
		{
			get
			{
				if (overhangRightInCM == int.MaxValue)
				{
					overhangRightInCM = GetOverhangRightInCM();
				}
				return overhangRightInCM;
			}
			set
			{
				overhangRightInCM = value;
			}
		}
		int overhangRightInCM = int.MaxValue;

		public int OverhangHeightInCM
		{
			get
			{
				if (overhangHeightInCM == int.MaxValue)
				{
					overhangHeightInCM = GetOverhangHeightInCM();
				}
				return overhangHeightInCM;
			}
			set
			{
				overhangHeightInCM = value;
			}
		}
		int overhangHeightInCM = int.MaxValue;

		public string FlatRackID
		{
			get
			{
				if (flatRackID == null)
				{
					flatRackID = GetFlatRackID();
				}
				return flatRackID;
			}
			set
			{
				flatRackID = value;
			}
		}
		string flatRackID;

		public string ReeferGeneratorID
		{
			get
			{
				if (reeferGeneratorID == null)
				{
					reeferGeneratorID = GetReeferGeneratorID();
				}
				return reeferGeneratorID;
			}
			set
			{
				reeferGeneratorID = value;
			}
		}
		string reeferGeneratorID;

		public string TerminalVBSBooking
		{
			get
			{
				if (terminalVBSBooking == null)
				{
					terminalVBSBooking = GetTerminalVBSBooking();
				}
				return terminalVBSBooking;
			}
			set
			{
				terminalVBSBooking = value;
			}
		}
		string terminalVBSBooking;

		public string TruckRegoNumber
		{
			get
			{
				if (truckRegoNumber == null)
				{
					truckRegoNumber = GetTruckRegoNumber();
				}
				return truckRegoNumber;
			}
			set
			{
				truckRegoNumber = value;
			}
		}
		string truckRegoNumber;

		public string RoadOrig1StopCode
		{
			get
			{
				if (roadOrig1StopCode == null)
				{
					roadOrig1StopCode = GetRoadOrig1StopCode();
				}
				return roadOrig1StopCode;
			}
			set
			{
				roadOrig1StopCode = value;
			}
		}
		string roadOrig1StopCode;

		public string RoadDest1StopCode
		{
			get
			{
				if (roadDest1StopCode == null)
				{
					roadDest1StopCode = GetRoadDest1StopCode();
				}
				return roadDest1StopCode;
			}
			set
			{
				roadDest1StopCode = value;
			}
		}
		string roadDest1StopCode;

		public ZDateTime RoadScheduledDeparture
		{
			get
			{
				if (roadScheduledDeparture == ZDateTimeNotFilledInYet)
				{
					roadScheduledDeparture = GetRoadScheduledDeparture();
				}
				return roadScheduledDeparture;
			}
			set
			{
				roadScheduledDeparture = value;
			}
		}
		ZDateTime roadScheduledDeparture = ZDateTimeNotFilledInYet;

		public ZDateTime RoadScheduledArrival
		{
			get
			{
				if (roadScheduledArrival == ZDateTimeNotFilledInYet)
				{
					roadScheduledArrival = GetRoadScheduledArrival();
				}
				return roadScheduledArrival;
			}
			set
			{
				roadScheduledArrival = value;
			}
		}
		ZDateTime roadScheduledArrival = ZDateTimeNotFilledInYet;

		#endregion

		#region Container Cartage Company FieldMappers

		public string CartageCompanyABN
		{
			get
			{
				if (cartageCompanyABN == null)
				{
					cartageCompanyABN = GetCartageCompanyABN();
				}
				return cartageCompanyABN;
			}
			set
			{
				cartageCompanyABN = value;
			}
		}
		string cartageCompanyABN;

		public string CartageBookingReference
		{
			get
			{
				if (cartageBookingReference == null)
				{
					cartageBookingReference = GetCartageBookingReference();
				}
				return cartageBookingReference;
			}
			set
			{
				cartageBookingReference = value;
			}
		}
		string cartageBookingReference;

		#endregion

		#region JobContainer Properties

		protected int GetAirVentSetting()
		{
			return (ZInt)JobContainer.JC_AirVentFlow;
		}

		protected virtual string GetShippingLineBookingReference()
		{
			string result = string.Empty;

			if (JobContainer.Consol != null)
			{
				result = JobContainer.Consol.JK_BookingReference;
			}

			if (string.IsNullOrEmpty(result))
			{
				result = JobContainer.JC_ReleaseNum;
			}

			return result;
		}

		protected bool GetArrivingAtCTOByRail()
		{
			return JobContainer.JC_DepartureDeliveryByRail;
		}

		protected int GetHumidityPercentage()
		{
			return Convert.ToInt16(JobContainer.JC_HumidityPercent);
		}

		protected string GetAirVentSettingUnit()
		{
			return JobContainer.JC_AirVentFlowRateUnit;
		}

		protected string GetTemperatureSettingFormatted()
		{
			return JobContainer.JC_SetPointTemp.ToString();
		}

		protected string GetReeferGeneratorID()
		{
			return JobContainer.JC_RefrigGeneratorID;
		}

		protected string GetTerminalVBSBooking()
		{
			return JobContainer.JC_DepartureSlotReference;
		}

		protected int GetTemperature()
		{
			return (ZInt)JobContainer.JC_SetPointTemp;
		}

		protected string GetCommodity1StopCode()
		{
			return JobContainer.JC_RH_NKContainerCommodityCode;
		}

		protected bool GetIsEmptyContainer()
		{
			return ComoditySaysEmpty(JobContainer.JC_RH_NKContainerCommodityCode);
		}

		public string GrossWeightVerifiedType
		{
			get
			{
				if (grossWeightVerifiedType.IsNullOrEmpty())
				{
					switch (JobContainer.JC_GrossWeightVerificationType)
					{
						case Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container:
							grossWeightVerifiedType = PRAConstants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
							break;

						case Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages:
							grossWeightVerifiedType = PRAConstants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
							break;
						case Core.Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal:
							grossWeightVerifiedType = PRAConstants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;
							break;
						default:
							grossWeightVerifiedType = "";
							break;
					}
				}
				return grossWeightVerifiedType;
			}
			set
			{
				grossWeightVerifiedType = value;
			}
		}
		string grossWeightVerifiedType;

		public ZDateTime GrossWeightVerifiedDateTime
		{
			get
			{
				if (grossWeightVerifiedDateTime.IsEmpty)
				{
					grossWeightVerifiedDateTime = JobContainer.JC_GrossWeightVerificationDateTime;
				}
				return grossWeightVerifiedDateTime;
			}
			set
			{
				grossWeightVerifiedDateTime = value;
			}
		}
		ZDateTime grossWeightVerifiedDateTime;

		public string GrossWeightVerifiedByAddress
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				var address = JobContainer.GrossWeightVerifiedByAddress;
				if (address != null && !address.IsEmpty)
				{
					var grossWeightVerifiedAddress = string.Join(" ", new ZString[] { address.E2_Address1, address.E2_Address2 }.Where(x => !x.IsEmpty)).Trim();

					result.Append(address.E2_CompanyName);
					result.Append(grossWeightVerifiedAddress);
					result.Append(address.E2_City);
					if (address.Country != null)
					{
						result.Append(address.Country.Code);
					}
				}

				return result.Length > 0
					? result.ToStringWithDelimiterBetweenAppends(";")
					: string.Empty;
			}
		}

		public string GrossWeightDeclarantCompanyName
		{
			get
			{
				if (string.IsNullOrWhiteSpace(grossWeightDeclarantCompanyName))
				{
					grossWeightDeclarantCompanyName = GetGrossWeightDeclarantCompanyName();
				}
				return grossWeightDeclarantCompanyName;
			}
			set
			{
				grossWeightDeclarantCompanyName = value;
			}
		}
		string grossWeightDeclarantCompanyName;

		public string GrossWeightVerifiedDeclarantSignature
		{
			get
			{
				if (grossWeightVerifiedDeclarantSignature == null)
				{
					grossWeightVerifiedDeclarantSignature = GetGrossWeightVerifiedDeclarantSignature();
				}
				return grossWeightVerifiedDeclarantSignature;
			}
			set
			{
				grossWeightVerifiedDeclarantSignature = value;
			}
		}
		string grossWeightVerifiedDeclarantSignature;

		public string GrossWeightVerifiedDeclarantContact
		{
			get
			{
				if (grossWeightVerifiedDeclarantContact == null)
				{
					grossWeightVerifiedDeclarantContact = GetGrossWeightVerifiedDeclarantContact();
				}
				return grossWeightVerifiedDeclarantContact;
			}
			set
			{
				grossWeightVerifiedDeclarantContact = value;
			}
		}
		string grossWeightVerifiedDeclarantContact;

		public CommonContainer JobContainer
		{
			get
			{
				if (jobContainer == null)
				{
					jobContainer = GetJobContainer();
				}
				return jobContainer;
			}
		}
		CommonContainer jobContainer;

		public ICommonContainer GetCommonContainer
		{
			get
			{
				return JobContainer;
			}
		}

		#endregion

		#region RefContainer Properties

		protected bool GetHasTynes()
		{
			return RefContainer.RC_HasTynes;
		}

		protected string GetISOContainerType()
		{
			return RefContainer.RC_ISOType;
		}

		protected int GetOverhangFrontInCM()
		{
			return JobContainer.JC_Calc_OverhangFront > 0
				? System.Convert.ToInt16(Core.Constants.Length.Convert(JobContainer.JC_Calc_OverhangFront, Core.Constants.Length.Feet, Core.Constants.Length.Centimetres))
				: 0;
		}

		protected int GetOverhangBackInCM()
		{
			return JobContainer.JC_OverhangBack > 0
				? System.Convert.ToInt16(Core.Constants.Length.Convert(JobContainer.JC_OverhangBack, Core.Constants.Length.Feet, Core.Constants.Length.Centimetres))
				: 0;
		}

		protected int GetOverhangLeftInCM()
		{
			return JobContainer.JC_Calc_OverhangLeft > 0
				? System.Convert.ToInt16(Core.Constants.Length.Convert(JobContainer.JC_Calc_OverhangLeft, Core.Constants.Length.Feet, Core.Constants.Length.Centimetres))
				: 0;
		}

		protected int GetOverhangRightInCM()
		{
			return JobContainer.JC_OverhangRight > 0
				? System.Convert.ToInt16(Core.Constants.Length.Convert(JobContainer.JC_OverhangRight, Core.Constants.Length.Feet, Core.Constants.Length.Centimetres))
				: 0;
		}

		protected int GetOverhangHeightInCM()
		{
			int result = 0;
			if (JobContainer.JC_TotalHeight > RefContainer.RC_Height)
			{
				decimal overSizeInFeet = JobContainer.JC_TotalHeight - RefContainer.RC_Height;
				result = System.Convert.ToInt32(Core.Constants.Length.Convert(overSizeInFeet, Core.Constants.Length.Feet, Core.Constants.Length.Centimetres));
			}
			else
			{
				result = 0;
			}
			return result;
		}

		public RefContainer RefContainer
		{
			get { return refContainer ?? (refContainer = GetRefContainer()); }
		}
		RefContainer refContainer;

		#endregion

		#region Other DataMappers

		public string MessageReference
		{
			get
			{
				if (messageReference == null)
				{
					messageReference = GetMessageReference();
				}
				return messageReference;
			}
			set
			{
				messageReference = value;
			}
		}
		string messageReference;

		public string SenderID
		{
			get
			{
				if (senderID == null)
				{
					var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
					var enterpriseCode = registrationKey.EnterpriseCode;
					var serverCode = registrationKey.ServerCode;

					senderID = (enterpriseCode.Length == 0
						? (string)GlbCompany.CurrentCompany.GC_Code
						: enterpriseCode)
						+ serverCode;
				}
				return senderID;
			}
			set
			{
				senderID = value;
			}
		}
		string senderID;

		public string GoodsDescription
		{
			get
			{
				if (goodsDescription == null)
				{
					goodsDescription = GetGoodsDescription();
				}
				return goodsDescription;
			}
			set
			{
				goodsDescription = value;
			}
		}
		string goodsDescription;

		public string DateTimeStringForMessage
		{
			get
			{
				if (dateTimeStringForMessage == null)
				{
					dateTimeStringForMessage = GetDateTimeStringForMessage();
				}
				return dateTimeStringForMessage;
			}
			set
			{
				dateTimeStringForMessage = value;
			}
		}
		string dateTimeStringForMessage;

		#endregion

		#region Dangerous Goods

		public DangerousGoodsCollection DangerousGoodsList
		{
			get
			{
				if (dangerousGoodsList == null)
				{
					dangerousGoodsList = GetDangerousGoodsList();
				}
				return dangerousGoodsList;
			}
		}
		DangerousGoodsCollection dangerousGoodsList;

		#endregion

		#region Implementation

		static bool ComoditySaysEmpty(ZString comodityCode)
		{
			switch (comodityCode)
			{
				case "MT":
				case "MTHZ":
					return true;

				default:
					return false;
			}
		}

		protected static readonly ZDateTime ZDateTimeNotFilledInYet = ZDateTime.MaxSmallDateTime;
		protected readonly BusinessObjectFactory SavedFactory;

		#endregion

		#region CheckMissingData

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ZString GetErrorText()
		{
			StringBuilder result = new StringBuilder();

			if (string.IsNullOrWhiteSpace(ShippingLineBookingReference))
			{
				result.Append("Shipping Line Booking Reference. (" + ShippingLineBookingReferenceGUILocation + ")\r\n");
			}

			if (string.IsNullOrWhiteSpace(ECNorCRN) && !ComoditySaysEmpty(Commodity1StopCode))
			{
				result.Append(ECNorCRNGUILocation);
			}

			if (string.IsNullOrWhiteSpace(ShippingLine1StopCode))
			{
				result.Append("Carrier 1-Stop Code. (" + ShippingLine1StopCodeGUILocation + ")\r\n");
			}
			else if (ShippingLine1StopCode.Length != 3)
			{
				result.AppendLine("Carrier 1-Stop Code. (" + ShippingLine1StopCodeGUILocation + ") Must be three characters");
			}

			if (string.IsNullOrWhiteSpace(VesselName))
			{
				result.Append("Vessel Name. (" + VesselNameGUILocation + ")\r\n");
			}

			if (string.IsNullOrWhiteSpace(Voyage))
			{
				result.Append("Voyage. (" + VoyageGUILocation + ")\r\n");
			}

			if (string.IsNullOrWhiteSpace(LloydsNumber))
			{
				result.Append("Lloyds Number. (" + LloydsNumberGUILocation + ")\r\n");
			}

			if (string.IsNullOrWhiteSpace(PortOfLoading))
			{
				result.Append("Port Of Loading. (" + PortOfLoadingGUILocation + ")\r\n");
			}

			if (string.IsNullOrWhiteSpace(LoadTerminal1StopCode))
			{
				result.Append("Departure CTO 1-Stop Code. (" + Terminal1StopCodeMissingGUILocation + ")\r\n");
			}
			else
			{
				MessagePartyPairList cTOList = new MessagePartyPairList();
				if (!cTOList.ContainsCode(LoadTerminal1StopCode))
				{
					result.Append("Departure CTO 1-Stop Code is Invalid. (" + Terminal1StopCodeInvalidGUILocation + ")\r\n");
				}
			}

			if (string.IsNullOrWhiteSpace(PortOfFinalDischarge))
			{
				result.Append("Port Of Discharge. (" + PortOfDischargeGUILocation + ")\r\n");
			}

			if (string.IsNullOrWhiteSpace(ContainerNumber))
			{
				result.Append("Container Number. (" + ContainerNumberGUILocation + ")\r\n");
			}

			if (string.IsNullOrWhiteSpace(ISOContainerType))
			{
				result.Append("ISO Container Type. (" + ISOContainerTypeGUILocation + ")\r\n");
			}

			if (string.IsNullOrWhiteSpace(Commodity1StopCode) || CommodityCode == null)
			{
				result.Append("Commodity 1-Stop Code. (" + Commodity1StopCodeGUILocation + ")\r\n");
			}

			if (ContainerGrossWeight == 0m)
			{
				result.Append("Container Gross Weight. (" + ContainerGrossWeightGUILocation + ")\r\n");
			}

			if (string.IsNullOrWhiteSpace(SealNumber))
			{
				result.Append("Seal Number Not Entered. (" + SealNumberGUILocation + ")\r\n");
			}

			if (GrossWeightVerifiedType == PRAConstants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal)
			{
				if (CommodityCode != null && CommodityCode.RH_Code == PRAConstants.WATCommodityNotAllowed.OutOfGauge)
				{
					result.AppendLine(string.Format(CultureInfo.InvariantCulture, "Verified Method {0} is not valid for Commodity {1}. ({2})",
						Core.Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal,
						CommodityCode.RH_Code,
						Commodity1StopCodeGUILocation));
				}

				if (string.IsNullOrWhiteSpace(LoadTerminal1StopCode) || LoadTerminal1StopCode != PRAConstants.VictoriaInternationalContainerTerminal)
				{
					result.AppendLine(string.Format(CultureInfo.InvariantCulture, "Verified Method {0} is only valid for VICT CTO. ({1})",
						Core.Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal,
						Terminal1StopCodeMissingGUILocation));
				}

				if (PortOfLoading != FreightConstants.VICTPort.AUMEL)
				{
					result.AppendLine(string.Format(CultureInfo.InvariantCulture, "Verified Method {0} is not valid for Load Port other than {1}. ({2})",
						Core.Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal,
						FreightConstants.VICTPort.AUMEL,
						PortOfLoadingGUILocation));
				}
			}

			if (GrossWeightVerifiedType == PRAConstants.ContainerGrossWeightVerificationTypes.Codes.Method1Container
				|| GrossWeightVerifiedType == PRAConstants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages
				|| GrossWeightVerifiedType == PRAConstants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal)
			{
				if (!GrossWeightVerifiedDateTime.IsValid)
				{
					result.Append("Container Verified Weight Date must be provided. (" + ContainerGrossWeightVerificationGUILocation + ")\r\n");
				}

				if (string.IsNullOrWhiteSpace(GrossWeightVerifiedDeclarantSignature))
				{
					result.Append("Declarant contact must be provided. (" + GrossWeightVerifiedDeclarantLocation + ")\r\n");
				}

				if (string.IsNullOrWhiteSpace(GrossWeightDeclarantCompanyName))
				{
					result.Append("Declarant company name must be provided. (" + GrossWeightVerifiedDeclarantLocation + ")\r\n");
				}

				if (string.IsNullOrWhiteSpace(GrossWeightVerifiedDeclarantContact))
				{
					result.Append("Either Phone or Email of Declarant must be provided. (" + GrossWeightVerifiedDeclarantLocation + ")\r\n");
				}
			}
			else if (!IsEmptyContainer)
			{
				result.Append(string.Format(CultureInfo.InvariantCulture, "Container Verified Weight Method must be {0} or {1}. (" + ContainerGrossWeightVerificationGUILocation + ")\r\n",
					Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container,
					Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages));
			}

			return result + DangerousGoodsList.GetErrors();
		}

		RefCommodityCode CommodityCode
		{
			get { return commodityCode ?? (commodityCode = SavedFactory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, Commodity1StopCode)); }
		}
		RefCommodityCode commodityCode;

#if DEBUG
		internal void ResetCacheForTesting()
		{
			commodityCode = null;
		}
#endif

		public ZString GetWarningText()
		{
			StringBuilder result = new StringBuilder();

			if (CommodityCode != null)
			{
				if (CommodityCode.RH_ReeferMinTemperature != 0 && CommodityCode.RH_ReeferMaxTemperature != 0)
				{
					if (Temperature < CommodityCode.RH_ReeferMinTemperature || Temperature > CommodityCode.RH_ReeferMaxTemperature)
					{
						result.Append("Temperature must be between " + CommodityCode.RH_ReeferMinTemperature + " and " + CommodityCode.RH_ReeferMaxTemperature + " for the commodity code " + CommodityCode.RH_Code + " (" + CommodityCode.RH_DescriptionMultilingual + ").\r\n");
					}
				}

				if (CommodityCode.RH_ContainerVentRequired && AirVentSetting == 0)
				{
					result.Append("Commodity code " + Commodity1StopCode + " requires a container air vent setting to be entered.\r\n");
				}

				if (!CommodityCode.RH_ExpiryDate.IsEmpty)
				{
					if (CommodityCode.RH_ExpiryDate < ZDateTime.Now)
					{
						result.Append("This Commodity code is marked as expired as of " + CommodityCode.RH_ExpiryDate + ".\r\n");
					}
				}
			}

			if (Commodity1StopCode == "GENL" && IsTempControlled)
			{
				result.Append("Commodity Code should not be 'GENL' for Temperature Controlled Containers.\r\n");
			}

			if (Commodity1StopCode == "GENL" && DangerousGoodsList.Count > 0)
			{
				result.Append("Commodity Code should not be 'GENL' for Containers containing Hazardous Goods.\r\n");
			}

			bool containerIs20Foot = ISOContainerType.StartsWith("2");
			if (containerIs20Foot)
			{
				if (IsTempControlled)
				{
					if (ContainerGrossWeight < 3000m)
					{
						result.Append("Gross Weight should not be less than 3000kg for a 20' Reefer\r\n");
					}
				}
				else
				{
					if (ContainerGrossWeight < 2300m)
					{
						result.Append("Gross Weight should not be less than 2300kg for a 20' Container\r\n");
					}
				}
			}

			bool containerIs40Foot = ISOContainerType.StartsWith("4");
			if (containerIs40Foot)
			{
				if (IsTempControlled)
				{
					if (ContainerGrossWeight < 4500m)
					{
						result.Append("Gross Weight should not be less than 4500kg for a 40' Reefer\r\n");
					}
				}
				else
				{
					if (ContainerGrossWeight < 4000m)
					{
						result.Append("Gross Weight should not be less than 4000kg for a 40' Container\r\n");
					}
				}
			}

			if (OverhangHeightInCM != 0 || OverhangFrontInCM != 0 || OverhangBackInCM != 0 || OverhangLeftInCM != 0 || OverhangRightInCM != 0)
			{
				if (!ISOType.OverDimensionAllowed)
				{
					result.Append("This container is being sent as 'Over Size' which is not normal for this ISO Container Type\r\n");
				}
			}

			if (ISOContainerType == "2000" || ISOContainerType == "20G0")
			{
				result.Append("1-Stop have advised us that there are no containers of ISO Type 2000 or 20G0\r\n");
				result.Append("   in service, and that they will reject PRA messages using this type. Please\r\n");
				result.Append("   make sure this is the ISO Container Type marked on the container itself.\r\n");
			}

			return result.ToString() + DangerousGoodsList.GetErrors();
		}

		#endregion
	}
}
