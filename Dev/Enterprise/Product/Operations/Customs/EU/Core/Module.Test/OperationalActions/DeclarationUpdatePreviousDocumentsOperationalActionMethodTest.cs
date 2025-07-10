using System.Linq;
using Enterprise.Customs.EU.Business.OperationalActions;
using Enterprise.Customs.EU.Module.OperationalActions;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing.OperationalActions
{
	[TestedType(typeof(DeclarationUpdatePreviousDocumentsOperationalActionMethod))]
	public class DeclarationUpdatePreviousDocumentsOperationalActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<DeclarationUpdatePreviousDocumentsOperationalActionMethod>
	{
		protected override DeclarationUpdatePreviousDocumentsOperationalActionMethod NewMethod()
		{
			return new DeclarationUpdatePreviousDocumentsOperationalActionMethod();
		}

		public void TestApplicatorTypeDependingOnCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var actionMethod = new DeclarationUpdatePreviousDocumentsOperationalActionMethod();
				AssertType<DeclarationUpdatePreviousDocumentsApplicator>(actionMethod.NewApplicator(Factory, null));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var actionMethod = new DeclarationUpdatePreviousDocumentsOperationalActionMethod();
				AssertEquals("Type should be country dependant when able.", "Enterprise.Customs.FR.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator", actionMethod.NewApplicator(Factory, null).GetType().ToString());
			}
		}

		public void TestMembers()
		{
			CombineAssertions("Members", () =>
			{
				AssertEquals("Previous Documents", actionMethod.Name);
				AssertEquals("Previous Documents", actionMethod.Description);
				AssertType<DeclarationUpdatePreviousDocumentsApplicator>("Applicator type", actionMethod.NewApplicator(Factory, null));
				using (var control = actionMethod.NewGuiControl())
				{
					AssertType<DeclarationUpdatePreviousDocumentsApplicatorControl>("User control type", control);
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
			actionMethod = new DeclarationUpdatePreviousDocumentsOperationalActionMethod();
		}
		DeclarationUpdatePreviousDocumentsOperationalActionMethod actionMethod;
	}
}
