using System;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

class InvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
{
	protected override ZString TariffCode => "0000000000";

	protected override ZString TariffCode2 => "0000000001";

	protected override ZString TariffDescription => "";

	protected override ZString TariffDescription2 => "";

	protected override Type DeclarationTypeForTest => typeof(JobDeclaration);

	protected override Customs.Business.BaseJobDeclaration CreateBaseJobDeclarationForMerge()
	{
		var result = base.CreateBaseJobDeclarationForMerge();
		result.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		return result;
	}
}
