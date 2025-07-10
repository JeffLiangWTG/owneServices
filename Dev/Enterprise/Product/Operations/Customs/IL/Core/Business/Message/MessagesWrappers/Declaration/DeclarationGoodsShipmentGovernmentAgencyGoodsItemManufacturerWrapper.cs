using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerWrapper : IDeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer
	{
		DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		internal static IDeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer NewOrNull(JobComInvoiceLine invoiceLine)
			=> invoiceLine == null ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerWrapper(invoiceLine);

		IIDType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer.ID => IDTypeWrapper.NewOrNull(invoiceLine.InvoiceHeader.ManufacturerAddress?.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.SupplierCode, Core.Constants.CountryCodes.Israel) ?? ZString.Empty);

		readonly JobComInvoiceLine invoiceLine;
	}
}
