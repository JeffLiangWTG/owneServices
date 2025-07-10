using System;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DK.GUI.Testing
{
	[TestedType(typeof(CustomsBrokerageUserControl))]
	sealed class CustomsBrokerageUserControlTest : CustomsBrokerageUserControlAbstractTest<CustomsBrokerageUserControl>
	{
		protected override Type JobDeclarationUserControlType => typeof(JobDeclarationUserControl);

		protected override Type ImportSupplierHeaderUserControlType => typeof(EUImportSupplierHeaderUserControl);

		protected override Type ExportSupplierHeaderUserControlType => typeof(EUExportSupplierHeaderUserControl);

		protected override Type ImportInvoiceLineUserControlType => typeof(ImportInvoiceLineUserControl);

		protected override Type ExportInvoiceLineUserControlType => typeof(ExportInvoiceLineUserControl);

		protected override Type MessageUserControlType => typeof(EntryMessageUserControl);

		protected override Type EntryInstructionDetailsUserControlType => typeof(EntryInstructionDetailsUserControl);
	}
}
