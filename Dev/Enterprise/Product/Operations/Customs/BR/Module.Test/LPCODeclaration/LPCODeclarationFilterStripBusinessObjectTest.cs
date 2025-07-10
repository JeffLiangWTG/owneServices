using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(LPCODeclarationFilterStripBusinessObject))]
	class LPCODeclarationFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilterForMessageType()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var dec5 = Factory.New<JobDeclaration>();
			dec5.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			Factory.Save();

			Assert(dec1.MatchesFilter(filterBO.Filter));
			Assert(!dec2.MatchesFilter(filterBO.Filter));
			Assert(!dec3.MatchesFilter(filterBO.Filter));
			Assert(!dec4.MatchesFilter(filterBO.Filter));
			Assert(!dec5.MatchesFilter(filterBO.Filter));
		}

		public void TestFiltersDescription()
		{
			CombineAssertions(() =>
			{
				AssertEquals(LPCODeclarationFilterStripBusinessObject.LPCONumberText, filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.NumberFilterTypes.EntryNumber].MultilingualDescription);
				AssertEquals(LPCODeclarationFilterStripBusinessObject.LPCOStatusText, filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.EntryStatusText].MultilingualDescription);
			});
		}

		public void TestAvailableFilters()
		{
			CombineAssertions(() =>
			{
				AssertNull(filterBO[Enterprise.Customs.GUI.InvoiceLineFilterConstants.ContainerNumber]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.NumberFilterTypes.HouseBill]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.NumberFilterTypes.MasterBill]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.NumberFilterTypes.PaymentNumber]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.NumberFilterTypes.PaymentAmount]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.DateFilterTypes.DateOfExport]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.DateFilterTypes.EstimatedTimeOfArrival]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.DateFilterTypes.ETAOfDischarge]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.DateFilterTypes.ETDOfLoading]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.PortFilterTypes.LoadDischarge]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.PortFilterTypes.PortOfFirstArrival]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.PortFilterTypes.OriginDestination]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.FlightVoyageVessel]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.ServiceLevel]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.ServiceType]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.ShipmentSubType]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.OrgFilterTypes.ShippingLineForwarder]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.OrgFilterTypes.PickupTransportCompany]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.OrgFilterTypes.CartageCoordinator]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.OrgFilterTypes.DeliveryTransportCompany]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.RelatedTransportBookings]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.RelatedContainers]);
				AssertNotNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.NumberFilterTypes.EntryNumber]);
				AssertNotNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.EntryStatusText]);
				AssertNotNull(filterBO[DeclarationFilterConstants.ShipmentExpiryDate]);
			});
		}

		[TestDate(2023, 1, 1)]
		public void TestValidityILShipmentDate()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			var header1 = declaration1.CustomsEntryHeaders.AddNew();
			header1.EntryNumber = "TST1";
			header1.CH_ValidityILShipmentDate = ZDateTime.Today;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			var header2 = declaration2.CustomsEntryHeaders.AddNew();
			header2.EntryNumber = "TST2";
			header2.CH_ValidityILShipmentDate = ZDateTime.Today.AddDays(-45);

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			var header3 = declaration3.CustomsEntryHeaders.AddNew();
			header3.EntryNumber = "TST3";
			header3.CH_ValidityILShipmentDate = ZDateTime.Today.AddDays(-15);

			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration4.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			var header4 = declaration4.CustomsEntryHeaders.AddNew();
			header4.EntryNumber = "TST4";
			header4.CH_ValidityILShipmentDate = ZDateTime.Empty;

			Factory.Save();

			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-50), ZDateTime.Today.AddDays(-40), new JobDeclaration[] { declaration2 }, DeclarationFilterConstants.ShipmentExpiryDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), new JobDeclaration[] { declaration1 }, DeclarationFilterConstants.ShipmentExpiryDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-20), ZDateTime.Empty, new JobDeclaration[] { declaration1, declaration3 }, DeclarationFilterConstants.ShipmentExpiryDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.HasDateEntered, ZDateTime.Empty, ZDateTime.Empty, new JobDeclaration[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.ShipmentExpiryDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.HasNoDateEntered, ZDateTime.Empty, ZDateTime.Empty, new JobDeclaration[] { declaration4 }, DeclarationFilterConstants.ShipmentExpiryDate);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new LPCODeclarationFilterStripBusinessObject();

		LPCODeclarationFilterStripBusinessObject filterBO;

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = (LPCODeclarationFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
