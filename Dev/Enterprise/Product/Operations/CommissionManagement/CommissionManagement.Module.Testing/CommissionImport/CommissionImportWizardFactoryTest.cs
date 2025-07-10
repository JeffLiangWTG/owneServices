using CargoWise.EntityFramework.Testing;
using Enterprise.CommissionManagement.Business;

namespace Enterprise.CommissionManagement.Module.Testing
{
	public class CommissionImportWizardFactoryTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var factory = new CommissionImportWizardFactory();
			var result = factory.New(new CommissionImportCollectionInfo(new CommissionFlattenedCollection(Factory), false), "");
			AssertType(typeof(CommissionImportWizard), result);
		}
	}
}
