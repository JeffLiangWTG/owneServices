using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.CA.Business.CusBondDetailCollection;

namespace Enterprise.Customs.CA.Business.Testing
{
	class BondDetailsDefaulterTest : TestCaseWithFactory
	{
		public void TestDefaultWhenBondTypeChanges()
		{
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(-1);
			declaration.CA_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertNullOrEmpty(declaration.CA_BondNo);
			AssertNullOrEmpty(declaration.CA_SuretyCode);

			var org = GetOrgHeaderWithBondDetails();
			declaration.JE_OH_Importer = org.PK;
			declaration.CA_BondType = BondTypeList.Codes.ContinuousBond;
			AssertEquals(BondTypeList.Codes.ContinuousBond, declaration.CA_BondType);
			AssertEquals("00003", declaration.CA_BondNo);
			AssertEquals("003", declaration.CA_SuretyCode);

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = org.PK;
			declaration.CA_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertEquals(BondTypeList.Codes.SingleTransactionBond, declaration.CA_BondType);
			AssertEquals("00001", declaration.CA_BondNo);
			AssertEquals("001", declaration.CA_SuretyCode);

			declaration.CA_BondType = ZString.Empty;
			AssertNullOrEmpty(declaration.CA_BondType);
			AssertNullOrEmpty(declaration.CA_BondNo);
			AssertNullOrEmpty(declaration.CA_SuretyCode);
		}

		public void TestDefault()
		{
			AssertNullOrEmpty(declaration.CA_BondType);
			AssertNullOrEmpty(declaration.CA_BondNo);
			AssertNullOrEmpty(declaration.CA_SuretyCode);

			declaration.CA_EstReleaseDate = ZDateTime.Today.AddDays(-1);
			var org = GetOrgHeaderWithBondDetails();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(BondTypeList.Codes.ContinuousBond, declaration.CA_BondType);
			AssertEquals("00003", declaration.CA_BondNo);
			AssertEquals("003", declaration.CA_SuretyCode);

			declaration.CA_EstReleaseDate = ZDateTime.Today;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = org.PK;
			declaration.JE_MessageType = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(BondTypeList.Codes.ContinuousBond, declaration.CA_BondType);
			AssertEquals("00002", declaration.CA_BondNo);
			AssertEquals("002", declaration.CA_SuretyCode);
		}

		public void TestGetBondDetailsStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var bondCollection = new CusBondDetailCollection(org);
			var bondData1 = bondCollection.AddNew();
			bondData1.PW_BondType = BondTypeList.Codes.SingleTransactionBond;
			bondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			bondData1.PW_BondNumber = "00001";
			bondData1.PW_SuretyCode = "001";
			var bondData2 = bondCollection.AddNew();
			bondData2.PW_BondType = BondTypeList.Codes.ContinuousBond;
			bondData2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			bondData2.PW_BondNumber = "00002";
			bondData2.PW_SuretyCode = "002";

			declaration.JE_OH_Importer = org.PK;
			AssertEquals(BondDetailsStatus.BondExist, new BondDetailsDefaulter().GetBondDetailsStatus(declaration, ZString.Empty));
			AssertEquals(BondDetailsStatus.BondExist, new BondDetailsDefaulter().GetBondDetailsStatus(declaration, BondTypeList.Codes.SingleTransactionBond));
			AssertEquals(BondDetailsStatus.BondExist, new BondDetailsDefaulter().GetBondDetailsStatus(declaration, BondTypeList.Codes.ContinuousBond));

			bondData1.PW_BondExpiryDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(BondDetailsStatus.BondExist, new BondDetailsDefaulter().GetBondDetailsStatus(declaration, ZString.Empty));
			AssertEquals(BondDetailsStatus.AllExpired, new BondDetailsDefaulter().GetBondDetailsStatus(declaration, BondTypeList.Codes.SingleTransactionBond));
			AssertEquals(BondDetailsStatus.BondExist, new BondDetailsDefaulter().GetBondDetailsStatus(declaration, BondTypeList.Codes.ContinuousBond));

			bondData2.PW_BondExpiryDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(BondDetailsStatus.AllExpired, new BondDetailsDefaulter().GetBondDetailsStatus(declaration, ZString.Empty));
			AssertEquals(BondDetailsStatus.AllExpired, new BondDetailsDefaulter().GetBondDetailsStatus(declaration, BondTypeList.Codes.SingleTransactionBond));
			AssertEquals(BondDetailsStatus.AllExpired, new BondDetailsDefaulter().GetBondDetailsStatus(declaration, BondTypeList.Codes.ContinuousBond));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		}
		JobDeclaration declaration;

		OrgHeader GetOrgHeaderWithBondDetails()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var bondCollection = new CusBondDetailCollection(org);
			var bondData1 = bondCollection.AddNew();
			bondData1.PW_BondType = BondTypeList.Codes.SingleTransactionBond;
			bondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			bondData1.PW_BondNumber = "00001";
			bondData1.PW_SuretyCode = "001";
			var bondData2 = bondCollection.AddNew();
			bondData2.PW_BondType = BondTypeList.Codes.ContinuousBond;
			bondData2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			bondData2.PW_BondNumber = "00002";
			bondData2.PW_SuretyCode = "002";
			var bondData3 = bondCollection.AddNew();
			bondData3.PW_BondType = BondTypeList.Codes.ContinuousBond;
			bondData3.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-5);
			bondData3.PW_BondExpiryDate = ZDateTime.Today.AddDays(-1);
			bondData3.PW_BondNumber = "00003";
			bondData3.PW_SuretyCode = "003";
			return org;
		}

		#endregion
	}
}
