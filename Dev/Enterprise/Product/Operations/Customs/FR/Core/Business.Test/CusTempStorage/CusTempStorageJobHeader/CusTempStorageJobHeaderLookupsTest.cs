using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	class CusTempStorageJobHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		#region GuaranteeList
		[TestDate(2019, 9, 5)]
		public void TestGuaranteeList()
		{
			var list = jobHeader.Lookups.GuaranteeList;
			AssertEquals(0, list.Count);
			AddGuarantees();
			AssertEquals(2, list.Count);
		}

		void AddGuarantees()
		{
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_Code = "Org1";
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			organization2.OH_Code = "Org2";

			var permitHeader3 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			permitHeader3.CPH_Number = "PERMIT3";
			permitHeader3.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			permitHeader3.CPH_OH_PermitHolder = organization1.PK;
			AddPermitLine(permitHeader3, "ARS", new ZDate(2019, 6, 4));

			var permitHeader4 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			permitHeader4.CPH_Number = "PERMIT4";
			permitHeader4.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			permitHeader3.CPH_OH_PermitHolder = organization2.PK;
			AddPermitLine(permitHeader4, "PAA", new ZDate(2019, 6, 4));
			AddPermitLine(permitHeader4, "PEE", new ZDate(2019, 6, 5));
			AddPermitLine(permitHeader4, "POO", new ZDate(2019, 9, 5));
			AddPermitLine(permitHeader4, "PZZ", new ZDate(2019, 9, 6));
		}

		void AddPermitLine(CusGuaranteeHeader header, ZString reference, ZDate transactionDate)
		{
			var line = header.CusGuaranteeLineTransactions.AddNew();
			line.FillWithValidTestData();
			line.CPL_Reference = reference;
			line.CPL_TransactionDate = transactionDate;
		}
		#endregion

		#region CustomsProfileList
		public void TestCustomsProfileList()
		{
			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Code = "ORG003";
			jobHeader.SJH_OH_Customer = orgHeader3.PK;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG001";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "ORG002";
			CreateAuthorisation(orgHeader.PK, "000001", "IST", "LAD", "FRC");
			CreateAuthorisation(orgHeader.PK, "000002", "IST");
			CreateAuthorisation(orgHeader.PK, "000003");
			CreateAuthorisation(orgHeader2.PK, "000004", "IST", "FRC");
			Factory.Save();

			AssertCustomsProfileList(orgHeader.PK, FRConstants.TemporaryStorage.AppCodeIST, 2);
			AssertCustomsProfileList(orgHeader.PK, FRConstants.TemporaryStorage.AppCodeFRC, 0);
			AssertCustomsProfileList(orgHeader.PK, FRConstants.TemporaryStorage.AppCodeLAD, 1);
			AssertCustomsProfileList(orgHeader2.PK, FRConstants.TemporaryStorage.AppCodeLAD, 0);
			AssertCustomsProfileList(ZGuid.Empty, FRConstants.TemporaryStorage.AppCodeLAD, 0);
		}

		void AssertCustomsProfileList(ZGuid orgHeaderPK, ZString appCode, ZInt expect)
		{
			var jobHeader = Factory.New<CusTempStorageJobHeader>();
			jobHeader.SJH_AppCode = appCode;
			jobHeader.SJH_OH_Customer = orgHeaderPK;
			AssertEquals(expect, jobHeader.Lookups.CustomsProfileList.Count);
		}

		void CreateAuthorisation(ZGuid orgHeaderPK, ZString permitNumber, params ZString[] ruleValues)
		{
			var header = Factory.New<CusAuthorisationHeader>();
			header.CPH_OH_PermitHolder = orgHeaderPK;
			header.CPH_Number = permitNumber;
			header.CPH_Type = "TST";
			foreach (var ruleValue in ruleValues)
			{
				var rule = header.CusAuthorisationRules.AddNew();
				rule.CPR_RuleCode = "USE";
				rule.CPR_ValueFrom = ruleValue;
			}
		}
		#endregion

		#region PreviousReferenceTypeList
		public void TestPreviousReferenceTypeList()
		{
			AssertType<PreviousDocumentCodeList>(jobHeader.Lookups.PreviousReferenceTypeList);
			AssertEquals("235, 270, 271, 325, 337, 355, 380, 703, 704, 705, 720, 722, 730, 740, 741, 750, 760, 785, 787, 820, 821, 822, 823, 952, 955, CLE, CO, EU, EX, IF3, IF8, IM, IST, MDT, MNS, T2F, T2M, TCS, ZZZ", jobHeader.Lookups.PreviousReferenceTypeList.CodesAsString);
		}
		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			jobHeader = Factory.New<CusTempStorageJobHeader>();
		}

		CusTempStorageJobHeader jobHeader;
	}
}
