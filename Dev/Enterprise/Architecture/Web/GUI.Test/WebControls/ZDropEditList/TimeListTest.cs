using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[TestedType(typeof(ZTimeEditList.TimeList))]
	sealed class TimeListTest : NonPersistentBusinessObjectCollectionTestCase<ZTimeEditList.TimeList>
	{
		protected override ZTimeEditList.TimeList GetCollectionToTest()
		{
			return new ZTimeEditList.TimeList();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ZTimeEditList.TimeEntry(1, 2);
		}
	}
}
