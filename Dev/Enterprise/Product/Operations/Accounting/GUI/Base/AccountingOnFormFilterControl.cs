using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AccountingOnFormFilterControl : ZFilterStripControl
	{
		public AccountingOnFormFilterControl(BusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			SetToolStripsIncludingChildrenToReadOnly(this);
		}

		void SetToolStripsIncludingChildrenToReadOnly(Control control)
		{
			ToolStrip toolStrip = control as ToolStrip;
			if (toolStrip != null)
			{
				foreach (ToolStripItem toolStripItem in toolStrip.Items)
				{
					TypeDescriptor.AddAttributes(toolStripItem, new CanBeReadOnlyUIAttribute());
				}
			}
			else
			{
				foreach (Control child in control.Controls)
				{
					SetToolStripsIncludingChildrenToReadOnly(child);
				}
			}
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return (ZFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingOnFormFilterStrip>());
		}

		protected override int MaxFilterStripPanelHeight
		{
			get
			{
				return MaxFilterStripPanelHeight_internalValue;
			}
		}

		public void SetMaxFilterStripPanelHeight(int value)
		{
			MaxFilterStripPanelHeight_internalValue = value;
		}

		internal int MaxFilterStripPanelHeight_internalValue;

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion

	}
}

