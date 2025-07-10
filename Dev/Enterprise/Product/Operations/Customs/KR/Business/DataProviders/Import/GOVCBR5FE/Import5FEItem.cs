using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5FEItem : IImport5FEItem
	{
		public string AmendType { get; set; }
		public int EntryLineNo { get; set; }
		public int InvoiceLineNo { get; set; }
		public string GARequirementApprovalNumber { get; set; }
		public int NonGASequnceNo { get; set; }
		public string ExportDeclarationNumber { get; set; }
		public int ExportDeclarationEntryLineNo { get; set; }
		public int ExportDeclarationInvoiceLineNo { get; set; }
		public int ContainerNo { get; set; }
		public int ImmediateDeliveryNo { get; set; }
		public int OnlineOrderNo { get; set; }
		public string AmendDataItemID { get; set; }
		public string BeforeDescription { get; set; }
		public string AfterDescription { get; set; }

		ZString IImport5FEItem.AmendType => AmendType;
		ZInt IImport5FEItem.EntryLineNo => EntryLineNo;
		ZInt IImport5FEItem.InvoiceLineNo => InvoiceLineNo;
		ZString IImport5FEItem.GARequirementApprovalNumber => GARequirementApprovalNumber;
		ZInt IImport5FEItem.NonGASequnceNo => NonGASequnceNo;
		ZString IImport5FEItem.ExportDeclarationNumber => ExportDeclarationNumber;
		ZInt IImport5FEItem.ExportDeclarationEntryLineNo => ExportDeclarationEntryLineNo;
		ZInt IImport5FEItem.ExportDeclarationInvoiceLineNo => ExportDeclarationInvoiceLineNo;
		ZInt IImport5FEItem.ContainerNo => ContainerNo;
		ZInt IImport5FEItem.ImmediateDeliveryNo => ImmediateDeliveryNo;
		ZInt IImport5FEItem.OnlineOrderNo => OnlineOrderNo;
		ZString IImport5FEItem.AmendDataItemID => AmendDataItemID;
		ZString IImport5FEItem.BeforeDescription => BeforeDescription;
		ZString IImport5FEItem.AfterDescription => AfterDescription;
	}
}
