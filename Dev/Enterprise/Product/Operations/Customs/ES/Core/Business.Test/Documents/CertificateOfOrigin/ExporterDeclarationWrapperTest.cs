using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin.Testing;

class ExporterDeclarationWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should be exception when declaration parameter is null", () => new ExporterDeclarationWrapper(declaration: null));
	}

	[TestDate(2025, 02, 16)]
	public void TestReferenceDate()
	{
		var wrapper = GetNewWrapper();
		var referenceDate = wrapper.ReferenceDate;
		CombineAssertions(() =>
		{
			AssertEquals("ReferenceDate is Today", new ZDate(2025, 02, 16), referenceDate);
			AssertEquals("ReferenceDate cached value", wrapper.ReferenceDate, referenceDate);
		});
	}

	public void TestPlace_WhenDeclarantDeclared()
	{
		var wrapper = GetNewWrapper();
		CombineAssertions(() =>
		{
			AssertEquals("Place", "", wrapper.Place);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			var declarantMainAddress = declarant.MainAddress;
			declarantMainAddress.OA_City = "BARCELONA";

			var declarantOrgAddress = Factory.New<OrgAddress>();
			declarantOrgAddress.OA_City = "MADRID";

			declaration.JE_OA_DeclarantAddress = declarantOrgAddress.PK;

			wrapper = GetNewWrapper();
			AssertEquals("Place", "MADRID", wrapper.Place);
		});
	}

	public void TestPlace_WhenOnlySupplierDeclared()
	{
		var wrapper = GetNewWrapper();
		CombineAssertions(() =>
		{
			AssertEquals("Place", "", wrapper.Place);

			SetSupplier();
			wrapper = GetNewWrapper();
			AssertEquals("Place is empty, Declarant address is not declared", "", wrapper.Place);
		});
	}

	public void TestSupplierDetails_WhenDeclarantDeclared()
	{
		var declarant = SetDeclarant();
		var wrapper = GetNewWrapper();
		CombineAssertions(() =>
		{
			AssertEquals("When NIF is not found, ExporterDetails", "MY TEST DECLARANT\r\nNIF: ", wrapper.ExporterDetails);

			declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "30521DDD", "ES");
			wrapper = GetNewWrapper();
			AssertEquals("When NIF is found, ExporterDetails", "MY TEST DECLARANT\r\nNIF: 30521DDD", wrapper.ExporterDetails);
		});
	}

	public void TestSupplierDetails_WhenOnlySupplierDeclared()
	{
		var supplier = SetSupplier();
		var wrapper = GetNewWrapper();
		CombineAssertions(() =>
		{
			AssertEquals("When NIF is not found, ExporterDetails", "MY TEST COMPANY\r\nNIF: ", wrapper.ExporterDetails);

			supplier.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "30521DDD", "ES");
			wrapper = GetNewWrapper();
			AssertEquals("When NIF is found, ExporterDetails", "MY TEST COMPANY\r\nNIF: 30521DDD", wrapper.ExporterDetails);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;

	EU.Business.Documents.CertificateOfOrigin.IExporterDeclaration GetNewWrapper() => new ExporterDeclarationWrapper(declaration);

	OrgHeader SetDeclarant()
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_FullName = "MY TEST DECLARANT";
		var declarantAddress = declarant.MainAddress;
		declarantAddress.OA_City = "BARCELONA";
		declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
		return declarant;
	}

	OrgHeader SetSupplier()
	{
		var supplier = Factory.NewWithValidTestData<OrgHeader>();
		supplier.OH_FullName = "MY TEST COMPANY";
		var supplierAddress = supplier.MainAddress;
		supplierAddress.OA_City = "MADRID";
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_OH_Supplier = supplier.PK;
		return supplier;
	}
}
