using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(KREntryCustomsBillsView))]
	sealed class KREntryCustomsBillsViewTest : EnterpriseBusinessObjectTestCase
	{
		// Not relevant for BizOs generated from views
		public override void TestSaveAndDeleteBusinessObject()
		{
		}

		[TestedType(typeof(KREntryCustomsBillsView.Loader))]
		class Test : LoaderTestCase
		{
			public void TestDefaultFilter()
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_ApplicationCode = "BLT";

				var entryNum1 = declaration.CustomsEntryHeaders.AddNew().EntryNumbers.AddNew();
				entryNum1.CE_EntryType = "IMP";
				entryNum1.CE_EntryNum = "1234522123450X";

				var statement1 = Factory.New<CusStatementHeader>();
				statement1.B2_StatementType = "D";
				statement1.B2_PaymentStatus = "PYC";
				statement1.B2_StatementNumber = "1111111111111111111";

				var statementLine1 = statement1.StatementLines.AddNew();
				statementLine1.B3_EntryNum = entryNum1.CE_EntryNum;

				var entryNum2 = declaration.CustomsEntryHeaders.AddNew().EntryNumbers.AddNew();
				entryNum2.CE_EntryType = "IMP";
				entryNum2.CE_EntryNum = "1234522123451X";

				var statement2 = Factory.New<CusStatementHeader>();
				statement2.B2_StatementType = "D";
				statement2.B2_PaymentStatus = "PYI";
				statement2.B2_StatementNumber = "2222222222222222222";

				var statementLine2 = statement2.StatementLines.AddNew();
				statementLine2.B3_EntryNum = entryNum2.CE_EntryNum;
				Factory.Save();

				var collection = new KREntryCustomsBillsViewCollection(Factory, new KREntryCustomsBillsView.Loader(Factory).GetQueryFor5UL(), GlbCompany.CurrentCompany.PK);
				AssertEquals(1, collection.Count);

				collection = new KREntryCustomsBillsViewCollection(Factory, new KREntryCustomsBillsView.Loader(Factory).GetCustomsDisbursementBills(entryNum1.CE_EntryNum), GlbCompany.CurrentCompany.PK);
				AssertEquals(1, collection.Count);
			}

			protected override BusinessObject.Loader GetNewLoaderToTest()
			{
				return new KREntryCustomsBillsView.Loader(Factory);
			}
		}
	}
}
