using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

public abstract class UnifiedDeclarationNegativeResponseMessage : UnifiedDeclarationResponseMessage
{
	public IrregularityRecord Irregularity { get; protected set; }

	internal override void Load(ZString content)
	{
		base.Load(content);
		Irregularity = new IrregularityRecord();
		Irregularity.Load(Lines.ElementAt(2));
	}
}
