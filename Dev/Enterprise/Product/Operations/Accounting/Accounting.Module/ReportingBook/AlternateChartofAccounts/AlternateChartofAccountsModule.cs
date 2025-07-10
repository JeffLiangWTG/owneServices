using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AlternateChartofAccountsModule : ZFilterGridModule
	{
		public AlternateChartofAccountsModule()
		{
		}

		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AlternateChartofAccounts; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new AlternateChartofAccountsController();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AlternateChartofAccountsFilterControl(GridCollection, (AlternateChartofAccountsFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccAlternateChartCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AlternateChartofAccountsFilterBusinessObject();
		}

		protected override void HandleDeleteClickCore(object sender, EventArgs e)
		{
			var selectedChartPKs = SelectedBusinessObjects.Cast<AccAlternateChart>().Select(chart => chart.PK).ToArray();
			if (!Factory.Exists(typeof(AccAlternateGLAccount), new ZQuery(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, selectedChartPKs))
				&& !Factory.Exists(typeof(AccReportingBook), new ZQuery(AccReportingBookSchema.ARB_AAC_AlternateChart, selectedChartPKs)))
			{
				base.HandleDeleteClickCore(sender, e);
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("E42E6377-6F27-40AC-8BD9-D23FA2F00AC4", "Can't delete Alternate Chart of Account because Reporting Books or Alternate GL Accounts are using this chart. Please remove this chart from all Reporting Books and delete all its Alternate GL Account before deleting."));
			}
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AlternateChartofAccounts; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override bool AllowUniversalCopy => false;

		protected override bool AllowAdvancedDataAutomationWizard => false;
		#endregion
	}
}
