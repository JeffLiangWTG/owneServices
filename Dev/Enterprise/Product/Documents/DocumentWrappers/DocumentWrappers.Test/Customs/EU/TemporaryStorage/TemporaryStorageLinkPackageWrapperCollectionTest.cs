using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage.Testing;

[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
[TestedType(typeof(TemporaryStorageLinkPackageWrapperCollection))]
sealed class TemporaryStorageLinkPackageWrapperCollectionTest : DocumentWrapperCollectionTest<TemporaryStorageLinkPackageWrapperCollection>
{
	public void TestConstruct()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TemporaryStorageLinkPackageWrapperCollection(null, Factory));
			AssertExceptionThrown<ArgumentNullException>(() => new TemporaryStorageLinkPackageWrapperCollection(linkPackages, null));
			AssertNoExceptionThrown(() => GetNewDocumentWrapperCollection());
			AssertEquals("Collection Count", 1, Collection.Count);
		});
	}

	public void TestPacks()
	{
		CombineAssertions(() =>
		{
			var linkPackage = linkPackages.First();
			AssertNotNull(linkPackage);
			linkPackage.Package.APA_PackQty = 150;
			linkPackage.Package.APA_PackUQ = Core.Constants.Weight.MetricCarat;
			var container = Factory.New<TemporaryStorageContainer>();
			container.ACN_ContainerNumber = "CNT000001";
			linkPackage.Package.ContainerPK = container.PK;
			linkPackage.Package.APA_MarksAndNumbers = "MarkMyNumbers";

			var wrapper = GetNewDocumentWrapperCollection()[0];
			AssertNotNull(wrapper);
			AssertEquals("Number", "MC in CNT000001 marked MarkMyNumbers", wrapper.Number);
			AssertEquals("Type", Core.Constants.Weight.MetricCarat, wrapper.Type);
			AssertEquals("MarksAndNumbers", "MarkMyNumbers", wrapper.MarksAndNumbers);
			AssertEquals("Container", "CNT000001", wrapper.Container);
		});
	}

	protected override TemporaryStorageLinkPackageWrapperCollection GetNewDocumentWrapperCollection() => new TemporaryStorageLinkPackageWrapperCollection(linkPackages, Factory);

	protected override object GetNewObjectToWrap() => null;

	protected override void SetUp()
	{
		base.SetUp();
		var linkPackage = new TemporaryStorageLinkPackage(Factory.New<TemporaryStoragePackedItem>());
		linkPackage.Package = Factory.New<TemporaryStoragePack>();
		linkPackages = new TemporaryStorageLinkPackage[] { linkPackage };
	}

	IEnumerable<TemporaryStorageLinkPackage> linkPackages;
}
