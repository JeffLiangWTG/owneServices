using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(KREntryCustomsBillsViewCollection))]
	sealed class KREntryCustomsBillsViewCollectionTest : ActiveBusinessObjectCollectionTestCase<KREntryCustomsBillsViewCollection>
	{
		public void TestGetCustomsDisbursementBills()
		{
			CreateKREntryCustomsBillsView("I", "PYC");
			CreateKREntryCustomsBillsView("D", "PYC");
			CreateKREntryCustomsBillsView("N", "PYC");
			CreateKREntryCustomsBillsView("I", "PYI");
			CreateKREntryCustomsBillsView("D", "PYI");
			CreateKREntryCustomsBillsView("N", "PYI");

			var collection = new KREntryCustomsBillsViewCollection(Factory, new ZQuery(), GlbBranch.CurrentBranch.Company.PK);
			AssertEquals(6, collection.Count);

			collection = new KREntryCustomsBillsViewCollection(Factory, new KREntryCustomsBillsView.Loader(Factory).GetCustomsDisbursementBills(entry.EntryNumber), GlbBranch.CurrentBranch.Company.PK);
			AssertEquals(2, collection.Count);

			void CreateKREntryCustomsBillsView(string type, string paymentStatus)
			{
				var view = Factory.New<KREntryCustomsBillsView>();
				view.KEB_StatementType = type;
				view.KEB_PaymentStatus = paymentStatus;
				view.KEB_ImportEntryNum = entry.EntryNumber;
				view.KEB_BranchPK = GlbBranch.CurrentBranch.PK;
				view.KEB_GC = GlbBranch.CurrentBranch.Company.PK;
			}
		}

		public void TestRelationshipFilter()
		{
			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Name = "Test Company Name";
			newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			var newBranch1 = newCompany.Branches.AddNew();
			newBranch1.GB_Code = "BR1";
			var newBranch2 = newCompany.Branches.AddNew();
			newBranch2.GB_Code = "BR2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GC = newCompany.PK;
			declaration.JE_GB = newBranch1.PK;
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";

			var entryNum = declaration.CustomsEntryHeaders.AddNew().EntryNumbers.AddNew();
			entryNum.CE_EntryType = "IMP";
			entryNum.CE_EntryNum = "1234522123450X";

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_GC = newCompany.PK;
			statement.B2_StatementType = "D";
			statement.B2_PaymentStatus = "PYC";
			statement.B2_StatementNumber = "1111111111111111111";

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = entryNum.CE_EntryNum;
			Factory.Save();

			var coll = new KREntryCustomsBillsViewCollection(Factory, new ZQuery(), newCompany.PK);
			AssertEquals(1, coll.Count);

			declaration.JE_GB = newBranch2.PK;
			Factory.Save();

			coll = new KREntryCustomsBillsViewCollection(Factory, new ZQuery(), newCompany.PK);
			AssertEquals(1, coll.Count);

			coll = new KREntryCustomsBillsViewCollection(Factory, new ZQuery(), GlbCompany.CurrentCompany.PK);
			AssertEquals(0, coll.Count);
		}

		protected override KREntryCustomsBillsViewCollection GetCollectionToTest() => new KREntryCustomsBillsViewCollection(Factory, new KREntryCustomsBillsView.Loader(Factory).GetCustomsDisbursementBills(entry.EntryNumber), GlbCompany.CurrentCompany.PK);

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1234520000045M";
		}
		CusEntryHeader entry;
	}
}
