using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

public class UnifiedDeclarationIrispPartsAggregator
{
	public UnifiedDeclarationIrispPartsAggregator(UnifiedDeclarationIrisp irisp)
	{
		this.irisp = Argument.NotNull(irisp, nameof(irisp));
	}

	readonly UnifiedDeclarationIrisp irisp;

	public ZString GetTextForMessage(ZString progressiveAnnualNumber)
	{
		var relatedResponseMessages = irisp.ResponseMessages.Where(x => x.DeclarationNumber == progressiveAnnualNumber).Select(x => x.InnerText).ToArray();
		if (relatedResponseMessages.Any())
		{
			return new ZStringBuilder()
				.AppendLine(irisp.Header.InnerText)
				.AppendLine(irisp.IdocElaborationInfo.InnerText)
				.AppendLine(irisp.ControlResult.InnerText)
				.Append(new ZStringBuilder(relatedResponseMessages))
				.ToString();
		}
		return ZString.Empty;
	}
}
