using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Module.Testing
{
	class EUH7BillFilterStripControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestConvertToStandAloneDeclarationMenuItemIsAdded()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			Factory.Save();

			using (var module = new EUH7BillModule())
			using (var form = new ZForm())
			{
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				var grid = filterControl.FilteredGrid;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.FirePerformSearch();

				grid.SelectAllElements();
				grid.ContextMenu.DoPopup();

				var convertMenuItem = grid.ContextMenu.MenuItems.FindByText("Convert to Stand Alone Declaration");
				AssertNotNull("Convert To Stand Alone Declaration menu item is added in bill filter grid", convertMenuItem);
			}
		}

		[RequiresSTA]
		public void TestConvertToStandAloneDeclarationActionMenu_OpenDeclarationForm()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Consignee";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "ACC";
			bill.ABL_OA_Consignee = orgHeader.MainAddress.PK;
			bill.ABL_OA_Shipper = orgHeader.MainAddress.PK;
			Factory.Save();

			using (var module = new EUH7BillModule())
			using (var form = new ZForm())
			{
				var filterControl = (EUH7BillFilterStripControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);
				grid.ContextMenu.DoPopup();

				var convertMenuItem = grid.ContextMenu.MenuItems.FindByText("Convert to Stand Alone Declaration");
				convertMenuItem.PerformClick();
				Assert(ZFormModaliser.LastFormShownDialogForTest is JobDeclarationForm);
			}
		}

		[RequiresSTA]
		public void TestHeader_AMA_JobReference()
		{
			AssertColumnExists("Header+AMA_JobReference");
		}

		[RequiresSTA]
		public void TestHeader_AMA_MasterBill()
		{
			AssertColumnExists("Header+AMA_MasterBill");
		}

		[RequiresSTA]
		public void TestABL_BillNumber()
		{
			AssertColumnExists("ABL_BillNumber");
		}

		[RequiresSTA]
		public void TestABL_MessageStatus()
		{
			AssertColumnExists("ABL_MessageStatus");
			AssertColumnCaption("ABL_MessageStatus", "Message Status", "Msg. Status", "Msg. Status");
		}

		[RequiresSTA]
		public void TestABL_BillStatus()
		{
			AssertColumnExists("ABL_BillStatus");
			AssertColumnCaption("ABL_BillStatus", "Customs Status", "Cus. Status", "Cus. Status");
		}

		[RequiresSTA]
		public void TestHeader_AMA_TransportMode()
		{
			AssertColumnExists("Header+AMA_TransportMode");
		}

		[RequiresSTA]
		public void TestHeader_AMA_RL_NKPortOfDischarge()
		{
			AssertColumnExists("Header+AMA_RL_NKPortOfDischarge");
		}

		[RequiresSTA]
		public void TestHeader_AMA_E_ARV()
		{
			AssertColumnExists("Header+AMA_E_ARV");
		}

		[RequiresSTA]
		public void TestHeader_AMA_RN_NKConveyanceNationality()
		{
			AssertColumnExists("Header+AMA_RN_NKConveyanceNationality");
		}

		[RequiresSTA]
		public void TestHeader_AMA_VesselName()
		{
			AssertColumnExists("Header+AMA_VesselName");
		}

		[RequiresSTA]
		public void TestHeader_AMA_Voyage()
		{
			AssertColumnExists("Header+AMA_Voyage");
		}

		[RequiresSTA]
		public void TestCountryCode()
		{
			AssertColumnExists("CountryCode");
		}

		[RequiresSTA]
		public void TestHeader_AMA_E_DEP()
		{
			AssertColumnExists("Header+AMA_E_DEP");
		}

		[RequiresSTA]
		public void TestHeader_AMA_RL_NKPortOfLoading()
		{
			AssertColumnExists("Header+AMA_RL_NKPortOfLoading");
		}

		[RequiresSTA]
		public void TestABL_SystemCreateUser()
		{
			AssertColumnExists("ABL_SystemCreateUser");
		}

		[RequiresSTA]
		public void TestABL_SystemCreateTimeUtc()
		{
			AssertColumnExists("ABL_SystemCreateTimeUtc");
		}

		[RequiresSTA]
		public void TestABL_SystemLastEditUser()
		{
			AssertColumnExists("ABL_SystemLastEditUser");
		}

		[RequiresSTA]
		public void TestABL_SystemLastEditTimeUtc()
		{
			AssertColumnExists("ABL_SystemLastEditTimeUtc");
		}

		[RequiresSTA]
		public void TestHeader_AMA_AgentType()
		{
			AssertColumnExists("Header+AMA_AgentType");
		}

		[RequiresSTA]
		public void TestABL_CustomsValue()
		{
			AssertColumnExists("ABL_CustomsValue");
		}

		[RequiresSTA]
		public void TestHeader_AMA_ContainerMode()
		{
			AssertColumnExists("Header+AMA_ContainerMode");
		}

		[RequiresSTA]
		public void TestHeader_AMA_VehicleRegistration()
		{
			AssertColumnExists("Header+AMA_VehicleRegistration");
		}

		[RequiresSTA]
		public void TestHeader_AMA_MasterBillIssueDate()
		{
			AssertColumnExists("Header+AMA_MasterBillIssueDate");
		}

		[RequiresSTA]
		public void TestHeader_AMA_ManifestType()
		{
			AssertColumnExists("Header+AMA_ManifestType");
		}

		[RequiresSTA]
		public void TestHeader_AMA_Nature()
		{
			AssertColumnExists("Header+AMA_Nature");
		}

		[RequiresSTA]
		public void TestHeader_AMA_CustomsOffice()
		{
			AssertColumnExists("Header+AMA_CustomsOffice");
		}

		[RequiresSTA]
		public void TestHeader_AMA_RL_NKPortOfFirstArrival()
		{
			AssertColumnExists("Header+AMA_RL_NKPortOfFirstArrival");
		}

		[RequiresSTA]
		public void TestHeader_ShippingAgentName()
		{
			AssertColumnExists("Header+ShippingAgentName");
		}

		[RequiresSTA]
		public void TestHeader_ShippingAgentAddress()
		{
			AssertColumnExists("Header+ShippingAgentAddress");
		}

		[RequiresSTA]
		public void TestHeader_AMA_CarrierCode()
		{
			AssertColumnExists("Header+AMA_CarrierCode");
		}

		[RequiresSTA]
		public void TestABL_RL_NKOrigin()
		{
			AssertColumnExists("ABL_RL_NKOrigin");
		}

		[RequiresSTA]
		public void TestABL_RL_NKFinalDestination()
		{
			AssertColumnExists("ABL_RL_NKFinalDestination");
		}

		[RequiresSTA]
		public void TestABL_ManifestQty()
		{
			AssertColumnExists("ABL_ManifestQty");
		}

		[RequiresSTA]
		public void TestABL_ManifestUQ()
		{
			AssertColumnExists("ABL_ManifestUQ");
		}

		[RequiresSTA]
		public void TestABL_GrossWeight()
		{
			AssertColumnExists("ABL_GrossWeight");
		}

		[RequiresSTA]
		public void TestABL_GrossWeightUQ()
		{
			AssertColumnExists("ABL_GrossWeightUQ");
		}

		[RequiresSTA]
		public void TestABL_Volume()
		{
			AssertColumnExists("ABL_Volume");
		}

		[RequiresSTA]
		public void TestABL_VolumeUQ()
		{
			AssertColumnExists("ABL_VolumeUQ");
		}

		[RequiresSTA]
		public void TestABL_GoodsDescription()
		{
			AssertColumnExists("ABL_GoodsDescription");
		}

		[RequiresSTA]
		public void TestShipper_Header_OH_FullName()
		{
			AssertColumnExists("Shipper+Header+OH_FullName");
		}

		[RequiresSTA]
		public void TestConsignee_Header_OH_FullName()
		{
			AssertColumnExists("Consignee+Header+OH_FullName");
		}

		[RequiresSTA]
		public void TestNotifyParty_Header_OH_FullName()
		{
			AssertColumnExists("NotifyParty+Header+OH_FullName");
		}

		[RequiresSTA]
		public void TestABL_Remarks()
		{
			AssertColumnExists("ABL_Remarks");
		}

		[RequiresSTA]
		public void TestABL_PrepaidCollect()
		{
			AssertColumnExists("ABL_PrepaidCollect");
		}

		[RequiresSTA]
		public void TestABL_FreightValue()
		{
			AssertColumnExists("ABL_FreightValue");
		}

		[RequiresSTA]
		public void TestABL_RX_NKFreightValueCurrency()
		{
			AssertColumnExists("ABL_RX_NKFreightValueCurrency");
		}

		[RequiresSTA]
		public void TestABL_TransportValue()
		{
			AssertColumnExists("ABL_TransportValue");
		}

		[RequiresSTA]
		public void TestABL_RX_NKTransportValueCurrency()
		{
			AssertColumnExists("ABL_RX_NKTransportValueCurrency");
		}

		[RequiresSTA]
		public void TestABL_InsuranceValue()
		{
			AssertColumnExists("ABL_InsuranceValue");
		}

		[RequiresSTA]
		public void TestABL_RX_NKInsuranceValueCurrency()
		{
			AssertColumnExists("ABL_RX_NKInsuranceValueCurrency");
		}

		[RequiresSTA]
		public void TestABL_RX_NKCustomsValueCurrency()
		{
			AssertColumnExists("ABL_RX_NKCustomsValueCurrency");
		}

		[RequiresSTA]
		public void TestDiscountValue()
		{
			AssertColumnExists("DiscountValue");
		}

		[RequiresSTA]
		public void TestDiscountValueCurrency()
		{
			AssertColumnExists("DiscountValueCurrency");
		}

		[RequiresSTA]
		public void TestOtherChargesValue()
		{
			AssertColumnExists("OtherChargesValue");
		}

		[RequiresSTA]
		public void TestOtherChargesValueCurrency()
		{
			AssertColumnExists("OtherChargesValueCurrency");
		}

		[RequiresSTA]
		public void TestABL_CarrierReference()
		{
			AssertColumnExists("ABL_CarrierReference");
		}

		[RequiresSTA]
		public void TestABL_UCRNumber()
		{
			AssertColumnExists("ABL_UCRNumber");
		}

		[RequiresSTA]
		public void TestRegistrationNumber()
		{
			AssertColumnExists("RegistrationNumber");
		}

		[RequiresSTA]
		public void TestRegistrationDate()
		{
			AssertColumnExists("RegistrationDate");
		}

		[RequiresSTA]
		public void TestCustomsJobNumber()
		{
			AssertColumnExists("CustomsJobNumber");
		}

		[RequiresSTA]
		public void TestABL_BillIssuer()
		{
			AssertColumnExists("ABL_BillIssuer");
		}

		[RequiresSTA]
		public void TestABL_BolType()
		{
			AssertColumnExists("ABL_BolType");
		}

		[RequiresSTA]
		public void TestABL_ShipmentType()
		{
			AssertColumnExists("ABL_ShipmentType");
		}

		[RequiresSTA]
		public void TestABL_CargoStatus()
		{
			AssertColumnExists("ABL_CargoStatus");
		}

		[RequiresSTA]
		public void TestABL_ConsigneeName()
		{
			AssertColumnExists("ABL_ConsigneeName");
		}

		[RequiresSTA]
		public void TestABL_ConsigneeStreet1()
		{
			AssertColumnExists("ABL_ConsigneeStreet1");
		}

		[RequiresSTA]
		public void TestABL_ConsigneeStreet2()
		{
			AssertColumnExists("ABL_ConsigneeStreet2");
		}

		[RequiresSTA]
		public void TestABL_ConsigneeCity()
		{
			AssertColumnExists("ABL_ConsigneeCity");
		}

		[RequiresSTA]
		public void TestABL_ConsigneeState()
		{
			AssertColumnExists("ABL_ConsigneeState");
		}

		[RequiresSTA]
		public void TestABL_RN_NKConsigneeCountry()
		{
			AssertColumnExists("ABL_RN_NKConsigneeCountry");
		}

		[RequiresSTA]
		public void TestABL_ConsigneePostcode()
		{
			AssertColumnExists("ABL_ConsigneePostcode");
		}

		[RequiresSTA]
		public void TestABL_ShipperName()
		{
			AssertColumnExists("ABL_ShipperName");
		}

		[RequiresSTA]
		public void TestABL_ShipperStreet1()
		{
			AssertColumnExists("ABL_ShipperStreet1");
		}

		[RequiresSTA]
		public void TestABL_ShipperStreet2()
		{
			AssertColumnExists("ABL_ShipperStreet2");
		}

		[RequiresSTA]
		public void TestABL_ShipperCity()
		{
			AssertColumnExists("ABL_ShipperCity");
		}

		[RequiresSTA]
		public void TestABL_ShipperState()
		{
			AssertColumnExists("ABL_ShipperState");
		}

		[RequiresSTA]
		public void TestABL_RN_NKShipperCountry()
		{
			AssertColumnExists("ABL_RN_NKShipperCountry");
		}

		[RequiresSTA]
		public void TestABL_ShipperPostcode()
		{
			AssertColumnExists("ABL_ShipperPostcode");
		}

		[RequiresSTA]
		public void TestABL_NotifyPartyName()
		{
			AssertColumnExists("ABL_NotifyPartyName");
		}

		[RequiresSTA]
		public void TestABL_NotifyPartyStreet1()
		{
			AssertColumnExists("ABL_NotifyPartyStreet1");
		}

		[RequiresSTA]
		public void TestABL_NotifyPartyStreet2()
		{
			AssertColumnExists("ABL_NotifyPartyStreet2");
		}

		[RequiresSTA]
		public void TestABL_NotifyPartyCity()
		{
			AssertColumnExists("ABL_NotifyPartyCity");
		}

		[RequiresSTA]
		public void TestABL_NotifyPartyState()
		{
			AssertColumnExists("ABL_NotifyPartyState");
		}

		[RequiresSTA]
		public void TestABL_RN_NKNotifyPartyCountry()
		{
			AssertColumnExists("ABL_RN_NKNotifyPartyCountry");
		}

		[RequiresSTA]
		public void TestABL_NotifyPartyPostcode()
		{
			AssertColumnExists("ABL_NotifyPartyPostcode");
		}

		[RequiresSTA]
		public void TestABL_GoodsLocation()
		{
			AssertColumnExists("ABL_GoodsLocation");
		}

		[RequiresSTA]
		public void TestABL_LocationInformation()
		{
			AssertColumnExists("ABL_LocationInformation");
		}

		[RequiresSTA]
		public void TestCustomsEntryNumber()
		{
			AssertColumnExists("CustomsEntryNumber");
		}

		[RequiresSTA]
		public void TestCustomsEntryNumberType()
		{
			AssertColumnExists("CustomsEntryNumberType");
		}

		[RequiresSTA]
		public void TestLocalReferenceNumber()
		{
			AssertColumnExists("LocalReferenceNumber");
		}

		[RequiresSTA]
		public void TestMovementReferenceNumber()
		{
			AssertColumnExists("MovementReferenceNumber");
		}

		void AssertColumnExists(string columnName)
		{
			using (var module = new EUH7BillModule())
			using (var form = new ZForm())
			{
				var filterControl = (EUH7BillFilterStripControl)module.EmbeddedControl;
				var column = filterControl.Grid.ColumnStyles
							.OfType<ZTextBoxColumnStyleInfo>()
							.FirstOrDefault(c => c.ColumnName == columnName);
				AssertNotNull(column);
			}
		}

		void AssertColumnCaption(string columnName, string caption, string mediumCaption, string shortCaption)
		{
			using (var module = new EUH7BillModule())
			using (var form = new ZForm())
			{
				var filterControl = (EUH7BillFilterStripControl)module.EmbeddedControl;
				var column = filterControl.Grid.ColumnStyles
							.OfType<ZTextBoxColumnStyleInfo>()
							.FirstOrDefault(c => c.ColumnName == columnName);

				CombineAssertions(() =>
				{
					AssertEquals("Caption", caption, column.CaptionResourceString.Caption);
					AssertEquals("MediumCaption", mediumCaption, column.CaptionResourceString.MediumCaption);
					AssertEquals("ShortCaption", shortCaption, column.CaptionResourceString.ShortCaption);
				});
			}
		}
	}
}
