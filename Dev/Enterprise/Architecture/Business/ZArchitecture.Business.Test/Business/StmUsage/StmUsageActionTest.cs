using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmUsageAction))]
	sealed class StmUsageActionTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var stmusageaction = (StmUsageAction)base.GetNewBusinessObjectForDeleteTest(factory);
			stmusageaction.XO_ActionCount = 1;
			stmusageaction.XO_StartTimeUtc = ZDateTime.BrettsBirthday;
			stmusageaction.XO_EndTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);
			stmusageaction.XO_ElapsedWithChildrenSeconds = 1m;
			stmusageaction.XO_ElapsedWithoutChildrenSeconds = 1m;
			stmusageaction.Usage.XW_StartTimeUtc = ZDateTime.BrettsBirthday;
			stmusageaction.Usage.XW_EndTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);
			return stmusageaction;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var stmusageaction = (StmUsageAction)Factory.NewWithValidTestData(GetExpectedBusinessObjectType(), TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections);
			stmusageaction.XO_ActionCount = 1;
			stmusageaction.XO_StartTimeUtc = ZDateTime.BrettsBirthday;
			stmusageaction.XO_EndTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);
			stmusageaction.XO_ElapsedWithChildrenSeconds = 1m;
			stmusageaction.XO_ElapsedWithoutChildrenSeconds = 1m;
			stmusageaction.Usage.XW_StartTimeUtc = ZDateTime.BrettsBirthday;
			stmusageaction.Usage.XW_EndTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);
			return stmusageaction;
		}
	}
}
