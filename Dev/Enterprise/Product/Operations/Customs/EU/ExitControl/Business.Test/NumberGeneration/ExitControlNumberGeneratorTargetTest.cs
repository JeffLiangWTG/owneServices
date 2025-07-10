using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class ExitControlNumberGeneratorTargetTest : TestCaseWithFactory
	{
		public void TestParameters()
		{
			var target = new ExitControlNumberGeneratorTarget();
			target.Context = new NumberGeneratorContext();
			CombineAssertions(() =>
			{
				AssertNotNull("target.NumberCustomisation not null", target.NumberCustomisation);
				AssertEquals("target.Name", "Exit control job number", target.Name);
				AssertEquals("target.MaxLength", CusExitHeaderSchema.CXH_JobReference.MaxLength, target.MaxLength);
				AssertEquals("target.NumberCustomisationLocation", "Customs -> European Union (Common) -> Exit Control -> Exit Control Job Number Customization", target.NumberCustomisationLocation);
			});
		}
	}
}
