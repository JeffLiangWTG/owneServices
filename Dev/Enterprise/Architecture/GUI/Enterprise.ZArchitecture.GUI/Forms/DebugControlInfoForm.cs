using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class DebugControlInfoForm : KForm
	{
		Control targetControl;
		ZForm TargetForm => targetControl?.FindForm() as ZForm;
		string controlInfo;
		string listContent;

		IMapTreePresentationManager MapTreePresentationManager
		{
			get
			{
				if (mapTreePresentationManager == null)
				{
					mapTreePresentationManager = ObjectFactory.Get<IMapTreePresentationManager>();
					mapTreePresentationManager.ModalParent = TargetForm;
					mapTreePresentationManager.UseBrowseMode = true;
				}

				return mapTreePresentationManager;
			}
		}
		IMapTreePresentationManager mapTreePresentationManager;

		IMapTreePresentationManager McrMapTreePresentationManager
		{
			get
			{
				if (mcrMapTreePresentationManager == null)
				{
					mcrMapTreePresentationManager = ObjectFactory.Get<IMapTreePresentationManager>();
					mcrMapTreePresentationManager.ModalParent = TargetForm;
					mcrMapTreePresentationManager.UseMcrEvaluator = true;
					mcrMapTreePresentationManager.UseBrowseMode = true;
					mcrMapTreePresentationManager.ShowEditField = true;
					mcrMapTreePresentationManager.ShowXmlFields = true;
					mcrMapTreePresentationManager.DefaultCollectionIndex = 0;
					mcrMapTreePresentationManager.OpeningMacroTag = string.Empty;
					mcrMapTreePresentationManager.ClosingMacroTag = string.Empty;
				}

				return mcrMapTreePresentationManager;
			}
		}
		IMapTreePresentationManager mcrMapTreePresentationManager;

		public DebugControlInfoForm()
		{
			InitializeComponent();
			messageTextBox.CharacterCasing = CharacterCasing.Normal;
			messageTextBox.ColorChanger.ForceBackColor(Color.White);
		}

		public DialogResult ShowFormInfo(Form form)
		{
			(controlInfo, listContent, targetControl) = FormDebugInfo.GetActiveControlInfoFromFormForDocEngine(form);
			Text = GetText(targetControl.Name);
			messageTextBox.Text = controlInfo;
			dataFieldMapButton.Visible = mcrDataFieldMapButton.Visible = form is ZForm;
			copyContentButton.Visible = !string.IsNullOrEmpty(listContent);
			return ZFormModaliser.ShowDialogAndDispose(this, form);
		}

		public DialogResult ShowControlInfo(Control control)
		{
			targetControl = control;
			Text = GetText(control.Name);
			(controlInfo, listContent) = FormDebugInfo.GetActiveControlInfoFromControlForDocEngine(control);
			messageTextBox.Text = controlInfo;
			dataFieldMapButton.Visible = mcrDataFieldMapButton.Visible = TargetForm != null;
			copyContentButton.Visible = !string.IsNullOrEmpty(listContent);
			return ZFormModaliser.ShowDialogAndDispose(this, control.FindForm());
		}

		string GetText(string name)
		{
			return Res.GetString("d11b3670-9ec5-4da1-81bb-6d6cbd228263", "Control Information for {0}", name);
		}

		void DataFieldMapButton_Click(object sender, EventArgs e)
		{
			if (TargetForm.DataSource is BusinessObject businessObject)
			{
				MapTreePresentationManager.ShowPresentationManagerForm(businessObject, targetControl);
			}
		}

		void mcrDataFieldMapButton_Click(object sender, EventArgs e)
		{
			if (TargetForm.DataSource is BusinessObject businessObject)
			{
				McrMapTreePresentationManager.ParentTypes = new[] { businessObject.GetType() }; 
				McrMapTreePresentationManager.ParentBusinessObjects = new MacroScope[] { new MacroScope(businessObject) }; 
				McrMapTreePresentationManager.ShowPresentationManagerForm();
			}
		}

		void CopyContentButton_Click(object sender, EventArgs e)
		{
			if (SafeClipboard.SetText(listContent))
			{
				Globals.Message.Show(Res.GetString("9574e786-a928-416f-894c-2fcf4ae91942", "List content has been copied to clipboard."));
			}
		}
	}
}
