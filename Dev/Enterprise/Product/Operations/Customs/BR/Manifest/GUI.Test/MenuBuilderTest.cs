using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.BR.Manifest.Business;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.Manifest.GUI.Testing
{
	public class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestFeatureControlProtectionOfMenuItemForBRManifestSending()
		{
			var countryCode = Core.Constants.CountryCodes.Brazil;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_RN_NKCountry = countryCode;
				header.CustomsOwnNumber = "1BR01831941200000000000000000062021";
				Factory.Save();

				using (var menu = new AsycudaMenuForTest(header))
				{
					using (var form = new ZForm(header))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

						AssertEquals(false, menu.MenuItems.ToList<MenuItem>().Any(x => x.Text == "Delete &Manifest"));
						AssertEquals("Send &Manifest", menu.MenuItems[0].Text);
					}
				}
			}
		}

		public void TestSaveMercanteRequest()
		{
			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "XX";
			pack1.RP_Type = RPTypeList.Codes.GlobalManifestLine;
			pack1.RP_CustomsCountry = Core.Constants.CountryCodes.Brazil;
			pack1.RP_ConversionFactor = 1;
			pack1.RP_CommercialPack = Core.Constants.PkgUnit.Bag;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Brazil);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Currency, "OUT", RefCusMapTypeList.Codes.Currency, true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Currency, "USD", "220", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Brazil);

			Factory.Save();

			using (BRCustomsDataRegistry.Instance.EnableMercanteSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header = CreateAndPopulateManifestHeader();
				header.AMA_ManifestType = BRManifestTypes.Codes.MER;
				Factory.Save();

				using (var menu = new AsycudaMenuForTest(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					using (var tempFile = TempFile.New())
					{
						try
						{
							AssertEquals(1, menu.MenuItems.Count);

							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

							menu.MenuItems.FindByText("Save Request Manifest").PerformClick();

							AssertMultilineASCIIEquals("Popup message", @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

ARV: You have not entered an Estimated Time of Arrival at Border.
Pack Unit: Package type BAG does not map to a Customs package type for country BR. Please add a mapping via Maintain > Customs > Customs Files > Packs Conversion.
Pack Unit: You have not entered a Pack Unit.
Document Type: You have not entered a Document Type.
Location of Goods: You have not entered a Location of Goods.
Manifest UQ: You have not entered a Manifest UQ.
Customs Own Number: You have not entered a CE Merchant.
Voyage/Flight: You have not entered a Voyage.

Do you want to send the message(s) despite these errors?", UnitTestUserNotification.Instance.LastMessage.Text);

							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

							var fileName = tempFile.Filename;

							ZFormModaliser.ShowDialogsInTest = true;
							ZFormModaliser.FileNameToSelectInShowCommonDialog = fileName;
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

							menu.MenuItems.FindByText("Save Request Manifest").PerformClick();

							var dateTimeExpected = DateTime.Now.ToString("yyyyMMdd");

							AssertContains("Request file MERCANTE_MEDUQ2394375_" + dateTimeExpected + " saved.", UnitTestUserNotification.Instance.LastMessage.Text);

							if (File.Exists(fileName))
							{
								Assert(true);
								File.Delete(fileName);
							}
							else
							{
								Fail("File is not Created.");
							}

							AssertEquals("The Message Status should be Sent", "SNT", header.AMA_MessageStatus);

							var storageMain = header.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(header, Core.Constants.RefDocTypes.MiscellaneousDocument);
							AssertEquals(1, storageMain.Files.Count);
							AssertEquals("MERCANTE_MEDUQ2394375_" + dateTimeExpected + ".txt", storageMain.Files[0].FileName);
							AssertEquals("MERCANTE_MEDUQ2394375_" + dateTimeExpected, header.Notes.FindByDescription("Request file downloaded")[0].ST_NoteDataAsText);
						}
						finally
						{
							DeleteIfExists(tempFile.Filename);
						}
					}
				}
			}
		}

		public void TestSaveMercanteRequestHeaderHBL()
		{
			using (BRCustomsDataRegistry.Instance.EnableMercanteSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manifestHeader = CreateAndPopulateManifestHeader();
				manifestHeader.AMA_ManifestType = BRManifestTypes.Codes.MER;
				Factory.Save();

				using (var menu = new AsycudaMenuForTest(manifestHeader))
				using (var form = new ZForm(manifestHeader))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					using (var tempFile = TempFile.New())
					{
						try
						{
							AssertEquals(1, menu.MenuItems.Count);

							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

							menu.MenuItems.FindByText("Save Request Manifest").PerformClick();

							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

							var fileName = tempFile.Filename;

							ZFormModaliser.ShowDialogsInTest = true;
							ZFormModaliser.FileNameToSelectInShowCommonDialog = fileName;
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

							menu.MenuItems.FindByText("Save Request Manifest").PerformClick();

							var header = "MD0001000000000018910000000000000001996    ";
							AssertContains(header, File.ReadAllText(fileName));

							if (File.Exists(fileName))
							{
								Assert(true);
								File.Delete(fileName);
							}
							else
							{
								Fail("File is not Created.");
							}
						}
						finally
						{
							DeleteIfExists(tempFile.Filename);
						}
					}
				}
			}
		}

		public void TestSaveMercanteRequestHBL()
		{
			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "XX";
			pack1.RP_Type = RPTypeList.Codes.GlobalManifestLine;
			pack1.RP_CustomsCountry = Core.Constants.CountryCodes.Brazil;
			pack1.RP_ConversionFactor = 1;
			pack1.RP_CommercialPack = Core.Constants.PkgUnit.Bag;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Brazil);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Currency, "OUT", RefCusMapTypeList.Codes.Currency, true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Currency, "USD", "220", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Brazil);

			Factory.Save();

			using (BRCustomsDataRegistry.Instance.EnableMercanteSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header = CreateAndPopulateManifestHeader();
				header.AMA_ManifestType = BRManifestTypes.Codes.MER;
				Factory.Save();

				using (var menu = new AsycudaMenuForTest(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					using (var tempFile = TempFile.New())
					{
						try
						{
							AssertEquals(1, menu.MenuItems.Count);

							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

							menu.MenuItems.FindByText("Save Request Manifest").PerformClick();

							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

							var fileName = tempFile.Filename;

							ZFormModaliser.ShowDialogsInTest = true;
							ZFormModaliser.FileNameToSelectInShowCommonDialog = fileName;
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

							menu.MenuItems.FindByText("Save Request Manifest").PerformClick();

							var hbl = "MD0001000000000018910000000000000001996    \r\nCD0002(H)QRHW20050055C  SN  S                                                                                                                                                                                                                                                             1996                          CONSIGNEE                                              00000000001996SHIPPER                                                                                                                                                                                                                                                      20231130GOODS                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  0000000200000UYMVDBRSAO00000000001234NOTIFY                                                                                                                                                                                                                                                                                                               0000000050000220PHPIABC        BRN                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                0011100000000020033P0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000\r\nID20001000000090718                         XX0000010                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              COMMODITY                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                \r\nID20002000000000000                           0000000                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      ";
							AssertContains(hbl, File.ReadAllText(fileName));

							if (File.Exists(fileName))
							{
								Assert(true);
								File.Delete(fileName);
							}
							else
							{
								Fail("File is not Created.");
							}
						}
						finally
						{
							DeleteIfExists(tempFile.Filename);
						}
					}
				}
			}
		}

		AsycudaManifestHeader CreateAndPopulateManifestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = BRManifestTypes.Codes.MER;
			header.AMA_RL_NKPortOfLoading = "UYMVD";
			header.AMA_RL_NKPortOfDischarge = "BRSSZ";
			header.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "123457";
			orgHeader.OH_FullName = "Party1";
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.NVOCCReference, "1996");

			var orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "1234";
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;

			header.AMA_OA_ShippingAgent = orgAddress.PK;

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "2";
			orgHeader.OH_FullName = "Party2";
			orgHeader.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "1891");

			orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "1345";
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;

			header.AMA_OA_DeconsolidateAddress = orgAddress.PK;

			var bill = CreateAndPopulateHouseBill(header);
			CreateAndPopulateMasterBill(header);
			CreateAndPopulateContainer(header);
			CreateAndPopulatePack(bill);
			CreateAndPopulateTax(bill);
			bill.Packs.AddNew();

			Factory.Save();

			return header;
		}

		AsycudaBill CreateAndPopulateHouseBill(AsycudaManifestHeader header)
		{
			var state = Factory.New<RefCountryStates>();
			state.RW_Code = "ABC";
			state.RW_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;

			var unloco = new RefUNLOCO.Loader(Factory).Load("BRSAO");
			unloco.RL_RW = state.PK;

			var bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_ManifestQty = 10;
			bill.ABL_RL_NKFinalDestination = unloco.RL_Code;
			bill.To_Order = ZBool.True;
			bill.ABL_ConsigneeName = "CONSIGNEE";
			bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Uruguay;
			bill.ABL_ConsigneeRegNoType = OrgCusCode.CodeTypes.PassportID;
			bill.ABL_ConsigneeRegNo = "1996";
			bill.ABL_ShipperRegNo = "1891";
			bill.ABL_ShipperName = "SHIPPER";
			bill.ABL_GoodsDescription = "GOODS";
			bill.ABL_Volume = 200;
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicInches;
			bill.BL_Service = ZBool.False;
			bill.FRTMode = FRTModeList.Codes.HP;
			bill.ABL_RN_NKSellerCountry = "BR";
			bill.ABL_RL_NKOrigin = "UYMVD";
			bill.ABL_NotifyPartyRegNo = "1234";
			bill.ABL_NotifyPartyName = "NOTIFY";
			bill.ABL_FreightValue = 500;
			bill.ABL_RX_NKFreightValueCurrency = "USD";
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			bill.ABL_GrossWeight = 200;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Pounds;

			return bill;
		}

		void CreateAndPopulateMasterBill(AsycudaManifestHeader header)
		{
			var bill = header.MasterBill;

			bill.ABL_BillNumber = "MEDUQ2394375";
			bill.ABL_BillIssueDate = new ZDate(2023, 11, 30);
			bill.ABL_CustomsDischargePort = "COBOG";
		}

		void CreateAndPopulateContainer(AsycudaManifestHeader header)
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "22G0";
			refContainer.RC_ContainerType = Core.Constants.ContainerTypes.FlatRack;
			refContainer.RC_TareWeight = 1000m;

			var container = header.Containers.AddNew();
			container.ACN_RC_ContainerType = refContainer.PK;
			container.ACN_ContainerNumber = "CAIU305178-6";
			container.ACN_EmptyFullIndicator = ASYCUDA.Business.EmptyFullIndicatorList.Codes.LessThanFullContainerLoad;
			container.ACN_Seal1 = "SEAL1";
			container.ACN_Seal2 = "SEAL2";
			container.ACN_Seal3 = "SEAL3";
		}

		void CreateAndPopulatePack(AsycudaBill bill)
		{
			var pack = bill.Packs.AddNew();

			pack.ContainerPK = bill.Header.Containers[0].PK;
			pack.APA_CommodityCode = "COMMODITY";
			pack.APA_GoodsDescription = "GOODS";
			pack.APA_PackQty = 10;
			pack.APA_PackUQ = Core.Constants.PkgUnit.Bag;
			pack.APA_Volume = 200;
			pack.APA_VolumeUQ = Core.Constants.Volume.CubicInches;
			pack.APA_Weight = 200;
			pack.APA_WeightUQ = Core.Constants.Weight.Pounds;
		}

		void CreateAndPopulateTax(AsycudaBill bill)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Brazil);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Currency, "OUT", RefCusMapTypeList.Codes.Currency, true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Currency, Core.Constants.CurrencyCodes.Brazil, "110", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Brazil);

			var tax = bill.AsycudaTaxes.AddNew();
			tax.AET_ChargeType = ChargeCodeList.Codes._001;
			tax.AET_ChargeAmount = 200.33;
			tax.AET_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			tax.AET_MethodOfPayment = Core.Constants.PaymentType.Prepaid;
		}
	}
}
