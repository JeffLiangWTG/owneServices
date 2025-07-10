using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IGuaranteeJobParent
	{
		// Have added this interface as it will be added for work item
		// WI00315149 - FR COD - guarantee helper - multiple jobs able to use guarantees - step 10
		// I will remove this comment in due course
		ZInt PackageCount { get; }

		ZDecimal AmountToBeGuaranteedInDeclarationCurrency { get; }
	}
}
