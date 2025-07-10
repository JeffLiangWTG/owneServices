using CargoWise.Types;

namespace Enterprise.Customs.EU.Business;

public class Error
{
	public Error(ZString error, ZBool isCritical)
	{
		ErrorText = error;
		IsCritical = isCritical;
	}

	public override bool Equals(object obj)
	{
		var otherError = obj as Error;
		if (otherError == null)
		{
			return false;
		}
		return ErrorText == otherError.ErrorText && IsCritical == otherError.IsCritical;
	}

	public override int GetHashCode()
	{
		return ErrorText.GetHashCode();
	}

	internal ZString ErrorText;
	internal ZBool IsCritical;
}
