using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class PartyConsigneeProvider : PartyAddressProvider, IEMCSPartyConsignee
	{
		PartyConsigneeProvider(JobDocAddress jobDocAddress) : base(jobDocAddress)
		{
			var codeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;
			var destinationTypeCode = (jobDocAddress.Parent as EMCSJobDeclaration)?.JE_MessageSubType.ToString() ?? string.Empty;
			var emitTraderId = destinationTypeCode != EMCSDestinationTypeList.Codes.DestinationExemptedConsignee && destinationTypeCode != EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown;
			var emitEORI = destinationTypeCode == EMCSDestinationTypeList.Codes.DestinationExport;

			if (jobDocAddress.E2_AddressOverride)
			{
				TraderId = emitTraderId && jobDocAddress.E2_GovRegNumType == codeType ? jobDocAddress.E2_GovRegNum.ToString() : string.Empty;
				EoriNumber = emitEORI && jobDocAddress.E2_GovRegNumType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori ? jobDocAddress.E2_GovRegNum.ToString() : string.Empty;
			}
			else
			{
				var org = jobDocAddress.Organisation;
				TraderId = emitTraderId ? org.GetCustomsRegNoIgnoringCountry(codeType).ToString() : string.Empty;
				EoriNumber = emitEORI ? org.GetEoriDetails().ToString() : string.Empty;
			}
		}

		public static new PartyConsigneeProvider NewOrNull(JobDocAddress jobDocAddress) => jobDocAddress != null && jobDocAddress.IsValidAddress ? new PartyConsigneeProvider(jobDocAddress) : null;

		public string EoriNumber { get; }

		public string TraderId { get; }
	}
}
