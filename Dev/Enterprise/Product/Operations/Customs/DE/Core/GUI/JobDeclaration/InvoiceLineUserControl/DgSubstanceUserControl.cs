using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class DgSubstanceUserControl : ZUserControl, IResourceStringBindingMember
	{
		public DgSubstanceUserControl()
		{
			InitializeComponent();
		}

		void MoreButton_Click(object sender, EventArgs e) => MoreButtonClick?.Invoke(sender, e);

		public string ResourceStringBindingMember => nameof(JobComInvoiceLine.DgSubstance);

		public event EventHandler MoreButtonClick;
	}
}
