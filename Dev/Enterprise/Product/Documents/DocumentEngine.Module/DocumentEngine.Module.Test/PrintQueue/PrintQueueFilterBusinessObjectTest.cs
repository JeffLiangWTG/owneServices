using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.Testing
{
	[TestedType(typeof(PrintQueueFilterBusinessObject))]
	sealed class PrintQueueFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLastUsedDateFilter()
		{
			var filter = GetNewFilterStripBusinessObject() as PrintQueueFilterBusinessObject;
			var lastUsedDataFilterUtc = filter["Last Used Date (UTC)"];
			var lastUsedDataFilterLocal = filter["Last Used Date (Local)"];

			AssertNotNull(lastUsedDataFilterUtc);
			AssertNotNull(lastUsedDataFilterLocal);
			AssertEquals(FilterCategories.AuditInformation, lastUsedDataFilterUtc.Category);
			AssertEquals(FilterCategories.AuditInformation, lastUsedDataFilterLocal.Category);
		}

		public void TestAllowPrintingFilter()
		{
			PrintQueueFilterBusinessObject filter = GetNewFilterStripBusinessObject() as PrintQueueFilterBusinessObject;
			AssertNotNull(filter["Allow Printing Only"]);
			AssertEquals(FilterVisibility.AlwaysVisible, filter["Allow Printing Only"].Visibility);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new PrintQueueFilterBusinessObject();
		}

		#endregion
	}
}
