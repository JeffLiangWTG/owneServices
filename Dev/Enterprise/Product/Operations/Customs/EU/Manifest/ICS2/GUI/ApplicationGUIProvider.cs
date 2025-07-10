using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BatchProcessor;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using ApplicationBusinessProvider = Enterprise.Customs.EU.Manifest.ICS2.Business.ApplicationBusinessProvider;
using AsycudaBill = Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill;
using AsycudaContainer = Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaContainer;
using AsycudaManifestHeader = Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new EUICS2ManifestLayouts();

		protected override IPanelLayoutProvider GetBillLayoutCore() => new EUICS2BillLayouts();

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new EUICS2BillPartiesLayouts();

		protected override IPanelLayoutProvider GetPackedItemDetailsLayoutCore() => new EUICS2PackedItemDetailsLayouts();

		protected override IReadOnlyDictionary<string, bool> GetBillsGridColumnVisiblilityOnValueChangedCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var manifestHeader = (AsycudaManifestHeader)header;
			bool isVisible = manifestHeader.IsForwarderManifest && manifestHeader.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F44;

			return new Dictionary<string, bool>
			{
				{ AsycudaBill.Schema.ReceptacleId, isVisible }
			};
		}

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
			yield return new SupportingDocumentsUserControl();
			yield return new AdditionalInfoUserControl(ParentTabType.Bill);
			yield return new HRCMScreeningResultUserControl();
			yield return new CusSupplyChainActorReferenceUserControl();
			yield return new AsycudaTransportMeansUserControl();
			yield return new SupplementaryDeclarantUserControl();
			yield return new AdditionalFiscalReferenceUserControl();
		}

		protected override IEnumerable<IAdditionalTabPage> GetHeaderAdditionalTabPageUserControlsCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			yield return new ItineraryForManifestHeaderUserControl();
			yield return new ReferralRequestUserControl();
			if (header is AsycudaManifestHeader euics2Header && euics2Header.IsCarrierManifest)
			{
				yield return new HeaderPartiesUserControl();
			}
		}

		protected override IEnumerable<IAdditionalTabPage> GetPackAdditionalTabPageUserControlCore()
		{
			yield return new SupportingDocumentsUserControl();
			yield return new AdditionalInfoUserControl(ParentTabType.Pack);
			yield return new CusSupplyChainActorReferenceUserControl();
			yield return new AsycudaTransportMeansUserControl();
		}

		protected override IReadOnlyDictionary<bool, string[]> GetPacksGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var euics2Header = header as AsycudaManifestHeader;
			return new Dictionary<bool, string[]>
			{
				{
					euics2Header.IsPackedItemTypeOfGoodsAndGoodsValueEnabled,
					new[]
					{
						$"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_GoodsValue)}",
						$"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_RX_NKGoodsValueCurrency)}",
						$"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_TypeOfGoods)}",
					}
				}
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetPacksGridExtraColumnInfosCore()
		{
			yield return new TariffColumnStyleInfo
			{
				Caption = Res.GetString("bf77ad2a-81a9-4691-9217-f91fe19aa3c8", "Tariff"),
				ColumnName = $"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_FormattedTariff)}",
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
			};

			yield return new ZCodeFindBoxColumnStyleInfo
			{
				ColumnName = $"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_ChemicalSubstanceCode)}",
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
			};

			yield return new ZCalcEditColumnStyleInfo
			{
				ColumnName = $"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_GoodsValue)}",
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
			};

			yield return new ZCodeFindBoxColumnStyleInfo
			{
				ColumnName = $"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_RX_NKGoodsValueCurrency)}",
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
			};

			yield return new ZDropEditColumnStyleInfo
			{
				ColumnName = $"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_TypeOfGoods)}",
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
			};

			yield return new ZTextBoxColumnStyleInfo
			{
				ColumnName = $"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_MessageStatus)}",
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
			};

			yield return new ZTextBoxColumnStyleInfo
			{
				ColumnName = $"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_PackStatus)}",
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
			};
		}

		protected override IReadOnlyDictionary<bool, string[]> GetContainersGridColumnAvailabilityCore()
		{
			var result = new Dictionary<bool, string[]>
			{
				{
					false,
					new[]
					{
						nameof(AsycudaContainer.ACN_SealType1),
						nameof(AsycudaContainer.ACN_SealingPartyType),
						nameof(AsycudaContainer.ACN_SealingPartyName),
						nameof(AsycudaContainer.ACN_SealType2),
						nameof(AsycudaContainer.ACN_SealingPartyType2),
						nameof(AsycudaContainer.ACN_SealType3),
						nameof(AsycudaContainer.ACN_SealingPartyType3),
						nameof(AsycudaContainer.ACN_StowageLocation)
					}
				},
			};

			return result;
		}

		protected override IEnumerable<ZGridColumnInfo> GetContainersGridExtraColumnInfosCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			yield return new ZCheckBoxColumnStyleInfo
			{
				ColumnName = nameof(AsycudaContainer.ACN_IsShipperOwned),
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var buyerOrgFindBoxColumnStyle = new ZOrganisationFindBoxColumnStyleInfo();
			buyerOrgFindBoxColumnStyle.ColumnName = nameof(AsycudaBill.BuyerOrgPK);
			buyerOrgFindBoxColumnStyle.CaptionResourceString = Res.GetData("6FAD7AD9-5FE1-47AD-B1BC-E1EF4CC2EBD1", "Buyer");

			var buyerGroupName = Res.GetData("94110F44-AB9B-4400-833E-A62982941D82", "Buyer");
			buyerOrgFindBoxColumnStyle.GroupName = buyerGroupName;

			buyerOrgFindBoxColumnStyle.IsVisible = false;
			buyerOrgFindBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return buyerOrgFindBoxColumnStyle;

			var buyerAddressDropEditColumnStyle = new ZGuidDropEditColumnStyleInfo();
			buyerAddressDropEditColumnStyle.CaptionResourceString = Res.GetData("295CEB24-73AD-4B33-9682-53A449D15BA8", "Address", "Buyer Address", "");
			buyerAddressDropEditColumnStyle.ColumnName = nameof(AsycudaBill.ABL_OA_Buyer);
			buyerAddressDropEditColumnStyle.GroupName = buyerGroupName;
			buyerAddressDropEditColumnStyle.IsVisible = false;
			buyerAddressDropEditColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return buyerAddressDropEditColumnStyle;

			var buyerNameTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
			buyerNameTextBoxColumnStyle.ColumnName = nameof(AsycudaBill.ABL_BuyerName);
			buyerNameTextBoxColumnStyle.IsVisible = false;
			buyerNameTextBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return buyerNameTextBoxColumnStyle;

			var buyerStreet1TextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
			buyerStreet1TextBoxColumnStyle.ColumnName = nameof(AsycudaBill.ABL_BuyerStreet1);
			buyerStreet1TextBoxColumnStyle.IsVisible = false;
			buyerStreet1TextBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return buyerStreet1TextBoxColumnStyle;

			var buyerStreet2TextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
			buyerStreet2TextBoxColumnStyle.ColumnName = nameof(AsycudaBill.ABL_BuyerStreet2);
			buyerStreet2TextBoxColumnStyle.IsVisible = false;
			buyerStreet2TextBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return buyerStreet2TextBoxColumnStyle;

			var buyerCityTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
			buyerCityTextBoxColumnStyle.ColumnName = nameof(AsycudaBill.ABL_BuyerCity);
			buyerCityTextBoxColumnStyle.IsVisible = false;
			buyerCityTextBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return buyerCityTextBoxColumnStyle;

			var buyerStateDropEditColumnStyle = new ZDropEditColumnStyleInfo();
			buyerStateDropEditColumnStyle.ColumnName = nameof(AsycudaBill.ABL_BuyerState);
			buyerStateDropEditColumnStyle.IsVisible = false;
			buyerStateDropEditColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return buyerStateDropEditColumnStyle;

			var buyerPhoneTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
			buyerPhoneTextBoxColumnStyle.ColumnName = nameof(AsycudaBill.ABL_BuyerPhone);
			buyerPhoneTextBoxColumnStyle.IsVisible = false;
			buyerPhoneTextBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return buyerPhoneTextBoxColumnStyle;

			var buyerPostcodeTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
			buyerPostcodeTextBoxColumnStyle.ColumnName = nameof(AsycudaBill.ABL_BuyerPostcode);
			buyerPostcodeTextBoxColumnStyle.IsVisible = false;
			buyerPostcodeTextBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return buyerPostcodeTextBoxColumnStyle;

			var buyerCountryCodeFindBoxColumnStyle = new ZCodeFindBoxColumnStyleInfo();
			buyerCountryCodeFindBoxColumnStyle.ColumnName = nameof(AsycudaBill.ABL_RN_NKBuyerCountry);
			buyerCountryCodeFindBoxColumnStyle.IsVisible = false;
			buyerCountryCodeFindBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return buyerCountryCodeFindBoxColumnStyle;

			var sellerOrgFindBoxColumnStyle = new ZOrganisationFindBoxColumnStyleInfo();
			sellerOrgFindBoxColumnStyle.ColumnName = nameof(AsycudaBill.SellerOrgPK);
			sellerOrgFindBoxColumnStyle.CaptionResourceString = Res.GetData("4C090B74-2BC4-4C28-A30B-A1AC642B7566", "Seller");

			var sellerGroupName = Res.GetData("D425925B-BC9E-40E1-B726-7B5CBB631225", "ICS2 Seller");

			sellerOrgFindBoxColumnStyle.GroupName = sellerGroupName;
			sellerOrgFindBoxColumnStyle.IsVisible = false;
			sellerOrgFindBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return sellerOrgFindBoxColumnStyle;

			var sellerAddressDropEditColumnStyle = new ZGuidDropEditColumnStyleInfo();
			sellerAddressDropEditColumnStyle.CaptionResourceString = Res.GetData("F313D6A0-39A3-40F9-B0C9-D499A84DB7B0", "Address", "Seller Address", "");
			sellerAddressDropEditColumnStyle.ColumnName = nameof(AsycudaBill.ABL_OA_Seller);
			sellerAddressDropEditColumnStyle.GroupName = sellerGroupName;
			sellerAddressDropEditColumnStyle.IsVisible = false;
			sellerAddressDropEditColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return sellerAddressDropEditColumnStyle;

			var sellerNameTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
			sellerNameTextBoxColumnStyle.ColumnName = nameof(AsycudaBill.ABL_SellerName);
			sellerNameTextBoxColumnStyle.IsVisible = false;
			sellerNameTextBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return sellerNameTextBoxColumnStyle;

			var sellerStreet1TextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
			sellerStreet1TextBoxColumnStyle.ColumnName = nameof(AsycudaBill.ABL_SellerStreet1);
			sellerStreet1TextBoxColumnStyle.IsVisible = false;
			sellerStreet1TextBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return sellerStreet1TextBoxColumnStyle;

			var sellerStreet2TextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
			sellerStreet2TextBoxColumnStyle.ColumnName = nameof(AsycudaBill.ABL_SellerStreet2);
			sellerStreet2TextBoxColumnStyle.IsVisible = false;
			sellerStreet2TextBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return sellerStreet2TextBoxColumnStyle;

			var sellerCityTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
			sellerCityTextBoxColumnStyle.ColumnName = nameof(AsycudaBill.ABL_SellerCity);
			sellerCityTextBoxColumnStyle.IsVisible = false;
			sellerCityTextBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return sellerCityTextBoxColumnStyle;

			var sellerStateDropEditColumnStyle = new ZDropEditColumnStyleInfo();
			sellerStateDropEditColumnStyle.ColumnName = nameof(AsycudaBill.ABL_SellerState);
			sellerStateDropEditColumnStyle.IsVisible = false;
			sellerStateDropEditColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return sellerStateDropEditColumnStyle;

			var sellerPhoneTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
			sellerPhoneTextBoxColumnStyle.ColumnName = nameof(AsycudaBill.ABL_SellerPhone);
			sellerPhoneTextBoxColumnStyle.IsVisible = false;
			sellerPhoneTextBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return sellerPhoneTextBoxColumnStyle;

			var sellerPostcodeTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
			sellerPostcodeTextBoxColumnStyle.ColumnName = nameof(AsycudaBill.ABL_SellerPostcode);
			sellerPostcodeTextBoxColumnStyle.IsVisible = false;
			sellerPostcodeTextBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return sellerPostcodeTextBoxColumnStyle;

			var sellerCountryCodeFindBoxColumnStyle = new ZCodeFindBoxColumnStyleInfo();
			sellerCountryCodeFindBoxColumnStyle.ColumnName = nameof(AsycudaBill.ABL_RN_NKSellerCountry);
			sellerCountryCodeFindBoxColumnStyle.IsVisible = false;
			sellerCountryCodeFindBoxColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return sellerCountryCodeFindBoxColumnStyle;

			var receptacleIdColumnStyle = new ZTextBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = nameof(AsycudaBill.ReceptacleId),
				IsVisible = false,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			};
			yield return receptacleIdColumnStyle;
		}

		protected override IEnumerable<ZMenuItem> GetActionsExtraMenuItemsCore()
		{
			yield return new ZMenuItem(ResString.GetMultilingualString("0DA26E62-CE7C-43EB-AA64-E3136224B94F", "ICS2 Test Message"), SendICS2TestMessage);
		}

		void SendICS2TestMessage(object sender, EventArgs e)
		{
			var zForm = ((sender as ZMenuItem).GetMainMenu().GetForm() as ZForm);
			var readOnlyFactory = new ReadOnlyBusinessObjectFactory() { ReportErrorOnSaveAttempt = false };
			var header = readOnlyFactory.ImportFromAnotherFactorySafe(zForm.DataSource as AsycudaManifestHeader);
			if (header.DeclarantEori.IsEmpty)
			{
				Globals.Message.ShowError(Res.GetString("A9E86D75-B95D-4245-AA11-D5E5D349D71C",
					"No Declarant in Manifest or the Declarant does not have an EORI. Could not send."));
				return;
			}
			new EUManifestMessageSender(header).SendMessage(MessageTypes.Codes.Q04);
			var newMessage = header.Messages.First(m => !m.IsInDatabase) as EDIMessage;
			newMessage.EM_MessageText = ZString.Empty;
			var interchange = newMessage.Factory.New<EDIInterchange>();
			_ = new EUICS2MessagePacker().Pack(newMessage, interchange, new LoggingInformation());
			var interchangeToSave = (zForm.DataSource as BusinessObject).Factory.ImportFromAnotherFactory(interchange) as EDIInterchange;
			interchangeToSave.SetEI_BodyDataSource(new StreamSource(interchange.GetEI_BodyDataReader()));
			if (zForm.FireSaveButton() == ContinueWithSave.Yes)
			{
				Globals.Message.Show($"EDI interchange {interchangeToSave.EI_InterchangeNum} created.");
			}
		}

		protected override bool IsInEnforceOnlyValidTransportModeCore => true;

		protected override bool IsInEnforceOnlyValidSpecificCircumstanceIndicatorCore => true;
	}
}
