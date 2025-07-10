using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	public class CusTempStorageJobHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckSJH_OA_Presenter()
		{
			jobHeader.SJH_AppCode = ZString.Empty;
			jobHeader.SJH_OA_Presenter = ZGuid.Empty;
			AssertNoMessageErrorContaining("There should not be any 'Please enter a value' type of error if not IST, even when Presenter is empty.", jobHeader.SJH_OA_PresenterInfo, MandatoryValidation.YouHaveNotEntered);

			jobHeader.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			jobHeader.RunPreSaveValidation();
			AssertHasMessageErrorContaining("There should be an error when IST and Presenter is left empty.", jobHeader.SJH_OA_PresenterInfo, MandatoryValidation.YouHaveNotEntered);

			jobHeader.SJH_OA_Presenter = ZGuid.BrettsGuid;
			AssertNoMessageErrorContaining("There should not be any 'Please enter a value' type of error if IST when Presenter is not empty.", jobHeader.SJH_OA_PresenterInfo, MandatoryValidation.YouHaveNotEntered);

			jobHeader.SJH_OA_Presenter = ZGuid.Empty;
		}

		public void TestCheckSJH_PreviousReferenceType()
		{
			jobHeader.SJH_AppCode = ZString.Empty;
			jobHeader.SJH_PreviousReferenceType = ZString.Empty;
			AssertNoMessageErrorContaining("There should not be any 'Please enter a value' type of error if not IST even when Previous Reference Type is empty.", jobHeader.SJH_PreviousReferenceTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError("If it is empty, there should not be list validation message error.", jobHeader.SJH_PreviousReferenceTypeInfo, ListValidation.InvalidCodeMessageError);

			jobHeader.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			jobHeader.SJH_PreviousReferenceType = "A";
			AssertHasMessageError("There should be list validation message error if the value is not in the list.", jobHeader.SJH_PreviousReferenceTypeInfo, ListValidation.InvalidCodeMessageError);

			jobHeader.SJH_PreviousReferenceType = "235";
			AssertNoMessageError("There should not be list validation message error if the value is in the list.", jobHeader.SJH_PreviousReferenceTypeInfo, ListValidation.InvalidCodeMessageError);

			jobHeader.SJH_PreviousReferenceType = "";
			jobHeader.RunPreSaveValidation();
			AssertNoMessageError("If it is empty, there should not be list validation message error.", jobHeader.SJH_PreviousReferenceTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining("There should be an error when IST and Previous Reference Type is left empty.", jobHeader.SJH_PreviousReferenceTypeInfo, MandatoryValidation.YouHaveNotEntered);

			jobHeader.SJH_PreviousReferenceType = "TYPE1";
			AssertNoMessageErrorContaining("There should not be any 'Please enter a value' type of error if IST when Previous Reference Type is not empty.", jobHeader.SJH_PreviousReferenceTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckSJH_PreviousReferenceNumber()
		{
			jobHeader.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			jobHeader.SJH_PreviousReferenceType = ZString.Empty;
			jobHeader.SJH_PreviousReferenceNumber = ZString.Empty;
			AssertNoMessageErrorContaining("There should not be any 'You have not entered' type of message error against Previous Reference Number when Previous Reference Type is empty.", jobHeader.SJH_PreviousReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			jobHeader.SJH_PreviousReferenceType = "TYPE1";
			AssertHasMessageErrorContaining("There should be an error when Previous Reference Type is not empty but Previous Reference Type is left empty.", jobHeader.SJH_PreviousReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			jobHeader.SJH_PreviousReferenceNumber = "REF1";
			AssertNoMessageErrorContaining("There should not be any 'You have not entered' type of message error against Previous Reference Number when it is not empty.", jobHeader.SJH_PreviousReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			jobHeader.SJH_PreviousReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining("Prerequisite for next assertion.", jobHeader.SJH_PreviousReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			jobHeader.SJH_PreviousReferenceType = ZString.Empty;
			AssertNoMessageErrorContaining("Change of Previous Reference Type should trigger validation on  Previous Reference Number.", jobHeader.SJH_PreviousReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			jobHeader.SJH_AppCode = ZString.Empty;
			jobHeader.SJH_PreviousReferenceType = "TYPE1";
			jobHeader.SJH_PreviousReferenceNumber = ZString.Empty;
			AssertNoMessageErrorContaining("There should not be any 'You have not entered' type of message error on Previous Reference Number if not IST, even when Previous Reference Type is not empty but Previous Reference Number is.", jobHeader.SJH_PreviousReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckSJH_CustomsOffice()
		{
			jobHeader.SJH_AppCode = ZString.Empty;
			jobHeader.SJH_CustomsOffice = ZString.Empty;
			AssertNoMessageErrorContaining("There should not be any 'Please enter a value' type of error if not IST even when Customs Office is empty.", jobHeader.SJH_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			jobHeader.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			jobHeader.RunPreSaveValidation();
			AssertHasMessageErrorContaining("There should be an error when IST and Customs Office is left empty.", jobHeader.SJH_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			jobHeader.SJH_CustomsOffice = "FR00001";
			AssertNoMessageErrorContaining("There should not be any 'Please enter a value' type of error if IST when Customs Office is not empty.", jobHeader.SJH_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
		}

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
			jobHeader.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
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
			oblTran1.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			oblTran1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
			guarantee.CPH_Balance = 55;
			Factory.Save();
			jobHeader.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			jobHeader.CreateRelatedCusTempStorageDec();
			var storageDec = jobHeader.CusTempStorageDec;
			storageDec.STH_OwnerReferenceNumber = "TEST000001";
			AssertSJH_CPH_guarantee(guarantee, storageDec);
		}

		public void TestCheckSJH_CPH_GuaranteeLADT()
		{
			var orgHeader = manageJobHeader();
			jobHeader.SJH_OH_Customer = orgHeader.PK;
			var guarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var oblTran1 = guarantee.CusGuaranteeLineTransactions.AddNew();
			oblTran1.CPL_Reference = "Ref";
			oblTran1.CPL_TranValue = 5;
			oblTran1.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			oblTran1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
			guarantee.CPH_Balance = 55;
			Factory.Save();

			AssertEquals(60m, guarantee.CPH_Calc_TotalBalanceIncludingPending.Amount);
			jobHeader.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeLAD;
			jobHeader.CreateRelatedCusTempStorageDec();
			var storageDec = jobHeader.CusTempStorageDec;
			storageDec.STH_OwnerReferenceNumber = "TEST000001";
			AssertSJH_CPH_guarantee(guarantee, storageDec);
		}

		public void TestCheckSJH_CPH_GuaranteeNorLADTNorIST()
		{
			var orgHeader = manageJobHeader();
			jobHeader.SJH_OH_Customer = orgHeader.PK;
			var guarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var oblTran1 = guarantee.CusGuaranteeLineTransactions.AddNew();
			oblTran1.CPL_Reference = "Ref";
			oblTran1.CPL_TranValue = 5;
			oblTran1.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			oblTran1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
			guarantee.CPH_Balance = 55;
			Factory.Save();
			jobHeader.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeFRC;
			jobHeader.CreateRelatedCusTempStorageDec();
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
			item2.TSI_GuaranteedValue = 10m;
			item3.TSI_GuaranteedValue = 10m;
			Factory.Save();

			item1.TSI_GuaranteedValue = 10m;
			item2.TSI_GuaranteedValue = 20m;
			item3.TSI_GuaranteedValue = 70m;
			AssertNoMessageErrors(jobHeader.SJH_CPH_GuaranteeInfo);

			Factory.Save();

			jobHeader.SJH_CPH_Guarantee = guarantee.PK;
			var errorMessage = guarantee.HumanReadableName + " has only " + guarantee.CPH_Calc_TotalBalanceIncludingPending.Amount + " remaining but this job requires 100.";
			if (jobHeader.IsIST || jobHeader.IsLADT)
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
