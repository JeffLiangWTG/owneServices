using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(AdditionalInformationUserControl))]
sealed class AdditionalInformationUserControlTest : TestCaseWithFactory
{
	public void TestInvoiceLineImportColumns() => AssertInvoiceLineColumns(Customs.Business.JobMessageTypeList.Codes.Import, invoiceLineImportControls, "FilteredInvoiceLines.AdditionalInformations");

	public void TestInvoiceLineExportColumns() => AssertInvoiceLineColumns(Customs.Business.JobMessageTypeList.Codes.Export, invoiceLineExportControls, "FilteredInvoiceLines.AdditionalInformations");

	public void TestEntryInstructionExportColumns() => AssertInvoiceLineColumns(Customs.Business.JobMessageTypeList.Codes.Export, entryInstructionExportControls, "CustomsEntryInstructions.AdditionalInformations");

	void AssertInvoiceLineColumns(string messageType, IEnumerable<(string columnName, Type columnType)> controls, string dataMember) => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = messageType;
		Declaration.CustomsEntryInstructions.AddNew().AdditionalInformations.AddNew();
		Declaration.Invoices.AddNew().InvoiceLines.AddNew().AdditionalInformations.AddNew();

		using (var form = new ZForm(Declaration))
		using (var control = new AdditionalInformationUserControl())
		{
			control.SetDataBinding(Declaration, dataMember);
			form.Controls.Add(control);
			form.Show();
			var additionalInformationGrid = control.AdditionalInformationGrid;
			var additionalInformationGridColumsStyles = additionalInformationGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			var index = 0;
			foreach (var currentItem in controls)
			{
				UserControlTestHelper.AssertColumnStyles(additionalInformationGridColumsStyles, currentItem.columnName, index, currentItem.columnType);
				index++;
			}
		}
	});

	readonly IEnumerable<(string, Type)> invoiceLineImportControls = new (string, Type)[]
	{
			(AdditionalInformation.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo)),
			(AdditionalInformation.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo)),
			(AdditionalInformation.Schema.CSI_ReferenceNumber, typeof(ZDropEditColumnStyleInfo))
	};

	readonly IEnumerable<(string, Type)> invoiceLineExportControls = new (string, Type)[]
	{
			(AdditionalInformation.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo)),
			(AdditionalInformation.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo)),
			(AdditionalInformation.Schema.CSI_Description, typeof(ZMultiControlColumnStyleInfo))
	};

	readonly IEnumerable<(string, Type)> entryInstructionExportControls = new (string, Type)[]
	{
			(AdditionalInformation.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo)),
			(AdditionalInformation.Schema.CSI_Description, typeof(ZMultiControlColumnStyleInfo))
	};

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;
}
