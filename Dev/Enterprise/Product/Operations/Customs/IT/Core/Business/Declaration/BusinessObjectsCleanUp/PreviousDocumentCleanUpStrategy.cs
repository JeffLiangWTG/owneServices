using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

class PreviousDocumentCleanUpStrategy : ICleanUpStrategy
{
	public PreviousDocumentCleanUpStrategy(PreviousDocument previousDocument)
	{
		PreviousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		Declaration = Argument.NotNull(previousDocument.Declaration, nameof(previousDocument.Declaration));
	}

	void ICleanUpStrategy.CleanUp() => CleanupCore();

	protected JobDeclaration Declaration { get; }
	protected PreviousDocument PreviousDocument { get; }

	protected virtual void CleanupCore()
	{
		if (!Declaration.IsUCC6)
		{
			PreviousDocument.CSI_PackType = ZString.Empty;
		}
	}
}
