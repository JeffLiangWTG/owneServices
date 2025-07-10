using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CommonGoodsItemWrapper : ICommonGoodsItem
	{
		public CommonGoodsItemWrapper(NctsCommonCargoDesc line)
		{
			this.line = Argument.NotNull(line, nameof(line));
		}

		public ZInt ItemNumber => line.BY_LineNo;

		public ZString CommodityCode => line.BY_HarmonisedTariff;

		public ZString TypeOfDeclaration => line.BY_Type;

		public ZString GoodsDescription => line.BY_Description;

		public ZString GoodsDescriptionLanguage => ZString.Empty;

		public ZDecimal GrossMass => line.GrossMassInKilograms;

		public ZDecimal NetMass => line.NetMassInKilograms;

		public ZInt DeclarationItemNumber => line.BY_DeclarationGoodsItemNumber;

		public ZShort BillSequenceNumber => line.Bill?.SequenceNumber ?? ZShort.Zero;

		public IReadOnlyCollection<ZString> Containers => Array.Empty<ZString>();

		public IReadOnlyCollection<IProducedDocumentCertificate> ProducedDocumentsCertificates => producedDocumentsCertificates ?? (producedDocumentsCertificates = line.SupportingDocuments.OfType<NctsSupportingDocument>().Select(s => new SupportingDocumentWrapper(s)).ToArray());
		IReadOnlyCollection<SupportingDocumentWrapper> producedDocumentsCertificates;

		public IReadOnlyCollection<IPackage> Packages => packages ?? (packages = Getpackages());
		IReadOnlyCollection<PackageWrapper> packages;

		protected virtual PackageWrapper[] Getpackages() => line.Packages.OfType<NctsPackage>().Select(p => new PackageWrapper(p)).ToArray();

		protected readonly NctsCommonCargoDesc line;
	}
}
