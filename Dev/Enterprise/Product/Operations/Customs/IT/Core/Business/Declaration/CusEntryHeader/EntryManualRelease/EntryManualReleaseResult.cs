using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class EntryManualReleaseResult
{
	public EntryManualReleaseResult(bool canDoManualRelease, string errorMessage)
	{
		CanDoManualRelease = canDoManualRelease;
		this.errorMessage = errorMessage;
	}
	readonly string errorMessage;

	public bool CanDoManualRelease { get; }

	public string ErrorMessage => errorMessage ?? ZString.Empty;
}
