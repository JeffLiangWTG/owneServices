using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Testing
{
	class JobComInvoiceLineValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestSettingValudationMethodsCreatesSupportingDocuments()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B0000100";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var inv = dec.Invoices.AddNew();
			inv.JZ_InvoiceNumber = "123456";
			AssertEquals(0, inv.SupportingDocuments.Count);

			var line = (EU.Business.Declaration.JobComInvoiceLine)inv.InvoiceLines.AddNew();
			line.JI_ValuationCode = ValuationMethodList.Codes._1;
			AssertEquals(1, line.SupportingDocuments.Count);
			AssertEquals("N934", line.SupportingDocuments[0].CSI_Code);
			AssertEquals("", line.SupportingDocuments[0].CSI_ReferenceNumber);
			AssertOtherValuationMethods(line);
		}

		void AssertOtherValuationMethods(EU.Business.Declaration.JobComInvoiceLine line)
		{
			foreach (var vm in new ZString[] { ValuationMethodList.Codes._2, ValuationMethodList.Codes._3 })
			{
				line.JI_ValuationCode = vm;
				AssertEquals(1, line.SupportingDocuments.Count);
				AssertEquals("9200", line.SupportingDocuments[0].CSI_Code);
				AssertEquals("", line.SupportingDocuments[0].CSI_ReferenceNumber);
			}

			foreach (var vm in new ZString[] { ValuationMethodList.Codes._4, ValuationMethodList.Codes._5, ValuationMethodList.Codes._6 })
			{
				line.JI_ValuationCode = vm;
				AssertEquals(2, line.SupportingDocuments.Count);
				AssertEquals("9200", line.SupportingDocuments[0].CSI_Code);
				AssertEquals("", line.SupportingDocuments[0].CSI_ReferenceNumber);
				AssertEquals("9WKS", line.SupportingDocuments[1].CSI_Code);
				AssertEquals("B0000100", line.SupportingDocuments[1].CSI_ReferenceNumber);
				AssertEquals(string.Format("SEE ATTACHED WORKSHEET {0}", line.Declaration.JE_DeclarationReference), line.SupportingDocuments[1].CSI_Description);
			}

			line.JI_ValuationCode = ValuationMethodList.Codes._7;
			AssertEquals(0, line.SupportingDocuments.Count);
			line.JI_ValuationCode = "";
			AssertEquals(0, line.SupportingDocuments.Count);
		}
	}
}
