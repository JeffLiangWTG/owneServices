using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ManualReleaseCancelBO))]
	sealed class RNSManualReleaseBOTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2011, 1, 1, 1, 1, 1)]
		public void TestManualReleaseCancelBO_Properties()
		{
			AssertManualReleaseCancelBO_Properties(Factory.New<CFSShipment>());
			AssertManualReleaseCancelBO_Properties(Factory.New<ForwardingShipment>());
		}

		void AssertManualReleaseCancelBO_Properties(Freight.Business.CommonShipment shipment)
		{
			var releaseBo = new ManualReleaseCancelBO(shipment.CustomsManualStatus, Factory);
			AssertEquals("Default Customs Manual Status Note", ZString.Empty, releaseBo.NoteText);
			AssertEquals("Default Customs Manual Status", ZString.Empty, releaseBo.NoteText);
			AssertEquals("Default Customs Manual Release Date", ZDateTime.Empty, releaseBo.ManualReleaseDate);
			AssertEquals("Default Customs Manual Release Reason", ZString.Empty, releaseBo.ManualReleaseReason);
			AssertEquals("Default Customs Manual SystemUser", ZString.Empty, releaseBo.ManualReleaseUser);
			AssertEquals("Default Customs Manual SystemDate", ZDateTime.Empty, releaseBo.ManualReleaseSystemDate);

			const string testNoteText = @"2015-12-30 15:26
TEST MANUAL RELEASE REASON
TESTLOGIN
2015-12-31 15:26";
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsManualStatus.Description, testNoteText);
			releaseBo = new ManualReleaseCancelBO(shipment.CustomsManualStatus, Factory);
			AssertEquals("Customs Manual Status Note", shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.CustomsManualStatus.Description).FirstOrDefault().ST_NoteText, releaseBo.NoteText);
			AssertEquals("Customs Manual Status", testNoteText, releaseBo.NoteText);
			AssertEquals("Customs Manual Release Date", new ZDateTime(2015, 12, 30, 15, 26, 0), releaseBo.ManualReleaseDate);
			AssertEquals("Customs Manual Release Reason", "TEST MANUAL RELEASE REASON", releaseBo.ManualReleaseReason);
			AssertEquals("Customs Manual SystemUser", "TESTLOGIN", releaseBo.ManualReleaseUser);
			AssertEquals("Customs Manual SystemDate", new ZDateTime(2015, 12, 31, 15, 26, 0), releaseBo.ManualReleaseSystemDate);
		}

		[TestDate(2015, 12, 31)]
		public void TestValidateReleaseDate()
		{
			var manualReleaseCancelBo = new ManualReleaseCancelBO(Factory.New<CFSShipment>().CustomsManualStatus, Factory);
			manualReleaseCancelBo.ManualReleaseDate = ZDateTime.Now;
			AssertNoNotifications(manualReleaseCancelBo.ManualReleaseDateInfo);
			manualReleaseCancelBo.ManualReleaseDate = ZDateTime.Invalid;
			AssertHasNotifications(manualReleaseCancelBo.ManualReleaseDateInfo);

			manualReleaseCancelBo = new ManualReleaseCancelBO(Factory.New<ForwardingShipment>().CustomsManualStatus, Factory);
			manualReleaseCancelBo.ManualReleaseDate = ZDateTime.Now;
			AssertNoNotifications(manualReleaseCancelBo.ManualReleaseDateInfo);
			manualReleaseCancelBo.ManualReleaseDate = ZDateTime.Invalid;
			AssertHasNotifications(manualReleaseCancelBo.ManualReleaseDateInfo);
		}

		public void TestValidateReleaseReason()
		{
			var manualReleaseCancelBo = new ManualReleaseCancelBO(Factory.New<CFSShipment>().CustomsManualStatus, Factory);
			manualReleaseCancelBo.ManualReleaseReason = "TESTRELEASEREASON";
			AssertNoNotifications(manualReleaseCancelBo.ManualReleaseReasonInfo);
			manualReleaseCancelBo.ManualReleaseReason = ZString.Empty;
			AssertHasNotifications(manualReleaseCancelBo.ManualReleaseReasonInfo);

			manualReleaseCancelBo = new ManualReleaseCancelBO(Factory.New<ForwardingShipment>().CustomsManualStatus, Factory);
			manualReleaseCancelBo.ManualReleaseReason = "TESTRELEASEREASON";
			AssertNoNotifications(manualReleaseCancelBo.ManualReleaseReasonInfo);
			manualReleaseCancelBo.ManualReleaseReason = ZString.Empty;
			AssertHasNotifications(manualReleaseCancelBo.ManualReleaseReasonInfo);
		}

		#region Implementation

		CFSShipment Shipment
		{
			get { return cfsShipment ?? (cfsShipment = Factory.New<GatePassShipment>()); }
		}
		CFSShipment cfsShipment;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ManualReleaseCancelBO(Shipment.CustomsManualStatus, Factory);
		}

		protected override void SetUp()
		{
			const string testNoteText = @"2015-12-30 15:26
TEST MANUAL RELEASE REASON
TESTLOGIN
2015-12-31 15:26";
			Shipment.CustomsManualStatus = testNoteText;
		}

		#endregion
	}
}
