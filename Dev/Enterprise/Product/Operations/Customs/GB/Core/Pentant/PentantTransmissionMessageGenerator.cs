using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Chief;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.Pentant
{
	public class PentantTransmissionMessageGenerator : GbChiefEdifactTransmissionMessageGenerator
	{
		public PentantTransmissionMessageGenerator(Customs.Business.CusdecMessageFunction declarationMessageFunction)
			: base(declarationMessageFunction)
		{ }

		protected override ZString GetApplicationCode(Customs.Business.CusEntryHeader entry)
		{
			return ApplicationCodeList.Codes.Pentant; //PNT
		}

		protected override ZString GetApplicationReference(CusEntryHeader entryHeader)
		{
			return ApplicationCodeList.Codes.Pentant + entryHeader.Declaration.JE_CustomsProfile;
		}
	}
}
