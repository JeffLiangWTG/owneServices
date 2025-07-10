using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	[TestedType(typeof(GLJournalLineForADAW))]
	public class GLJournalLineForADAWTest : GLJournalEnvironmentForADAWTest
	{
		public void TestPropertyValues()
		{
			BizObj.CompanyCode = "DAU";
			BizObj.BranchCode = "SYD";
			BizObj.GLAccount = "1234.56.78";
			BizObj.DepartmentCode = "FEA";
			BizObj.LocalAmount = "200";
			BizObj.Currency = "AUD";
			BizObj.Amount = "100";
			BizObj.JournalLineDescription = "Line Description";
			BizObj.OrganisationCode = "WISESYD";
			BizObj.SubAccountType1 = "ORG";
			BizObj.SubAccountValue1 = "CARGOWSYD";
			BizObj.SubAccountType2 = "SEG";
			BizObj.SubAccountValue2 = "ADDMIN";
			BizObj.SubAccounts.AddNew();

			AssertEquals("Company", "DAU", BizObj.CompanyCode);
			AssertEquals("Branch", "SYD", BizObj.BranchCode);
			AssertEquals("GLAccount", "1234.56.78", BizObj.GLAccount);
			AssertEquals("Department", "FEA", BizObj.DepartmentCode);
			AssertEquals("Amount", "200", BizObj.LocalAmount);
			AssertEquals("Currency", "AUD", BizObj.Currency);
			AssertNotNull(BizObj.CurrencyBizO);
			AssertEquals("Amount", "100", BizObj.Amount);
			AssertEquals("Description", "Line Description", BizObj.JournalLineDescription);
			AssertEquals("Organisation Code", "WISESYD", BizObj.OrganisationCode);
			AssertEquals("Sub Account Type 1", "ORG", BizObj.SubAccountType1);
			AssertEquals("Sub Account Value 1", "CARGOWSYD", BizObj.SubAccountValue1);
			AssertEquals("Sub Account Type 2", "SEG", BizObj.SubAccountType2);
			AssertEquals("Sub Account Value 2", "ADDMIN", BizObj.SubAccountValue2);
			AssertEquals("Sub Accounts", 1, BizObj.SubAccounts.Count);
		}

		public override void TestProperties()
		{
			base.TestProperties();

			CombineAssertions(() =>
			{
				AssertNotNull("GLAccount Property", BizObj.FindPropertyInfo(nameof(BizObj.GLAccount)));
				AssertNotNull("DepartmentCode Property", BizObj.FindPropertyInfo(nameof(BizObj.DepartmentCode)));
				AssertNotNull("LocalAmount Property", BizObj.FindPropertyInfo(nameof(BizObj.LocalAmount)));
				AssertNotNull("Currency Property", BizObj.FindPropertyInfo(nameof(BizObj.Currency)));
				AssertNotNull("Amount Property", BizObj.FindPropertyInfo(nameof(BizObj.Amount)));
				AssertNotNull("JournalLineDescription Property", BizObj.FindPropertyInfo(nameof(BizObj.JournalLineDescription)));
				AssertNotNull("OrganisationCode Property", BizObj.FindPropertyInfo(nameof(BizObj.OrganisationCode)));
				AssertNotNull("SubAccountType1 Property", BizObj.FindPropertyInfo(nameof(BizObj.SubAccountType1)));
				AssertNotNull("SubAccountValue1 Property", BizObj.FindPropertyInfo(nameof(BizObj.SubAccountValue1)));
				AssertNotNull("SubAccountType2 Property", BizObj.FindPropertyInfo(nameof(BizObj.SubAccountType2)));
				AssertNotNull("SubAccountValue2 Property", BizObj.FindPropertyInfo(nameof(BizObj.SubAccountValue2)));
				AssertNotNull("AttributeORG Property", BizObj.FindPropertyInfo(nameof(BizObj.AttributeORG)));
				AssertNotNull("AttributeOCG Property", BizObj.FindPropertyInfo(nameof(BizObj.AttributeOCG)));
				AssertNotNull("AttributeLFO Property", BizObj.FindPropertyInfo(nameof(BizObj.AttributeLFO)));
				AssertNotNull("AttributeLFE Property", BizObj.FindPropertyInfo(nameof(BizObj.AttributeLFE)));
				AssertNotNull("AttributeTIC Property", BizObj.FindPropertyInfo(nameof(BizObj.AttributeTIC)));
				AssertNotNull("AttributeSPR Property", BizObj.FindPropertyInfo(nameof(BizObj.AttributeSPR)));
			});
		}

		public void TestCurrency()
		{
			BizObj.Currency = "XXX";
			AssertNull(BizObj.CurrencyBizO);

			BizObj.Currency = "";
			AssertNull(BizObj.CurrencyBizO);

			BizObj.CompanyCode = "EDI";
			AssertNotNull("Currency is defaulted to company's local currency", BizObj.CurrencyBizO);
		}

		public void TestSchema()
		{
			AssertEquals("Schema.PK", "IAL_PK", GLJournalLineForADAW.Schema.PK);
		}

		public void TestMappingFieldMustHaveProperty()
		{
			var type = typeof(IGLJournalLine);
			var properties = type.GetProperties();

			foreach (var property in properties)
			{
				if (property.CanWrite)
				{
					AssertNotNull($"Mapping field {property.Name} should have property.", BizObj.FindPropertyInfo(property.Name));
				}
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GLJournalLineForADAW(Factory, HeaderBizObj);
		}

		protected override void SetUp()
		{
			base.SetUp();

			HeaderBizObj = new GLJournalHeaderForADAW(Factory, "H");
			BizObj = new GLJournalLineForADAW(Factory, HeaderBizObj);
			BizObj.RunPreSaveValidation();
		}

		GLJournalHeaderForADAW HeaderBizObj;

		GLJournalLineForADAW BizObj;

		#endregion
	}
}
