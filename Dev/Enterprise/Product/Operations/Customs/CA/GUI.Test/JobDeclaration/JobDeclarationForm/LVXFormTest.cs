using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class LVXFormTest : JobDeclarationFormTest
	{
		public void TestLVXPreSaveDialogStrategy()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Business.JobMessageTypeList.Codes.LVSForConsolidation;
			Factory.Save();

			using (var form = new JobDeclarationFormForTest(dec))
			{
				var strategies = form.GetPreSaveDialogStrategiesForTesting();
				Assert(strategies.Any(x => x.GetType() == typeof(LVXPreSaveDialogStrategy)));
			}
		}

		public void TestCADPreSaveDialogStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CA.Business.JobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			cadEntry.CH_EntryStatus = CADEntryStatusList.Codes.Approved;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_GoodsShipmentSequence = 1;
			entryLine.CL_CommoditySequence = 1;
			entryLine.ConfirmedFees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 1m);
			entryLine.ConfirmedFees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 2m);
			Factory.Save();
			using (var form = new JobDeclarationFormForTest(declaration))
			{
				var strategies = form.GetPreSaveDialogStrategiesForTesting();
				Assert(strategies.Any(x => x.GetType() == typeof(CADPreSaveDialogStrategy)));
			}

			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 1m);
			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 2m);
			entryLine = cadEntry.MergedLines.AddNew();
			Factory.Save();
			using (var form = new JobDeclarationFormForTest(declaration))
			{
				var strategies = form.GetPreSaveDialogStrategiesForTesting();
				Assert(strategies.Any(x => x.GetType() == typeof(CADPreSaveDialogStrategy)));
			}
		}

		public void TestCADPreSaveDialogStrategyForLVSDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CA.Business.JobMessageTypeList.Codes.LowValueShipments;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			cadEntry.CH_EntryStatus = CADEntryStatusList.Codes.Approved;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_GoodsShipmentSequence = 1;
			entryLine.CL_CommoditySequence = 1;
			entryLine.ConfirmedFees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 1m);
			entryLine.ConfirmedFees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 2m);
			Factory.Save();
			using (var form = new JobDeclarationFormForTest(declaration))
			{
				var strategies = form.GetPreSaveDialogStrategiesForTesting();
				Assert(strategies.Any(x => x.GetType() == typeof(CADPreSaveDialogStrategy)));
			}

			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 1m);
			entryLine.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 2m);
			entryLine = cadEntry.MergedLines.AddNew();
			Factory.Save();
			using (var form = new JobDeclarationFormForTest(declaration))
			{
				var strategies = form.GetPreSaveDialogStrategiesForTesting();
				Assert(strategies.Any(x => x.GetType() == typeof(CADPreSaveDialogStrategy)));
			}
		}

		public new void TestPlugins()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Business.JobMessageTypeList.Codes.LVSForConsolidation;
			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		public override ZString MessageTypeForFormBashing => Business.JobMessageTypeList.Codes.LVSForConsolidation;

		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashingCore();
			declaration.CA_RequiresMerge = false;
			return declaration;
		}
	}
}
