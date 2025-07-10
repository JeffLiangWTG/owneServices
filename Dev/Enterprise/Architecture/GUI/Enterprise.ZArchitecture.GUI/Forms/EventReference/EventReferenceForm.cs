using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class EventReferenceForm : ZChildForm, IFindBoxPopup
	{
		IFindBox findBox;

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public EventReferenceForm()
		{
			InitializeComponent();
		}

		public EventReferenceForm(EventReference bo)
			: base(bo)
		{
			InitializeComponent();
		}

		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DescriptionTextLabel.Text = Res.GetString("36d89973-5d03-49a5-8043-f0b209831649", "Syntax of structured event reference is: \r\noptional text|parameter 1=value 1|parameter 2=value 2 etc. Parameters must be 1-3 characters.\r\nExample: |TYP=Import Detention|RES=Client Delay|TO=Client Name\r\n\r\nChoose an applicable parameter code from the list (or use your own at your discretion), add value, use “Free Text” to enter optional additional text if required.");
		}

		#endregion

		public ZString ReferenceText { get; private set; }

		#region Overriden

		public new EventReference BusinessEntity
		{
			get { return base.BusinessEntity as EventReference; }
		}

		#region ConfimButton_Click

#if DEBUG
		protected virtual
#endif

		void ConfimButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.RunPreSaveValidation();

				if (BusinessEntity.HasErrors())
				{
					Globals.Message.ShowError(Res.GetString("e491ff68-8f19-41c6-9f7d-aa0abf585fb0", "Please fix all the errors before proceeding."));
					return;
				}

				if (BusinessEntity.CompleteText.Length > BaseStmALog.Schema.SL_ReferenceMaxLength)
				{
					Globals.Message.ShowError(Res.GetString("b9b066b8-b910-4280-b789-945d9a1cdbc4", "The event reference generated cannot be more than {0} characters.", BaseStmALog.Schema.SL_ReferenceMaxLength));
					return;
				}

				ReferenceText = BusinessEntity.CompleteText;

				if (findBox != null)
				{
					findBox.Code = BusinessEntity != null ? BusinessEntity.CompleteText : ZString.Empty;
				}

				DialogResult = DialogResult.OK;
			}

			Close();
		}

		#endregion

		#region CancelButton_Click

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			this.findBox = findBox;
			ZFormModaliser.Show(this, parentForm);
		}

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		public void SelectRowByPK(ZGuid pK)
		{
		}

		#endregion

	}
}
