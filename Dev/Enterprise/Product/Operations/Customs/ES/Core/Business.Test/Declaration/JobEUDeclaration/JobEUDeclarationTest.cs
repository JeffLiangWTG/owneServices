using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

[TestedType(typeof(JobEUDeclaration))]
public class JobEUDeclarationTest : IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporterTestCase<JobEUDeclaration>
{
	protected override string ExpectedUniqueIndexName => ZArchitecture.Schema.JobEUDeclarationSchema.Constants.Indexes.FK_UX__EUD_JE;

	protected override SchemaIntColumn ExpectedClusterKeyColumn => ZArchitecture.Schema.JobEUDeclarationSchema.EUD_ClusterKey;

	protected override string ExpectedUniqueClusterIndexName => ZArchitecture.Schema.JobEUDeclarationSchema.Constants.Indexes.NR_UC__EUD_ClusterKey;

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		return declaration.AddInfoChild;
	}

	protected override EnterpriseBusinessObject GetParent(JobEUDeclaration bizObj) => bizObj.Declaration;

	public void TestEUD_RegionOrTerritoryOfDestinationList()
	{
		var euDeclaration = Factory.New<JobEUDeclaration>();
		AssertEquals("Lookups.RegionOrTerritoryOfDestinationList", Customs.Business.ZPropertyInfoExtensions.GetAttribute<ListAttribute>(euDeclaration.EUD_RegionOrTerritoryOfDestinationInfo).ListDataSourceMember);
	}

	public void TestJobDeclarationType()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var euDeclaration = Factory.New<JobEUDeclaration>();
		euDeclaration.EUD_JE = jobDeclaration.PK;
		AssertType<JobDeclaration>(euDeclaration.Declaration);
	}

	public void TestLookups()
	{
		AssertType<JobEUDeclarationLookups>(Factory.New<JobEUDeclaration>().Lookups);
	}
}
