using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Generators;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUKG2G
{
	namespace Basic.Testing
	{
		abstract class CUKG2GTestsBasicBase : TestCaseWithFactory
		{
			protected override void SetUp()
			{
				base.SetUp();
				LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false, false, AgentType, direction: "EXP", mucrGenerationStyle: MucrGenerationStyles.Codes.Air);
				var notifier = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
				consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_MasterBillNum = "11122222222";
				consol.JK_RL_NKLoadPort = "GBLHR";
				consol.JK_AgentType = "DRT";
				shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_UniqueConsignRef = "S000100";
				shipment.JS_HouseBill = "IGNORE";
				var wrapper = new CustomsExportConsolIntegrationWrapper(consol, notifier);
				mawbExport = wrapper.MawbExportHelper;
				mawbExport.ME_Profile = "LXA";
				mawbExport.ME_ExportShed = "BAC";
				mawbExport.ME_ExportLocation = "LHR";
				mawbExport.CalculateMUCR();
				errorCollector = new ErrorCollector();
				generator = new CUKG2GMessageGenerator(wrapper, new UkCharSet(), errorCollector);
			}

			protected abstract ZString AgentType { get; }

			protected Common.CusEntryNumber MakeCar(Common.CusEntryNumAdditionalReferenceCollection numbs)
			{
				var car = numbs.AddNew();
				car.CE_EntryNum = "123456789012345";
				car.CE_EntryType = "CAR";
				return car;
			}

			protected ForwardingShipment shipment;
			protected ForwardingConsol consol;
			protected CUKG2GMessageGenerator generator;
			protected ErrorCollector errorCollector;
			protected MawbExportAddInfo mawbExport;
		}

		class Type1 : CUKG2GTestsBasicBase
		{
			public void TestMakeG2g_Basic()
			{
				var messageText = generator.MakeMessageText();
				AssertContains("Type 1 agents cannot send G2G without a consol- or shipment-level customs authorisaton reference and without any valid declarations. Add a CAR to the AWB or add a DUCR or CAR to Direct Shipment S000100", errorCollector.GetErrorsAsString());
				MakeCar(consol.Numbers);
				messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertEquals(@"UNH+<<MSGNO PLACEHOLDER>>+CUKG2G:A:04A:BT+<<SYSCAR>>'BGM+740+11122222222'RFF+UCN:A?:11122222222'RFF+AWB:11122222222'RFF+ZZZ:123456789012345'NAD+CB+LXA'GEI+4+TP1'GID+1'UNT+9+<<MSGNO PLACEHOLDER>>'", messageText);
				var interpretation = generator.MessageInterpretation;
				AssertContains("A:11122222222", interpretation);
				AssertContains("AWB</td><td>11122222222</td>", interpretation);
				AssertContains("Master CAR</td><td>123456789012345</td>", interpretation);
				AssertContains("Location</td><td>LHRBAC</td>", interpretation);
				AssertContains("<td>Agent badge</td><td>LXA</td>", interpretation);
				AssertContains("<td>Agent type</td><td>TP1</td>", interpretation);
				AssertContains("<h3>Direct Shipment S000100</h3>", interpretation);
			}

			public void TestMakeG2g_BasicWithInternalDeclaration()
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_UCR = "2GB123456789000-B0001000";
				declaration.JE_DeclarationReference = "B0001000";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_BGMReference = "2GB123456789000-B0001000/99";
				var messageText = generator.MakeMessageText();
				var errors = errorCollector.GetErrorsAsString();
				AssertContains("Entry 2GB123456789000-B0001000/99 on declaration B0001000", errors);
				AssertNotContains("must supply an airport and shed code", errors);
				AssertNotContains("Type 1 agents should supply an airport and shed code when route is not H", errors);
				AssertContains("cannot omit both the customs authorisation reference and the declaration details", errors);

				var declarationCar = MakeCar(declaration.AdditionalReferenceNumbers);
				declaration.JE_LocationOfGoods = "LHR";
				declaration.SubLocation = "BAC";
				messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"GID+1'RFF+ABO:2GB123456789000-B0001000:99'LOC+14+LHR:145:3:BAC'RFF+ZZZ:123456789012345'UNT", messageText);

				declarationCar.Delete();
				declaration.SingleEntry.CH_StyleOfEntrySOE = "6";
				entry.EntryNumber = "120-123456A";
				entry.CusEntryNumber.CE_IssueDate = ZDateTime.BrettsBirthday;
				messageText = generator.MakeMessageText();
				AssertContains("SOE=7", errorCollector.GetErrorsAsString());
				declaration.SingleEntry.CH_StyleOfEntrySOE = "7";
				messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"GID+1'RFF+ABO:2GB123456789000-B0001000:99'LOC+22+120'RFF+ABT:123456A'DTM+7:19710918:102'GEI+SOE+7'LOC+14+LHR:145:3:BAC'UNT", messageText);

				declaration.JE_LocationOfGoods = "";
				declaration.SubLocation = "";
				messageText = generator.MakeMessageText();
				errors = errorCollector.GetErrorsAsString();
				AssertNotContains("must supply an airport and shed code", errors);
				AssertContains(@"GID+1'RFF+ABO:2GB123456789000-B0001000:99'LOC+22+120'RFF+ABT:123456A'DTM+7:19710918:102'GEI+SOE+7'UNT", messageText);  // airport/shed segement is missing

				declaration.SingleEntry.CH_StyleOfEntrySOE = "6";
				messageText = generator.MakeMessageText();
				errors = errorCollector.GetErrorsAsString();
				AssertNotContains("must supply an airport and shed code", errors);
				AssertNotContains("Type 1 agents should supply an airport and shed code when route is not H", errors);
				declaration.SingleEntry.CH_RouteOfEntry = "1";
				messageText = generator.MakeMessageText();
				errors = errorCollector.GetErrorsAsString();
				AssertContains("Only declarations with SOE=7, or SOE=1 & route=H, can be sent", errors);
				declaration.SingleEntry.CH_RouteOfEntry = "H";
				messageText = generator.MakeMessageText();
				errors = errorCollector.GetErrorsAsString();
				AssertContains("Only declarations with SOE=7, or SOE=1 & route=H, can be sent", errors);
				declaration.SingleEntry.CH_StyleOfEntrySOE = "1";
				messageText = generator.MakeMessageText();
				errors = errorCollector.GetErrorsAsString();
				AssertNotContains("Only declarations with SOE=7, or SOE=1 & route=H, can be sent", errors);
			}

			public void TestMakeG2g_BasicWithUnmergedDeclaration()
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_UCR = "2GB123456789000-WHATEVER";
				declaration.JE_DeclarationReference = "B0001000";
				var messageText = generator.MakeMessageText();
				var errors = errorCollector.GetErrorsAsString();
				AssertContains("Declaration without entry 2GB123456789000-WHATEVER on B0001000", errors);

				var declarationCar = MakeCar(declaration.AdditionalReferenceNumbers);
				declaration.JE_LocationOfGoods = "LHR";
				declaration.SubLocation = "BAC";
				messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"GID+1'RFF+ABO:2GB123456789000-WHATEVER'LOC+14+LHR:145:3:BAC'RFF+ZZZ:123456789012345'UNT", messageText);
			}

			public void TestMakeG2g_BasicMultipleEntries()
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				var invoice1 = declaration.Invoices.AddNew();
				var invoice2 = declaration.Invoices.AddNew();
				invoice1.ZG_CustomsAuthorisationReferenceForExportFallback = "CAR1";
				invoice2.ZG_CustomsAuthorisationReferenceForExportFallback = "CAR2";
				var invLine1 = invoice1.InvoiceLines.AddNew();
				var invLine2 = invoice2.InvoiceLines.AddNew();
				var entry1 = declaration.CustomsEntryHeaders.AddNew();
				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				var entryLine1 = entry1.MergedLines.AddNew();
				var entryLine2 = entry2.MergedLines.AddNew();
				invLine1.JI_CL = entryLine1.PK;
				invLine2.JI_CL = entryLine2.PK;
				declaration.JE_UCR = "2GB123456789000-B0001000";
				declaration.JE_DeclarationReference = "B0001000";
				entry1.CH_BGMReference = "2GB123456789000-B0001000/1";
				entry2.CH_BGMReference = "2GB123456789000-B0001000/2";
				declaration.JE_LocationOfGoods = "LHR";
				declaration.SubLocation = "BAC";

				var messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"GID+1'RFF+ABO:2GB123456789000-B0001000:1'LOC+14+LHR:145:3:BAC'RFF+ZZZ:CAR1'RFF+ABO:2GB123456789000-B0001000:2'LOC+14+LHR:145:3:BAC'RFF+ZZZ:CAR2'UNT", messageText);
				AssertContains("<h4>Entry 2GB123456789000-B0001000/1 on declaration B0001000</h4>", generator.MessageInterpretation);
			}

			public void TestMakeG2g_BasicExternalDeclaration()
			{
				// We do not support external entry numbers - use DUCR & CAR only
				var ducr = shipment.Numbers.AddNew();
				ducr.CE_EntryType = "UCR";
				ducr.CE_EntryNum = "2GB123456789000-EXTERNAL";
				shipment.CustomsEntryNumber = "IGNORE";
				var messageText = generator.MakeMessageText();
				var errors = errorCollector.GetErrorsAsString();
				AssertNotContains("Manual entry number from shipment form is ignored", "IGNORE", messageText);
				AssertContains("Type 1 agents cannot omit both the customs authorisation reference and the declaration details", errors);
				AssertContains("External declaration 2GB123456789000-EXTERNAL on shipment S000100", errors);
				ducr.CE_EntryLineReference = "EXTCAR4UCR12345";
				messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains("Shed and airport from consol, DUCR and DUCR-CAR from shipment's numbers grid", "RFF+AWB:11122222222'NAD+CB+LXA'GEI+4+TP1'GID+1'RFF+ABO:2GB123456789000-EXTERNAL'LOC+14+LHR:145:3:BAC'RFF+ZZZ:EXTCAR4UCR12345'UNT", messageText);
				AssertNotContains("Manual entry number from shipment form is ignored", "IGNORE", messageText);
			}

			protected override ZString AgentType
			{
				get { return AgentTypeForExportFallbackList.Codes.Type1; }
			}
		}

		class Type2 : CUKG2GTestsBasicBase
		{
			public void TestMakeG2g_Basic()
			{
				var messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"RFF+AWB:11122222222'NAD+CB+LXA'GEI+4+TP2'GID+1'UNT", messageText);
				MakeCar(consol.Numbers);
				messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"RFF+AWB:11122222222'RFF+ZZZ:123456789012345'NAD+CB+LXA'GEI+4+TP2'GID+1'UNT", messageText);
			}

			public void TestMakeG2g_BasicWithInternalDeclaration()
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_UCR = "2GB123456789000-B0001000";
				declaration.JE_DeclarationReference = "B0001000";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_BGMReference = "2GB123456789000-B0001000/99";
				var messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains("RFF+AWB:11122222222'NAD+CB+LXA'GEI+4+TP2'GID+1'RFF+ABO:2GB123456789000-B0001000:99'UNT", messageText);

				var declarationCar = MakeCar(declaration.AdditionalReferenceNumbers);
				declaration.JE_LocationOfGoods = "LHR";
				declaration.SubLocation = "BAC";
				messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"RFF+AWB:11122222222'NAD+CB+LXA'GEI+4+TP2'GID+1'RFF+ABO:2GB123456789000-B0001000:99'LOC+14+LHR:145:3:BAC'RFF+ZZZ:123456789012345'UNT", messageText);

				declarationCar.Delete();
				declaration.SingleEntry.CH_StyleOfEntrySOE = "6";
				entry.EntryNumber = "120-123456A";
				entry.CusEntryNumber.CE_IssueDate = ZDateTime.BrettsBirthday;
				messageText = generator.MakeMessageText();
				AssertNotContains("SOE 7", errorCollector.GetErrorsAsString());
				AssertContains(@"RFF+AWB:11122222222'NAD+CB+LXA'GEI+4+TP2'GID+1'RFF+ABO:2GB123456789000-B0001000:99'LOC+22+120'RFF+ABT:123456A'DTM+7:19710918:102'GEI+SOE+6'LOC+14+LHR:145:3:BAC'UNT", messageText);
			}

			public void TestMakeG2g_BasicWithUnmergedDeclaration()
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_UCR = "2GB123456789000-WHATEVER";
				declaration.JE_DeclarationReference = "B0001000";
				var messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains("RFF+AWB:11122222222'NAD+CB+LXA'GEI+4+TP2'GID+1'RFF+ABO:2GB123456789000-WHATEVER'UNT", messageText);

				var declarationCar = MakeCar(declaration.AdditionalReferenceNumbers);
				messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"RFF+AWB:11122222222'NAD+CB+LXA'GEI+4+TP2'GID+1'RFF+ABO:2GB123456789000-WHATEVER'RFF+ZZZ:123456789012345'UNT", messageText);
			}

			public void TestMakeG2g_BasicMultipleEntries()
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				var invoice1 = declaration.Invoices.AddNew();
				var invoice2 = declaration.Invoices.AddNew();
				invoice1.ZG_CustomsAuthorisationReferenceForExportFallback = "CAR1";
				invoice2.ZG_CustomsAuthorisationReferenceForExportFallback = "CAR2";
				var invLine1 = invoice1.InvoiceLines.AddNew();
				var invLine2 = invoice2.InvoiceLines.AddNew();
				var entry1 = declaration.CustomsEntryHeaders.AddNew();
				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				var entryLine1 = entry1.MergedLines.AddNew();
				var entryLine2 = entry2.MergedLines.AddNew();
				invLine1.JI_CL = entryLine1.PK;
				invLine2.JI_CL = entryLine2.PK;
				declaration.JE_UCR = "2GB123456789000-B0001000";
				declaration.JE_DeclarationReference = "B0001000";
				entry1.CH_BGMReference = "2GB123456789000-B0001000/1";
				entry2.CH_BGMReference = "2GB123456789000-B0001000/2";
				declaration.JE_LocationOfGoods = "LHR";
				declaration.SubLocation = "BAC";

				var messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"GID+1'RFF+ABO:2GB123456789000-B0001000:1'LOC+14+LHR:145:3:BAC'RFF+ZZZ:CAR1'RFF+ABO:2GB123456789000-B0001000:2'LOC+14+LHR:145:3:BAC'RFF+ZZZ:CAR2'UNT", messageText);
				AssertContains("<h4>Entry 2GB123456789000-B0001000/1 on declaration B0001000</h4>", generator.MessageInterpretation);
			}

			public void TestMakeG2g_BasicExternalDeclaration()
			{
				// We do not support external entry numbers - use DUCR & CAR only
				var ducr = shipment.Numbers.AddNew();
				ducr.CE_EntryType = "UCR";
				ducr.CE_EntryNum = "2GB123456789000-EXTERNAL";
				shipment.CustomsEntryNumber = "IGNORE";
				var messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertNotContains("Manual entry number from shipment form is ignored", "IGNORE", messageText);
			}

			protected override ZString AgentType
			{
				get { return AgentTypeForExportFallbackList.Codes.Type2; }
			}
		}
	}

	namespace Consol.Testing
	{
		abstract class CUKG2GTestsConsolBase : TestCaseWithFactory
		{
			protected override void SetUp()
			{
				base.SetUp();
				LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false, false, AgentType, direction: "EXP", mucrGenerationStyle: Registry.MucrGenerationStyles.Codes.Air);
				var notifier = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
				consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_MasterBillNum = "11122222222";
				consol.JK_RL_NKLoadPort = "GBLHR";
				consol.JK_AgentType = "AGT";
				shipment1 = consol.Shipments.AddNew();
				shipment1.JS_RL_NKOrigin = "GBLHR";
				shipment1.JS_UniqueConsignRef = "S000100";
				shipment1.JS_HouseBill = "HOUSE001";
				shipment2 = consol.Shipments.AddNew();
				shipment2.JS_RL_NKOrigin = "GBLHR";
				shipment2.JS_UniqueConsignRef = "S000222";
				shipment2.JS_HouseBill = "HOUSE002";
				var wrapper = new CustomsExportConsolIntegrationWrapper(consol, notifier);
				mawbExport = wrapper.MawbExportHelper;
				mawbExport.ME_Profile = "LXA";
				mawbExport.ME_ExportShed = "BAC";
				mawbExport.ME_ExportLocation = "LHR";
				errorCollector = new ErrorCollector();
				generator = new CUKG2GMessageGenerator(wrapper, new UkCharSet(), errorCollector);
			}

			protected abstract ZString AgentType { get; }

			protected Common.CusEntryNumber MakeCar(Common.CusEntryNumAdditionalReferenceCollection numbs, string suffix)
			{
				var car = numbs.AddNew();
				car.CE_EntryNum = "1234567890123" + "-" + suffix;
				car.CE_EntryType = "CAR";
				return car;
			}

			protected ForwardingShipment shipment1;
			protected ForwardingShipment shipment2;
			protected ForwardingConsol consol;
			protected CUKG2GMessageGenerator generator;
			protected ErrorCollector errorCollector;
			protected MawbExportAddInfo mawbExport;
		}

		class Type1 : CUKG2GTestsConsolBase
		{
			public void TestMakeG2g_Consol()
			{
				var messageText = generator.MakeMessageText();
				var errors = errorCollector.GetErrorsAsString();
				AssertContains("Type 1 agents cannot send G2G without a consol- or shipment-level customs authorisaton reference and without any valid declarations. Add a CAR to the AWB or add a DUCR or CAR to Shipment S000100", errors);
				AssertContains("Type 1 agents cannot send G2G without a consol- or shipment-level customs authorisaton reference and without any valid declarations. Add a CAR to the AWB or add a DUCR or CAR to Shipment S000222", errors);
				MakeCar(consol.Numbers, "C");
				messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertEquals(@"UNH+<<MSGNO PLACEHOLDER>>+CUKG2G:A:04A:BT+<<SYSCAR>>'BGM+740+11122222222'RFF+UCN:A?:11122222222'RFF+AWB:11122222222'RFF+ZZZ:1234567890123-C'NAD+CB+LXA'GEI+4+TP1'GID+1'RFF+HWB:HOUSE001'GID+2'RFF+HWB:HOUSE002'UNT+12+<<MSGNO PLACEHOLDER>>'", messageText);
				var interpretation = generator.MessageInterpretation;
				AssertContains("A:11122222222", interpretation);
				AssertContains("AWB</td><td>11122222222</td>", interpretation);
				AssertContains("Master CAR</td><td>1234567890123-C</td>", interpretation);
				AssertContains("Location</td><td>LHRBAC</td>", interpretation);
				AssertContains("<td>Agent badge</td><td>LXA</td>", interpretation);
				AssertContains("<td>Agent type</td><td>TP1</td>", interpretation);
				AssertContains("<h3>Shipment S000100</h3>", interpretation);
				AssertContains("<h3>Shipment S000222</h3>", interpretation);
			}

			public void TestMakeG2g_ConsolWithInternalDeclaration()
			{
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_JS = shipment1.PK;
				declaration1.JE_UCR = "2GB123456789000-B0001000";
				declaration1.JE_DeclarationReference = "B0001000";
				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_JS = shipment2.PK;
				declaration2.JE_UCR = "2GB123456789000-B0002000";
				declaration2.JE_DeclarationReference = "B0002000";
				var entry1 = declaration1.CustomsEntryHeaders.AddNew();
				entry1.CH_BGMReference = "2GB123456789000-B0001000/99";
				var entry2 = declaration2.CustomsEntryHeaders.AddNew();
				entry2.CH_BGMReference = "2GB123456789000-B0002000";
				var messageText = generator.MakeMessageText();
				var errors = errorCollector.GetErrorsAsString();
				AssertNotContains("airport and shed code when SOE=7. Entry 2GB123456789000-B0001000/99 on declaration B0001000", errors);
				AssertContains("Obtain a CAR from the NCH and supply it. Entry 2GB123456789000-B0001000/99", errors);
				AssertNotContains("airport and shed code when SOE=7. Entry 2GB123456789000-B0002000 on declaration B0002000", errors);
				AssertContains("Obtain a CAR from the NCH and supply it. Entry 2GB123456789000-B0002000", errors);

				var declarationCar1 = MakeCar(declaration1.AdditionalReferenceNumbers, "1");
				declaration1.JE_LocationOfGoods = "LHR";
				declaration1.SubLocation = "BAC";
				var declarationCar2 = MakeCar(declaration2.AdditionalReferenceNumbers, "2");
				declaration2.JE_LocationOfGoods = "MAN";
				declaration2.SubLocation = "SLS";
				messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"GID+1'RFF+HWB:HOUSE001'RFF+ABO:2GB123456789000-B0001000:99'LOC+14+LHR:145:3:BAC'RFF+ZZZ:1234567890123-1'GID+2'RFF+HWB:HOUSE002'RFF+ABO:2GB123456789000-B0002000'LOC+14+MAN:145:3:SLS'RFF+ZZZ:1234567890123-2'UNT", messageText);

				declarationCar1.Delete();
				declaration1.SingleEntry.CH_StyleOfEntrySOE = "6";
				entry1.EntryNumber = "120-123456A";
				entry1.CusEntryNumber.CE_IssueDate = ZDateTime.BrettsBirthday;
				messageText = generator.MakeMessageText();
				AssertContains("SOE=7", errorCollector.GetErrorsAsString());
				declaration1.SingleEntry.CH_StyleOfEntrySOE = "7";
				messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"GID+1'RFF+HWB:HOUSE001'RFF+ABO:2GB123456789000-B0001000:99'LOC+22+120'RFF+ABT:123456A'DTM+7:19710918:102'GEI+SOE+7'LOC+14+LHR:145:3:BAC'GID+2'RFF+HWB:HOUSE002'RFF+ABO:2GB123456789000-B0002000'LOC+14+MAN:145:3:SLS'RFF+ZZZ:1234567890123-2'UNT", messageText);
			}

			public void TestMakeG2g_ConsolWithUnmergedDeclaration()
			{
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_JS = shipment1.PK;
				declaration1.JE_UCR = "2GB123456789000-WHATEVER1";
				declaration1.JE_DeclarationReference = "B0001000";
				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_JS = shipment2.PK;
				declaration2.JE_UCR = "2GB123456789000-WHATEVER2";
				declaration2.JE_DeclarationReference = "B0002000";
				var messageText = generator.MakeMessageText();
				var errors = errorCollector.GetErrorsAsString();
				AssertContains("Declaration without entry 2GB123456789000-WHATEVER1 on B0001000", errors);
				AssertContains("Declaration without entry 2GB123456789000-WHATEVER2 on B0002000", errors);

				MakeCar(declaration1.AdditionalReferenceNumbers, "1");
				MakeCar(declaration2.AdditionalReferenceNumbers, "2");
				declaration1.JE_LocationOfGoods = "LHR";
				declaration1.SubLocation = "BAC";
				declaration2.JE_LocationOfGoods = "MAN";
				declaration2.SubLocation = "SLS";
				messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"GID+1'RFF+HWB:HOUSE001'RFF+ABO:2GB123456789000-WHATEVER1'LOC+14+LHR:145:3:BAC'RFF+ZZZ:1234567890123-1'GID+2'RFF+HWB:HOUSE002'RFF+ABO:2GB123456789000-WHATEVER2'LOC+14+MAN:145:3:SLS'RFF+ZZZ:1234567890123-2'UNT", messageText);
			}

			public void TestMakeG2g_ConsolMultipleEntries()
			{
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_JS = shipment1.PK;
				var invoice11 = declaration1.Invoices.AddNew();
				var invoice12 = declaration1.Invoices.AddNew();
				invoice11.ZG_CustomsAuthorisationReferenceForExportFallback = "CAR1";
				invoice12.ZG_CustomsAuthorisationReferenceForExportFallback = "CAR2";
				var invLine11 = invoice11.InvoiceLines.AddNew();
				var invLine12 = invoice12.InvoiceLines.AddNew();
				var entry11 = declaration1.CustomsEntryHeaders.AddNew();
				var entry12 = declaration1.CustomsEntryHeaders.AddNew();
				var entryLine11 = entry11.MergedLines.AddNew();
				var entryLine12 = entry12.MergedLines.AddNew();
				invLine11.JI_CL = entryLine11.PK;
				invLine12.JI_CL = entryLine12.PK;
				declaration1.JE_UCR = "2GB123456789000-B0001000";
				declaration1.JE_DeclarationReference = "B0001000";
				entry11.CH_BGMReference = "2GB123456789000-B0001000/1";
				entry12.CH_BGMReference = "2GB123456789000-B0001000/2";
				declaration1.JE_LocationOfGoods = "LHR";
				declaration1.SubLocation = "BAC";

				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_JS = shipment2.PK;
				var invoice21 = declaration2.Invoices.AddNew();
				var invoice22 = declaration2.Invoices.AddNew();
				invoice21.ZG_CustomsAuthorisationReferenceForExportFallback = "CAR1";
				invoice22.ZG_CustomsAuthorisationReferenceForExportFallback = "CAR2";
				var invLine21 = invoice21.InvoiceLines.AddNew();
				var invLine22 = invoice22.InvoiceLines.AddNew();
				var entry21 = declaration2.CustomsEntryHeaders.AddNew();
				var entry22 = declaration2.CustomsEntryHeaders.AddNew();
				var entryLine21 = entry21.MergedLines.AddNew();
				var entryLine22 = entry22.MergedLines.AddNew();
				invLine21.JI_CL = entryLine21.PK;
				invLine22.JI_CL = entryLine22.PK;
				declaration2.JE_UCR = "2GB123456789000-B0002000";
				declaration2.JE_DeclarationReference = "B0002000";
				entry21.CH_BGMReference = "2GB123456789000-B0002000/1";
				entry22.CH_BGMReference = "2GB123456789000-B0002000/2";
				declaration2.JE_LocationOfGoods = "MAN";
				declaration2.SubLocation = "SLS";

				var messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"GID+1'RFF+HWB:HOUSE001'RFF+ABO:2GB123456789000-B0001000:1'LOC+14+LHR:145:3:BAC'RFF+ZZZ:CAR1'RFF+ABO:2GB123456789000-B0001000:2'LOC+14+LHR:145:3:BAC'RFF+ZZZ:CAR2'GID+2'RFF+HWB:HOUSE002'RFF+ABO:2GB123456789000-B0002000:1'LOC+14+MAN:145:3:SLS'RFF+ZZZ:CAR1'RFF+ABO:2GB123456789000-B0002000:2'LOC+14+MAN:145:3:SLS'RFF+ZZZ:CAR2'UNT", messageText);
				AssertContains("<h4>Entry 2GB123456789000-B0001000/1 on declaration B0001000</h4>", generator.MessageInterpretation);
				AssertContains("<h4>Entry 2GB123456789000-B0002000/1 on declaration B0002000</h4>", generator.MessageInterpretation);
			}

			public void TestMakeG2g_ConsolExternalDeclaration()
			{
				// We do not support external entry numbers - use DUCR & CAR only
				var ducr1 = shipment1.Numbers.AddNew();
				ducr1.CE_EntryType = "UCR";
				ducr1.CE_EntryNum = "2GB123456789000-EXTERNAL1";
				shipment1.CustomsEntryNumber = "IGNORE1";
				var ducr2 = shipment2.Numbers.AddNew();
				ducr2.CE_EntryType = "UCR";
				ducr2.CE_EntryNum = "2GB123456789000-EXTERNAL2";
				shipment2.CustomsEntryNumber = "IGNORE2";
				var messageText = generator.MakeMessageText();
				var errors = errorCollector.GetErrorsAsString();
				AssertNotContains("Manual entry number from shipment form is ignored", "IGNORE", messageText);
				AssertContains("Type 1 agents cannot omit both the customs authorisation reference and the declaration details", errors);
				AssertContains("External declaration 2GB123456789000-EXTERNAL1 on shipment S000100", errors);
				AssertContains("External declaration 2GB123456789000-EXTERNAL2 on shipment S000222", errors);
				ducr1.CE_EntryLineReference = "EXTCAR4UC1";
				ducr2.CE_EntryLineReference = "EXTCAR4UC2";
				messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains("Shed and airport from consol, DUCR and DUCR-CAR from shipment's numbers grid", "GID+1'RFF+HWB:HOUSE001'RFF+ABO:2GB123456789000-EXTERNAL1'LOC+14+LHR:145:3:BAC'RFF+ZZZ:EXTCAR4UC1'GID+2'RFF+HWB:HOUSE002'RFF+ABO:2GB123456789000-EXTERNAL2'LOC+14+LHR:145:3:BAC'RFF+ZZZ:EXTCAR4UC2'UNT", messageText);
				AssertNotContains("Manual entry number from shipment form is ignored", "IGNORE", messageText);
			}

			protected override ZString AgentType
			{
				get { return AgentTypeForExportFallbackList.Codes.Type1; }
			}
		}

		class Type2 : CUKG2GTestsConsolBase
		{
			public void TestMakeG2g_Consol()
			{
				var messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"RFF+AWB:11122222222'NAD+CB+LXA'GEI+4+TP2'GID+1'RFF+HWB:HOUSE001'GID+2'RFF+HWB:HOUSE002'UNT", messageText);
			}

			public void TestMakeG2g_ConsolWithInternalDeclaration()
			{
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_JS = shipment1.PK;
				declaration1.JE_UCR = "2GB123456789000-B0001000";
				declaration1.JE_DeclarationReference = "B0001000";
				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_JS = shipment2.PK;
				declaration2.JE_UCR = "2GB123456789000-B0002000";
				declaration2.JE_DeclarationReference = "B0002000";
				var entry1 = declaration1.CustomsEntryHeaders.AddNew();
				entry1.CH_BGMReference = "2GB123456789000-B0001000/99";
				var entry2 = declaration2.CustomsEntryHeaders.AddNew();
				entry2.CH_BGMReference = "2GB123456789000-B0002000";
				var messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);

				declaration1.JE_LocationOfGoods = "LHR";
				declaration1.SubLocation = "BAC";
				declaration2.JE_LocationOfGoods = "MAN";
				declaration2.SubLocation = "SLS";
				declaration1.SingleEntry.CH_StyleOfEntrySOE = "6";
				entry1.EntryNumber = "120-123456A";
				entry1.CusEntryNumber.CE_IssueDate = ZDateTime.BrettsBirthday;
				messageText = generator.MakeMessageText();
				AssertContains("Type 2 agents cannot send for declarations at a frontier location with SOE not 7 and route not H", errorCollector.GetErrorsAsString());
				declaration1.SingleEntry.CH_StyleOfEntrySOE = "7";
				messageText = generator.MakeMessageText();
				AssertNotContains("Type 2 agents cannot send for declarations at a frontier location with SOE not 7 and route not H", errorCollector.GetErrorsAsString());
				declaration1.SingleEntry.CH_StyleOfEntrySOE = "6";
				declaration1.SubLocation = "XCW"; // not a frontier location
				messageText = generator.MakeMessageText();
				AssertNotContains("Type 2 agents cannot send for declarations at a frontier location with SOE not 7 and route not H", errorCollector.GetErrorsAsString());
				AssertContains(@"GID+1'RFF+HWB:HOUSE001'RFF+ABO:2GB123456789000-B0001000:99'LOC+22+120'RFF+ABT:123456A'DTM+7:19710918:102'GEI+SOE+6'LOC+14+LHR:145:3:XCW'GID+2'RFF+HWB:HOUSE002'RFF+ABO:2GB123456789000-B0002000'LOC+14+MAN:145:3:SLS'UNT", messageText);
			}

			public void TestMakeG2g_ConsolWithUnmergedDeclaration()
			{
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_JS = shipment1.PK;
				declaration1.JE_UCR = "2GB123456789000-WHATEVER1";
				declaration1.JE_DeclarationReference = "B0001000";
				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_JS = shipment2.PK;
				declaration2.JE_UCR = "2GB123456789000-WHATEVER2";
				declaration2.JE_DeclarationReference = "B0002000";
				var messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				MakeCar(declaration1.AdditionalReferenceNumbers, "1");
				MakeCar(declaration2.AdditionalReferenceNumbers, "2");
				declaration1.JE_LocationOfGoods = "LHR";
				declaration1.SubLocation = "BAC";
				declaration2.JE_LocationOfGoods = "MAN";
				declaration2.SubLocation = "SLS";
				messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"GID+1'RFF+HWB:HOUSE001'RFF+ABO:2GB123456789000-WHATEVER1'LOC+14+LHR:145:3:BAC'RFF+ZZZ:1234567890123-1'GID+2'RFF+HWB:HOUSE002'RFF+ABO:2GB123456789000-WHATEVER2'LOC+14+MAN:145:3:SLS'RFF+ZZZ:1234567890123-2'UNT", messageText);
			}

			public void TestMakeG2g_ConsolMultipleEntries()
			{
				var declaration1 = Factory.New<JobDeclaration>();
				var entry11 = declaration1.CustomsEntryHeaders.AddNew();
				var entry12 = declaration1.CustomsEntryHeaders.AddNew();
				declaration1.JE_JS = shipment1.PK;
				declaration1.JE_UCR = "2GB123456789000-B0001000";
				declaration1.JE_DeclarationReference = "B0001000";
				entry11.CH_BGMReference = "2GB123456789000-B0001000/1";
				entry12.CH_BGMReference = "2GB123456789000-B0001000/2";
				declaration1.JE_LocationOfGoods = "LHR";
				declaration1.SubLocation = "BAC";

				var declaration2 = Factory.New<JobDeclaration>();
				var entry21 = declaration2.CustomsEntryHeaders.AddNew();
				var entry22 = declaration2.CustomsEntryHeaders.AddNew();
				declaration2.JE_JS = shipment2.PK;
				declaration2.JE_UCR = "2GB123456789000-B0002000";
				declaration2.JE_DeclarationReference = "B0002000";
				entry21.CH_BGMReference = "2GB123456789000-B0002000/1";
				entry22.CH_BGMReference = "2GB123456789000-B0002000/2";
				declaration2.JE_LocationOfGoods = "MAN";
				declaration2.SubLocation = "SLS";

				var messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertContains(@"GID+1'RFF+HWB:HOUSE001'RFF+ABO:2GB123456789000-B0001000:1'LOC+14+LHR:145:3:BAC'RFF+ABO:2GB123456789000-B0001000:2'LOC+14+LHR:145:3:BAC'GID+2'RFF+HWB:HOUSE002'RFF+ABO:2GB123456789000-B0002000:1'LOC+14+MAN:145:3:SLS'RFF+ABO:2GB123456789000-B0002000:2'LOC+14+MAN:145:3:SLS'UNT", messageText);
				AssertContains("<h4>Entry 2GB123456789000-B0001000/1 on declaration B0001000</h4>", generator.MessageInterpretation);
				AssertContains("<h4>Entry 2GB123456789000-B0002000/1 on declaration B0002000</h4>", generator.MessageInterpretation);
			}

			public void TestMakeG2g_ConsolExternalDeclaration()
			{
				// We do not support external entry numbers - use DUCR & CAR only
				var ducr1 = shipment1.Numbers.AddNew();
				ducr1.CE_EntryType = "UCR";
				ducr1.CE_EntryNum = "2GB123456789000-EXTERNAL1";
				shipment1.CustomsEntryNumber = "IGNORE1";
				var ducr2 = shipment2.Numbers.AddNew();
				ducr2.CE_EntryType = "UCR";
				ducr2.CE_EntryNum = "2GB123456789000-EXTERNAL2";
				shipment2.CustomsEntryNumber = "IGNORE2";
				var messageText = generator.MakeMessageText();
				AssertEquals(0, errorCollector.ErrorCount);
				AssertNotContains("Manual entry number from shipment form is ignored", "IGNORE", messageText);
				ducr1.CE_EntryLineReference = "EXTCAR4UC1";
				ducr2.CE_EntryLineReference = "EXTCAR4UC2";
				messageText = generator.MakeMessageText();
				AssertContains("Shed and airport from consol, DUCR and DUCR-CAR from shipment's numbers grid", "GID+1'RFF+HWB:HOUSE001'RFF+ABO:2GB123456789000-EXTERNAL1'LOC+14+LHR:145:3:BAC'RFF+ZZZ:EXTCAR4UC1'GID+2'RFF+HWB:HOUSE002'RFF+ABO:2GB123456789000-EXTERNAL2'LOC+14+LHR:145:3:BAC'RFF+ZZZ:EXTCAR4UC2'UNT", messageText);
				AssertNotContains("Manual entry number from shipment form is ignored", "IGNORE", messageText);
			}

			protected override ZString AgentType
			{
				get { return AgentTypeForExportFallbackList.Codes.Type2; }
			}
		}
	}
}
