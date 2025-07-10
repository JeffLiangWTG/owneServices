using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Moq.Protected;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.FR.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageWrapperHeader))]
	public class TemporaryStorageWrapperHeaderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new TemporaryStorageWrapperHeader();

		public void TestNewFromShipment()
		{
			var shipment = Factory.NewMoq<ForwardingShipment>();
			var org = Factory.New<OrgHeader>();

			var consol = shipment.Object.Consols.AddNew();
			consol.MasterBillAirlinePrefix = "A";
			consol.MasterBillMAWB = "12345";
			var transport = consol.MostInterestingTransportForBinding[0];
			transport.RefVessels[0].RV_Code = "Vessel";
			transport.JW_VoyageFlightForBinding = "BA1234";
			transport.JW_ATDForBinding = ZDateTime.BrettsBirthday;
			shipment.Setup(m => m.JS_RL_NKDestination).Returns("FRCDG");
			shipment.Setup(m => m.ImportBroker).Returns(org);
			shipment.Setup(m => m.JS_RL_NKOrigin).Returns("GBLHR");
			shipment.Setup(m => m.JS_TransportMode).Returns("AIR");
			shipment.Setup(m => m.JS_E_ARV).Returns(ZDateTime.BrettsBirthday);
			shipment.Setup(m => m.JS_ActualWeight).Returns(1.11);
			shipment.Setup(m => m.JS_UnitOfWeight).Returns("KG");
			shipment.Setup(m => m.JS_OuterPacks).Returns(5);
			shipment.Setup(m => m.ConsigneeDocumentaryAddress).Returns(documentaryAddress);
			shipment.Protected().Setup<JobDocAddress>("GetNewConsignorPickupAddress").Returns(pickupAddress);
			shipment.Setup(m => m.JS_UniqueConsignRef).Returns("123");

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = RefContainer.New(Factory).PK;
			container1.RefContainer.RC_Code = "T10";
			container1.JC_ContainerNum = "CNT10";
			container1.JC_ContainerCount = 6;
			var container2 = consol.Containers.AddNew();
			container2.JC_RC = RefContainer.New(Factory).PK;
			container2.RefContainer.RC_Code = "T20";
			container2.JC_ContainerNum = "CNT20";
			container2.JC_ContainerCount = 7;

			var pivot = shipment.Object.OuterPackLines.AddNew();
			pivot.JL_JC = container1.PK;
			pivot.JL_PackageCount = 69;
			pivot.JL_Description = "Quite a long description";
			pivot.JL_HarmonisedCode = "99887766";
			pivot.JL_RN_NKOrigin = "US";
			pivot.JL_OutturnedWeight = 2.2;
			pivot.JL_LinePrice = 123.45;
			pivot.JL_ActualWeightUQ = "LB";

			var temporaryStorageHeader = TemporaryStorageWrapperHeader.NewFromShipment(shipment.Object);

			CombineAssertions(() =>
			{
				AssertEquals(org.MainAddress.PK, temporaryStorageHeader.PresenterPK);
				AssertEquals(ZDateTime.BrettsBirthday, temporaryStorageHeader.DepartureDate);
				AssertEquals(ZDateTime.BrettsBirthday, temporaryStorageHeader.ArrivalDate);
				AssertEquals("Vessel", temporaryStorageHeader.Vessel);
				AssertEquals("BA1234", temporaryStorageHeader.TransportID);
				AssertEquals("AIR", temporaryStorageHeader.TransportMode);
				AssertEquals("GBLHR", temporaryStorageHeader.PlaceOfLoading);
				AssertEquals("123", temporaryStorageHeader.JobNumber);
				AssertEquals(2, temporaryStorageHeader.ContainerCount);
				AssertEquals("We don't set previous ref type & number when import from shipment.", "", temporaryStorageHeader.PreviousEntryType);
				AssertEquals("We don't set previous ref type & number when import from shipment.", "", temporaryStorageHeader.PreviousEntryNumber);
				AssertEquals("AWB", temporaryStorageHeader.TemporaryStorageLines[0].OwnerReferenceType);
				AssertEquals("A12345", temporaryStorageHeader.TemporaryStorageLines[0].OwnerReferenceNumber);
				AssertEquals((ZDecimal)1.11, temporaryStorageHeader.TemporaryStorageLines[0].GrossWeight);
				AssertEquals("KG", temporaryStorageHeader.TemporaryStorageLines[0].GrossWeightUQ);
				AssertEquals(5, temporaryStorageHeader.TemporaryStorageLines[0].PackageQty);
				AssertEquals("Quite a long description", temporaryStorageHeader.TemporaryStorageLines[0].GoodsDescription);
				AssertEquals(2, temporaryStorageHeader.TemporaryStorageContainers.Count);
				AssertEquals("T10", temporaryStorageHeader.TemporaryStorageContainers[0].ContainerType);
				AssertEquals("CNT10", temporaryStorageHeader.TemporaryStorageContainers[0].ContainerNumber);
				AssertEquals("T20", temporaryStorageHeader.TemporaryStorageContainers[1].ContainerType);
				AssertEquals("CNT20", temporaryStorageHeader.TemporaryStorageContainers[1].ContainerNumber);
				AssertEquals("99887766", temporaryStorageHeader.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].CommodityCode);
				AssertEquals("US", temporaryStorageHeader.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].OriginCountry);
				AssertEquals((ZDecimal)2.2, temporaryStorageHeader.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].NetMass);
				AssertEquals("LB", temporaryStorageHeader.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].NetMassUQ);
				AssertEquals((ZDecimal)123.45, temporaryStorageHeader.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].GoodsValue);
			});
			shipment.VerifyAll();
		}

		public void TestNewFromDeclaration()
		{
			var importerOrgHeader = Factory.New<OrgHeader>();
			importerOrgHeader.OH_Code = "IM1";
			var importerMainAddress = importerOrgHeader.MainAddress;
			importerOrgHeader.MainAddress.OA_Address1 = "ImporterAddress1";

			var exporterOrgHeader = Factory.New<OrgHeader>();
			exporterOrgHeader.OH_Code = "EX1";
			var exporterMainAddress = exporterOrgHeader.MainAddress;
			exporterOrgHeader.MainAddress.OA_Address1 = "ExporterAddress1";

			var declarantOrgHeader = Factory.New<OrgHeader>();
			declarantOrgHeader.OH_Code = "NWG";
			declarantOrgHeader.OH_FullName = "DeclarantName";
			var declarantMainAddress = declarantOrgHeader.MainAddress;
			declarantOrgHeader.MainAddress.OA_Address1 = "DeclarantAddress";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.SetupDeclarant(declarantOrgHeader.MainAddress);
			dec.WithFlux(EU.Business.MessageTypeList.Codes.Import);
			dec.JE_CustomsProfile = "DGI002";
			dec.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;

			var entryInstruction1 = dec.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "10P";

			dec.JE_EntryStyle = "FR";
			dec.JE_OA_DeclarantAddress = declarantOrgHeader.MainAddress.PK;
			dec.JE_OH_Importer = importerOrgHeader.PK;
			dec.JE_OH_Exporter = exporterOrgHeader.PK;
			dec.JE_TransportMode = "AIR";
			dec.JE_VoyageFlightNo = "BA123";
			dec.JE_VesselName = "BIGSHIP";
			dec.JE_RL_NKPortOfLoading = "GBLHR";
			dec.JE_ExportDate = ZDateTime.BrettsBirthday;
			dec.JE_DateOfArrival = ZDateTime.BrettsBirthday.AddDays(1);
			dec.JE_DeclarationReference = "REF123";
			dec.DeclarationNumber = "DEC123";
			dec.JE_TotalNoOfPacks = 11;
			dec.JE_LocationOfGoods = "LOC123";
			dec.JE_MasterBill = "MAWB123";

			var package1 = (EU.Business.Declaration.Package)dec.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackType = "AA";
			var package2 = (EU.Business.Declaration.Package)dec.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package2.CW_PackType = "BB";
			var package3 = (EU.Business.Declaration.Package)dec.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package3.CW_PackType = "CC";
			var package4 = (EU.Business.Declaration.Package)dec.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package4.CW_PackType = "DD";

			var invoice1 = dec.Invoices.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CEI = entryInstruction1.PK;
			line1.JI_Procedure = "51Im123";
			line1.JI_Weight = 1.11d;
			line1.JI_WeightUQ = "KG";
			line1.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;
			line1.JI_Description = "GoodsDescription1";
			line1.JI_FormattedTariff = "111.111.111";
			line1.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			line1.JI_NetWeight = 1.1d;
			line1.JI_NetWeightUQ = "KG";
			line1.JI_LinePrice = 111.1d;

			var office1 = dec.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == "CAU") ?? dec.CustomsOffices.AddNew();
			office1.CY_Code = "CAU";
			office1.CY_Data = "999";
			var office2 = dec.CustomsOffices.AddNew();
			office2.CY_Code = "ENT";
			office2.CY_Data = "555";

			var container1 = dec.CusContainers.AddNew().JobContainer;
			container1.JC_RC = RefContainer.New(Factory).PK;
			container1.RefContainer.RC_Code = "T10";
			container1.JC_ContainerNum = "CNT10";
			var container2 = dec.CusContainers.AddNew().JobContainer;
			container2.JC_RC = RefContainer.New(Factory).PK;
			container2.RefContainer.RC_Code = "T20";
			container2.JC_ContainerNum = "CNT20";

			var entry1 = dec.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "ENT001";
			entry1.CH_CEI_Instruction = entryInstruction1.PK;

			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.A00).CF_ChargeAmount = 11.11m;
			entryLine1.InvoiceLines.Add(line1);

			var entryLine2 = entry1.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.A00).CF_ChargeAmount = 22.22m;
			var line2 = invoice1.JobComInvoiceLines.AddNew();
			line2.JI_CEI = entryInstruction1.PK;
			line2.JI_Procedure = "51Im123";
			line2.JI_Weight = 2.22d;
			line2.JI_WeightUQ = "KG";
			line2.PackagesForInvoiceLinesForBindingOnly[2].IsLinked = true;
			line2.JI_Description = "GoodsDescription2";
			line2.JI_FormattedTariff = "222.222.222";
			line2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			line2.JI_NetWeight = 2.2d;
			line2.JI_NetWeightUQ = "KG";
			line2.JI_LinePrice = 222.2d;
			entryLine2.InvoiceLines.Add(line2);

			var entry2 = dec.CustomsEntryHeaders.AddNew();
			entry2.EntryNumber = "ENT002";
			entry2.CH_CEI_Instruction = entryInstruction1.PK;

			var entryLine3 = entry2.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			entryLine3.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.A00).CF_ChargeAmount = 33.33m;
			var line3 = invoice1.JobComInvoiceLines.AddNew();
			line3.JI_CEI = entryInstruction1.PK;
			line3.JI_Procedure = "51Im123";
			line3.JI_Weight = 3.33d;
			line3.JI_WeightUQ = "KG";
			line3.PackagesForInvoiceLinesForBindingOnly[3].IsLinked = true;
			line3.JI_Description = "GoodsDescription3";
			line3.JI_FormattedTariff = "333.333.333";
			line3.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			line3.JI_NetWeight = 3.3d;
			line3.JI_NetWeightUQ = "KG";
			line3.JI_LinePrice = 333.3d;
			entryLine3.InvoiceLines.Add(line3);

			var entryLine4 = entry2.MergedLines.AddNew();
			entryLine4.CL_LineNumber = 4;
			entryLine4.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.A00).CF_ChargeAmount = 44.44m;
			var line4 = invoice1.JobComInvoiceLines.AddNew();
			line4.JI_CEI = entryInstruction1.PK;
			line4.JI_Procedure = "51Im123";
			line4.JI_Weight = 4.44d;
			line4.JI_WeightUQ = "KG";
			line4.PackagesForInvoiceLinesForBindingOnly[4].IsLinked = true;
			line4.JI_Description = "GoodsDescription4";
			line4.JI_FormattedTariff = "444.444.444";
			line4.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			line4.JI_NetWeight = 4.4d;
			line4.JI_NetWeightUQ = "KG";
			line4.JI_LinePrice = 444.4d;
			entryLine4.InvoiceLines.Add(line4);

			if (dec.SupportingDocuments.Count > 0)
			{
				dec.SupportingDocuments[0].CSI_DateOfIssue = ZDateTime.Today;
			}

			var supportingDoc1 = dec.SupportingDocuments.AddNew();
			supportingDoc1.CSI_Code = "CSI01";
			var supportingDoc2 = invoice1.SupportingDocuments.AddNew();
			supportingDoc2.CSI_Code = "CSI02";
			var supportingDoc3 = line1.SupportingDocuments.AddNew();
			supportingDoc3.CSI_Code = "CSI03";
			var supportingDoc4 = line1.SupportingDocuments.AddNew();
			supportingDoc4.CSI_Code = "CSI04";

			var temporaryStorageHeader = TemporaryStorageWrapperHeader.NewFromDeclaration(dec);

			CombineAssertions(() =>
			{
				AssertEquals(importerOrgHeader.PK, temporaryStorageHeader.CustomerPK);
				AssertEquals("BA123", temporaryStorageHeader.TransportID);
				AssertEquals("AIR", temporaryStorageHeader.TransportMode);
				AssertEquals("GBLHR", temporaryStorageHeader.PlaceOfLoading);
				AssertEquals(ZDateTime.BrettsBirthday, temporaryStorageHeader.DepartureDate);
				AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), temporaryStorageHeader.ArrivalDate);
				AssertEquals("REF123", temporaryStorageHeader.JobNumber);
				AssertEquals("FR", temporaryStorageHeader.PreviousEntryType);
				AssertEquals("ENT001,ENT002", temporaryStorageHeader.PreviousEntryNumber.ToString());
				AssertEquals("999", temporaryStorageHeader.CustomsOfficeOfDestination);
				AssertEquals("555", temporaryStorageHeader.CustomsOfficeOfEntry);
				AssertEquals("Presenter address", declarantMainAddress.PK, temporaryStorageHeader.PresenterPK);
				AssertEquals("BA123", temporaryStorageHeader.TransportMeansDescription);
				AssertEquals(2, temporaryStorageHeader.ContainerCount);

				AssertEquals(2, temporaryStorageHeader.TemporaryStorageContainers.Count);
				var tempStorageContainer1 = temporaryStorageHeader.TemporaryStorageContainers[0];
				AssertEquals("T10", tempStorageContainer1.ContainerType);
				AssertEquals("CNT10", tempStorageContainer1.ContainerNumber);
				var tempStorageContainer2 = temporaryStorageHeader.TemporaryStorageContainers[1];
				AssertEquals("T20", tempStorageContainer2.ContainerType);
				AssertEquals("CNT20", tempStorageContainer2.ContainerNumber);
				AssertContainsExactElementsInAnyOrder(new CusSupportingInfo[] { supportingDoc1, supportingDoc2, supportingDoc3, supportingDoc4 }, temporaryStorageHeader.SupportingDocuments);

				AssertEquals(4, temporaryStorageHeader.TemporaryStorageLines.Count);

				var temporaryStorageLine1 = temporaryStorageHeader.TemporaryStorageLines[0];
				AssertEquals("AWB", temporaryStorageLine1.OwnerReferenceType);
				AssertEquals("MAWB123", temporaryStorageLine1.OwnerReferenceNumber);
				AssertZDecimalEquals("", 1.11, temporaryStorageLine1.GrossWeight);
				AssertEquals("KG", temporaryStorageLine1.GrossWeightUQ);
				AssertEquals(11, temporaryStorageLine1.PackageQty);
				AssertEquals("LOC123", temporaryStorageLine1.LocationOfGoods);
				AssertEquals("AA", temporaryStorageLine1.PackageType);
				AssertEquals("GoodsDescription1", temporaryStorageLine1.GoodsDescription);
				AssertEquals("1111.11.11 1", temporaryStorageLine1.TemporaryStorageFurtherDetails[0].CommodityCode);
				AssertEquals("US", temporaryStorageLine1.TemporaryStorageFurtherDetails[0].OriginCountry);
				AssertZDecimalEquals("", 1.1, temporaryStorageLine1.TemporaryStorageFurtherDetails[0].NetMass);
				AssertEquals("KG", temporaryStorageLine1.TemporaryStorageFurtherDetails[0].NetMassUQ);

				var temporaryStorageLine2 = temporaryStorageHeader.TemporaryStorageLines[1];
				AssertEquals("AWB", temporaryStorageLine2.OwnerReferenceType);
				AssertEquals("MAWB123", temporaryStorageLine2.OwnerReferenceNumber);
				AssertZDecimalEquals("", 2.22, temporaryStorageLine2.GrossWeight);
				AssertEquals("KG", temporaryStorageLine2.GrossWeightUQ);
				AssertEquals(11, temporaryStorageLine2.PackageQty);
				AssertEquals("LOC123", temporaryStorageLine2.LocationOfGoods);
				AssertEquals("BB", temporaryStorageLine2.PackageType);
				AssertEquals("GoodsDescription2", temporaryStorageLine2.GoodsDescription);
				AssertEquals("2222.22.22 2", temporaryStorageLine2.TemporaryStorageFurtherDetails[0].CommodityCode);
				AssertEquals("AU", temporaryStorageLine2.TemporaryStorageFurtherDetails[0].OriginCountry);
				AssertZDecimalEquals("", 2.2, temporaryStorageLine2.TemporaryStorageFurtherDetails[0].NetMass);
				AssertEquals("KG", temporaryStorageLine2.TemporaryStorageFurtherDetails[0].NetMassUQ);

				var temporaryStorageLine3 = temporaryStorageHeader.TemporaryStorageLines[2];
				AssertEquals("AWB", temporaryStorageLine3.OwnerReferenceType);
				AssertEquals("MAWB123", temporaryStorageLine3.OwnerReferenceNumber);
				AssertZDecimalEquals("", 3.33, temporaryStorageLine3.GrossWeight);
				AssertEquals("KG", temporaryStorageLine3.GrossWeightUQ);
				AssertEquals(11, temporaryStorageLine3.PackageQty);
				AssertEquals("LOC123", temporaryStorageLine3.LocationOfGoods);
				AssertEquals("CC", temporaryStorageLine3.PackageType);
				AssertEquals("GoodsDescription3", temporaryStorageLine3.GoodsDescription);
				AssertEquals("3333.33.33 3", temporaryStorageLine3.TemporaryStorageFurtherDetails[0].CommodityCode);
				AssertEquals("US", temporaryStorageLine3.TemporaryStorageFurtherDetails[0].OriginCountry);
				AssertZDecimalEquals("", 3.3, temporaryStorageLine3.TemporaryStorageFurtherDetails[0].NetMass);
				AssertEquals("KG", temporaryStorageLine3.TemporaryStorageFurtherDetails[0].NetMassUQ);

				var temporaryStorageLine4 = temporaryStorageHeader.TemporaryStorageLines[3];
				AssertEquals("AWB", temporaryStorageLine4.OwnerReferenceType);
				AssertEquals("MAWB123", temporaryStorageLine4.OwnerReferenceNumber);
				AssertZDecimalEquals("", 4.44, temporaryStorageLine4.GrossWeight);
				AssertEquals("KG", temporaryStorageLine4.GrossWeightUQ);
				AssertEquals(11, temporaryStorageLine4.PackageQty);
				AssertEquals("LOC123", temporaryStorageLine4.LocationOfGoods);
				AssertEquals("DD", temporaryStorageLine4.PackageType);
				AssertEquals("GoodsDescription4", temporaryStorageLine4.GoodsDescription);
				AssertEquals("4444.44.44 4", temporaryStorageLine4.TemporaryStorageFurtherDetails[0].CommodityCode);
				AssertEquals("AU", temporaryStorageLine4.TemporaryStorageFurtherDetails[0].OriginCountry);
				AssertZDecimalEquals("", 4.4, temporaryStorageLine4.TemporaryStorageFurtherDetails[0].NetMass);
				AssertEquals("KG", temporaryStorageLine4.TemporaryStorageFurtherDetails[0].NetMassUQ);
			});
		}

		public void TestNewFromDeclaration_Guarantees()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "GP";
			procedure.ZZ6_IsGuaranteeConsumed = Universal.CodeDescriptionPairLists.YesNoList.Codes.Yes;
			procedure.ZZ6_IsGuaranteeReleased = Universal.CodeDescriptionPairLists.YesNoList.Codes.No;
			procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			procedure.ZZ6_Description = "DESC";
			procedure.ZZ6_ShipmentType = "EXP";
			var cphCurrency = RefCurrency.New(Factory);
			cphCurrency.RX_Code = "UD1";
			cphCurrency.SetCustomsRate(ZDateTime.Today, ZDateTime.MaxSmallDateTimeValue, 2m);
			Factory.Save();

			var declaration = Factory.New<DummyJobDeclaration_TestNewFromDeclaration_Guarantees>();
			var localCurrencyCode = declaration.LocalCurrencyCode;
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, localCurrencyCode);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var entryHeaderMock = Factory.NewMoq<CusEntryHeader>();
			CusEntryHeader entryHeader = entryHeaderMock.Object;
			entryHeader.CH_JE = declaration.PK;
			declaration.ActiveEntryHeaders.Add(entryHeader);

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = localCurrencyCode;
			//new List<int>() { 1, 2, 3 }.ForEach(i =>
			//{
			//	var entryLine = entryHeader.AllEntryLines.AddNew();
			//	var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			//	invoiceLine.JI_CL = entryLine.PK;
			//	invoiceLine.JI_Procedure = i == 2 ? "XX" : "GP";
			//	invoiceLine.JI_LinePrice = i * 100m;
			//});

			var entryLineMock1 = Factory.NewMoq<CusEntryLine>();
			var entryLine1 = entryLineMock1.Object;
			entryLine1.CL_CH = entryHeader.PK;
			entryHeader.AllEntryLines.Add(entryLine1);
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Procedure = "GP";
			invoiceLine1.JI_LinePrice = 100m;

			var entryLineMock2 = Factory.NewMoq<CusEntryLine>();
			var entryLine2 = entryLineMock2.Object;
			entryLine2.CL_CH = entryHeader.PK;
			entryHeader.AllEntryLines.Add(entryLine2);
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "XX";
			invoiceLine2.JI_LinePrice = 200m;

			var entryLineMock3 = Factory.NewMoq<CusEntryLine>();
			var entryLine3 = entryLineMock3.Object;
			entryLine3.CL_CH = entryHeader.PK;
			entryHeader.AllEntryLines.Add(entryLine3);
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_Procedure = "GP";
			invoiceLine3.JI_LinePrice = 300m;

			var guaranteedAmountDetails = new List<AmountAndTypeToBeGuaranteed>()
			{
				new AmountAndTypeToBeGuaranteed() { AmountInDeclarationCurrency = 4000 }
			};
			entryLineMock1.Protected().Setup<IEnumerable<AmountAndTypeToBeGuaranteed>>("AmountAndTypeToBeGuaranteedsCore").Returns(guaranteedAmountDetails);

			var tempHeader1 = TemporaryStorageWrapperHeader.NewFromDeclaration(declaration);
			AssertEquals("JobDeclaration.CustomsGuarantee is null", ZString.Empty, tempHeader1.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].Currency);
			AssertZDecimalEquals("JobDeclaration.CustomsGuarantee is null", 0, tempHeader1.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].GuaranteedValue);
			AssertZDecimalEquals("JobDeclaration.CustomsGuarantee is null", 0, tempHeader1.TemporaryStorageLines[1].TemporaryStorageFurtherDetails[0].GuaranteedValue);
			AssertZDecimalEquals("JobDeclaration.CustomsGuarantee is null", 0, tempHeader1.TemporaryStorageLines[2].TemporaryStorageFurtherDetails[0].GuaranteedValue);

			declaration.CusGuaranteeHeaderForReturn = Factory.New<CusGuaranteeHeader>();
			declaration.CusGuaranteeHeaderForReturn.CPH_UnitOfMeasure = localCurrencyCode;

			var tempHeader2 = TemporaryStorageWrapperHeader.NewFromDeclaration(declaration);
			AssertEquals("Guarantee Currency is EUR", Core.Constants.CurrencyCodes.EuropeanUnion, tempHeader2.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].Currency);
			AssertZDecimalEquals("Guarantee Currency is EUR and consumingProcedure", 1000m, tempHeader2.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].GuaranteedValue);
			AssertZDecimalEquals("Guarantee Currency is EUR and not consumingProcedure", 0, tempHeader2.TemporaryStorageLines[1].TemporaryStorageFurtherDetails[0].GuaranteedValue);
			AssertZDecimalEquals("Guarantee Currency is EUR and consumingProcedure", 3000m, tempHeader2.TemporaryStorageLines[2].TemporaryStorageFurtherDetails[0].GuaranteedValue);

			declaration.CusGuaranteeHeaderForReturn.CPH_UnitOfMeasure = "UD1";

			var tempHeader3 = TemporaryStorageWrapperHeader.NewFromDeclaration(declaration);
			AssertEquals("Guarantee Currency is not EUR", "UD1", tempHeader3.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].Currency);
			AssertZDecimalEquals("Guarantee Currency is not EUR and consumingProcedure", 2000m, tempHeader3.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].GuaranteedValue);
			AssertZDecimalEquals("Guarantee Currency is not EUR and not consumingProcedure", 0, tempHeader3.TemporaryStorageLines[1].TemporaryStorageFurtherDetails[0].GuaranteedValue);
			AssertZDecimalEquals("Guarantee Currency is not EUR and consumingProcedure", 6000m, tempHeader3.TemporaryStorageLines[2].TemporaryStorageFurtherDetails[0].GuaranteedValue);
		}

		public void TestNewFromDeclaration_OwnerReference_AIR()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = "AIR";
			dec.JE_MasterBill = "MAWB123";
			dec.JE_VesselName = "BIGSHIP";
			var entryInstruction1 = dec.CustomsEntryInstructions.AddNew();
			var entry1 = dec.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "ENT001";
			entry1.CH_CEI_Instruction = entryInstruction1.PK;
			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoice1 = dec.Invoices.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CEI = entryInstruction1.PK;
			entryLine1.InvoiceLines.Add(line1);
			var temporaryStorageHeader = TemporaryStorageWrapperHeader.NewFromDeclaration(dec);
			var temporaryStorageLine1 = temporaryStorageHeader.TemporaryStorageLines[0];

			AssertEquals("AWB", temporaryStorageLine1.OwnerReferenceType);
			AssertEquals(dec.JE_MasterBill, temporaryStorageLine1.OwnerReferenceNumber);
		}

		public void TestNewFromDeclaration_OwnerReference_SEA()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = "SEA";
			dec.JE_MasterBill = "MAWB123";
			dec.JE_VesselName = "BIGSHIP";
			var entryInstruction1 = dec.CustomsEntryInstructions.AddNew();
			var entry1 = dec.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "ENT001";
			entry1.CH_CEI_Instruction = entryInstruction1.PK;
			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoice1 = dec.Invoices.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CEI = entryInstruction1.PK;
			entryLine1.InvoiceLines.Add(line1);
			var temporaryStorageHeader = TemporaryStorageWrapperHeader.NewFromDeclaration(dec);
			var temporaryStorageLine1 = temporaryStorageHeader.TemporaryStorageLines[0];

			AssertEquals("MBL", temporaryStorageLine1.OwnerReferenceType);
			AssertEquals(dec.JE_MasterBill, temporaryStorageLine1.OwnerReferenceNumber);
		}

		public void TestNewFromDeclaration_OwnerReference_ROA()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = "ROA";
			dec.JE_MasterBill = "MAWB123";
			dec.JE_VesselName = "BIGSHIP";
			var entryInstruction1 = dec.CustomsEntryInstructions.AddNew();
			var entry1 = dec.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "ENT001";
			entry1.CH_CEI_Instruction = entryInstruction1.PK;
			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoice1 = dec.Invoices.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CEI = entryInstruction1.PK;
			entryLine1.InvoiceLines.Add(line1);
			var temporaryStorageHeader = TemporaryStorageWrapperHeader.NewFromDeclaration(dec);
			var temporaryStorageLine1 = temporaryStorageHeader.TemporaryStorageLines[0];

			AssertEquals("TRK", temporaryStorageLine1.OwnerReferenceType);
			AssertEquals(dec.JE_VesselName, temporaryStorageLine1.OwnerReferenceNumber);
		}

		[TestDate(2021, 09, 26)]
		public void TestNewFromDeltaT()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "C0009");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "FR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			var principal = Factory.NewWithValidTestData<OrgHeader>();

			var permitHeader = Factory.New<CusAuthorisationHeader>();
			permitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			permitHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			permitHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			permitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			permitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			permitHeader.CPH_OH_PermitHolder = declarant.PK;
			permitHeader.CPH_Number = "PH123";

			var rule = permitHeader.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.USE;
			rule.CPR_ValueFrom = AuthorizationRuleUseValueFromList.Codes.IST;

			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guarantee.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			guarantee.CPH_Type = GuaranteeTypeList.Codes.COD;
			guarantee.CPH_StartDate = ZDate.Today.AddDays(-1);
			guarantee.CPH_EndDate = ZDate.Today.AddDays(1);
			guarantee.CPH_OH_PermitHolder = principal.PK;
			guarantee.CPH_Number = "GH123";

			Factory.Save();

			var nctsHeader = Factory.New<DummyNctsHeader_TestNewFromDeltaT>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.DestinationTrader.OrganisationPK = documentaryAddress.OrganisationPK;
			nctsHeader.ArrivalMovementHeader.BM_TransportAtDeparture = "BA1234";
			nctsHeader.ArrivalMrnFromUser = "MRN004";
			nctsHeader.UnloadingRemark.G9_UnloadingDate = ZDateTime.BrettsBirthday;
			nctsHeader.MovementHeader.BM_InlandTransportMode = "1";
			nctsHeader.MovementHeader.BM_TransportAtDeparture = "123";
			nctsHeader.MovementHeader.BM_RL_NKForeignDestPort = "AU";

			nctsHeader.DepartureHeaderContainers.RemoveAndDeleteAll();
			nctsHeader.DepartureHeaderContainers.AddNew().BC_ContainerNum = "CNT1";
			nctsHeader.DepartureHeaderContainers.AddNew().BC_ContainerNum = "CNT2";
			nctsHeader.DepartureHeaderContainers.AddNew().BC_ContainerNum = "CNT3";
			nctsHeader.DepartureHeaderContainers.AddNew().BC_ContainerNum = "CNT3";

			var message1 = nctsHeader.Messages.AddNew();
			message1.EM_MessageType = "007";
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2021, 09, 27);

			var message2 = nctsHeader.Messages.AddNew();
			message2.EM_MessageType = "029";
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2021, 09, 28);

			var co1 = nctsHeader.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == "DES") ?? nctsHeader.CustomsOffices.AddNew();
			co1.CY_Code = "DES";
			co1.CY_Data = "999";

			var dsa = nctsHeader.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == "DSA") ?? nctsHeader.CustomsOffices.AddNew();
			dsa.CY_Code = "DSA";
			dsa.CY_Data = "999";

			var co2 = nctsHeader.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == "DEP") ?? nctsHeader.CustomsOffices.AddNew();
			co2.CY_Code = "DEP";
			co2.CY_Data = "CH123";

			var co3 = nctsHeader.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == "TRA") ?? nctsHeader.CustomsOffices.AddNew();
			co3.CY_Code = "TRA";
			co3.CY_Data = "FR123";

			nctsHeader.BH_JobReferenceForReturn = "123";
			nctsHeader.ArrivalMrnFromUserForReturn = "12345";

			nctsHeader.Declarant.E2_OA_Address = declarant.MainAddress.PK;
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;

			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "GH123";
			nctsGuarantee.PW_BondAmount = 100m;

			var goodItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var goodItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			nctsHeader.MovementHeader.BM_LocationOfGoods = "LOCATION";
			goodItem1.BY_GrossWeight = 10m;
			goodItem1.BY_GrossWeightUnit = "KG";
			goodItem1.BY_MonetaryValue = 30m;
			goodItem2.BY_GrossWeight = 20m;
			goodItem2.BY_GrossWeightUnit = "KG";
			goodItem2.BY_MonetaryValue = 40m;
			var package11 = goodItem1.Packages.AddNew();
			package11.B5_UnitType = "PK";
			package11.B5_UnitCount = 1;
			var package12 = goodItem1.Packages.AddNew();
			package12.B5_UnitCount = 2;
			var package21 = goodItem2.Packages.AddNew();
			package21.B5_UnitType = "1A";
			package21.B5_UnitCount = 3;
			var package22 = goodItem2.Packages.AddNew();
			package22.B5_UnitCount = 4;

			goodItem1.BY_Description = "Description1";
			goodItem2.BY_Description = "Description2";
			goodItem1.BY_HarmonisedTariff = "340026560";
			goodItem2.BY_HarmonisedTariff = "1020304500";
			goodItem1.BY_NetWeight = 30m;
			goodItem1.BY_NetWeightUnit = "KG";
			goodItem2.BY_NetWeight = 40m;
			goodItem2.BY_NetWeightUnit = "KG";

			var goodsItem = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
			var supportingDoc1 = goodsItem.SupportingDocuments.AddNew();
			supportingDoc1.CSI_Code = "CSI01";
			var supportingDoc2 = goodsItem.SupportingDocuments.AddNew();
			supportingDoc2.CSI_Code = "CSI02";
			var supportingDoc3 = goodsItem.SupportingDocuments.AddNew();
			supportingDoc3.CSI_Code = "CSI03";

			var temporaryStorageHeader = TemporaryStorageWrapperHeader.NewFromDeltaT(nctsHeader);

			AssertEquals(documentaryAddress.OrganisationPK, temporaryStorageHeader.CustomerPK);
			AssertEquals("BA1234", temporaryStorageHeader.TransportID);
			AssertEquals(new ZDateTime(2021, 09, 27), temporaryStorageHeader.ArrivalDate);
			AssertEquals("123", temporaryStorageHeader.JobNumber);
			AssertEquals("Hard coded type. Should be 821 by default", PreviousDocumentCodeList.Codes._821, temporaryStorageHeader.PreviousEntryType);
			AssertEquals("12345", temporaryStorageHeader.PreviousEntryNumber);
			AssertEquals("999", temporaryStorageHeader.CustomsOfficeOfDestination);
			AssertEquals(declarant.MainAddress.PK, temporaryStorageHeader.PresenterPK);
			AssertEquals("FR123", temporaryStorageHeader.CustomsOfficeOfEntry);
			AssertEquals(ZDateTime.BrettsBirthday, temporaryStorageHeader.PresentationDate);
			AssertEquals("PH123", temporaryStorageHeader.CustomsProfile);
			AssertEquals("1", temporaryStorageHeader.TransportMode);
			AssertEquals("123", temporaryStorageHeader.BorderTransportInfo);
			AssertEquals("123", temporaryStorageHeader.TransportRegNo);
			AssertEquals("AU", temporaryStorageHeader.PlaceOfLoading);
			AssertEquals(new ZDateTime(2021, 09, 28), temporaryStorageHeader.DepartureDate);
			AssertEquals(3, temporaryStorageHeader.ContainerCount);
			AssertEquals(guarantee.PK, temporaryStorageHeader.GuaranteePK);

			AssertEquals(4, temporaryStorageHeader.TemporaryStorageContainers.Count);
			AssertEquals("NCTS doesn't have a container type. Set as hard coded N/A.", "N/A", temporaryStorageHeader.TemporaryStorageContainers[0].ContainerType);
			AssertEquals("CNT1", temporaryStorageHeader.TemporaryStorageContainers[0].ContainerNumber);
			AssertEquals("N/A", temporaryStorageHeader.TemporaryStorageContainers[1].ContainerType);
			AssertEquals("CNT2", temporaryStorageHeader.TemporaryStorageContainers[1].ContainerNumber);
			AssertEquals("N/A", temporaryStorageHeader.TemporaryStorageContainers[2].ContainerType);
			AssertEquals("CNT3", temporaryStorageHeader.TemporaryStorageContainers[2].ContainerNumber);
			AssertContainsExactElementsInAnyOrder(new CusSupportingInfo[] { supportingDoc1, supportingDoc2, supportingDoc3 }, temporaryStorageHeader.SupportingDocuments);

			AssertEquals(2, temporaryStorageHeader.TemporaryStorageLines.Count);
			AssertEquals("LOCATION", temporaryStorageHeader.TemporaryStorageLines[0].LocationOfGoods);
			AssertEquals("LOCATION", temporaryStorageHeader.TemporaryStorageLines[1].LocationOfGoods);
			AssertEquals(10m, temporaryStorageHeader.TemporaryStorageLines[0].GrossWeight);
			AssertEquals(20m, temporaryStorageHeader.TemporaryStorageLines[1].GrossWeight);
			AssertEquals("KG", temporaryStorageHeader.TemporaryStorageLines[0].GrossWeightUQ);
			AssertEquals("KG", temporaryStorageHeader.TemporaryStorageLines[1].GrossWeightUQ);
			AssertEquals(3, temporaryStorageHeader.TemporaryStorageLines[0].PackageQty);
			AssertEquals(7, temporaryStorageHeader.TemporaryStorageLines[1].PackageQty);
			AssertEquals("Description1", temporaryStorageHeader.TemporaryStorageLines[0].GoodsDescription);
			AssertEquals("Description2", temporaryStorageHeader.TemporaryStorageLines[1].GoodsDescription);
			AssertEquals("PK", temporaryStorageHeader.TemporaryStorageLines[0].PackageType);
			AssertEquals("1A", temporaryStorageHeader.TemporaryStorageLines[1].PackageType);

			AssertEquals(1, temporaryStorageHeader.TemporaryStorageLines[0].TemporaryStorageFurtherDetails.Count);
			AssertEquals(1, temporaryStorageHeader.TemporaryStorageLines[1].TemporaryStorageFurtherDetails.Count);

			AssertEquals("340026560", temporaryStorageHeader.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].CommodityCode);
			AssertEquals("1020304500", temporaryStorageHeader.TemporaryStorageLines[1].TemporaryStorageFurtherDetails[0].CommodityCode);
			AssertEquals(30M, temporaryStorageHeader.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].NetMass);
			AssertEquals(40M, temporaryStorageHeader.TemporaryStorageLines[1].TemporaryStorageFurtherDetails[0].NetMass);
			AssertEquals("KG", temporaryStorageHeader.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].NetMassUQ);
			AssertEquals("KG", temporaryStorageHeader.TemporaryStorageLines[1].TemporaryStorageFurtherDetails[0].NetMassUQ);
			AssertEquals(50m, temporaryStorageHeader.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].GuaranteedValue);
			AssertEquals(50m, temporaryStorageHeader.TemporaryStorageLines[1].TemporaryStorageFurtherDetails[0].GuaranteedValue);
			AssertEquals(Enterprise.Core.Constants.CurrencyCodes.EuropeanUnion, temporaryStorageHeader.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].Currency);
			AssertEquals(Enterprise.Core.Constants.CurrencyCodes.EuropeanUnion, temporaryStorageHeader.TemporaryStorageLines[1].TemporaryStorageFurtherDetails[0].Currency);
			AssertEquals(30m, temporaryStorageHeader.TemporaryStorageLines[0].TemporaryStorageFurtherDetails[0].GoodsValue);
			AssertEquals(40m, temporaryStorageHeader.TemporaryStorageLines[1].TemporaryStorageFurtherDetails[0].GoodsValue);

			var nctsHeader2 = Factory.NewMoq<NctsHeader>();
			nctsHeader2.Object.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader2.Object.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeader2.Object.UnloadedMeansOfTransportAtDepartureIdentity = "IDENTITY";
			var goodItem = nctsHeader2.Object.UnloadingMovementHeader.GoodsItems.AddNew();
			goodItem.Containers.AddNew().BC_ContainerNum = "5";

			var temporaryStorageHeader2 = TemporaryStorageWrapperHeader.NewFromDeltaT(nctsHeader2.Object);
			AssertEquals("IDENTITY", temporaryStorageHeader2.BorderTransportInfo);
			AssertEquals("IDENTITY", temporaryStorageHeader2.TransportRegNo);
			AssertEquals(1, temporaryStorageHeader2.ContainerCount);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var documentaryOrgHeader = Factory.New<OrgHeader>();
			documentaryOrgHeader.OH_Code = "XXX";
			documentaryOrgHeader.OH_FullName = "Doc";
			documentaryAddress = Factory.New<JobDocAddress>();
			documentaryAddress.OrganisationPK = documentaryOrgHeader.PK;
			documentaryAddress.E2_ParentID = documentaryAddress.PK;
			documentaryAddress.E2_ParentTableCode = "OH";
			documentaryAddress.AddressCode = "DocumentaryAddress";
			documentaryAddress.Address1 = "DocumentaryAddress";

			var pickupOrgHeader = Factory.New<OrgHeader>();
			pickupOrgHeader.OH_RL_NKClosestPort = "GBLHR";
			pickupOrgHeader.OH_Code = "ZZZ";
			pickupOrgHeader.OH_FullName = "Pick";
			pickupAddress = Factory.New<JobDocAddress>();
			pickupAddress.ClosestPort = "GBLHR";
			pickupAddress.OrganisationPK = pickupOrgHeader.PK;
			pickupAddress.E2_ParentID = pickupAddress.PK;
			pickupAddress.E2_ParentTableCode = "OH";
		}

		JobDocAddress documentaryAddress;
		JobDocAddress pickupAddress;
		//ForwardingConsol consol;
	}

	public class DummyJobDeclaration_TestNewFromDeclaration_Guarantees : JobDeclaration
	{
		public DummyJobDeclaration_TestNewFromDeclaration_Guarantees(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public CusGuaranteeHeader CusGuaranteeHeaderForReturn { get; set; }

		protected override EU.Business.CusGuaranteeHeader GetCustomsGuaranteeCore => CusGuaranteeHeaderForReturn;
	}

	public class DummyNctsHeader_TestNewFromDeltaT : NctsHeader
	{
		public DummyNctsHeader_TestNewFromDeltaT(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString BH_JobReferenceForReturn { get; set; }
		public ZString ArrivalMrnFromUserForReturn { get; set; }

		public override ZString BH_JobReference { get => BH_JobReferenceForReturn; }
		public override ZString ArrivalMrnFromUser { get => ArrivalMrnFromUserForReturn; }
	}
}
