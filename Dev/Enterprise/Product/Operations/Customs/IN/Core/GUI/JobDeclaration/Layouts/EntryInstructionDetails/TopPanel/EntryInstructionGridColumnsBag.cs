using System;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

sealed class EntryInstructionGridColumnsBag
{
	EntryInstructionGridColumnsBag()
	{
		LocalReferenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(nameof(CusEntryInstruction.Schema.LocalReferenceNumber), 80, info => info.IsMandatory = true);
		LocalReferenceNumberDateEditColumn = new GridColumnReference<ZDateEditColumnStyleInfo>(nameof(CusEntryInstruction.Schema.LocalReferenceNumberDate), 80, info => info.IsMandatory = true);
		CEI_StyleDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(nameof(CusEntryInstruction.Schema.CEI_Style), 80);
		CEI_SubStyleDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(nameof(CusEntryInstruction.Schema.CEI_SubStyle), 80);
		CEI_DescriptionTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(nameof(CusEntryInstruction.Schema.CEI_Description), 250);
		ShippingBillNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(nameof(CusEntryInstruction.Schema.ShippingBillNumber), 80, info => info.IsReadOnly = true);
		ShippingBillDateDateEditColumn = new GridColumnReference<ZDateEditColumnStyleInfo>(nameof(CusEntryInstruction.Schema.ShippingBillDate), 80, info => info.IsReadOnly = true);
	}

	public static EntryInstructionGridColumnsBag Instance => instance ??= new EntryInstructionGridColumnsBag();

	public IGridColumnReference LocalReferenceNumberTextBoxColumn { get; }
	public IGridColumnReference LocalReferenceNumberDateEditColumn { get; }
	public IGridColumnReference CEI_StyleDropEditColumn { get; }
	public IGridColumnReference CEI_SubStyleDropEditColumn { get; }
	public IGridColumnReference CEI_DescriptionTextBoxColumn { get; }
	public IGridColumnReference ShippingBillNumberTextBoxColumn { get; }
	public IGridColumnReference ShippingBillDateDateEditColumn { get; }

	[ThreadStatic]
	static EntryInstructionGridColumnsBag instance;
}
