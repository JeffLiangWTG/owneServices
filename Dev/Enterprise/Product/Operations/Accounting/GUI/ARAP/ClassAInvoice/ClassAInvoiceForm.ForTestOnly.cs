#if DEBUG

using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class ClassAInvoiceForm
	{
		public ZDropEdit ComplianceSubTypeDropEdit_ForTestOnly
		{
			get { return ComplianceSubTypeDropEdit; }
			set { ComplianceSubTypeDropEdit = value; }
		}

		public ZDateEdit ComplianceDocDateEdit_ForTestOnly
		{
			get { return ComplianceDocDateEdit; }
			set { ComplianceDocDateEdit = value; }
		}

		public ZTextBox ClassAInvoiceNumTextBox_ForTestOnly
		{
			get { return ClassAInvoiceNumTextBox; }
			set { ClassAInvoiceNumTextBox = value; }
		}

		public void OnPostButtonClick_ForTestOnly(object sender, EventArgs e)
		{
			OnPostButtonClick(sender, e);
		}
	}
}

#endif
