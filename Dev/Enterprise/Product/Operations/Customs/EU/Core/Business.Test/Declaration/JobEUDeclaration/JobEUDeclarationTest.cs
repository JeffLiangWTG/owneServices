using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
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

		public void TestJobDeclarationType()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var euDeclaration = Factory.New<JobEUDeclaration>();
			euDeclaration.EUD_JE = jobDeclaration.PK;
			AssertType<JobDeclaration>(euDeclaration.Declaration);
		}

		public void TestJE_ShipmentIncoTermPlace_OnValidUnloco()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ShipmentIncoTermPlace = "abc";
				declaration.EUD_AgreedPlaceCode = Core.Constants.CountryCodes.Netherlands;
				AssertEquals("AgreedPlaceCode is country", "abc", declaration.JE_ShipmentIncoTermPlace);

				declaration.EUD_AgreedPlaceCode = "NLAAA";
				AssertEquals("Invalid Unloco Code", "abc", declaration.JE_ShipmentIncoTermPlace);

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_ShipmentIncoTermPlace = "abc";
					declaration.EUD_AgreedPlaceCode = "NLAMS";
					AssertEquals("AgreedPlaceCode enabled - Valid Unloco Code", ZString.Empty, declaration.JE_ShipmentIncoTermPlace);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_ShipmentIncoTermPlace = "abc";
					declaration.EUD_AgreedPlaceCode = "NLAMS";
					AssertEquals("AgreedPlaceCode disabled - Valid Unloco Code", "abc", declaration.JE_ShipmentIncoTermPlace);
				}
			});
		}

		public void TestEUD_AgreedPlaceCode_ResourceStringData() => AssertEntity<JobEUDeclaration>()
			.HasProperty(x => x.EUD_AgreedPlaceCode)
			.WithCaption("Incoterm Place Code")
			.WithShortCaption("Inco. Place Code")
			.WithMediumCaption("Inco. Place Code")
			.WithFullDescription("Incoterm Place Code: insert a Country (2 chars) or an UNLOCO (5 chars)");
	}

	[TestedType(typeof(JobEUDeclaration))]
	class JobEUDeclarationClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.AddInfoChild;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			return Factory.New<JobDeclaration>();
		}
	}
}
