using CargoWise.EntityFramework;
using Enterprise.Client.Rohlig.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.Rohlig.Bellin
{
	public partial class BellinExportForm : ZForm
	{
		public BellinExportForm(BellinExportGUIWrapper wrapper) : base(wrapper)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton, null);
			this.Wrapper = wrapper;
		}

		public override string FormCaption
		{
			get { return Wrapper.FormCaption; }
		}

		internal void ExportButton_Click(object sender, System.EventArgs e)
		{
			Wrapper.Export();
			((IZPropertyInfoObsolete)Wrapper.DateFromInfo).ReadOnly = false;
			((IZPropertyInfoObsolete)Wrapper.DateToInfo).ReadOnly = false;
		}

		readonly BellinExportGUIWrapper Wrapper;
	}
}
