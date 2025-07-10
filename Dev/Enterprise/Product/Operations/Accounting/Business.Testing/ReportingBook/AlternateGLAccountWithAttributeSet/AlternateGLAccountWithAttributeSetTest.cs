using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AlternateGLAccountWithAttributeSet))]
	public class AlternateGLAccountWithAttributeSetTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetAlternateGLAccountNum()
		{
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			AssertNull(alternateGLAccountWithAttributeSet.AlternateGLAccount);

			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1010";
			AssertNotNull(alternateGLAccountWithAttributeSet.AlternateGLAccount);
			AssertAlternateGLAccount(alternateGLAccountWithAttributeSet);

			var originalAlternateGLAccount = alternateGLAccountWithAttributeSet.AlternateGLAccount;
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = AlternateGLAccount.AGA_AccountNum;

			Assert(originalAlternateGLAccount.IsDeleted);
			AssertNotEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.PK, originalAlternateGLAccount.PK);
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.PK, AlternateGLAccount.PK);
			AssertAlternateGLAccount(alternateGLAccountWithAttributeSet);
			Assert(alternateGLAccountWithAttributeSet.AlternateGLAccount.IsInDatabase);
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.HasChanges, true);

			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1010";
			AssertNotEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.PK, AlternateGLAccount.PK);
			AssertAlternateGLAccount(alternateGLAccountWithAttributeSet);
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.IsInDatabase, false);
		}

		public void TestSetAlternateGLAccountNum_AlternateGLAccountWillNotBeDeleteWhenExistOtherAttributeRelated()
		{
			var dissection = GLHeader.AlternateGLAccountDissections.AddNew();
			dissection.ADC_AAC_AlternateChart = Chart.PK;
			dissection.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO;
			dissection.ADC_SeparateNumbering = true;

			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			AssertNull(alternateGLAccountWithAttributeSet.AlternateGLAccount);

			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1010";
			var attribute = alternateGLAccountWithAttributeSet.Attributes.Cast<AccAlternateGLAccountAttribute>().FirstOrDefault();
			attribute.AAA_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO;
			attribute.AAA_Value = AccountingMasterFilesConstants.LFOCodes.FOR;
			AssertNotNull(alternateGLAccountWithAttributeSet.AlternateGLAccount);
			AssertAlternateGLAccount(alternateGLAccountWithAttributeSet);

			var anotherAlternateGLAccountWithAttributeSetDetails = new AlternateGLAccountWithAttributeSetDetails(GLHeader.PK, Chart.PK, 1, "BSH", "CSH", "KG");
			var anotherAlternateGLAccountWithAttributeSetWithSameAccountNumber = new AlternateGLAccountWithAttributeSet(anotherAlternateGLAccountWithAttributeSetDetails, Factory);
			anotherAlternateGLAccountWithAttributeSetWithSameAccountNumber.AlternateGLAccount = alternateGLAccountWithAttributeSet.AlternateGLAccount;
			attribute = anotherAlternateGLAccountWithAttributeSetWithSameAccountNumber.Attributes.Cast<AccAlternateGLAccountAttribute>().FirstOrDefault();
			attribute.AAA_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO;
			attribute.AAA_Value = AccountingMasterFilesConstants.LFOCodes.LOC;

			var originalAlternateGLAccount = alternateGLAccountWithAttributeSet.AlternateGLAccount;
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = AlternateGLAccount.AGA_AccountNum;

			Assert(!originalAlternateGLAccount.IsDeleted);
			AssertNotEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.PK, originalAlternateGLAccount.PK);
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.PK, AlternateGLAccount.PK);
			AssertAlternateGLAccount(alternateGLAccountWithAttributeSet);
			Assert(alternateGLAccountWithAttributeSet.AlternateGLAccount.IsInDatabase);
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.HasChanges, true);

			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1010";
			AssertNotEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.PK, AlternateGLAccount.PK);
			AssertAlternateGLAccount(alternateGLAccountWithAttributeSet);
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.IsInDatabase, false);
		}

		void AssertAlternateGLAccount(AlternateGLAccountWithAttributeSet alternateGLAccountWithAttributeSet)
		{
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AAC_AlternateChart, Chart.PK);
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AccountType, "BSH");
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_ReportSection, "OV");
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_DebitCredit, "DR");
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.CashFlowType, "CSH");
		}

		public void TestSetAlternateGLAccountNumForExistAlternateGLAccountWithoutParent()
		{
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			AssertEquals(false, alternateGLAccountWithAttributeSet.IsInDatabase);

			alternateGLAccountWithAttributeSet.AccountType = "TTL";
			alternateGLAccountWithAttributeSet.ParentGLAccountPK = Guid.Empty;
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "A";
			alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_Description = "Description";
			alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_DebitCredit = "DR";
			Factory.Save();

			AssertEquals(true, alternateGLAccountWithAttributeSet.IsInDatabase);

			var orignalAlternateGLAccount = alternateGLAccountWithAttributeSet.AlternateGLAccount;

			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "B";
			AssertEquals(true, alternateGLAccountWithAttributeSet.IsInDatabase);
			AssertEquals(orignalAlternateGLAccount.PK, alternateGLAccountWithAttributeSet.AlternateGLAccount.PK);
		}

		public void TestAddAttributeAfterSetAlternateGLAccountNum()
		{
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();

			AssertEquals(alternateGLAccountWithAttributeSet.Attributes.Any(), false);

			alternateGLAccountWithAttributeSet.ParentGLAccountPK = ZGuid.Empty;
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1010";

			AssertEquals(alternateGLAccountWithAttributeSet.Attributes.Any(), false);

			Assert(!alternateGLAccountWithAttributeSet.ORG.IsValid);
			Assert(string.IsNullOrEmpty(alternateGLAccountWithAttributeSet.OCG));
			Assert(string.IsNullOrEmpty(alternateGLAccountWithAttributeSet.LFE));
			Assert(string.IsNullOrEmpty(alternateGLAccountWithAttributeSet.LFO));
			Assert(string.IsNullOrEmpty(alternateGLAccountWithAttributeSet.TIC));
			Assert(string.IsNullOrEmpty(alternateGLAccountWithAttributeSet.SPR));
			AssertEquals(alternateGLAccountWithAttributeSet.Attributes.Any(), false);

			alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.ParentGLAccountPK = GLHeader.PK;
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1010";
			AssertEquals(alternateGLAccountWithAttributeSet.Attributes.Count, 1);
			AssertAttribute(alternateGLAccountWithAttributeSet.Attributes.Cast<AccAlternateGLAccountAttribute>().FirstOrDefault(), alternateGLAccountWithAttributeSet.AlternateGLAccount.PK, string.Empty, string.Empty);

			alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.ParentGLAccountPK = GLHeader.PK;
			alternateGLAccountWithAttributeSet.ORG = Org.PK;
			alternateGLAccountWithAttributeSet.OCG = "TPY";
			alternateGLAccountWithAttributeSet.LFO = "LOC";
			alternateGLAccountWithAttributeSet.LFE = "WEU";
			alternateGLAccountWithAttributeSet.TIC = "STI";
			alternateGLAccountWithAttributeSet.SPR = "SPS";
			alternateGLAccountWithAttributeSet.Attributes.RemoveAndDeleteAll();
			AssertEquals(alternateGLAccountWithAttributeSet.Attributes.Any(), false);

			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1010";
			AssertEquals(alternateGLAccountWithAttributeSet.Attributes.Count, 6);
			AssertAttribute(alternateGLAccountWithAttributeSet.Attributes.Cast<AccAlternateGLAccountAttribute>().FirstOrDefault(x => x.AAA_Attribute == "ORG"), alternateGLAccountWithAttributeSet.AlternateGLAccount.PK, "ORG", string.Empty, Org.PK);
			AssertAttribute(alternateGLAccountWithAttributeSet.Attributes.Cast<AccAlternateGLAccountAttribute>().FirstOrDefault(x => x.AAA_Attribute == "OCG"), alternateGLAccountWithAttributeSet.AlternateGLAccount.PK, "OCG", "TPY");
			AssertAttribute(alternateGLAccountWithAttributeSet.Attributes.Cast<AccAlternateGLAccountAttribute>().FirstOrDefault(x => x.AAA_Attribute == "LFO"), alternateGLAccountWithAttributeSet.AlternateGLAccount.PK, "LFO", "LOC");
			AssertAttribute(alternateGLAccountWithAttributeSet.Attributes.Cast<AccAlternateGLAccountAttribute>().FirstOrDefault(x => x.AAA_Attribute == "LFE"), alternateGLAccountWithAttributeSet.AlternateGLAccount.PK, "LFE", "WEU");
			AssertAttribute(alternateGLAccountWithAttributeSet.Attributes.Cast<AccAlternateGLAccountAttribute>().FirstOrDefault(x => x.AAA_Attribute == "TIC"), alternateGLAccountWithAttributeSet.AlternateGLAccount.PK, "TIC", "STI");
			AssertAttribute(alternateGLAccountWithAttributeSet.Attributes.Cast<AccAlternateGLAccountAttribute>().FirstOrDefault(x => x.AAA_Attribute == "SPR"), alternateGLAccountWithAttributeSet.AlternateGLAccount.PK, "SPR", "SPS");
		}

		public void TestAlternateGLAccountsWithAttributeSetCanAddNAV()
		{
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, true);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, true);
			Factory.Save();

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = Chart.PK;
			alternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
			alternateGLAccounts.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();
			alternateGLAccounts.ResetAlternateGLAccountsWithAttributeSet(alternateGLAccounts.ChartPK, alternateGLAccounts.ParentGLAccountPK);

			CombineAssertions(() =>
			{
				AssertEquals(12, alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Count);
				AssertEquals(1, alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Where(x => x.OCG == "TPY" && x.LFE == "NAV").Count());
				AssertEquals(1, alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Where(x => x.OCG == "INT" && x.LFE == "NAV").Count());
				AssertEquals(1, alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Where(x => x.OCG == "NAV" && x.LFE == "NAV").Count());
				AssertEquals(1, alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Where(x => x.OCG == "NAV" && x.LFE == "LOC").Count());
				AssertEquals(1, alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Where(x => x.OCG == "NAV" && x.LFE == "WEU").Count());
				AssertEquals(1, alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Where(x => x.OCG == "NAV" && x.LFE == "OEU").Count());
			});
		}

		public void TestSetAttribute()
		{
			AssertSetAttribute("ORG", string.Empty, Org.PK);
			AssertSetAttribute("OCG", "TPY");
			AssertSetAttribute("LFO", "LOC");
			AssertSetAttribute("LFE", "WEU");
			AssertSetAttribute("TIC", "STI");
			AssertSetAttribute("SPR", "SPS");

			void AssertSetAttribute(string attributeType, string attributeValue, ZGuid? attributeID = null)
			{
				var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();

				AssertEquals(alternateGLAccountWithAttributeSet.Attributes.Any(), false);

				alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1010";
				alternateGLAccountWithAttributeSet.Attributes.RemoveAndDeleteAll();

				AssertEquals(alternateGLAccountWithAttributeSet.Attributes.Count, 0);

				switch (attributeType)
				{
					case "ORG":
						alternateGLAccountWithAttributeSet.ORG = attributeID ?? ZGuid.Empty;
						break;
					case "OCG":
						alternateGLAccountWithAttributeSet.OCG = attributeValue;
						break;
					case "LFO":
						alternateGLAccountWithAttributeSet.LFO = attributeValue;
						break;
					case "LFE":
						alternateGLAccountWithAttributeSet.LFE = attributeValue;
						break;
					case "TIC":
						alternateGLAccountWithAttributeSet.TIC = attributeValue;
						break;
					case "SPR":
						alternateGLAccountWithAttributeSet.SPR = attributeValue;
						break;
				}

				AssertEquals(alternateGLAccountWithAttributeSet.Attributes.Count, 1);
				AssertAttribute(alternateGLAccountWithAttributeSet.Attributes.Cast<AccAlternateGLAccountAttribute>().FirstOrDefault(), alternateGLAccountWithAttributeSet.AlternateGLAccount.PK, attributeType, attributeValue, attributeID);
			}
		}

		void AssertAttribute(AccAlternateGLAccountAttribute attribute, ZGuid alternateGLAccountPK, string attributeType, string attributeValue, ZGuid? attributeValueID = null)
		{
			AssertEquals(attribute.AAA_Attribute, attributeType);
			AssertEquals(attribute.AAA_Value, attributeValue);
			if (attributeValueID != null)
			{
				AssertEquals(attribute.AAA_AttributeValueID, attributeValueID);
			}
			AssertEquals(attribute.AAA_AAC_AlternateChart, Chart.PK);
			AssertEquals(attribute.AAA_AG_GLHeader, GLHeader.PK);
			AssertEquals(attribute.AAA_AGA_AlternateGLAccount, alternateGLAccountPK);
		}

		public void TestSetParentGLAccountPK()
		{
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1010";

			AssertEquals(alternateGLAccountWithAttributeSet.ParentGLAccountPK, GLHeader.PK);
			Assert(alternateGLAccountWithAttributeSet.Attributes.Cast<AccAlternateGLAccountAttribute>().All(x => x.AAA_AG_GLHeader == GLHeader.PK));

			alternateGLAccountWithAttributeSet.ParentGLAccountPK = GLHeader2.PK;
			Assert(alternateGLAccountWithAttributeSet.Attributes.Cast<AccAlternateGLAccountAttribute>().All(x => x.AAA_AG_GLHeader == GLHeader2.PK));
		}

		public void TestSetChart()
		{
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1010";

			AssertEquals(alternateGLAccountWithAttributeSet.Chart.PK, Chart.PK);
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AAC_AlternateChart, Chart.PK);
			Assert(alternateGLAccountWithAttributeSet.Attributes.Cast<AccAlternateGLAccountAttribute>().All(x => x.AAA_AAC_AlternateChart == Chart.PK));

			alternateGLAccountWithAttributeSet.Chart = Chart2;
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AAC_AlternateChart, Chart2.PK);
			Assert(alternateGLAccountWithAttributeSet.Attributes.Cast<AccAlternateGLAccountAttribute>().All(x => x.AAA_AAC_AlternateChart == Chart2.PK));
		}

		public void TestSetAccountType()
		{
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1010";

			AssertEquals(alternateGLAccountWithAttributeSet.AccountType, "BSH");
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AccountType, "BSH");

			alternateGLAccountWithAttributeSet.AccountType = "P&L";
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AccountType, "P&L");
		}

		public void TestSetAttributes()
		{
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1020";

			AssertEquals(alternateGLAccountWithAttributeSet.ORG, ZGuid.Empty);
			AssertNullOrEmpty(alternateGLAccountWithAttributeSet.OCG);
			AssertNullOrEmpty(alternateGLAccountWithAttributeSet.LFE);
			AssertNullOrEmpty(alternateGLAccountWithAttributeSet.LFO);
			AssertNullOrEmpty(alternateGLAccountWithAttributeSet.TIC);
			AssertNullOrEmpty(alternateGLAccountWithAttributeSet.SPR);

			var attributes = new AccAlternateGLAccountAttributeCollection(Factory);
			SetAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, string.Empty, Org.PK);
			SetAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "TPY", ZGuid.Empty);
			SetAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, AccountingMasterFilesConstants.LFECodes.WEU, ZGuid.Empty);
			SetAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, AccountingMasterFilesConstants.LFOCodes.LOC, ZGuid.Empty);
			SetAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, AccountingMasterFilesConstants.TICCodes.STI, ZGuid.Empty);
			SetAttribute(AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, AccountingMasterFilesConstants.SPRCodes.SPS, ZGuid.Empty);
			AssertEquals(attributes.Count, 6);

			alternateGLAccountWithAttributeSet.Attributes = attributes;
			AssertEquals(alternateGLAccountWithAttributeSet.ORG, Org.PK);
			AssertEquals(alternateGLAccountWithAttributeSet.OCG, "TPY");
			AssertEquals(alternateGLAccountWithAttributeSet.LFE, "WEU");
			AssertEquals(alternateGLAccountWithAttributeSet.LFO, "LOC");
			AssertEquals(alternateGLAccountWithAttributeSet.TIC, "STI");
			AssertEquals(alternateGLAccountWithAttributeSet.SPR, "SPS");

			void SetAttribute(string attribute, string attributeValue, ZGuid attributeValueID)
			{
				var newAttribute = attributes.AddNew();
				newAttribute.AAA_Attribute = attribute;
				if (!string.IsNullOrEmpty(attributeValue))
				{
					newAttribute.AAA_Value = attributeValue;
				}

				if (attributeValueID != ZGuid.Empty)
				{
					newAttribute.AAA_AttributeValueID = attributeValueID;
				}
			}
		}

		public void TestAlternateGLAccountNumExistsInDatabase()
		{
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1100";
			Assert(alternateGLAccountWithAttributeSet.AlternateGLAccountNumExistsInDatabase("10.00.1100"));
		}

		public void TestOriginalAlternateGLAccountHasOtherParent()
		{
			var attribute = Factory.NewWithValidTestData<AccAlternateGLAccountAttribute>();
			attribute.AAA_AAC_AlternateChart = Chart.PK;
			attribute.AAA_AG_GLHeader = GLHeader2.PK;
			attribute.AAA_AGA_AlternateGLAccount = AlternateGLAccount.PK;
			Factory.Save();

			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.AccountType = "BSH";
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1100";
			Assert(alternateGLAccountWithAttributeSet.OriginalAlternateGLAccountHasOtherParent());
		}

		public void TestIsInDatabase()
		{
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			AssertEquals(false, alternateGLAccountWithAttributeSet.IsInDatabase);

			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1100";
			alternateGLAccountWithAttributeSet.ParentGLAccountPK = GLHeader.PK;
			Factory.Save();
			AssertEquals(true, alternateGLAccountWithAttributeSet.IsInDatabase);
		}

		public void TestDebitCredit()
		{
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = Factory.New<AccAlternateGLAccount>();
			alternateGLAccountWithAttributeSet.DebitCredit = string.Empty;
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_DebitCredit, string.Empty);
			Assert(alternateGLAccountWithAttributeSet.DebitCreditInfo.HasError("Please enter a DR/CR."));

			alternateGLAccountWithAttributeSet.DebitCredit = "XX";
			AssertEquals("XX", alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_DebitCredit);
			Assert(alternateGLAccountWithAttributeSet.DebitCreditInfo.HasError("Enter a valid DR/CR."));

			alternateGLAccountWithAttributeSet.ReadOnly = true;
			alternateGLAccountWithAttributeSet.Validation.ValidateDebitCredit();
			Assert(!alternateGLAccountWithAttributeSet.DebitCreditInfo.HasErrors());

			alternateGLAccountWithAttributeSet.ReadOnly = false;
			alternateGLAccountWithAttributeSet.DebitCredit = "DR";
			AssertEquals("DR", alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_DebitCredit);
			Assert(!alternateGLAccountWithAttributeSet.DebitCreditInfo.HasErrors());
		}

		public void TestDescription()
		{
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = Factory.New<AccAlternateGLAccount>();
			alternateGLAccountWithAttributeSet.Description = string.Empty;
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_Description, string.Empty);
			Assert(alternateGLAccountWithAttributeSet.DescriptionInfo.HasError("Please enter an Alternate Account Name."));

			alternateGLAccountWithAttributeSet.ReadOnly = true;
			alternateGLAccountWithAttributeSet.Validation.ValidateDescription();
			Assert(!alternateGLAccountWithAttributeSet.DescriptionInfo.HasErrors());

			alternateGLAccountWithAttributeSet.ReadOnly = false;
			alternateGLAccountWithAttributeSet.Description = "Desc";
			AssertEquals("Desc", alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_Description);
			Assert(!alternateGLAccountWithAttributeSet.DescriptionInfo.HasErrors());
		}

		public void TestReportSection()
		{
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = Factory.New<AccAlternateGLAccount>();
			alternateGLAccountWithAttributeSet.ReportSection = string.Empty;
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_ReportSection, string.Empty);
			Assert(alternateGLAccountWithAttributeSet.ReportSectionInfo.HasError("Please enter a Report Section."));

			alternateGLAccountWithAttributeSet.ReportSection = "XX";
			AssertEquals("XX", alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_ReportSection);
			Assert(alternateGLAccountWithAttributeSet.ReportSectionInfo.HasError("Enter a valid Report Section."));

			alternateGLAccountWithAttributeSet.ReadOnly = true;
			alternateGLAccountWithAttributeSet.Validation.ValidateReportSection();
			Assert(!alternateGLAccountWithAttributeSet.ReportSectionInfo.HasErrors());

			alternateGLAccountWithAttributeSet.ReadOnly = false;
			alternateGLAccountWithAttributeSet.ReportSection = "OV";
			AssertEquals("OV", alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_ReportSection);
			Assert(!alternateGLAccountWithAttributeSet.ReportSectionInfo.HasErrors());
		}

		public void TestTotalLevel()
		{
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = Factory.New<AccAlternateGLAccount>();
			alternateGLAccountWithAttributeSet.AccountType = "TTL";
			alternateGLAccountWithAttributeSet.TotalLevel = -10;
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_TotalLevel, -10);
			Assert(alternateGLAccountWithAttributeSet.TotalLevelInfo.HasError("Total Level should be between 1 and 999."));

			alternateGLAccountWithAttributeSet.ReadOnly = true;
			alternateGLAccountWithAttributeSet.Validation.ValidateTotalLevel();
			Assert(!alternateGLAccountWithAttributeSet.TotalLevelInfo.HasErrors());

			alternateGLAccountWithAttributeSet.ReadOnly = false;
			alternateGLAccountWithAttributeSet.TotalLevel = 10;
			AssertEquals(10, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_TotalLevel);
			Assert(!alternateGLAccountWithAttributeSet.TotalLevelInfo.HasErrors());
		}

		public void TestPrintSequence()
		{
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = Factory.New<AccAlternateGLAccount>();
			alternateGLAccountWithAttributeSet.PrintSequence = -10;
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_PrintSequence, -10);
			Assert(alternateGLAccountWithAttributeSet.PrintSequenceInfo.HasError("Print Sequence should be between 0 and 999."));

			alternateGLAccountWithAttributeSet.ReadOnly = true;
			alternateGLAccountWithAttributeSet.Validation.ValidatePrintSequence();
			Assert(!alternateGLAccountWithAttributeSet.PrintSequenceInfo.HasErrors());

			alternateGLAccountWithAttributeSet.ReadOnly = false;
			alternateGLAccountWithAttributeSet.PrintSequence = 0;
			AssertEquals(0, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_PrintSequence);
			Assert(!alternateGLAccountWithAttributeSet.PrintSequenceInfo.HasErrors());
		}

		public void TestPercentNum()
		{
			var alternateGLAccountWithAttributeSet = SetUpForPercentAndConsolidateAndAlternateAndTotalReference(Core.Constants.AccountType.Consolidation, Core.Constants.AccountType.BalanceSheetAccount, out var clnAlternateGLAccount, out var otherAlternateGLAccount, out var cln2AlternateGLAccount);
			alternateGLAccountWithAttributeSet.PercentNum = otherAlternateGLAccount.PK;
			AssertEquals(otherAlternateGLAccount.PK, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_PercentNum);
			Assert(alternateGLAccountWithAttributeSet.PercentNumInfo.HasError("Invalid Percent Account. A valid Percent Account must be 'CLN' or 'TTL' account type."));

			alternateGLAccountWithAttributeSet.PercentNum = clnAlternateGLAccount.PK;
			AssertEquals(clnAlternateGLAccount.PK, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_PercentNum);
			Assert(!alternateGLAccountWithAttributeSet.PercentNumInfo.HasErrors());

			alternateGLAccountWithAttributeSet.PercentNum = cln2AlternateGLAccount.PK;
			AssertEquals(cln2AlternateGLAccount.PK, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_PercentNum);
			Assert(alternateGLAccountWithAttributeSet.PercentNumInfo.HasError("The alternate account '3F99 - DES2' cannot be chosen here as it belongs to a different chart 'MG2 - Management Reporting', please choose another 'CLN or TTL' alternate account from the same chart 'MGT - Management Reporting'."));

			alternateGLAccountWithAttributeSet.ReadOnly = true;
			alternateGLAccountWithAttributeSet.Validation.ValidatePercentNum();
			Assert(!alternateGLAccountWithAttributeSet.PercentNumInfo.HasErrors());
		}

		public void TestAlternateNum()
		{
			var alternateGLAccountWithAttributeSet = SetUpForPercentAndConsolidateAndAlternateAndTotalReference(Core.Constants.AccountType.Alternate, Core.Constants.AccountType.BalanceSheetAccount, out var alternateAlternateGLAccount, out var otherAlternateGLAccount, out var alt2AlternateGLAccount);
			otherAlternateGLAccount.AGA_AGA_AlternateNum = alternateAlternateGLAccount.PK;
			otherAlternateGLAccount.Factory.Save();
			alternateGLAccountWithAttributeSet.AlternateNum = alternateAlternateGLAccount.PK;
			alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AAC_AlternateChart = Chart2.PK;
			AssertEquals(alternateAlternateGLAccount.PK, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_AlternateNum);
			Assert(alternateGLAccountWithAttributeSet.AlternateNumInfo.HasError("The Alternate Number is already used by another Alternate GL account 3F99."));

			alternateGLAccountWithAttributeSet.AlternateNum = otherAlternateGLAccount.PK;
			AssertEquals(otherAlternateGLAccount.PK, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_AlternateNum);
			Assert(alternateGLAccountWithAttributeSet.AlternateNumInfo.HasError("Invalid Alternate Account. A valid Alternate Account must be 'ALT' account type."));

			alternateGLAccountWithAttributeSet.AlternateNum = alt2AlternateGLAccount.PK;
			AssertEquals(alt2AlternateGLAccount.PK, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_AlternateNum);
			Assert(!alternateGLAccountWithAttributeSet.AlternateNumInfo.HasErrors());

			otherAlternateGLAccount.AGA_AGA_AlternateNum = ZGuid.Empty;
			otherAlternateGLAccount.Factory.Save();
			alternateGLAccountWithAttributeSet.AlternateNum = alternateAlternateGLAccount.PK;
			AssertEquals(alternateAlternateGLAccount.PK, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_AlternateNum);
			Assert(alternateGLAccountWithAttributeSet.AlternateNumInfo.HasError("The alternate account '2F99 - DES1' cannot be chosen here as it belongs to a different chart 'MGT - Management Reporting', please choose another 'ALT' alternate account from the same chart 'MG2 - Management Reporting'."));

			alternateGLAccountWithAttributeSet.ReadOnly = true;
			alternateGLAccountWithAttributeSet.Validation.ValidateAlternateNum();
			Assert(!alternateGLAccountWithAttributeSet.AlternateNumInfo.HasErrors());
		}

		public void TestConsolidationNum()
		{
			var alternateGLAccountWithAttributeSet = SetUpForPercentAndConsolidateAndAlternateAndTotalReference(Core.Constants.AccountType.Consolidation, Core.Constants.AccountType.BalanceSheetAccount, out var consolidateAccount, out var otherAlternateGLAccount, out var cln2AlternateGLAccount);
			alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AAC_AlternateChart = Chart2.PK;
			alternateGLAccountWithAttributeSet.ConsolidationNum = otherAlternateGLAccount.PK;
			AssertEquals(otherAlternateGLAccount.PK, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_ConsolidationNum);
			Assert(alternateGLAccountWithAttributeSet.ConsolidationNumInfo.HasError("Invalid Consolidate Account. A valid Consolidate Account must be 'CLN' account type."));

			alternateGLAccountWithAttributeSet.ConsolidationNum = cln2AlternateGLAccount.PK;
			AssertEquals(cln2AlternateGLAccount.PK, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_ConsolidationNum);
			Assert(!alternateGLAccountWithAttributeSet.ConsolidationNumInfo.HasErrors());

			alternateGLAccountWithAttributeSet.ConsolidationNum = consolidateAccount.PK;
			AssertEquals(consolidateAccount.PK, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_ConsolidationNum);
			Assert(alternateGLAccountWithAttributeSet.ConsolidationNumInfo.HasError("The alternate account '2F99 - DES1' cannot be chosen here as it belongs to a different chart 'MGT - Management Reporting', please choose another 'CLN' alternate account from the same chart 'MG2 - Management Reporting'."));

			alternateGLAccountWithAttributeSet.ReadOnly = true;
			alternateGLAccountWithAttributeSet.Validation.ValidateConsolidationNum();
			Assert(!alternateGLAccountWithAttributeSet.ConsolidationNumInfo.HasErrors());
		}

		public void TestConsolidationNum_ReadOnly()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart2.PK, "3F99", Core.Constants.AccountType.Total, description: "DES2", drCR: "DR", printSequence: 2);
			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = alternateGLAccount;

			Assert(!alternateGLAccountWithAttributeSet.ConsolidationNumInfo.ReadOnly);

			alternateGLAccountWithAttributeSet.AccountType = Core.Constants.AccountType.Header;
			Assert(alternateGLAccountWithAttributeSet.ConsolidationNumInfo.ReadOnly);

			alternateGLAccountWithAttributeSet.AccountType = Core.Constants.AccountType.Note;
			Assert(alternateGLAccountWithAttributeSet.ConsolidationNumInfo.ReadOnly);

			alternateGLAccountWithAttributeSet.AccountType = Core.Constants.AccountType.Rollup;
			Assert(alternateGLAccountWithAttributeSet.ConsolidationNumInfo.ReadOnly);

			alternateGLAccountWithAttributeSet.AccountType = Core.Constants.AccountType.Group;
			Assert(alternateGLAccountWithAttributeSet.ConsolidationNumInfo.ReadOnly);

			alternateGLAccountWithAttributeSet.AccountType = Core.Constants.AccountType.ChartOnly;
			Assert(alternateGLAccountWithAttributeSet.ConsolidationNumInfo.ReadOnly);

			alternateGLAccountWithAttributeSet.AccountType = Core.Constants.AccountType.Consolidation;
			Assert(alternateGLAccountWithAttributeSet.ConsolidationNumInfo.ReadOnly);
		}

		public void TestValidateAGA_AGA_HeaderDependsOnTotal()
		{
			var alternateGLAccountWithAttributeSet = SetUpForPercentAndConsolidateAndAlternateAndTotalReference(Core.Constants.AccountType.Total, Core.Constants.AccountType.BalanceSheetAccount, out var ttlAlternateGLAccount, out var otherAlternateGLAccount, out var ttl2AlternateGLAccount);
			alternateGLAccountWithAttributeSet.AccountType = Core.Constants.AccountType.Header;
			alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AAC_AlternateChart = Chart2.PK;
			alternateGLAccountWithAttributeSet.HeaderDependsOnTotal = otherAlternateGLAccount.PK;

			AssertEquals(otherAlternateGLAccount.PK, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_HeaderDependsOnTotal);
			Assert(alternateGLAccountWithAttributeSet.HeaderDependsOnTotalInfo.HasError("Invalid Total Reference. A valid Total Reference must be 'TTL' account type."));

			alternateGLAccountWithAttributeSet.HeaderDependsOnTotal = ttl2AlternateGLAccount.PK;
			AssertEquals(ttl2AlternateGLAccount.PK, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_HeaderDependsOnTotal);
			Assert(!alternateGLAccountWithAttributeSet.HeaderDependsOnTotalInfo.HasErrors());

			alternateGLAccountWithAttributeSet.HeaderDependsOnTotal = ttlAlternateGLAccount.PK;
			AssertEquals(ttlAlternateGLAccount.PK, alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AGA_HeaderDependsOnTotal);
			Assert(alternateGLAccountWithAttributeSet.HeaderDependsOnTotalInfo.HasError("The alternate account '2F99 - DES1' cannot be chosen here as it belongs to a different chart 'MGT - Management Reporting', please choose another 'TTL' alternate account from the same chart 'MG2 - Management Reporting'."));

			alternateGLAccountWithAttributeSet.ReadOnly = true;
			alternateGLAccountWithAttributeSet.Validation.ValidateHeaderDependsOnTotal();
			Assert(!alternateGLAccountWithAttributeSet.HeaderDependsOnTotalInfo.HasErrors());
		}

		AlternateGLAccountWithAttributeSet SetUpForPercentAndConsolidateAndAlternateAndTotalReference(string validAccountType, string otherAccountType, out AccAlternateGLAccount notCurrentChartAlternateGLAccount, out AccAlternateGLAccount otherAlternateGLAccount, out AccAlternateGLAccount currentChartAlternateGLAccount)
		{
			var newFactoryCreator = new TestObjectCreator(new BusinessObjectFactory());
			notCurrentChartAlternateGLAccount = newFactoryCreator.CreateAccAlternateGlAccount(Chart.PK, "2F99", validAccountType, description: "DES1", drCR: "DR", printSequence: 1);
			otherAlternateGLAccount = newFactoryCreator.CreateAccAlternateGlAccount(Chart.PK, "3F99", otherAccountType, description: "DES2", drCR: "DR", printSequence: 2);
			currentChartAlternateGLAccount = newFactoryCreator.CreateAccAlternateGlAccount(Chart2.PK, "3F99", validAccountType, description: "DES2", drCR: "DR", printSequence: 2);

			newFactoryCreator.Factory.Save();

			var alternateGLAccountWithAttributeSet = (AlternateGLAccountWithAttributeSet)GetNewBusinessObject();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = Factory.New<AccAlternateGLAccount>();
			return alternateGLAccountWithAttributeSet;
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var alternateGLAccountWithAttributeSetDetails = new AlternateGLAccountWithAttributeSetDetails(GLHeader.PK, Chart.PK, 0, "BSH", "CSH", "KG");
			return new AlternateGLAccountWithAttributeSet(alternateGLAccountWithAttributeSetDetails, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);

			Chart = Creator.CreateAlternateChart("MGT", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			Chart2 = Creator.CreateAlternateChart("MG2", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			Creator.CreateAccAlternateChartFormat(Chart, 1, "X", "2", ".");
			Creator.CreateAccAlternateChartFormat(Chart2, 1, "X", "2", ".");
			Factory.Save();

			GLHeader = Creator.CreateAccGLHeader("10.00.1010", AccGLHeader.Constants.SectionTypes.Codes.Overheads, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			GLHeader.AG_CashFlowType = "CSH";
			GLHeader.AG_StatisticalUnits = "KG";
			GLHeader2 = Creator.CreateAccGLHeader("20.00.1010", AccGLHeader.Constants.SectionTypes.Codes.Overheads, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);

			AlternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "10.00.1100", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1);
			Org = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();
		}

		#endregion

		AccAlternateChart Chart;
		AccAlternateChart Chart2;
		AccGLHeader GLHeader;
		AccGLHeader GLHeader2;
		AccAlternateGLAccount AlternateGLAccount;
		OrgHeader Org;
		TestObjectCreator Creator;
	}
}
