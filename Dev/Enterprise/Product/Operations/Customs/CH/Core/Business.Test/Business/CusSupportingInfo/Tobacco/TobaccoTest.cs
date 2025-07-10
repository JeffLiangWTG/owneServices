using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(Tobacco))]
public class TobaccoTest : Customs.Business.Testing.CusSupportingInfoTest<Tobacco>
{
	public void TestCSI_CodeMaxLength() => AssertEquals(1, Tobacco.CSI_CodeInfo.MaxLength);

	public void TestCSI_SubtypeMaxLength() => AssertEquals(2, Tobacco.CSI_SubTypeInfo.MaxLength);

	public void TestCSI_DescriptionMaxLength() => AssertEquals(50, Tobacco.CSI_DescriptionInfo.MaxLength);

	public void TestCSI_ItemNumberMaxLength() => AssertEquals(5, Tobacco.CSI_ItemNumberInfo.MaxLength);

	public void TestCSI_AdditionalDescriptionMaxLength() => AssertEquals(3, Tobacco.CSI_AdditionalDescriptionInfo.MaxLength);

	public void TestCSI_CodeCaption() => AssertEquals("Product main group", Tobacco.CSI_CodeInfo.Description);

	public void TestCSI_SubtypCaption() => AssertEquals("Product subgroup", Tobacco.CSI_SubTypeInfo.Description);

	public void TestCSICSI_DescriptionCaption() => AssertEquals("Designation", Tobacco.CSI_DescriptionInfo.Description);

	public void TestCSI_ItemNumberCaption() => AssertEquals("Sequential number", Tobacco.CSI_ItemNumberInfo.Description);

	public void TestCSI_ValueCaption()
	{
		CaptionTestHelper.AssertCaptions(Tobacco.CSI_ValueInfo, multipleResourceKey: MessageTypeCodeList.Codes.Import, "Retail price");
		CaptionTestHelper.AssertCaptions(Tobacco.CSI_ValueInfo, multipleResourceKey: MessageTypeCodeList.Codes.Export, "Rate");
	}

	public void TestCSI_AdditionalDescriptionCaption() => AssertEquals("Tobacco brand", Tobacco.CSI_AdditionalDescriptionInfo.Description);

	public void TestCSI_ReferenceNumberCaption() => AssertEquals("Reverse number", Tobacco.CSI_ReferenceNumberInfo.Description);

	public void TestCSI_UnitOfQuantityCaption() => AssertEquals("Special Unit of measure", Tobacco.CSI_UnitOfQuantityInfo.Description);

	public void TestCSI_ValueDecimal() => AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(Tobacco), nameof(Tobacco.CSI_Value), includesInherit: false, x => x.DecimalPlaces == 4);

	protected override BusinessObject GetNewBusinessObject() => GetNewTobacco(Factory);

	protected override IEnumerable<Tobacco> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return GetNewTobacco(factory);
	}

	Tobacco GetNewTobacco(BusinessObjectFactory factory)
	{
		return factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().Tobaccos.AddNew();
	}

	Tobacco Tobacco => tobacco ?? (tobacco = GetNewTobacco(Factory));
	Tobacco tobacco;
}
