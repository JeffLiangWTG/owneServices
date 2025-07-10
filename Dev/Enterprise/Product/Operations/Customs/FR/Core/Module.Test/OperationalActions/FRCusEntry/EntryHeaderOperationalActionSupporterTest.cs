using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Module.Testing;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	[TestedType(typeof(EntryHeaderOperationalActionSupporter))]
	public class EntryHeaderOperationalActionSupporterTest : EntryHeaderOperationalActionSupporterAbstractTest<EntryHeaderOperationalActionSupporter>
	{
		public void TestPopulateMethods()
		{
			var supporter = new EntryHeaderOperationalActionSupporter();
			var reulst = new ActionMethodProviderID[] { ActionMethodProviderIDs.General,
				ActionMethodProviderIDs.FRCusEntry
			};
			AssertContainsExactElementsInAnyOrder(reulst, supporter.Methods.GetAllIds());
		}

		public override BusinessObject NewTarget()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.CustomsEntryHeaders.AddNew();
		}
	}
}
