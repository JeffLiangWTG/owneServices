using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(MoveLicencesToNewEnterpriseIDBizO))]
	public class MoveLicencesToNewEnterpriseIDBizOTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			LicenceEnterprise licEntForBizOPropertyTest = this.licEntForBizOPropertyTest;
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();
			MoveLicencesToNewEnterpriseIDBizO newBizO = new MoveLicencesToNewEnterpriseIDBizO(LicEntToPassToCtor, org, null);
			return newBizO;
		}

		public void TestCtor()
		{
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			MoveLicencesToNewEnterpriseIDBizOForTest moveBizO = new MoveLicencesToNewEnterpriseIDBizOForTest(LicEntToPassToCtor, org);
			AssertEquals("licEntToCopyFrom should contain LicenceEnterprise passed to ctor of MoveLicencesToNewEnterpriseIDBizO", LicEntToPassToCtor, moveBizO.licEntToCopyFrom);
		}

		public void TestLicenceEnterpriseList()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			MoveLicencesToNewEnterpriseIDBizO moveBizO = (MoveLicencesToNewEnterpriseIDBizO)GetNewBusinessObject();
			moveBizO.licEntToCopyFrom.LE_EnterpriseID = "AA1";
			moveBizO.LicenceEnterpriseList.Load();
			int licenceEnterprisesAtStartOfList = moveBizO.LicenceEnterpriseList.Count;
			Assert("MoveLicencesToNewEnterpriseIDBizO.LicenceEnterpriseList should never contain LicenceEnterprise passed to this bizO's constructor", !moveBizO.LicenceEnterpriseList.Contains(LicEntToPassToCtor));

			Factory.Save();
			org1.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseID = "XXA";
			Factory.Save();
			moveBizO = (MoveLicencesToNewEnterpriseIDBizO)GetNewBusinessObject();
			moveBizO.licEntToCopyFrom.LE_EnterpriseID = "AA2";
			moveBizO.LicenceEnterpriseList.Load();
			AssertEquals("Licence Enterprise List has one extra entry", 1 + licenceEnterprisesAtStartOfList, moveBizO.LicenceEnterpriseList.Count);
			Assert("MoveLicencesToNewEnterpriseIDBizO.LicenceEnterpriseList should never contain LicenceEnterprise passed to this bizO's constructor", !moveBizO.LicenceEnterpriseList.Contains(LicEntToPassToCtor));

			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();
			org2.CreateAndLoadLicenceForOrg();
			org2.LicenceEnterpriseID = "XXB";
			Factory.Save();
			moveBizO = (MoveLicencesToNewEnterpriseIDBizO)GetNewBusinessObject();
			moveBizO.licEntToCopyFrom.LE_EnterpriseID = "AA3";
			moveBizO.LicenceEnterpriseList.Load();
			AssertEquals("Licence Enterprise List has two extra entries", 2 + licenceEnterprisesAtStartOfList, moveBizO.LicenceEnterpriseList.Count);
			Assert("MoveLicencesToNewEnterpriseIDBizO.LicenceEnterpriseList should never contain LicenceEnterprise passed to this bizO's constructor", !moveBizO.LicenceEnterpriseList.Contains(LicEntToPassToCtor));
		}

		public void TestMoveLicencesToNewEnterpriseID()
		{
			LicenceEnterprise sourceLicEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			sourceLicEnt.LE_EnterpriseID = "SLE";
			LicenceEnterprise targetLicEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			targetLicEnt.LE_EnterpriseID = "TLE";

			LicenceEnterprise tempLicEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			tempLicEnt.LE_EnterpriseID = "TEM";
			EDIOrgHeader tempOrgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			CreateLicenceCompanyForOrg(tempOrgHeader);
			tempOrgHeader.LicCompany.LC_LE = tempLicEnt.PK;
			LicenceCompany tempLicCompany = tempOrgHeader.LicCompany;

			#region case 1: no similar Licence Databases exist under target LicenceEnterprise

			LicenceDatabase sourceLicDb1 = sourceLicEnt.Databases.AddNew();
			FillLicenceDatabaseFieldsWithTestValues(sourceLicDb1, 1);
			sourceLicDb1.LD_ServerCode = "DB1";
			LicenceDatabase sourceLicDb2 = sourceLicEnt.Databases.AddNew();
			FillLicenceDatabaseFieldsWithTestValues(sourceLicDb2, 2);
			sourceLicDb2.LD_ServerCode = "DB2";
			LicenceDatabase sourceLicDbForAnotherCompany = sourceLicEnt.Databases.AddNew();
			FillLicenceDatabaseFieldsWithTestValues(sourceLicDbForAnotherCompany, 0);
			sourceLicDbForAnotherCompany.LD_ServerCode = "DBX";

			EDIOrgHeader sourceOrgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			CreateLicenceCompanyForOrg(sourceOrgHeader);
			sourceOrgHeader.LicCompany.LC_LE = sourceLicEnt.PK;
			sourceOrgHeader.LicCompany.LicDatabases.Add(sourceLicDb1);
			sourceOrgHeader.LicCompany.LicDatabases.Add(sourceLicDb2);
			EDIOrgHeader anotherSourceOrgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			CreateLicenceCompanyForOrg(anotherSourceOrgHeader);
			anotherSourceOrgHeader.LicCompany.LC_LE = sourceLicEnt.PK;
			anotherSourceOrgHeader.LicCompany.LicDatabases.Add(sourceLicDbForAnotherCompany);

			LicenceDatabase licDbForChecking1 = tempLicEnt.Databases.AddNew();
			licDbForChecking1.CopyPersistentValuesFrom(sourceLicDb1);
			licDbForChecking1.LD_LE = tempLicEnt.PK;
			tempLicCompany.LicDatabases.Add(licDbForChecking1);
			FillLicenceHeaderFieldsWithTestValues(sourceOrgHeader.LicCompany.GetHeader(sourceLicDb1), 1);
			CopyLicenceHeaderWithModules(sourceOrgHeader.LicCompany.GetHeader(sourceLicDb1), tempLicCompany.GetHeader(licDbForChecking1));
			LicenceDatabase licDbForChecking2 = tempLicEnt.Databases.AddNew();
			licDbForChecking2.CopyPersistentValuesFrom(sourceLicDb2);
			licDbForChecking2.LD_LE = tempLicEnt.PK;
			tempLicCompany.LicDatabases.Add(licDbForChecking2);
			FillLicenceHeaderFieldsWithTestValues(sourceOrgHeader.LicCompany.GetHeader(sourceLicDb2), 2);
			CopyLicenceHeaderWithModules(sourceOrgHeader.LicCompany.GetHeader(sourceLicDb2), tempLicCompany.GetHeader(licDbForChecking2));

			anotherSourceOrgHeader.LicCompany.LicDatabases.Add(sourceLicDb1);

			Factory.Save();
			AssertEquals("precondition", 0, targetLicEnt.Databases.Count);

			MoveLicencesToNewEnterpriseIDBizOForTest moveBizO = new MoveLicencesToNewEnterpriseIDBizOForTest(sourceLicEnt, sourceOrgHeader);
			moveBizO.LicenceEnterpriseID = targetLicEnt.LE_EnterpriseID;
			moveBizO.MoveLicencesToNewEnterpriseID();
			Factory.Save();

			AssertEquals("if no other companies are installed on LicenceDatabase and no similar database exists under target LicenceEnterprise then should move LicenceDatabase to target LicenceEnterprise", false, sourceLicDb2.IsDeleted);
			AssertEquals("if no other companies are installed on LicenceDatabase and no similar database exists under target LicenceEnterprise then should move LicenceDatabase to target LicenceEnterprise", targetLicEnt.PK, sourceLicDb2.LD_LE);
			AssertEquals("if no other companies are installed on LicenceDatabase and no similar database exists under target LicenceEnterprise then should move LicenceDatabase to target LicenceEnterprise", 2, sourceLicEnt.Databases.Count);
			ZQuery query = new ZQuery(LicenceDatabaseSchema.LD_LE, targetLicEnt.PK);
			query.AddToFilter(JoinCondition.And, LicenceDatabaseSchema.LD_ServerCode, licDbForChecking2.LD_ServerCode);
			AssertEquals("if no other companies are installed on LicenceDatabase and no similar database exists under target LicenceEnterprise then should move LicenceDatabase to target LicenceEnterprise", 1, Factory.Load<LicenceDatabase>(query).Length);
			LicenceDatabase movedDB = Factory.Load<LicenceDatabase>(query)[0];
			var movedLicHeader = sourceOrgHeader.LicCompany.GetHeader(movedDB);
			AssertLicenceDatabaseFieldsAreSame(licDbForChecking2, movedDB);
			AssertLicenceHeaderFieldsAreSame(tempLicCompany.GetHeader(licDbForChecking2), movedLicHeader);

			query = new ZQuery(LicenceDatabaseSchema.LD_LE, targetLicEnt.PK);
			query.AddToFilter(JoinCondition.And, LicenceDatabaseSchema.LD_ServerCode, sourceLicDb1.LD_ServerCode);
			AssertEquals("if other companies are installed on LicenceDatabase then should copy LicenceDatabase to target LicenceEnterprise", 1, Factory.Load<LicenceDatabase>(query).Length);
			LicenceDatabase copiedDB = Factory.Load<LicenceDatabase>(query)[0];
			var copiedLicHeader = sourceOrgHeader.LicCompany.GetHeader(copiedDB);
			AssertEquals("if other companies are installed on LicenceDatabase then should copy LicenceDatabase to target LicenceEnterprise", sourceLicEnt.PK, sourceLicDb1.LD_LE);
			AssertEquals("if other companies are installed on LicenceDatabase then should copy LicenceDatabase to target LicenceEnterprise", 2, targetLicEnt.Databases.Count);
			AssertEquals("if other companies are installed on LicenceDatabase then should copy LicenceDatabase to target LicenceEnterprise", false, sourceLicDb1.IsDeleted);
			AssertLicenceDatabaseFieldsAreSame(sourceLicDb1, copiedDB);
			AssertLicenceHeaderFieldsAreSame(tempLicCompany.GetHeader(licDbForChecking1), copiedLicHeader);

			AssertEquals("only databases related to source OrgHeader should be moved/copied to target LicenceEnterprise", 2, targetLicEnt.Databases.Count);
			for (int i = 0; i < targetLicEnt.Databases.Count; i++)
			{
				AssertNotEquals("only databases related to source OrgHeader should be moved/copied to target LicenceEnterprise", "DBX", targetLicEnt.Databases[i].LD_ServerCode);
			}
			AssertEquals("sourceOrgHeader's LicenceCompany should be moved to target LicenceEnterprise regardless of additional conditions", sourceOrgHeader.LicCompany.LC_LE, targetLicEnt.PK);
			AssertEquals("ServerCode auto-adjustment should occur only if target LicenceEnterprise already contains database with same LD_ServerCode as source database but different LD_PublicEmailAddressForUpdate (which should not be a case here)", false, moveBizO.ServerCodeWasAdjusted);

			Assert("Licence Database installations on other Licence Companies should remain unchanged", anotherSourceOrgHeader.LicCompany.LicDatabases.Contains(sourceLicDbForAnotherCompany));

			AssertEquals("Log about moving all organization's licences should be added", GetLogRecordFor_TestMoveLicencesToNewEnterpriseID(sourceLicEnt, targetLicEnt), sourceOrgHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, GetLogRecordFor_TestMoveLicencesToNewEnterpriseID(sourceLicEnt, targetLicEnt)))[0].SL_Reference);

			#endregion

			#region case 2: Licence Databases with same ServerCode and PublicEmailAddressToUpdate exist under target LicenceEnterprise

			LicenceDatabase sourceLicDb3 = sourceLicEnt.Databases.AddNew();
			FillLicenceDatabaseFieldsWithTestValues(sourceLicDb3, 3);
			sourceLicDb3.LD_ServerCode = "DB3";
			copiedDB.LD_ServerCode = sourceLicDb3.LD_ServerCode;
			sourceLicDb3.LD_PublicEmailAddressForUpdate = "email@address.toupdate";
			copiedDB.LD_PublicEmailAddressForUpdate = sourceLicDb3.LD_PublicEmailAddressForUpdate;

			LicenceDatabase sourceLicDb4 = sourceLicEnt.Databases.AddNew();
			FillLicenceDatabaseFieldsWithTestValues(sourceLicDb4, 4);
			movedDB.LD_ServerCode = "DB4";
			sourceLicDb4.LD_ServerCode = movedDB.LD_ServerCode;
			movedDB.LD_PublicEmailAddressForUpdate = "email@other.address";
			sourceLicDb4.LD_PublicEmailAddressForUpdate = movedDB.LD_PublicEmailAddressForUpdate;

			sourceOrgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			CreateLicenceCompanyForOrg(sourceOrgHeader);
			sourceOrgHeader.LicCompany.LC_LE = sourceLicEnt.PK;
			sourceOrgHeader.LicCompany.LicDatabases.Add(sourceLicDb3);
			anotherSourceOrgHeader.LicCompany.LicDatabases.Add(sourceLicDb3);
			sourceOrgHeader.LicCompany.LicDatabases.Add(sourceLicDb4);

			licDbForChecking1 = tempLicEnt.Databases.AddNew();
			licDbForChecking1.CopyPersistentValuesFrom(copiedDB);
			licDbForChecking1.LD_LE = tempLicEnt.PK;
			tempLicCompany.LicDatabases.Add(licDbForChecking1);
			FillLicenceHeaderFieldsWithTestValues(copiedLicHeader, 3);
			CopyLicenceHeaderWithModules(sourceOrgHeader.LicCompany.GetHeader(sourceLicDb3), tempLicCompany.GetHeader(licDbForChecking1));
			licDbForChecking2 = tempLicEnt.Databases.AddNew();
			licDbForChecking2.CopyPersistentValuesFrom(movedDB);
			licDbForChecking2.LD_LE = tempLicEnt.PK;
			tempLicCompany.LicDatabases.Add(licDbForChecking2);
			FillLicenceHeaderFieldsWithTestValues(movedLicHeader, 4);
			CopyLicenceHeaderWithModules(sourceOrgHeader.LicCompany.GetHeader(sourceLicDb4), tempLicCompany.GetHeader(licDbForChecking2));

			Factory.Save();

			moveBizO = new MoveLicencesToNewEnterpriseIDBizOForTest(sourceLicEnt, sourceOrgHeader);
			moveBizO.LicenceEnterpriseID = targetLicEnt.LE_EnterpriseID;
			moveBizO.SuppressCheck_GetAutoAdjustedLD_ServerCode_WillBeAbleToProcessAllCodes = true;
			moveBizO.MoveLicencesToNewEnterpriseID();
			Factory.Save();
			targetLicEnt.Databases.Load();

			AssertEquals("if target LicenceEnterprise contains similar LicenceDatabase then should copy/move LicenceDatabase's LicenceHeader to this similar LicenceDatabase", sourceLicEnt.PK, sourceLicDb3.LD_LE);
			AssertEquals("if target LicenceEnterprise contains similar LicenceDatabase then should copy/move LicenceDatabase's LicenceHeader to this similar LicenceDatabase", 2, targetLicEnt.Databases.Count);
			AssertEquals("if target LicenceEnterprise contains similar LicenceDatabase then should copy/move LicenceDatabase's LicenceHeader to this similar LicenceDatabase", 2, Factory.Load<LicenceHeader>(new ZQuery(LicenceHeaderSchema.LA_LD, targetLicEnt.Databases[0].PK)).Length);
			AssertEquals("if target LicenceEnterprise contains similar LicenceDatabase then should copy/move LicenceDatabase's LicenceHeader to this similar LicenceDatabase", 2, Factory.Load<LicenceHeader>(new ZQuery(LicenceHeaderSchema.LA_LD, targetLicEnt.Databases[1].PK)).Length);

			AssertEquals("if other companies are installed on LicenceDatabase and similar database does exist under target LicenceEnterprise then should copy LicenceDatabase's Licences (LicenceHeader) under existing target LicenceEnterprise's Licence Database", false, sourceLicDb3.IsDeleted);
			AssertLicenceDatabaseFieldsAreSame(licDbForChecking1, copiedDB);    //should not overwrite persistent values on target database if target database already exists
			AssertLicenceHeaderFieldsAreSame(tempLicCompany.GetHeader(licDbForChecking1), sourceOrgHeader.LicCompany.GetHeader(sourceLicDb3));
			AssertEquals("if no other companies are installed on LicenceDatabase and similar database does exist under target LicenceEnterprise then should move LicenceDatabase's Licences (LicenceHeader) under existing target LicenceEnterprise's Licence Database (and remove source LicenceDatabase)", true, sourceLicDb4.IsDeleted);
			AssertLicenceDatabaseFieldsAreSame(licDbForChecking2, movedDB);     //should not overwrite persistent values on target database if target database already exists
			AssertLicenceHeaderFieldsAreSame(tempLicCompany.GetHeader(licDbForChecking2), sourceOrgHeader.LicCompany.GetHeader(sourceLicDb4));
			for (int i = 0; i < targetLicEnt.Databases.Count; i++)
			{
				AssertNotEquals("only databases related to source OrgHeader should be moved/copied to target LicenceEnterprise", "DBX", targetLicEnt.Databases[i].LD_ServerCode);
			}
			AssertEquals("sourceOrgHeader's LicenceCompany should be moved to target LicenceEnterprise regardless of additional conditions", sourceOrgHeader.LicCompany.LC_LE, targetLicEnt.PK);
			AssertEquals("ServerCode auto-adjustment should occur only if target LicenceEnterprise already contains database with same LD_ServerCode as source database but different LD_PublicEmailAddressForUpdate", false, moveBizO.ServerCodeWasAdjusted);

			AssertEquals("Log about moving all organization's licences should be added", GetLogRecordFor_TestMoveLicencesToNewEnterpriseID(sourceLicEnt, targetLicEnt), sourceOrgHeader.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, GetLogRecordFor_TestMoveLicencesToNewEnterpriseID(sourceLicEnt, targetLicEnt)))[0].SL_Reference);

			#endregion
		}

		public void TestMoveLicencesToNewEnterpriseIDShouldRecordLog()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1", true);
			var sourceLicEnt = lic1.Database.LicEnterprise;
			EDIOrgHeader sourceOrgHeader = lic1.Company.Header;

			LicenceEnterprise targetLicEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			targetLicEnt.LE_EnterpriseID = "TLE";
			Factory.Save();

			var originalEnterpriseCode = sourceLicEnt.LE_EnterpriseCode;

			MoveLicencesToNewEnterpriseIDBizOForTest moveBizO = new MoveLicencesToNewEnterpriseIDBizOForTest(sourceLicEnt, sourceOrgHeader);
			moveBizO.LicenceEnterpriseID = "TLE";
			moveBizO.MoveLicencesToNewEnterpriseID();
			Factory.Save();

			var licenceDataBaseCurrentEnterpriseCode = targetLicEnt.LE_EnterpriseCode;
			AssertNotEquals(originalEnterpriseCode, licenceDataBaseCurrentEnterpriseCode);
			var licenceDataBaseEntIdChangeLog = $"LicenceDataBase enterpriseCode change from '{originalEnterpriseCode}' to '{licenceDataBaseCurrentEnterpriseCode}'";
			var logs = lic1.Database.Logs.Find(x => x.SL_SE_NKEvent == Events.MessageStatusChange.Code && x.SL_Reference == licenceDataBaseEntIdChangeLog).ToArray().Length;
			Assert(logs > 0);
		}

		public void TestMoveLicencesToNewEnterpriseID_UsageData()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1", true);
			var sourceLicEnt = lic1.Database.LicEnterprise;
			var sourceLicDb1 = lic1.Database;
			EDIOrgHeader sourceOrgHeader = lic1.Company.Header;

			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "DB2", true);
			var sourceLicDb2 = lic2.Database;

			var clientCompany1 = lic1.ClientCompany;
			var clientCompany2 = lic2.ClientCompany;

			var staff1 = BillingTestHelper.CreateClientStaff(sourceLicDb1, "", "Staff1");
			var staff2 = BillingTestHelper.CreateClientStaff(sourceLicDb2, "", "Staff2");

			var cfsUsage1 = BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, staff1, LicenceTypes.Codes.ODM, "CFS", new ZDateTime(2018, 6, 1));
			var fwdModuleUsage1 = BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, staff1, LicenceTypes.Codes.ODM, "FOR", new ZDateTime(2018, 6, 1));
			var cptUsage = BillingTestHelper.CreateCptLicenceUsage(clientCompany1, staff1, "ACS", new ZDateTime(2018, 6, 1));

			var coreUsage1 = BillingTestHelper.CreateEdiLicenceUsage(clientCompany2, staff2, LicenceTypes.Codes.ODM, "COR", new ZDateTime(2018, 6, 1));

			LicenceEnterprise targetLicEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			targetLicEnt.LE_EnterpriseID = "TLE";

			MoveLicencesToNewEnterpriseIDBizOForTest moveBizO = new MoveLicencesToNewEnterpriseIDBizOForTest(sourceLicEnt, sourceOrgHeader);
			moveBizO.LicenceEnterpriseID = "TLE";
			moveBizO.MoveLicencesToNewEnterpriseID();
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();

			var usageData = anotherFactory.Load<EdiLicenceUsage>(new ZQuery());
			Assert(usageData.Any(usage => usage.PK == cfsUsage1.PK));
			Assert(usageData.Any(usage => usage.PK == fwdModuleUsage1.PK));
			Assert(usageData.Any(usage => usage.PK == coreUsage1.PK));

			var cptUsageData = anotherFactory.Load<ClientLicenceUsage>(new ZQuery());
			Assert(cptUsageData.Any(usage => usage.PK == cptUsage.PK));
		}

		public void TestMoveLicencesToNewEnterpriseID_UpdateMasterOrgsToNewEnterpriseProxyOrg()
		{
			var sourceLicEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			sourceLicEnt.LE_EnterpriseID = "SLE";
			var targetLicEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			targetLicEnt.LE_EnterpriseID = "TLE";

			var sourceLicDb1 = sourceLicEnt.Databases.AddNew();
			FillLicenceDatabaseFieldsWithTestValues(sourceLicDb1, 1);
			sourceLicDb1.LD_ServerCode = "DB1";
			var sourceLicDb2 = sourceLicEnt.Databases.AddNew();
			FillLicenceDatabaseFieldsWithTestValues(sourceLicDb2, 2);
			sourceLicDb2.LD_ServerCode = "DB2";

			var sourceOrgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			CreateLicenceCompanyForOrg(sourceOrgHeader);
			sourceOrgHeader.LicCompany.LC_LE = sourceLicEnt.PK;
			sourceOrgHeader.LicCompany.LicDatabases.Add(sourceLicDb1);
			sourceOrgHeader.LicCompany.LicDatabases.Add(sourceLicDb2);

			FillLicenceHeaderFieldsWithTestValues(sourceOrgHeader.LicCompany.GetHeader(sourceLicDb1), 1);
			FillLicenceHeaderFieldsWithTestValues(sourceOrgHeader.LicCompany.GetHeader(sourceLicDb2), 2);

			Factory.Save();
			AssertEquals("precondition", 0, targetLicEnt.Databases.Count);

			var moveBizO = new MoveLicencesToNewEnterpriseIDBizOForTest(sourceLicEnt, sourceOrgHeader);
			moveBizO.LicenceEnterpriseID = targetLicEnt.LE_EnterpriseID;
			moveBizO.MoveLicencesToNewEnterpriseID();
			Factory.Save();
			AssertEquals("Moving licence to other enterprise also update the master orgs to new enterprise proxy org", targetLicEnt.LE_OH, sourceLicDb1.LD_OH_WebAccessOrg);
			AssertEquals("Moving licence to other enterprise also update the master orgs to new enterprise proxy org", targetLicEnt.LE_OH, sourceLicDb2.LD_OH_WebAccessOrg);
		}

		#region Implementation

		LicenceEnterprise LicEntToPassToCtor
		{
			get
			{
				if (fLicEntToPassToCtor == null)
				{
					fLicEntToPassToCtor = Factory.NewWithValidTestData<LicenceEnterprise>();
					fLicEntToPassToCtor.LE_EnterpriseID = "YYY";
				}
				return fLicEntToPassToCtor;
			}
		}
		LicenceEnterprise fLicEntToPassToCtor;

		LicenceEnterprise licEntForBizOPropertyTest
		{
			get
			{
				if (fLicEntToPassToCtor == null)
				{
					flicEntForBizOPropertyTest = Factory.NewWithValidTestData<LicenceEnterprise>();
					flicEntForBizOPropertyTest.LE_EnterpriseID = "AAA";
				}
				return flicEntForBizOPropertyTest;
			}
		}
		LicenceEnterprise flicEntForBizOPropertyTest;

		void CreateLicenceCompanyForOrg(EDIOrgHeader orgHeader)
		{
			LicenceCompany licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			using (licenceCompany.SuspendSettingHasChanges())
			{
				licenceCompany.LC_CompanyCode = orgHeader.OH_Code.Right(3);
				licenceCompany.LC_CompanyCountry = orgHeader.UNLOCO != null ? orgHeader.UNLOCO.RL_RN_NKCountryCode : ZString.Empty;
				licenceCompany.LC_RX_NKCurrency = orgHeader.UNLOCO != null ? orgHeader.UNLOCO.Country.RN_RX_NKLocalCurrency : ZString.Empty;
				licenceCompany.LC_OH = orgHeader.PK;
			}
		}

		void FillLicenceDatabaseFieldsWithTestValues(LicenceDatabase licDB, int seed)
		{
			licDB.LD_IsActive = true;
			licDB.LD_ServerCode = "ST" + seed.ToString();
			licDB.LD_LicenceType = "ST" + seed.ToString();
			licDB.LD_LicenceExpiry = new ZDateTime(2000, 1, 1);
			licDB.LD_LastHeartbeat = new ZDateTime(2000, 1, 1);
			licDB.LD_ReleaseRing = "ST" + seed.ToString();
			licDB.LD_AvailableUpgradeMethod = "ST" + seed.ToString();
			licDB.LD_DBServerSecurityMode = "ST" + seed.ToString();
			licDB.LD_PublicEmailAddressForUpdate = "ST1aaa" + seed.ToString();
			licDB.LD_InternalPop3EmailAddress = "ST1@aaa";
			licDB.LD_InternalPop3UserName = "ST1aaa" + seed.ToString();
			licDB.LD_InternalPop3Port = 2;
			licDB.LD_InternalSmtpEmailAddress = "ST1@aaaa";
			licDB.LD_InternalSmtpPort = 3;
			licDB.LD_HostServerSID = ZGuid.NewZGuid();
			licDB.LD_HostServerName = "ST1aa" + seed.ToString();
			licDB.LD_HostDBName = "ST1aa" + seed.ToString();
			licDB.LD_HostDBInstance = "ST1aa" + seed.ToString();
			licDB.LD_RetryTimeoutInMinutes = 1;
			licDB.LD_MaxDataInMegBeforeAck = 5;
			licDB.LD_NoOfActivePrintQueues = 5;
			licDB.LD_LogFileOnDifferentPhysicalVolume = true;
			licDB.LD_DatabaseFilePathDetail = "ST" + seed.ToString();
			licDB.LD_SQLEdition = "ST" + seed.ToString();
			licDB.LD_SQLVersion = "ST" + seed.ToString();
			licDB.LD_SQLVerString = "ST" + seed.ToString();
		}

		void AssertLicenceDatabaseFieldsAreSame(LicenceDatabase sourceLicDB, LicenceDatabase targetLicDB)
		{
			AssertEquals(sourceLicDB.LD_IsActive, targetLicDB.LD_IsActive);
			AssertEquals(sourceLicDB.LD_ServerCode, targetLicDB.LD_ServerCode);
			AssertEquals(sourceLicDB.LD_PublicEmailAddressForUpdate, targetLicDB.LD_PublicEmailAddressForUpdate);
			AssertEquals(sourceLicDB.LD_LicenceType, targetLicDB.LD_LicenceType);
			AssertEquals(sourceLicDB.LD_LicenceExpiry, targetLicDB.LD_LicenceExpiry);
			AssertEquals(sourceLicDB.LD_LastHeartbeat, targetLicDB.LD_LastHeartbeat);
			AssertEquals(sourceLicDB.LD_ReleaseRing, targetLicDB.LD_ReleaseRing);
			AssertEquals(sourceLicDB.LD_AvailableUpgradeMethod, targetLicDB.LD_AvailableUpgradeMethod);
			AssertEquals(sourceLicDB.LD_DBServerSecurityMode, targetLicDB.LD_DBServerSecurityMode);
			AssertEquals(sourceLicDB.LD_InternalPop3EmailAddress, targetLicDB.LD_InternalPop3EmailAddress);
			AssertEquals(sourceLicDB.LD_InternalPop3UserName, targetLicDB.LD_InternalPop3UserName);
			AssertEquals(sourceLicDB.LD_InternalPop3Port, targetLicDB.LD_InternalPop3Port);
			AssertEquals(sourceLicDB.LD_InternalSmtpEmailAddress, targetLicDB.LD_InternalSmtpEmailAddress);
			AssertEquals(sourceLicDB.LD_InternalSmtpPort, targetLicDB.LD_InternalSmtpPort);
			AssertEquals(sourceLicDB.LD_HL_CurrentRunningVersion, targetLicDB.LD_HL_CurrentRunningVersion);
			AssertEquals(sourceLicDB.LD_HL_CurrentSentVersion, targetLicDB.LD_HL_CurrentSentVersion);
			AssertEquals(sourceLicDB.LD_HostServerSID, targetLicDB.LD_HostServerSID);
			AssertEquals(sourceLicDB.LD_HostServerName, targetLicDB.LD_HostServerName);
			AssertEquals(sourceLicDB.LD_HostDBName, targetLicDB.LD_HostDBName);
			AssertEquals(sourceLicDB.LD_HostDBInstance, targetLicDB.LD_HostDBInstance);
			AssertEquals(sourceLicDB.LD_RetryTimeoutInMinutes, targetLicDB.LD_RetryTimeoutInMinutes);
			AssertEquals(sourceLicDB.LD_MaxDataInMegBeforeAck, targetLicDB.LD_MaxDataInMegBeforeAck);
			AssertEquals(sourceLicDB.LD_OC_ContractInstallerOrInternalTechContact, targetLicDB.LD_OC_ContractInstallerOrInternalTechContact);
			AssertEquals(sourceLicDB.LD_OA_SoftwareInstallAddressDetails, targetLicDB.LD_OA_SoftwareInstallAddressDetails);
			AssertEquals(sourceLicDB.LD_OC_LicenseeAdminContact, targetLicDB.LD_OC_LicenseeAdminContact);
			AssertEquals(sourceLicDB.LD_NoOfActivePrintQueues, targetLicDB.LD_NoOfActivePrintQueues);
			AssertEquals(sourceLicDB.LD_LogFileOnDifferentPhysicalVolume, targetLicDB.LD_LogFileOnDifferentPhysicalVolume);
			AssertEquals(sourceLicDB.LD_DatabaseFilePathDetail, targetLicDB.LD_DatabaseFilePathDetail);
			AssertEquals(sourceLicDB.LD_SQLEdition, targetLicDB.LD_SQLEdition);
			AssertEquals(sourceLicDB.LD_SQLVersion, targetLicDB.LD_SQLVersion);
			AssertEquals(sourceLicDB.LD_SQLVerString, targetLicDB.LD_SQLVerString);
		}

		void FillLicenceHeaderFieldsWithTestValues(LicenceHeader licHeader, int seed)
		{
			licHeader.LA_SupportMode = "ii" + seed.ToString();
			licHeader.LA_SpecialSupportConditions = "ii" + seed.ToString();
			licHeader.LA_SiteLiveDate = new ZDateTime(2003, 3, seed);
			licHeader.LA_SupportStartDate = new ZDateTime(2003, 3, seed);
			licHeader.LA_ContractExpiryDate = new ZDateTime(2003, 3, seed);
			licHeader.LA_AMS_USMode = "ii" + seed.ToString();
			licHeader.LA_AgreedLiveDate = new ZDateTime(2003, 3, seed);
			licHeader.LA_InstallationCompleteDate = new ZDateTime(2003, 3, seed);
			licHeader.LA_LastFaxReport = new ZDateTime(2003, 3, seed);
			licHeader.LA_IsActive = false;
			licHeader.LA_LicenceAdvStdOth = "ii" + seed.ToString();
			licHeader.LA_EstimatedLiveDate = new ZDateTime(2003, 3, seed);
			licHeader.LA_LastLicenceSyncCheck = new ZDateTime(2003, 3, seed);
			licHeader.LA_LastLicenceCheckInSync = true;

			for (int i = 0; i < licHeader.Modules.Count; i++)
			{
				licHeader.Modules[i].LM_Checksum = 12340 + seed;
				licHeader.Modules[i].LM_ExpiryDate = new ZDateTime(2008, 1, seed);
				licHeader.Modules[i].LM_LicenceType = "UU" + seed.ToString();
				licHeader.Modules[i].LM_PartPurchasedCount = (ZShort)seed + 1;
				licHeader.Modules[i].LM_UserCount = (ZShort)(seed * 100 + i);
			}
		}

		void AssertLicenceHeaderFieldsAreSame(LicenceHeader sourceLicH, LicenceHeader targetLicH)
		{
			AssertEquals(sourceLicH.LA_SupportMode, targetLicH.LA_SupportMode);
			AssertEquals(sourceLicH.LA_SpecialSupportConditions, targetLicH.LA_SpecialSupportConditions);
			AssertEquals(sourceLicH.LA_SiteLiveDate, targetLicH.LA_SiteLiveDate);
			AssertEquals(sourceLicH.LA_SupportStartDate, targetLicH.LA_SupportStartDate);
			AssertEquals(sourceLicH.LA_ContractExpiryDate, targetLicH.LA_ContractExpiryDate);
			AssertEquals(sourceLicH.LA_AMS_USMode, targetLicH.LA_AMS_USMode);
			AssertEquals(sourceLicH.LA_AgreedLiveDate, targetLicH.LA_AgreedLiveDate);
			AssertEquals(sourceLicH.LA_InstallationCompleteDate, targetLicH.LA_InstallationCompleteDate);
			AssertEquals(sourceLicH.LA_LastFaxReport, targetLicH.LA_LastFaxReport);
			AssertEquals(sourceLicH.LA_IsActive, targetLicH.LA_IsActive);
			AssertEquals(sourceLicH.LA_LicenceAdvStdOth, targetLicH.LA_LicenceAdvStdOth);
			AssertEquals(sourceLicH.LA_EstimatedLiveDate, targetLicH.LA_EstimatedLiveDate);
			AssertEquals(sourceLicH.LA_LastLicenceSyncCheck, targetLicH.LA_LastLicenceSyncCheck);
			AssertEquals(sourceLicH.LA_LastLicenceCheckInSync, targetLicH.LA_LastLicenceCheckInSync);

			sourceLicH.Modules.Sort("LM_GroupModuleCode");
			targetLicH.Modules.Sort("LM_GroupModuleCode");
			for (int i = 0; i < sourceLicH.Modules.Count; i++)
			{
				AssertEquals(sourceLicH.Modules[i].LM_Checksum, targetLicH.Modules[i].LM_Checksum);
				AssertEquals(sourceLicH.Modules[i].LM_ExpiryDate, targetLicH.Modules[i].LM_ExpiryDate);
				AssertEquals(sourceLicH.Modules[i].LM_GroupModuleCode, targetLicH.Modules[i].LM_GroupModuleCode);
				AssertEquals(sourceLicH.Modules[i].LM_LicenceType, targetLicH.Modules[i].LM_LicenceType);
				AssertEquals(sourceLicH.Modules[i].LM_PartPurchasedCount, targetLicH.Modules[i].LM_PartPurchasedCount);
				AssertEquals(sourceLicH.Modules[i].LM_UserCount, targetLicH.Modules[i].LM_UserCount);
			}
		}

		void CopyLicenceHeaderWithModules(LicenceHeader sourceLicHeader, LicenceHeader targetLicHeader)
		{
			targetLicHeader.CopyPersistentValuesFrom(sourceLicHeader, new BusinessObjectCloneArgs(new string[] { "LA_LC", "LA_LD" }));
			targetLicHeader.Modules.RemoveAndDeleteAll();
			for (int i = 0; i < sourceLicHeader.Modules.Count; i++)
			{
				LicenceModules targetLicModule = (LicenceModules)(sourceLicHeader.Modules[i].Clone());
				targetLicHeader.Modules.Add(targetLicModule);
			}
		}

		ZString GetLogRecordFor_TestMoveLicencesToNewEnterpriseID(LicenceEnterprise sourceLicEnt, LicenceEnterprise targetLicEnt)
		{
			return "moved Licences from enterprise ID '" + sourceLicEnt.LE_EnterpriseID + "' to enterprise ID '" + targetLicEnt.LE_EnterpriseID + "'";
		}

		public class MoveLicencesToNewEnterpriseIDBizOForTest : MoveLicencesToNewEnterpriseIDBizO
		{
			public MoveLicencesToNewEnterpriseIDBizOForTest(LicenceEnterprise licEnt, EDIOrgHeader org, IMoveLicencesToNewEnterpriseIDBizOCallbacks moveLicencesToNewEnterpriseIDBizOCallbacks = null)
				: base(licEnt, org, moveLicencesToNewEnterpriseIDBizOCallbacks)
			{
			}

			public LicenceEnterprise fLicEnterpriseAccessor
			{
				get { return this.licEntToCopyTo; }
				set { this.licEntToCopyTo = value; }
			}

			public override void MoveLicencesToNewEnterpriseID()
			{
				MoveLicencesToNewEnterpriseID_WasCalled = true;
				if (MoveLicencesToNewEnterpriseID_CallBase)
				{
					base.MoveLicencesToNewEnterpriseID();
				}
			}
			public bool MoveLicencesToNewEnterpriseID_WasCalled;
			public bool MoveLicencesToNewEnterpriseID_CallBase = true;

			public ZString Call_GetAutoAdjustedLD_ServerCode(LicenceEnterprise targetLicEnt, ZString serverCodeToAdjust)
			{
				licEntToCopyTo = targetLicEnt;
				return this.GetAutoAdjustedLD_ServerCode(serverCodeToAdjust);
			}

			protected override ZString GetAutoAdjustedLD_ServerCode(ZString serverCodeToAdjust)
			{
				ZString result = base.GetAutoAdjustedLD_ServerCode(serverCodeToAdjust);
				if (result != serverCodeToAdjust)
				{
					ServerCodeWasAdjusted = true;
				}
				return result;
			}
			public bool ServerCodeWasAdjusted;

			public bool SuppressCheck_GetAutoAdjustedLD_ServerCode_WillBeAbleToProcessAllCodes;
			protected override bool Check_GetAutoAdjustedLD_ServerCode_WillBeAbleToProcessAllCodes(LicenceDatabase[] sourceLicDbList)
			{
				return SuppressCheck_GetAutoAdjustedLD_ServerCode_WillBeAbleToProcessAllCodes || base.Check_GetAutoAdjustedLD_ServerCode_WillBeAbleToProcessAllCodes(sourceLicDbList);
			}
		}

		#endregion
	}
}
