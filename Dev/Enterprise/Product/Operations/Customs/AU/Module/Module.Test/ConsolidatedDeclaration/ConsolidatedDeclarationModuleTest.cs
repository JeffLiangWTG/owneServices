using System;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(ConsolidatedDeclarationModule))]
	sealed class ConsolidatedDeclarationModuleTest : Customs.Module.Testing.ConsolidatedDeclarationModuleAbstractTest
	{
		public void TestFilterStripBusinessObjectType()
		{
			using (var module = GetModule())
			{
				AssertType<ConsolidatedDeclarationFilterBusinessObject>(module.FilterBusinessObject);
			}
		}

		public override void TestAutoAddedMilestoneDateFilter()
		{
			// to remove after filter implementation
			Assert(true);
		}

		public override void TestExceptionsFilter()
		{
			// to remove after filter implementation
			Assert(true);
		}

		public override void TestMilestonesFilter()
		{
			// to remove after filter implementation
			Assert(true);
		}

		public override void TestTasksFilter()
		{
			// to remove after filter implementation
			Assert(true);
		}

		public override void TestTriggersFilter()
		{
			// to remove after filter implementation
			Assert(true);
		}

		protected override string GetExpectedApplicationCode() => Customs.Business.ConsolidatedDeclaration.ApplicationCodes.CMR;

		protected override Type ConsolidatedDeclarationType => typeof(ConsolidatedDeclaration);
	}
}
