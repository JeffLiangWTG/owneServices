using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	[SuppressControlRequiresTextBasher]
	public partial class AdditionalProcedureCodesUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
	{
		public AdditionalProcedureCodesUserControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		void AdditionalProcedureCodesEditButton_Click(object sender, EventArgs e)
		{
			var bill = (AsycudaBill)CurrentDataItem;

			if (bill != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new AdditionalProcedureCodeForm(bill));
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		public Control Host => this;

		public IControlExtensionCollection Extensions { get; }

		public string ResourceStringBindingMember => nameof(AsycudaBill.AdditionalProcedureCodesAsString);

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
