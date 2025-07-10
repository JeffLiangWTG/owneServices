using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.Manifest.Business.Testing;

[TestedType(typeof(AsycudaContainer))]
sealed class AsycudaContainerTest : EnterpriseBusinessObjectTestCase
{
	public void TestContainerTypeAndContainerSize()
	{
		var containerSet = Common.Testing.ContainerHelperTest.CreateContainerTestData(Factory);
		var containerHelper = new ContainerHelper(Factory);
		CombineAssertions(() =>
		{
			Container.ACN_RC_ContainerType = containerSet.container1;
			AssertEquals("NACCSContainerSize is 12", "12", Container.NACCSContainerSize);
			AssertEquals("NACCSContainerType is GP", "GP", Container.NACCSContainerType);
			AssertEquals("Same as the value of GetNACCSContainerSize", containerHelper.GetNACCSContainerSize(Container.ContainerType), Container.NACCSContainerSize);
			AssertEquals("Same as the value of GetNACCSContainerType", containerHelper.GetNACCSContainerType(Container.ContainerType), Container.NACCSContainerType);

			Container.ACN_RC_ContainerType = containerSet.container4;
			AssertEquals("NACCSContainerSize is 99", "99", Container.NACCSContainerSize);
			AssertEquals("NACCSContainerType is SN", "SN", Container.NACCSContainerType);
		});
	}

	public void TestACN_MoveOutDate()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(Container.ACN_MoveOutDateInfo);
		CombineAssertions(() =>
		{
			AssertEquals("ACN_MoveOutDate Caption", "Move Out Date", resourceStringData.Caption);
			AssertEquals("ACN_MoveOutDate MediumCaption", "Move Out", resourceStringData.MediumCaption);
		});
	}

	public void TestAdditionalSeals()
	{
		AssertType<CusSealCollection>(Container.AdditionalSeals);
	}

	public void TestCustomsTareWeight()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(Container.CustomsTareWeightInfo);
		CombineAssertions(() =>
		{
			AssertEquals("CustomsTareWeight Caption", "Customs Tare Weight", resourceStringData.Caption);
			AssertEquals("CustomsTareWeight MediumCaption", "Tare Weight", resourceStringData.MediumCaption);
			AssertEquals("CustomsTareWeight ShortCaption", "Tare Wgt.", resourceStringData.ShortCaption);
		});
	}

	public void TestCustomsWeightUQAfterContainerTypeChanged()
	{
		var refContainer1 = Factory.New<RefContainer>();
		refContainer1.RC_Code = "42G1";
		var refContainer2 = Factory.New<RefContainer>();
		refContainer2.RC_Code = "42G2";
		refContainer2.RC_TareWeight = 17m;

		Container.ACN_RC_ContainerType = refContainer1.PK;
		AssertEquals("container type tare weight is 0, CustomsWeightUQ is empty.", ZString.Empty, Container.CustomsWeightUQ);

		Container.ACN_RC_ContainerType = refContainer2.PK;
		AssertEquals("container type tare weight is not 0, CustomsWeightUQ is empty.", Weight.Kilograms, Container.CustomsWeightUQ);

		refContainer1.RC_TareWeight = 15m;
		Container.CustomsWeightUQ = Weight.Pounds;
		Container.ACN_RC_ContainerType = refContainer1.PK;
		AssertEquals("container type tare weight is not 0, CustomsWeightUQ is not empty.", Weight.Pounds, Container.CustomsWeightUQ);
	}

	public void TestCustomsTareWeightAfterContainerTypeChanged()
	{
		var refContainer1 = Factory.New<RefContainer>();
		refContainer1.RC_Code = "42G1";
		refContainer1.RC_TareWeight = 17m;
		var refContainer2 = Factory.New<RefContainer>();
		refContainer2.RC_Code = "42G2";
		refContainer2.RC_TareWeight = 15m;
		var refContainer3 = Factory.New<RefContainer>();
		refContainer3.RC_Code = "42G3";
		refContainer3.RC_TareWeight = 0m;
		Container.ACN_RC_ContainerType = refContainer1.PK;
		AssertEquals("CustomsTareWeight is 0, RC_TareWeight is not 0.", 17m, Container.CustomsTareWeight);

		Container.ACN_RC_ContainerType = refContainer2.PK;
		AssertEquals("CustomsTareWeight is not 0, RC_TareWeight is not 0.", 17m, Container.CustomsTareWeight);

		Container.ACN_RC_ContainerType = refContainer3.PK;
		AssertEquals("CustomsTareWeight is not 0, RC_TareWeight is 0.", 17m, Container.CustomsTareWeight);

		Container.CustomsTareWeight = 0m;
		Container.CustomsWeightUQ = Weight.Pounds;
		Container.ACN_RC_ContainerType = refContainer1.PK;
		AssertEquals("CustomsTareWeight is 0, CustomsWeightUQ is LB, RC_TareWeight is not 0.", 37.478585m, Container.CustomsTareWeight);
	}

	public void TestCustomsWeightUQ()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(Container.CustomsWeightUQInfo);
		CombineAssertions(() =>
		{
			AssertEquals("CustomsWeightUQ Caption", "Customs Weight UQ", resourceStringData.Caption);
			AssertEquals("CustomsTareWeight MediumCaption", "Weight UQ", resourceStringData.MediumCaption);
			AssertEquals("CustomsWeightUQ ShortCaption", "UQ", resourceStringData.ShortCaption);
		});
	}

	public void TestValidationType()
	{
		AssertType<AsycudaContainerValidation>(Container.Validation);
	}

	public void TestLookupsType()
	{
		AssertType<AsycudaContainerLookups>(Container.Lookups);
	}

	protected override BusinessObject GetNewBusinessObject() => Factory.New<AsycudaManifestHeader>().Containers.AddNew();

	AsycudaContainer Container => container ??= (AsycudaContainer)GetNewBusinessObject();
	AsycudaContainer container;
}
