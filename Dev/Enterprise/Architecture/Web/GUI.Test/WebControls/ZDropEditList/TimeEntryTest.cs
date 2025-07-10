using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[TestedType(typeof(ZTimeEditList.TimeEntry))]
	sealed class TimeEntryTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ZTimeEditList.TimeEntry(0, 0);
		}
	}
}
