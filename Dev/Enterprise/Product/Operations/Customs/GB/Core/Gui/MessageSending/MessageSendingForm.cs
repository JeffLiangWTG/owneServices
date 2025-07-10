using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.GUI
{
	public class MessageSendingForm : MessageSendingObjectForm
	{
		public MessageSendingForm(JobDeclarationMessageSendingObjectParent declarationWrapper)
			: base(declarationWrapper)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();

			if (DataSource is JobDeclarationMessageSendingObjectParent parent && parent.ParentDeclaration.JE_ApplicationCode == GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services)
			{
				SuspendLayout();
				waitForResponseCheckBox = new ZArchitecture.GUI.ZCheckBox();
				waitForResponseCheckBox.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
				waitForResponseCheckBox.AutoSize = true;
				waitForResponseCheckBox.CaptionResourceString = Res.GetData("e365afa1-9552-4b09-9c69-f02902514f74", "Lock declaration until a response is received");
				waitForResponseCheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(13, 260, true);
				waitForResponseCheckBox.Name = "WaitForResponseCheckBox";
				waitForResponseCheckBox.Size = ControlDpiScalingHelper.NewScaledSize(236, 17, true);
				waitForResponseCheckBox.TabIndex = 1;
				waitForResponseCheckBox.UseVisualStyleBackColor = true;
				waitForResponseCheckBox.Visible = true;
				waitForResponseCheckBox.SetBindingMember(nameof(parent.LockDeclarationUntilResponseReceived));
				Controls.Add(waitForResponseCheckBox);
				ResumeLayout(false);
				PerformLayout();
			}

			InitializeNewColumns();
		}

		void InitializeNewColumns()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo messageTypeDropEditColumnStyleInfo = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			messageTypeDropEditColumnStyleInfo.ColumnName = JobDeclarationMessageSendingObject.Schema.MessageType;
			messageTypeDropEditColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			messageTypeDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			messageTypeDropEditColumnStyleInfo.IsMandatory = true;
			this.MessageSendingObjectsGrid.ColumnStyles.Add(messageTypeDropEditColumnStyleInfo);

			ZTextBoxColumnStyleInfo ducrTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ducrTextBoxColumnStyleInfo.ColumnName = JobDeclarationMessageSendingObject.Schema.LocalReferenceNumber;
			ducrTextBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(ducrTextBoxColumnStyleInfo);

			ZTextBoxColumnStyleInfo mRNTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			mRNTextBoxColumnStyleInfo.ColumnName = JobDeclarationMessageSendingObject.Schema.MovementReferenceNumber;
			mRNTextBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(mRNTextBoxColumnStyleInfo);

			ZTextBoxColumnStyleInfo entryStatusTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			entryStatusTextBoxColumnStyleInfo.ColumnName = JobDeclarationMessageSendingObject.Schema.EntryStatus;
			entryStatusTextBoxColumnStyleInfo.GroupName = Res.GetData("B6376C48-35F4-4DF2-8D06-0261BD8970CF", "Customs Status");
			entryStatusTextBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(entryStatusTextBoxColumnStyleInfo);

			ZTextBoxColumnStyleInfo entryStatusDescriptionTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			entryStatusDescriptionTextBoxColumnStyleInfo.ColumnName = EU.Business.JobDeclarationMessageSendingObject.Schema.EntryStatusDescription;
			entryStatusDescriptionTextBoxColumnStyleInfo.GroupName = Res.GetData("B6376C48-35F4-4DF2-8D06-0261BD8970CF", "Customs Status");
			entryStatusDescriptionTextBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(entryStatusDescriptionTextBoxColumnStyleInfo);

			ZArchitecture.GUI.ZDropEditColumnStyleInfo amendmentReasonDropEditColumnStyleInfo = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			amendmentReasonDropEditColumnStyleInfo.ColumnName = JobDeclarationMessageSendingObject.Schema.ChangeAcknowledgementIndicator;
			amendmentReasonDropEditColumnStyleInfo.GroupName = Res.GetData("2988DA69-05EF-40CF-BDFF-92C32F730B4F", "Amendment Reason");
			amendmentReasonDropEditColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			amendmentReasonDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			amendmentReasonDropEditColumnStyleInfo.IsMandatory = true;
			this.MessageSendingObjectsGrid.ColumnStyles.Add(amendmentReasonDropEditColumnStyleInfo);

			ZTextBoxColumnStyleInfo amendmentTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			amendmentTextBoxColumnStyleInfo.ColumnName = JobDeclarationMessageSendingObject.Schema.VOCReason;
			amendmentTextBoxColumnStyleInfo.GroupName = Res.GetData("2988DA69-05EF-40CF-BDFF-92C32F730B4F", "Amendment Reason");
			amendmentTextBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(amendmentTextBoxColumnStyleInfo);

			ZTextBoxColumnStyleInfo entryTypeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			entryTypeTextBoxColumnStyleInfo.ColumnName = EU.Business.JobDeclarationMessageSendingObject.Schema.EntryType;
			entryTypeTextBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(entryTypeTextBoxColumnStyleInfo);

			ZTextBoxColumnStyleInfo referenceNoTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			referenceNoTextBoxColumnStyleInfo.ColumnName = EU.Business.JobDeclarationMessageSendingObject.Schema.BGMReference;
			referenceNoTextBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(referenceNoTextBoxColumnStyleInfo);
		}

		ZArchitecture.GUI.ZCheckBox waitForResponseCheckBox;
	}
}
