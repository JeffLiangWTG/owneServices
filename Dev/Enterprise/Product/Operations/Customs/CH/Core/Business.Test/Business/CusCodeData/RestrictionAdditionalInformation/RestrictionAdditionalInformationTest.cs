using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(RestrictionAdditionalInformation))]
sealed class RestrictionAdditionalInformationTest : Customs.Business.Testing.CusCodeDataTest<RestrictionAdditionalInformation>
{
	public void TestLookups() => AssertType<RestrictionAdditionalInformationLookups>(RestrictionAdditionalInformation.Lookups);

	public void TestParent() => AssertSame(Restriction, RestrictionAdditionalInformation.Parent);

	public void TestValidation() => CombineAssertions(() =>
	{
		AssertType<ExportRestrictionAdditionalInformationValidation>(RestrictionAdditionalInformation.Validation);
		AssertType<CusCodeDataValidation>(Factory.New<RestrictionAdditionalInformation>().Validation);
	});

	public void TestCY_Order()
	{
		CaptionTestHelper.AssertCaptions(RestrictionAdditionalInformation.CY_OrderInfo, caption: "Item Number");

		AssertEquals("1 added - 1st", 1, (ZInt)RestrictionAdditionalInformation.CY_Order);

		var additionalInformation2 = Restriction.AdditionalInformations.AddNew();
		AssertEquals("2 added - 1st", 1, (ZInt)RestrictionAdditionalInformation.CY_Order);
		AssertEquals("2 added - 2nd", 2, (ZInt)additionalInformation2.CY_Order);

		var additionalInformation3 = Restriction.AdditionalInformations.AddNew();
		AssertEquals("3 added-  1st", 1, (ZInt)RestrictionAdditionalInformation.CY_Order);
		AssertEquals("3 added - 2nd", 2, (ZInt)additionalInformation2.CY_Order);
		AssertEquals("3 added - 3rd", 3, (ZInt)additionalInformation3.CY_Order);

		additionalInformation2.Delete();
		AssertEquals("2 deleted - 1st", 1, (ZInt)RestrictionAdditionalInformation.CY_Order);
		AssertEquals("2 deleted - 3rd", 2, (ZInt)additionalInformation3.CY_Order);

		var additionalInformation4 = Restriction.AdditionalInformations.AddNew();
		AssertEquals("4 added - 1st", 1, (ZInt)RestrictionAdditionalInformation.CY_Order);
		AssertEquals("4 added - 3rd", 2, (ZInt)additionalInformation3.CY_Order);
		AssertEquals("4 added - 4th", 3, (ZInt)additionalInformation4.CY_Order);
	}

	public void TestCY_Code()
	{
		CaptionTestHelper.AssertCaptions(RestrictionAdditionalInformation.CY_CodeInfo, caption: "Code");
		AssertEquals(5, RestrictionAdditionalInformation.CY_CodeInfo.MaxLength);
	}

	public void TestCY_Data() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateAdditionalInformationCodeRestrictions(Factory, includeLinkedCodeTypes: true, additionalLinkedCodeValues: new[] { "Abc" });

		CaptionTestHelper.AssertCaptions(RestrictionAdditionalInformation.CY_DataInfo, caption: "Text");
		AssertEquals(512, RestrictionAdditionalInformation.CY_DataInfo.MaxLength);

		RestrictionAdditionalInformation.CY_Code = RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_N1004;

		RestrictionAdditionalInformation.CY_Data = "aBc";
		AssertEquals("Different casing", "Abc", RestrictionAdditionalInformation.CY_Data);
	});

	public void TestCY_DataFieldType() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateAdditionalInformationCodeRestrictions(Factory, includeLinkedCodeTypes: true);

		RestrictionAdditionalInformation.CY_Code = RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_N1004;
		AssertEquals("With LinkedCodeList attribute", nameof(FieldType.TextCodeFindBox), RestrictionAdditionalInformation.CY_DataFieldType);

		RestrictionAdditionalInformation.CY_Code = RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_B1001;
		AssertEquals("Without LinkedCodeList attribute", nameof(FieldType.Text), RestrictionAdditionalInformation.CY_DataFieldType);

		RestrictionAdditionalInformation.CY_Code = RefCusCodeTestHelper.InvalidAdditionalInformationWithRestrictionCode;
		AssertEquals("With unkown restriction code", nameof(FieldType.Text), RestrictionAdditionalInformation.CY_DataFieldType);

		RestrictionAdditionalInformation.CY_Code = ZString.Empty;
		AssertEquals("With empty restricon code", nameof(FieldType.Text), RestrictionAdditionalInformation.CY_DataFieldType);
	});

	public void TestLinkedCodeType() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateAdditionalInformationCodeRestrictions(Factory, includeLinkedCodeTypes: true);

		RestrictionAdditionalInformation.CY_Code = RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_N1004;
		AssertEquals("With LinkedCodeList attribute", RefCusCodeTestHelper.AdditionalInformationLinkedCodeType_N5004, RestrictionAdditionalInformation.LinkedCodeType);

		RestrictionAdditionalInformation.CY_Code = RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_B1001;
		AssertEquals("Without LinkedCodeList attribute", ZString.Empty, RestrictionAdditionalInformation.LinkedCodeType);

		RestrictionAdditionalInformation.CY_Code = RefCusCodeTestHelper.InvalidAdditionalInformationWithRestrictionCode;
		AssertEquals("With unkown restriction code", ZString.Empty, RestrictionAdditionalInformation.LinkedCodeType);

		RestrictionAdditionalInformation.CY_Code = ZString.Empty;
		AssertEquals("With empty restriction code", ZString.Empty, RestrictionAdditionalInformation.LinkedCodeType);

		RestrictionAdditionalInformation.CY_Code = RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_N1004;
		AssertEquals("With LinkedCodeList attribute", RefCusCodeTestHelper.AdditionalInformationLinkedCodeType_N5004, RestrictionAdditionalInformation.LinkedCodeType);
	});

	public void TestDefaults() => CombineAssertions(() =>
	{
		var additionalInformation = Factory.New<RestrictionAdditionalInformation>();
		AssertEquals("CY_Type", CusCodeDataTypeList.Codes.RestrictionAdditionalInformation, additionalInformation.CY_Type);
		AssertEquals("CY_ParentTableCode", CusSupportingInfoSchema.Constants.Prefix, additionalInformation.CY_ParentTableCode);
	});

	protected override IEnumerable<RestrictionAdditionalInformation> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return CreateRestriction(factory).AdditionalInformations.AddNew();
	}

	Restriction Restriction => restriction ??= CreateRestriction(Factory);
	Restriction restriction;

	RestrictionAdditionalInformation RestrictionAdditionalInformation => restrictionAdditionalInformation ??= Restriction.AdditionalInformations.AddNew();
	RestrictionAdditionalInformation restrictionAdditionalInformation;

	Restriction CreateRestriction(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		return invoiceLine.Restrictions.AddNew();
	}
}
