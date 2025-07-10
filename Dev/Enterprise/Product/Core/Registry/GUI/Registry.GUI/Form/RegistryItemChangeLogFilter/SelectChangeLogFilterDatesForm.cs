using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class SelectChangeLogFilterDatesForm : ZChildForm
	{
		internal FilterRegistryChangeLogsByDate FilterParameters
		{
			get
			{
				return this.BusinessEntity as FilterRegistryChangeLogsByDate;
			}
		}

		public SelectChangeLogFilterDatesForm() : base(new FilterRegistryChangeLogsByDate())
		{
			InitializeComponent();
		}

		void okButton_Click(object sender, EventArgs e)
		{
			FilterParameters.Validation.ValidateAll();
			if (!FilterParameters.HasErrors())
			{
				FindForm().DialogResult = DialogResult.OK;
			}
		}
	}
}
