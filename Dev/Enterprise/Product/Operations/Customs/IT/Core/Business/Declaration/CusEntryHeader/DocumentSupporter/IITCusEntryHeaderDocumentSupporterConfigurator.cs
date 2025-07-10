using System.ComponentModel;

namespace Enterprise.Customs.IT.Business.Declaration;

public interface IITCusEntryHeaderDocumentSupporterConfigurator
{
	CancelEventArgs Configure(JobDeclarationSadDocumentSupporter supporter);
}
