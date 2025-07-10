using System;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Module.Testing;

[TestedType(typeof(JobDeclarationModule))]
sealed class JobDeclarationModuleTest : EU.Module.Testing.JobDeclarationModuleTest
{
	protected override string CountryCode => Core.Constants.CountryCodes.Italy;

	protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

	protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

	protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

	protected override Customs.Business.BaseJobDeclaration CreateDeclarationForFetchHintTest(CargoWise.EntityFramework.BusinessObjectFactory factory, string messageType, int i)
	{
		var declaration = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
		declaration.JE_LocationOfGoods = "MIL";
		declaration.ZG_CTStatusID = "A";
		declaration.MessageVersion = MessageVersionList.Codes.TXT;
		var cei = declaration.CustomsEntryInstructions.AddNew();
		cei.CEI_Style = "EFD";
		return declaration;
	}
}
