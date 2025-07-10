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
	[TestedType(typeof(ManifestForwardFilterBusinessObject))]
	sealed class ManifestForwardFilterBusinessObjectTest : EDIMessageFilterBusinessObjectTest
	{
		public void TestSubLocationFilter()
		{
			var filter = (ModuleNkFilter)filters.ModuleFilters[ManifestForwardFilterBusinessObject.Constants.SubLocation];
			AssertNotNull("Sub-Location filter exists", filter);
			AssertEquals(FilterCategories.Locations, filter.Category);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Exact;
			filter.Property = "1111";
			AssertFilterResult(filter, new[] { message1, message3 });

			filter.Property = "2222";
			AssertFilterResult(filter, new[] { message2 });

			var message4 = GetEDIReleaseMessage("12345000067897", "CB", "12345678912345", "4", "", "4444");
			var message5 = GetEDIReleaseMessageWithoutLOC("12345000067897", "CB", "12345678912345", "4");
			Factory.Save();

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.IsBlank;
			AssertFilterResult(filter, new[] { message4, message5 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.IsNotBlank;
			AssertFilterResult(filter, new[] { message1, message2, message3 });
		}

		public void TestCBSAOfficeFilter()
		{
			var filter = (ModuleNkFilter)filters.ModuleFilters[ManifestForwardFilterBusinessObject.Constants.CBSAOffice];
			AssertNotNull("CBSA Office filter exists", filter);
			AssertEquals(FilterCategories.Locations, filter.Category);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Exact;
			filter.Property = "4444";
			AssertFilterResult(filter, new[] { message1 });

			filter.Property = "5555";
			AssertFilterResult(filter, new[] { message2, message3 });

			var message4 = GetEDIReleaseMessage("12345000067897", "CB", "12345678912345", "4", "4444", "");
			var message5 = GetEDIReleaseMessageWithoutLOC("12345000067897", "CB", "12345678912345", "4");
			Factory.Save();

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.IsBlank;
			AssertFilterResult(filter, new[] { message4, message5 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.IsNotBlank;
			AssertFilterResult(filter, new[] { message1, message2, message3 });
		}

		public void TestHouseCCNFilter()
		{
			var filter = (ModuleNumberFilter)filters.ModuleFilters[ManifestForwardFilterBusinessObject.Constants.HouseCCN];
			AssertNotNull("House CCN filter exists", filter);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Exact;
			filter.Property = "12345000012345";
			AssertFilterResult(filter, new[] { message2 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotEqual;
			AssertFilterResult(filter, new[] { message1, message3 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Contains;
			filter.Property = "67";
			AssertFilterResult(filter, new[] { message1, message3 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotContain;
			AssertFilterResult(filter, new[] { message2 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.StartsWith;
			filter.Property = "1234";
			AssertFilterResult(filter, new[] { message1, message2 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotStartsWith;
			AssertFilterResult(filter, new[] { message3 });

			AssertEquals(filter.MaxLength, ModuleNumberFilter.MultiplyMaxLength(EDIMessageSchema.EM_ApplicationReference.MaxLength));
		}

		public void TestPrimaryCCNFilter()
		{
			var filter = (ModuleTextFilter)filters.ModuleFilters[ManifestForwardFilterBusinessObject.Constants.PrimaryCCN];
			AssertNotNull("Primary CCN filter exists", filter);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Exact;
			filter.Property = "91651782345691";
			AssertFilterResult(filter, new[] { message2 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotEqual;
			AssertFilterResult(filter, new[] { message1, message3 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Contains;
			filter.Property = "123456";
			AssertFilterResult(filter, new[] { message1, message3 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotContain;
			AssertFilterResult(filter, new[] { message2 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.StartsWith;
			filter.Property = "91651";
			AssertFilterResult(filter, new[] { message2, message3 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotStartsWith;
			AssertFilterResult(filter, new[] { message1 });

			AssertEquals(GenAddOnColumnSchema.XA_Data.MaxLength, filter.MaxLength);
		}

		public void TestSNPTypeFilter()
		{
			var filter = (ModuleTextFilter)filters.ModuleFilters[ManifestForwardFilterBusinessObject.Constants.SNPType];
			AssertNotNull("SNP Type filter exists", filter);
			AssertEquals(FilterCategories.ModesAndTypes, filter.Category);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Exact;
			filter.Property = "CB";
			AssertFilterResult(filter, new[] { message1 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotEqual;
			AssertFilterResult(filter, new[] { message2, message3 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Exact;
			filter.Property = "FW";
			AssertFilterResult(filter, new[] { message2 });

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotEqual;
			AssertFilterResult(filter, new[] { message1, message3 });

			AssertEquals(GenAddOnColumnSchema.XA_Data.MaxLength, filter.MaxLength);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ManifestForwardFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			if (RunMethod.Name != "TestFilterByMessageText" && RunMethod.Name != "TestFilterBySender" && RunMethod.Name != "TestFilterByReceiver")
			{
				message1 = GetEDIReleaseMessage("12345000067897", "CB", "12345678912345", "4", "1111", "4444");
				message2 = GetEDIReleaseMessage("12345000012345", "FW", "91651782345691", "4", "2222", "5555");
				message3 = GetEDIReleaseMessage("10207000067897", "WH", "91651234567893", "9", "1111", "5555");
				Factory.Save();

				filters = new ManifestForwardFilterBusinessObject();
			}
		}

		void AssertFilterResult(ModuleTextBaseFilter filter, EDIMessage[] expectedMessages)
		{
			var actualMessages = Factory.Load<EDIMessage>(filter.Query);
			AssertEquals(filter.ComparisonOperator + " filter result count", expectedMessages.Length, actualMessages.Length);
			foreach (var expectedMessage in expectedMessages)
			{
				AssertEquals(filter.ComparisonOperator + " filter result should contain expected message", true, actualMessages.Contains(expectedMessage));
			}
		}

		EDIMessage GetEDIReleaseMessage(string houseCCN, string snpType, string primaryCCN, string messageFunction, string subLocation, string cbsaOffice)
		{
			var message = Factory.New<EDIReleaseMessage>();
			message.EM_MessageText = string.Format(
				@"UNH+1+GOVCBR:D:11B:UN
BGM+714+{0}+{3}
RFF+AFM:10207:{1}
RFF+UCN:UCR555
DOC+23+:24
DOC+85+{2}
RCS+15
FTX+ACB+++SOM B 2 B COMMENTS
TDT+11++1
UNS+D
HYN+3
CNI+1
STS++0
NAD+CN+++VANCOUVER IMPORT/ EXPORT COMPANY+99 MAIN ST+VANCOUVER+BC+V6B3G2+CA
NAD+CZ+++TREETOYS PTY LTD+105 WOMBAT DRIVE+KATOOMBA+NSW+2780+AU
LOC+8+{5}+{4}'
LOC+11+0495+3559'", houseCCN, snpType, primaryCCN, messageFunction, subLocation, cbsaOffice).Replace("\r\n", "'");
			message.EM_ApplicationReference = houseCCN;
			message.SetSystemDefinedValue(EDIMessage.Schema.SubLocation, new ZString(subLocation));
			message.SetSystemDefinedValue(EDIMessage.Schema.CBSAOffice, new ZString(cbsaOffice));
			message.SetSystemDefinedValue(EDIMessage.Schema.SNPType, (ZString)snpType);
			message.SetSystemDefinedValue("PrimaryCCN", (ZString)primaryCCN);
			return message;
		}
		EDIMessage GetEDIReleaseMessageWithoutLOC(string houseCCN, string snpType, string primaryCCN, string messageFunction)
		{
			var message = Factory.New<EDIReleaseMessage>();
			message.EM_MessageText = string.Format(
				@"UNH+1+GOVCBR:D:11B:UN
BGM+714+{0}+{3}
RFF+AFM:10207:{1}
RFF+UCN:UCR555
DOC+23+:24
DOC+85+{2}
RCS+15
FTX+ACB+++SOM B 2 B COMMENTS
TDT+11++1
UNS+D
HYN+3
CNI+1
STS++0
NAD+CN+++VANCOUVER IMPORT/ EXPORT COMPANY+99 MAIN ST+VANCOUVER+BC+V6B3G2+CA
NAD+CZ+++TREETOYS PTY LTD+105 WOMBAT DRIVE+KATOOMBA+NSW+2780+AU
LOC+11+0495+3559'", houseCCN, snpType, primaryCCN, messageFunction).Replace("\r\n", "'");

			message.EM_ApplicationReference = houseCCN;
			message.SetSystemDefinedValue(EDIMessage.Schema.SNPType, (ZString)snpType);
			message.SetSystemDefinedValue("PrimaryCCN", (ZString)primaryCCN);
			return message;
		}

		EDIMessage message1;
		EDIMessage message2;
		EDIMessage message3;
		ManifestForwardFilterBusinessObject filters;
	}
}
