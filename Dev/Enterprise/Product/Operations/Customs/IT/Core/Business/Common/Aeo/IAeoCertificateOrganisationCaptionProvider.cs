namespace Enterprise.Customs.IT.Business;

public interface IAeoCertificateOrganisationCaptionProvider
{
	string SupplierCaption { get; }
	string ImporterCaption { get; }
	string DeclarantCaption { get; }
}
