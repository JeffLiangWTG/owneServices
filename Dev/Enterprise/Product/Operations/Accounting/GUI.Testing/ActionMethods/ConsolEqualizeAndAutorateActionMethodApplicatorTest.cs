using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(ConsolEqualizeAndAutorateActionMethodApplicator))]
	internal class ConsolEqualizeAndAutorateActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestRunAction_EmptyTargets()
		{
			ApplyApplicator(System.Array.Empty<BusinessObject>(), "");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
		}
	}
}
