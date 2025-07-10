using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public partial class EntryLineTaxAndFeeUserControl : ZUserControl
{
	public EntryLineTaxAndFeeUserControl()
	{
		InitializeComponent();
	}

	protected override void OnCurrentDataItemChanged(EventArgs e)
	{
		base.OnCurrentDataItemChanged(e);
		var entryLine = CurrentDataItem as Business.Declaration.CusEntryLine;
		var entryHeader = entryLine?.Header as Business.Declaration.CusEntryHeader;
		entryHeader?.SetAllEntryLinesReadOnly();
	}
}
