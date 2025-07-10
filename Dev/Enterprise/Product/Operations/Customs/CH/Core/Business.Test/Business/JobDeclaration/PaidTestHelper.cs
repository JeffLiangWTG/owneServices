using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using static CargoWise.EntityFramework.Testing.TestCaseWithFactory;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.CH.Business.Testing;

internal class PaidTestHelper
{
	internal const string SupplierAccountNo = "SUP12345";
	internal const string ImporterAccountNo = "IMP12345";
	internal const string ConsigneeAccountNo = "CON12345";
	internal const string CompanyAccountNo = "COM12345";
	internal const string FreightForwarderAccountNo = "FWD12345";
	internal const string DeclarantAccountNo = "DEC12345";

	internal PaidTestHelper(BusinessObjectFactory factory)
	{
		Factory = factory;
	}

	BusinessObjectFactory Factory { get; }

	internal void TestPaidByDefault(ZString codeType, ZString shipmentIncoTerm, bool supplierHasRegNo, bool importerHasRegNo, Func<JobDeclaration, ZString> paidByGetter, ZString expectedPaidBy)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		declaration.JE_ShipmentIncoTerm = shipmentIncoTerm;

		var supplierOrgHeader = CreateOrgHeader();
		if (supplierHasRegNo)
		{
			CreateCustomsCode(supplierOrgHeader, codeType, SupplierAccountNo);
		}
		declaration.JE_OH_Supplier = supplierOrgHeader.PK;

		var importerOrgHeader = CreateOrgHeader();
		if (importerHasRegNo)
		{
			CreateCustomsCode(importerOrgHeader, codeType, ImporterAccountNo);
		}
		declaration.JE_OH_Importer = importerOrgHeader.PK;

		CreateCustomsCode(GlbCompany.CurrentCompany.OrgProxy, codeType, CompanyAccountNo);

		AssertEquals($"shipmentIncoTerm={shipmentIncoTerm} supplierHasRegNo={supplierHasRegNo} imorterHasRegNo={importerHasRegNo}", expectedPaidBy, paidByGetter(declaration));
	}

	internal void TestPaidByAccountNo(ZString codeType, Action<JobDeclaration, ZString> paidBySetter, Func<JobDeclaration, ZString> accountNoGetter, ZString paidBy, ZString expectedAccountNo)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var supplierOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateCustomsCode(supplierOrgHeader, codeType, SupplierAccountNo);
		declaration.JE_OH_Supplier = supplierOrgHeader.PK;

		var importerOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateCustomsCode(importerOrgHeader, codeType, ImporterAccountNo);
		declaration.JE_OH_Importer = importerOrgHeader.PK;

		var consigneeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateCustomsCode(consigneeOrgHeader, codeType, ConsigneeAccountNo);
		declaration.JE_OH_Consignee = consigneeOrgHeader.PK;

		var freightForwarderOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateCustomsCode(freightForwarderOrgHeader, codeType, FreightForwarderAccountNo);
		declaration.JE_OH_Forwarder = freightForwarderOrgHeader.PK;

		var declarantOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateCustomsCode(declarantOrgHeader, codeType, DeclarantAccountNo);
		declaration.JE_OA_DeclarantAddress = declarantOrgHeader.MainAddress.PK;

		paidBySetter.Invoke(declaration, paidBy);
		AssertEquals($"For PaidBy={paidBy}", expectedAccountNo, accountNoGetter(declaration));
	}

	internal void TestPaidByAccountNoFallback(ZString codeType, Action<JobDeclaration, ZString> paidBySetter, Func<JobDeclaration, ZString> accountNoGetter, ZString paidBy, ZString expectedAccountNo)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var supplierOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateCustomsCode(supplierOrgHeader, codeType, SupplierAccountNo);
		declaration.JE_OH_Supplier = supplierOrgHeader.PK;

		var importerOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateCustomsCode(importerOrgHeader, codeType, ImporterAccountNo);
		declaration.JE_OH_Importer = importerOrgHeader.PK;

		var consigneeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateCustomsCode(consigneeOrgHeader, codeType, ConsigneeAccountNo);
		declaration.JE_OH_Consignee = consigneeOrgHeader.PK;

		var freightForwarderOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateCustomsCode(freightForwarderOrgHeader, codeType, FreightForwarderAccountNo);
		declaration.JE_OH_Forwarder = freightForwarderOrgHeader.PK;

		var declarantOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateCustomsCode(declarantOrgHeader, codeType, DeclarantAccountNo);
		declaration.JE_OA_DeclarantAddress = declarantOrgHeader.MainAddress.PK;

		paidBySetter.Invoke(declaration, paidBy);
		AssertEquals($"For PaidBy={paidBy}", expectedAccountNo, accountNoGetter(declaration));
	}

	public void TestDutyByAccountNoForDeclarantFallback(ZString codeType, Action<JobDeclaration, ZString> paidBySetter, Func<JobDeclaration, ZString> accountNoGetter)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var paidBy = DeclarationPayerList.Codes.Declarant;

		CreateCustomsCode(GlbCompany.CurrentCompany.OrgProxy, codeType, CompanyAccountNo);

		paidBySetter.Invoke(declaration, paidBy);
		AssertEquals($"For PaidBy={paidBy}", CompanyAccountNo, accountNoGetter(declaration));

		var declarantOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateCustomsCode(declarantOrgHeader, codeType, DeclarantAccountNo);
		declaration.JE_OA_DeclarantAddress = declarantOrgHeader.MainAddress.PK;

		paidBySetter.Invoke(declaration, paidBy);
		AssertEquals($"For PaidBy={paidBy}", DeclarantAccountNo, accountNoGetter(declaration));
	}

	internal void TestCheckPaidBy(Func<JobDeclaration, ZPropertyInfo> paidByPropertyInfoGetter, Action<JobDeclaration, ZString> otherPaidBySetter, string errorMessage)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var paidByPropertyInfo = paidByPropertyInfoGetter(declaration);

		paidByPropertyInfo.Value = new ZString(DeclarationPayerList.Codes.Cash);
		otherPaidBySetter(declaration, DeclarationPayerList.Codes.Forwarder);
		AssertHasMessageErrorContaining("Cash/Non-Cash", paidByPropertyInfo, errorMessage);

		otherPaidBySetter(declaration, DeclarationPayerList.Codes.Cash);
		AssertNoMessageError("Cash/Cash", paidByPropertyInfo, errorMessage);
	}

	OrgHeader CreateOrgHeader()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader.MiscServ.OM_IMDefaultINCOTerm = ZString.Empty;
		orgHeader.MiscServ.OM_EXDefaultIncoTerm = ZString.Empty;
		orgHeader.Addresses.AddNewMainAddress();
		return orgHeader;
	}

	void CreateCustomsCode(OrgHeader orgHeader, ZString codeType, ZString customsRegNo)
	{
		orgHeader.CustomsCodes.AddNew(codeType, customsRegNo);
	}
}
