using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5FEItem
	{
		ZString AmendType { get; }
		ZInt EntryLineNo { get; }
		ZInt InvoiceLineNo { get; }
		ZString GARequirementApprovalNumber { get; }
		ZInt NonGASequnceNo { get; }
		ZString ExportDeclarationNumber { get; }
		ZInt ExportDeclarationEntryLineNo { get; }
		ZInt ExportDeclarationInvoiceLineNo { get; }
		ZInt ContainerNo { get; }
		ZInt ImmediateDeliveryNo { get; }
		ZInt OnlineOrderNo { get; }
		ZString AmendDataItemID { get; }
		ZString BeforeDescription { get; }
		ZString AfterDescription { get; }
	}
}
