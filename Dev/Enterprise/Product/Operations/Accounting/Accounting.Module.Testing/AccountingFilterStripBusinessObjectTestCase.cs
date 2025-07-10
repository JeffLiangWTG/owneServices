using System.Linq;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccountingFilterStripBusinessObject))]
	public class AccountingFilterStripBusinessObjectTestCase : FilterStripBusinessObjectTestCase
	{
		TestObjectCreator fTestObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();
			fTestObjectCreator = new TestObjectCreator(Factory);
		}
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccountingFilterStripBusinessObject();
		}

		public void TestActiveStatusFilterNotAlwaysAppliedInAccountingModules()
		{
			AccountingFilterStripBusinessObject filterStrip = new AccountingFilterStripBusinessObject();
			filterStrip.SetActiveStatusFilter(DummyBizoSchema.Z0_IsValid, false);

			ModuleTextFilter filter = (ModuleTextFilter)filterStrip["Active Status"];
			Assert("Active Status Filter must be visible and must not be Always Applied in Accounting Modules", filter.Visibility == FilterVisibility.Visible);
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator; }
		}

		protected override ModuleFilter SetupFilterForTest(ModuleFilter moduleFilter)
		{
			return
				new[]
				{
					MatchingFilterBusinessObject.AllNumbers,
					GenericConsolFilterBusinessObject.FilterConstants.FlightVoyageNumber,
				}.Contains(moduleFilter.Description.ToString()) ? null : base.SetupFilterForTest(moduleFilter);
		}
	}
}
