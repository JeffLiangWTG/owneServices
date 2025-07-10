using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class ImportH1CommonSendMessageWrapper : EntryHeaderCommonSendMessageWrapper, IH1CommonImportDataProvider
{
	public ImportH1CommonSendMessageWrapper(CusEntryHeader entryHeader, ICertificateProvider certificate) : base(entryHeader, certificate)
	{
	}

	const string OperationCodeA = "A";
	const string OperationCodeM = "M";

	public ZString Operation => entryHeader.MovementReferenceNumber.IsEmpty ? OperationCodeA : OperationCodeM;

	public ZString CustomsOfficeOfImport => declaration.JE_CustomsOffice;

	public ZString CustomOfficeOfPresentation
	{
		get
		{
			if (officeOfPresentation == null)
			{
				officeOfPresentation = new CachedProperty<ZString>(declaration.Factory, declaration.GetPresentationCustomsOffice);
			}
			return officeOfPresentation.Value;
		}
	}

	CachedProperty<ZString> officeOfPresentation;

	public IH1PartyProviderWithAddress Importer => importer ??= ImportH1CommonImporterWrapper.New(declaration.ImporterDocumentaryAddress);
	ImportH1CommonImporterWrapper importer;

	public IPartyIdProviderWithContactPerson Declarant => declarant ??= ImportH1CommonDeclarantWrapperWithContactPerson.New(declaration);
	ImportH1CommonDeclarantWrapperWithContactPerson declarant;

	public ICommonRepresentativeWithContactPerson Representative => representative ??= ImportH1CommonRepresentativeWrapperWithContactPerson.New(declaration);
	ImportH1CommonRepresentativeWrapperWithContactPerson representative;
}
