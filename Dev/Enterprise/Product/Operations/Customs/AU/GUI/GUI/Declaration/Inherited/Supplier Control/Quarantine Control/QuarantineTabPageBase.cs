using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI;

public class QuarantineTabPage : BaseDeclarationTabPage
{
	public QuarantineTabPage(QuarantineExDocHeader exDocHeader, CurrencyManager currencyManager)
		: base()
	{
		this.EXDocHeader = exDocHeader;
		this.CurrencyManager = currencyManager;

		HookEvents();
	}

	protected QuarantineExDocHeader EXDocHeader { get; private set; }
	protected CurrencyManager CurrencyManager { get; }

	void HookEvents()
	{
		if (EXDocHeader != null)
		{
			EXDocHeader.QH_ProduceTypeInfo.ValueChanged += QH_ProduceTypeInfo_ValueChanged;
		}

		if (CurrencyManager != null)
		{
			CurrencyManager.CurrentItemChanged += CurrencyManager_CurrentItemChanged;
		}
	}

	void UnHookEvents()
	{
		if (EXDocHeader != null)
		{
			EXDocHeader.QH_ProduceTypeInfo.ValueChanged -= QH_ProduceTypeInfo_ValueChanged;
		}

		if (CurrencyManager != null)
		{
			CurrencyManager.CurrentItemChanged -= CurrencyManager_CurrentItemChanged;
		}
	}

	void QH_ProduceTypeInfo_ValueChanged(object sender, EventArgs e)
	{
		UpdateVisibleAndText();
	}

	void CurrencyManager_CurrentItemChanged(object sender, EventArgs e)
	{
		UnHookEvents();

		EXDocHeader = (CurrencyManager?.GetCurrent() as JobComInvoiceHeader)?.QuarantineExDocHeader;
		UpdateVisibleAndText();

		HookEvents();
	}

	void UpdateVisibleAndText()
	{
		if (EXDocHeader != null)
		{
			var isActiveOnNewHeader = EXDocHeader.IsNEXDOCSActive;

			TabVisible = IsVisibleFunc?.Invoke(isActiveOnNewHeader, EXDocHeader) ?? TabVisible;
			Text = GetCaptionFunc?.Invoke(isActiveOnNewHeader) ?? Text;
		}
	}

	public Func<bool, QuarantineExDocHeader, bool> IsVisibleFunc { get; set; }

	public Func<bool, string> GetCaptionFunc { get; set; }

	protected override void Dispose(bool disposing)
	{
		UnHookEvents();
		base.Dispose(disposing);
	}
}
