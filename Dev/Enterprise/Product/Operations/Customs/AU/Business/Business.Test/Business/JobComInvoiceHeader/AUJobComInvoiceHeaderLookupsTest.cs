using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class AUJobComInvoiceHeaderLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MakeNonPersistent();

			var invoice = declaration.Invoices.AddNew();

			Assert("Should contains the AQS on a AU commercial invoice.", invoice.Lookups.MessageTypes.ContainsCode(AUJobMessageTypeList.Codes.Quarantine));
		}

		[TestDate(2020, 01, 01)]
		public void TestGetCorrectIncoTermImportList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			var group1 = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = group1.JobComInvoiceHeaders.AddNew();

			AssertEquals("Incoterms", 10, invoice.Lookups.JZ_IncoTerm_List.Count);

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Incoterms", 14, invoice.Lookups.JZ_IncoTerm_List.Count);
		}

		[TestDate(2020, 01, 01)]
		public void TestGetCorrectIncotermForCMR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var group1 = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = group1.JobComInvoiceHeaders.AddNew();

			AssertEquals("Incoterms", 14, invoice.Lookups.JZ_IncoTerm_List.Count);
		}
	}
}
