using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business;

sealed class Ucc6ExportPreviousDocumentReferenceNumberValidator : IPreviousDocumentReferenceNumberValidator
{
	public Ucc6ExportPreviousDocumentReferenceNumberValidator(BusinessObjectFactory factory, IPreviousDocumentReferenceNumberProvider previousDocumentReferenceNumberProvider)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.previousDocumentReferenceNumberProvider = Argument.NotNull(previousDocumentReferenceNumberProvider, nameof(previousDocumentReferenceNumberProvider));
	}

	readonly IPreviousDocumentReferenceNumberProvider previousDocumentReferenceNumberProvider;
	readonly BusinessObjectFactory factory;

	void IPreviousDocumentReferenceNumberValidator.CheckReferenceNumber()
	{
		var referenceNumberProvider = previousDocumentReferenceNumberProvider.ReferenceNumberProvider;
		var documentType = referenceNumberProvider.DocumentType;

		if (!documentType.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(referenceNumberProvider.ReferenceNumberInfo);
		}

		IPreviousDocumentRefNumberValidationStrategy validationStrategy = documentType.ToString() switch
		{
			UniversalReferenceConstants.RefCusCodeListTypes.DeclarationOrNotificationMrn => new Ucc6ExportPrevDocNMRNRefNumberValidationStrategy(factory),
			UniversalReferenceConstants.RefCusCodeListTypes.TemporaryStorageDeclaration => new Ucc6ExportPrevDocN337RefNumberValidationStrategy(factory),
			_ => null
		};

		validationStrategy?.CheckReferenceNumber(referenceNumberProvider.ReferenceNumberInfo as ZPropertyInfoString);
	}
}
