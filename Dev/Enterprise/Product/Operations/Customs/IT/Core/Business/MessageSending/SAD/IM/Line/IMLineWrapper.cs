using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public abstract class IMLineWrapper : SADLineCommonWrapper, IIMLine
{
	protected IMLineWrapper(CusEntryLine entryLine)
		: base(entryLine)
	{
	}

	public ZString Preferences => PreferencesCore;
	protected abstract ZString PreferencesCore { get; }

	public IEnumerable<ZString> Quotas => QuotasCore;
	protected abstract IEnumerable<ZString> QuotasCore { get; }

	public ZDecimal? ItemPriceEuro => ItemPriceEuroCore;
	protected abstract ZDecimal? ItemPriceEuroCore { get; }

	public ZDecimal? AdjustmentInEuro => AdjustmentInEuroCore;
	protected abstract ZDecimal? AdjustmentInEuroCore { get; }

	public ZString EvaluationMethod => entryLine.ValuationMethod;

	public IPackage Package => new SADLinePackageWrapper(entryLine);

	public IIMLineSpecialMentionGroup SpecialMentionGroup => new IMLineSpecialMentionGroupWrapper(entryLine);
}
