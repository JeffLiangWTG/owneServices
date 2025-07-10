using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class QuarantineTabPage<T> : QuarantineTabPage where T : ZUserControl
	{
		public QuarantineTabPage(QuarantineExDocHeader exDocHeader, KBindingSource bindingSource, CurrencyManager currencyManager)
			: base(exDocHeader, currencyManager)
		{
			Name = typeof(T).Name.Replace(nameof(UserControl), nameof(TabPage));
			this.bindingSource = bindingSource;
		}

		readonly KBindingSource bindingSource;

		protected override void OnLazyCreateControls(EventArgs e)
		{
			if (Controls.Count == 0)
			{
				var ctr = (ZUserControl)Activator.CreateInstance(typeof(T));
				ctr.Name = ctr.GetType().Name;
				ctr.Dock = DockStyle.Fill;

				bindingSource?.SetBindingMember(ctr, nameof(JobDeclaration.Invoices));
				Controls.Add(ctr);
			}

			base.OnLazyCreateControls(e);
		}
	}
}
