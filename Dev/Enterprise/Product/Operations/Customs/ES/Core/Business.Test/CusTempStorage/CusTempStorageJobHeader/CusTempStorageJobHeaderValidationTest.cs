using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	public class CusTempStorageJobHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckSJH_CustomsProfile()
		{
			var messageErrorInvalidCode = ListValidation.InvalidCodeMessageError;
			var info = jobHeader.SJH_CustomsProfileInfo;
			jobHeader.SJH_CustomsProfile = "XXXXX";
			AssertNoMessageErrors(info);

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "TESTTST2";
			jobHeader.SJH_OH_Customer = orgHeader2.PK;
			jobHeader.SJH_CustomsProfile = "XXXXX";
			AssertNoMessageErrors(info);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTTST";
			jobHeader.SJH_AppCode = "IST";
			var header = Factory.New<CusAuthorisationHeader>();
			header.CPH_OH_PermitHolder = orgHeader.PK;
			header.CPH_Number = "110001";
			header.CPH_Type = "TST";
			var rule = header.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = "USE";
			rule.CPR_ValueFrom = "IST";
			Factory.Save();
			jobHeader.SJH_OH_Customer = orgHeader.PK;
			jobHeader.SJH_CustomsProfile = "110001";
			AssertNoMessageErrors(info);
			jobHeader.SJH_CustomsProfile = "XXXXX";
			AssertHasMessageError(info, messageErrorInvalidCode);
			jobHeader.SJH_OH_Customer = ZGuid.Empty;
			AssertNoMessageErrors(info);
		}

		public void TestCheckSJH_CPH_GuaranteeIST()
		{
			var orgHeader = manageJobHeader();
			jobHeader.SJH_OH_Customer = orgHeader.PK;
			var guarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var oblTran1 = guarantee.CusGuaranteeLineTransactions.AddNew();
			oblTran1.CPL_Reference = "Ref";
			oblTran1.CPL_TranValue = 5;
			oblTran1.CPL_TransactionType = "OBL";
			oblTran1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
			guarantee.CPH_Balance = 55;
			Factory.Save();
			jobHeader.SJH_AppCode = "IST";
			var newStorageDec = CusTempStorageDec.New(jobHeader);
			newStorageDec.CusTempStorageLines.AddNew();

			var storageDec = jobHeader.CusTempStorageDec;
			storageDec.STH_OwnerReferenceNumber = "TEST000001";
			AssertSJH_CPH_guarantee(guarantee, storageDec);
		}

		void AssertSJH_CPH_guarantee(CusGuaranteeHeader guarantee, EU.Business.CusTempStorage.CusTempStorageDec storageDec)
		{
			storageDec.FillWithValidTestData();
			var line1 = storageDec.CusTempStorageLines.AddNew();
			line1.TSL_PackageQty = 2;
			var item1 = line1.CusTempStorageLineItems.AddNew();
			var line2 = storageDec.CusTempStorageLines.AddNew();
			line2.TSL_PackageQty = 8;
			var item2 = line2.CusTempStorageLineItems.AddNew();
			var item3 = line2.CusTempStorageLineItems.AddNew();

			jobHeader.SJH_CPH_Guarantee = guarantee.PK;
			item1.TSI_GuaranteedValue = 10m;
			item1.TSI_RX_NKCurrency = Core.Constants.CurrencyCodes.Spain;
			item2.TSI_GuaranteedValue = 10m;
			item2.TSI_RX_NKCurrency = Core.Constants.CurrencyCodes.Spain;
			item3.TSI_GuaranteedValue = 10m;
			item3.TSI_RX_NKCurrency = Core.Constants.CurrencyCodes.Spain;
			Factory.Save();

			item1.TSI_GuaranteedValue = 10m;
			item2.TSI_GuaranteedValue = 20m;
			item3.TSI_GuaranteedValue = 70m;
			AssertNoMessageErrors(jobHeader.SJH_CPH_GuaranteeInfo);

			Factory.Save();

			jobHeader.SJH_CPH_Guarantee = guarantee.PK;
			var errorMessage = guarantee.HumanReadableName + " has only " + guarantee.CPH_Calc_TotalBalanceIncludingPending.Amount + " remaining but this job requires 100.";
			if (jobHeader.IsIST)
			{
				AssertHasMessageError(jobHeader.SJH_CPH_GuaranteeInfo, errorMessage);
			}
			else
			{
				AssertNoMessageErrors(jobHeader.SJH_CPH_GuaranteeInfo);
			}
		}

		OrgHeader manageJobHeader()
		{
			var messageErrorInvalidCode = ListValidation.InvalidCodeMessageError;
			var info = jobHeader.SJH_CustomsProfileInfo;
			jobHeader.SJH_CustomsProfile = "XXXXX";
			AssertNoMessageErrors(info);

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "TESTTST2";
			jobHeader.SJH_OH_Customer = orgHeader2.PK;
			jobHeader.SJH_CustomsProfile = "XXXXX";
			AssertNoMessageErrors(info);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTTST";
			var header = Factory.New<CusAuthorisationHeader>();
			header.CPH_OH_PermitHolder = orgHeader.PK;
			header.CPH_Number = "110001";
			header.CPH_Type = "TST";
			var rule = header.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = "USE";
			rule.CPR_ValueFrom = "IST";
			Factory.Save();
			return orgHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobHeader = CusTempStorageJobHeader.New(Factory);
		}

		CusTempStorageJobHeader jobHeader;
	}
}
