using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class IE513And515ConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<IE513And515ConsignmentProvider>
	{
		#region IAESConsignment members
		public void TestContainerIndicator()
		{
			var provider = GetProvider();
			AssertEquals("No for no elements in Containers", AESFlagCodeList.Codes.No, provider.ContainerIndicator);
			declaration.CusContainers.AddNew();
			provider = GetProvider();
			AssertEquals("Yes for elements in Containers", AESFlagCodeList.Codes.Yes, provider.ContainerIndicator);

			instruction.CEI_SubStyle = "B";
			provider = GetProvider();
			AssertEquals("Elements in containers but not included", string.Empty, provider.ContainerIndicator);
		}

		public void TestInlandTransportMode()
		{
			var provider = GetProvider();
			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
			AssertEquals("InlandTransportMode", "1", provider.InlandTransportMode);

			instruction.CEI_SubStyle = "B";
			provider = GetProvider();
			AssertEquals("InlandTransportMode not included", string.Empty, provider.InlandTransportMode);
		}

		public void TestBorderModeOfTransport()
		{
			var provider = GetProvider();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("BorderModeOfTransport", "4", provider.BorderModeOfTransport);

			instruction.CEI_SubStyle = "C";
			provider = GetProvider();
			AssertEquals("BorderModeOfTransport not included", string.Empty, provider.BorderModeOfTransport);
		}

		public void TestGrossMass()
		{
			invoiceLine.JI_Weight = 0.325m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoiceLine2 = entryLine2.InvoiceLines.AddNew();
			invoiceLine2.JI_Weight = 0.300m;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			invoiceLine2.JI_CL = entryLine.PK;

			AssertEquals("GrossMass", 625m, Provider.GrossMass);
		}

		public void TestCarrier()
		{
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OH_ShippingLine = orgHeader.PK;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "REG111");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");

			AssertEquals("Carrier.CarrierId", "IEREG222", Provider.Carrier.CarrierId);
		}

		public void TestTransportEquipment()
		{
			AssertEquals("TransportEquipment being empty for now.", 0, Provider.TransportEquipment.Count);
		}

		public void TestLocationOfGoods()
		{
			AssertSame("LocationOfGoods", Provider, Provider.LocationOfGoods);
		}

		public void TestDepartureTransportMeans()
		{
			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
			declaration.JE_TransportMeans = "AB";
			declaration.JE_TransportIDInland = "ID1234567";
			declaration.JE_RN_NKTransportNationalityInland = "NZ";
			CombineAssertions(() =>
			{
				DepartureTransportMeansProviderTest.AssertDepartureTransportMeans(Provider.DepartureTransportMeans.Single(), "AB", "ID1234567", "NZ");
			});
		}

		public void TestDepartureTransportMeansNotIncluded()
		{
			declaration.JE_TransportModeInland = string.Empty;
			declaration.JE_TransportMeans = "AB";
			declaration.JE_TransportIDInland = "ID1234567";
			declaration.JE_RN_NKTransportNationalityInland = "NZ";

			AssertEquals("DepartureTransportMeans not included", 0, Provider.DepartureTransportMeans.Count);
		}

		public void TestCountryOfRoutingConsignment()
		{
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			instruction.CEI_Style = "B3";
			AssertEquals("CountryOfRoutingConsignment should be empty", 0, Provider.CountryOfRoutingConsignment.Count);

			instruction.CEI_Style = "B1";
			AssertArrayEqualsByElements("CountryOfRoutingConsignment", new[] { "AU" }, new IE513And515ConsignmentProvider(entryHeaderWrapper, instruction.IsSubStyle_B_C_E_F).CountryOfRoutingConsignment.ToArray());

			instruction.CEI_Style = "B2";
			declaration.ItineraryCountries.AddNew().CY_Code = "CN";
			declaration.ItineraryCountries.AddNew().CY_Code = "IE";
			AssertArrayEqualsByElements("CountryOfRoutingConsignment", new[] { "CN", "IE", "AU" }, new IE513And515ConsignmentProvider(entryHeaderWrapper, instruction.IsSubStyle_B_C_E_F).CountryOfRoutingConsignment.ToArray());

			instruction.CEI_Style = "C1";
			declaration.ItineraryCountries.AddNew().CY_Code = "AU";
			declaration.ItineraryCountries.AddNew().CY_Code = "US";
			AssertArrayEqualsByElements("CountryOfRoutingConsignment", new[] { "CN", "IE", "AU", "US" }, new IE513And515ConsignmentProvider(entryHeaderWrapper, instruction.IsSubStyle_B_C_E_F).CountryOfRoutingConsignment.ToArray());
		}

		public void TestCountryOfRoutingConsignment_GetDefaultTerritory()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: grouping);

			var countryTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Spain, "EUSFT", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var europeTradeGroup = helper.CreateTradeGroup("EUN", "EUSFR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.AddCountry(countryTradeGroup, "AB", ZDate.BrettsBirthday, ZDate.Today.AddMonths(2));
			helper.AddCountry(europeTradeGroup, Core.Constants.CountryCodes.Spain, ZDate.BrettsBirthday, ZDate.Today.AddMonths(2));
			Factory.Save();

			declaration.JE_RL_NKPortOfArrival = "ABCDE";
			instruction.CEI_Style = "B1";
			AssertArrayEqualsByElements("CountryOfRoutingConsignment", new[] { Core.Constants.CountryCodes.Spain }, new IE513And515ConsignmentProvider(entryHeaderWrapper, instruction.IsSubStyle_B_C_E_F).CountryOfRoutingConsignment.ToArray());

			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.ItineraryCountries.AddNew().CY_Code = "AB";
			AssertArrayEqualsByElements("CountryOfRoutingConsignment", new[] { "ES", "AU" }, new IE513And515ConsignmentProvider(entryHeaderWrapper, instruction.IsSubStyle_B_C_E_F).CountryOfRoutingConsignment.ToArray());
		}

		public void TestActiveTransportMeans()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = "FL1234";
			CombineAssertions(() =>
			{
				AssertType<ActiveTransportMeansProvider>("ActiveTransportMeans", Provider.ActiveTransportMeans);
				AssertEquals("ActiveTransportMeans ID Type", "40", Provider.ActiveTransportMeans.TypeOfIdentification);
				AssertEquals("ActiveTransportMeans ID Number", "FL1234", Provider.ActiveTransportMeans.IdentificationNumber);
			});
		}

		public void TestActiveTransportMeansNotIncluded()
		{
			declaration.JE_TransportMode = string.Empty;
			declaration.JE_VoyageFlightNo = "FL1234";

			AssertEquals("ActiveTransportMeans not included while JE_TransportMode is Empty", null, Provider.ActiveTransportMeans);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.C1;
			var provider = GetProvider();
			AssertEquals("ActiveTransportMeans not included while CEI_Style is C1 and JE_TransportMode is not null", null, provider.ActiveTransportMeans);

			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			declaration.JE_VoyageFlightNo = "FL1234";
			provider = GetProvider();
			CombineAssertions("ActiveTransportMeans included while CEI_Style is not C1 and JE_TransportMode is not empty", () =>
			{
				AssertType<ActiveTransportMeansProvider>("ActiveTransportMeans", provider.ActiveTransportMeans);
				AssertEquals("ActiveTransportMeans ID Type", "40", provider.ActiveTransportMeans.TypeOfIdentification);
				AssertEquals("ActiveTransportMeans ID Number", "FL1234", provider.ActiveTransportMeans.IdentificationNumber);
			});
		}

		public void TestTransportDocuments()
		{
			var ref0 = instruction.AdditionalInfos.AddNew();
			ref0.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			ref0.CSI_Code = "TD0";
			ref0.CSI_ReferenceNumber = "REF1";
			ref0.CSI_Description = "Description0";
			var ref00 = invoiceHeader.AdditionalInfos.AddNew();
			ref00.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			ref00.CSI_Code = "TD0";
			ref00.CSI_ReferenceNumber = "REF1";
			ref00.CSI_Description = "Description00";
			var ref1 = invoiceHeader.AdditionalInfos.AddNew();
			ref1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			ref1.CSI_Code = "TD1";
			ref1.CSI_ReferenceNumber = "REF1";
			ref1.CSI_Description = "Description1";
			var ref2 = invoiceLine.AdditionalInfos.AddNew();
			ref2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			ref2.CSI_Code = "TD2";
			ref2.CSI_ReferenceNumber = "REF1";
			ref2.CSI_Description = "Description2";
			var ref5 = invoiceHeader.AdditionalInfos.AddNew();
			ref5.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			ref5.CSI_Code = "TD4";
			ref5.CSI_ReferenceNumber = "REF1";
			ref5.CSI_Description = "Description4";

			AssertContainsExactElementsInAnyOrder("After Transition Period", new[] { "TD0|REF1", "TD1|REF1", "TD4|REF1" }, Provider.TransportDocuments.Select(x => x.Type + "|" + x.Reference));

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				AssertNull("During Transition Period", GetProvider().TransportDocuments);
			}
		}

		#endregion

		#region ILocationOfGoods Members
		public void TestLocationCodeType()
		{
			declaration.JE_LocationOtherInformation = "KD";
			AssertEquals("LocationCodeType", "KD", Provider.LocationCodeType);
		}

		public void TestUNLocode()
		{
			declaration.JE_LocationOfGoods = "KD";
			AssertEquals("UNLocode", "KD", Provider.UNLocode);
		}
		#endregion

		protected override IE513And515ConsignmentProvider GetProvider() => new IE513And515ConsignmentProvider(entryHeaderWrapper, instruction.IsSubStyle_B_C_E_F);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryLine = entryHeader.MergedLines.AddNew();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
		}

		EntryHeaderWrapper entryHeaderWrapper;
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		CusEntryInstruction instruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
	}
}
