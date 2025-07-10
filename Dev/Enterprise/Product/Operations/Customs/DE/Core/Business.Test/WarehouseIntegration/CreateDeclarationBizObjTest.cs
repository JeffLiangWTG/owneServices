using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing.WarehouseIntegration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CreateDeclarationBizObj))]
	sealed class CreateDeclarationBizObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCPC()
		{
			var resourceStringDataAttribute = createDeclarationBizObj.CPCInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Default Value", "40", createDeclarationBizObj.CPC);
				AssertEquals("MaxLength", 5, createDeclarationBizObj.CPCInfo.MaxLength);
				AssertEquals("Caption", "Customs Procedure Code", resourceStringDataAttribute.Caption);
				AssertEquals("Short Caption", "CPC", resourceStringDataAttribute.ShortCaption);
			});
		}

		public void TestDeclarationType()
		{
			var resourceStringDataAttribute = createDeclarationBizObj.DeclarationTypeInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Default Value", "EZA", createDeclarationBizObj.DeclarationType);
				AssertEquals("MaxLength", 7, createDeclarationBizObj.DeclarationTypeInfo.MaxLength);
				AssertEquals("Caption", "Declaration Type", resourceStringDataAttribute.Caption);
			});
		}

		public void TestCustomsOffice()
		{
			var resourceStringDataAttribute = createDeclarationBizObj.CustomsOfficeInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("MaxLength", 10, createDeclarationBizObj.CustomsOfficeInfo.MaxLength);
				AssertEquals("Caption", "Customs Office", resourceStringDataAttribute.Caption);
			});
		}

		public void TestDeclarantsReference()
		{
			var resourceStringDataAttribute = createDeclarationBizObj.DeclarantsReferenceInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("MaxLength", 19, createDeclarationBizObj.DeclarantsReferenceInfo.MaxLength);
				AssertEquals("Caption", "Declarant's Reference", resourceStringDataAttribute.Caption);
			});
		}

		public void TestCanCreateDeclarations()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Message Errors present as no value entered for DeclarantsReference and CustomsOffice", false, createDeclarationBizObj.CanCreateDeclarations);

				createDeclarationBizObj.DeclarantsReference = "REF123";
				createDeclarationBizObj.CustomsOffice = "DE004323";

				AssertEquals("Valid values entered for DeclarantsReference and CustomsOffice", true, createDeclarationBizObj.CanCreateDeclarations);
			});
		}

		public void TestCreateDeclarationsForWarehouseOrderWithSinglePickLine()
		{
			var bizObj = GetBizObjForDeclarationCreationFromWhsOrder(); 
			var (order, _) = BondedWarehousingHelperTest.CreateOrderWithPick(Factory, createMultiplePickLines: false);

			var errorMsg = bizObj.CreateDeclarationsForWarehouseOrder(order);
			AssertEquals("No error message, declarations will be created", string.Empty, errorMsg);
		}

		public void TestCreateDeclarationsForWarehouseOrderWithMultiplePickLines()
		{
			var bizObj = GetBizObjForDeclarationCreationFromWhsOrder();
			var (order, _) = BondedWarehousingHelperTest.CreateOrderWithPick(Factory, createMultiplePickLines: true);

			var errorMsg = bizObj.CreateDeclarationsForWarehouseOrder(order);
			AssertEquals($"You cannot import order {order.WD_ExternalReference} with multiple picks on order line 1", errorMsg);
		}

		public void TestCreateDeclarationsForWarehouseOrder()
		{
			var bizObj = GetBizObjForDeclarationCreationFromWhsOrder();
			var (order, _) = BondedWarehousingHelperTest.CreateOrderWithPick(Factory, createMultiplePickLines: false);

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, bizObj.DeclarantsReference);
				var errorMsg = bizObj.CreateDeclarationsForWarehouseOrder(order);

				AssertEquals("No error msg - Declaration(s) were successfully created", ZString.Empty, errorMsg);
				AssertSame("Selected order should be stored", order, bizObj.SelectedWhsOrder);
				AssertNotNullOrEmpty(order.WD_ExternalReference);
				AssertEquals("If empty, Declarant's Reference is defaulted with order.WD_ExternalReference", order.WD_ExternalReference, bizObj.DeclarantsReference);

				var declarations = Factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MessageType, EUJobMessageTypeList.Codes.Import));
				var declaration =
					declarations.SingleOrDefault(d => d.JE_HouseBill == $"{order.WD_ExternalReference}_01");
				AssertNotNull(declaration);
				var invoiceLine = declaration.InvoiceLines[0];
				AssertEquals("JI_BondedWhsQuantity should be populated on the Invoice Line of the created Declaration", 1.0m, invoiceLine.JI_BondedWhsQuantity);
			});
		}

		public void TestCreateDeclarationsForWarehouseOrder_OriginalInventory()
		{
			var bizObj = GetBizObjForDeclarationCreationFromWhsOrder();
			var helper = WhsDataTestHelper.New(Factory);
			helper.WhsHelper.SetUpBondedWarehouse(helper.WhsWarehouse.PK, helper.WhsWarehouse.WarehouseAddress.PK);

			var receivePk = helper.WhsHelper.CreateWhsReceive(helper.Importer.PK, helper.WhsWarehouse.PK, "REF1", null);
			var view = helper.WhsHelper.CreateWhsReceiveInventoryLine(receivePk, helper.Part.PK, 1.0m, 1m, 1.0m, ZString.Empty, ZString.Empty, ZDateTimeOffset.Today, "ENTRY1", ZString.Empty);

			var row = helper.WhsWarehouse.Rows.Cast<IWhsRow>().FirstOrDefault(r => r.Locations.ToArray().Cast<IWhsLocation>().Any(l => l.WLV_PickingAreaType == "BON"));
			var location = row.Locations.ToArray().Cast<IWhsLocation>().First(x => x.WLV_PickingAreaType == "BON");
			view.WI_WL = location.PK;

			Factory.Save();

			helper.WhsHelper.FinaliseDocketWithoutUserConfirmation(receivePk);
			Factory.Save();

			var order = (IWhsOrder)helper.WhsHelper.CreateWhsOrder(helper.Importer.PK, helper.WhsWarehouse.PK, helper.Importer.PK, "REF3");
			order.WD_DocketSubType = "CUS";

			var docketLinePk = helper.GetNewWhsOrderLine(order, helper.Part, 1);
			var attr = helper.GetNewWhsBondedWarehouseAttribute(docketLinePk.PK, 1, 1, "KG", ZString.Empty, ZDecimal.Zero, "", "", "EN00123", (ZShort)1);
			attr.WB_OutwardType = "TOF";
			Factory.Save();
			var pickPK = helper.WhsHelper.CreateWhsPick(new[] { order.PK });
			var pick = Factory.Load<IWhsPick>(pickPK);
			pick.WP_FinalizedDateUtc = ZDateTime.Now;
			var pickLines = helper.WhsHelper.GetPickLines(pickPK);

			helper.WhsHelper.PickAndMakeInTransitTransfer(pickLines.First(), ZDateTime.Now, true);

			helper.WhsHelper.FinaliseDocketWithoutUserConfirmation(order.PK);
			helper.WhsHelper.FinalisePick(pickPK);

			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, bizObj.DeclarantsReference);
				var errorMsg = bizObj.CreateDeclarationsForWarehouseOrder(order);

				AssertEquals("No error msg - Declaration(s) were successfully created", ZString.Empty, errorMsg);
				AssertSame("Selected order should be stored", order, bizObj.SelectedWhsOrder);
				AssertNotNullOrEmpty(order.WD_ExternalReference);
				AssertEquals("If empty, Declarant's Reference is defaulted with order.WD_ExternalReference", order.WD_ExternalReference, bizObj.DeclarantsReference);

				var declarations = Factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MessageType, EUJobMessageTypeList.Codes.Import));
				var declaration = declarations.Single(d => d.JE_HouseBill == $"{order.WD_ExternalReference}_01");
				var invoiceLine = declaration.InvoiceLines[0];
				AssertEquals("JI_BondedWhsQuantity should be populated on the Invoice Line of the created Declaration", 1.0m, invoiceLine.JI_BondedWhsQuantity);
			});
		}

		public void TestCreateDeclarationsForWarehouseOrder_DeclarantsRefNotDefaultedIfNotEmpty()
		{
			var bizObj = GetBizObjForDeclarationCreationFromWhsOrder();
			var (order, _) = BondedWarehousingHelperTest.CreateOrderWithPick(Factory, createMultiplePickLines: false);

			bizObj.DeclarantsReference = "ABC123";
			bizObj.CreateDeclarationsForWarehouseOrder(order);

			AssertEquals("ABC123", bizObj.DeclarantsReference);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CreateDeclarationBizObj(createFromWarehouseOrder: false);
		}

		protected override void SetUp()
		{
			base.SetUp();

			createDeclarationBizObj = new CreateDeclarationBizObj(createFromWarehouseOrder: false);

			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", "GERMAN OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);

			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "40", "78", "", "DES2", "IMP", group: "EZL");

			Factory.Save();
		}

		CreateDeclarationBizObj createDeclarationBizObj;

		CreateDeclarationBizObj GetBizObjForDeclarationCreationFromWhsOrder() => new CreateDeclarationBizObj(createFromWarehouseOrder: true) { CustomsOffice = "DE004323" };
	}
}
