using System;
using System.Collections.Generic;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(EntryInstructionGridColumnsLayout))]
sealed class EntryInstructionGridColumnsLayoutTest : GridColumnLayoutProviderAbstractTest<EntryInstructionGridColumnsLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(CusEntryInstruction.Schema.LocalReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 80),
		(CusEntryInstruction.Schema.LocalReferenceNumberDate, typeof(ZDateEditColumnStyleInfo), 80),
		(CusEntryInstruction.Schema.CEI_Style, typeof(ZDropEditColumnStyleInfo), 80),
		(CusEntryInstruction.Schema.CEI_SubStyle, typeof(ZDropEditColumnStyleInfo), 80),
		(CusEntryInstruction.Schema.CEI_Description, typeof(ZTextBoxColumnStyleInfo), 250),
		(CusEntryInstruction.Schema.ShippingBillNumber, typeof(ZTextBoxColumnStyleInfo), 80),
		(CusEntryInstruction.Schema.ShippingBillDate, typeof(ZDateEditColumnStyleInfo), 80)
	};

	protected override Type GridBoundEntityType => typeof(CusEntryInstruction);
}
