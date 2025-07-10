using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	class BISIShipmentsUploadedReporterTest : TestCaseWithFactory
	{
		[TestDate(2005, 7, 21, 3, 44, 0)]
		public void TestSendEmailIfRequired()
		{
			EmailNotificationGroup.Factory.Save();

			ShipmentDataForTest shipmentData1 = CreateShipmentDataForTest("SHIPMENT001", "AUSYD", BillingTermsCodeDescriptionPairList.Codes.Prepaid, "TRS");
			shipmentData1.ChargesData = new ShipmentChargeData[]
			{
				new ShipmentChargeData(ShipmentChargeTypeCode.GST, 1362.17m, Core.Constants.CurrencyCodes.Australia),
				new ShipmentChargeData(ShipmentChargeTypeCode.Security, 9.95m, Core.Constants.CurrencyCodes.Australia),
				new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 10.3m, Core.Constants.CurrencyCodes.Australia),
			};

			ShipmentDataForTest shipmentData2 = CreateShipmentDataForTest("SHIPMENT002", "AUSYD", BillingTermsCodeDescriptionPairList.Codes.FreeOnBoard, "CLR");
			shipmentData2.ChargesData = new ShipmentChargeData[]
			{
				new ShipmentChargeData(ShipmentChargeTypeCode.GST, 1362.17m, Core.Constants.CurrencyCodes.Australia),
				new ShipmentChargeData(ShipmentChargeTypeCode.Security, 9.95m, Core.Constants.CurrencyCodes.Australia),
			};

			ShipmentDataForTest shipmentData3 = CreateShipmentDataForTest("SHIPMENT003", "AUSYD", BillingTermsCodeDescriptionPairList.Codes.FreeOnBoard, "CLR");
			shipmentData3.ChargesData = new ShipmentChargeData[]
			{
				new ShipmentChargeData(ShipmentChargeTypeCode.GST, 1362.17m, Core.Constants.CurrencyCodes.Australia),
			};

			ShipmentDataForTest shipmentData4 = CreateShipmentDataForTest("SHIPMENT004", "AUSYD", BillingTermsCodeDescriptionPairList.Codes.FreeDomicile, "CLR");
			shipmentData4.ChargesData = System.Array.Empty<ShipmentChargeData>();

			ShipmentDataForTest shipmentData5 = CreateShipmentDataForTest("SHIPMENT005", "AUSYD", BillingTermsCodeDescriptionPairList.Codes.FreeOnBoard, "CLR");
			shipmentData5.ChargesData = new ShipmentChargeData[]
			{
				new ShipmentChargeData(ShipmentChargeTypeCode.GST, 1362.17m, Core.Constants.CurrencyCodes.Australia),
				new ShipmentChargeData(ShipmentChargeTypeCode.Security, 9.95m, Core.Constants.CurrencyCodes.Australia),
			};

			BISIShipmentsUploadedReporter reporter = new BISIShipmentsUploadedReporter(new IShipmentData[] { shipmentData1, shipmentData2, shipmentData3, shipmentData4, shipmentData5 }, 5);
			reporter.SendEmailIfRequired(Notifications);

			AssertMultilineASCIIEquals("Notifications for batch processor log", @"
Preparing Interchange Report
Interchange Report Sent".Trim(), Notifications.AsString);

			AssertSentEmail("Interchange Report 5",
@"Interchange Report

Interchange #  : 5
Uploaded       : 21-Jul-05 03:44
Total Shipments: 5
Total Charges  : 5488.83

Destination Port AUSYD
----------------------------------------------------------------------------------------
Shipment #    Destination  Status    Billing         Charge     Amount $  Total Charge $
                                      Terms           Code
----------------------------------------------------------------------------------------
SHIPMENT001   AUSYD        TRS       Prepaid         206         1362.17                
                                                     348            9.95                
                                                     201           10.30         1382.42

SHIPMENT002   AUSYD        CLR       Free On Board   206         1362.17                
                                                     348            9.95         1372.12

SHIPMENT003   AUSYD        CLR       Free On Board   206         1362.17         1362.17
SHIPMENT004   AUSYD        CLR       Free Domicile                                  0.00

SHIPMENT005   AUSYD        CLR       Free On Board   206         1362.17                
                                                     348            9.95         1372.12
----------------------------------------------------------------------------------------
Total for Destination AUSYD: 5                                 Sub-Total         5488.83
----------------------------------------------------------------------------------------

--- End of Report ---
");
		}

		[TestDate(2005, 7, 21, 3, 44, 0)]
		public void TestSendEmailIfRequired_GroupByPort()
		{
			EmailNotificationGroup.Factory.Save();

			ShipmentDataForTest shipmentData1 = CreateShipmentDataForTest("SHIPMENT001", "AUSYD", BillingTermsCodeDescriptionPairList.Codes.Prepaid, "TRS");
			shipmentData1.ChargesData = new ShipmentChargeData[]
			{
				new ShipmentChargeData(ShipmentChargeTypeCode.GST, 1362.1611m, Core.Constants.CurrencyCodes.Australia),
				new ShipmentChargeData(ShipmentChargeTypeCode.Security, 9.95m, Core.Constants.CurrencyCodes.Australia),
				new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 10.3m, Core.Constants.CurrencyCodes.Australia),
			};

			ShipmentDataForTest shipmentData2 = CreateShipmentDataForTest("SHIPMENT002", "AUMEL", BillingTermsCodeDescriptionPairList.Codes.FreeOnBoard, "CLR");
			shipmentData2.ChargesData = new ShipmentChargeData[]
			{
				new ShipmentChargeData(ShipmentChargeTypeCode.GST, 1362.17m, Core.Constants.CurrencyCodes.Australia),
				new ShipmentChargeData(ShipmentChargeTypeCode.Security, 9.95m, Core.Constants.CurrencyCodes.Australia),
			};

			ShipmentDataForTest shipmentData3 = CreateShipmentDataForTest("SHIPMENT003", "AUMEL", BillingTermsCodeDescriptionPairList.Codes.FreeOnBoard, "CLR");
			shipmentData3.ChargesData = new ShipmentChargeData[]
			{
				new ShipmentChargeData(ShipmentChargeTypeCode.GST, 1362.17m, Core.Constants.CurrencyCodes.Australia),
			};

			BISIShipmentsUploadedReporter reporter = new BISIShipmentsUploadedReporter(new IShipmentData[] { shipmentData1, shipmentData2, shipmentData3 }, 5);
			reporter.SendEmailIfRequired(Notifications);

			AssertMultilineASCIIEquals("Notifications for batch processor log", @"
Preparing Interchange Report
Interchange Report Sent".Trim(), Notifications.AsString);

			AssertSentEmail("Interchange Report 5",
@"Interchange Report

Interchange #  : 5
Uploaded       : 21-Jul-05 03:44
Total Shipments: 3
Total Charges  : 4116.70

Destination Port AUSYD
----------------------------------------------------------------------------------------
Shipment #    Destination  Status    Billing         Charge     Amount $  Total Charge $
                                      Terms           Code
----------------------------------------------------------------------------------------
SHIPMENT001   AUSYD        TRS       Prepaid         206         1362.16                
                                                     348            9.95                
                                                     201           10.30         1382.41
----------------------------------------------------------------------------------------
Total for Destination AUSYD: 1                                 Sub-Total         1382.41
----------------------------------------------------------------------------------------

Destination Port AUMEL
----------------------------------------------------------------------------------------
Shipment #    Destination  Status    Billing         Charge     Amount $  Total Charge $
                                      Terms           Code
----------------------------------------------------------------------------------------
SHIPMENT002   AUMEL        CLR       Free On Board   206         1362.17                
                                                     348            9.95         1372.12

SHIPMENT003   AUMEL        CLR       Free On Board   206         1362.17         1362.17
----------------------------------------------------------------------------------------
Total for Destination AUMEL: 2                                 Sub-Total         2734.29
----------------------------------------------------------------------------------------

--- End of Report ---
");
		}

		public void TestSendEmailIfRequired_WithMissingRecipientGroup()
		{
			ShipmentDataForTest shipmentData = new ShipmentDataForTest();
			shipmentData.ShipmentRef = "SHIPMENT";
			shipmentData.DischargePort = "AUPER";
			shipmentData.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.Prepaid;
			shipmentData.CustomsStatus = "CLR";
			shipmentData.ChargesData = System.Array.Empty<ShipmentChargeData>();

			BISIShipmentsUploadedReporter reporter = new BISIShipmentsUploadedReporter(new IShipmentData[] { shipmentData }, 5);
			reporter.SendEmailIfRequired(new NotificationBuffer());
			AssertEquals("No emails should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSendEmailIfRequired_ThrowsEmailHasNoRecipientsException()
		{
			EmailNotificationGroup.Staff[0].GS_EmailAddress = ZString.Empty;
			EmailNotificationGroup.Factory.Save();

			ShipmentDataForTest shipmentData = new ShipmentDataForTest();
			shipmentData.ShipmentRef = "SHIPMENT";
			shipmentData.DischargePort = "AUPER";
			shipmentData.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.Prepaid;
			shipmentData.CustomsStatus = "CLR";
			shipmentData.ChargesData = System.Array.Empty<ShipmentChargeData>();

			try
			{
				BISIShipmentsUploadedReporter reporter = new BISIShipmentsUploadedReporter(new IShipmentData[] { shipmentData }, 5);
				reporter.SendEmailIfRequired(new NotificationBuffer());
				AssertEquals("No emails should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertNotNull(ErrorReporter.LastExceptionReported);
				AssertEquals(typeof(EmailHasNoRecipientsException), ErrorReporter.LastExceptionReported.GetType());
				AssertEquals("Exception when sending Interchange Report", ErrorReporter.LastMessageReported);
				AssertEquals("52A9E458-AB25-4408-863E-40F11867FACA", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		[TestDate(2005, 7, 21, 3, 44, 0)]
		public void TestSendEmailIfRequired_WithNoShipmentsToReport()
		{
			EmailNotificationGroup.Factory.Save();

			BISIShipmentsUploadedReporter reporter = new BISIShipmentsUploadedReporter(System.Array.Empty<IShipmentData>(), 5);
			reporter.SendEmailIfRequired(new NotificationBuffer());
			AssertSentEmail("Interchange Report 5",
@"Interchange Report

Interchange #  : 5
Uploaded       : 21-Jul-05 03:44
Total Shipments: 0

--- End of Report ---
");
		}

		#region Implementation

		GlbGroup EmailNotificationGroup
		{
			get
			{
				if (fEmailNotificationGroup == null)
				{
					fEmailNotificationGroup = Factory.New<GlbGroup>();
					fEmailNotificationGroup.GG_Code = "UPW";
					GlbStaff recipient = fEmailNotificationGroup.Staff.AddNew();
					recipient.GS_EmailAddress = "bob@edi.com.au";
					recipient.GS_Code = "ZAC";
					UPEDataRegistry.Instance.InterchangeReportNotificationGroup = fEmailNotificationGroup.PK.ToGuid();
				}
				return fEmailNotificationGroup;
			}
		}
		GlbGroup fEmailNotificationGroup;

		NotificationBuffer Notifications
		{
			get
			{
				if (fNotifications == null)
				{
					fNotifications = new NotificationBuffer();
				}
				return fNotifications;
			}
		}
		NotificationBuffer fNotifications;

		ShipmentDataForTest CreateShipmentDataForTest(ZString shipmentRef, ZString destinationPort, ZString billingTerms, ZString customsStatus)
		{
			ShipmentDataForTest result = new ShipmentDataForTest();
			result.ShipmentRef = shipmentRef;
			result.DischargePort = destinationPort;
			result.BillingTerms = billingTerms;
			result.CustomsStatus = customsStatus;
			return result;
		}

		void AssertSentEmail(ZString expectedSubject, ZString expectedReportContent)
		{
			EmailDef sentEmail = Env.OutgoingMailManager.EmailsCreated[Env.OutgoingMailManager.EmailsCreated.Count - 1];
			AssertEquals("Subject", expectedSubject, sentEmail.Subject);
			AssertEquals("Body", expectedReportContent, sentEmail.Body);
			AssertEquals("To", "bob@edi.com.au", sentEmail.Recipients[0]);

			string attachmentData = Encoding.UTF8.GetString(sentEmail.Attachments[0].Data);
			AssertEquals("Attachment Filename", "BISIShipmentsUploaded" + ZDateTime.Now.ToString("yyyyMMddHHmm") + ".txt", sentEmail.Attachments[0].DisplayName);
			AssertEquals("Attachment Data", expectedReportContent, attachmentData);
		}

		#endregion
	}
}


