using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	public class BISIUploadWarningReporterTest : TestCaseWithFactory
	{
		public void TestGenerateReportContent()
		{
			UPEDataRegistry.Instance.BISIUploadWarningHWM = ThreeHoursAgo;
			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>();
			mawb.CM_GB = Env.CurrentBranchPK;

			var calloutAlreadyDownloaded = NewCalloutItem(
				"NOTTOREPORT", BillingTermsCodeDescriptionPairList.Codes.Prepaid,
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 7.2m, 0m, false, creditor.PK));
			calloutAlreadyDownloaded.BisiUploadDate = OneHourAgo;
			calloutAlreadyDownloaded.BisiDownloadDate = OneHourAgo;
			calloutAlreadyDownloaded.CS_JE_CustomsFormalEntry = uPEJobDeclaration.PK;
			calloutAlreadyDownloaded.CS_CM = mawb.PK;

			var calloutAlreadyReported = NewCalloutItem(
				"V0018885095", BillingTermsCodeDescriptionPairList.Codes.Prepaid,
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 26.1m, 0m, false, creditor.PK),
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DeclarationProcessingCharge, 24.2m, 0m, false, creditor.PK));
			calloutAlreadyReported.BisiUploadDate = FourHoursAgo;
			calloutAlreadyReported.CS_JE_CustomsFormalEntry = uPEJobDeclaration.PK;
			calloutAlreadyReported.CS_CM = mawb.PK;

			var calloutNotDownloaded1 = NewCalloutItem(
				"1R06V634GTN", BillingTermsCodeDescriptionPairList.Codes.FreightCollect,
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.AQISProcessingCharge, 43.2m, 0m, false, creditor.PK),
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 52.25m, 0m, false, creditor.PK));
			calloutNotDownloaded1.BisiUploadDate = TwoHoursAgo;
			calloutNotDownloaded1.CS_JE_CustomsFormalEntry = uPEJobDeclaration.PK;
			calloutNotDownloaded1.CS_CM = mawb.PK;

			var calloutNotDownloaded2 = NewCalloutItem(
				"27387AGLXNX", BillingTermsCodeDescriptionPairList.Codes.Prepaid,
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.Woodlevy, 53.2m, 0m, false, creditor.PK),
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 12.25m, 0m, false, creditor.PK));
			calloutNotDownloaded2.BisiUploadDate = OneHourAgo;
			calloutNotDownloaded2.CS_JE_CustomsFormalEntry = uPEJobDeclaration.PK;
			calloutNotDownloaded2.CS_CM = mawb.PK;

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();
			UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber = 929423;
			new BISIShipmentDataAccessor(Factory).UpdateUploadData(calloutAlreadyDownloaded, calloutAlreadyReported, calloutNotDownloaded1, calloutNotDownloaded2);

			var expected =
								@"The following Shipment Numbers have not been received back from EBS within 60 Minutes:

Shipment #           Batch #   Cargo    Freight            Charge      Amount $   Total Charge $
                               Report                      Code
1R06V634GTN          929422    NOT      Freight Collect    201            52.25  
                                                           231            43.20            95.45

27387AGLXNX          929422    NOT      Prepaid            201            12.25  
                                                           216            53.20            65.45


The following Shipment Numbers have been previously notified but not yet responded to:

Shipment #           Batch #   Cargo    Freight            Charge      Amount $   Total Charge $
                               Report                      Code
V0018885095          929422    NOT      Prepaid            201            26.10  
                                                           231            24.20            50.30

--- End of Report ---
";
			var actual = Reporter.GenerateReportContent();
			AssertMultilineASCIIEquals("Output of generation", expected, actual);
		}

		public void TestGenerateReportContent_ExcludesExcludedShipments()
		{
			UPEDataRegistry.Instance.BISIUploadWarningHWM = ThreeHoursAgo;
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>();
			mawb.CM_GB = Env.CurrentBranchPK;

			var calloutNotDownloaded = NewCalloutItem("INCLUDED", BillingTermsCodeDescriptionPairList.Codes.FreightCollect);
			calloutNotDownloaded.BisiUploadDate = OneHourAgo;
			calloutNotDownloaded.CS_CM = mawb.PK;

			var calloutNotDownloadedButExcluded = NewCalloutItem("EXCLUDED", BillingTermsCodeDescriptionPairList.Codes.FreightCollect);
			calloutNotDownloadedButExcluded.BisiUploadDate = OneHourAgo;
			calloutNotDownloadedButExcluded.IsExcludedFromBISIWarning = true;
			calloutNotDownloadedButExcluded.CS_CM = mawb.PK;

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();
			UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber = 929423;
			new BISIShipmentDataAccessor(Factory).UpdateUploadData(calloutNotDownloaded, calloutNotDownloadedButExcluded);

			var expected =
								@"The following Shipment Numbers have not been received back from EBS within 60 Minutes:

Shipment #           Batch #   Cargo    Freight            Charge      Amount $   Total Charge $
                               Report                      Code
INCLUDED             929422    NOT      Freight Collect    -                  -             0.00

--- End of Report ---
";
			var actual = Reporter.GenerateReportContent();
			AssertMultilineASCIIEquals("Output of generation", expected, actual);
		}

		public void TestGenerateReportContent_ExcludesFreeDomicile()
		{
			UPEDataRegistry.Instance.BISIUploadWarningHWM = ThreeHoursAgo;
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>();
			mawb.CM_GB = Env.CurrentBranchPK;

			var calloutNotDownloaded = NewCalloutItem("INCLUDED", BillingTermsCodeDescriptionPairList.Codes.FreightCollect);
			calloutNotDownloaded.BisiUploadDate = OneHourAgo;
			calloutNotDownloaded.CS_CM = mawb.PK;

			var calloutNotDownloadedButExcluded = NewCalloutItem("EXCLUDED", BillingTermsCodeDescriptionPairList.Codes.FreightCollect);
			calloutNotDownloadedButExcluded.BisiUploadDate = OneHourAgo;
			calloutNotDownloadedButExcluded.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeDomicile;
			calloutNotDownloadedButExcluded.CS_CM = mawb.PK;

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();
			UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber = 929423;
			new BISIShipmentDataAccessor(Factory).UpdateUploadData(calloutNotDownloaded, calloutNotDownloadedButExcluded);

			var expected =
				@"The following Shipment Numbers have not been received back from EBS within 60 Minutes:

Shipment #           Batch #   Cargo    Freight            Charge      Amount $   Total Charge $
                               Report                      Code
INCLUDED             929422    NOT      Freight Collect    -                  -             0.00

--- End of Report ---
";
			var actual = Reporter.GenerateReportContent();
			AssertMultilineASCIIEquals("Output of generation", expected, actual);
		}

		public void TestGenerateReportContent_WithNoUnactionedItems()
		{
			UPEDataRegistry.Instance.BISIUploadWarningHWM = TwoHoursAgo;
			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>();
			mawb.CM_GB = Env.CurrentBranchPK;

			var calloutNotDownloaded = NewCalloutItem(
				"1R06V634GTN", BillingTermsCodeDescriptionPairList.Codes.FreightCollect,
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.AQISProcessingCharge, 43.2m, 0m, false, creditor.PK),
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 52.25m, 0m, false, creditor.PK));
			calloutNotDownloaded.BisiUploadDate = OneHourAgo;
			calloutNotDownloaded.CS_CM = mawb.PK;

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();
			UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber = 929423;
			new BISIShipmentDataAccessor(Factory).UpdateUploadData(calloutNotDownloaded);

			var expected =
								@"The following Shipment Numbers have not been received back from EBS within 60 Minutes:

Shipment #           Batch #   Cargo    Freight            Charge      Amount $   Total Charge $
                               Report                      Code
1R06V634GTN          929422    NOT      Freight Collect    201            52.25  
                                                           231            43.20            95.45

--- End of Report ---
";
			var actual = Reporter.GenerateReportContent();
			AssertMultilineASCIIEquals("Output of generation", expected, actual);
		}

		public void TestGenerateReportContent_SecondRunTurnsPreviouslyReportedIntoUnactionedShipments()
		{
			TestGenerateReportContent_WithNoUnactionedItems();

			GlbStaff recipient = BISIUploadNotificationGroup.Staff.AddNew();
			recipient.GS_EmailAddress = "warning_recipient@client.com.au";
			recipient.GS_Code = "ZAC";
			Factory.Save();

			Reporter.SendWarningEmailIfRequired(new NotificationBuffer()); // should change the HWM

			string expected =
								@"The following Shipment Numbers have been previously notified but not yet responded to:

Shipment #           Batch #   Cargo    Freight            Charge      Amount $   Total Charge $
                               Report                      Code
1R06V634GTN          929422    NOT      Freight Collect    201            52.25  
                                                           231            43.20            95.45

--- End of Report ---
";

			string actual = Reporter.GenerateReportContent();
			AssertMultilineASCIIEquals("Output of second report", expected, actual);
		}

		public void TestGenerateReportContent_WithAllUnactionedItems()
		{
			UPEDataRegistry.Instance.BISIUploadWarningHWM = OneHourAgo;
			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>();
			mawb.CM_GB = Env.CurrentBranchPK;

			var calloutNotDownloaded = NewCalloutItem(
				"1R06V634GTN", BillingTermsCodeDescriptionPairList.Codes.FreightCollect,
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.AQISProcessingCharge, 43.2m, 0m, false, creditor.PK),
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 52.25m, 0m, false, creditor.PK));
			calloutNotDownloaded.BisiUploadDate = TwoHoursAgo;
			calloutNotDownloaded.CS_CM = mawb.PK;

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();
			UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber = 929423;
			new BISIShipmentDataAccessor(Factory).UpdateUploadData(calloutNotDownloaded);

			string expected =
								@"The following Shipment Numbers have been previously notified but not yet responded to:

Shipment #           Batch #   Cargo    Freight            Charge      Amount $   Total Charge $
                               Report                      Code
1R06V634GTN          929422    NOT      Freight Collect    201            52.25  
                                                           231            43.20            95.45

--- End of Report ---
";
			string actual = Reporter.GenerateReportContent();
			AssertMultilineASCIIEquals("Output of generation", expected, actual);
		}

		public void TestGenerateReportContent_WithNoCharges()
		{
			UPEDataRegistry.Instance.BISIUploadWarningHWM = FourHoursAgo.AddHours(-1);
			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>();
			mawb.CM_GB = Env.CurrentBranchPK;

			var calloutNotDownloaded1 = NewCalloutItem(
				"WithChgs1", BillingTermsCodeDescriptionPairList.Codes.FreightCollect,
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.AQISProcessingCharge, 43.2m, 0m, false, creditor.PK),
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 52.25m, 0m, false, creditor.PK));
			calloutNotDownloaded1.BisiUploadDate = FourHoursAgo;
			calloutNotDownloaded1.CS_CM = mawb.PK;

			var calloutNotDownloaded2 = NewCalloutItem("NoCharges1", BillingTermsCodeDescriptionPairList.Codes.FreightCollect);
			calloutNotDownloaded2.BisiUploadDate = ThreeHoursAgo;
			calloutNotDownloaded2.CS_CM = mawb.PK;

			var calloutNotDownloaded3 = NewCalloutItem("NoCharges2", BillingTermsCodeDescriptionPairList.Codes.FreightCollect);
			calloutNotDownloaded3.BisiUploadDate = TwoHoursAgo;
			calloutNotDownloaded3.CS_CM = mawb.PK;

			var calloutNotDownloaded4 = NewCalloutItem(
				"WithChgs2", BillingTermsCodeDescriptionPairList.Codes.FreightCollect,
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.AQISProcessingCharge, 43.2m, 0m, false, creditor.PK),
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 52.25m, 0m, false, creditor.PK));
			calloutNotDownloaded4.BisiUploadDate = OneHourAgo;
			calloutNotDownloaded4.CS_CM = mawb.PK;

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();
			UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber = 929423;
			new BISIShipmentDataAccessor(Factory).UpdateUploadData(calloutNotDownloaded1, calloutNotDownloaded2, calloutNotDownloaded3, calloutNotDownloaded4);

			var expected =
@"The following Shipment Numbers have not been received back from EBS within 60 Minutes:

Shipment #           Batch #   Cargo    Freight            Charge      Amount $   Total Charge $
                               Report                      Code
WithChgs1            929422    NOT      Freight Collect    201            52.25  
                                                           231            43.20            95.45

NoCharges1           929422    NOT      Freight Collect    -                  -             0.00
NoCharges2           929422    NOT      Freight Collect    -                  -             0.00

WithChgs2            929422    NOT      Freight Collect    201            52.25  
                                                           231            43.20            95.45

--- End of Report ---
";
			var actual = Reporter.GenerateReportContent();
			AssertMultilineASCIIEquals("Output of generation", expected, actual);
		}

		public void TestGenerateReportContent_WithNoShipmentsToWarnAbout()
		{
			UPEDataRegistry.Instance.BISIUploadWarningHWM = ZDateTime.Now;
			string actual = Reporter.GenerateReportContent();
			AssertNull("Output of generation should be null", actual);
		}

		public void TestSendWarningEmailIfRequired()
		{
			UPEDataRegistry.Instance.BISIUploadWarningHWM = ThreeHoursAgo;
			var recipient = BISIUploadNotificationGroup.Staff.AddNew();
			recipient.GS_Code = "ZAC";
			recipient.GS_EmailAddress = "warning_recipient@client.com.au";
			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var mawb = Factory.NewWithValidTestData<UPECusMAWB>();
			mawb.CM_GB = Env.CurrentBranchPK;

			var calloutNotDownloaded = NewCalloutItem(
				"1R06V634GTN", BillingTermsCodeDescriptionPairList.Codes.FreightCollect,
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.AQISProcessingCharge, 43.2m, 0m, false, creditor.PK),
				new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 52.25m, 0m, false, creditor.PK));
			calloutNotDownloaded.BisiUploadDate = OneHourAgo;
			calloutNotDownloaded.CS_CM = mawb.PK;

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.Save();
			UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber = 123;
			new BISIShipmentDataAccessor(Factory).UpdateUploadData(calloutNotDownloaded);

			var notifications = new NotificationBuffer();
			Reporter.SendWarningEmailIfRequired(new ZDateTime(2005, 1, 2, 3, 4, 0), notifications);
			AssertEquals("Preparing Warning Report\r\nWarning Report Sent", notifications.AsString.Trim());

			var emailSent = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Subject", "BISI Upload Warning", emailSent.Subject);
			AssertEquals("Recipient", "warning_recipient@client.com.au", emailSent.Recipients[0]);
			AssertEquals("Body", true, emailSent.Body.IndexOf("have not been received back from EBS") != -1);

			var attachmentData = Encoding.UTF8.GetString(emailSent.Attachments[0].Data);
			AssertEquals("Attachment Data", true, attachmentData.IndexOf("have not been received back from EBS") != -1);
			AssertEquals("Attachment Filename", "BISIUploadWarning200501020304.txt", emailSent.Attachments[0].DisplayName);
		}

		public void TestSendWarningEmailIfRequired_WithNoShipmentsToWarnAbout()
		{
			UPEDataRegistry.Instance.BISIUploadWarningHWM = ZDateTime.Now;

			NotificationBuffer notifications = new NotificationBuffer();
			Reporter.SendWarningEmailIfRequired(notifications);
			AssertEquals("Preparing Warning Report\r\nNo outstanding shipments found", notifications.AsString.Trim());

			AssertEquals("No email should be sent if there are no shipments to warn about", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#region Test Classes

		class TestBISIUploadWarningReporter : BISIUploadWarningReporter
		{
			public new void SendWarningEmailIfRequired(ZDateTime now, INotifications notifications)
			{
				base.SendWarningEmailIfRequired(now, notifications);
			}

			public new string GenerateReportContent()
			{
				return base.GenerateReportContent();
			}
		}

		#endregion

		#region Implementation

		TestBISIUploadWarningReporter Reporter;
		ZDateTime OneHourAgo;
		ZDateTime TwoHoursAgo;
		ZDateTime ThreeHoursAgo;
		ZDateTime FourHoursAgo;

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUp();
			Reporter = new TestBISIUploadWarningReporter();

			OneHourAgo = ZDateTime.Now.AddHours(-1);
			TwoHoursAgo = ZDateTime.Now.AddHours(-2);
			ThreeHoursAgo = ZDateTime.Now.AddHours(-3);
			FourHoursAgo = ZDateTime.Now.AddHours(-4);
		}

		Callout NewCalloutItem(ZString shortHAWB, ZString billingTerms, params CustomsCharge[] customsCharges)
		{
			CusHAWBWithDummyDeclarationChargesForTest result = Factory.NewWithValidTestData<CusHAWBWithDummyDeclarationChargesForTest>();
			string longHAWBForDecoy = shortHAWB + shortHAWB.Substring(7);
			result.CS_HAWB = longHAWBForDecoy;
			result.WayBillShort = shortHAWB;

			result.BillingTerms = billingTerms;
			result.SetCustomsCharges(customsCharges);

			result.Declaration.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			return result;
		}

		GlbGroup BISIUploadNotificationGroup
		{
			get
			{
				GlbGroup result = Factory.Load<GlbGroup>(UPEDataRegistry.Instance.WarningReportNotificationGroup);
				if (result == null)
				{
					result = Factory.NewWithValidTestData<GlbGroup>();
					result.GG_Code = "UPW";
					UPEDataRegistry.Instance.WarningReportNotificationGroup = result.PK.ToGuid();
				}
				return result;
			}
		}

		#endregion
	}
}
