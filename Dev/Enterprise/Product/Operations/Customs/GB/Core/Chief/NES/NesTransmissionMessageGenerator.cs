using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Chief.NES
{
	public class NesTransmissionMessageGenerator : GbChiefEdifactTransmissionMessageGenerator
	{
		public NesTransmissionMessageGenerator(Customs.Business.CusdecMessageFunction declarationMessageFunction) : base(declarationMessageFunction)
		{
			this.declarationMessageFunction = declarationMessageFunction;
		}

		protected override ZString GetApplicationCode(Customs.Business.CusEntryHeader entry)
		{
			return NesConstants.ApplicationCode;
		}

		protected override ZString GetApplicationReference(CusEntryHeader entryHeader)
		{
			return NesConstants.ApplicationCode + "/" + entryHeader.Declaration.JE_CustomsProfile;
		}
	}
}
