using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DE.DataTransfer.Universal
{
	public class WarehouseCustomsFallbackDetailWithEntryInstruction : EU.DataTransfer.Universal.WarehouseCustomsFallbackDetailWithEntryInstruction
	{
		public ZString InvoiceNumber;

		public ZDateTime InvoiceDate;

		public ZString IncotermCode;

		public ZString IncotermPlace;

		public ZString ValuationCode;

		public OrganizationAddress ImporterAddress;

		public ZString PortOfLoading;

		public ZString PortOfFirstEUArrival;

		public ZString TransportMode;

		public OrganizationAddress BuyerAddress;

		public OrganizationAddress SellerAddress;

		public List<CustomsSupportingInformation> supportingInfos;

		public static WarehouseCustomsFallbackDetailWithEntryInstruction CloneBaseProperties(EU.DataTransfer.Universal.WarehouseCustomsFallbackDetailWithEntryInstruction source)
			=> new WarehouseCustomsFallbackDetailWithEntryInstruction
			{
				IsExport = source.IsExport,
				IsExWarehouse = source.IsExWarehouse,
				SupplierAddress = source.SupplierAddress,
				InvoiceLineAddInfosApplicableForInwardWarehousing = source.InvoiceLineAddInfosApplicableForInwardWarehousing,
				EntryInstructionProcedureMap = source.EntryInstructionProcedureMap,
				EntryInstructionEntryHeaderMap = source.EntryInstructionEntryHeaderMap,
				FallbackAddInfos = source.FallbackAddInfos,
				LinePriceCurrency = source.LinePriceCurrency,
				CountryCode = source.CountryCode,
			};

		protected override WarehouseCustomsFallbackDetail CloneCore()
		{
			var result = CloneBaseProperties(this);
			result.LinePriceCurrency = LinePriceCurrency;
			result.CountryCode = CountryCode;
			result.InvoiceNumber = InvoiceNumber;
			result.InvoiceDate = InvoiceDate;
			result.IncotermCode = IncotermCode;
			result.IncotermPlace = IncotermPlace;
			result.ValuationCode = ValuationCode;
			result.ImporterAddress = ImporterAddress;
			result.BuyerAddress = BuyerAddress;
			result.SellerAddress = SellerAddress;
			result.PortOfLoading = PortOfLoading;
			result.PortOfFirstEUArrival = PortOfFirstEUArrival;
			result.TransportMode = TransportMode;
			result.supportingInfos = supportingInfos;
			return result;
		}
	}
}
