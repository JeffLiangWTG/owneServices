using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM433GoodsShipmentItemProvider : IM433GoodsShipmentItem
	{
		readonly EntryLineWrapper entryLineWrapper;
		readonly JobComInvoiceLine randomInvoiceLine;
		readonly CusEntryLine entryLine;

		public IM433GoodsShipmentItemProvider(CusEntryLine entryLine, EntryHeaderWrapper entryHeaderWrapper)
		{
			this.entryLine = entryLine;
			entryLineWrapper = new EntryLineWrapper(entryLine, entryHeaderWrapper);
			randomInvoiceLine = entryLineWrapper.RandomInvoiceLine;
		}

		public string DeclarationGoodsItemNumber => entryLine.CL_LineNumber.ToString();

		public IReadOnlyCollection<IAuthorisation> Authorisations => authorisations ?? (authorisations = randomInvoiceLine.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Select(x => new InvoiceLineAuthorizationProvider(x)).ToArray());
		IReadOnlyCollection<IAuthorisation> authorisations;

		public IProcedure Procedure => CachedValueHelper.GetValue(ref procedure, () => new ProcedureProvider(randomInvoiceLine));
		CachedValue<IProcedure> procedure;

		public IMCommodity Commodity => CachedValueHelper.GetValue(ref commodity, () => new MCommodityType04Provider(entryLineWrapper));
		CachedValue<IMCommodity> commodity;

		public IReadOnlyCollection<IPackaging> Packagings => packagings ?? (packagings = randomInvoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().Select(x => new PackagingProvider(x.Package.CW_PackType, BulkPackageTypeList.ContainsCode(x.Package.CW_PackType) ? ZInt.Zero : x.Package.CW_PackQty, x.Package.CW_MarksAndNos)).ToArray());
		IReadOnlyCollection<IPackaging> packagings;

		CodeDescriptionPairList BulkPackageTypeList => Universal.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(randomInvoiceLine.Factory,
																															Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
																															Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
																															UNPackTypeStartDate,
																															false,
																															Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk);

		public IReadOnlyCollection<IPreviousDocumentGoodsShipmentItem> PreviousDocuments => previousDocumentsCached ?? (previousDocumentsCached =
			entryLineWrapper.EntryLine.PreviousDocuments.Select(x => new PreviousDocumentGoodsShipmentItemProvider(x)).ToArray<IPreviousDocumentGoodsShipmentItem>());
		IReadOnlyCollection<IPreviousDocumentGoodsShipmentItem> previousDocumentsCached;
	}
}
