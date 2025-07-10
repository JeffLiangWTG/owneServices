using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class ApplyMultiSubAccountFilterTest : ScriptTest
	{
		public void TestApplyMultiSubAccountFilter()
		{
			ApplyMultiSubAccountFilter(OrgHeaderSchema.Constants.Prefix, TestObjectCreator.Creditor1.PK, TestObjectCreator.Creditor2.PK, GlbStaffSchema.Constants.Prefix);
			ApplyMultiSubAccountFilter(GlbStaffSchema.Constants.Prefix, TestObjectCreator.Staff.PK, TestObjectCreator.CreateStaff("AAA").PK, OrgHeaderSchema.Constants.Prefix);
			ApplyMultiSubAccountFilter(AccGroupsSchema.Constants.Prefix, TestObjectCreator.CreateSalesGroup("BBB").PK, TestObjectCreator.CreateSalesGroup("CCC").PK, GlbStaffSchema.Constants.Prefix);
			ApplyMultiSubAccountFilter(GlbGroupSchema.Constants.Prefix, TestObjectCreator.CreateStaffGroup("EEE").PK, TestObjectCreator.CreateStaffGroup("DDD").PK, GlbStaffSchema.Constants.Prefix);

			void ApplyMultiSubAccountFilter(ZString subAccountType1, ZGuid firstSubAccountValue1, ZGuid firstSubAccountValue2, ZString subAccountType2)
			{
				var firstSubAccountType = SubAccountCodeConverter.ConvertSubAccountDBParentTableCodeToSubClassCode(subAccountType1);
				var secondSubAccountType = SubAccountCodeConverter.ConvertSubAccountDBParentTableCodeToSubClassCode(subAccountType2);
				var glAccount1 = TestObjectCreator.CreateGLHeaderWithSubAccount(subAccountType1, false);
				var glAccount2 = TestObjectCreator.CreateGLHeaderWithSubAccount(subAccountType1, false);
				TestObjectCreator.CreateGLHeaderSubAccount(glAccount2, subAccountType2, false);

				var excludeFirstSubAccountTypes = new List<string>(new string[] { Core.Constants.SubAccountType.Organization, Core.Constants.SubAccountType.SalesGroup, Core.Constants.SubAccountType.StaffAndResources, Core.Constants.SubAccountType.StaffGroup });
				excludeFirstSubAccountTypes.Remove(firstSubAccountType);

				AssertEquals(1, glAccount1.SubAccountTypes.Count);
				AssertEquals(2, glAccount2.SubAccountTypes.Count);

				var mismatchedSubClassParentId = ZGuid.NewZGuid();

				#region line

				var header1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);
				var line1 = (InvoicingLineBase)header1.Lines.AddNew();
				line1.AL_AG = glAccount1.PK;

				var header2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);
				var line2 = (InvoicingLineBase)header2.Lines.AddNew();
				line2.AL_AG = glAccount2.PK;

				line1.SubAccounts.Cast<TransactionLineSubAccount>().FirstOrDefault(x => x.AL1_SubClassParentTableCode == subAccountType1).AL1_SubClassParentId = firstSubAccountValue1;
				line2.SubAccounts.Cast<TransactionLineSubAccount>().FirstOrDefault(x => x.AL1_SubClassParentTableCode == subAccountType1).AL1_SubClassParentId = firstSubAccountValue2;
				Factory.Save();

				AssertEquals(1, line1.SubAccounts.Count);
				AssertEquals(1, line2.SubAccounts.Count);

				#endregion

				AssertApplyMultiSubAccountFilterWithHeaderOrLine(line1.PK, line2.PK);

				#region header

				var journal1 = TestObjectCreator.CreateJournal<APJournal>(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
				journal1.AH_AG = glAccount1.PK;

				var journal2 = TestObjectCreator.CreateJournal<APJournal>(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
				journal2.AH_AG = glAccount2.PK;

				journal1.SubAccounts.Cast<JournalSubAccount>().FirstOrDefault(x => x.AHS_SubClassParentTableCode == subAccountType1).AHS_SubClassParentId = firstSubAccountValue1;
				journal2.SubAccounts.Cast<JournalSubAccount>().FirstOrDefault(x => x.AHS_SubClassParentTableCode == subAccountType1).AHS_SubClassParentId = firstSubAccountValue2;
				Factory.Save();

				AssertEquals(1, journal1.SubAccounts.Count);
				AssertEquals(1, journal2.SubAccounts.Count);

				#endregion

				AssertApplyMultiSubAccountFilterWithHeaderOrLine(journal1.PK, journal2.PK);

				void AssertApplyMultiSubAccountFilterWithHeaderOrLine(ZGuid parent1PK, ZGuid parent2PK)
				{
					AssertEquals(1, glAccount1.SubAccountTypes.Count);
					AssertEquals(2, glAccount2.SubAccountTypes.Count);

					AssertApplyMultiSubAccountFilter(1, "", null, parent1PK, null, "1", "match all data when both the subaccounttype and SubAccountPK are empty");
					AssertApplyMultiSubAccountFilter(1, "", null, parent1PK, glAccount1.PK, "1", "match all data when both the subaccounttype and SubAccountPK are empty");
					AssertApplyMultiSubAccountFilter(1, firstSubAccountType, null, parent1PK, null, "1", "match table code");
					AssertApplyMultiSubAccountFilter(1, firstSubAccountType, null, parent1PK, glAccount1.PK, "1", "match table code");
					AssertApplyMultiSubAccountFilter(1, firstSubAccountType, firstSubAccountValue1, parent1PK, null, "1", "match table code and parent id");
					AssertApplyMultiSubAccountFilter(1, firstSubAccountType, firstSubAccountValue1, parent1PK, glAccount1.PK, "1", "match table code and parent id");
					AssertApplyMultiSubAccountFilter(1, firstSubAccountType, mismatchedSubClassParentId, parent1PK, null, "0", "match the table code but not the parent id");
					AssertApplyMultiSubAccountFilter(1, firstSubAccountType, mismatchedSubClassParentId, parent1PK, glAccount1.PK, "0", "match the table code but not the parent id");
					foreach (var subAccountType in excludeFirstSubAccountTypes)
					{
						AssertApplyMultiSubAccountFilter(1, subAccountType, null, parent1PK, null, "0", "mismatched table code");
						AssertApplyMultiSubAccountFilter(1, subAccountType, null, parent1PK, glAccount1.PK, "0", "mismatched table code");
					}

					AssertApplyMultiSubAccountFilter(1, "", null, parent2PK, null, "1", "match all data when both the subaccounttype and SubAccountPK are empty");
					AssertApplyMultiSubAccountFilter(1, "", null, parent2PK, glAccount2.PK, "1", "match all data when both the subaccounttype and SubAccountPK are empty");
					AssertApplyMultiSubAccountFilter(1, firstSubAccountType, null, parent2PK, null, "1", "match table code");
					AssertApplyMultiSubAccountFilter(1, firstSubAccountType, null, parent2PK, glAccount2.PK, "1", "match table code");
					AssertApplyMultiSubAccountFilter(1, firstSubAccountType, firstSubAccountValue2, parent2PK, null, "1", "match table code and parent id");
					AssertApplyMultiSubAccountFilter(1, firstSubAccountType, firstSubAccountValue2, parent2PK, glAccount2.PK, "1", "match table code and parent id");
					AssertApplyMultiSubAccountFilter(1, firstSubAccountType, mismatchedSubClassParentId, parent2PK, null, "0", "match the table code but not the parent id");
					AssertApplyMultiSubAccountFilter(1, firstSubAccountType, mismatchedSubClassParentId, parent2PK, glAccount2.PK, "0", "match the table code but not the parent id");
					foreach (var subAccountType in excludeFirstSubAccountTypes)
					{
						AssertApplyMultiSubAccountFilter(1, subAccountType, null, parent2PK, null, "0", "mismatched table code");

						if (subAccountType == secondSubAccountType)
						{
							AssertApplyMultiSubAccountFilter(1, subAccountType, null, parent2PK, glAccount2.PK, "1", "matched table code in gl account");
						}
						else
						{
							AssertApplyMultiSubAccountFilter(1, subAccountType, null, parent2PK, glAccount2.PK, "0", "mismatched table code");
						}
					}

					AssertApplyMultiSubAccountFilter(0, "", null, parent1PK, null, "0", "header/line has a parent id");
					AssertApplyMultiSubAccountFilter(0, "", null, parent1PK, glAccount1.PK, "0", "header/line has a parent id");
					AssertApplyMultiSubAccountFilter(0, firstSubAccountType, null, parent1PK, null, "0", "header/line has a parent id");
					AssertApplyMultiSubAccountFilter(0, firstSubAccountType, null, parent1PK, glAccount1.PK, "0", "header/line has a parent id");
					AssertApplyMultiSubAccountFilter(0, firstSubAccountType, firstSubAccountValue1, parent1PK, null, "0", "header/line has a parent id");
					AssertApplyMultiSubAccountFilter(0, firstSubAccountType, firstSubAccountValue1, parent1PK, glAccount1.PK, "0", "header/line has a parent id");
					AssertApplyMultiSubAccountFilter(0, firstSubAccountType, mismatchedSubClassParentId, parent1PK, null, "0", "header/line has a parent id");
					AssertApplyMultiSubAccountFilter(0, firstSubAccountType, mismatchedSubClassParentId, parent1PK, glAccount1.PK, "0", "header/line has a parent id");
					foreach (var subAccountType in excludeFirstSubAccountTypes)
					{
						AssertApplyMultiSubAccountFilter(0, secondSubAccountType, null, parent1PK, null, "0", "header/line has a parent id");
						AssertApplyMultiSubAccountFilter(0, secondSubAccountType, null, parent1PK, glAccount1.PK, "0", "header/line has a parent id");
					}

					AssertApplyMultiSubAccountFilter(0, "", null, parent2PK, null, "0", "header/line has a parent id");
					AssertApplyMultiSubAccountFilter(0, "", null, parent2PK, glAccount2.PK, "1", "match gl account 2");
					AssertApplyMultiSubAccountFilter(0, firstSubAccountType, null, parent2PK, null, "0", "header/line has a parent id");
					AssertApplyMultiSubAccountFilter(0, firstSubAccountType, null, parent2PK, glAccount2.PK, "0", "header/line has a parent id");
					AssertApplyMultiSubAccountFilter(0, firstSubAccountType, firstSubAccountValue2, parent2PK, null, "0", "header/line has a parent id");
					AssertApplyMultiSubAccountFilter(0, firstSubAccountType, firstSubAccountValue2, parent2PK, glAccount2.PK, "0", "header/line has a parent id");
					AssertApplyMultiSubAccountFilter(0, firstSubAccountType, mismatchedSubClassParentId, parent2PK, null, "0", "header/line has a parent id");
					AssertApplyMultiSubAccountFilter(0, firstSubAccountType, mismatchedSubClassParentId, parent2PK, glAccount2.PK, "0", "header/line has a parent id");
					foreach (var subAccountType in excludeFirstSubAccountTypes)
					{
						AssertApplyMultiSubAccountFilter(0, subAccountType, null, parent2PK, null, "0", "header/line has a parent id");

						if (subAccountType == secondSubAccountType)
						{
							AssertApplyMultiSubAccountFilter(0, subAccountType, null, parent2PK, glAccount2.PK, "1", "match gl account 2");
						}
						else
						{
							AssertApplyMultiSubAccountFilter(0, subAccountType, null, parent2PK, glAccount2.PK, "0", "header/line has a parent id");
						}
					}
				}
			}

			void AssertApplyMultiSubAccountFilter(int useSubAccountValue, string subAccountType, ZGuid? subAccountPK, ZGuid subAccountParentId, ZGuid? additionalSubAccountParentId, string expectedResult, string message = "")
			{
				var result = RunScript(useSubAccountValue, subAccountType, subAccountPK, subAccountParentId, additionalSubAccountParentId);
				AssertEquals(1, result.Rows.Count);
				AssertEquals(message, expectedResult, result.Rows[0]["Result"].ToString());
			}
		}

		DataTable RunScript(int useSubAccountValue, string subAccountType, ZGuid? subAccountPK, ZGuid subAccountParentId, ZGuid? additionalSubAccountParentId)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, $@"
				SELECT * FROM ApplyMultiSubAccountFilter(
				{useSubAccountValue},
				'{subAccountType}',
				{(subAccountPK.HasValue ? string.Format("'{0}'", subAccountPK.ToString()) : "NULL")},
				'{subAccountParentId.ToString()}',
				{(additionalSubAccountParentId.HasValue ? string.Format("'{0}'", additionalSubAccountParentId.ToString()) : "NULL")})");
		}
	}
}


