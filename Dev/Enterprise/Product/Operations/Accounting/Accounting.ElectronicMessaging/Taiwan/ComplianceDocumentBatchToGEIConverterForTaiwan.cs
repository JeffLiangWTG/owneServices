using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;

namespace Enterprise.Accounting.ElectronicMessaging.Taiwan
{
	public class ComplianceDocumentBatchToGEIConverterForTaiwan : ComplianceDocumentBatchToGEIConverter
	{
		protected override IGlobalElectronicInvoiceBuilder GetGEIBuilder(string batchNumber)
		{
			return new GlobalElectronicInvoiceBuilderForTaiwan(batchNumber, ComplianceBatch);
		}
	}
}