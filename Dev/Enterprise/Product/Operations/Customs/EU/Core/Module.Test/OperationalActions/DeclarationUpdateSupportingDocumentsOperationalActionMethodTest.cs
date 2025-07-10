using System.Linq;
using Enterprise.Customs.EU.Business.OperationalActions;
using Enterprise.Customs.EU.Module.OperationalActions;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing.OperationalActions
{
	[TestedType(typeof(DeclarationUpdateSupportingDocumentsOperationalActionMethod))]
	public class DeclarationUpdateSupportingDocumentsOperationalActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<DeclarationUpdateSupportingDocumentsOperationalActionMethod>
	{
		protected override DeclarationUpdateSupportingDocumentsOperationalActionMethod NewMethod()
		{
			return new DeclarationUpdateSupportingDocumentsOperationalActionMethod();
		}

		public void TestMembers()
		{
			CombineAssertions("Members", () =>
			{
				AssertEquals("Supporting Documents", actionMethod.Name);
				AssertEquals("Supporting Documents", actionMethod.Description);
				AssertType<DeclarationUpdateSupportingDocumentsApplicator>("Applicator type", actionMethod.NewApplicator(Factory, null));
				using (var control = actionMethod.NewGuiControl())
				{
					AssertType<DeclarationUpdateSupportingDocumentsApplicatorControl>("User control type", control);
				}
				Assert("HasControl", actionMethod.HasControl);
				var filter = actionMethod.GetFilterRequirements();
				AssertEquals("Filter Count", 1, filter.Count);
				AssertContainsExactElementsInAnyOrder("Filter data", new[] { "Y" }, filter["IsInEuropeanCustomsUnionOrInheritsFromEU"].ToArray());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			actionMethod = new DeclarationUpdateSupportingDocumentsOperationalActionMethod();
		}
		DeclarationUpdateSupportingDocumentsOperationalActionMethod actionMethod;
	}
}
