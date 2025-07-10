using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[TestedType(typeof(ZTimezoneEditList.TimezoneList))]
	sealed class TimezoneListTest : NonPersistentBusinessObjectCollectionTestCase<ZTimezoneEditList.TimezoneList>
	{
		protected override ZTimezoneEditList.TimezoneList GetCollectionToTest()
		{
			return new ZTimezoneEditList.TimezoneList();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ZTimezoneEditList.TimezoneEntry("GMT +00:00");
		}
	}
}
