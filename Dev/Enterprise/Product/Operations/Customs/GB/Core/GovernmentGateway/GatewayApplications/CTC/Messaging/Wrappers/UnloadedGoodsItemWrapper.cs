using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	internal class UnloadedGoodsItemWrapper : IUnloadedGoodsItem
	{
		public UnloadedGoodsItemWrapper(NctsArrivalAndUnloadingCargoDesc line)
		{
			this.line = Argument.NotNull(line, nameof(line));
		}

		public ZString ItemNumber => line.BY_LineNo.ToString();

		public ZString CommodityCode => line.BY_HarmonisedTariff;

		public ZString GoodsDescription => line.BY_Description;

		public ZString GoodsDescriptionLanguage => ZString.Empty;

		public ZDecimal GrossMass => line.GrossMassInKilograms;

		public ZDecimal NetMass => line.NetMassInKilograms;

		public IEnumerable<IProducedDocumentCertificate> ProducedDocumentsCertificates => producedDocumentsCertificates ?? (producedDocumentsCertificates = line.SupportingDocuments.OfType<NctsSupportingDocument>().Select(s => new SupportingDocumentWrapper(s)).ToArray());
		SupportingDocumentWrapper[] producedDocumentsCertificates;

		public IEnumerable<IPackage> Packages => packages ?? (packages = line.Packages.OfType<NctsPackage>().Select(p => new PackageWrapper(p)).ToArray());
		PackageWrapper[] packages;

		public IEnumerable<ZString> Containers => containers ?? (containers = line.Containers.Select(x => x.ContainerNumber).ToArray());
		ZString[] containers;

		public IEnumerable<IControlResult> ResultsOfControl => line.ResultsOfControlCollection.OfType<CusAddInfo<ResultsOfControlAddInfo>>().Select(x => new ControlResultWrapper(x.Data)).ToArray();

		public IEnumerable<ISgiCode> SGICodes => line.SgiCodes.ToList();

		public bool IsNew => line.IsNew;

		public bool IsMissing => line.IsMissing;

		public bool HasDifferences => line.HasDifferences;

		readonly NctsArrivalAndUnloadingCargoDesc line;
	}
}
