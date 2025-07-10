using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(CusContainer))]
class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
{
	public void TestTypeDecider()
	{
		Assert("Update BaseCusContainerTypeDecider to include a decider for this class", Factory.New(typeof(BaseCusContainer)).GetType() == typeof(CusContainer));
	}

	public void TestDeclaration()
	{
		CusContainer container = declaration.CusContainers.AddNew();
		AssertEquals(declaration, container.Declaration);
	}

	public void TestLookupsCachesInstance()
	{
		CusContainerLookups lookup1 = container.Lookups;
		CusContainerLookups lookup2 = container.Lookups;
		AssertEquals(lookup2, lookup1);
	}

	#region Implementation
	protected override BusinessObject GetNewBusinessObject()
	{
		declaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
		declaration.Importer.OH_FullName = "Test Importer";
		declaration.Importer.MainAddress.OA_Address1 = "Importers Address";
		declaration.Importer.OH_RL_NKClosestPort = "AUSYD";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		CusContainer result = declaration.CusContainers.AddNew();
		return result;
	}

	protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bO)
	{
		ICustomLabelsProvider result = new BaseCusContainer.CustomLabelsProvider(((CusContainer)bO).Declaration);
		return result;
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
		container = declaration.CusContainers.AddNew();
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		JobDeclaration declaration = factory.New<JobDeclaration>();
		declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
		CusContainer result = declaration.CusContainers.AddNew();
		Customs.Business.Testing.TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(result.Factory);
		return result;
	}

	protected JobDeclaration declaration;
	protected CusContainer container;
	#endregion
}
