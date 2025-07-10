using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Module.Testing
{
	[TestedType(typeof(AccCommissionLineController))]
	internal class AccCommissionLineControllerTest : ZControllerBasherTest
	{
		#region Standard Overrides

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CommissionLine;
		}

		#endregion

		#region ShowViewForm

		public void TestShowViewForm()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var invoiceCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			invoiceCommissionHeader.CH0_AH_Source = invoice.PK;
			invoiceCommissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			invoiceCommissionHeader.CH0_GroupingSourceID = invoice.PK;
			var invoiceCommissionLine = invoiceCommissionHeader.Lines.AddNew();
			invoiceCommissionLine.FillWithValidTestData();

			Factory.Save();

			var viewCommissionLine = Factory.Load<ViewCommissionLine>(invoiceCommissionLine.PK);
			var controller = ZControllerFactory.Create(ControllerIDs.CommissionLine);
			using (var invoiceForm = controller.ShowViewForm(viewCommissionLine))
			{
				AssertNotNull(invoiceForm);
				AssertEquals(ControllerIDs.ARInvoice, invoiceForm.ControllerID);
			}

			using (var form = controller.ShowViewForm(Factory.New<ViewCommissionLine>()))
			{
				AssertNull(form);
			}
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var line = Factory.NewWithValidTestData<AccCommissionLine>();
			Factory.Save();

			return Factory.Load<ViewCommissionLine>(line.PK);
		}

		#endregion
	}
}
