using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business;

public interface ISupportingDocumentsWithHarbourRateProvider
{
	ISupportingDocumentsProvider SupportingDocumentsMaster { get; }

	IHarbourRateProvider HarbourRateProvider { get; }
}
