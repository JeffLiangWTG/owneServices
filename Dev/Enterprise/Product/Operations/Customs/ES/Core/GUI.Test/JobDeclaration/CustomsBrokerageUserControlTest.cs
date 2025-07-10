using System;
using Enterprise.Customs.EU.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(CustomsBrokerageUserControl))]
	class CustomsBrokerageUserControlTest : CustomsBrokerageUserControlAbstractTest<CustomsBrokerageUserControl>
	{
		protected override Type JobDeclarationUserControlType => typeof(JobDeclarationUserControl);

		protected override Type ImportSupplierHeaderUserControlType => typeof(ImportSupplierHeaderUserControl);

		protected override Type ExportSupplierHeaderUserControlType => typeof(ExportSupplierHeaderUserControl);

		protected override Type ImportInvoiceLineUserControlType => typeof(ImportInvoiceLineUserControl);

		protected override Type ExportInvoiceLineUserControlType => typeof(ExportInvoiceLineUserControl);

		protected override Type MiscOptionsUserControlType => typeof(EU.GUI.MiscOptionsUserControl);

		protected override Type MessageUserControlType => typeof(EntryMessageUserControl);

		protected override Type EntryInstructionDetailsUserControlType => typeof(EntryInstructionDetailsUserControl);
	}
}
