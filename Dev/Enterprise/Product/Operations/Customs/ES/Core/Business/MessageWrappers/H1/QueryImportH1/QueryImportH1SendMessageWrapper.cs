using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class QueryImportH1SendMessageWrapper : ImportH1CommonSendMessageWrapper, IQueryImportH1MessageDataProvider
{
	public QueryImportH1SendMessageWrapper(CusEntryHeader entryHeader, ICertificateProvider certificate, bool isATC) : base(entryHeader, certificate)
	{
		ATC = isATC ? ATCDataRequest : ZString.Empty;
	}
	const string ATCDataRequest = "S";

	public IH1CommonMRN DataProviderMRN => dataProviderMRN ??= new ImportH1CommonMRNWrapper(entryHeader);
	ImportH1CommonMRNWrapper dataProviderMRN;

	public ZString CustomsRegistrationNumber => ZString.Empty;

	public ZString ATC { get; }
}
