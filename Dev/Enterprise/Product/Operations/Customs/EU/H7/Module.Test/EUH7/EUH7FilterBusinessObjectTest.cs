using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Module.Testing
{
	[TestedType(typeof(EUH7FilterBusinessObject))]
	class EUH7FilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestJobNumberFilter()
		{
			var consignment = Factory.New<AsycudaManifestHeader>();
			consignment.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.EuH7;
			consignment.AMA_JobReference = "MAN001";

			var consignmentNotMatched = Factory.New<AsycudaManifestHeader>();
			consignmentNotMatched.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.EuH7;
			consignmentNotMatched.AMA_JobReference = "MAN002";

			var filter = (ModuleTextFilter)filterBO[EUH7FilterBusinessObject.Descriptions.JobNumber];
			filter.IsActive = true;
			filter.Property = "MAN001";

			Assert(consignment.MatchesFilter(filterBO.Filter));
			Assert(!consignmentNotMatched.MatchesFilter(filterBO.Filter));
		}

		public void TestMRNFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			var bill1 = asyheader1.Bills.AddNew();
			var entry1 = Factory.New<CusEntryNumber>();
			entry1.CE_ParentID = bill1.PK;
			entry1.CE_ParentTable = bill1.TableName;
			entry1.CE_EntryType = "MRN";
			entry1.CE_EntryNum = "MRN1";

			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			var bill2 = asyheader2.Bills.AddNew();
			var entry2 = Factory.New<CusEntryNumber>();
			entry2.CE_ParentID = bill2.PK;
			entry2.CE_ParentTable = bill2.TableName;
			entry2.CE_EntryType = "MRN";
			entry2.CE_EntryNum = "NOTMATCH";

			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[EUH7FilterBusinessObject.Descriptions.MRN];
			filter.IsActive = true;
			filter.Property = "M";

			Assert(asyheader1.MatchesFilter(filterBO.Filter));
			Assert(!asyheader2.MatchesFilter(filterBO.Filter));

			filter.Property = "NO";

			Assert(!asyheader1.MatchesFilter(filterBO.Filter));
			Assert(asyheader2.MatchesFilter(filterBO.Filter));
		}

		public void TestFilters()
		{
			AssertNotNull(filterBO[EUH7FilterBusinessObject.Descriptions.JobNumber]);
			AssertNotNull(filterBO[EUH7FilterBusinessObject.Descriptions.MRN]);
			AssertNotNull(filterBO[EUH7FilterBusinessObject.Descriptions.HasOutboundMessage]);
			AssertNotNull(filterBO[EUH7FilterBusinessObject.Descriptions.HasInboundMessage]);
		}

		public void TestHasOutboundMessageFilter()
		{
			AssertOutboundInboundMessageFilters(true);
		}

		public void TestHasInboundMessageFilter()
		{
			AssertOutboundInboundMessageFilters(false);
		}

		void AssertOutboundInboundMessageFilters(bool outBound)
		{
			var direction = outBound ? EDIInterchange.Direction.Transmit : EDIInterchange.Direction.Receive;
			var filterName = outBound
				? EUH7FilterBusinessObject.Descriptions.HasOutboundMessage
				: EUH7FilterBusinessObject.Descriptions.HasInboundMessage;

			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_BillNumber = "123";
			var bill2 = asyheader1.Bills.AddNew();
			bill2.ABL_BillNumber = "456";
			var outBoundMessage = Factory.NewWithValidTestData<DummyEDIMessage>();
			outBoundMessage.EM_ReceiveTransmit = direction;
			outBoundMessage.EM_LinkedObject = bill1;

			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var bill3 = asyheader1.Bills.AddNew();
			bill1.ABL_BillNumber = "321";
			var bill4 = asyheader1.Bills.AddNew();
			bill2.ABL_BillNumber = "654";
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.Property = EUH7FilterBusinessObject.HasMessageFilterItems.Yes;
			filter.IsActive = true;

			Assert(asyheader1.MatchesFilter(filterObj.Filter));
			Assert(!asyheader2.MatchesFilter(filterObj.Filter));

			filter.Property = EUH7FilterBusinessObject.HasMessageFilterItems.No;
			Assert(!asyheader1.MatchesFilter(filterObj.Filter));
			Assert(asyheader2.MatchesFilter(filterObj.Filter));

			filter.Property = "NOTVALID";
			Assert(!asyheader1.MatchesFilter(filterObj.Filter));
			Assert(!asyheader2.MatchesFilter(filterObj.Filter));
		}

		#region Implementations

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EUH7FilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = new EUH7FilterBusinessObject();
		}
		EUH7FilterBusinessObject filterBO;

		#endregion

		class DummyEDIMessage : EDIMessage
		{
			public DummyEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber()
			{
				return string.Empty;
			}
		}
	}
}
