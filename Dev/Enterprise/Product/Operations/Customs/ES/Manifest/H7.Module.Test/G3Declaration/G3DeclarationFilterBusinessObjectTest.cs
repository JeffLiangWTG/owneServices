using CargoWise.Types;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Module.Testing
{
	[TestedType(typeof(G3DeclarationFilterBusinessObject))]
	class G3DeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filterBO = GetNewFilterStripBusinessObject();

			CombineAssertions("Filters", () =>
			{
				AssertNotNull("Job # filter", filterBO["Job #"]);
				AssertNotNull("G3 Interchange Number filter", filterBO["G3 Interchange Number"]);
				AssertNotNull("G3 Message Number filter", filterBO["G3 Message Number"]);
				AssertNotNull("DSDT/Flight No. filter", filterBO["DSDT/Flight No."]);
				AssertNotNull("Declarant filter", filterBO["Declarant"]);
				AssertNotNull("Presenter filter", filterBO["Presenter"]);
				AssertNotNull("G3 Message Status filter", filterBO["G3 Message Status"]);
				AssertNotNull("G3 Direction filter", filterBO["G3 Direction"]);
				AssertNotNull("G3 Message Type filter", filterBO["G3 Message Type"]);
			});
		}

		public void TestJobNumberFilter()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "MAN001";
			var matchedMessage = header1.Messages.AddNew();
			matchedMessage.MessageNumberStrategy = new G3MessageNumberStrategyForTest("111");
			matchedMessage.EM_MessageType = "G3D";

			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "MAN002";
			var unmatchedMessage = header2.Messages.AddNew();
			unmatchedMessage.MessageNumberStrategy = new G3MessageNumberStrategyForTest("222");
			unmatchedMessage.EM_MessageType = "G3D";

			Factory.Save();

			var filter = (ModuleTextFilter)filterBO["Job #"];
			filter.IsActive = true;
			filter.Property = "MAN001";

			CombineAssertions("Job # filter", () =>
			{
				AssertEquals(true, matchedMessage.MatchesFilter(filterBO.Filter));
				AssertEquals(false, unmatchedMessage.MatchesFilter(filterBO.Filter));
			});
		}

		public void TestG3InterchangeNumber()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "MAN001";
			var matchedMessage = header1.Messages.AddNew();
			matchedMessage.MessageNumberStrategy = new G3MessageNumberStrategyForTest("111");
			matchedMessage.EM_MessageType = "G3D";
			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_InterchangeNum = "123456";
			matchedMessage.EM_EI = interchange1.PK;

			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "MAN002";
			var unmatchedMessage = header2.Messages.AddNew();
			unmatchedMessage.MessageNumberStrategy = new G3MessageNumberStrategyForTest("222");
			unmatchedMessage.EM_MessageType = "G3D";
			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_InterchangeNum = "654321";
			unmatchedMessage.EM_EI = interchange2.PK;

			Factory.Save();

			var filter = (ModuleTextFilter)filterBO["G3 Interchange Number"];
			filter.IsActive = true;
			filter.Property = "123456";

			CombineAssertions("G3 Interchange Number filter", () =>
			{
				AssertEquals(true, matchedMessage.MatchesFilter(filterBO.Filter));
				AssertEquals(false, unmatchedMessage.MatchesFilter(filterBO.Filter));
			});
		}

		public void TestG3MessageNumber()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			var header1 = Factory.New<AsycudaManifestHeader>();
			var matchedMessage = header1.Messages.AddNew();
			matchedMessage.EM_MessageType = "G3D";
			matchedMessage.EM_MessageNum = "123456";

			var header2 = Factory.New<AsycudaManifestHeader>();
			var unmatchedMessage = header2.Messages.AddNew();
			unmatchedMessage.EM_MessageType = "G3D";
			unmatchedMessage.EM_MessageNum = "654321";

			var filter = (ModuleTextFilter)filterBO["G3 Message Number"];
			filter.IsActive = true;
			filter.Property = "123456";

			CombineAssertions("G3 Message Number filter", () =>
			{
				AssertEquals(true, matchedMessage.MatchesFilter(filterBO.Filter));
				AssertEquals(false, unmatchedMessage.MatchesFilter(filterBO.Filter));
			});
		}

		public void TestDeclarantFilter()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			var dummyOrg = Factory.New<OrgHeader>();
			dummyOrg.OH_Code = "DMY";

			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_OA_Declarant = dummyOrg.MainAddress.PK;
			var matchedMessage = header1.Messages.AddNew();
			matchedMessage.MessageNumberStrategy = new G3MessageNumberStrategyForTest("111");
			matchedMessage.EM_MessageType = "G3D";

			var header2 = Factory.New<AsycudaManifestHeader>();
			var unmatchedMessage = header2.Messages.AddNew();
			unmatchedMessage.MessageNumberStrategy = new G3MessageNumberStrategyForTest("222");
			unmatchedMessage.EM_MessageType = "G3D";

			Factory.Save();

			var filter = (ModuleGuidFilter)filterBO["Declarant"];
			filter.IsActive = true;
			filter.Property = dummyOrg.PK;

			CombineAssertions("Declarant filter", () =>
			{
				AssertEquals(true, matchedMessage.MatchesFilter(filterBO.Filter));
				AssertEquals(false, unmatchedMessage.MatchesFilter(filterBO.Filter));
			});
		}

		public void TestDeclarant_IsOrIsNotBlank()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBO["Declarant"];
			filter.IsActive = true;

			CombineAssertions("query text when is/is not blank filter is attached", () =>
			{
				filter.ComparisonOperator = "is blank";
				AssertContains("AMA_PK FROM dbo.AsycudaManifestHeader WHERE AMA_OA_Declarant is NULL", filterBO.Filter.LiteralTextSqlFormatted);

				filter.ComparisonOperator = "is not blank";
				AssertContains("AMA_PK FROM dbo.AsycudaManifestHeader WHERE AMA_OA_Declarant is not NULL", filterBO.Filter.LiteralTextSqlFormatted);
			});
		}

		public void TestPresenterFilter()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			var dummyOrg = Factory.New<OrgHeader>();
			dummyOrg.OH_Code = "DMY";

			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_OA_Presenter = dummyOrg.MainAddress.PK;
			var matchedMessage = header1.Messages.AddNew();
			matchedMessage.MessageNumberStrategy = new G3MessageNumberStrategyForTest("111");
			matchedMessage.EM_MessageType = "G3D";

			var header2 = Factory.New<AsycudaManifestHeader>();
			var unmatchedMessage = header2.Messages.AddNew();
			unmatchedMessage.MessageNumberStrategy = new G3MessageNumberStrategyForTest("222");
			unmatchedMessage.EM_MessageType = "G3D";

			var filter = (ModuleGuidFilter)filterBO["Presenter"];
			filter.IsActive = true;
			filter.Property = dummyOrg.PK;

			Factory.Save();

			CombineAssertions("Presenter filter", () =>
			{
				AssertEquals(true, matchedMessage.MatchesFilter(filterBO.Filter));
				AssertEquals(false, unmatchedMessage.MatchesFilter(filterBO.Filter));
			});	
		}

		public void TestPresenter_IsOrIsNotBlank()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBO["Presenter"];
			filter.IsActive = true;

			CombineAssertions("query text when is/is not blank filter is attached", () =>
			{
				filter.ComparisonOperator = "is blank";
				AssertContains("AMA_PK FROM dbo.AsycudaManifestHeader WHERE AMA_OA_Presenter is NULL", filterBO.Filter.LiteralTextSqlFormatted);

				filter.ComparisonOperator = "is not blank";
				AssertContains("AMA_PK FROM dbo.AsycudaManifestHeader WHERE AMA_OA_Presenter is not NULL", filterBO.Filter.LiteralTextSqlFormatted);
			});
		}

		public void TestDSDTFlightNoFilter()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_MasterInformation = "MA0001";
			var matchedMessage = header1.Messages.AddNew();
			matchedMessage.MessageNumberStrategy = new G3MessageNumberStrategyForTest("111");
			matchedMessage.EM_MessageType = "G3D";

			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_MasterInformation = "MA0002";
			var unmatchedMessage = header2.Messages.AddNew();
			unmatchedMessage.MessageNumberStrategy = new G3MessageNumberStrategyForTest("222");
			unmatchedMessage.EM_MessageType = "G3D";

			var filter = (ModuleTextFilter)filterBO["DSDT/Flight No."];
			filter.IsActive = true;
			filter.Property = "MA0001";

			Factory.Save();

			CombineAssertions("DSDT/ Flight No. filter", () =>
			{
				AssertEquals(true, matchedMessage.MatchesFilter(filterBO.Filter));
				AssertEquals(false, unmatchedMessage.MatchesFilter(filterBO.Filter));
			});
		}

		public void TestG3MessageStatusFilter()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			var header1 = Factory.New<AsycudaManifestHeader>();
			var matchedMessage = header1.Messages.AddNew();
			matchedMessage.EM_MessageType = "G3D";
			matchedMessage.EM_Status = "QUE";

			var header2 = Factory.New<AsycudaManifestHeader>();
			var unmatchedMessage = header2.Messages.AddNew();
			unmatchedMessage.EM_MessageType = "G3D";
			unmatchedMessage.EM_Status = "RCV";

			var filter = (ModuleTextFilter)filterBO["G3 Message Status"];
			filter.IsActive = true;
			filter.Property = "QUE";

			CombineAssertions("G3 Message Status filter", () =>
			{
				AssertEquals(true, matchedMessage.MatchesFilter(filterBO.Filter));
				AssertEquals(false, unmatchedMessage.MatchesFilter(filterBO.Filter));
			});
		}

		public void TestG3DirectionFilter()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			var header1 = Factory.New<AsycudaManifestHeader>();
			var matchedMessage = header1.Messages.AddNew();
			matchedMessage.EM_MessageType = "G3D";
			matchedMessage.EM_ReceiveTransmit = "RCV";

			var header2 = Factory.New<AsycudaManifestHeader>();
			var unmatchedMessage = header2.Messages.AddNew();
			unmatchedMessage.EM_MessageType = "G3D";
			unmatchedMessage.EM_ReceiveTransmit = "TRX";

			var filter = (ModuleTextFilter)filterBO["G3 Direction"];
			filter.IsActive = true;
			filter.Property = "RCV";

			CombineAssertions("G3 Direction filter", () =>
			{
				AssertEquals(true, matchedMessage.MatchesFilter(filterBO.Filter));
				AssertEquals(false, unmatchedMessage.MatchesFilter(filterBO.Filter));
			});
		}

		public void TestG3MessageTypeFilter()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			var header1 = Factory.New<AsycudaManifestHeader>();
			var matchedMessage = header1.Messages.AddNew();
			matchedMessage.EM_MessageType = "G3D";

			var header2 = Factory.New<AsycudaManifestHeader>();
			var unmatchedMessage = header2.Messages.AddNew();
			unmatchedMessage.EM_MessageType = "G3R";

			var filter = (ModuleTextFilter)filterBO["G3 Message Type"];
			filter.IsActive = true;
			filter.Property = "G3D";

			CombineAssertions("G3 Message Type filter", () =>
			{
				AssertEquals(true, matchedMessage.MatchesFilter(filterBO.Filter));
				AssertEquals(false, unmatchedMessage.MatchesFilter(filterBO.Filter));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new G3DeclarationFilterBusinessObject();

		class G3MessageNumberStrategyForTest : IMessageNumberStrategy
		{
			public G3MessageNumberStrategyForTest(ZString messageNumber)
			{
				this.messageNumber = messageNumber;
			}

			readonly ZString messageNumber;

			public string GetMessageReferenceNumber() => messageNumber;
		}
	}
}
