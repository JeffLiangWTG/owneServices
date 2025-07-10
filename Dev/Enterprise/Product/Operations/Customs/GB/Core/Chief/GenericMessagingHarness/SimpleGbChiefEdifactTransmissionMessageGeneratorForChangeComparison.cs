using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.GB.Chief.GenericMessagingHarness
{
	public class SimpleGbChiefEdifactTransmissionMessageGeneratorForChangeComparison : GbChiefEdifactTransmissionMessageGenerator
	{
		public SimpleGbChiefEdifactTransmissionMessageGeneratorForChangeComparison(CusdecMessageFunction declarationMessageFunction)
			: base(declarationMessageFunction)
		{ }

		internal ErrorCollector ErrorCollectorForTest { get { return errorCollector; } }

		protected override ZString GetApplicationCode(CusEntryHeader entry)
		{
			return "XXX";
		}

		protected override ZString GetApplicationReference(EU.Business.Declaration.CusEntryHeader entryHeader)
		{
			return "YYY";
		}
	}
}
