using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPEModuleFilter))]
	public class UPEModuleFilterTest : ModuleFilterTestCase<UPEModuleFilter>
	{
		#region TestIsExpensiveQuery
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion
		protected override UPEModuleFilter GetNewModuleFilter()
		{
			QueueFilterHelper helper = new QueueFilterHelper(new UPEJobDeclarationFilterBusinessObject());
			return new UPEModuleFilter(helper);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get
			{
				return UPEFilterConstants.UPEFilterCategory;
			}
		}

		protected override ZString ExpectedDescription
		{
			get
			{
				return "Queue Status";
			}
		}
	}
}
