using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;
using Enterprise.Customs.GB.Registry;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business
{
	public class NctsHeader : EU.NCTS.Business.NctsHeader
		, Integration.Customs.GB.ICusInBondHeader
	{
		public NctsHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		readonly ZString[] arrivalMessageTypes = { "007", "044" };
		readonly ZString[] departureMessageTypes = { "015", "014", "013", "170" };

		public ZString GetServiceReference(ZString outboundMessageType)
		{
			var serviceReference = "Cannot determine service ID for " + outboundMessageType;

			if (arrivalMessageTypes.Contains(outboundMessageType))
			{
				serviceReference = ArrivalID;
			}
			else if (departureMessageTypes.Contains(outboundMessageType))
			{
				serviceReference = DepartureID;
			}

			return serviceReference;
		}

		ZString ArrivalID => GetReferenceFromMatchingMessage(arrivalMessageTypes);

		ZString DepartureID => GetReferenceFromMatchingMessage(departureMessageTypes);

		ZString GetReferenceFromMatchingMessage(ZString[] matchingTypes)
		{
			return LinkedMessages.OfType<EDIMessage>().Where(m => m.IsTransmitMessage
															&& !m.EM_ApplicationReference.IsEmpty
															&& matchingTypes.Contains(GetMessageTypeBasdedOnApplicationCode(m)))
				.OrderBy(m => m.EM_SystemCreateTimeUtc).LastOrDefault()?.EM_ApplicationReference ?? ZString.Empty;
		}

		ZString GetMessageTypeBasdedOnApplicationCode(EDIMessage message)
		{
			return message.EM_ApplicationCode.Equals(EDIInterchange.ApplicationCodes.GbCommonTransitConvention) ? message.EM_MessageSubType :
							message.EM_ApplicationCode.Equals(EDIInterchange.ApplicationCodes.GbCustomsNCTS) ? message.EM_MessageType : ZString.Empty;
		}

		public new NctsDepartureMovementHeader MovementHeader => (NctsDepartureMovementHeader)base.MovementHeader;

		public new INctsGuaranteeCollection<Guarantee> Guarantees => (INctsGuaranteeCollection<Guarantee>)base.Guarantees;

		protected override INctsGuaranteeCollection<NctsGuarantee> GetGuaranteesForNonPhase5Departure() => new NctsGuaranteeCollection<Guarantee>(this);

		public EDIMessage GetOutgoingMessage(EDIMessage inboundMessage)
		{
			var matchingOutgoingMessage = LinkedMessages.Cast<EDIMessage>().FirstOrDefault(m => inboundMessage.EM_InterchangeNumber == m.EM_InterchangeNumber + "." && m.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit);
			return matchingOutgoingMessage ?? LinkedMessages.LastOutgoingMessage ?? GetDepartureArrivalOutgoingMessage(inboundMessage);
		}

		EDIMessage GetDepartureArrivalOutgoingMessage(EDIMessage inboundMessage)
		{
			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, inboundMessage.EM_LinkUniqueID);
			_ = query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, NctsMovementType.Codes.Departure);
			_ = query.AddToFilter(JoinCondition.Or, CusInBondMoveHeaderSchema.BM_SubApplicationCode, NctsMovementType.Codes.DepartureAndArrival);
			var movementHeader = Factory.LoadTop1<NctsDepartureMovementHeader>(query);
			return movementHeader.Messages.LastOutgoingMessage;
		}

		protected override ZString[] GetDataGroupsForDestinationOfficeLookupCore() => new ZString[] { Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, Core.Constants.CountryCodes.UnitedKingdom };

		protected override NctsIE29CusdecParser GetNctsIE29CusdecParser()
		{
			if (IsPhase5)
			{
				return new GBPhase5CtcIE29CusdecParser();
			}
			else
			{
				return new GBCtcIE29CusdecParser();
			}
		}

		public virtual EDIMessageCollection LinkedMessages => IsPhase5Departure ? MovementHeader.Messages : Messages;

		protected override ZString GetFallbackInformationCore()
		{
			if (IsPhase5 && GBCustomsDataRegistry.Instance.NCTS_Fallback_Business_Continuity_Is_Active.Value && MovementHeader != null)
			{
				var office = MovementHeader.DepartureCustomsOffice?.OfficeCode;
				var officeOfDeparture = string.IsNullOrEmpty(office) ? ZString.Empty : (ZString)MovementHeader.Lookups.OfficeCodeList.GetDescriptionFromCode(office);
				var jobNumber = BH_JobReference;
				var consignorName = Consignor.E2_CompanyName;
				var date = ZDateTime.Today.ToString("dd/MM/yy");
				var consignorAuthNumber = MovementHeader.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Code == Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit)?.AGC_Number ?? ZString.Empty;
				var leftWidth = Math.Max(CountryCode.Length, Math.Max(jobNumber.Length, consignorName.Length)) + 4;
				return $@"UK {officeOfDeparture.PadRight(leftWidth)}
{jobNumber.PadRight(leftWidth)}{date}
{consignorName.PadRight(leftWidth)}{consignorAuthNumber}";
			}
			return ZString.Empty;
		}
	}
}
