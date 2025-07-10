using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Chief;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.CNS
{
	public class CnsTransmissionMessageGenerator : GbChiefEdifactTransmissionMessageGenerator
	{
		public CnsTransmissionMessageGenerator(Customs.Business.CusdecMessageFunction declarationMessageFunction)
			: base(declarationMessageFunction)
		{ }

		protected override ZString GetApplicationCode(Customs.Business.CusEntryHeader entry)
		{
			return ApplicationCodeList.Codes.GbCnsEdifactOutboundOnly;  // not "CNS"
		}

		protected override ZString GetApplicationReference(CusEntryHeader entryHeader)
		{
			return "CNS/" + entryHeader.Declaration.JE_CustomsProfile;
		}
	}
}
