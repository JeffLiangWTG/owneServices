using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(AQISProducerCodeForm))]
	sealed class AQISProducerCodeFormTest : ZFormBasherTest
	{
		public void TestGridId()
		{
			using (var form = (AQISProducerCodeForm)GetFormToBashCore())
			{
				AssertEquals("GridLayoutLJyjvt06xHjWp3458eWURQ==", form.zGrid1.GridId);
			}
		}

		public void TestOKButton()
		{
			using (AQISProducerCodeForm form = (AQISProducerCodeForm)GetFormToBashCore())
			{
				AQISProducerCode producerCode1 = invoiceLine.AQISProducerCodes.AddNew();
				producerCode1.Code = "1";
				AQISProducerCode producerCode2 = invoiceLine.AQISProducerCodes.AddNew();
				producerCode2.Code = "2";
				form.OKButton_Click(null, null);
				AssertEquals("Add Info string", false, invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden.IsEmpty);
				AssertEquals("Container Code 1", true, invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("1"));
				AssertEquals("Container Code 2", true, invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("2"));
			}
		}

		public void TestCodeColumnModuleID()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var form = (AQISProducerCodeForm)GetFormToBashCore())
				{
					var zCodeFindBoxColumnStyleInfo = (ZCodeFindBoxColumnStyleInfo)form.zGrid1.GetColumnStyle(nameof(AQISProducerCode.Code));
					AssertEquals("UseRefDatabaseData = False", Enterprise.ZArchitecture.Modules.ModuleIDs.AQISProducerCode, zCodeFindBoxColumnStyleInfo.ModuleID);
				}
			}

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = (AQISProducerCodeForm)GetFormToBashCore())
				{
					var zCodeFindBoxColumnStyleInfo = (ZCodeFindBoxColumnStyleInfo)form.zGrid1.GetColumnStyle(nameof(AQISProducerCode.Code));
					AssertEquals("UseRefDatabaseData = True", Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList, zCodeFindBoxColumnStyleInfo.ModuleID);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			return new AQISProducerCodeForm(invoiceLine);
		}

		JobComInvoiceLine invoiceLine;
	}
}
