using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;

namespace Enterprise.Accounting.GUI.ComplianceReport.HMRC
{
	public sealed class FormProxyImpl : FraudPreventionDataProvider.IFormProxy
	{
		readonly Form form;

		public FormProxyImpl()
		{
		}

		public FormProxyImpl(Form form)
		{
			this.form = form;
		}

		public IEnumerable<FraudPreventionDataProvider.IFormProxy> GetApplicationOpenFormProxies()
		{
			return ZApplication.GetOpenForms().Select(form => new FormProxyImpl(form));
		}

		public bool ContainsFocus => form.ContainsFocus;

		public int Width => form.Width;

		public int Height => form.Height;
	}
}
