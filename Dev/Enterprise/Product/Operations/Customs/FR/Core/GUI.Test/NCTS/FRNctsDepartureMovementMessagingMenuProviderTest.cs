using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.NCTS.ServiceTask;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsTransitStatusList = Enterprise.Customs.FR.Business.NctsTransitStatusList;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	class FRNctsDepartureMovementMessagingMenuProviderTest : TestCaseWithFactory
	{
		public void TestIsSendDepartureDeclarationMenuAllowed()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var departureMovement = nctsHeader.MovementHeader;
			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			{
				var departureMovementMessagingMenuProvider = new FRNctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm);

				departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
				Assert(departureMovementMessagingMenuProvider.IsSendDepartureDeclarationMenuAllowed());

				departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationRejected;
				Assert(departureMovementMessagingMenuProvider.IsSendDepartureDeclarationMenuAllowed());

				departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
				Assert(departureMovementMessagingMenuProvider.IsSendDepartureDeclarationMenuAllowed());

				departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.ReadyForAmendment;
				Assert(departureMovementMessagingMenuProvider.IsSendDepartureDeclarationMenuAllowed());

				departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
				Assert(!departureMovementMessagingMenuProvider.IsSendDepartureDeclarationMenuAllowed());
			}
		}

		[TestDate(2020, 5, 20, 10, 0, 0)]
		public void TestMessageTypeForF15Message()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_InBondEntryType = "T1";
			nctsHeader.MovementHeader.BM_RL_NKDestinationPort = "IT";
			nctsHeader.MovementHeader.BM_LocationOfGoods = "Pre-Lodged";
			nctsHeader.MovementHeader.BM_LocationOfGoodsCode = "954131533-GB60DEP"; // TODO Port of presentation - GBLHRBAC
			nctsHeader.MovementHeader.BM_RL_NKForeignDestPort = "GBDVR";
			nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.France;
			nctsHeader.MovementHeader.BM_TOLCarrierID = "NC15REG";
			nctsHeader.MovementHeader.BM_EntryDate = ZDateTime.Today;

			var departureOffice = nctsHeader.CustomsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			departureOffice.CY_Data = "FR000060";
			var destinationOffice = nctsHeader.CustomsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDestination);
			destinationOffice.CY_Data = "IT021300";

			nctsHeader.MovementHeader.BM_MethodOfPayment = "A";
			nctsHeader.MovementHeader.BM_ExportTransportMode = EU.Business.ModeOfTransportList.Codes._3_RoadTransport;
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			nctsHeader.IsPrelodgedMovement = true;
			Factory.Save();

			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			{
				var menu = nctsMovementForm.Menu.MenuItems.FindByText("Send &Departure Message");
				var departureMovementMessagingMenuProvider = new FRNctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm);

				departureMovementMessagingMenuProvider.SendDepartureMessasgeClickCore(menu);
				AssertEquals(1, nctsHeader.Messages.Count);
				var message = nctsHeader.Messages[0];
				AssertEquals("F15", message.EM_MessageType);

				nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
				nctsHeader.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.AmendmentAccepted;
				nctsHeader.IsPrelodgedMovement = true;
				Factory.Save();

				departureMovementMessagingMenuProvider = new FRNctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm);
				departureMovementMessagingMenuProvider.SendDepartureMessasgeClickCore(menu);
				AssertEquals(2, nctsHeader.Messages.Count);
				message = nctsHeader.Messages[1];
				AssertEquals("F15", message.EM_MessageType);

				nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
				nctsHeader.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.AmendmentRefused;
				nctsHeader.IsPrelodgedMovement = true;
				Factory.Save();

				departureMovementMessagingMenuProvider = new FRNctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm);
				departureMovementMessagingMenuProvider.SendDepartureMessasgeClickCore(menu);
				AssertEquals(3, nctsHeader.Messages.Count);
				message = nctsHeader.Messages[2];
				AssertEquals("F15", message.EM_MessageType);

				nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
				nctsHeader.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.AmendmentRefused;
				nctsHeader.IsPrelodgedMovement = false;
				Factory.Save();

				departureMovementMessagingMenuProvider = new FRNctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm);
				departureMovementMessagingMenuProvider.SendDepartureMessasgeClickCore(menu);
				AssertEquals(3, nctsHeader.Messages.Count);
			}
		}

		public void TestWriteOffMenuItemVisibility()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var departureMovement = nctsHeader.MovementHeader;
			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			{
				var departureMovementMessagingMenuProvider = new FRNctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm);

				AssertWriteOffMenuItemVisibility(true, EU.NCTS.Business.NctsMovementType.Codes.Departure, true, NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture);

				AssertWriteOffMenuItemVisibility(true, EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival, true, NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture);

				AssertWriteOffMenuItemVisibility(false, EU.NCTS.Business.NctsMovementType.Codes.Arrival, true, NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture);

				AssertWriteOffMenuItemVisibility(false, EU.NCTS.Business.NctsMovementType.Codes.Departure, false, NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture);

				AssertWriteOffMenuItemVisibility(false, EU.NCTS.Business.NctsMovementType.Codes.Departure, true, NctsTransitStatusList.Codes.Unknown);

				void AssertWriteOffMenuItemVisibility(bool expectedResult, ZString movementType, bool frNctsManualWriteOffAllowed, ZString customsStatus)
				{
					nctsHeader.BH_HeaderType = movementType;
					Env.Security.FRNctsManualWriteOff.IsAllowed = frNctsManualWriteOffAllowed;
					departureMovement.BM_CustomsStatus = customsStatus;

					AssertEquals("Write-off menu item should be visible only when movement is Departure and Customs Status is DRL and Security allows to force write-off of guarantees.", expectedResult, departureMovementMessagingMenuProvider.IsWriteOffAllowed());
				}
			}
		}

		public void TestWriteOffClick()
		{
			var nctsHeader = CreateNctsHeader("NCT00001001", "NCTS0001");
			var permitHelper = new PermitTestDataHelper(Factory);
			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			{
				var writeOffMenuItem = nctsMovementForm.Menu.MenuItems.FindByText("Write Off", true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				writeOffMenuItem.PerformClick();

				AssertEquals("The Job has not yet been saved, Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				writeOffMenuItem.PerformClick();

				AssertEquals("Please confirm by typing \"Yes\" if you wish to manually close this job. By doing that, the departure status will be set to AWO (Goods Written-off) and guarantees will be written off (if any).", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				writeOffMenuItem.PerformClick();
				var headerFromDb = new BusinessObjectFactory().Load<NctsHeader>(nctsHeader.PK);

				AssertEquals("BM_CustomsStatus should set to AWO", NctsTransitStatusList.Codes.GoodsWrittenOff, nctsHeader.MovementHeader.BM_CustomsStatus);
				AssertEquals("After the guarantees are written off, we should save the Job", NctsTransitStatusList.Codes.GoodsWrittenOff, headerFromDb.MovementHeader.BM_CustomsStatus);
				AssertEquals("Should show successful message when operation is done", "Success!", UnitTestUserNotification.Instance.LastMessage.Text);

				var nctsGuarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
				nctsGuarantee.PW_BondNumber = "001";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				writeOffMenuItem.PerformClick();

				AssertEquals("Should show error message when operation is done", "Guarantee was not written off for NCT00001001 because 001 does not refer to a guarantee managed by CW1.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUpdateGuaranteeTransactionsIfNeeded()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.France);
			var nctsHeader = CreateNctsHeader("NCT00001001", "NCTS0001");
			var permitHelper = new PermitTestDataHelper(Factory);
			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			{
				var departureMovementMessagingMenuProvider = new FRNctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm);
				var errorMessageList = new List<ZString>();
				DTCC045AProcessor.UpdateGuaranteeTransactionsIfNeeded(nctsHeader, errorMessageList);
				Factory.Save();
				var query = permitHelper.GetPermitLineTransactionQuery("NCT00001001", "NCTS write-off NCT00001001 [NCTS0001]", "Manual Closure", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
				var transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);

				AssertEquals(0, errorMessageList.Count);
				AssertEquals("Without guarantees, we shouldn't add the transaction.", 0, transactionRequested.Length);

				CreateGuaranteeHeader("001", nctsHeader.Principal.Address.OA_OH);
				var nctsGuarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
				nctsGuarantee.PW_BondNumber = "001";
				nctsGuarantee.PW_BondAmount = 100m;
				DTCC045AProcessor.UpdateGuaranteeTransactionsIfNeeded(nctsHeader, errorMessageList);
				Factory.Save();
				transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);

				AssertEquals(0, errorMessageList.Count);
				AssertEquals(1, transactionRequested.Length);
				AssertEquals(PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);
				AssertEquals("NCT00001001", transactionRequested[0].CPL_Reference);
				AssertEquals(100m, transactionRequested[0].CPL_TranValue);
			}
		}

		public void TestUpdateGuaranteeTransactionsIfNeeded_ErrorMessage()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.France);
			var nctsHeader = CreateNctsHeader("NCT00001001", "NCTS0001");
			var permitHelper = new PermitTestDataHelper(Factory);
			using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
			{
				var departureMovementMessagingMenuProvider = new FRNctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm);
				var errorMessageList = new List<ZString>();
				var nctsGuarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
				nctsGuarantee.PW_BondNumber = "001";
				nctsGuarantee.PW_BondAmount = -100m;
				DTCC045AProcessor.UpdateGuaranteeTransactionsIfNeeded(nctsHeader, errorMessageList);
				Factory.Save();
				var query = permitHelper.GetPermitLineTransactionQuery("NCT00001001", "NCTS write-off NCT00001001 [NCTS0001]", "Manual Closure", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
				var transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);

				AssertEquals(1, errorMessageList.Count);
				AssertEquals("Guarantee was not written off for NCT00001001 because 001 does not refer to a guarantee managed by CW1.", errorMessageList[0]);
				AssertEquals(0, transactionRequested.Length);

				CreateGuaranteeHeader("001", nctsHeader.Principal.Address.OA_OH);
				DTCC045AProcessor.UpdateGuaranteeTransactionsIfNeeded(nctsHeader, errorMessageList);
				Factory.Save();
				transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);

				AssertEquals(2, errorMessageList.Count);
				AssertEquals("The guarantee 001 available amount will be exceeded by 100. The total is 0 and the available is 0.", errorMessageList[1]);
				AssertEquals(0, transactionRequested.Length);
			}
		}

		NctsHeader CreateNctsHeader(ZString reference, ZString entryNum)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_JobReference = reference;
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = entryNum;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return nctsHeader;
		}

		CusGuaranteeHeader CreateGuaranteeHeader(ZString number, ZGuid orgHeaderPK, string countryCode = Core.Constants.CountryCodes.France)
		{
			var result = Factory.New<CusGuaranteeHeader>();
			result.CPH_OH_PermitHolder = orgHeaderPK;
			result.CPH_RN_NKCountryCode = countryCode;
			result.CPH_StartDate = ZDate.Today.AddDays(-1);
			result.CPH_Type = GuaranteeTypeList.Codes.COD;
			result.CPH_Number = number;
			return result;
		}
	}

	class FRNctsDepartureMovementMessagingMenuProviderForTest : FRNctsDepartureMovementMessagingMenuProvider
	{
		public FRNctsDepartureMovementMessagingMenuProviderForTest(NctsHeader header, NctsMovementForm nctsMovementForm) : base(header, nctsMovementForm)
		{ }
		public new bool IsSendDepartureDeclarationMenuAllowed()
		{
			return base.IsSendDepartureDeclarationMenuAllowed();
		}

		public new bool IsWriteOffAllowed()
		{
			return base.IsWriteOffAllowed();
		}

		public new void SendDepartureMessasgeClickCore(object sender)
		{
			base.SendDepartureMessasgeClickCore(sender);
		}
	}
}
