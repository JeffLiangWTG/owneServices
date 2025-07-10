using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class PreValidateTraderInfoTest : TestCaseWithFactory
	{
		public void TestPreValidateTraderInfo()
		{
			CombineAssertions(() =>
			{
				AssertEquals(preValidateTraderInfo.CanSend, actual: true);
				declaration = Factory.New<EMCSJobDeclaration>();
				declaration.SupplierDocumentaryAddress.OrganisationPK = TestHelper.GetPartyTraderExciseNumberOrg(Factory, string.Empty, Core.Constants.CountryCodes.UnitedKingdom).PK;
				preValidateTraderInfo = new PreValidateTraderInfo(declaration);
				AssertEquals(preValidateTraderInfo.CanSend, actual: false);
			});
		}

		public void TestTraderData()
		{
			var trader1 = new PreValidateTraderInfo.TraderData("Trader", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.EU);
			var trader2 = new PreValidateTraderInfo.TraderData("Trader", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.EU);
			var trader3 = trader2;
			var trader4 = new PreValidateTraderInfo.TraderData("Trader1", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.UK);
			var trader5 = new PreValidateTraderInfo.TraderData(string.Empty, EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.UK);

			CombineAssertions(() =>
			{
				AssertTraderData(trader1, "Trader", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.EU, isValid: true);
				AssertTraderData(trader2, "Trader", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.EU, isValid: true);
				AssertEquals(trader2, trader1);
				AssertTraderData(trader3, "Trader", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.EU, isValid: true);
				AssertEquals(trader3, trader2);
				AssertTraderData(trader4, "Trader1", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.UK, isValid: true);
				AssertNotEquals(trader4, trader3);
				AssertTraderData(trader5, string.Empty, EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.UK, isValid: false);
			});
		}

		public void TestGetTraders()
		{
			var expected = new PreValidateTraderInfo.TraderData[]
			{
				new PreValidateTraderInfo.TraderData("TEN100", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.UK),
				new PreValidateTraderInfo.TraderData("TEN200", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.EU),
				new PreValidateTraderInfo.TraderData("TEN300", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.UK),
				new PreValidateTraderInfo.TraderData("TEN400", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.EU),
			};
			AssertContainsExactElementsInAnyOrder(expected, preValidateTraderInfo.Traders);

			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.SupplierDocumentaryAddress.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;
			declaration.SupplierDocumentaryAddress.E2_GovRegNum = "TEN110";
			declaration.DispatchWarehouseDocumentaryAddress.E2_AddressOverride = true;
			declaration.DispatchWarehouseDocumentaryAddress.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;
			declaration.DispatchWarehouseDocumentaryAddress.E2_GovRegNum = "TEN330";
			expected = new PreValidateTraderInfo.TraderData[]
			{
				new PreValidateTraderInfo.TraderData("TEN110", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.UK),
				new PreValidateTraderInfo.TraderData("TEN200", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.EU),
				new PreValidateTraderInfo.TraderData("TEN330", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.UK),
				new PreValidateTraderInfo.TraderData("TEN400", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.EU),
			};
			AssertContainsExactElementsInAnyOrder(expected, preValidateTraderInfo.Traders);

			declaration.SupplierDocumentaryAddress.OrganisationPK = TestHelper.GetPartyTraderExciseNumberOrg(Factory, "TEN400", Core.Constants.CountryCodes.UnitedKingdom).PK;
			expected = new PreValidateTraderInfo.TraderData[]
			{
				new PreValidateTraderInfo.TraderData("TEN400", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.UK),
				new PreValidateTraderInfo.TraderData("TEN200", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.EU),
				new PreValidateTraderInfo.TraderData("TEN330", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.UK),
				new PreValidateTraderInfo.TraderData("TEN400", EMCSPreValidateTraderInfoHelper.Constants.TraderTypes.EU),
			};
			AssertContainsExactElementsInAnyOrder(expected, preValidateTraderInfo.Traders);
		}

		public void TestGetProductCodes()
		{
			var expected = new string[] { "AAA", "AAAA", "BBBB", "CCCC" };
			AssertContainsExactElementsInExactOrder(expected, preValidateTraderInfo.ProductCodes);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			declaration.SupplierDocumentaryAddress.OrganisationPK = TestHelper.GetPartyTraderExciseNumberOrg(Factory, "TEN100", Core.Constants.CountryCodes.UnitedKingdom).PK;
			declaration.ImporterDocumentaryAddress.OrganisationPK = TestHelper.GetPartyTraderExciseNumberOrg(Factory, "TEN200", Core.Constants.CountryCodes.Portugal).PK;
			declaration.DispatchWarehouseDocumentaryAddress.OrganisationPK = TestHelper.GetPartyTraderExciseNumberOrg(Factory, "TEN300", Core.Constants.CountryCodes.UnitedKingdom, isWarehouse: true).PK;
			declaration.DestinationWarehouseDocumentaryAddress.OrganisationPK = TestHelper.GetPartyTraderExciseNumberOrg(Factory, "TEN400", Core.Constants.CountryCodes.Germany, isWarehouse: true).PK;

			package = declaration.EMCSPackages.AddNew();
			package.B5_UnitCount = 100;
			package.B5_UnitType = Core.Constants.PkgUnit.Package;

			AddInvoiceLine("AAAA");
			AddInvoiceLine("BBBB");
			AddInvoiceLine("AAAA");
			AddInvoiceLine("AAA");
			AddInvoiceLine("CCCC");

			preValidateTraderInfo = new PreValidateTraderInfo(declaration);
		}

		EMCSJobDeclaration declaration;
		EMCSPackage package;
		PreValidateTraderInfo preValidateTraderInfo;

		void AddInvoiceLine(string exciseCode)
		{
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.ZG_ExciseProductCode = exciseCode;
		}

		void AssertTraderData(PreValidateTraderInfo.TraderData trader, string traderId, string traderType, bool isValid)
		{
			AssertEquals(trader.Key, $"{traderId}{traderType}");
			AssertEquals(trader.TraderID, traderId);
			AssertEquals(trader.TraderType, traderType);
			AssertEquals(trader.IsValid, actual: isValid);
		}
	}
}
