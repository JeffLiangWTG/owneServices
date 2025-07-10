using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(BondedWarehousingHelper))]

namespace Enterprise.Customs.DE.DataTransfer.Universal
{
	public class WarehouseCustomsLineDetails : EU.DataTransfer.Universal.WarehouseCustomsLineDetails
	{
		public WarehouseCustomsLineDetails(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, WarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail, Shipment shipment)
			: base(factory, invoiceLine, fallbackDetail, shipment)
		{
		}

		public new WarehouseCustomsFallbackDetailWithEntryInstruction FallbackDetail => (WarehouseCustomsFallbackDetailWithEntryInstruction)base.FallbackDetail;

		protected override List<AddInfo> GetAddInfosApplicableForWarehousing()
		{
			var addInfos = base.GetAddInfosApplicableForWarehousing();

			var invoiceDate = FallbackDetail.InvoiceDate;
			addInfos.AddRange(new List<AddInfo>
			{
				AddInfo.New(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.LineNetPrice, InvoiceLine.AddInfoCollection?.GetZStringValue(Customs.DataTransfer.Universal.Constants.AddInfoKeys.InvoiceLine.NetPrice) ?? ZString.Empty),
				AddInfo.New(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.InvoiceNumber, FallbackDetail.InvoiceNumber),
				AddInfo.New(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.InvoiceDate, !invoiceDate.IsEmpty ? (ZString)invoiceDate.ToShortDateString() : ZString.Empty),
				AddInfo.New(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.IncotermCode, FallbackDetail.IncotermCode),
				AddInfo.New(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.IncotermPlace, FallbackDetail.IncotermPlace),
				AddInfo.New(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.TransNature, FallbackDetail.ValuationCode),
				AddInfo.New(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.Supplier, GetAddInfoZStringFromOrganizationAddress(FallbackDetail.SupplierAddress)),
				AddInfo.New(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.Importer, GetAddInfoZStringFromOrganizationAddress(FallbackDetail.ImporterAddress)),
				AddInfo.New(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.Buyer, GetAddInfoZStringFromOrganizationAddress(FallbackDetail.BuyerAddress)),
				AddInfo.New(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.Seller, GetAddInfoZStringFromOrganizationAddress(FallbackDetail.SellerAddress)),
				AddInfo.New(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.PortOfLoading, FallbackDetail.PortOfLoading),
				AddInfo.New(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.FirstEUArrival, FallbackDetail.PortOfFirstEUArrival),
				AddInfo.New(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.Transport, FallbackDetail.TransportMode),
			});

			return addInfos;
		}

		protected override IEnumerable<IWarehouseCustomsLineAddInfo> GetAdditionalAddInfos()
		{
			var res = base.GetAdditionalAddInfos().ToList();

			var supportingDocs = InvoiceLine.CustomsSupportingInformationCollection;
			if (supportingDocs != null)
			{
				foreach (var supportingInfo in supportingDocs)
				{
					res.Add(new WarehouseCustomsLineAddInfoSupportingInfo(supportingInfo));
				}
			}
			var supportingInfos = FallbackDetail.supportingInfos;
			if (supportingInfos != null)
			{
				foreach (var supportingInfo in supportingInfos)
				{
					res.Add(new WarehouseCustomsFallbackAddInfoSupportingInfo(supportingInfo));
				}
			}
			return res;
		}

		protected override bool IsOutward
		{
			get
			{
				if (isOutward == null)
				{
					isOutward = new RefCusProcedure.Loader(factory).LoadTop1FromFullCodeCurrentPlusPreviousPlusConcession(InvoiceLine.Procedure.GetValueOrDefault().PadRight(7), CountryCode, ZDateTime.Today)?.IsOutOfRegime() ?? shipment.HasRecipientRole(UniversalDataBuss.Integration.RecipientRoleType.BWR);
				}
				return isOutward.Value;
			}
		}
		bool? isOutward;

		ZString GetAddInfoZStringFromOrganizationAddress(OrganizationAddress orgAddress) => orgAddress != null
			? (ZString)$"{orgAddress.OrganizationCode};{orgAddress.AddressShortCode};{orgAddress.CompanyName};{orgAddress.Address1};{orgAddress.Address2};{orgAddress.City};{orgAddress.Country?.Code};{orgAddress.Postcode}"
			: ZString.Empty;
	}
}
