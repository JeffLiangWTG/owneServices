using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI;

public class QuarantineTabPageWithSubTabs : QuarantineTabPage
{
	KBindingSource BindingSource { get; }

	public QuarantineTabPageWithSubTabs(QuarantineExDocHeader exDocHeader, KBindingSource bindingSource, CurrencyManager currencyManager)
		: base(exDocHeader, currencyManager)
	{
		BindingSource = bindingSource;
	}

	public Func<QuarantineExDocHeader, KBindingSource, CurrencyManager, IEnumerable<QuarantineTabPage>> TabsPages { get; set; }

	protected override void OnLazyCreateControls(EventArgs e)
	{
		if (Controls.Count == 0 && TabsPages != null)
		{
			var tabControl = new ZTabControl();
			tabControl.Dock = DockStyle.Fill;

			var isNEXDOCSActive = EXDocHeader?.IsNEXDOCSActive ?? false;

			foreach (var tabPage in TabsPages(EXDocHeader, BindingSource, CurrencyManager))
			{
				tabControl.TabPages.Add(tabPage);
				tabPage.Text = tabPage.GetCaptionFunc(isNEXDOCSActive);
				tabPage.TabVisible = tabPage.IsVisibleFunc(isNEXDOCSActive, EXDocHeader);
			}

			Controls.Add(tabControl);
		}

		base.OnLazyCreateControls(e);
	}
}
