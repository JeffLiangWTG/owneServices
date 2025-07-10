using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[TestedType(typeof(ZTimezoneEditList.TimezoneEntry))]
	sealed class TimezoneEntryTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ZTimezoneEditList.TimezoneEntry("GMT +00:00");
		}
	}
}
