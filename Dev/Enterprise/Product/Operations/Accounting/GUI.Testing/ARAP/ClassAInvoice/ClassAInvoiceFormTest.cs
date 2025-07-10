using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(ClassAInvoiceForm))]
	class ClassAInvoiceFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			GovernmentInvoice testARInvoice = Factory.NewWithValidTestData<GovernmentInvoice>();
			Factory.Save();
			return new ClassAInvoiceForm(testARInvoice);
		}

		[GuiTest]
		public void TestComplianceSubTypeDropEditVisibility()
		{
			var invoice = Factory.NewWithValidTestData<GovernmentInvoice>();
			invoice.AH_OH = new TestObjectCreator(Factory).Debtor.PK;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Peru))
			using (ClassAInvoiceForm form = new ClassAInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				Assert(form.ComplianceSubTypeDropEdit_ForTestOnly.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (ClassAInvoiceForm form = new ClassAInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				Assert(form.ComplianceSubTypeDropEdit_ForTestOnly.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Indonesia))
			using (ClassAInvoiceForm form = new ClassAInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				Assert(form.ComplianceSubTypeDropEdit_ForTestOnly.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (ClassAInvoiceForm form = new ClassAInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				Assert(form.ComplianceSubTypeDropEdit_ForTestOnly.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (ClassAInvoiceForm form = new ClassAInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				Assert(!form.ComplianceSubTypeDropEdit_ForTestOnly.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			using (ClassAInvoiceForm form = new ClassAInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				Assert(form.ComplianceSubTypeDropEdit_ForTestOnly.Visible);
			}
		}

		[GuiTest]
		public void TestComplianceDocDateVisibility()
		{
			var type = typeof(Core.Constants.CountryCodes);
			var invoice = Factory.NewWithValidTestData<GovernmentInvoice>();

			foreach (var fieldinfo in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
			{
				string countryname = fieldinfo.GetValue(type).ToString();
				var bizo = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryname);
				if (bizo == null)
				{
					continue;
				}

				if (countryname == Core.Constants.CountryCodes.China)
				{
					var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();

					var complianceSubTypeAndNumberUpdateRulesMock = new Mock<IComplianceSubTypeAndNumberUpdateRules>();
					complianceSubTypeAndNumberUpdateRulesMock.Setup(x => x.IsComplianceDocDateAllowed(It.IsAny<AccTransactionHeader>())).Returns(false);
					countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeAndNumberUpdateRules(It.IsAny<ZString>())).Returns(complianceSubTypeAndNumberUpdateRulesMock.Object);

					using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryname))
					using (ClassAInvoiceForm form = new ClassAInvoiceForm(invoice))
					{
						form.Show();
						Application.DoEvents();
						Assert(!form.ComplianceDocDateEdit_ForTestOnly.Visible);
					}

					countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();

					complianceSubTypeAndNumberUpdateRulesMock = new Mock<IComplianceSubTypeAndNumberUpdateRules>();
					complianceSubTypeAndNumberUpdateRulesMock.Setup(x => x.IsComplianceDocDateAllowed(It.IsAny<AccTransactionHeader>())).Returns(true);
					countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeAndNumberUpdateRules(It.IsAny<ZString>())).Returns(complianceSubTypeAndNumberUpdateRulesMock.Object);

					using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryname))
					using (ClassAInvoiceForm form = new ClassAInvoiceForm(invoice))
					{
						form.Show();
						Application.DoEvents();
						Assert(form.ComplianceDocDateEdit_ForTestOnly.Visible);
					}
				}
				else if (countryname == Core.Constants.CountryCodes.Taiwan)
				{
					invoice.AH_Ledger = LedgerTypes.AccountsPayable;
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryname))
					using (ClassAInvoiceForm form = new ClassAInvoiceForm(invoice))
					{
						form.Show();
						Application.DoEvents();
						Assert(form.ComplianceDocDateEdit_ForTestOnly.Visible);
					}

					invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryname))
					using (ClassAInvoiceForm form = new ClassAInvoiceForm(invoice))
					{
						form.Show();
						Application.DoEvents();
						Assert(!form.ComplianceDocDateEdit_ForTestOnly.Visible);
					}
				}
				else
				{
					invoice.AH_Ledger = LedgerTypes.AccountsPayable;
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryname))
					using (ClassAInvoiceForm form = new ClassAInvoiceForm(invoice))
					{
						form.Show();
						Application.DoEvents();
						Assert(!form.ComplianceDocDateEdit_ForTestOnly.Visible);
					}

					invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryname))
					using (ClassAInvoiceForm form = new ClassAInvoiceForm(invoice))
					{
						form.Show();
						Application.DoEvents();
						Assert(!form.ComplianceDocDateEdit_ForTestOnly.Visible);
					}
				}
			}
		}

		[GuiTest]
		public void TestComplianceNumberVisibility()
		{
			var invoice = Factory.NewWithValidTestData<GovernmentInvoice>();

			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
			var complianceSubTypeAndNumberUpdateRulesMock = new Mock<IComplianceSubTypeAndNumberUpdateRules>();
			complianceSubTypeAndNumberUpdateRulesMock.Setup(x => x.IsComplianceNumberAllowed(It.IsAny<AccTransactionHeader>())).Returns(false);
			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeAndNumberUpdateRules(It.IsAny<ZString>())).Returns(complianceSubTypeAndNumberUpdateRulesMock.Object);

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (var form = new ClassAInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				Assert(!form.ClassAInvoiceNumTextBox_ForTestOnly.Visible);
			}

			countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
			complianceSubTypeAndNumberUpdateRulesMock = new Mock<IComplianceSubTypeAndNumberUpdateRules>();
			complianceSubTypeAndNumberUpdateRulesMock.Setup(x => x.IsComplianceNumberAllowed(It.IsAny<AccTransactionHeader>())).Returns(true);
			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeAndNumberUpdateRules(It.IsAny<ZString>())).Returns(complianceSubTypeAndNumberUpdateRulesMock.Object);

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (var form = new ClassAInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				Assert(form.ClassAInvoiceNumTextBox_ForTestOnly.Visible);
			}
		}

		public void TestFormBecomesReadOnlyAfterACriticalValidationError()
		{
			GovernmentInvoice testARInvoice = Factory.NewWithValidTestData<GovernmentInvoice>();
			Factory.Save();

			var bizo = Factory.Load<DummyGovernmentInvoiceCriticalValidationParent>(testARInvoice.PK);
			bizo.AH_TransactionNum = "TST0123";
			bizo.AH_TransactionReference = "TSR0125";
			using (var form = new ClassAInvoiceFormTestForCriticalValidationException(bizo))
			{
				AssertEquals("Context (Before Critical Validation Error)", false, form.BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation));
				AssertNotEquals("DisplayMode (Before Critical Validation Error)", ODisplayMode.ReadOnly, form.DisplayMode);

				bizo.CriticalValidation.RegisterOnSavingCheck();
				form.DisplayMode = ODisplayMode.New;

				bizo.RefreshBinding();
				form.Show();
				form.OnPostButtonClick_ForTestOnly(null, null);

				AssertEquals("Context (After Critical Validation Error)", true, form.BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation));
				AssertEquals("DisplayMode (After Critical Validation Error)", ODisplayMode.ReadOnly, form.DisplayMode);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		class ClassAInvoiceFormTestForCriticalValidationException : ClassAInvoiceForm
		{
			public ClassAInvoiceFormTestForCriticalValidationException(DummyGovernmentInvoiceCriticalValidationParent bizo)
				: base(bizo)
			{
			}
		}

		class DummyGovernmentInvoiceCriticalValidationParent : GovernmentInvoice, ISupportCriticalValidation
		{
			public DummyGovernmentInvoiceCriticalValidationParent(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			#region ISupportCriticalValidation Members

			public ICriticalValidation CriticalValidation
			{
				get { return new DummyCriticalValidation(this); }
			}

			#endregion
		}

		class DummyCriticalValidation : CriticalValidation<DummyGovernmentInvoiceCriticalValidationParent>
		{
			public DummyCriticalValidation(DummyGovernmentInvoiceCriticalValidationParent parent)
				: base(parent)
			{
			}

			protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.DummyErrorKeyForTest, ResString.GetMultilingualString("d6217571-847f-472f-9347-47ec07e7c256", "Test error message."), "E=MC2");
			}

			protected override IEnumerable<CriticalValidationResult> DeletedObjectOnSavingCriticalChecks()
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.DummyErrorKeyForTest, ResString.GetMultilingualString("d6217571-847f-472f-9347-47ec07e7c256", "Test error message."), "E=MC2");
			}
		}
	}
}
