using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	sealed class DeclarationGoodsShipmentUcrWrapper : IDeclarationGoodsShipmentUniqueConsignmentReference
	{
		DeclarationGoodsShipmentUcrWrapper(JobComInvoiceHeader invoiceHeader)
		{
			this.invoiceHeader = invoiceHeader;
		}
		readonly JobComInvoiceHeader invoiceHeader;

		internal static DeclarationGoodsShipmentUcrWrapper NewOrNull(JobComInvoiceHeader invoiceHeader)
			=> invoiceHeader == null
			? null
			: new DeclarationGoodsShipmentUcrWrapper(invoiceHeader);

		#region IDeclarationGoodsShipmentUniqueConsignmentReference

		IIDType IDeclarationGoodsShipmentUniqueConsignmentReference.Id => IDTypeWrapper.NewOrNull(invoiceHeader.JZ_UCR);

		IIDType IDeclarationGoodsShipmentUniqueConsignmentReference.TraderAssignedReferenceId => null;

		#endregion
	}
}
