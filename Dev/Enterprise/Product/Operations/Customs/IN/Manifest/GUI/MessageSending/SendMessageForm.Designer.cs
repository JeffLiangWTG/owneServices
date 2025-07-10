using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.IN.Manifest.GUI;

partial class SendMessageForm
{
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	new void InitializeComponent()
	{
		this.MessageTypeTextLabel = new ZArchitecture.ZLabel();
		this.MessageTypeTextLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.MessageTypeTextLabel.AutoSize = true;
		this.MessageTypeTextLabel.Name = "MessageTypeTextLabel";
		this.MessageTypeTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 40, true);
		this.BindingSource.SetBindingMember(this.MessageTypeTextLabel, "SendingObjectsCollection.MessageTypeText");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Manifest.Business.ManifestMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IN.Manifest.Business.ManifestMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).MessageTypeText)));
		this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageTypeTextLabel, false);
		this.messageSendingObjectsGroupBox.Controls.Add(this.MessageTypeTextLabel);

		this.components = new System.ComponentModel.Container();
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.ClientSize = new System.Drawing.Size(800, 450);
		this.Text = "ManifestSendMessageForm";
	}

	protected ZArchitecture.ZLabel MessageTypeTextLabel;

	#endregion
}
