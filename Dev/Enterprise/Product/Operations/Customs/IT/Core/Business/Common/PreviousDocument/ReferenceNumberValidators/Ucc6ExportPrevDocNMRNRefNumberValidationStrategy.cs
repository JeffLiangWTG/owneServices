using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business;

sealed class Ucc6ExportPrevDocNMRNRefNumberValidationStrategy(BusinessObjectFactory factory) : IPreviousDocumentRefNumberValidationStrategy
{
	readonly BusinessObjectFactory factory = Argument.NotNull(factory, nameof(factory));

	void IPreviousDocumentRefNumberValidationStrategy.CheckReferenceNumber(ZPropertyInfoString referenceNumberPropertyInfo)
	{
		Argument.NotNull(referenceNumberPropertyInfo, nameof(referenceNumberPropertyInfo));

		var referenceNumber = referenceNumberPropertyInfo.Value;
		if (referenceNumber.IsEmpty)
		{
			return;
		}

		var nmrnFormatValidationMessage = ITPreviousDocumentValidationHelper.ValidateReferenceNumberFormatForNmrn(factory, referenceNumber);

		if (!string.IsNullOrEmpty(nmrnFormatValidationMessage))
		{
			referenceNumberPropertyInfo.AddMessageError(nmrnFormatValidationMessage);
		}
	}
}
