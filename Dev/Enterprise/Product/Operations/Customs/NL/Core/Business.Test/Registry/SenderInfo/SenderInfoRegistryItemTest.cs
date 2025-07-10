using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(SenderInfoRegistryItem))]
public class SenderInfoRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<SenderInfoCollection>
{
	protected override StronglyTypedRegistryItem<SenderInfoCollection, SenderInfoCollection> GetNewRegistryItem()
	{
		return new SenderInfoRegistryItem("", null, null, null, RegistryStorageFlags.Company);
	}

	protected override SenderInfoCollection ValidValue
	{
		get
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "Test Company 1";
			org1.OH_Code = "TestComp1";
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "Test Company 2";
			org2.OH_Code = "TestComp2";
			Factory.Save();

			var collection = new SenderInfoCollection();
			var mapping1 = collection.AddNew();
			mapping1.OrganizationPK = org1.PK;
			mapping1.SenderID = "123";
			mapping1.DefaultSenderID = true;
			var mapping2 = collection.AddNew();
			mapping2.OrganizationPK = org2.PK;
			mapping2.SenderID = "456";
			mapping2.DefaultSenderID = false;
			return collection;
		}
	}
}

[TestedType(typeof(SenderInfoRegistryDataType))]
class SenderInfoRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SenderInfoRegistryDataType>
{
	protected override string ExpectedEditorName
	{
		get { return "SenderInfoRegistryItemEditor"; }
	}

	protected override SenderInfoRegistryDataType GetNewDataType()
	{
		return new SenderInfoRegistryDataType();
	}

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var factory = new BusinessObjectFactory();
		var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
		var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
		orgHeaderQuery.AddSubQuery(orgCompanyDataQuery, JoinCondition.And);
		var organisation = factory.LoadTop1<OrgHeader>(orgHeaderQuery);
		factory.Save();

		var collection = new SenderInfoCollection();
		var mapping1 = collection.AddNew();
		mapping1.OrganizationPK = organisation.PK;
		mapping1.SenderID = "123";
		mapping1.DefaultSenderID = true;
		var collection2 = new SenderInfoCollection();
		var mapping2 = collection2.AddNew();
		mapping2.OrganizationPK = organisation.PK;
		mapping2.SenderID = "456";
		mapping2.DefaultSenderID = false;

		var dataType = new SenderInfoRegistryDataType();
		var dt = dataType.Serialise(collection);
		return new ValidSampleAndBinaryValueInDB[]
		{
			new ValidSampleAndBinaryValueInDB(collection, dataType.Serialise(collection)),
			new ValidSampleAndBinaryValueInDB(collection2, dataType.Serialise(collection2))
		};
	}
}
