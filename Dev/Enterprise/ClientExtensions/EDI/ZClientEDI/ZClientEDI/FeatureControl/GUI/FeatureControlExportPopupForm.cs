using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.FeatureControl.GUI
{
	public partial class FeatureControlExportPopupForm : ZChildForm
	{
		public FeatureControlExportPopupForm()
		{
		}

		public FeatureControlExportPopupForm(FeatureControlExportBizObj bizO)
			: base(bizO)
		{
		}

		protected new FeatureControlExportBizObj BusinessEntity => (FeatureControlExportBizObj)base.BusinessEntity;

		void ButtonOk_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors)
			{
				Globals.Message.ShowError(new ZNotificationCollector(BusinessEntity, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).ToMessageListString());
			}
			else
			{
				BusinessEntity.Export();
			}
		}
	}
}
