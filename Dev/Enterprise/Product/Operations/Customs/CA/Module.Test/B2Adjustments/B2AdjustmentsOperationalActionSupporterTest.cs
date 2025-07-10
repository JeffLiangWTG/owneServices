using CargoWise.Definitions;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(B2AdjustmentsOperationalActionSupporter))]
	sealed class B2AdjustmentsOperationalActionSupporterTest : OperationalActionSupporterTest<B2AdjustmentsOperationalActionSupporter>
	{
		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.Customs, Supporter.DocumentBusinessContext);
		}

		public void TestPopulateMethods()
		{
			var supporter = new B2AdjustmentsOperationalActionSupporter();
			var allIds = supporter.Methods.GetAllIds();
			AssertCollectionContains(ActionMethodProviderIDs.B2Adjustments, allIds);
		}

		public override void TestBusinessContextIsConsistent()
		{
			Assert("B2/B3X module share the same context with Declaration, but this operational action is only for B2/B3X.", true);
		}

		public override void TestDocumentBusinessContextIsConsistent()
		{
			Assert("B2/B3X module share the same context with Declaration, but this operational action is only for B2/B3X.", true);
		}

		protected override ModuleIdentifier ModuleID => ModuleIDs.Customs.CA.B2Adjustments;
	}
}
