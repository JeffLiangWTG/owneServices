using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class QuotaWithCheckLinkUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
	{
		public QuotaWithCheckLinkUserControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);

			var descriptionBox = QuotaDropEdit.Controls.Find("DescriptionBox", true).FirstOrDefault();
			descriptionBox.AllowOutsideOfParent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		void CheckQuotaBalanceLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var url = EU.Registry.EUCustomsDataRegistry.Instance.QuotaBalanceURL.Value;
			var lang = GlbStaff.CurrentUser.Language;
			var quotaCode = (ZString)QuotaDropEdit.Text;
			if (quotaCode.Length > 6)
			{
				quotaCode = string.Empty;
			}
			url = url + (NoResString)"?" + (NoResString)"Lang=" + lang.ToLower() + (quotaCode.IsEmpty ? string.Empty : (NoResString)"&Code=" + quotaCode + (NoResString)"&Expand=true");
			WebUrlLauncher.Launch(url);
		}

		public Control Host => this;

		public IControlExtensionCollection Extensions { get; }

		public string ResourceStringBindingMember => nameof(JobComInvoiceLine.JI_ConcessionOrder);

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
