using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public interface IUnloadedGoodsItem
	{
		ZString ItemNumber { get; }
		ZString CommodityCode { get; }
		ZString GoodsDescription { get; }
		ZString GoodsDescriptionLanguage { get; }
		ZDecimal GrossMass { get; }
		ZDecimal NetMass { get; }
		IEnumerable<IProducedDocumentCertificate> ProducedDocumentsCertificates { get; }
		IEnumerable<IPackage> Packages { get; }
		IEnumerable<ZString> Containers { get; }
		IEnumerable<IControlResult> ResultsOfControl { get; }
		IEnumerable<ISgiCode> SGICodes { get; }
		bool IsNew { get; }
		bool IsMissing { get; }
		bool HasDifferences { get; }
	}
}
