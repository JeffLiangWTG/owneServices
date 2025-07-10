using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDIMessageTextFilter))]
	sealed class EDIMessageTextFilterTest : ModuleTextFilterTest
	{
		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new EDIMessageTextFilter("Message Text", new EDIMessageFilterBusinessObject());
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		protected override ZString ExpectedDescription
		{
			get { return "Message Text"; }
		}

		public void TestEDIMessageTextFilter()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageData = CompressBytes(Encoding.UTF8.GetBytes("Test Data"));
			message.EM_SystemCreateTimeUtc = new DateTime(2016, 05, 18);
			Factory.Save();

			var messageFilterBusinessObject = new EDIMessageFilterBusinessObject();
			var filter = (EDIMessageTextFilter)messageFilterBusinessObject["Message Text"];
			var subGroup = (ModuleFilterSubGroup)filter.SubGroup;
			{
				filter.Property = "Test";
				filter.ComparisonOperator = EDIMessageTextFilter.ComparisonConstants.StartsWith;
				EDIMessage[] messages = Factory.Load<EDIMessage>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("Message text StartsWith 'Test' should be 1", 1, messages.Length);
				Assert("Message text StartsWith 'Test' Should contain message", messages.Contains<EDIMessage>(message));
			}

			{
				filter.Property = "Test";
				filter.ComparisonOperator = EDIMessageTextFilter.ComparisonConstants.Contains;
				EDIMessage[] messages = Factory.Load<EDIMessage>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("Message text contains 'Test' should be 1", 1, messages.Length);
				Assert("Message text contains 'Test' Should contain message", messages.Contains<EDIMessage>(message));
			}

			{
				filter.Property = "AAA";
				filter.ComparisonOperator = EDIMessageTextFilter.ComparisonConstants.NotContain;
				EDIMessage[] messages = Factory.Load<EDIMessage>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("Message text not contains 'AAA' should be 1", 1, messages.Length);
				Assert("Message text not contains 'AAA' Should contain message", messages.Contains<EDIMessage>(message));
			}
		}

		public void TestEDIMessageTextFilterValidationErrors()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageData = CompressBytes(Encoding.UTF8.GetBytes("Test Data"));
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();

			var messageFilterBusinessObject = new EDIMessageFilterBusinessObject();
			var textFilter = (EDIMessageTextFilter)messageFilterBusinessObject["Message Text"];
			textFilter.IsActive = true;
			textFilter.Property = "Test";
			textFilter.ComparisonOperator = EDIMessageTextFilter.ComparisonConstants.StartsWith;

			messageFilterBusinessObject.FilterStrips.AddNew("Message Text");
			textFilter.Validation.ValidateAll();
			AssertHasError(textFilter.PropertyInfo, "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Date based filter with a range of 7 days or less. (E.g.: Created Time (UTC))");

			messageFilterBusinessObject.FilterStrips.AddNew("Message Time");
			var dateFilter = (ModuleDateFilter)messageFilterBusinessObject.FilterStrips[1].CurrentModuleFilter;
			dateFilter.Property1 = ZDateTime.Now.AddDays(-10);
			dateFilter.Property2 = ZDateTime.Now.AddDays(+1);
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			textFilter.Validation.ValidateAll();
			AssertHasError(textFilter.PropertyInfo, "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Date based filter with a range of 7 days or less. (E.g.: Created Time (UTC))");

			dateFilter.Property1 = ZDateTime.Now.AddDays(-3);
			dateFilter.Property2 = ZDateTime.Now.AddDays(+1);
			textFilter.Validation.ValidateAll();
			AssertNoErrors(textFilter.PropertyInfo);
		}

		public void TestEDIMessageTextInvalidDatesValidation()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageData = CompressBytes(Encoding.UTF8.GetBytes("Test Data"));
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();

			var messageFilterBusinessObject = new EDIMessageFilterBusinessObject();

			messageFilterBusinessObject.FilterStrips.AddNew("Message Text");
			var textFilter = (EDIMessageTextFilter)messageFilterBusinessObject["Message Text"];
			textFilter.IsActive = true;
			textFilter.Property = "Test";
			textFilter.ComparisonOperator = EDIMessageTextFilter.ComparisonConstants.StartsWith;

			messageFilterBusinessObject.FilterStrips.AddNew("Message Time");
			var dateFilter = (ModuleDateFilter)messageFilterBusinessObject["Message Time"];
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			dateFilter.Property1 = ZDateTime.Now.AddDays(-1);
			dateFilter.Property2 = ZDateTime.Now.AddDays(+1);
			textFilter.Validation.ValidateAll();
			AssertNoErrors(textFilter.PropertyInfo);

			dateFilter.Property1 = ZDateTime.Invalid;
			dateFilter.Property2 = ZDateTime.Now.AddDays(+1);
			textFilter.Validation.ValidateAll();
			AssertHasError(textFilter.PropertyInfo, "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Date based filter with a range of 7 days or less. (E.g.: Created Time (UTC))");

			dateFilter.Property1 = ZDateTime.Now.AddDays(-1);
			dateFilter.Property2 = ZDateTime.Invalid;
			textFilter.Validation.ValidateAll();
			AssertHasError(textFilter.PropertyInfo, "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Date based filter with a range of 7 days or less. (E.g.: Created Time (UTC))");
		}

		public void TestErrorsNotCopiedTwiceWhenCloningEDIMessageTextFilter()
		{
			var messageFilterBusinessObject = new EDIMessageFilterBusinessObject();

			messageFilterBusinessObject.FilterStrips.AddNew("Message Text");
			var textFilter1 = (EDIMessageTextFilter)messageFilterBusinessObject["Message Text"];
			textFilter1.IsActive = true;
			textFilter1.Property = "Test";
			textFilter1.ComparisonOperator = EDIMessageTextFilter.ComparisonConstants.StartsWith;

			messageFilterBusinessObject.FilterStrips.AddNew("Message Text");
			var textFilter2 = (EDIMessageTextFilter)messageFilterBusinessObject.FilterStrips[1].CurrentModuleFilter;
			textFilter2.IsActive = true;
			textFilter2.Property = "Te";
			textFilter2.ComparisonOperator = EDIMessageTextFilter.ComparisonConstants.Contains;

			AssertHasError(textFilter1.PropertyInfo, "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Date based filter with a range of 7 days or less. (E.g.: Created Time (UTC))");
			AssertHasError(textFilter2.PropertyInfo, "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Date based filter with a range of 7 days or less. (E.g.: Created Time (UTC))");

			messageFilterBusinessObject.FilterStrips.AddNew("Message Time");
			var dateFilter = (ModuleDateFilter)messageFilterBusinessObject.FilterStrips[2].CurrentModuleFilter;
			dateFilter.Property1 = ZDateTime.Now.AddDays(-3);
			dateFilter.Property2 = ZDateTime.Now.AddDays(+1);
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;

			textFilter1.Validation.ValidateAll();
			textFilter2.Validation.ValidateAll();

			AssertNoErrors(textFilter1);
			AssertNoErrors(textFilter2);
		}

		static byte[] CompressBytes(byte[] buffer)
		{
			using (MemoryStream ms = new MemoryStream(buffer.Length))
			{
				WriteCompressedStream(buffer, ms);
				return ms.ToArray();
			}
		}

		static void WriteCompressedStream(byte[] bytes, Stream stream)
		{
			stream.Write(new byte[] { (byte)'P', (byte)'Z' }, 0, 2);

			using (DeflateStream zipStream = new DeflateStream(stream, CompressionMode.Compress, true))
			{
				zipStream.Write(bytes, 0, bytes.Length);
			}
		}
	}
}
