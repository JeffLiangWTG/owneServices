using System;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class FiscalReferenceWrapperTest : TestCaseWithFactory
{
	public void TestContructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new FiscalReferenceWrapper(null));
	}

	public void TestIdentificationNumber()
	{
		var fiscalReference = GetNewFiscalReference();
		AssertEquals(nameof(IFiscalReference.IdentificationNumber), "", fiscalReference.IdentificationNumber);

		cusFiscalReference.CFR_Reference = "ID NUMBER";
		fiscalReference = GetNewFiscalReference();
		AssertEquals(nameof(IFiscalReference.IdentificationNumber), "ID NUMBER", fiscalReference.IdentificationNumber);
	}

	public void TestRole()
	{
		var fiscalReference = GetNewFiscalReference();
		AssertEquals(nameof(IFiscalReference.Role), "", fiscalReference.Role);

		cusFiscalReference.CFR_Code = "CFR";
		fiscalReference = GetNewFiscalReference();
		AssertEquals(nameof(IFiscalReference.Role), "CFR", fiscalReference.Role);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		cusFiscalReference = invoiceLine.FiscalReferences.AddNew();
	}

	EU.Business.Declaration.CusFiscalReference cusFiscalReference;

	IFiscalReference GetNewFiscalReference() => new FiscalReferenceWrapper(cusFiscalReference);
}
