using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class JobComInvoiceHeaderTestForDocumentWrapper : Customs.Business.Testing.BaseJobComInvoiceHeaderTestForDocumentWrapper
	{
		protected override BaseJobDeclaration GetNewDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			return declaration;
		}

		protected override ZDecimal ExpectedIncludedTotalInInvoiceCurr => 54m;
	}
}
