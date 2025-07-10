using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CusTempStorageRegHeader = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

class IntoTemporaryStorageHelperTest : TestCaseWithFactory
{
	public void TestGoodsLocationExistsInPremises_NotLAM()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		var premises1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises1.SRP_Code = "COD";
		premises1.SRP_Description = "Desc";
		premises1.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		premises1.SRP_OA_PremisesAddress = orgAddress.PK;

		var premises2 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises2.SRP_Code = "CO2";
		premises2.SRP_Description = "Desc2";
		premises2.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;
		premises2.SRP_OA_PremisesAddress = orgAddress.PK;

		const string expectedMessage = "Temporary Storage Location (ES009999000002) does not exist in Maintain/Customs/Customs Files/Temporary Storage Premises module.";
		const string expectedMessage2 = "The associated Premises to the Temporary Storage Location (ES009999000002) has no active numbering configuration or it has no available numbers.";

		CombineAssertions(() => 
		{
			AssertEquals("Goods Location doesn't exist in premises", false, IntoTemporaryStorageHelper.GoodsLocationExistsInPremises(Factory, "ES009999000002", false));
			AssertEquals("Goods Location doesn't exist in premises and show Message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			premises2.SRP_CustomsLocation = "ES009999000002";
			Factory.Save();
			AssertEquals("Goods Location doesn't exist in premises because the premises with the correct code doesn't have the correct type", false, IntoTemporaryStorageHelper.GoodsLocationExistsInPremises(Factory, "ES009999000002", false));
			AssertEquals("Goods Location doesn't exist in premises because the premises with the correct code doesn't have the correct type and show Message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			premises1.SRP_CustomsLocation = "ES009999000002";
			Factory.Save();
			AssertEquals("Goods Location exists in premises but not active wrapper", false, IntoTemporaryStorageHelper.GoodsLocationExistsInPremises(Factory, "ES009999000002", false));
			AssertEquals("Goods Location exists in premises not show Message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			AssertEquals(message: "Premises NumberProvider has not active wrapper",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertNull("Last Message was cleared after error for Premises", UnitTestUserNotification.Instance.LastMessage.Text);

			var provider = premises1.NumberProvider;
			_ = provider.CustomsNumbers.AddNew();
			var wrapper1 = provider.CustomsNumberWrappers[0];
			wrapper1.IsActive = true;
			wrapper1.SN_MinimumValue = 1;
			wrapper1.SN_Count = 1;
			Factory.Save();
			_ = wrapper1.StmNums.GenerateNextCustomsNumber(Factory);

			AssertEquals("Goods Location exists in premises but no available numbers", false, IntoTemporaryStorageHelper.GoodsLocationExistsInPremises(Factory, "ES009999000002", false));
			AssertEquals(message: "Premises NumberProvider has active wrapper without available numbers",
						 expected: true,
						 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertNull("Last Message was cleared after error for Premises", UnitTestUserNotification.Instance.LastMessage.Text);

			var otherFactory = new BusinessObjectFactory();
			var reloadedPremises = otherFactory.Load<EU.TemporaryStorage.Business.CusTempStorageRegPremises>(premises1.PK);
			reloadedPremises.NumberProvider.CustomsNumberWrappers[0].IsActive = false;

			_ = provider.CustomsNumbers.AddNew();
			var wrapper2 = provider.CustomsNumberWrappers[1];
			wrapper2.IsActive = true;
			wrapper2.SN_MinimumValue = 1;
			wrapper2.SN_Count = 1;
			Factory.Save();
			otherFactory.Save();

			AssertEquals("Goods Location exists in premises with active wrapper and available numbers", true, IntoTemporaryStorageHelper.GoodsLocationExistsInPremises(Factory, "ES009999000002", false));
			AssertEquals(message: "Premises NumberProvider has active wrapper with available numbers",
						 expected: false,
						 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));
		});
	}

	public void TestGoodsLocationExistsInPremises_LAM()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		var premises1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises1.SRP_Code = "COD";
		premises1.SRP_Description = "Desc";
		premises1.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		premises1.SRP_OA_PremisesAddress = orgAddress.PK;

		var premises2 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises2.SRP_Code = "CO2";
		premises2.SRP_Description = "Desc2";
		premises2.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;
		premises2.SRP_OA_PremisesAddress = orgAddress.PK;

		const string expectedMessage = "Temporary Storage Location (ES009999000002) does not exist in Maintain/Customs/Customs Files/Temporary Storage Premises module.";
		const string expectedMessage2 = "The associated Premises to the Temporary Storage Location (ES009999000002) has no active numbering configuration or it has no available numbers.";

		CombineAssertions(() =>
		{
			AssertEquals("Goods Location doesn't exist in premises", false, IntoTemporaryStorageHelper.GoodsLocationExistsInPremises(Factory, "ES009999000002", true));
			AssertEquals("Goods Location doesn't exist in premises and show Message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			premises1.SRP_CustomsLocation = "ES009999000002";
			Factory.Save();
			AssertEquals("Goods Location doesn't exist in premises because the premises with the correct code doesn't have the correct type", false, IntoTemporaryStorageHelper.GoodsLocationExistsInPremises(Factory, "ES009999000002", true));
			AssertEquals("Goods Location doesn't exist in premises because the premises with the correct code doesn't have the correct type and show Message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			premises2.SRP_CustomsLocation = "ES009999000002";
			Factory.Save();
			AssertEquals("Goods Location exists in premises but not active wrapper", false, IntoTemporaryStorageHelper.GoodsLocationExistsInPremises(Factory, "ES009999000002", false));
			AssertEquals("Goods Location exists in premises not show Message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			AssertEquals(message: "Premises NumberProvider has not active wrapper",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertNull("Last Message was cleared after error for Premises", UnitTestUserNotification.Instance.LastMessage.Text);

			var provider = premises1.NumberProvider;
			_ = provider.CustomsNumbers.AddNew();
			var wrapper1 = provider.CustomsNumberWrappers[0];
			wrapper1.IsActive = true;
			wrapper1.SN_MinimumValue = 1;
			wrapper1.SN_Count = 1;
			Factory.Save();
			_ = wrapper1.StmNums.GenerateNextCustomsNumber(Factory);

			AssertEquals("Goods Location exists in premises but no available numbers", false, IntoTemporaryStorageHelper.GoodsLocationExistsInPremises(Factory, "ES009999000002", false));
			AssertEquals(message: "Premises NumberProvider has active wrapper without available numbers",
						 expected: true,
						 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertNull("Last Message was cleared after error for Premises", UnitTestUserNotification.Instance.LastMessage.Text);

			var otherFactory = new BusinessObjectFactory();
			var reloadedPremises = otherFactory.Load<EU.TemporaryStorage.Business.CusTempStorageRegPremises>(premises1.PK);
			reloadedPremises.NumberProvider.CustomsNumberWrappers[0].IsActive = false;

			_ = provider.CustomsNumbers.AddNew();
			var wrapper2 = provider.CustomsNumberWrappers[1];
			wrapper2.IsActive = true;
			wrapper2.SN_MinimumValue = 1;
			wrapper2.SN_Count = 1;
			Factory.Save();
			otherFactory.Save();

			AssertEquals("Goods Location exists in premises with active wrapper and available numbers", true, IntoTemporaryStorageHelper.GoodsLocationExistsInPremises(Factory, "ES009999000002", false));
			AssertEquals(message: "Premises NumberProvider has active wrapper with available numbers",
						 expected: false,
						 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));
		});
	}

	public void TestGetMutexLockText()
	{
		var expectedResult = "APC is already in the process of creating a Temporary Storage Register Header.\r\nYou should be able to access this option when the person has saved the record. Please try later.";
		AssertEquals("String returned is correct for Mutex", expectedResult, IntoTemporaryStorageHelper.GetMutexLockText("APC"));
	}

	public void TestGuaranteeAndLiabilityAmountAreDeclared()
	{
		var guarantee = Factory.New<CommonGuarantee>();
		guarantee.PW_BondNumber = "Test";

		const string expectedMessage = "To enter goods into the Temporary Storage, a liability amount for a related Guarantee must be supplied.";

		CombineAssertions(() =>
		{
			AssertEquals("Reference declared in the guarantee and BondAmount is empty", true, IntoTemporaryStorageHelper.GuaranteeAndLiabilityAmountAreDeclared(guarantee));
			AssertEquals("Reference declared in the guarantee and BondAmount is empty and show message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			guarantee.PW_BondAmount = 1m;
			AssertEquals("Reference declared in the guarantee and BondAmount is not empty", true, IntoTemporaryStorageHelper.GuaranteeAndLiabilityAmountAreDeclared(guarantee));
			AssertEquals("Reference declared in the guarantee and BondAmount is not empty not show message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			guarantee.PW_BondNumber = ZString.Empty;
			AssertEquals("No reference declared in the guarantee and BondAmount is not empty", false, IntoTemporaryStorageHelper.GuaranteeAndLiabilityAmountAreDeclared(guarantee));
			AssertEquals("No reference declared in the guarantee and BondAmount is not empty and show message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
		});
	}

	public void TestShowWarningIfGuaranteeLiabilityAmountIsZero()
	{
		var guarantee = Factory.New<CommonGuarantee>();

		const string expectedMessage = @"If liability amount is 0, no guarantee transaction will be created for the goods that enter the Temporary Storage.
Would you like to cancel this action to check if the data needed to calculate the liability amount has been entered?
(if data is filled and result is 0, please ignore this warning message and press No)";

		CombineAssertions(() =>
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals("BondAmount is empty and answer is Yes", false, IntoTemporaryStorageHelper.ShowWarningIfGuaranteeLiabilityAmountIsZero(guarantee));
			AssertEquals("BondAmount is empty and show message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals("BondAmount is empty and answer is No", true, IntoTemporaryStorageHelper.ShowWarningIfGuaranteeLiabilityAmountIsZero(guarantee));
			AssertEquals("BondAmount is empty and show message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			guarantee.PW_BondAmount = 1m;
			AssertEquals("BondAmount is not empty", true, IntoTemporaryStorageHelper.ShowWarningIfGuaranteeLiabilityAmountIsZero(guarantee));
			AssertEquals("BondAmount is not empty not show message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
		});
	}

	public void TestGuaranteNumberExistsAndIsValid()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "ES009999000002";

		var guarantee = Factory.New<CommonGuarantee>();
		guarantee.Parent = tempStorageHeader;
		guarantee.PW_BondNumber = ZString.Empty;
		Factory.Save();

		const string expectedMessageWhenBondNumberEmpty = "Guarantee Nº () does not exist in Maintain/Customs/Customs Files/Customs Guarantees module or is not valid.";
		const string expectedMessage = "Guarantee Nº (GUARANTEEREF) does not exist in Maintain/Customs/Customs Files/Customs Guarantees module or is not valid.";

		CombineAssertions(() =>
		{
			AssertEquals("Guarantee BondNumber is empty", false, IntoTemporaryStorageHelper.GuaranteNumberExistsAndIsValid(guarantee, guarantee.CusGuarantee));
			AssertEquals("Guarantee BondNumber is empty show message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageWhenBondNumberEmpty));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			guarantee.PW_BondNumber = "GUARANTEEREF";
			AssertEquals("Guarantee CusPermitHeader does not exists", false, IntoTemporaryStorageHelper.GuaranteNumberExistsAndIsValid(guarantee, guarantee.CusGuarantee));
			AssertEquals("Guarantee CusPermitHeader does not exists show message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
			cusPermitHeader.CPH_Number = "GUARANTEEREF";
			cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
			cusPermitHeader.CPH_Type = "TST";
			cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(1);
			guarantee.PW_CPH_Guarantee = cusPermitHeader.PK;
			cusPermitHeader.Factory.Save();
			AssertEquals("Guarantee start date is in the future", false, IntoTemporaryStorageHelper.GuaranteNumberExistsAndIsValid(guarantee, guarantee.CusGuarantee));
			AssertEquals("Guarantee start date is in the future show message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(-1);
			cusPermitHeader.Factory.Save();
			AssertEquals("Guarantee end date is in the past", false, IntoTemporaryStorageHelper.GuaranteNumberExistsAndIsValid(guarantee, guarantee.CusGuarantee));
			AssertEquals("Guarantee end date is in the past show message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			cusPermitHeader.Factory.Save();
			AssertEquals("Guarantee has no opening balance transaction", false, IntoTemporaryStorageHelper.GuaranteNumberExistsAndIsValid(guarantee, guarantee.CusGuarantee));
			AssertEquals("Guarantee has no opening balance transaction show message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
			transaction.CPL_TransactionType = "OBL";
			transaction.CPL_Reference = "REF";
			transaction.Factory.Save();
			AssertEquals("Guarantee exists and is valid", true, IntoTemporaryStorageHelper.GuaranteNumberExistsAndIsValid(guarantee, guarantee.CusGuarantee));
			AssertEquals("Guarantee exists and is valid show message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
		});
	}

	public void TestGoodsItemsAreNotInTemporaryStorage_NotLAM()
	{
		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "ES009999000002";
		Factory.Save();

		const string expectedMessage = "Goods Items from Summary Declaration ES009999000002 are already in the Temporary Storage.";
		CombineAssertions(() =>
		{
			AssertEquals("Goods Item are into TmpStorage", false, IntoTemporaryStorageHelper.GoodsItemsAreNotInTemporaryStorage(Factory, "ES009999000002", false));
			AssertEquals("Goods Item are into TmpStorage show message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("Goods Item are not into TmpStorage", true, IntoTemporaryStorageHelper.GoodsItemsAreNotInTemporaryStorage(Factory, "ES006543", false));
			AssertEquals("Goods Item are not into TmpStorage show message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
		});
	}

	public void TestGoodsItemsAreNotInTemporaryStorage_LAM()
	{
		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "ES009999000002";
		Factory.Save();

		const string expectedMessage = "Goods Items for LAME Reception Certificate ES009999000002 are already in the Temporary Storage.";
		CombineAssertions(() =>
		{
			AssertEquals("Goods Item are into TmpStorage", false, IntoTemporaryStorageHelper.GoodsItemsAreNotInTemporaryStorage(Factory, "ES009999000002", true));
			AssertEquals("Goods Item are into TmpStorage show message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("Goods Item are not into TmpStorage", true, IntoTemporaryStorageHelper.GoodsItemsAreNotInTemporaryStorage(Factory, "ES006543", true));
			AssertEquals("Goods Item are not into TmpStorage show message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
		});
	}

	public void TestAtLeastOneLineNotMissing()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		var bill1 = tempStorage.Bills.AddNew();
		var bill2 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = true;
		var item2 = bill2.PackedItems.AddNew();
		item2.IsMissing = true;
		var item3 = bill2.PackedItems.AddNew();
		item3.IsMissing = true;
		item3.API_GrossWeight = 1;
		item3.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		item3.PackagesPivot.AddPivotFor(pack1);

		const string expectedMessage = "There are no good items available to enter the Temporary Storage.";
		CombineAssertions(() =>
		{
			AssertEquals("There are no not missing Goods Items", false, IntoTemporaryStorageHelper.AtLeastOneLineNotMissing(tempStorage));
			AssertEquals("There are no not missing Goods Items show message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			item2.IsMissing = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("There is at least one not missing Goods Item but it has no packages associated", false, IntoTemporaryStorageHelper.AtLeastOneLineNotMissing(tempStorage));
			AssertEquals("There is at least one not missing Goods Item but it has no packages associated show message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			item3.IsMissing = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("There is at least one not missing Goods Item and it has packages associated", true, IntoTemporaryStorageHelper.AtLeastOneLineNotMissing(tempStorage));
			AssertEquals("There is at least one not missing Goods Item and it has packages associated show message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
		});
	}

	[RequiresSTA]
	public void TestShowTemporaryStorageRegisterForm_EntryFound()
	{
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "ArrivalLoc";
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "1234567890";
		tempStorageHeader.SRH_SRP_Premises = premises.PK;
		Factory.Save();

		IntoTemporaryStorageHelper.ShowTemporaryStorageRegisterForm(Factory, "1234567890", "ArrivalLoc");

		CombineAssertions(() =>
		{
			var tempStorageRegisterForm = ZFormModaliser.LastFormShownDialogForTest;
			AssertType<TempStorageRegisterForm>(tempStorageRegisterForm);
			AssertNull("No Error when record entry found and Temporary Storage Register form opens successfully", UnitTestUserNotification.Instance.LastMessage.Text);
		});
	}

	public void TestShowTemporaryStorageRegisterForm_NoEntryFound()
	{
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "ArrivalLoc";
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "1234567890";
		tempStorageHeader.SRH_SRP_Premises = premises.PK;
		Factory.Save();

		CombineAssertions(() =>
		{
			IntoTemporaryStorageHelper.ShowTemporaryStorageRegisterForm(Factory, "0987654321", "ArrivalLoc");
			AssertEquals(message: "Error when no matching record found for Incorrect Summary Entry Number (0987654321)",
						expected: "No Entry in the Temporary Register found for (0987654321) in (ArrivalLoc)",
						actual: UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			IntoTemporaryStorageHelper.ShowTemporaryStorageRegisterForm(Factory, "1234567890", "GoodsLocation");
			AssertEquals(message: "Error when no matching record found for Incorrect Goods Location (GoodsLocation)",
						expected: "No Entry in the Temporary Register found for (1234567890) in (GoodsLocation)",
						actual: UnitTestUserNotification.Instance.LastMessage.Text);
		});
	}

	public void TestShowWarningIfMoreThanOnePackageLinkedToALine()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		var bill1 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		item1.IsMissing = false;
		item1.API_GrossWeight = 1;
		item1.API_GrossWeightUQ = "KG";
		var pack1 = bill1.Packs.AddNew();
		var pack2 = bill1.Packs.AddNew();
		item1.PackagesPivot.AddPivotFor(pack1);

		const string expectedMessage = "There are some Items with multiple package lines. For these packages, the Goods Item’s gross weight will be automatically apportioned on the Temporary Storage Register.";
		CombineAssertions(() =>
		{
			IntoTemporaryStorageHelper.ShowWarningIfMoreThanOnePackageLinkedToALine(tempStorage);
			AssertEquals("There is only one package associated to each Goods Item show message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			item1.PackagesPivot.AddPivotFor(pack2);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			IntoTemporaryStorageHelper.ShowWarningIfMoreThanOnePackageLinkedToALine(tempStorage);
			AssertEquals("There are multiple packages associated to one Goods Item show message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
		});
	}

	public void TestOnlyOneLineForLAM()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		var bill1 = tempStorage.Bills.AddNew();
		var bill2 = tempStorage.Bills.AddNew();
		var item1 = bill1.PackedItems.AddNew();
		var item2 = bill2.PackedItems.AddNew();

		const string expectedMessage = "Only one Goods Item is allowed per LAME Reception Certificate. Please, create a different LAM record per Goods Item and try again.";
		CombineAssertions(() =>
		{
			AssertEquals("There are multiple Goods Items", false, IntoTemporaryStorageHelper.OnlyOneLineForLAM(tempStorage));
			AssertEquals("There are multiple Goods Items show message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

			item2.Delete();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("There is only one Goods Item", true, IntoTemporaryStorageHelper.OnlyOneLineForLAM(tempStorage));
			AssertEquals("There is only one Goods Item show message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
		});
	}

	public void TestGetFormattedManualLocationOfGoodsDescription()
	{
		var expectedResult = CusGoodsLocationQualifierList.Codes.AuthorizationNumber + ";" + CusGoodsLocationTypeList.Codes.AuthorizedPlace + ";AAA";
		AssertEquals(expectedResult, IntoTemporaryStorageHelper.GetFormattedManualLocationOfGoodsDescription("AAA"));
	}
}
