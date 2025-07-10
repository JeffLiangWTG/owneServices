using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing;

public class AUDeclarationSpecificTest : TestCaseWithFactory
{
	public void TestGetReasonNotToAllowAutoRatesWithDeclaration_WhenDeferredGSTIsInconsistent()
	{
		var testDec = JobDeclarationTest.SetUpAndMergeImportDec(Factory);
		testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		testDec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
		testDec.Importer.OH_Code = "TESTIMPORG";
		testDec.Importer.MiscServ.OM_IMIsGSTDeferred = true;
		testDec.ActiveEntryHeaders[0].CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
		var fee = testDec.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.GSTAmount);
		fee.CF_ChargeAmount = 1m;
		var invoicingJob = Factory.NewJobForTesting<JobHeader>();
		invoicingJob.JH_ParentID = testDec.PK;
		invoicingJob.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
		invoicingJob.JH_JobNum = "Job1";
		invoicingJob.JH_GB = GlbBranch.CurrentBranch.PK;
		invoicingJob.JH_GC = GlbCompany.CurrentCompany.PK;
		invoicingJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
		invoicingJob.JH_OA_LocalChargesAddr = testDec.Importer.MainAddress.PK;
		testDec.Importer.MiscServ.OM_IMIsGSTDeferred = false;
		Factory.Save();

		using (var form = new ZAUCustomsDeclarationForm(testDec))
		{
			var runner = new AutoRatingStarter(testDec, new AutoRatingGUIInteractor(form));
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			Assert(!UnitTestUserNotification.Instance.LastMessage.Text.Contains("The consignee tab of the importer organization of this declaration has the GST Deferred box ticked"));
		}

		testDec.Importer.MiscServ.OM_IMIsGSTDeferred = true;
		Factory.Save();

		using (var form = new ZAUCustomsDeclarationForm(testDec))
		{
			var runner = new AutoRatingStarter(testDec, new AutoRatingGUIInteractor(form));
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertEquals(@"The consignee tab of the importer organization of this declaration has the GST Deferred box ticked,
	however GST information returned by Customs indicates that GST is not deferred.
	You should correct the Importer organization GST setting,
	and then attempt the auto-rating again.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestGetReasonNotToAllowAutoRatesWithShipment_WhenDeferredGSTIsInconsistent()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var testDec = JobDeclarationTest.SetUpAndMergeImportDec(Factory);
		testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		testDec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
		testDec.Importer.OH_Code = "TESTIMPORG";
		testDec.Importer.MiscServ.OM_IMIsGSTDeferred = true;
		testDec.ActiveEntryHeaders[0].CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
		testDec.JE_OverrideFreightDefaults = true;
		testDec.JE_JS = shipment.PK;
		var fee = testDec.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.GSTAmount);
		fee.CF_ChargeAmount = 1m;
		var invoicingJob = Factory.NewJobForTesting<JobHeader>();
		invoicingJob.JH_ParentID = shipment.PK;
		invoicingJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
		invoicingJob.JH_JobNum = "Job1";
		invoicingJob.JH_GB = GlbBranch.CurrentBranch.PK;
		invoicingJob.JH_GC = GlbCompany.CurrentCompany.PK;
		invoicingJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
		invoicingJob.JH_OA_LocalChargesAddr = testDec.Importer.MainAddress.PK;
		testDec.Importer.MiscServ.OM_IMIsGSTDeferred = false;
		Factory.Save();

		using (var form = new ZForm(shipment))
		{
			var runner = new AutoRatingStarter(shipment, new AutoRatingGUIInteractor(form));
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			Assert(!UnitTestUserNotification.Instance.LastMessage.Text.Contains("The consignee tab of the importer organization of this declaration has the GST Deferred box ticked"));
		}

		testDec.Importer.MiscServ.OM_IMIsGSTDeferred = true;
		Factory.Save();

		using (var form = new ZForm(shipment))
		{
			var runner = new AutoRatingStarter(shipment, new AutoRatingGUIInteractor(form));
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertEquals(@"The consignee tab of the importer organization of this declaration has the GST Deferred box ticked,
	however GST information returned by Customs indicates that GST is not deferred.
	You should correct the Importer organization GST setting,
	and then attempt the auto-rating again.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestGetReasonNotToAllowAutoRatesWithDeclaration_WhenDeferredDutyIsInconsistent()
	{
		var testDec = JobDeclarationTest.SetUpAndMergeImportDec(Factory);
		testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		testDec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
		testDec.Importer.OH_Code = "TESTIMPORG";
		testDec.ActiveEntryHeaders[0].CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
		testDec.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyAmount, 100m);
		var invoicingJob = Factory.NewJobForTesting<JobHeader>();
		invoicingJob.JH_ParentID = testDec.PK;
		invoicingJob.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
		invoicingJob.JH_JobNum = "Job1";
		invoicingJob.JH_GB = GlbBranch.CurrentBranch.PK;
		invoicingJob.JH_GC = GlbCompany.CurrentCompany.PK;
		invoicingJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
		invoicingJob.JH_OA_LocalChargesAddr = testDec.Importer.MainAddress.PK;
		testDec.Importer.AUIsDutyDeferred = true;
		Factory.Save();

		using (var form = new ZAUCustomsDeclarationForm(testDec))
		{
			var runner = new AutoRatingStarter(testDec, new AutoRatingGUIInteractor(form));
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertEquals(@"The consignee tab of the importer organization of this declaration has the Duty Deferred box ticked,
however duty information returned by Customs indicates that duty is not deferred.
You should correct the Importer organization duty setting,
and then attempt the auto-rating again.
Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		using (var form = new ZAUCustomsDeclarationForm(testDec))
		{
			var runner = new AutoRatingStarter(testDec, new AutoRatingGUIInteractor(form));
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertStartsWith("AutoRated", "AutoRating has been completed.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		var entryHeader = testDec.ActiveEntryHeaders[0];
		entryHeader.Charges.AddNew(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyAmount, 100m);
		entryHeader.Charges.AddNew(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
		Factory.Save();

		using (var form = new ZAUCustomsDeclarationForm(testDec))
		{
			var runner = new AutoRatingStarter(testDec, new AutoRatingGUIInteractor(form));
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertStartsWith("Deferred Duty is AutoRated", "AutoRating has been completed.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		entryHeader.Charges.SetAmount(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyDeferredAmount, 80m);
		Factory.Save();

		using (var form = new ZAUCustomsDeclarationForm(testDec))
		{
			var runner = new AutoRatingStarter(testDec, new AutoRatingGUIInteractor(form));
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertStartsWith("Deferred Duty with Excise is AutoRated", "AutoRating has been completed.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		entryHeader.Charges.AddNew(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISProcessingCharge, 30m);
		entryHeader.Charges.SetAmount(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyDeferredAmount, 130m);
		Factory.Save();

		using (var form = new ZAUCustomsDeclarationForm(testDec))
		{
			var runner = new AutoRatingStarter(testDec, new AutoRatingGUIInteractor(form));
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertStartsWith("Deferred Duty Plus is AutoRated", "AutoRating has been completed.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		entryHeader.Charges.SetAmount(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyDeferredAmount, 120m);
		Factory.Save();

		using (var form = new ZAUCustomsDeclarationForm(testDec))
		{
			var runner = new AutoRatingStarter(testDec, new AutoRatingGUIInteractor(form));
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertStartsWith("Deferred Duty Plus with Excise is AutoRated", "AutoRating has been completed.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		testDec.Importer.AUIsDutyDeferred = false;
		using (var form = new ZAUCustomsDeclarationForm(testDec))
		{
			var runner = new AutoRatingStarter(testDec, new AutoRatingGUIInteractor(form));
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertEquals(@"The consignee tab of the importer organization of this declaration has the Duty Deferred box NOT ticked,
however duty information returned by Customs indicates that duty is deferred.
You should correct the Importer organization duty setting,
and then attempt the auto-rating again.
Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		using (var form = new ZAUCustomsDeclarationForm(testDec))
		{
			var runner = new AutoRatingStarter(testDec, new AutoRatingGUIInteractor(form));
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertStartsWith("AutoRated", "AutoRating has been completed.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestGetReasonNotToAllowAutoRatesWithShipment_WhenDeferredDutyIsInconsistent()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var testDec = JobDeclarationTest.SetUpAndMergeImportDec(Factory);
		testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		testDec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
		testDec.Importer.OH_Code = "TESTIMPORG";
		testDec.ActiveEntryHeaders[0].CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
		testDec.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyAmount, 100m);
		testDec.JE_OverrideFreightDefaults = true;
		testDec.JE_JS = shipment.PK;
		var invoicingJob = Factory.NewJobForTesting<JobHeader>();
		invoicingJob.JH_ParentID = shipment.PK;
		invoicingJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
		invoicingJob.JH_JobNum = "Job1";
		invoicingJob.JH_GB = GlbBranch.CurrentBranch.PK;
		invoicingJob.JH_GC = GlbCompany.CurrentCompany.PK;
		invoicingJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
		invoicingJob.JH_OA_LocalChargesAddr = testDec.Importer.MainAddress.PK;
		testDec.Importer.AUIsDutyDeferred = true;
		Factory.Save();

		using (var form = new ZForm(shipment))
		{
			var runner = new AutoRatingStarter(shipment, new AutoRatingGUIInteractor(form));
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertEquals(@"The consignee tab of the importer organization of this declaration has the Duty Deferred box ticked,
however duty information returned by Customs indicates that duty is not deferred.
You should correct the Importer organization duty setting,
and then attempt the auto-rating again.
Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		using (var form = new ZForm(shipment))
		{
			var runner = new AutoRatingStarter(shipment, new AutoRatingGUIInteractor(form));
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			Assert(UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("AutoRating has been completed."));
		}

		testDec.Importer.AUIsDutyDeferred = false;
		testDec.ActiveEntryHeaders[0].Charges.AddNew(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
		Factory.Save();

		using (var form = new ZForm(shipment))
		{
			var runner = new AutoRatingStarter(shipment, new AutoRatingGUIInteractor(form));
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertEquals(@"The consignee tab of the importer organization of this declaration has the Duty Deferred box NOT ticked,
however duty information returned by Customs indicates that duty is deferred.
You should correct the Importer organization duty setting,
and then attempt the auto-rating again.
Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		using (var form = new ZForm(shipment))
		{
			var runner = new AutoRatingStarter(shipment, new AutoRatingGUIInteractor(form));
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			Assert(UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("AutoRating has been completed."));
		}
	}
}
