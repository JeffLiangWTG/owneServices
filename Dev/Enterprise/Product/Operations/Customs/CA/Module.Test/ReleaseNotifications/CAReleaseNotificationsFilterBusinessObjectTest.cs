using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CAReleaseNotificationsFilterBusinessObject))]
	sealed class CAReleaseNotificationsFilterBusinessObjectTest : EDIMessageFilterBusinessObjectTest
	{
		public void TestWarehouseCodeFilter()
		{
			var filter = (ModuleNkFilter)filters.ModuleFilters[CAReleaseNotificationsFilterBusinessObject.Constants.WarehouseCode];
			AssertNotNull("Warehouse Code filter exists", filter);
			AssertEquals(FilterCategories.Locations, filter.Category);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Exact;
			filter.Property = "3395";
			AssertFilterResult(filter, new[] { message2 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotEqual;
			AssertFilterResult(filter, new[] { message1, message3, message4 });

			var message5 = GetEDIReleaseMessage("12345000067897", "", "12345678912345", "0497");
			var message6 = GetEDIReleaseMessageWithoutLOC("12345000067897", "12345678912345");
			Factory.Save();

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.IsBlank;
			AssertFilterResult(filter, new[] { message4, message5, message6 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.IsNotBlank;
			AssertFilterResult(filter, new[] { message1, message2, message3 });
		}

		public void TestOfficeFilter()
		{
			var filter = (ModuleNkFilter)filters.ModuleFilters[CAReleaseNotificationsFilterBusinessObject.Constants.CBSAOffice];
			AssertNotNull("CBSA Office filter exists", filter);
			AssertEquals(FilterCategories.Locations, filter.Category);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Exact;
			filter.Property = "0497";
			AssertFilterResult(filter, new[] { message1, message3 });

			filter.Property = "0495";
			AssertFilterResult(filter, new[] { message2, message4 });

			var message5 = GetEDIReleaseMessage("12345000067897", "3072", "12345678912345", "");
			var message6 = GetRNSRequest("12345000067897", "12345678912345", "");
			var message7 = GetEDIReleaseMessageWithoutLOC("12345000067897", "12345678912345");
			var message8 = GetRNSRequestWithoutLOC("12345000067897", "12345678912345");
			Factory.Save();

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.IsBlank;
			AssertFilterResult(filter, new[] { message5, message6, message7, message8 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.IsNotBlank;
			AssertFilterResult(filter, new[] { message1, message2, message3, message4 });
		}

		public void TestCargoControlNumberFilter()
		{
			var filter = (ModuleNumberFilter)filters.ModuleFilters[CAReleaseNotificationsFilterBusinessObject.Constants.CargoControlNumber];
			AssertNotNull("Cargo Control Number filter exists", filter);
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Exact;
			filter.Property = "12345678912345";
			AssertFilterResult(filter, new[] { message1, message4 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotEqual;
			AssertFilterResult(filter, new[] { message2, message3 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Contains;
			filter.Property = "123456789";
			AssertFilterResult(filter, new[] { message1, message3, message4 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotContain;
			AssertFilterResult(filter, new[] { message2 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.StartsWith;
			filter.Property = "91651";
			AssertFilterResult(filter, new[] { message2, message3 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotStartsWith;
			AssertFilterResult(filter, new[] { message1, message4 });

			AssertEquals(filter.MaxLength, ModuleNumberFilter.MultiplyMaxLength(EDIMessageSchema.EM_ApplicationReference.MaxLength));
		}

		public void TestTransactionNumberFilter()
		{
			var filter = (ModuleTextFilter)filters.ModuleFilters[CAReleaseNotificationsFilterBusinessObject.Constants.TransactionNumber];
			AssertNotNull("Transaction Number filter exists", filter);
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Exact;
			filter.Property = "12345000012345";
			AssertFilterResult(filter, new[] { message2 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotEqual;
			AssertFilterResult(filter, new[] { message1, message3, message4 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Contains;
			filter.Property = "00006";
			AssertFilterResult(filter, new[] { message1, message3, message4 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotContain;
			AssertFilterResult(filter, new[] { message2 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.StartsWith;
			filter.Property = "12345";
			AssertFilterResult(filter, new[] { message1, message2, message4 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotStartsWith;
			AssertFilterResult(filter, new[] { message3 });

			AssertEquals(EDIMessageSchema.EM_ApplicationReference.MaxLength, filter.MaxLength);
		}

		public void TestRNSProcessingDateFilters()
		{
			var dateFilter = (ModuleDateFilter)filters[CAReleaseNotificationsFilterBusinessObject.Constants.ProcessingDate];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = new ZDateTime(2012, 1, 1);
			dateFilter.Property2 = new ZDateTime(2012, 1, 3);
			dateFilter.IsActive = true;
			var collection = new Enterprise.Messaging.Business.NonDependentEDIMessageCollection(Factory);
			collection.Load(filters.Filter);
			AssertEquals("2 messages selected", 2, collection.Count);
			Assert(CAReleaseNotificationsFilterBusinessObject.Constants.ProcessingDate, collection.Contains(message1));
			Assert(CAReleaseNotificationsFilterBusinessObject.Constants.ProcessingDate, collection.Contains(message2));
		}

		public void TestRNSReleaseDateFilters()
		{
			var dateFilter = (ModuleDateFilter)filters[CAReleaseNotificationsFilterBusinessObject.Constants.ReleaseDate];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = new ZDateTime(2012, 1, 1);
			dateFilter.Property2 = new ZDateTime(2012, 1, 3);
			dateFilter.IsActive = true;
			var collection = new Enterprise.Messaging.Business.NonDependentEDIMessageCollection(Factory);
			collection.Load(filters.Filter);
			AssertEquals("1 message selected", 1, collection.Count);
			Assert(CAReleaseNotificationsFilterBusinessObject.Constants.ReleaseDate, collection.Contains(message1));
			AssertCollectionContains(EDIMessage.Schema.RNSReleaseDate, message1, collection);
		}

		protected override void SetUp()
		{
			base.SetUp();
			if (RunMethod.Name != "TestFilterByMessageText" && RunMethod.Name != "TestFilterBySender" && RunMethod.Name != "TestFilterByReceiver")
			{
				message1 = GetEDIReleaseMessage("12345000067897", "3072", "12345678912345", "0497");
				message1.RNSProcessingDate = new ZDateTime(2012, 01, 01);
				message1.RNSReleaseDate = new ZDateTime(2012, 01, 02);
				message2 = GetEDIReleaseMessage("12345000012345", "3395", "91651782345691", "0495");
				message2.RNSProcessingDate = new ZDateTime(2012, 01, 03);
				message3 = GetEDIReleaseMessage("10207000067897", "3396", "91651234567893", "0497");
				message3.RNSProcessingDate = new ZDateTime(2012, 01, 04);
				message3.RNSReleaseDate = new ZDateTime(2012, 01, 05);
				message4 = GetRNSRequest("12345000067897", "12345678912345", "0495");
				Factory.Save();

				filters = new CAReleaseNotificationsFilterBusinessObject();
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CAReleaseNotificationsFilterBusinessObject();

		void AssertFilterResult(ModuleTextBaseFilter filter, EDIMessage[] expectedMessages)
		{
			var actualMessages = Factory.Load<EDIMessage>(filter.Query);
			AssertEquals(filter.ComparisonOperator + " filter result count", expectedMessages.Length, actualMessages.Length);
			foreach (var expectedMessage in expectedMessages)
			{
				AssertEquals(filter.ComparisonOperator + " filter result should contain expected message", true, actualMessages.Contains(expectedMessage));
			}
		}

		EDIMessage GetEDIReleaseMessage(string tranNumber, string warehouse, string ccn, string office)
		{
			var message = Factory.New<EDIReleaseMessage>();
			message.EM_MessageText = string.Format(
				@"UNH+<<MSGNO PLACEHOLDER>>+CUSRES:D:96A:UN'
BGM+:::125+{0}+11'
LOC+22+{3}:129::{1}'
DTM+9:201006221028:203'
GIS+4'
RFF+XC:{2}'
UNT+5+1'", tranNumber, warehouse, ccn, office).Replace("\r\n", "");
			message.EM_ApplicationReference = ccn;
			message.SetSystemDefinedValue(EDIMessage.Schema.TransactionNumber, new ZString(tranNumber));
			message.SetSystemDefinedValue(ReleaseStatus.Schema.RL_ReleaseOffice, new ZString(office));
			message.SetSystemDefinedValue(ReleaseStatus.Schema.RL_WarehouseCode, new ZString(warehouse));
			return message;
		}

		EDIMessage GetEDIReleaseMessageWithoutLOC(string tranNumber, string ccn)
		{
			var message = Factory.New<EDIReleaseMessage>();
			message.EM_MessageText = string.Format(
				@"UNH+<<MSGNO PLACEHOLDER>>+CUSRES:D:96A:UN'
BGM+:::125+{0}+11'
DTM+9:201006221028:203'
GIS+4'
RFF+XC:{1}'
UNT+5+1'", tranNumber, ccn).Replace("\r\n", "");
			message.SetSystemDefinedValue(EDIMessage.Schema.TransactionNumber, new ZString(tranNumber));
			return message;
		}

		EDIMessage GetRNSRequest(string tranNumber, string ccn, string office)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = string.Format(
				@"UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:96A:UN'
BGM+631'
DTM+132:200801202200:203'
RFF+ABT:{0}'
RFF+TN:{1}'
LOC+14+{2}'
UNT+7+<<MSGNO PLACEHOLDER>>'", ccn, tranNumber, office).Replace("\r\n", "");
			message.SetSystemDefinedValue(EDIMessage.Schema.TransactionNumber, new ZString(tranNumber));
			message.SetSystemDefinedValue(ReleaseStatus.Schema.RL_ReleaseOffice, new ZString(office));
			message.EM_ApplicationReference = ccn;
			return message;
		}

		EDIMessage GetRNSRequestWithoutLOC(string tranNumber, string ccn)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = string.Format(
				@"UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:96A:UN'
BGM+631'
DTM+132:200801202200:203'
RFF+ABT:{0}'
RFF+TN:{1}'
UNT+7+<<MSGNO PLACEHOLDER>>'", ccn, tranNumber).Replace("\r\n", "");
			message.SetSystemDefinedValue(EDIMessage.Schema.TransactionNumber, new ZString(tranNumber));
			return message;
		}

		EDIMessage message1;
		EDIMessage message2;
		EDIMessage message3;
		EDIMessage message4;
		CAReleaseNotificationsFilterBusinessObject filters;
	}
}
