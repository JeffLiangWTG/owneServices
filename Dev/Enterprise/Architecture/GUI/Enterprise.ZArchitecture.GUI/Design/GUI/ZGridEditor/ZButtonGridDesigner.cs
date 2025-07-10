using System;
using System.Collections;
using System.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Design
{
	internal class ZButtonGridDesigner : ZGridDesigner
	{
		protected override ArrayList ColumnStyles
		{
			get { return InnerGrid.ColumnStyles; }
		}

		public override PropertyDescriptor PropertyDescriptor
		{
			get { return TypeDescriptor.GetProperties(Control).Find("InnerGrid", false); }
		}

		ZGrid InnerGrid
		{
			get
			{
				if (Control is IButtonGrid)
				{
					return ((IButtonGrid)Control).InnerGrid;
				}
				else
				{
					throw new Exception("Unknown type of button grid!");
				}
			}
		}
	}
}
