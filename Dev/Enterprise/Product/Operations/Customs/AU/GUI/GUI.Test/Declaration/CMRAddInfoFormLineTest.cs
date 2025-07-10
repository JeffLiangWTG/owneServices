using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(CMRAddInfoForm))]
	sealed class CMRAddInfoFormLineTest : ZFormBasherTest
	{
		public void TestGSTECodeFindBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			using (var form = new CMRAddInfoForm(invoiceLine.AddInfo))
			{
				form.Show();
				var codeFindBox = form.FindSingle<ZCodeFindBox>("GSTECodeFindBox");
				AssertEquals("ZA_GSTE", ((IDataBoundControl)codeFindBox).DataMember);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			return new CMRAddInfoForm(invoiceLine.AddInfo);
		}
	}
}
