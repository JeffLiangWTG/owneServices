using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Module Controller for AlternateChartofAccounts.
	/// </summary>
	public class AlternateChartofAccountsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AlternateChartofAccountsController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AlternateChartofAccounts; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AlternateChartofAccounts; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccAlternateChart); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AlternateChartofAccountsView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AlternateChartofAccountsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AlternateChartofAccountsNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AlternateChartofAccountsDelete; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AlternateChartofAccountsForm((AccAlternateChart)businessEntity);
		}
	}
}
