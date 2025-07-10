using CargoWise.Types;

namespace Enterprise.Customs.AE.Business;

sealed class SyntaxErrorProvider : ISyntaxErrorProvider
{
	public SyntaxErrorProvider(string syntaxErrorCode, string dataElementPosition, string componentPosition)
	{
		SyntaxErrorCode = syntaxErrorCode;
		ErrorDataElementPosition = dataElementPosition;
		ErrorDataElementComponentPosition = componentPosition;
	}

	public ZString SyntaxErrorCode { get; }

	public ZString ErrorDataElementPosition { get; }

	public ZString ErrorDataElementComponentPosition { get; }
}
