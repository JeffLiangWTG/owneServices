using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Core.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	public partial class MoveDatabasesToNewEnterprisePopupForm : ZChildForm
	{
		public MoveDatabasesToNewEnterprisePopupForm()
		{
		}

		public MoveDatabasesToNewEnterprisePopupForm(MoveDatabasesToNewEnterpriseBizObj moveLicEntBizO)
			: base(moveLicEntBizO)
		{
		}

		protected new MoveDatabasesToNewEnterpriseBizObj BusinessEntity
		{
			get { return (MoveDatabasesToNewEnterpriseBizObj)base.BusinessEntity; }
		}

		void ButtonOk_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors)
			{
				Globals.Message.ShowError(new ZNotificationCollector(BusinessEntity, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).ToMessageListString());
			}
			else if (ShowConfirmationDialog())
			{
				try
				{
					BusinessEntity.MoveDatabasesToNewEnterprise();
				}
				finally
				{
					this.Close();
				}
			}
		}

		protected virtual bool ShowConfirmationDialog()
		{
			DialogResult result = UserNotification.Instance.ShowConfirmation("You are about to set Enterprise ID/Code. Are you sure you want to proceed?", "Warning", "SET", MessageBoxIcon.Warning);
			if (result == DialogResult.OK)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}

	public class MoveDatabasesToNewEnterpriseLogger : ILogger
	{
		void ILogger.Log(LogType type, string message) =>
			Globals.Message.Show(message, type.ToString(), MessageBoxButtons.OK, DialogResult.OK);

		void ILogger.Log(LogType type, string message, Exception ex) =>
			Globals.Message.Show(FormattableString.Invariant($"{message} {ex}"), type.ToString(), MessageBoxButtons.OK, DialogResult.OK);
	}
}