using Enterprise.Core;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	sealed class EDIInterchangeAssemblyDataTest : TestCase
	{
		public void TestOverrides()
		{
			var data = new EDIInterchangeAssemblyData();
			AssertEquals("BusinessObjectType", typeof(EDIInterchange), data.BusinessObjectType);
			AssertEquals("ReferenceType", Constants.ReferenceTypes.Unallocated, data.ReferenceType);
		}
	}
}
