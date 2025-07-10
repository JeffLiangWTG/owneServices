using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage.Testing;

[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
[TestedType(typeof(TemporaryStorageLinkPackageWrapper))]
sealed class TemporaryStorageLinkPackageWrapperTest : DocBaseWrapperTest
{
	public void TestNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Number", ZString.Empty, Wrapper.Number);

			var container = Factory.New<TemporaryStorageContainer>();
			container.ACN_ContainerNumber = "CNT000001";
			linkPackage.Package.ContainerPK = container.PK;
			AssertEquals("Number", " in CNT000001", Wrapper.Number);

			linkPackage.Package.APA_MarksAndNumbers = "MarkMyNumbers";
			AssertEquals("Number", " in CNT000001 marked MarkMyNumbers", Wrapper.Number);

			linkPackage.Package.APA_PackUQ = Core.Constants.Weight.MetricCarat;
			AssertEquals("Number", "MC in CNT000001 marked MarkMyNumbers", Wrapper.Number);
		});
	}

	public void TestPackType()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Type", ZString.Empty, Wrapper.Type);
			linkPackage.Package.APA_PackUQ = Core.Constants.Weight.MetricCarat;
			AssertEquals("Type", Core.Constants.Weight.MetricCarat, Wrapper.Type);
		});
	}

	public void TestPackMarksAndNumbers()
	{
		CombineAssertions(() =>
		{
			AssertEquals("MarksAndNumbers", ZString.Empty, Wrapper.MarksAndNumbers);
			linkPackage.Package.APA_MarksAndNumbers = "MarkMyNumbers";
			AssertEquals("MarksAndNumbers", "MarkMyNumbers", Wrapper.MarksAndNumbers);
		});
	}

	public void TestPackContainer()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Container", ZString.Empty, Wrapper.Container);
			var container = Factory.New<TemporaryStorageContainer>();
			container.ACN_ContainerNumber = "CNT000001";
			linkPackage.Package.ContainerPK = container.PK;
			AssertEquals("Container", "CNT000001", Wrapper.Container);
		});
	}

	new TemporaryStorageLinkPackageWrapper Wrapper => (TemporaryStorageLinkPackageWrapper)base.Wrapper;

	protected override DocBaseWrapper GetNewDocumentWrapper() => TemporaryStorageLinkPackageWrapper.New(linkPackage, Factory);

	protected override void SetUp()
	{
		base.SetUp();
		linkPackage = new TemporaryStorageLinkPackage(Factory.New<TemporaryStoragePackedItem>());
		linkPackage.Package = Factory.New<TemporaryStoragePack>();
	}

	TemporaryStorageLinkPackage linkPackage;
}
