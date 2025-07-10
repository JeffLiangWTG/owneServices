using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.GUI
{
	public partial class FinalPriceExtensionRequestNewForm : ZChildForm
	{
		public FinalPriceExtensionRequestNewForm(FinalPriceReportByDateExtensionHeader header)
			: base(header)
		{
			this.header = header;
			InitializeComponent();
		}
		readonly FinalPriceReportByDateExtensionHeader header;

		public override string FormCaption => Res.GetString("C3BB933C-0B52-4F92-BBE5-DFA0BCD1F458", "(Imp) Final Price Period Extension Application");

		void SendButton_Click(object sender, EventArgs e)
		{
			try
			{
				header.Validation.ValidateAll();
				foreach (FinalPriceReportByDateExtensionLine line in header.FinalPriceReportByDateExtensionLines)
				{
					line.Validation.ValidateAll();
				}

				if (header.HasErrors)
				{
					using (ZMessageBox msgBox = new ZErrorMessageBox(BusinessEntityForValidation, false))
					{
						msgBox.Text = Res.GetString("5E33F22A-18CD-46FA-A943-1DA949998B86", "Unable to send...");
						msgBox.Message = Res.GetString("0C44C108-A88C-4212-AFB0-B531A87FE52C", "There are errors on the form. Please fix them first.");
						ZFormModaliser.ShowDialogWithoutDispose(msgBox);
					}
					return;
				}
				else
				{
					if (header.HasMessageErrors)
					{
						if (header.SendWithMessageErrorsIsAllowed)
						{
							var errorMessage = Res.GetString("7AFF1104-FE3F-4DED-B4A1-70FA38CCEB5D", "There are message errors on the form. Are you sure you want to send a message regardless of those message errors?");
							var caption = Res.GetString("AA217823-DEFA-43DB-93C0-204A57C83E4E", "Warning");
							using (var notification = new ZMessageBox(errorMessage, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
							{
								var result = ZFormModaliser.ShowDialogWithoutDispose(notification);
								if (result != DialogResult.Yes)
								{
									return;
								}
							}
						}
						else
						{
							Globals.Message.ShowInformation(Res.GetString("A8B0ABC5-428B-42B5-A98F-9439727490F4", "There are message errors on the form. Unless you fix them, you will not be able to send a message as you don't have the security right to send with message errors."));
							return;
						}
					}
				}

				var miscRequestHeaderCreator = new CusMiscRequestHeaderCreator();
				var miscRequestHeader = miscRequestHeaderCreator.Create(header);
				var message = miscRequestHeaderCreator.CreateEdiMessage(miscRequestHeader, header);

				header.Factory.Save();

				DialogResult = System.Windows.Forms.DialogResult.OK;
				if (message != null)
				{
					Globals.Message.ShowInformation(Res.GetString("B1AB5801-7701-415A-9A30-6C05AC3FE9F9", "1 Message of # {0} is generated.", message.EM_MessageNum));
				}
				Close();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		void SearchButton_Click(object sender, EventArgs e)
		{
			var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.KR.EntryDetailsFor5SG);
			var popup = new EmbeddedModulePopup(module);
			var strategy = new KREntryDetailsPopupOKButtonStrategy(popup, header.FinalPriceReportByDateExtensionLines);
			popup.EmbeddedModulePopupOKButtonStrategy = strategy;
			module.OverrideModuleDecisionProvider(strategy);

			ZFormModaliser.Show(popup, this);
		}
	}
}
