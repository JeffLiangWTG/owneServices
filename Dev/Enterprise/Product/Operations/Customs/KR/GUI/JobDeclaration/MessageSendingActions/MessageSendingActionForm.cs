using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class MessageSendingActionForm : MessageSendingObjectForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public MessageSendingActionForm()
		{
		}

		public MessageSendingActionForm(Business.IJobDeclarationMessageSendingObjectParent declarationWrapper, MessageSendingFormBuilder builder)
		: base((BaseMessageSendingObjectParent)declarationWrapper)
		{
			messageType = declarationWrapper.MessageType;
			InitializeNewColumns(builder);
		}
		readonly ZString messageType;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			this.MessageSendingObjectsGrid.RemoveAction = RemoveAction.NoRemovePossible;
		}
		public override string FormHeading => Res.GetString("9A99DE3E-89CB-416E-92A6-DF9A27A51327", @"Sending {0} Messages", messageType);

		public new Business.IJobDeclarationMessageSendingObjectParent BusinessEntity => (Business.IJobDeclarationMessageSendingObjectParent)base.BusinessEntity;

		void InitializeNewColumns(MessageSendingFormBuilder builder)
		{
			MessageSendingObjectsGrid.ColumnStyles.Clear();
			var additionalColumnStyles = builder.GetColumnStyles();
			if (additionalColumnStyles != null && additionalColumnStyles.Length > 0)
			{
				MessageSendingObjectsGrid.ColumnStyles.AddRange(additionalColumnStyles);
			}
			ManageAmendmentUserControlInSplitContainer(builder);
			ChangeValidationErrorsToTabPages(builder);
			SetFormMinimumSize(builder);
		}

		void ManageAmendmentUserControlInSplitContainer(MessageSendingFormBuilder builder)
		{
			userControl = builder.GetUserControl();
			if (userControl != null)
			{
				userControl.AllowDrop = true;
				userControl.Dock = System.Windows.Forms.DockStyle.Fill;
				userControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 17, true);
				userControl.TabIndex = 0;
				ItemsGroupBox.Visible = true;
				ItemsGroupBox.CaptionResourceString = builder.GetUserControlGroupBoxCaption();
				ItemsGroupBox.Controls.Add(userControl);
				BindingSource.SetBindingMember(userControl, "SendingObjectsCollection");
			}
			else
			{
				ItemsGroupBox.Visible = false;
				messageSendingObjectsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
				ItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			}
			SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(builder.Panel1MinSize);
			if (builder.Panel2MinSize != 0)
			{
				SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(builder.Panel2MinSize);
			}
			SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(builder.Panel1MinSize);
		}
		ZUserControl userControl { get; set; }

		void ChangeValidationErrorsToTabPages(MessageSendingFormBuilder builder)
		{
			var tabPages = builder.GetAdditionalTabPages(MessageSendingObjectsGrid);
			if (tabPages == null || tabPages.Length == 0)
			{
				return;
			}
			tabControl = new ZTabControl();
			var validationErrorsTabPage = new ZTabPage();

			this.ValidationErrorsGroupBox.Visible = false;

			this.SplitContainer.Panel2.Controls.Add(tabControl);

			tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			tabControl.Name = "ValidationErrorsTabControl";
			tabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			tabControl.SelectedIndex = 0;

			validationErrorsTabPage.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("70accbe8-3b1c-4be6-91df-1e449a91c93c", "Validation Errors");
			validationErrorsTabPage.Controls.Add(this.ValidationErrorsTextBox);
			validationErrorsTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			validationErrorsTabPage.Name = "ValidationErrorsTabPage";
			validationErrorsTabPage.TabStop = false;
			builder.ChangeValidationErrorBindingIfNeeded(this.BindingSource, this.ValidationErrorsTextBox);

			tabControl.Controls.Add(validationErrorsTabPage);
			tabControl.Controls.AddRange(tabPages);
		}
		ZTabControl tabControl { get; set; }

		void SetFormMinimumSize(MessageSendingFormBuilder builder)
		{
			var width = builder.GetFormSize()[0];
			var height = builder.GetFormSize()[1];

			ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(width, height, true);
			MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(width, height, true);
		}
	}
}
