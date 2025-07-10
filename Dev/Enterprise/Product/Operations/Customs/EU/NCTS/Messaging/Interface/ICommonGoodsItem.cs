using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface ICommonGoodsItem
	{
		ZInt ItemNumber { get; }
		ZString CommodityCode { get; }
		ZString TypeOfDeclaration { get; }
		ZString GoodsDescription { get; }
		ZString GoodsDescriptionLanguage { get; }
		ZDecimal GrossMass { get; }
		ZDecimal NetMass { get; }
		IReadOnlyCollection<ZString> Containers { get; }
		IReadOnlyCollection<IProducedDocumentCertificate> ProducedDocumentsCertificates { get; }
		IReadOnlyCollection<IPackage> Packages { get; }
	}
}
