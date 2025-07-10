using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.YAS.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.YAS.Business.Testing
{
	public class YASForwardingShipmentTest : TestCaseWithFactory
	{
		public void TestExceptionNotThrown()
		{
			AssertNoExceptionThrown(() =>
			{
				YASForwardingShipmentForTest shipment = Factory.NewWithValidTestData<YASForwardingShipmentForTest>();
				shipment.ConsigneeDocumentaryAddressChanged();
			});
		}

		public void TestExceptionNotThrownByNullConsignee()
		{
			AssertNoExceptionThrown(() =>
			{
				YASForwardingShipmentForTest shipment = Factory.NewWithValidTestData<YASForwardingShipmentForTest>();

				shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "UNMATCHED ORGANISATION";
				shipment.ConsigneeDocumentaryAddress_OrgAddressBeforeChange(shipment.ConsigneeDocumentaryAddress, new EventArgs());
				shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "NOT UNMATCHED ORGANISATION";
				shipment.ConsigneeDocumentaryAddressChanged();
			});
		}

		public void TestExceptionNotThrownWhenNoUnmatchedOrgNote()
		{
			AssertNoExceptionThrown(() =>
			{
				YASForwardingShipmentForTest shipment = Factory.NewWithValidTestData<YASForwardingShipmentForTest>();

				var unmatchedOrg = Factory.NewWithValidTestData<OrgHeader>();
				unmatchedOrg.OH_FullName = "UNMATCHED ORGANISATION";
				var newOrg = Factory.NewWithValidTestData<OrgHeader>();
				newOrg.OH_FullName = "NOT UNMATCHED ORGANISATION";
				Factory.Save();

				shipment.ConsigneeDocumentaryAddress.OrganisationPK = unmatchedOrg.PK;
				shipment.ConsigneeDocumentaryAddress_OrgAddressBeforeChange(shipment.ConsigneeDocumentaryAddress, new EventArgs());
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = newOrg.PK;
				shipment.ConsigneeDocumentaryAddressChanged();
			});
		}

		public void TestOrgAddressBeforeChangeNotExceptionThrownByDetachedAddress()
		{
			AssertNoExceptionThrown(() =>
			{
				YASForwardingShipmentForTest shipment = Factory.NewWithValidTestData<YASForwardingShipmentForTest>();

				var detachedAddress = Factory.NewWithValidTestData<JobDocAddress>();
				detachedAddress.E2_RN_NKCountryCode = "XX";
				detachedAddress.Delete();

				shipment.ConsigneeDocumentaryAddress_OrgAddressBeforeChange(detachedAddress, new EventArgs());
			});
		}

		public void TestOrgAddressBeforeChangeNotExceptionThrownByDeletedAddress()
		{
			AssertNoExceptionThrown(() =>
			{
				YASForwardingShipmentForTest shipment = Factory.NewWithValidTestData<YASForwardingShipmentForTest>();

				var deleteAddress = Factory.NewWithValidTestData<JobDocAddress>();
				deleteAddress.E2_RN_NKCountryCode = "XX";
				Factory.Save();
				deleteAddress.Delete();

				shipment.ConsigneeDocumentaryAddress_OrgAddressBeforeChange(deleteAddress, new EventArgs());
			});
		}

		public void TestJS_BookingReference()
		{
			YASForwardingShipmentForTest shipment = Factory.NewWithValidTestData<YASForwardingShipmentForTest>();
			shipment.JS_BookingReference = ZString.Empty;
			StmNote[] notes = shipment.Notes.FindByDescription("Shipper's Reference");
			AssertEquals("No reference note for empty value", 0, notes.Length);

			shipment.JS_BookingReference = "aaaaaaaa";
			notes = shipment.Notes.FindByDescription("Shipper's Reference");
			AssertEquals(1, notes.Length);
			AssertEquals("aaaaaaaa", notes[0].ST_NoteDataAsText);

			shipment.JS_BookingReference = "vvvvvvvvv";
			notes = shipment.Notes.FindByDescription("Shipper's Reference");
			AssertEquals(1, notes.Length);
			AssertEquals("aaaaaaaa", notes[0].ST_NoteDataAsText);
		}

		public void TestConsigneeAddressChangeDetected()
		{
			YASForwardingShipmentForTest shipment = Factory.NewWithValidTestData<YASForwardingShipmentForTest>();
			AssertEquals("Precondition: Consignee address not changed", false, shipment.ConsigneeOrgAddressChanged);

			OrgHeader newConsignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneeNameOrPK = newConsignee.PK.ToString();
			AssertEquals("Precondition: Consignee address changed", true, shipment.ConsigneeOrgAddressChanged);
		}

		public void TestCodeMapConsigneeWithNotesInOldFormat()
		{
			YASForwardingShipment shipment = Factory.NewWithValidTestData<YASForwardingShipment>();
			shipment.ConsigneePK = YASExtentions.UnmatchedOrganisationPK;
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = YASExtentions.UnmatchedOrganisationPK;
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, NoteTextInOldFormat);

			Factory.Save();

			OrgHeader orgProxy = GlbCompany.CurrentCompany.OrgProxy;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			OrgPatternMatchOverride orgPattern = Factory.LoadTop1<OrgPatternMatchOverride>(TestHelper.QueryToGetOrgPattern("POWERHOUSE LOGISTICS PTY LTD", OrgForTesting.PK));
			AssertNull("Org pattern doesn't exist", orgPattern);

			shipment.ConsigneePK = OrgForTesting.PK;

			ZString expectedMessage = ZString.Format(TestHelper.ExpectedMessageShowedForAutoCodeMap, "Owner Code", "POWLOGSYD", OrgForTesting.OH_Code);
			AssertEquals("the last message showed", expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());

			Factory.Save();
			orgPattern = Factory.LoadTop1<OrgPatternMatchOverride>(TestHelper.QueryToGetOrgPattern("POWLOGSYD", OrgForTesting.PK));
			AssertNotNull(orgPattern);
		}

		public void TestCodeMapConsigneeWithNotesInOldFormatAndLongOwnerCode()
		{
			YASForwardingShipment shipment = Factory.NewWithValidTestData<YASForwardingShipment>();
			shipment.ConsigneePK = YASExtentions.UnmatchedOrganisationPK;
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = YASExtentions.UnmatchedOrganisationPK;
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, @"Organization matching failed to find one or more organizations during data import. Details of the unmatched organizations are shown below:

None:- 
NAME: PROPERT AUSTRALIA PTY.LTD
ADDRESS: UNIT 1, 16 ANELLA AVENUE, CASTLE HILL, 2154, NSW
EDICODE: PROPTYSYD
OWNERCODE: PROPTYSYD

Consignee:- 
NAME: POWERHOUSE LOGISTICS PTY LTD
ADDRESS: 292 COWARD STREET, MASCOT, 2020, NSW
EDICODE: POWLOGSYD
OWNERCODE: MITSUBISHI CHEMICAL LOGISTICS CORPORATION HEAD OFFICE");

			Factory.Save();

			OrgHeader orgProxy = GlbCompany.CurrentCompany.OrgProxy;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			OrgPatternMatchOverride orgPattern = Factory.LoadTop1<OrgPatternMatchOverride>(TestHelper.QueryToGetOrgPattern("POWERHOUSE LOGISTICS PTY LTD", OrgForTesting.PK));
			AssertNull("Org pattern doesn't exist", orgPattern);

			shipment.ConsigneePK = OrgForTesting.PK;

			ZString expectedMessage = ZString.Format("Information The organization {0}({1}) has more than 50 characters. It cannot be matched to this CargoWise One Code ({2}).",
				"Owner Code ", "MITSUBISHI CHEMICAL LOGISTICS CORPORATION HEAD OFFICE", OrgForTesting.OH_Code);
			AssertEquals("the last message showed", expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestHasArrived()
		{
			YASForwardingShipment shipment = Factory.NewWithValidTestData<YASForwardingShipment>();
			AssertEquals("Has not arrived", false, shipment.Transports.HasArrived());

			Transport transport = shipment.Transports.AddNew();
			transport.JW_ATA = ZDateTime.UtcNow;
			AssertEquals("Has not arrived", true, shipment.Transports.HasArrived());
		}

		public void TestHasInvoiceOrCostBeenPosted()
		{
			YASForwardingShipment shipment = Factory.NewWithValidTestData<YASForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";
			AssertEquals("Has a cost", false, shipment.HasInvoiceOrCostBeenPosted());

			AccTransactionHeader accHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeader.AH_TransactionType = TransactionTypes.Invoice;

			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_JobNum = shipment.JS_UniqueConsignRef;

			accHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader.AH_JH = shipment.ShipmentJobHeader.PK;
			accHeader.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			accHeader.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			Factory.Save();
			AssertEquals("Has no cost", true, shipment.HasInvoiceOrCostBeenPosted());
		}

		public void TestJobRefNum()
		{
			YASForwardingShipment shipment1 = Factory.NewWithValidTestData<YASForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00001000";
			AccTransactionHeader accHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeader.AH_TransactionType = TransactionTypes.Invoice;

			JobHeader jobheader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobheader1.JH_ParentID = shipment1.PK;
			jobheader1.JH_JobNum = shipment1.JS_UniqueConsignRef;

			accHeader.AH_JH = shipment1.ShipmentJobHeader.PK;
			accHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			Factory.Save();

			YASForwardingShipment shipment2 = Factory.NewWithValidTestData<YASForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S000010002";
			AccTransactionHeader accHeader2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeader2.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeader2.AH_TransactionType = TransactionTypes.Invoice;

			JobHeader jobheader2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobheader2.JH_ParentID = shipment2.PK;
			jobheader2.JH_JobNum = shipment2.JS_UniqueConsignRef;

			accHeader2.AH_JH = shipment2.ShipmentJobHeader.PK;
			accHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader2.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			Factory.Save();

			YASForwardingShipment shipment3 = Factory.NewWithValidTestData<YASForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "S000010001";
			AccTransactionHeader accHeader3 = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeader3.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeader3.AH_TransactionType = TransactionTypes.Invoice;
			accHeader3.AH_ConsolidatedInvoiceRef = shipment2.JS_UniqueConsignRef;
			accHeader3.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			Factory.Save();

			AssertEquals("HasInvoiceOrCostBeenPosted", true, shipment1.HasInvoiceOrCostBeenPosted());
			AssertEquals("HasInvoiceOrCostBeenPosted", true, shipment2.HasInvoiceOrCostBeenPosted());
			AssertEquals("HasInvoiceOrCostBeenPosted", false, shipment3.HasInvoiceOrCostBeenPosted());
		}

		OrgHeader OrgForTesting
		{
			get { return consignee ?? (consignee = TestHelper.FindOrCreateOrgHeader("ABCSYD")); }
		}
		OrgHeader consignee;

		ZString NoteTextInOldFormat
		{
			get
			{
				return @"Organization matching failed to find one or more organizations during data import. Details of the unmatched organizations are shown below:

None:- 
NAME: PROPERT AUSTRALIA PTY.LTD
ADDRESS: UNIT 1, 16 ANELLA AVENUE, CASTLE HILL, 2154, NSW
EDICODE: PROPTYSYD
OWNERCODE: PROPTYSYD

Consignee:- 
NAME: POWERHOUSE LOGISTICS PTY LTD
ADDRESS: 292 COWARD STREET, MASCOT, 2020, NSW
EDICODE: POWLOGSYD
OWNERCODE: POWLOGSYD
";
			}
		}
		class YASForwardingShipmentForTest : YASForwardingShipment
		{
			public YASForwardingShipmentForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				ConsigneeOrgAddressChanged = false;
			}

			internal override void ConsigneeDocumentaryAddress_OrgAddressBeforeChange(object sender, EventArgs e)
			{
				ConsigneeOrgAddressChanged = true;
				base.ConsigneeDocumentaryAddress_OrgAddressBeforeChange(sender, e);
			}

			public new void ConsigneeDocumentaryAddressChanged()
			{
				base.ConsigneeDocumentaryAddressChanged();
			}

			public bool ConsigneeOrgAddressChanged { get; set; }
		}

		YASTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new YASTestHelper(Factory)); }
		}
		YASTestHelper testHelper;
	}
}
