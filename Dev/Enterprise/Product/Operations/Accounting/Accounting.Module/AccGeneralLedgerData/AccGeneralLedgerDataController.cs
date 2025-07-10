using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class AccGeneralLedgerDataController : ZController
	{
		#region Overrides

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AccGeneralLedgerData; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccGeneralLedgerData; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccGeneralLedgerData); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			Globals.Message.Show(Res.GetString("F231E0D8-A427-43B8-AD5A-51537926FC6F", "General Ledger Data don't have form"));
			return null;
		}

		public override IZForm ShowNewForm()
		{
			Globals.Message.Show(Res.GetString("3B63A556-91FF-408F-B0CE-2756AA60137A", "General Ledger Data don't have form"));
			return null;
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		#endregion

		#endregion
	}
}
