using System;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business.Testing
{
	public class InvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		public override void TestDescriptionOnTarrifWhenMerged()
		{
			Assert("For integrated countries, merge is turned off.", true);
		}

		protected override ZString TariffCode => "0000.00.00";

		protected override ZString TariffCode2 => "0000.00.00";

		protected override ZString TariffDescription => TariffDescriptionCore;
		internal const string TariffDescriptionCore = "";

		protected override ZString TariffDescription2 => TariffDescriptionCore2;
		internal const string TariffDescriptionCore2 = "";

		protected override Type DeclarationTypeForTest => typeof(JobDeclaration);
	}
}
