using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IIMLine : ILineCommon
{
	ZString Preferences { get; }
	IEnumerable<ZString> Quotas { get; }
	ZDecimal? ItemPriceEuro { get; }
	ZString EvaluationMethod { get; }
	ZDecimal? AdjustmentInEuro { get; }
	IPackage Package { get; }
	IIMLineSpecialMentionGroup SpecialMentionGroup { get; }
}
