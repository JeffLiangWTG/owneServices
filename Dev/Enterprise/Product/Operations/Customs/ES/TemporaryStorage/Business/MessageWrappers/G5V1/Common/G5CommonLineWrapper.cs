using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class G5CommonLineWrapper : IG5CommonLine
	{
		public G5CommonLineWrapper(TemporaryStoragePackedItem tempItem)
		{
			item = Argument.NotNull(tempItem, nameof(tempItem));
			bill = item.Bill;
			tempHeader = bill.Header;
		}

		protected readonly TemporaryStoragePackedItem item;
		protected readonly TemporaryStorageBill bill;
		protected readonly TemporaryStorageHeader tempHeader;

		const int tarifCodeLength = 8;

		public ZString LineNumber => item.API_LineNo.ToString();

		public IG5PreviousDocument PreviousDocument
		{
			get
			{
				if (previousDocument == null)
				{
					var previousDoc = item.PreviousDocuments.FirstOrDefault() ?? bill.PreviousDocuments.FirstOrDefault();

					previousDocument = previousDoc != null ? new G5PreviousDocumentWrapper(tempHeader.AMA_CustomsOffice, previousDoc) : null;
				}
				return previousDocument;
			}
		}
		G5PreviousDocumentWrapper previousDocument;

		public ZInt PackagesNum => item.TotalPackageQuantity;

		public IReadOnlyCollection<IInternalPackageIdentificationCommon> Packages
		{
			get
			{
				if (packages == null)
				{
					var packagesList = new List<InternalPackageIdentificationCommonWrapper>();

					var packs = item.TemporaryStorageLinkPackages.Where(p => p.IsLinked).Select(p => (p.Package, p.PackQty));

					var packagingDetails = packs.GroupBy(x => new { x.Package.APA_MarksAndNumbers, x.Package.APA_PackUQ });
					foreach (var pack in packagingDetails)
					{
						var packqty = ZLong.Zero;
						foreach (var p in pack.ToArray())
						{
							packqty += p.PackQty;
						}
						packagesList.Add(new InternalPackageIdentificationCommonWrapper(pack.Key.APA_MarksAndNumbers, pack.Key.APA_PackUQ, packqty));
					}

					packages = packagesList.AsReadOnly();
				}
				return packages;
			}
		}
		IReadOnlyCollection<InternalPackageIdentificationCommonWrapper> packages;

		public ZDecimal GrossWeightInKG
		{
			get
			{
				var grossWeight = item.GrossWeightInKG;
				return grossWeight > 1 ? (ZDecimal)Math.Ceiling(grossWeight) : grossWeight;
			}
		}

		public IDocumentsCommon TransportDocument
		{
			get
			{
				if (transportDocument == null)
				{
					var doc = item.AdditionalInfos.Where(doc => doc.CSI_SubType == AdditionalDocList.Codes.TransportDocuments).FirstOrDefault();

					transportDocument = doc != null ? new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber) : null;
				}
				return transportDocument;
			}
		}
		DocumentCommonWrapper transportDocument;

		public ZString UCRCode => item.UCR;

		public ZString CommodityCode => item.API_Tariff.SubstringSafe(0, item.API_Tariff.Length < tarifCodeLength ? item.API_Tariff.Length : tarifCodeLength);

		public ZString GoodsDescription => item.API_GoodsDescription;

		public ZString CusCode => item.API_ChemicalSubstanceCode;

		public IReadOnlyCollection<IG5TransportEquipment> TransportEquipments
		{
			get
			{
				if (transportEquipments == null)
				{
					var containersList = new List<G5TransportEquipmentWrapper>();

					item.TemporaryStorageLinkPackages
						.Where(linkPackage => linkPackage.IsLinked)
						.Select(linkPackage => linkPackage.Package)
							.Where(package => package.Container != null)
							.Select(package => package.Container)
								.Distinct()
								.ForEach(container => containersList.Add(new G5TransportEquipmentWrapper(container)));

					transportEquipments = containersList.AsReadOnly();
				}
				return transportEquipments;
			}
		}
		IReadOnlyCollection<G5TransportEquipmentWrapper> transportEquipments;

		public ZDateTime PresentationDateAtOrigin => tempHeader.AMA_CustomsOffice.StartsWith(Core.Constants.CountryCodes.Spain) ? ZDateTime.Empty : item.PresentationDate;

		public IReadOnlyCollection<IDocumentsCommon> SupportingDocuments => supportingDocuments ??= item.SupportingDocuments.Select(doc => new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber)).ToList().AsReadOnly();
		IReadOnlyCollection<DocumentCommonWrapper> supportingDocuments;

		public IReadOnlyCollection<IDocumentsCommon> AdditionalInfo => additionalInfos ??= item.AdditionalInfos.Where(doc => doc.CSI_SubType == AdditionalDocList.Codes.AdditionalInformation)
																												.Select(doc => new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_Description)).ToList().AsReadOnly();
		IReadOnlyCollection<DocumentCommonWrapper> additionalInfos;
	}
}
