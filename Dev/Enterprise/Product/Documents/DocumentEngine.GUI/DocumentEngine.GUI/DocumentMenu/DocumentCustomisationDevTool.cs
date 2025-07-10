using System.Windows.Forms;
using Enterprise.DocumentEngine.Build;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	/// <summary>
	/// Allows the customisation of documents from the developer diagnostics form when added to the forms
	/// list of IDevTool's. (for use with forms that dont otherwise show a documents menu).
	/// </summary>
	public partial class DocumentCustomisationDevTool : IDevTool
	{
		protected virtual void ShowCore(Form form)
		{
			ZForm zForm;
			IDocumentSupportable supportable;

			if ((zForm = form as ZForm) == null)
			{
				Globals.Message.Show((NoResString)"Not a ZForm");
			}
			else if ((supportable = zForm.BusinessEntity as IDocumentSupportable) == null)
			{
				Globals.Message.Show((NoResString)"IDocumentSupportable not implemented");
			}
			else
			{
				ShowCustomisationForm(supportable);
			}
		}

		protected static void ShowCustomisationForm(IDocumentSupportable supportable)
		{
			DocumentMenuCustomisation customisation = DocumentMenuCustomisation.New(supportable, new UserControlProviderList());
			customisation.EditingMode = EditingMode;
			ZFormModaliser.ShowDialogAndDispose(new DocumentCustomisationForm(customisation));
		}

		static MenuEditingMode EditingMode
		{
			get
			{
#if DEBUG
				if (new DocumentsSetupController().IsCheckedOutByMe)
				{
					if (string.IsNullOrEmpty(Env.Registry.CurrentCheckedOutClientName))
					{
						return MenuEditingMode.AllowEditingOfSystemDefinedOnly;
					}
					else
					{
						return MenuEditingMode.AllowEditingOfClientSpecificOnly;
					}
				}
				else
#endif
				{
					return MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
				}
			}
		}

		#region IDevTool

		bool IDevTool.AddAsButton
		{
			get { return false; }
		}

		public virtual string Name
		{
			get { return Res.GetString("1ad30485-5d49-477c-8a6a-64f284bec040", "Customize Documents"); }
		}

		public void Show(Form form)
		{
			ShowCore(form);
		}

		#endregion
	}
}
