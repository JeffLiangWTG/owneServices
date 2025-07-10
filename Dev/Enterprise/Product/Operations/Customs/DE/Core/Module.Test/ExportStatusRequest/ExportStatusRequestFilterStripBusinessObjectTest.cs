using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(ExportStatusRequestFilterBusinessObject))]
	class ExportStatusRequestFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestModuleFilterForNCTS()
		{
			var moduleFilter = (ModuleTextFilter)filter[ExportStatusRequestFilterBusinessObject.Schema.Module];
			moduleFilter.Property = ExportStatusRequestModuleCodeList.Codes.NCTS;
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("NCTS Status Request", true, nctsStatusRequest.MatchesFilter(filter.Filter));
				AssertEquals("AES Status Request", false, aesStatusRequest.MatchesFilter(filter.Filter));
			});
		}

		public void TestModuleFilterForAES()
		{
			var moduleFilter = (ModuleTextFilter)filter[ExportStatusRequestFilterBusinessObject.Schema.Module];
			moduleFilter.Property = ExportStatusRequestModuleCodeList.Codes.AES;
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Module group", FilterCategories.ModesAndTypes, moduleFilter.Category);
				AssertEquals("NCTS Status Request", false, nctsStatusRequest.MatchesFilter(filter.Filter));
				AssertEquals("AES Status Request", true, aesStatusRequest.MatchesFilter(filter.Filter));
			});
		}

		public void TestMRNCategory()
		{
			var mrnFilter = (ModuleTextFilter)filter[ExportStatusRequestFilterBusinessObject.Schema.MRN];
			AssertEquals(FilterCategories.NumbersAndReferences, mrnFilter.Category);
		}

		public void TestMRNSupportsBlankComparisonOperators()
		{
			var mrnFilter = (ModuleTextFilter)filter[ExportStatusRequestFilterBusinessObject.Schema.MRN];
			AssertEquals(false, mrnFilter.SupportsBlankComparisonOperators);
		}

		public void TestMRNStartsWith()
		{
			Factory.Save(); // DBOnlyQuery
			var mrnFilter = (ModuleTextFilter)filter[ExportStatusRequestFilterBusinessObject.Schema.MRN];
			mrnFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			mrnFilter.Property = "20DE1234";
			mrnFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("NCTS Status Request", true, nctsStatusRequest.MatchesFilter(filter.Filter));
				AssertEquals("AES Status Request", true, aesStatusRequest.MatchesFilter(filter.Filter));
			});
		}

		public void TestMRNEquals()
		{
			Factory.Save(); // DBOnlyQuery
			var mrnFilter = (ModuleTextFilter)filter[ExportStatusRequestFilterBusinessObject.Schema.MRN];
			mrnFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			mrnFilter.Property = "20DE12345678901234";
			mrnFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("NCTS Status Request", true, nctsStatusRequest.MatchesFilter(filter.Filter));
				AssertEquals("AES Status Request", false, aesStatusRequest.MatchesFilter(filter.Filter));
			});
		}

		public void TestHasResponse()
		{
			Factory.Save();
			var hasResponseFilter = (ModuleFlagsFilter)filter[ExportStatusRequestFilterBusinessObject.Schema.HasResponse];
			hasResponseFilter.Property0 = true;
			hasResponseFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Module group", FilterCategories.StatusAndFlags, hasResponseFilter.Category);
				AssertEquals("NCTS Status Request", false, nctsStatusRequest.MatchesFilter(filter.Filter));
				AssertEquals("AES Status Request", true, aesStatusRequest.MatchesFilter(filter.Filter));
			});
		}

		public void TestNoResponse()
		{
			Factory.Save();
			var hasResponseFilter = (ModuleFlagsFilter)filter[ExportStatusRequestFilterBusinessObject.Schema.HasResponse];
			hasResponseFilter.Property0 = false;
			hasResponseFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("NCTS Status Request", true, nctsStatusRequest.MatchesFilter(filter.Filter));
				AssertEquals("AES Status Request", false, aesStatusRequest.MatchesFilter(filter.Filter));
			});
		}

		public void TestDefaultCreatedTimeFilter()
		{
			var createdTimeFilter = (ModuleDateFilter)filter[FilterDescriptions.CreatedTime];
			CombineAssertions(() =>
			{
				AssertEquals("AlwaysVisible", FilterVisibility.AlwaysVisible, createdTimeFilter.Visibility);
				AssertEquals("Default Value", ModuleDateFilter.DateRangeSearchTexts.Last3Mths, createdTimeFilter.PropertySearch);
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ExportStatusRequestFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			nctsStatusRequest = CreateStatusRequest(ApplicationCodes.DECustomsAtlasSystem, Messaging.EDIMessageTypeList.Codes.NCTS, "20DE12345678901234");
			aesStatusRequest = CreateStatusRequest(ApplicationCodes.DECustomsAesSystem, Messaging.EDIMessageTypeList.Codes.AES, "20DE12349999999999");
			aesStatusRequest.EM_Status = EDIMessage.Status.Acknowledged;
			filter = new ExportStatusRequestFilterBusinessObject();
			filter.QueryObjectType = typeof(StatusRequest);
		}
		StatusRequest nctsStatusRequest;
		StatusRequest aesStatusRequest;
		ExportStatusRequestFilterBusinessObject filter;

		StatusRequest CreateStatusRequest(ZString applicationCode, ZString messageType, ZString mrn)
		{
			var result = Factory.New<StatusRequest>();
			result.EM_ApplicationCode = applicationCode;
			result.EM_MessageType = messageType;
			result.MovementReferenceNumber = mrn;
			return result;
		}
	}
}
