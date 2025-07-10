using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusTempStorageRegLineTransactionReferenceTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionReferenceTypeList;
using CusTempStorageRegPremisesTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageRegLineTransactionFormEditable))]
public class CusTempStorageRegLineTransactionFormEditableTest : NonPersistentBusinessObjectTestCase
{
	public void TestInternalReferenceTypeList() => CombineAssertions(() =>
	{
		var transactionEditable = (CusTempStorageRegLineTransactionFormEditable)GetNewBusinessObject();
		var premises = transactionEditable.RegLine.RegHeader.Premises;
		premises.SRP_Type = ZString.Empty;
		var list = transactionEditable.InternalReferenceTypeList;
		AssertEquals("CodesAsString when premises type is empty", "ABD, DES, DUA, DUE, DVD, EXS, G5, H7, LAM, OTH, T2L, TRA, TSM", list.CodesAsString);

		premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;
		list = transactionEditable.InternalReferenceTypeList;
		AssertEquals("CodesAsString when premises type is LAM", "ABD, DES, DUE, LAM, OTH, T2L", list.CodesAsString);

		premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		list = transactionEditable.InternalReferenceTypeList;
		AssertEquals("CodesAsString when premises type is ADT", "ABD, DES, DUA, DVD, EXS, G5, H7, OTH, TRA, TSM", list.CodesAsString);

		AssertSame("List is cached", list, transactionEditable.InternalReferenceTypeList);
	});

	public void TestReferenceTypeList() => CombineAssertions(() =>
	{
		var transactionEditable = (CusTempStorageRegLineTransactionFormEditable)GetNewBusinessObject();
		var list = transactionEditable.ReferenceTypeList;

		AssertType<CusTempStorageRegLineTransactionReferenceTypeList>("Type", list);
		AssertSame("Cached", list, transactionEditable.ReferenceTypeList);
	});

	public void TestValidation()
	{
		var transactionEditable = (CusTempStorageRegLineTransactionFormEditable)GetNewBusinessObject();
		AssertType<CusTempStorageRegLineTransactionFormEditableValidation>(transactionEditable.Validation);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_Reference = "UNITTEST";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "DESC";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "TestAddress";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		regHeader.SRH_SRP_Premises = premises.PK;
		var regLine = regHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
		return new CusTempStorageRegLineTransactionFormEditable(regLine);
	}
}
