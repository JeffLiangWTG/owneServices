using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PermitItemDetail))]
class PermitItemDetailTest : Customs.Business.Testing.CusCodeDataTest<PermitItemDetail>
{
	public void TestLookups()
	{
		AssertType<PermitItemDetailLookups>(PermitItemDetail.Lookups);
	}

	public void TestValidation()
	{
		AssertType<PermitItemDetailValidation>(PermitItemDetail.Validation);
	}

	public void TestParent()
	{
		AssertSame(Permit, PermitItemDetail.Parent);
	}

	public void TestCY_Code()
	{
		CombineAssertions(() =>
		{
			AssertEquals("MaxLength", 2, PermitItemDetail.CY_CodeInfo.MaxLength);
			AssertEquals("Caption", "Key", PermitItemDetail.CY_CodeInfo.Description);
		});
	}

	public void TestCY_Data()
	{
		CombineAssertions(() =>
		{
			PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key1;
			AssertEquals($"MaxLength CY_Code={PermitItemDetail.CY_Code}", 50, PermitItemDetail.CY_DataInfo.MaxLength);

			PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key2;
			AssertEquals($"MaxLength CY_Code={PermitItemDetail.CY_Code}", 50, PermitItemDetail.CY_DataInfo.MaxLength);

			PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key3;
			AssertEquals($"MaxLength CY_Code={PermitItemDetail.CY_Code}", 5, PermitItemDetail.CY_DataInfo.MaxLength);

			PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key4;
			AssertEquals($"MaxLength CY_Code={PermitItemDetail.CY_Code}", 5, PermitItemDetail.CY_DataInfo.MaxLength);

			PermitItemDetail.CY_Code = "9";
			AssertEquals($"MaxLength CY_Code={PermitItemDetail.CY_Code}", 50, PermitItemDetail.CY_DataInfo.MaxLength);

			AssertEquals("Caption", "Value", PermitItemDetail.CY_DataInfo.Description);
		});
	}

	public void TestCY_DataFieldType()
	{
		CombineAssertions(() =>
		{
			PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key1;
			AssertEquals($"Key {PermitItemDetail.CY_Code}", nameof(FieldType.Integer), PermitItemDetail.CY_DataFieldType);
			PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key2;
			AssertEquals($"Key {PermitItemDetail.CY_Code}", nameof(FieldType.Decimal), PermitItemDetail.CY_DataFieldType);
			PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key3;
			AssertEquals($"Key {PermitItemDetail.CY_Code}", nameof(FieldType.TextCodeFindBox), PermitItemDetail.CY_DataFieldType);
			PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key4;
			AssertEquals($"Key {PermitItemDetail.CY_Code}", nameof(FieldType.TextCodeFindBox), PermitItemDetail.CY_DataFieldType);
			PermitItemDetail.CY_Code = "9";
			AssertEquals($"Key {PermitItemDetail.CY_Code}", nameof(FieldType.Text), PermitItemDetail.CY_DataFieldType);
		});
	}

	public void TestCY_DataDecimalPlaces()
	{
		PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key1;
		AssertEquals($"CY_Code={PermitItemDetail.CY_Code}", 0, PermitItemDetail.CY_DataDecimalPlaces);
		PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key2;
		AssertEquals($"CY_Code={PermitItemDetail.CY_Code}", 2, PermitItemDetail.CY_DataDecimalPlaces);
	}

	public void TestDataDescription()
	{
		RefCusCodeTestHelper.CreateCITESCommodityTypeList(Factory);
		RefCusCodeTestHelper.CreateCITESScientificNameList(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Description", PermitItemDetail.DataDescriptionInfo.Description);

			PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key1;
			AssertEquals($"Key={PermitItemDetail.CY_Code}", "Position number", PermitItemDetail.DataDescription);

			PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key2;
			AssertEquals($"Key={PermitItemDetail.CY_Code}", "Depreciated quantity", PermitItemDetail.DataDescription);

			PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key3;
			PermitItemDetail.CY_Data = RefCusCodeTestHelper.ValidCITESCommodityTypeListCode;
			AssertEquals($"Key={PermitItemDetail.CY_Code}", RefCusCodeTestHelper.ValidCITESCommodityTypeListCodeDescription, PermitItemDetail.DataDescription);

			PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key4;
			PermitItemDetail.CY_Data = RefCusCodeTestHelper.ValidCITESScientificNameListCode;
			AssertEquals($"Key={PermitItemDetail.CY_Code}", RefCusCodeTestHelper.ValidCITESScientificNameListCodeDescription, PermitItemDetail.DataDescription);

			PermitItemDetail.CY_Code = "9";
			AssertEquals($"Key={PermitItemDetail.CY_Code}", ZString.Empty, PermitItemDetail.DataDescription);
		});
	}

	protected override IEnumerable<PermitItemDetail> GetBizObjsForCorrectlyTypeDecideTest(
		BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var permit = invoiceLine.Permits.AddNew();
		permit.CSI_Code = UniversalReferenceConstants.PermitCodes.GeneralEPermit;
		yield return permit.PermitItemDetails.AddNew();
	}

	Permit Permit => permit ?? (permit = Factory.New<Permit>());
	Permit permit;

	PermitItemDetail PermitItemDetail => permitItemDetail ?? (permitItemDetail = Permit.PermitItemDetails.AddNew());
	PermitItemDetail permitItemDetail;
}
