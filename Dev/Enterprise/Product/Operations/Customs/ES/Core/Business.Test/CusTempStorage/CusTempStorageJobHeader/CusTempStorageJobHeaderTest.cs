using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageJobHeader))]
	public class CusTempStorageJobHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CusTempStorageJobHeader.New(Factory);

		[UseSnapshotProtection]
		public void TestSavingSetsJobReference_NoDuplicateReferenceException()
		{
			this.header.Delete();
			var header = Factory.New<CusTempStorageJobHeader>();

			var dbConnection = ((CargoWise.Data.IDbConnected)Factory).Connection;
			header.PopulateJobReferenceIfNeeded();
			var jobReference = header.SJH_JobReference;
			dbConnection.RollbackTransaction();

			var newFactory = new BusinessObjectFactory();
			var orgHeader2 = newFactory.New<OrgHeader>();
			orgHeader2.OH_Code = "TEST2";
			var header2 = newFactory.New<CusTempStorageJobHeader>();
			header2.SJH_OH_Customer = orgHeader2.PK;

			newFactory.Save();
			AssertEquals(jobReference, header2.SJH_JobReference);

			dbConnection.BeginTransaction();
			AssertEquals(jobReference, header.SJH_JobReference);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TEST1";
			header.SJH_OH_Customer = orgHeader.PK;
			Factory.Save();
			AssertNotEquals(jobReference, header.SJH_JobReference);
		}

		[UseSnapshotProtection]
		public void TestPopulateJobReferenceIfNeeded()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			ZString jobReference = Env.NumberFountains.FRTempStorageJobReference.PeekPreliminaryFormatted(Factory);
			AssertEquals(ZString.Empty, header.SJH_JobReference);
			header.SJH_JobReference = "BSZSSD23423";
			header.PopulateJobReferenceIfNeeded();
			AssertEquals("BSZSSD23423", header.SJH_JobReference);
			header.SJH_JobReference = ZString.Empty;
			header.PopulateJobReferenceIfNeeded();
			AssertEquals(jobReference, header.SJH_JobReference);
		}

		public void TestCusTempStorageDec()
		{
			var cusTempStorageJobHeader = Factory.New<CusTempStorageJobHeader>();
			cusTempStorageJobHeader.FillWithValidTestData();
			cusTempStorageJobHeader.SJH_AppCode = "IST";

			var cusTempStorageDec = Factory.New<CusTempStorageDec>();
			cusTempStorageDec.FillWithValidTestData();
			cusTempStorageDec.STH_DeclarationType = "IST";
			cusTempStorageDec.STH_SJH = cusTempStorageJobHeader.PK;

			AssertType<CusTempStorageDec>(cusTempStorageJobHeader.CusTempStorageDec);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("SJH_AppCode should be 'IST'", "IST", header.SJH_AppCode);
		}

		public void TestLookups()
		{
			AssertType<CusTempStorageJobHeaderLookups>(header.Lookups);
		}

		public void TestValidation()
		{
			AssertType<CusTempStorageJobHeaderValidation>(header.Validation);
		}

		public void TestApplicationCode()
		{
			Assert(header.IsIST);
		}

		public void TestDocumentSupporter()
		{
			AssertType<CusTempStorageJobHeaderDocumentSupporter>(header.DocumentSupporter);
		}

		public void TestSJH_CPH_Guarantee()
		{
			var typeToCheck = header.GetType();
			AssertHasCustomAttribute<ListAttribute>(typeToCheck, CusTempStorageJobHeader.Schema.SJH_CPH_Guarantee, false, attrib => attrib.ListDataSourceMember == "Lookups.GuaranteeList");
		}

		#region SJH_TempStorageEndDateUtc

		void CreateAuthorisation(ZGuid orgHeaderPK, ZString number, ZString daysForSTO)
		{
			var header = Factory.New<CusAuthorisationHeader>();
			header.CPH_OH_PermitHolder = orgHeaderPK;
			header.CPH_Number = number;
			header.CPH_Type = "TST";
			var rule = header.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = "USE";
			rule.CPR_ValueFrom = "LAD";
			rule = header.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = "USE";
			rule.CPR_ValueFrom = "IST";
			if (!daysForSTO.IsEmpty)
			{
				rule = header.CusAuthorisationRules.AddNew();
				rule.CPR_RuleCode = "STO";
				rule.CPR_ValueFrom = daysForSTO;
			}
		}

		public void TestTempStorageEndDateUtcReadOnly()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TestEND";
			header.SJH_OH_Customer = orgHeader.PK;
			header.SJH_TempStorageEndDateUtc = ZDateTime.Empty;
			var info = header.SJH_TempStorageEndDateUtcInfo;
			AssertEquals(ZBool.False, info.ReadOnly);
			Factory.Save();
			AssertEquals(ZBool.False, info.ReadOnly);

			header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_OH_Customer = orgHeader.PK;
			info = header.SJH_TempStorageEndDateUtcInfo;
			header.SJH_TempStorageEndDateUtc = ZDateTime.Today;
			AssertEquals(ZBool.False, info.ReadOnly);
			Factory.Save();
			AssertEquals(ZBool.True, info.ReadOnly);
		}

		public void TestShowPreSaveDialogIfTempStorageEndDateUtcIsNotEmpty()
		{
			header.SJH_TempStorageEndDateUtc = ZDateTime.Empty;
			AssertEquals(ZBool.False, header.ShowPreSaveDialogIfTempStorageEndDateUtcIsNotEmpty);
			header.SJH_TempStorageEndDateUtc = ZDateTime.Now;
			AssertEquals(ZBool.True, header.ShowPreSaveDialogIfTempStorageEndDateUtcIsNotEmpty);
		}
		#endregion

		#region SJH_CustomsProfile
		public void TestSJH_CustomsProfile()
		{
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "TESTLAD2";
			header.SJH_OH_Customer = orgHeader2.PK;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTLAD";
			CreateAuthorisation(orgHeader.PK, "110001", "");
			Factory.Save();
			AssertCustomsProfileDefaultValue(orgHeader.PK, "110001");

			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTLAD3";
			CreateAuthorisation(orgHeader.PK, "220001", "");
			CreateAuthorisation(orgHeader.PK, "220002", "");
			Factory.Save();
			AssertCustomsProfileDefaultValue(orgHeader.PK, ZString.Empty);
			AssertCustomsProfileDefaultValue(orgHeader.PK, ZString.Empty);
		}

		void AssertCustomsProfileDefaultValue(ZGuid orgHeaderPk, ZString expect)
		{
			header = CusTempStorageJobHeader.New(Factory);
			header.SJH_OH_Customer = orgHeaderPk;
			AssertEquals(expect, header.SJH_CustomsProfile);
		}
		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			header = CusTempStorageJobHeader.New(Factory);
		}
		CusTempStorageJobHeader header;

		#endregion
	}
}
