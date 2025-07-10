using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public partial class CountryRelatedFilterControl : ZUserControl
	{
		public CountryRelatedFilterControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var dataSource = DataSource as CountryRelatedFilter;
			if (dataSource != null)
			{
				switch (dataSource.Property2FieldType)
				{
					case FieldType.TextCodeFindBox:
						Property2FindBox.Enabled = true;
						Property2FindBox.Visible = true;
						Property2DropEdit.Enabled = false;
						Property2DropEdit.Visible = false;
						break;
					case FieldType.TextDropEdit:
						Property2FindBox.Enabled = false;
						Property2FindBox.Visible = false;
						Property2DropEdit.Enabled = true;
						Property2DropEdit.Visible = true;
						break;
					default:
						Property2FindBox.Enabled = false;
						Property2FindBox.Visible = false;
						Property2DropEdit.Enabled = false;
						Property2DropEdit.Visible = false;
						break;
				}
			}
		}
	}
}
