using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobCADeclaration))]
	sealed class JobCADeclarationTest : IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporterTestCase<JobCADeclaration>
	{
		public void TestIJobCADeclarationCorrectlySetup()
		{
			var data = (BusinessObject)Factory.New<Integration.Customs.CA.IJobCADeclaration>();
			AssertType<JobCADeclaration>(data);
			AssertType<JobCADeclaration>(Factory.Load(data.TablePrefix, data.PK));
		}

		public void TestMakeNonPersistent()
		{
			var declaration = Factory.New<JobCADeclaration>();
			declaration.HasChanges = true;
			Assert("Should be saved", declaration.IsSavedByFactory);
			declaration.MakeNonPersistent();
			Assert("Should no longer be saved", !declaration.IsSavedByFactory);
		}

		public void TestIsPersistent()
		{
			var declaration = Factory.New<JobCADeclaration>();
			Assert("Default is persistent", declaration.IsPersistent);
			declaration.MakeNonPersistent();
			Assert("Now non-persistent", !declaration.IsPersistent);
		}

		protected override CargoWise.Schema.SchemaIntColumn ExpectedClusterKeyColumn => JobCADeclarationSchema.CAD_ClusterKey;
		protected override string ExpectedUniqueClusterIndexName => JobCADeclarationSchema.Constants.Indexes.NR_UC__CAD_ClusterKey;
		protected override string ExpectedUniqueIndexName => JobCADeclarationSchema.Constants.Indexes.FK_UX__CAD_JE;
		protected override EnterpriseBusinessObject GetParent(JobCADeclaration bizObj) => bizObj.Declaration;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			return declaration.CADeclaration;
		}
	}
}
