using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	[SuppressControlRequiresTextBasher]
	public partial class AdditionalProcedureCodesUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
	{
		public AdditionalProcedureCodesUserControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		void AdditionalProcedureCodesEditButton_Click(object sender, System.EventArgs e)
		{
			var invoiceLine = (JobComInvoiceLine)CurrentDataItem;
			if (invoiceLine != null)
			{
				if (!invoiceLine.JI_FormattedProcedure.IsEmpty)
				{
					ZFormModaliser.ShowDialogAndDispose(new AdditionalProcedureCodeForm(invoiceLine));
				}
				else
				{
					Globals.Message.ShowWarning(invoiceLine.ProcedureMustBeEnteredForAdditionalProceduresSelectionErrorMessage, Res.GetString("EEA991DD-478A-4C3C-B9BB-245810EA9B9D", "Error"));
				}
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		public Control Host => this;

		public IControlExtensionCollection Extensions { get; }

		public string ResourceStringBindingMember => nameof(JobComInvoiceLine.AdditionalProcedureCodesAsString);

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
