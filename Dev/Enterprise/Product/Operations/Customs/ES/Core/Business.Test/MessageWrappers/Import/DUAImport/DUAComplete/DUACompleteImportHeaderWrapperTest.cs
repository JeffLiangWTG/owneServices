using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DUACompleteImportHeaderWrapperTest : DUAImportCommonHeaderWrapperTest
	{
		public void TestNullExporter()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Exporter.ToString());
		}

		public void TestExporter()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = HeaderData.ExporterCode;
				orgHeader.Addresses.AddNew();
				invoiceHeader.JZ_OH_Supplier = orgHeader.PK;

				var exporter = wrapper.Exporter;

				AssertNotNull("Expected filled Exporter", exporter);
				AssertSame("Cached Exporter", wrapper.Exporter, exporter);
			});
		}

		public void TestDestinationCountry()
		{
			declaration.JE_GoodsDestination = HeaderData.CountryOfDestination;
			AssertEquals("Expected filled DestinationCountry", HeaderData.CountryOfDestination, wrapper.DestinationCountry);
		}

		public void TestDestinationState()
		{
			declaration.ZG_DestinationState = HeaderData.StateOfDestination;
			AssertEquals("Expected filled DestinationCountry", HeaderData.StateOfDestination, wrapper.DestinationState);
		}

		public void TestArrivalTransportId()
		{
			declaration.ZG_Box18TransportID = HeaderData.TransportModeId;
			AssertEquals("Expected filled ArrivalTransportId", HeaderData.TransportModeId, wrapper.ArrivalTransportId);
		}

		public void TestDeliveryConditions()
		{
			declaration.JE_ShipmentIncoTerm = HeaderData.TermsOfDeliveryDeclarationCode;
			var deliveryConditions = wrapper.DeliveryConditions;

			CombineAssertions(() =>
			{
				AssertEquals("DeliveryConditions wrapped", HeaderData.TermsOfDeliveryDeclarationCode, deliveryConditions.Code);
				AssertSame("Cached DeliveryConditions", wrapper.DeliveryConditions, deliveryConditions);
			});
		}

		public void TestFrontierTransportCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "MQ", "FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);

			Factory.Save();

			CombineAssertions(() =>
			{
				declaration.JE_RN_NKTransportNationality = "RS";
				AssertEquals("Expected filled FrontierTransportCountry with Default Territory (XS)", "XS", wrapper.FrontierTransportCountry);

				declaration.JE_RN_NKTransportNationality = "ES";
				AssertEquals("Expected filled FrontierTransportCountry with given code since there is no Default Territory", "ES", wrapper.FrontierTransportCountry);

				declaration.JE_RN_NKTransportNationality = "MQ";
				AssertEquals("Expected filled FrontierTransportCountry with Default Territory (FR)", "FR", wrapper.FrontierTransportCountry);
			});
		}

		public void TestInvoiceAmount()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_LinePrice = HeaderData.TotalAmount / 2;
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = HeaderData.TotalAmount / 2;

				var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge", true, mergeResult);

				entryHeader = declaration.CustomsEntryHeaders[0];

				AssertEquals("Expected filled InvoiceAmount", HeaderData.TotalAmount, wrapper.InvoiceAmount);
			});
		}

		public void TestTransactionNature()
		{
			invoiceHeader.JZ_ValuationCode = HeaderData.ValuationCode;
			AssertEquals("Expected filled TransactionNature", HeaderData.ValuationCode, wrapper.TransactionNature);
		}

		public void TestFrontierTransportMode()
		{
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = BorderTransportAir.Mode;
				AssertEquals("Expected filled FrontierTransportMode Air (4)", BorderTransportAir.ModeCoded, wrapper.FrontierTransportMode);

				declaration.JE_TransportMode = BorderTransportRoad.Mode;
				AssertEquals("Expected filled FrontierTransportMode Road (3)", BorderTransportRoad.ModeCoded, wrapper.FrontierTransportMode);
			});
		}

		public void TestInteriorTransportMode()
		{
			CombineAssertions(() =>
			{
				declaration.JE_TransportModeInland = BorderTransportAir.Mode;
				AssertEquals("Expected filled InteriorTransportMode Air (4)", BorderTransportAir.ModeCoded, wrapper.InteriorTransportMode);

				declaration.JE_TransportModeInland = BorderTransportRoad.Mode;
				AssertEquals("Expected filled InteriorTransportMode Road (3)", BorderTransportRoad.ModeCoded, wrapper.InteriorTransportMode);
			});
		}

		public void TestCustomsOfficeOfEntry()
		{
			CombineAssertions(() =>
			{
				var officeOfExit = declaration.CustomsOffices.AddNew();
				officeOfExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent;
				officeOfExit.CY_Data = HeaderData.CustomsOfficeOfEntry;

				AssertEquals("Expected filled CustomsOfficeOfEntry", HeaderData.CustomsOfficeOfEntry, wrapper.CustomsOfficeOfEntry);

				officeOfExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDelivery;
				officeOfExit.CY_Data = HeaderData.CustomsOfficeOfEntry;

				AssertEquals("Expected empty CustomsOfficeOfEntry when office code is not ENT", ZString.Empty, wrapper.CustomsOfficeOfEntry);

				declaration.CustomsOffices.RemoveAndDeleteAll();

				AssertEquals("Expected empty CustomsOfficeOfEntry when no offices are declared", ZString.Empty, wrapper.CustomsOfficeOfEntry);
			});
		}

		public void TestDepositId()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DepositId", ZString.Empty, wrapper.DepositId);

				var warehouse1 = Factory.New<OrgHeader>();
				var warehouseAddress1 = warehouse1.Addresses.AddNew();
				var cusCode1 = warehouse1.CustomsCodes.AddNew();
				cusCode1.OK_CustomsRegNo = HeaderData.FromWarehouseId;
				cusCode1.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.Code;
				cusCode1.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
				cusCode1.OK_OA_PremisesAddress = warehouseAddress1.PK;
				entryInstruction.CEI_OA_Warehouse = warehouseAddress1.PK;

				AssertEquals("Expected filled DepositId with FromWarehouseCode", HeaderData.FromWarehouseId, wrapper.DepositId);

				var warehouse2 = Factory.New<OrgHeader>();
				var warehouseAddress2 = warehouse2.Addresses.AddNew();
				var cusCode2 = warehouse2.CustomsCodes.AddNew();
				cusCode2.OK_CustomsRegNo = HeaderData.ToWarehouseId;
				cusCode2.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.Code;
				cusCode2.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
				cusCode2.OK_OA_PremisesAddress = warehouseAddress2.PK;
				entryInstruction.CEI_OA_Warehouse2 = warehouseAddress2.PK;

				AssertEquals("Expected filled DepositId with ToWarehouseCode (even when having fromwarehousecode)", HeaderData.ToWarehouseId, wrapper.DepositId);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapper = (DUACompleteImportHeaderWrapper)GetWrapper(entryHeader);
		}
		DUACompleteImportHeaderWrapper wrapper;

		protected override DUAImportCommonHeaderWrapper GetWrapper(CusEntryHeader cusEntryHeader) => new DUACompleteImportHeaderWrapper(cusEntryHeader);

		protected override DUAImportCommonHeaderWrapper GetProvider() => wrapper;
	}
}
