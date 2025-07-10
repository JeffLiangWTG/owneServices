using CargoWise.Types;
using Enterprise.Customs.GB.Chief;
using Enterprise.Messaging.Integration;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;

namespace Enterprise.Customs.GB.Ccsuk.Declaration
{
	public class CcsukTransmissionMessageGenerator : GbChiefEdifactTransmissionMessageGenerator
	{
		public CcsukTransmissionMessageGenerator(Customs.Business.CusdecMessageFunction declarationMessageFunction)
			: base(declarationMessageFunction)
		{ }

		protected override ZString GetApplicationCode(CusEntryHeader entry)
		{
			return ApplicationCodeList.Codes.GbCcsuk;
		}

		// Allows us to send to CHIEF.... later we will be able to send to other participants on the CUK network
		protected override ZString GetApplicationReference(EU.Business.Declaration.CusEntryHeader entryHeader)
		{
			return string.Format("CUK/CHIEF/{0}/{1}", entryHeader.Declaration.JE_CustomsProfile, entryHeader.Declaration.JE_MessageType);  // we need import/export info to work out the Chief PIMA
		}
	}
}
