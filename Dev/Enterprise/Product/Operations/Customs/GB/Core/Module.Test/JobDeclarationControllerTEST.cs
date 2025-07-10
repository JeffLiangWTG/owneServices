using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	public class JobDeclarationControllerTest : Customs.Module.Testing.JobDeclarationControllerTestCase
	{
		// **** GB ****
		protected override bool CountryHasExWarehouse
		{
			get { return false; }
		}

		public void TestFormControllerType()
		{
			AssertEquals(typeof(JobDeclarationController).FullName, ControllerToBashType.FullName);
		}
	}
}
