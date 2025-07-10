using System;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.MX.Business.Testing
{
	class InvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		protected override ZString TariffCode => "0000.00.00.00K";

		protected override ZString TariffCode2 => "0000.00.00.01K";

		// to be overridden once the Tariff is setup for a new country
		protected override ZString TariffDescription => "TARIFF_DESCRIPTION";

		protected override ZString TariffDescription2 => "TARIFF_DESCRIPTION";

		protected override Type DeclarationTypeForTest => typeof(JobDeclaration);

		protected override BaseJobDeclaration CreateBaseJobDeclarationForMerge()
		{
			var declaration = base.CreateBaseJobDeclarationForMerge();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			return declaration;
		}
	}
}
