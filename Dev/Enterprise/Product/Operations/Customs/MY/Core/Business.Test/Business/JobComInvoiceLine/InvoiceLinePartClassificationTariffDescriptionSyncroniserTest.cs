using System;
using CargoWise.Types;

namespace Enterprise.Customs.MY.Business.Testing
{
	class InvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		protected override ZString TariffCode => "0000.00.00.00K";

		protected override ZString TariffCode2 => "0000.00.00.01K";

		protected override ZString TariffDescription => TariffDescriptionCore;

		protected override ZString TariffDescription2 => TariffDescriptionCore2;

		protected override Type DeclarationTypeForTest => typeof(JobDeclaration);

		internal const string TariffDescriptionCore = "";

		internal const string TariffDescriptionCore2 = "";
	}
}
