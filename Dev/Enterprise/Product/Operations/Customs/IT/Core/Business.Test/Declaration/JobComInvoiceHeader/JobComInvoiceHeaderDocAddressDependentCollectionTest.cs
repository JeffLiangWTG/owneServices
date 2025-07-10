using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceHeaderDocAddressDependentCollection))]
sealed class JobComInvoiceHeaderDocAddressDependentCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestIndexerType()
	{
		var collection = (JobComInvoiceHeaderDocAddressDependentCollection)GetCollectionToTest();
		collection.AddNew();
		AssertType<JobComInvoiceHeaderDocAddress>("Indexer type", collection[0]);
	}

	public void TestAddNewType()
	{
		var collection = (JobComInvoiceHeaderDocAddressDependentCollection)GetCollectionToTest();
		AssertType<JobComInvoiceHeaderDocAddress>("AddNew type", collection.AddNew());
	}

	public void TestFindOrCreateWithRequirementType()
	{
		var collection = (JobComInvoiceHeaderDocAddressDependentCollection)GetCollectionToTest();
		var addressCreatedWithRequirement = collection.FindOrCreateWithRequirement(new JobDocAddressRequirement(DocAddressType.None, ContactType.NoContactType));
		AssertType<JobComInvoiceHeaderDocAddress>("FindOrCreateWithRequirement type", addressCreatedWithRequirement);
	}

	protected override BusinessObjectCollection GetCollectionToTest() => new JobComInvoiceHeaderDocAddressDependentCollection(Factory.New<JobComInvoiceHeader>());
}
