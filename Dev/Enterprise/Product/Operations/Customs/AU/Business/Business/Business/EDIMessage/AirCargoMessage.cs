using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCargoMessage : EDIMessage, Integration.Customs.AU.IAirCargoMessage
	{
		public AirCargoMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Constants

		public const string PartShipmentReferencePlaceHolder = "<<PARTSHIPMENT REFERENCE PLACEHOLDER>>";
		public const string ShipmentReferencePlaceHolder = "<<SHIPMENT REFERENCE PLACEHOLDER>>";

		public static class MessageType
		{
			public const string CIREPT = "CIR";
			public const string CSINFO = "CSF";
			public const string CSTNOT = "CST";
			public const string CONTRL = "CLT";
		}

		public static class MessageSubType
		{
			public const string Original = "ORG";
			public const string Amendment = "AMD";
			public const string Withdraw = "ZRO";
			public const string PartShipment = "PRT";
			public const string UnderbondRequest = "UBR";
			public const string UnderbondAcquittal = "UBQ";
			public const string UnderbondCancel = "UBC";
		}

		public static class MessageSubTypeDescription
		{
			public const string Original = "Original";
			public const string Amendment = "Amendment including discrepancy";
			public const string Withdraw = "Zero-Landing";
			public const string PartShipment = "Part shipment";
			public const string UnderbondRequest = "Underbond request";
			public const string UnderbondAcquittal = "Underbond acquittal";
			public const string UnderbondCancel = "Underbond calcellation";
		}

		public class AirCargoMessageSubTypeList : CodeDescriptionPairList
		{
			public AirCargoMessageSubTypeList()
			{
				AddPair(MessageSubType.Original, MessageSubTypeDescription.Original);
				AddPair(MessageSubType.Amendment, MessageSubTypeDescription.Amendment);
				AddPair(MessageSubType.Withdraw, MessageSubTypeDescription.Withdraw);
				AddPair(MessageSubType.PartShipment, MessageSubTypeDescription.PartShipment);
				AddPair(MessageSubType.UnderbondRequest, MessageSubTypeDescription.UnderbondRequest);
				AddPair(MessageSubType.UnderbondAcquittal, MessageSubTypeDescription.UnderbondAcquittal);
				AddPair(MessageSubType.UnderbondCancel, MessageSubTypeDescription.UnderbondCancel);
			}
		}

		public static class MessageFunctionCode
		{
			public const string Add = "5";
			public const string Change = "2";
			public const string Cancel = "3";
			public const string UnderbondCancel = "4";
		}

		public static class NewStatus
		{
			public const string Zero_landed = "Z998";
			public const string NotSent = "NOT";
			public const string Waiting = "WAIT";
			public const string Rejected = "REJ";
			public const string Acquitted = "ACQT";

			public const string Cleared = "CLC";
			public const string Impeded = "IPD";
		}

		public const string WaitingDescription = "Waiting for response";
		public const string NotSentDescription = "Not Sent";

		public static class LOCQualifier
		{
			public const string Origin = "10";
			public const string Destination = "8";
			public const string Load = "6";
			public const string Discharge = "12";
			public const string UnderbondCurrent = "14";
		}

		public static class SQDQualifier
		{
			public const string TotalNumberOfPacks_24 = "24";
			public const string TotalNumberOfPacksThisHouseBill_21 = "21";
			public const string PiecesManifested_22 = "22";
			public const string PiecesLanded_23 = "23";
		}

		public static class UNSQualifier
		{
			public const string SummarySection = "S";
			public const string DetailSection = "D";
		}

		public static class RFFQualifier
		{
			public const string PhoneNumber = "PH";
			public const string ConsolidationHAWB = "CB";
		}

		public static class FTXQualifier
		{
			public const string Withdraw = "10";
			public const string UnderbondRequest = "00";
			public const string UnderbondAcquittal = "60";
			public const string UnderbondCancel = "00";
			public const string SurplusShipment = "20";
		}

		public static class NADQualifier
		{
			public const string Consignor = "CZ";
			public const string Consignee = "CN";
		}

		public bool IsMessageResponded
		{
			get
			{
				return EM_MessageSubType != MessageSubTypes.Withdraw.Code
					&& EM_MessageSubType != MessageSubTypes.UnderbondAcquittal.Code;
				//This is work-around before we know why Customs sometimes dont respond to Acquittal report
			}
		}

		#endregion

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			throw new NotSupportedException("AirCargo message left only for legacy support");
		}

		protected CusHAWB HouseBill
		{
			get { return Factory.Load<CusHAWB>(EM_LinkUniqueID); }
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.AirCargo;
			EM_Status = EDIMessage.Status.Queued;
			EM_IsTestMessage = Env.Registry.AUCustomsAirCargoTestMode;
		}

		CodeDescriptionPairList fMessageSubTypeList;

		protected internal CodeDescriptionPairList MessageSubTypeListInternal => MessageSubTypeList;
		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get
			{
				if (fMessageSubTypeList == null)
				{
					fMessageSubTypeList = new AirCargoMessageSubTypeList();
				}
				return fMessageSubTypeList;
			}
		}

		#endregion
	}
}
