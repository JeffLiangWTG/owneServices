using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(Alert))]
	public class AlertTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAlerts()
		{
			StringCollection stringCollection = new StringCollection();
			stringCollection.Add("Test");
			stringCollection.Add("Test1");
			Alert alert = new Alert(stringCollection, Factory);
			AssertEquals("Test\nTest1\n", alert.Alerts);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Alert(new StringCollection(), Factory);
		}
	}
}
