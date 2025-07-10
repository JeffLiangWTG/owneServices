using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class AdditionalSupplementaryCodesUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
	{
		public AdditionalSupplementaryCodesUserControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		void AdditionalSupplementaryCodesEditButton_Click(object sender, System.EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				AdditionalSupplementaryCodesForm.ShowDialog((TemporaryStoragePackedItem)CurrentDataItem);
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		Control IExtendedControl.Host => this;

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; }

		string IResourceStringBindingMember.ResourceStringBindingMember => nameof(TemporaryStoragePackedItem.API_Supplements);

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			Extensions.Dispose();
			base.Dispose(disposing);
		}
	}
}
