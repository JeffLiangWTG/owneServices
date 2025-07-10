using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.ExitControl.Business;

public class EALAESGoodsItemWrapper : IEALAESGoodsItem
{
	public EALAESGoodsItemWrapper(CusExitConsignmentItem consignmentItem, ZDecimal grossMass, ZDecimal netMass, IEnumerable<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString packageMarks)> packageData, IEnumerable<AdditionalInfo> transportDocs)
	{
		exitConsignmentItem = Argument.NotNull(consignmentItem, nameof(consignmentItem));
		this.grossMass = grossMass;
		this.netMass = netMass;
		this.transportDocs = transportDocs;
		this.packageData = packageData;

		isConsignmentItemMissing = exitConsignmentItem.StatusIsMissing;

		SequenceNumber = exitConsignmentItem.CCI_LineNumber.ToString();
		ReferenceNumberUCR = !isConsignmentItemMissing && exitConsignmentItem.UCRStatusIsDifferencesToDeclared ? exitConsignmentItem.CCI_UniqueConsignmentReference : ZString.Empty;
	}
	readonly CusExitConsignmentItem exitConsignmentItem;
	readonly ZDecimal grossMass;
	readonly ZDecimal netMass;
	readonly IEnumerable<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString packageMarks)> packageData;
	readonly IEnumerable<AdditionalInfo> transportDocs;
	readonly ZBool isConsignmentItemMissing;

	public ZString SequenceNumber { get; }

	public ZString ReferenceNumberUCR { get; }

	public IEALAESCommodity Commodity => commodity ?? (commodity = !exitConsignmentItem.StatusIsDifferencesToDeclared ? null : new EALAESCommodityWrapper(exitConsignmentItem, grossMass, netMass));
	EALAESCommodityWrapper commodity;

	public IReadOnlyCollection<ICommonPackageWithSequenceAndPackNum> Packaging
	{
		get
		{
			if (packaging == null)
			{
				var packagingList = new List<CommonPackageWithSequenceAndPackNumWrapper>();

				if (!isConsignmentItemMissing)
				{
					foreach (var data in packageData)
					{
						packagingList.Add(new CommonPackageWithSequenceAndPackNumWrapper(data.packageType, data.packageMarks, data.packageQuantity, data.seqNum));
					}
				}

				packaging = packagingList.AsReadOnly();
			}
			return packaging;
		}
	}
	IReadOnlyCollection<CommonPackageWithSequenceAndPackNumWrapper> packaging;

	public IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocuments
	{
		get
		{
			if (transportDocuments == null)
			{
				var transportDocsList = new List<CommonDocumentSequenceNumberWrapper>();

				if (!isConsignmentItemMissing)
				{
					foreach (var doc in transportDocs)
					{
						var isMissing = doc.StatusIsMissing;
						transportDocsList.Add(new CommonDocumentSequenceNumberWrapper(isMissing ? ZString.Empty : doc.CSI_Code, isMissing ? ZString.Empty : doc.CSI_ReferenceNumber, doc.CSI_ItemNumber));
					}
				}

				transportDocuments = transportDocsList.AsReadOnly();
			}
			return transportDocuments;
		}
	}
	IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> transportDocuments;

	public static List<EALAESGoodsItemWrapper> GetGoodsItemsList(CusExitReport exitReport)
	{
		var itemsMapping = new Dictionary<CusExitConsignmentItem, (List<CusExitReportItem> reportItems, Dictionary<CusExitConsignmentPackage, CusExitReportItem> packages)>();

		foreach (var reportItem in exitReport.CusExitReportItems)
		{
			if (reportItem.ConsignmentItem is CusExitConsignmentItem consignmentItem)
			{
				var items = itemsMapping.GetOrAdd(consignmentItem, () => (new List<CusExitReportItem>(), new Dictionary<CusExitConsignmentPackage, CusExitReportItem>()));

				items.Item1.Add(reportItem);

				if (reportItem.Package != null
					&& reportItem.Package is CusExitConsignmentPackage package
					&& package.StatusIsDifferencesToDeclared
					&& !items.Item2.ContainsKey(package))
				{
					items.Item2.Add(package, reportItem);
				}
			}
		}

		foreach (var consignmentItem in exitReport.Consignment.CusExitConsignmentItems.Where(x => !itemsMapping.ContainsKey(x)))
		{
			if (consignmentItem.StatusIsMissing || consignmentItem.UCRStatusIsDifferencesToDeclared)
			{
				itemsMapping.GetOrAdd(consignmentItem, () => (new List<CusExitReportItem>(), new Dictionary<CusExitConsignmentPackage, CusExitReportItem>()));
			}
		}

		var goodsItemsList = new List<EALAESGoodsItemWrapper>();

		foreach (var mapping in itemsMapping)
		{
			var documents = new List<AdditionalInfo>();
			var packages = new List<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString packageMarks)>();

			if (mapping.Value.reportItems != null)
			{
				mapping.Value.reportItems.ForEach(r => documents.AddRange(r.AdditionalInfos.Cast<AdditionalInfo>().Where(doc => doc.IsATransportDocument
																															&& doc.StatusIsDifferencesToDeclared)));
				documents.AddRange(mapping.Key.AdditionalInfos.Cast<AdditionalInfo>().Where(doc => doc.IsATransportDocument
																								&& doc.StatusIsMissing));
				documents = documents.OrderBy(doc => doc.CSI_ItemNumber).ToList();
			}

			if (mapping.Value.packages != null)
			{
				foreach (var pack in mapping.Value.packages)
				{
					packages.Add((pack.Key.CXP_Sequence,
									pack.Key.CXP_PackageType,
									pack.Key.CXP_Quantity == pack.Value.ERI_Quantity ? ZString.Empty : (ZString)pack.Value.ERI_Quantity.ToString(),
									pack.Key.CXP_MarksAndNumbers));
				}
			}

			var mappingConsignmentItem = mapping.Key;

			var missingPackagesInConsignmentItem = mappingConsignmentItem.CusExitConsignmentPackagePivots.Where(p => p.Package != null && ((CusExitConsignmentPackage)p.Package).StatusIsMissing).Select(p => p.Package);
			missingPackagesInConsignmentItem.ForEach(p => packages.Add((p.CXP_Sequence, ZString.Empty, ZString.Empty, ZString.Empty)));
			packages = packages.OrderBy(pack => pack.seqNum).ToList();

			var reportItem = mapping.Value.reportItems?.FirstOrDefault();

			var isStatusMISorDIF = mappingConsignmentItem.StatusIsMissing || mappingConsignmentItem.StatusIsDifferencesToDeclared;
			var isUCRStatusMISorDIF = mappingConsignmentItem.CCI_UniqueConsignmentReferenceStatus == EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing || mappingConsignmentItem.UCRStatusIsDifferencesToDeclared;

			if (isStatusMISorDIF || isUCRStatusMISorDIF || documents.Any() || packages.Any())
			{
				goodsItemsList.Add(new EALAESGoodsItemWrapper(mappingConsignmentItem, reportItem?.ERI_GrossMass ?? ZDecimal.Zero, reportItem?.ERI_NetMass ?? ZDecimal.Zero, packages, documents));
			}
		}

		return goodsItemsList.OrderBy(x => x.SequenceNumber).ToList();
	}
}
