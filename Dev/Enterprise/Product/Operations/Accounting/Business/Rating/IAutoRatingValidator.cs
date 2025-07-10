using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	public interface IAutoRatingValidator
	{
		ZString GetAutoratingNotPermittedReason();
	}
}
