using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationGoodsLocationTest : TestCaseWithFactory
{
	public void TestImplementICusGoodsLocationProvider()
	{
		AssertEquals("Declaration is ICusGoodsLocationProvider", true, declaration is ICusGoodsLocationProvider);
	}

	public void TestGoodsLocation()
	{
		var goodsLocation = declaration.GoodsLocation;
		CombineAssertions(() =>
		{
			AssertEquals("CGL_ParentID", declaration.PK, goodsLocation.CGL_ParentID);
			AssertEquals("CGL_ParentTableCode", ZArchitecture.Schema.JobDeclarationSchema.Constants.Prefix, goodsLocation.CGL_ParentTableCode);
			AssertEquals("CGL_LocationUse", CusGoodsLocationUseList.Codes.Departure, goodsLocation.CGL_LocationUse);
			AssertSame("Cached", goodsLocation, declaration.GoodsLocation);
			AssertEquals("IsRegisteredEditableChildObject", true, declaration.IsRegisteredEditableChildObject(goodsLocation));
		});
	}

	public void TestGoodsLocationDescription()
	{
		CombineAssertions(() =>
		{
			AssertNull("GoodsLocation doesn't exist", Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(declaration, CusGoodsLocationUseList.Codes.Departure));
			AssertEquals("GoodsLocationDescription empty when there's no GoodsLocation", ZString.Empty, declaration.GoodsLocationDescription);

			var goodsLocation = declaration.GoodsLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			AssertEquals("GoodsLocationDescription when there's GoodsLocation", "Z", declaration.GoodsLocationDescription);
		});
	}

	public void TestGoodsLocationDescription_Caption()
	{
		var goodsLocationDescriptionCaption = DataBoundResourceStrings.GetDataForProperty(declaration.GoodsLocationDescriptionInfo)?.Caption ?? string.Empty;
		AssertEquals("Location of Goods", "Location of Goods", goodsLocationDescriptionCaption);
	}

	public void TestGoodsLocationDescription_RegisterEditableChildObject()
	{
		CusGoodsLocation.New<CusGoodsLocation>(declaration, CusGoodsLocationUseList.Codes.Departure);
		_ = declaration.GoodsLocationDescription;
		AssertEquals("IsRegisteredEditableChildObject", true, declaration.IsRegisteredEditableChildObject(declaration.GoodsLocation));
	}

	public void TestGoodsLocationDescriptionInfo()
	{
		AssertEquals(nameof(JobDeclaration.GoodsLocationDescription), declaration.GoodsLocationDescriptionInfo.Name);
	}

	public void TestICusGoodsLocationProvider_ProviderKey()
	{
		AssertEquals("ITDECL", (declaration as ICusGoodsLocationProvider).ProviderKey);
	}

	public void TestClearGoodsLocation()
	{
		var goodsLocation = declaration.GoodsLocation;
		goodsLocation.CGL_Qualifier = "V";
		goodsLocation.CGL_Type = "B";
		goodsLocation.CGL_CustomsOffice = "ABC";

		declaration.ClearGoodsLocation();

		CombineAssertions("When Goods Location cleared", () =>
		{
			AssertEquals("", goodsLocation.CGL_Qualifier);
			AssertEquals("", goodsLocation.CGL_Type);
			AssertEquals("", goodsLocation.CGL_CustomsOffice);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
}
