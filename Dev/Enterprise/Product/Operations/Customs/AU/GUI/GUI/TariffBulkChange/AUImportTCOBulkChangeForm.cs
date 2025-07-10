using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class AUImportTCOBulkChangeForm : ZForm
	{
		public AUImportTCOBulkChangeForm(AUImportTariffBulkChange businessEntity)
			: base(businessEntity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostButton, CloseButton);
			DisableNewAction();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			BusinessEntity.HasChanges = true;
		}

		public new AUImportTariffBulkChange BusinessEntity
		{
			get { return base.BusinessEntity as AUImportTariffBulkChange; }
		}

		public override string FormCaption
		{
			get { return "Import TCO Bulk Change"; }
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				result = BusinessEntity.TCOAdditionalContinueWithSave();
			}
			return result;
		}

		void PostingButtonsUserControl_Load(object sender, EventArgs e)
		{
		}
	}
}
