using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class AP_ARInvoiceQueryServiceTest : TestCaseWithFactory
	{
		public void TestGetPostingDetailsForAP()
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			var chargeCode1 = CreateChargeCode("TEST1", "TEST1 Charge Code", GlbCompany.CurrentCompany.PK);

			var chargeCode2 = CreateChargeCode("TEST2", "TEST2 Charge Code", GlbCompany.CurrentCompany.PK);

			var chargeCode3 = CreateChargeCode("TEST3", "TEST3 Charge Code", GlbCompany.CurrentCompany.PK);
			chargeCode3.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Comment;

			var chargeCode4 = CreateChargeCode("TEST4", "TEST4 Charge Code", GlbCompany.CurrentCompany.PK);
			chargeCode4.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Revenue;

			var jobLinked = CreateJob(declaration.PK, JobDeclarationSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
			var customsCharge = AddCharge(jobLinked, chargeCode1.PK, "", 0m, false, 0m, false, false);
			var costPostedCharge = AddCharge(jobLinked, chargeCode2.PK, "2", 20m, true, 25m, false, false);
			var commentCharge = AddCharge(jobLinked, chargeCode3.PK, "3", 80m, false, 85m, false, false);
			var revenueCharge = AddCharge(jobLinked, chargeCode4.PK, "4", 80m, false, 85m, false, false);
			var unpostedInvalidCharge = AddCharge(jobLinked, chargeCode2.PK, "", 80m, false, 85m, false, false);
			var unpostedValidCharge = AddCharge(jobLinked, chargeCode2.PK, "5", 80m, false, 85m, false, false);

			var service = new AP_ARInvoiceQueryService();

			var result = service.GetPostingDetails((ICustomsJobInfo)declaration, new ZGuid[] { chargeCode1.PK }, true, false);
			AssertEquals("NumberOfUnpostedAPCharges", 3, result.NumberOfUnpostedAPCharges);
			AssertEquals("NumberOfUnpostedAPChargesReadyForPosting", 2, result.NumberOfUnpostedAPChargesReadyForPosting);

			result = service.GetPostingDetails((ICustomsJobInfo)declaration, new ZGuid[] { chargeCode1.PK }, false, false);
			AssertEquals("NumberOfUnpostedAPCharges", 2, result.NumberOfUnpostedAPCharges);
			AssertEquals("NumberOfUnpostedAPChargesReadyForPosting", 1, result.NumberOfUnpostedAPChargesReadyForPosting);
		}

		public void TestCS00126183_TwoCUSDISLinesOneToRefundToImporter()
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			var chargeCode1 = CreateChargeCode("TEST1", "TEST1 Charge Code", GlbCompany.CurrentCompany.PK);

			var chargeCode2 = CreateChargeCode("TEST2", "TEST2 Charge Code", GlbCompany.CurrentCompany.PK);

			var jobLinked = CreateJob(declaration.PK, JobDeclarationSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
			var costPostedCharge = AddCharge(jobLinked, chargeCode1.PK, "2", 20m, true, 25m, true, true);
			var refundChargeToImp = AddCharge(jobLinked, chargeCode2.PK, "", 0m, false, -5m, true, false);

			var service = new AP_ARInvoiceQueryService();

			var result = service.GetInvoiceAmount((ICustomsJobInfo)declaration, new List<ZGuid>(new ZGuid[] { chargeCode1.PK, chargeCode2.PK }));

			Assert("APFullyPaid", result.APFullyPaid);
			AssertEquals("APPostedAmount", 20m, result.APPostedAmount);
			AssertEquals("ARPostedAmount", 20m, result.ARPostedAmount);
		}

		public void TestGetPostingDetailsForAR()
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			var chargeCode1 = CreateChargeCode("TEST1", "TEST1 Charge Code", GlbCompany.CurrentCompany.PK);

			var chargeCode2 = CreateChargeCode("TEST2", "TEST2 Charge Code", GlbCompany.CurrentCompany.PK);

			var chargeCode3 = CreateChargeCode("TEST3", "TEST3 Charge Code", GlbCompany.CurrentCompany.PK);
			chargeCode3.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Overhead;

			var jobLinked = CreateJob(declaration.PK, JobDeclarationSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
			var revenuePostedCharge = AddCharge(jobLinked, chargeCode2.PK, "1", 20m, true, 25m, true, false);
			var commentCharge = AddCharge(jobLinked, chargeCode3.PK, "3", 80m, false, 85m, false, false);
			var overheadCharge = AddCharge(jobLinked, chargeCode3.PK, "4", 80m, false, 85m, false, false);
			var unpostedInvalidCharge = AddCharge(jobLinked, chargeCode2.PK, "", 80m, false, 85m, false, false);
			unpostedInvalidCharge.JR_OH_SellAccount = ZGuid.Empty;

			var unpostedValidCharge = AddCharge(jobLinked, chargeCode2.PK, "5", 80m, false, 85m, false, false);

			var service = new AP_ARInvoiceQueryService();

			var result = service.GetPostingDetails((ICustomsJobInfo)declaration, new ZGuid[] { chargeCode1.PK }, false, true);
			AssertEquals("NumberOfUnpostedARCharges", 2, result.NumberOfUnpostedARCharges);
			AssertEquals("NumberOfUnpostedARChargesReadyForPosting", 1, result.NumberOfUnpostedARChargesReadyForPosting);
		}

		public void TestGetPostingDetailsForNumberOfARInvoices()
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			var chargeCode1 = CreateChargeCode("TEST1", "TEST1 Charge Code", GlbCompany.CurrentCompany.PK);

			var chargeCode2 = CreateChargeCode("TEST2", "TEST2 Charge Code", GlbCompany.CurrentCompany.PK);

			var chargeCode3 = CreateChargeCode("TEST3", "TEST3 Charge Code", GlbCompany.CurrentCompany.PK);
			chargeCode3.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Overhead;

			var jobLinked = CreateJob(declaration.PK, JobDeclarationSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);

			var debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "DEB";
			var commentCharge = AddCharge(jobLinked, chargeCode2.PK, "3", 80m, false, 85m, false, false);
			commentCharge.JR_OH_SellAccount = debtor.PK;
			commentCharge.JR_InvoiceType = "FIN";

			var overheadCharge = AddCharge(jobLinked, chargeCode3.PK, "4", 80m, false, 85m, false, false);
			overheadCharge.JR_OH_SellAccount = debtor.PK;
			overheadCharge.JR_InvoiceType = "FIN";

			var unpostedValidCharge = AddCharge(jobLinked, chargeCode2.PK, "5", 80m, false, 85m, false, false);
			unpostedValidCharge.JR_InvoiceType = "FIN";

			var service = new AP_ARInvoiceQueryService();

			var result = service.GetPostingDetails((ICustomsJobInfo)declaration, new ZGuid[] { chargeCode1.PK }, false, true);
			AssertEquals("NumberOfARInvoicesToBeIssued", 2, result.NumberOfARInvoicesToBeIssued);

			unpostedValidCharge.JR_OH_SellAccount = debtor.PK;
			unpostedValidCharge.JR_InvoiceType = "FIN";
			result = service.GetPostingDetails((ICustomsJobInfo)declaration, new ZGuid[] { chargeCode1.PK }, false, true);
			AssertEquals("NumberOfARInvoicesToBeIssued", 1, result.NumberOfARInvoicesToBeIssued);
		}

		public void TestGetInvoiceAmountFromStandAloneDeclaration()
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			var chargeCode2 = CreateChargeCode("TEST2", "TEST2 Charge Code", GlbCompany.CurrentCompany.PK);

			var chargeCode3 = CreateChargeCode("TEST3", "TEST3 Charge Code", GlbCompany.CurrentCompany.PK);

			var chargeCode4 = CreateChargeCode("TEST4", "TEST4 Charge Code", GlbCompany.CurrentCompany.PK);

			var jobLinked = CreateJob(declaration.PK, JobDeclarationSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
			AddCharge(jobLinked, chargeCode2.PK, "2", 20m, true, 25m, false, false);
			AddCharge(jobLinked, chargeCode3.PK, "2", 40m, false, 45m, true, false);
			AddCharge(jobLinked, chargeCode2.PK, "3", 80m, true, 85m, false, false);
			AddCharge(jobLinked, chargeCode4.PK, "4", 320m, false, 325m, true, false);//will be ignored as charge code does not match

			var service = new AP_ARInvoiceQueryService();

			var result = service.GetInvoiceAmount((ICustomsJobInfo)declaration, new List<ZGuid>(new ZGuid[] { chargeCode2.PK, chargeCode3.PK }));

			AssertEquals("APPostedAmount", 20m + 80m, result.APPostedAmount);
			AssertEquals("APUnPostedAmount", 40m, result.APUnPostedAmount);
			AssertEquals("ARPostedAmount", 45m, result.ARPostedAmount);
			AssertEquals("ARUnPostedAmount", 25m + 85m, result.ARUnPostedAmount);

			AssertEquals("IsAPFullyPaid", false, result.APFullyPaid);
		}

		public void TestGetInvoiceAmountFromPluginDeclaration()
		{
			var shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var shipment2 = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declaration2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			declaration2[JobDeclarationSchema.Constants.JE_JS] = shipment2.PK;

			var otherCompany = Factory.New<GlbCompany>();
			var otherBranch = otherCompany.Branches.AddNew();

			var chargeCode1 = CreateChargeCode("TEST1", "TEST1 Charge Code", GlbCompany.CurrentCompany.PK);
			var chargeCode1_1 = CreateChargeCode("TEST1", "TEST1 Charge Code", otherCompany.PK);

			var chargeCode2 = CreateChargeCode("TEST2", "TEST2 Charge Code", GlbCompany.CurrentCompany.PK);

			var chargeCode3 = CreateChargeCode("TEST3", "TEST3 Charge Code", GlbCompany.CurrentCompany.PK);

			var chargeCode4 = CreateChargeCode("TEST4", "TEST4 Charge Code", GlbCompany.CurrentCompany.PK);

			var jobLinkedInOtherDec = CreateJob(shipment2.PK, JobDeclarationSchema.Constants.Prefix, otherBranch.PK, otherCompany.PK);
			AddCharge(jobLinkedInOtherDec, chargeCode1_1.PK, "1", 10m, true, 15m, false, false);

			var jobLinked = CreateJob(shipment.PK, JobDeclarationSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
			AddCharge(jobLinked, chargeCode2.PK, "2", 20m, true, 25m, false, false);
			AddCharge(jobLinked, chargeCode3.PK, "2", 40m, false, 45m, true, false);
			AddCharge(jobLinked, chargeCode2.PK, "3", 80m, true, 85m, false, false);
			AddCharge(jobLinked, chargeCode4.PK, "4", 320m, false, 325m, true, false);//will be ignored as charge code does not match

			var service = new AP_ARInvoiceQueryService();

			//even though a declaration is passed, system should look at shipment to get a JobHeader if exists
			var result = service.GetInvoiceAmount((ICustomsJobInfo)declaration, new List<ZGuid>(new ZGuid[] { chargeCode2.PK, chargeCode3.PK }));

			AssertEquals("APPostedAmount", 20m + 80m, result.APPostedAmount);
			AssertEquals("APUnPostedAmount", 40m, result.APUnPostedAmount);
			AssertEquals("ARPostedAmount", 45m, result.ARPostedAmount);
			AssertEquals("ARUnPostedAmount", 25m + 85m, result.ARUnPostedAmount);

			AssertEquals("IsAPFullyPaid", false, result.APFullyPaid);
		}

		public void TestGetTotalInvoicedDetails()
		{
			var shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;

			var declaration2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var chargeCode1 = CreateChargeCode("TEST1", "TEST1 Charge Code", GlbCompany.CurrentCompany.PK);
			var chargeCode2 = CreateChargeCode("TEST2", "TEST2 Charge Code", GlbCompany.CurrentCompany.PK);
			var chargeCode3 = CreateChargeCode("TEST3", "TEST3 Charge Code", GlbCompany.CurrentCompany.PK);
			var chargeCode4 = CreateChargeCode("TEST4", "TEST4 Charge Code", GlbCompany.CurrentCompany.PK);

			var jobLinkedToDeclaration2 = CreateJob(declaration2.PK, JobDeclarationSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
			var charge = AddCharge(jobLinkedToDeclaration2, chargeCode1.PK, "1", 10m, true, 15m, true, false, 12m);
			charge.ARLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			charge.ARLine.TransactionHeader.AH_JH = jobLinkedToDeclaration2.PK;

			var jobLinked = CreateJob(shipment.PK, JobShipmentSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
			var charge2 = AddCharge(jobLinked, chargeCode2.PK, "2", 20m, true, 25m, true, false);
			charge2.ARLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			charge2.ARLine.AL_JH = jobLinked.PK;
			charge2.ARLine.AL_AC = chargeCode2.PK;
			charge2.ARLine.AL_LineAmount = 20m;
			charge2.ARLine.AL_Desc = "charge2";
			var charge3 = AddCharge(jobLinked, chargeCode3.PK, "2", 40m, false, 45m, true, false);
			charge3.ARLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			charge3.ARLine.AL_JH = jobLinked.PK;
			charge3.ARLine.AL_AC = chargeCode3.PK;
			charge3.ARLine.AL_LineAmount = 40m;
			charge3.ARLine.AL_Desc = "charge3";
			var charge4 = AddCharge(jobLinked, chargeCode2.PK, "3", 20m, true, 85m, true, false, 90m);
			charge4.ARLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			charge4.ARLine.AL_JH = jobLinked.PK;
			charge4.ARLine.AL_AC = chargeCode2.PK;
			charge4.ARLine.AL_LineAmount = 30m;
			var charge5 = AddCharge(jobLinked, chargeCode4.PK, "4", 320m, false, 325m, true, false);

			var service = new AP_ARInvoiceQueryService();
			var result = service.GetTotalInvoicedDetails((ICustomsJobInfo)declaration, new List<ZGuid>(new ZGuid[] { chargeCode2.PK, chargeCode3.PK }));

			AssertEquals("TotalBilledAmount", 90m, result.TotalBilledAmount);
			AssertEquals("TotalInvoicedAmount", 90m, result.TotalInvoicedAmount);
			AssertEquals("TotalOutstandingAmount", 80m, result.TotalOutstandingAmount);

			result = service.GetTotalInvoicedDetails((ICustomsJobInfo)declaration, new List<ZGuid>(new ZGuid[] { chargeCode2.PK, chargeCode3.PK }), "charge3");

			AssertEquals("TotalBilledAmount", 40m, result.TotalBilledAmount);
			AssertEquals("TotalInvoicedAmount", 0m, result.TotalInvoicedAmount);

			service = new AP_ARInvoiceQueryService();
			result = service.GetTotalInvoicedDetails((ICustomsJobInfo)declaration2, new List<ZGuid>(new ZGuid[] { chargeCode1.PK, chargeCode2.PK, chargeCode3.PK, chargeCode4.PK }));

			AssertEquals("TotalBilledAmount", 0m, result.TotalBilledAmount);
			AssertEquals("TotalInvoicedAmount", 12m, result.TotalInvoicedAmount);
			AssertEquals("TotalOutstandingAmount", 10m, result.TotalOutstandingAmount);
		}

		public void TestAPFullyPaid()
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			var chargeCode2 = CreateChargeCode("TEST2", "TEST2 Charge Code", GlbCompany.CurrentCompany.PK);

			var chargeCode3 = CreateChargeCode("TEST3", "TEST3 Charge Code", GlbCompany.CurrentCompany.PK);

			var chargeCode4 = CreateChargeCode("TEST4", "TEST4 Charge Code", GlbCompany.CurrentCompany.PK);

			var jobLinked = CreateJob(declaration.PK, JobDeclarationSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
			AddCharge(jobLinked, chargeCode2.PK, "2", 20m, true, 25m, false, false);
			AddCharge(jobLinked, chargeCode3.PK, "2", 40m, true, 45m, true, false);
			AddCharge(jobLinked, chargeCode2.PK, "3", 80m, true, 85m, false, false);
			AddCharge(jobLinked, chargeCode4.PK, "4", 320m, true, 325m, true, false);//will be ignored as charge code does not match

			var service = new AP_ARInvoiceQueryService();

			var result = service.GetInvoiceAmount((ICustomsJobInfo)declaration, new List<ZGuid>(new ZGuid[] { chargeCode2.PK, chargeCode3.PK }));
			AssertEquals("IsAPFullyPaid", false, result.APFullyPaid);

			jobLinked.Charges[0].APLine.TransactionHeader.AH_OutstandingAmount = 0m;
			result = service.GetInvoiceAmount((ICustomsJobInfo)declaration, new List<ZGuid>(new ZGuid[] { chargeCode2.PK, chargeCode3.PK }));
			AssertEquals("IsAPFullyPaid", false, result.APFullyPaid);

			jobLinked.Charges[1].APLine.TransactionHeader.AH_OutstandingAmount = 0m;
			result = service.GetInvoiceAmount((ICustomsJobInfo)declaration, new List<ZGuid>(new ZGuid[] { chargeCode2.PK, chargeCode3.PK }));
			AssertEquals("IsAPFullyPaid", false, result.APFullyPaid);

			jobLinked.Charges[2].APLine.TransactionHeader.AH_OutstandingAmount = 0m;
			result = service.GetInvoiceAmount((ICustomsJobInfo)declaration, new List<ZGuid>(new ZGuid[] { chargeCode2.PK, chargeCode3.PK }));
			AssertEquals("IsAPFullyPaid", true, result.APFullyPaid);
		}

		public void TestGetTotalInvoicedDetailsDBHits_AccTransactionLines()
		{
			#region Create Declarations and Jobs

			var objectCreator = new TestObjectCreator(Factory);

			var shipment1 = objectCreator.CreateShipment("S00001", true);
			var declaration1 = objectCreator.CreateDeclaration("B00001");
			declaration1.JE_JS = shipment1.PK;
			var job1 = CreateJob(shipment1.PK, JobShipmentSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
			var charge = AddCharge(job1, objectCreator.CC1.PK, "1", 20m, true, 25m, true, false, 10);
			charge.ARLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			charge.ARLine.AL_JH = job1.PK;
			charge.ARLine.AL_AC = objectCreator.CC1.PK;
			charge.ARLine.AL_LineAmount = 20m;
			charge = AddCharge(job1, objectCreator.CC2.PK, "1", 40m, false, 45m, true, false, 30);
			charge.ARLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			charge.ARLine.AL_JH = job1.PK;
			charge.ARLine.AL_AC = objectCreator.CC2.PK;
			charge.ARLine.AL_LineAmount = 40m;

			var shipment2 = objectCreator.CreateShipment("S00002", true);
			var declaration2 = objectCreator.CreateDeclaration("B00002");
			declaration2.JE_JS = shipment2.PK;
			var job2 = CreateJob(shipment2.PK, JobShipmentSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
			charge = AddCharge(job2, objectCreator.CC1.PK, "2", 30m, true, 35m, true, false, 20);
			charge.ARLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			charge.ARLine.AL_JH = job2.PK;
			charge.ARLine.AL_AC = objectCreator.CC1.PK;
			charge.ARLine.AL_LineAmount = 30m;
			charge = AddCharge(job2, objectCreator.CC2.PK, "2", 50m, false, 55m, true, false, 40);
			charge.ARLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			charge.ARLine.AL_JH = job2.PK;
			charge.ARLine.AL_AC = objectCreator.CC2.PK;
			charge.ARLine.AL_LineAmount = 50m;

			#endregion

			var service = new AP_ARInvoiceQueryService();

			var dBHitsBefore = Factory.GetTableHitCount(AccTransactionLines.Schema.TableName);

			var result = service.GetTotalInvoicedDetails((ICustomsJobInfo)declaration1, new List<ZGuid>(new ZGuid[] { objectCreator.CC1.PK, objectCreator.CC2.PK }));
			AssertEquals(60m, result.TotalBilledAmount);
			AssertEquals(40m, result.TotalInvoicedAmount);
			AssertEquals(60m, result.TotalOutstandingAmount);

			var dBHitsAfterFirstResult = Factory.GetTableHitCount(AccTransactionLines.Schema.TableName);
			AssertEquals(dBHitsAfterFirstResult, dBHitsBefore + 1);

			result = service.GetTotalInvoicedDetails((ICustomsJobInfo)declaration2, new List<ZGuid>(new ZGuid[] { objectCreator.CC1.PK, objectCreator.CC2.PK }));
			AssertEquals(80m, result.TotalBilledAmount);
			AssertEquals(60m, result.TotalInvoicedAmount);
			AssertEquals(80m, result.TotalOutstandingAmount);

			var dBHitsAfterSecondResult = Factory.GetTableHitCount(AccTransactionLines.Schema.TableName);
			AssertEquals(dBHitsAfterSecondResult, dBHitsAfterFirstResult);
		}

		public void TestGetTotalInvoicedDetailsDBHits_AccTransactionHeader()
		{
			#region Create Declarations and Jobs

			var objectCreator = new TestObjectCreator(Factory);

			var declaration1 = objectCreator.CreateDeclaration("B00001");
			var job1 = CreateJob(declaration1.PK, JobDeclarationSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
			var charge = AddCharge(job1, objectCreator.CC1.PK, "1", 20m, true, 25m, true, false, 10m);
			charge.ARLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			charge.ARLine.TransactionHeader.AH_JH = job1.PK;

			var declaration2 = objectCreator.CreateDeclaration("B00002");
			var job2 = CreateJob(declaration2.PK, JobDeclarationSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
			charge = AddCharge(job2, objectCreator.CC2.PK, "2", 30m, true, 35m, true, false, 20m);
			charge.ARLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			charge.ARLine.TransactionHeader.AH_JH = job2.PK;

			#endregion

			var service = new AP_ARInvoiceQueryService();

			var dBHitsBefore = Factory.GetTableHitCount(AccTransactionHeader.Schema.TableName);

			var result = service.GetTotalInvoicedDetails((ICustomsJobInfo)declaration1, new List<ZGuid>(new ZGuid[] { objectCreator.CC1.PK, objectCreator.CC2.PK }));
			AssertEquals(0m, result.TotalBilledAmount);
			AssertEquals(10m, result.TotalInvoicedAmount);
			AssertEquals(20m, result.TotalOutstandingAmount);

			var dBHitsAfterFirstResult = Factory.GetTableHitCount(AccTransactionHeader.Schema.TableName);
			AssertEquals(dBHitsAfterFirstResult, dBHitsBefore + 1);

			result = service.GetTotalInvoicedDetails((ICustomsJobInfo)declaration2, new List<ZGuid>(new ZGuid[] { objectCreator.CC1.PK, objectCreator.CC2.PK }));
			AssertEquals(0m, result.TotalBilledAmount);
			AssertEquals(20m, result.TotalInvoicedAmount);
			AssertEquals(30m, result.TotalOutstandingAmount);

			var dBHitsAfterSecondResult = Factory.GetTableHitCount(AccTransactionHeader.Schema.TableName);
			AssertEquals(dBHitsAfterSecondResult, dBHitsAfterFirstResult);
		}

		public void TestGetTotalInvoicedDetailsForAccountsReceivable()
		{
			var shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			var jobLinkedToShipment = CreateJob(shipment.PK, JobShipmentSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
			var chargeCode1 = CreateChargeCode("TEST1", "TEST1 Charge Code", GlbCompany.CurrentCompany.PK);

			var charge1 = AddCharge(jobLinkedToShipment, chargeCode1.PK, "2", 20m, true, 25m, true, false, 30m);
			charge1.ARLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			charge1.ARLine.AL_JH = jobLinkedToShipment.PK;
			charge1.ARLine.AL_AC = chargeCode1.PK;

			var service = new AP_ARInvoiceQueryService();
			var result = service.GetTotalInvoicedDetails((ICustomsJobInfo)declaration, new List<ZGuid>(new ZGuid[] { chargeCode1.PK }));
			AssertEquals("TotalInvoicedAmount", 30m, result.TotalInvoicedAmount);
			AssertEquals("TotalOutstandingAmount", 20m, result.TotalOutstandingAmount);

			charge1.ARLine.TransactionHeader.AH_Ledger = Enterprise.ZArchitecture.Core.LedgerTypes.JobCosting;
			charge1.ARLine.TransactionHeader.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.JobRevenueJournal;

			ErrorReporter.Clear();
			result = service.GetTotalInvoicedDetails((ICustomsJobInfo)declaration, new List<ZGuid>(new ZGuid[] { chargeCode1.PK }));
			AssertEquals("TotalInvoicedAmount", 0m, result.TotalInvoicedAmount);
			AssertEquals("TotalOutstandingAmount", 0m, result.TotalOutstandingAmount);

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestFetchHints_GetTotalInvoicedDetails()
		{
			var chargeCode2 = CreateChargeCode("TEST2", "TEST2 Charge Code", GlbCompany.CurrentCompany.PK);
			var chargeCode3 = CreateChargeCode("TEST3", "TEST3 Charge Code", GlbCompany.CurrentCompany.PK);

			var objectCreator = new TestObjectCreator(Factory);
			ZGuid glHeader = objectCreator.CreateGLHeader().PK;

			var declarationPKs = new List<ZGuid>();
			for (int i = 0; i < 10; i++)
			{
				var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declarationPKs.Add(declaration.PK);
				var job = CreateJob(declaration.PK, JobDeclarationSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);

				var transactionLine = objectCreator.CreateRevenueLineAndCharge(job.PK, 10m, chargeCode2.AC_Code);
				transactionLine.AL_AG = glHeader;
				transactionLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			}

			Factory.Save();

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (AssertDbHitsForAllFactories(new Dictionary<string, int>
			{
				{ "AccTransactionHeader", 1 },
				{ "AccTransactionLines", 1 },
				// BaseJobDeclarationFetchStrategy should be responsible for other tables
			}, ignoreUnspecified: true, thresholdForUnspecified: 100, useOnlyNewFactories: true))
			{
				var factory2 = NewFactory();
				var loadedDeclarations = factory2.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.PK, declarationPKs));

				foreach (var declaration in loadedDeclarations)
				{
					var service = new AP_ARInvoiceQueryService();
					var result = service.GetTotalInvoicedDetails((ICustomsJobInfo)declaration, new List<ZGuid>(new ZGuid[] { chargeCode2.PK, chargeCode3.PK }));
					AssertEquals("TotalBilledAmount", 10m, result.TotalBilledAmount);
				}
			}
		}

		public void TestClearLoadedDeclarationJobs()
		{
			#region Create Declarations and Jobs

			var objectCreator = new TestObjectCreator(Factory);

			var shipment1 = objectCreator.CreateShipment("S00001", true);
			var declaration1 = objectCreator.CreateDeclaration("B00001");
			declaration1.JE_JS = shipment1.PK;
			var job1 = CreateJob(shipment1.PK, JobShipmentSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
			var charge = AddCharge(job1, objectCreator.CC1.PK, "1", 20m, true, 25m, true, false, 10);
			charge.ARLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			charge.ARLine.AL_JH = job1.PK;
			charge.ARLine.AL_AC = objectCreator.CC1.PK;
			charge.ARLine.AL_LineAmount = 20m;
			charge = AddCharge(job1, objectCreator.CC2.PK, "1", 40m, false, 45m, true, false, 30);
			charge.ARLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			charge.ARLine.AL_JH = job1.PK;
			charge.ARLine.AL_AC = objectCreator.CC2.PK;
			charge.ARLine.AL_LineAmount = 40m;

			#endregion

			var service = new AP_ARInvoiceQueryService();
			var dBHitsBefore = Factory.GetTableHitCount(AccTransactionLines.Schema.TableName);

			var result = service.GetTotalInvoicedDetails((ICustomsJobInfo)declaration1, new List<ZGuid>(new ZGuid[] { objectCreator.CC1.PK }));
			AssertEquals(20m, result.TotalBilledAmount);
			AssertEquals(10m, result.TotalInvoicedAmount);
			AssertEquals(20m, result.TotalOutstandingAmount);

			var dBHitsAfterFirstResult = Factory.GetTableHitCount(AccTransactionLines.Schema.TableName);
			AssertEquals(dBHitsAfterFirstResult, dBHitsBefore + 1);

			service.ClearServiceCache(declaration1.Factory);

			result = service.GetTotalInvoicedDetails((ICustomsJobInfo)declaration1, new List<ZGuid>(new ZGuid[] { objectCreator.CC1.PK, objectCreator.CC2.PK }));
			AssertEquals(60m, result.TotalBilledAmount);
			AssertEquals(40m, result.TotalInvoicedAmount);
			AssertEquals(60m, result.TotalOutstandingAmount);

			var dBHitsAfterClearingLoadedDeclarationJobs = Factory.GetTableHitCount(AccTransactionLines.Schema.TableName);
			AssertEquals("LoadedDeclarationJobs was cleared, should hit DB.", dBHitsAfterClearingLoadedDeclarationJobs, dBHitsAfterFirstResult + 1);
		}

		Job CreateJob(ZGuid pK, ZString tableCode, ZGuid branchPK, ZGuid companyPK)
		{
			Job result = Factory.NewJobForTesting<Job>();
			result.JH_ParentID = pK;
			result.JH_ParentTableCode = tableCode;
			result.JH_GB = branchPK;
			result.JH_GC = companyPK;
			return result;
		}

		Charge AddCharge(Job job, ZGuid chargeCode, ZString apInvoiceNum, ZDecimal costAmount, ZBool isCostPosted, ZDecimal sellAmount, ZBool isRevenuePosted, ZBool isAPPaid, decimal invoicedAmount = 0m)
		{
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode;

			charge.JR_APInvoiceNum = apInvoiceNum;
			charge.JR_APInvoiceDate = ZDateTime.Today;

			charge.JR_OSCostAmt = costAmount;
			charge.JR_LocalCostAmt = costAmount;

			OrgHeader costAccount = Factory.New<OrgHeader>();
			costAccount.OH_Code = "CA1";
			charge.JR_OH_CostAccount = costAccount.PK;

			if (isCostPosted)
			{
				APInvoice invoice = Factory.New<APInvoice>();
				invoice.AH_TransactionNum = apInvoiceNum;
				invoice.AH_OutstandingAmount = isAPPaid ? ZDecimal.Zero : costAmount;

				APInvoiceLine invoiceLine = (APInvoiceLine)invoice.Lines.AddNew();
				invoiceLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
				charge.JR_AL_APLine = invoiceLine.PK;
			}

			charge.JR_OSSellAmt = sellAmount;
			charge.JR_LocalSellAmt = sellAmount;
			OrgHeader sellAccount = Factory.New<OrgHeader>();
			sellAccount.OH_Code = "SA1";
			charge.JR_OH_SellAccount = sellAccount.PK;

			if (isRevenuePosted)
			{
				ARInvoice invoice = Factory.New<ARInvoice>();
				ARInvoiceLine invoiceLine = (ARInvoiceLine)invoice.Lines.AddNew();
				invoiceLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
				charge.JR_AL_ARLine = invoiceLine.PK;
				invoice.AH_OutstandingAmount = isAPPaid ? ZDecimal.Zero : costAmount;
				invoice.AH_InvoiceAmount = invoicedAmount;
			}

			return charge;
		}

		AccChargeCode CreateChargeCode(ZString code, ZString desc, ZGuid companyPK)
		{
			AccChargeCode result = Factory.New<AccChargeCode>();
			result.AC_Code = code;
			result.AC_GC = companyPK;
			result.AC_Desc = desc;

			return result;
		}
	}
}
