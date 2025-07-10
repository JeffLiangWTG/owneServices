using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ChangeTransactionDatesBusinessObjectBase))]
	public class ChangeTransactionDatesBusinessObjectBaseTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ChangeTransactionDatesBusinessObjectBase(Factory);
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_innerValue ?? (TestObjectCreator_innerValue = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_innerValue;

		ChangeTransactionDatesBusinessObjectBase TestBizo
		{
			get { return TestBizo_innerValue ?? (TestBizo_innerValue = (ChangeTransactionDatesBusinessObjectBase)GetNewBusinessObject()); }
		}
		ChangeTransactionDatesBusinessObjectBase TestBizo_innerValue;

		#endregion

		public void TestRunPreSaveValidation()
		{
			TestBizo.RunPreSaveValidation();
			Assert(TestBizo.HasErrors());

			TestBizo.InvoiceDate = ZDateTime.Now;
			TestBizo.RunPreSaveValidation();
			Assert(TestBizo.HasErrors());

			TestBizo.PostDate = TestBizo.InvoiceDate;
			TestBizo.RunPreSaveValidation();
			Assert(TestBizo.HasErrors());

			TestObjectCreator.CreateTestPeriods(TestBizo.InvoiceDate);
			Factory.Save();
			TestBizo.RunPreSaveValidation();
			Assert(!TestBizo.HasErrors());
		}

		public void TestValidateTransactionDate()
		{
			TestBizo.InvoiceDate = ZDateTime.Invalid;
			AssertHasErrors(TestBizo.InvoiceDateInfo);

			TestBizo.InvoiceDate = ZDateTime.Empty;
			AssertHasErrors(TestBizo.InvoiceDateInfo);

			TestBizo.InvoiceDate = ZDateTime.BrettsBirthday;
			AssertHasErrors(TestBizo.InvoiceDateInfo);

			TestBizo.InvoiceDate = ZDateTime.Now;
			AssertNoErrors(TestBizo.InvoiceDateInfo);

			TestBizo.InvoiceDate = ZDateTime.Now.AddDays(1);
			AssertNoErrors(TestBizo.InvoiceDateInfo);
			AssertHasWarningContaining(TestBizo.InvoiceDateInfo, "Please note: This date is in the future. Issuing a transaction with an Invoice Date set in the future may cause confusion for your Debtor.");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				TestBizo.InvoiceDate = ZDateTime.Now.AddDays(2);
				AssertNoErrors(TestBizo.InvoiceDateInfo);
				AssertHasWarningContaining(TestBizo.InvoiceDateInfo, @"Invoice Date is in the future. Please check the Invoice Date against the current system date and time. If this invoice is posted, future invoices cannot use current or previous date.");
			}
		}

		public void TestValidatePostDate()
		{
			TestBizo.PostDate = ZDateTime.Invalid;
			AssertHasErrors(TestBizo.PostDateInfo);

			TestBizo.PostDate = ZDateTime.Empty;
			AssertHasErrors(TestBizo.PostDateInfo);

			TestBizo.PostDate = ZDateTime.BrettsBirthday;
			AssertHasErrors(TestBizo.PostDateInfo);

			TestBizo.PostDate = ZDateTime.Now;
			AssertHasErrors(TestBizo.PostDateInfo);

			TestObjectCreator.CreateTestPeriods(TestBizo.PostDate);
			Factory.Save();
			TestBizo.PostDate = ZDateTime.Empty;
			TestBizo.PostDate = ZDateTime.Now;
			AssertNoErrors(TestBizo.PostDateInfo);

			TestBizo.PostDate = ZDateTime.Now.AddDays(1);
			AssertHasErrors(TestBizo.PostDateInfo);
		}

		public void TestDefaultPostDateFromInvoiceDate()
		{
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BackDateInvoicesConfiguration() { DefaultPostDateFromInvoiceDate = false });
			TestBizo.InvoiceDate = ZDateTime.Empty;
			TestBizo.PostDate = ZDateTime.Empty;

			TestBizo.InvoiceDate = ZDateTime.Now;
			AssertEquals("Post Date should not be defaulted to Invoice Date without appropriate registry configuration.", ZDateTime.Empty, TestBizo.PostDate);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BackDateInvoicesConfiguration() { DefaultPostDateFromInvoiceDate = true });
			TestBizo.InvoiceDate = ZDateTime.Empty;
			TestBizo.PostDate = ZDateTime.Empty;

			ZDateTime expectedNowDate = ZDateTime.Now;
			TestBizo.InvoiceDate = expectedNowDate;
			AssertEquals("Post Date should be defaulted to Invoice Date.", expectedNowDate, TestBizo.PostDate);

			TestBizo.InvoiceDate = ZDateTime.BrettsBirthday;
			AssertHasErrors(TestBizo.InvoiceDateInfo);
			AssertEquals("Post Date should not be defaulted to Invoice Date with errors.", expectedNowDate, TestBizo.PostDate);

			TestBizo.InvoiceDate = expectedNowDate;
			AssertNoErrors(TestBizo.InvoiceDateInfo);
			AssertEquals("Post Date should be defaulted to Invoice Date without errors.", expectedNowDate, TestBizo.PostDate);
		}

		public void TestValidateInvoiceDate_AddErrorIfInvoiceDateIsInTheFuture()
		{
			AccountingMasterFilesRegistry.Instance.DisallowPostingInvoicesWithAFutureInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			TestBizo.InvoiceDate = ZDateTime.Now;
			AssertNoErrors(TestBizo.InvoiceDateInfo);

			TestBizo.InvoiceDate = ZDateTime.Now.AddDays(1);
			AssertHasError(TestBizo.InvoiceDateInfo, "The invoice date cannot be in the future because the registry 'Accounting > Receivable > Default Settings > Disallow Posting Invoices With A Future Invoice Date' is set to Yes.");
		}

		public void TestValidateInvoiceDate_IInvoiceDateValidation()
		{
			var validationMock = new Mock<IInvoiceDateValidation>();
			var countryFactoryMock = new Mock<IAccountingCountryFactory>();
			countryFactoryMock.As<IInstanceProvider<IInvoiceDateValidation>>().Setup(x => x.Get()).Returns(validationMock.Object);
			var factoryMock = new Mock<IGlobalAccountingCountryFactory>();
			factoryMock.Setup(c => c.GetCountryFactory(It.IsAny<ZString>())).Returns(countryFactoryMock.Object);

			using (ObjectFactory.Substitute(factoryMock.Object))
			{
				validationMock.Setup(x => x.ValidateInvoiceDate(It.IsAny<ZDateTime>())).Returns((ResourceString)null);
				TestBizo.InvoiceDate = ZDateTime.Now;
				AssertNoErrors(TestBizo.InvoiceDateInfo);

				validationMock.Reset();

				validationMock.Setup(x => x.ValidateInvoiceDate(It.IsAny<ZDateTime>())).Returns(ResString.GetMultilingualString("Test", "Dummy Error"));
				TestBizo.InvoiceDate = ZDateTime.Now.AddDays(1);
				AssertHasError(TestBizo.InvoiceDateInfo, "Dummy Error");
			}
		}
	}
}
