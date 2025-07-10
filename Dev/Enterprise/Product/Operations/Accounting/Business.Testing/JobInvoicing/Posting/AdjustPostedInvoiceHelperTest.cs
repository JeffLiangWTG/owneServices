using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.ARAP.JobInvoicing.Testing
{
	public class AdjustPostedInvoiceHelperTest : TestCaseWithFactory
	{
		public void TestInvoiceRoundingLineCreatorType()
		{
			var adjustPostedInvoiceHelper = new AdjustPostedInvoiceHelper();
			AssertType<InvoiceRoundingLineCreator>(adjustPostedInvoiceHelper.InvoiceRoundingLineCreator_ExposedForTestOnly);
		}

		[ExpectNoExceptions]
		public void TestAddRoundingLine()
		{
			var adjustPostedInvoiceHelper = new AdjustPostedInvoiceHelper();

			var arInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", creator.AUD, 1m, 100m, 0m, 100m, 0m);

			var invoiceRoundingLineCreator = new Mock<IInvoiceRoundingLineCreator>(MockBehavior.Strict);
			invoiceRoundingLineCreator.Setup(x => x.AddRoundingLine(It.IsAny<InvoicingBase>()));
			adjustPostedInvoiceHelper.SubstituteInvoiceRoundingLineCreator_ForTestOnly(invoiceRoundingLineCreator.Object);

			((IAdjustPostedInvoiceHelper)adjustPostedInvoiceHelper).AdjustPostedInvoice(arInvoice);
			invoiceRoundingLineCreator.Verify(x => x.AddRoundingLine(It.Is<InvoicingBase>(y => y.AH_OSTotalAmount == 100m)), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestAddSurchargeLine()
		{
			var adjustPostedInvoiceHelper = new AdjustPostedInvoiceHelper();

			var arInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", creator.AUD, 1m, 100m, 0m, 100m, 0m);

			var surchargeLineCreator = new Mock<ISurchargeLineCreator>(MockBehavior.Strict);
			surchargeLineCreator.Setup(x => x.AddSurchargeLine(It.IsAny<InvoicingBase>()));
			ObjectFactory.Substitute(surchargeLineCreator.Object);

			((IAdjustPostedInvoiceHelper)adjustPostedInvoiceHelper).AdjustPostedInvoice(arInvoice);
			surchargeLineCreator.Verify(x => x.AddSurchargeLine(It.Is<InvoicingBase>(y => y.AH_OSTotalAmount == 100m)), Times.Once);
		}

		#region Implementation

		TestObjectCreator creator;

		protected override void SetUp()
		{
			base.SetUp();

			creator = new TestObjectCreator(Factory);
		}

		#endregion
	}
}
