using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CA.GUI
{
	public class MessageInstructionUserNotification : UserNotification, IMessageInstructionUserNotification
	{
		#region Implementation of IMessageInstructionUserNotification

		public bool ShowMessageInstructionForm(MessageInstruction instruction)
		{
			var result = DialogResult.OK;
			if (IsShowForm(instruction))
			{
				result = Show(new MessageInstructionForm(instruction));
				if (result == DialogResult.OK)
				{
					if (Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed && instruction.ContainsValidationErrors)
					{
						var businessEntity = instruction.BusinessObject as EnterpriseBusinessObject;
						if (businessEntity != null)
						{
							var supervisorOverrides = new SupervisorOverrides(businessEntity, Business.SupervisorOverridesContext.SendingMessages);
							if (!SupervisorOverridesHelper.IsSupervisorApproved(supervisorOverrides, businessEntity.Logs))
							{
								result = DialogResult.Cancel;
							}
						}
					}
				}
			}
			return result == DialogResult.OK;
		}

		#endregion

		protected bool IsShowForm(MessageInstruction instruction)
		{
			return instruction.IsWaitingForResponse || instruction.ContainsValidationErrors || instruction.ContainsAdditionalWarnings || instruction.ShowJobReadyForPosting;
		}
	}
}
