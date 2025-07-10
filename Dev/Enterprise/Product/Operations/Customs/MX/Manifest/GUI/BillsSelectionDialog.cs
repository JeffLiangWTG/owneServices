using System;
using Enterprise.Customs.MX.Manifest.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.GUI
{
	public partial class BillsSelectionDialog : ASYCUDA.GUI.AsycudaItemSelectionDialog
	{
		[Obsolete("This constructor is just for the designer")]
		public BillsSelectionDialog()
		{
			InitializeComponent();
		}

		public BillsSelectionDialog(MXMessageChooser messageChooser, string itemsType)
			: base(messageChooser, itemsType)
		{
			InitializeComponent();
			ItemsGroupBox.Text = ResString.GetMultilingualString("04C9462C-941D-4F12-9805-CC881745D667", "Manifest - {0}", BusinessEntity.Header.AMA_JobReference);
			Name = FormattableString.Invariant($"MessageTo{messageChooser.ActionType}Dialog");
			Text = ResString.GetMultilingualString("31C91395-E509-447E-A61E-C96FB6F3210D", "Message to {0}", messageChooser.ActionType);
			ItemsGrid.SetColumnCaption("Description", Text);
			ReasonDropEdit.Visible = messageChooser.IsChangeOrCancellation && messageChooser.IsSeaMode;
		}

		public new MXMessageChooser BusinessEntity => (MXMessageChooser)base.BusinessEntity;
	}
}
