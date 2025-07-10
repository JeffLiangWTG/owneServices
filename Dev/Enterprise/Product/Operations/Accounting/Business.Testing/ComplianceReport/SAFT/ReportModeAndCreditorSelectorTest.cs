using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.SAFT.Testing
{
	[TestedType(typeof(ReportModeAndCreditorSelector))]
	internal class ReportModeAndCreditorSelectorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues_Portugal()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				var selector = (ReportModeAndCreditorSelector)GetNewBusinessObject();

				Assert("GenerateSalesInvoices", selector.GenerateSalesInvoices);
				AssertEquals("GenerateCreditorInvoices", false, selector.GenerateCreditorInvoices);
				Assert("CreditorPK should be empty", selector.CreditorPK.IsEmpty);
				Assert("CreditorPK should be ReadOnly", selector.CreditorPKInfo.ReadOnly);
				AssertEquals("GenerateAllInvoices", false, selector.GenerateAllInvoices);
				Assert("ShouldPopup", selector.ShouldPopup);
			}
		}

		public void TestDefaultValues_Norway()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Norway))
			{
				var selector = (ReportModeAndCreditorSelector)GetNewBusinessObject();

				Assert("GenerateAllInvoices", selector.GenerateAllInvoices);
				AssertEquals("ShouldPopup", false, selector.ShouldPopup);
			}
		}

		public void TestGenerateSalesInvoicesAndGenerateCreditorInvoicesAreMutuallyExclusive()
		{
			Selector.GenerateCreditorInvoices = true;
			AssertEquals("GenerateSalesInvoices", false, Selector.GenerateSalesInvoices);

			Selector.GenerateSalesInvoices = true;
			AssertEquals("GenerateCreditorInvoices", false, Selector.GenerateCreditorInvoices);
		}

		public void TestCreditorPK_ReadOnly()
		{
			Selector.GenerateCreditorInvoices = true;
			Assert("CreditorPK should be not ReadOnly", !Selector.CreditorPKInfo.ReadOnly);

			Selector.GenerateCreditorInvoices = false;
			Assert("CreditorPK should be ReadOnly", Selector.CreditorPKInfo.ReadOnly);
		}

		public void TestCreditor()
		{
			Assert("Pre-condition: CreditorPK should be empty", Selector.CreditorPK.IsEmpty);
			AssertNull("Creditor", Selector.Creditor);

			Selector.CreditorPK = ZGuid.Invalid;
			AssertNull("Creditor", Selector.Creditor);

			var creator = new TestObjectCreator(Factory);

			Selector.CreditorPK = creator.ABIGAS.PK;
			AssertNotNull("Creditor", Selector.Creditor);
			AssertEquals(creator.ABIGAS.PK, Selector.Creditor.PK);
		}

		public void TestRunPreSaveValidationCallsValidateCreditorPK()
		{
			AssertEquals("Pre-condition: GenerateSalesInvoices", false, Selector.GenerateCreditorInvoices);
			Assert("Pre-condition: CreditorPK should be empty", Selector.CreditorPK.IsEmpty);

			Selector.RunPreSaveValidation();
			AssertNoErrors(Selector.CreditorPKInfo);

			Selector.GenerateCreditorInvoices = true;
			Assert("Pre-condition: CreditorPK should be empty", Selector.CreditorPK.IsEmpty);
			Selector.RunPreSaveValidation();
			AssertHasError(Selector.CreditorPKInfo, "Please enter a value.");

			Selector.CreditorPK = ZGuid.Invalid;
			Selector.RunPreSaveValidation();
			AssertHasError(Selector.CreditorPKInfo, "Enter a valid selection.");

			var creator = new TestObjectCreator(Factory);
			var notCreditor = creator.CreateOrgHeader("NOTCRD", creditor: false, false);

			Selector.CreditorPK = notCreditor.PK;
			Selector.RunPreSaveValidation();
			AssertHasError(Selector.CreditorPKInfo, "Enter a valid selection.");

			AssertEquals("Pre-condition: OB_APCostsSelfBilled", false, creator.Creditor1.CompanyData.OB_APCostsSelfBilled);
			Selector.CreditorPK = creator.Creditor1.PK;
			Selector.RunPreSaveValidation();
			AssertHasError(Selector.CreditorPKInfo, "Select a Cost Self Billed Creditor.");

			creator.Creditor1.CompanyData.OB_APCostsSelfBilled = true;
			Selector.RunPreSaveValidation();
			AssertNoErrors(Selector.CreditorPKInfo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReportModeAndCreditorSelector(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Selector = (ReportModeAndCreditorSelector)GetNewBusinessObject();
		}

		ReportModeAndCreditorSelector Selector;
	}
}
