using Enterprise.Customs.CA.DIF.Business;
using Enterprise.Customs.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DIF.GUI
{
	public sealed class MessageSendingForm : MessageSendingFormBase<MessageSendingAction, DIFDocument>
	{
		public MessageSendingForm(MessageSendingActionCollection coll) : base(coll)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				SetupNewColumns();
			}
		}

		public override string FormCaption
		{
			get { return Res.GetString("155D65F8-43AA-410A-A48E-FFBB7800710D", "Send DIF Messages"); }
		}

		void SetupNewColumns()
		{
			this.SuspendLayout();
			var zCheckBoxColumnStyleInfo3 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			zCheckBoxColumnStyleInfo3.Caption = Res.GetString("F3E5CEC9-3CF4-47FE-841B-A8D130FDCDBD", "Send Amendment");
			zCheckBoxColumnStyleInfo3.ColumnName = MessageSendingAction.Schema.SendAmendment;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(99);
			this.DocumentsGrid.ColumnStyles.Insert(1, zCheckBoxColumnStyleInfo3);
			this.DocumentsGrid.SetColumnCaption(MessageSendingAction.Schema.Send, Res.GetString("10E189C7-2273-4858-B511-77D6566E6B2F", "Send Add/Change"));
			// 
			// MessageSendingForm
			// 
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
