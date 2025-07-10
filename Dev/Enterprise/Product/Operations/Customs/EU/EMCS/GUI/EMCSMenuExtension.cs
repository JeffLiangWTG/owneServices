using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public static class EMCSMenuExtension
	{
		public static bool CanSend(this EMCSJobDeclaration declaration, ZForm formToFireSaveButton, string inactiveInformationText, Func<bool> activeCondition)
			=> IsActive(inactiveInformationText, activeCondition) && formToFireSaveButton.SafeAndValidateData(declaration);

		public static bool IsActive(string notActiveInformationText, Func<bool> activeCondition)
		{
			var isActive = activeCondition.Invoke();
			if (!isActive)
			{
				Globals.Message.ShowInformation(notActiveInformationText);
			}
			return isActive;
		}

		static bool SafeAndValidateData(this ZForm formToFireSaveButton, EMCSJobDeclaration declaration)
		{
			var saved = SaveDataFirst.Confirm(declaration, formToFireSaveButton);
			if (saved && !declaration.HasChanges)
			{
				declaration.MarkAsNeedingValidationIncludingChildren();
				declaration.RunPreSaveValidation();
				if (declaration.HasErrors)
				{
					Globals.Message.Show(Res.GetString("d5843d5b-f916-476a-b44c-73b59f413c3d", "The message cannot be sent. Please fix all errors on the form before trying to send a message."), Res.GetString("126f93f0-e820-4cca-8db1-3fc25e3255f1", "Cannot Send Message"), MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
			}
			return saved;
		}
	}
}
