using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration.Testing
{
	public class NctsTransmissionMessageGeneratorTests : TestCaseWithFactory
	{
		public void TestMakePrettyForInterpretation_15()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Departure office code is GB00000x
			{
				var departure = Factory.New<NctsHeader>();
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				departure = MakeDefaultDepartureForMessagingTest(departure, Factory, addAllMandatoryData: true);
				new NctsMessageManager(departure, new NctsTransmissionMessageGenerator(new NctsMessageFunctionSet.DeclarationDataMessage())).SendNctsMessage(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(1, departure.Messages.Count);
				var message = departure.Messages[0];
				var messageInterpretation = message.EM_MessageInterpretation;
				AssertContains("<tr><td>Message code</td><td>IE015</td></tr><tr><td>Message number</td><td>1</td></tr><tr><td>LRN</td><td>NCT001</td></tr><tr><td>Declarant</td><td>NCTS UK TEST LAB HMCE</td></tr><tr><td>Departing</td><td>GB000060</td></tr><tr><td>Arriving</td><td>IT025100</td></tr></table>", messageInterpretation);
				AssertContains("<h3>Departure Declaration Sent</h3>", messageInterpretation);
			}
		}

		public void TestMakePrettyForInterpretation_14()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Departure office code is GB00000x
			{
				var departure = Factory.New<NctsHeader>();
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				departure = MakeDefaultDepartureForMessagingTest(departure, Factory);
				var mrn = CusEntryNumber.New(departure, CusEntryNumberTypes.Standard.MovementReferenceNumber, departure.Branch.Company.GC_RN_NKCountryCode);
				mrn.CE_EntryNum = "MRN123";
				new NctsMessageManager(departure, new NctsTransmissionMessageGenerator(new NctsMessageFunctionSet.DeclarationCancellationRequestMessage("Test Reason", "Test Comment"))).SendNctsMessage(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(1, departure.Messages.Count);
				var message = departure.Messages[0];
				var messageInterpretation = message.EM_MessageInterpretation;
				AssertContains("<tr><td>Message code</td><td>IE014</td></tr><tr><td>Message number</td><td>1</td></tr><tr><td>Extra Information</td><td>Test Reason</td></tr><tr><td>MRN</td><td>MRN123</td></tr><tr><td>LRN</td><td>NCT001</td></tr><tr><td>Declarant</td><td>NCTS UK TEST LAB HMCE</td></tr><tr><td>Departing</td><td>GB000060</td></tr><tr><td>Arriving</td><td>IT025100</td></tr><tr><td>Comment On Cancellation</td><td>Test Comment</td></tr></table>", messageInterpretation);
				AssertContains("<h3>Cancellation Request Sent</h3>", messageInterpretation);
			}
		}

		public void TestMakePrettyForInterpretation_13()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Departure office code is GB00000x
			{
				var departure = Factory.New<NctsHeader>();
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				departure = MakeDefaultDepartureForMessagingTest(departure, Factory);
				new NctsMessageManager(departure, new NctsTransmissionMessageGenerator(new NctsMessageFunctionSet.DeclarationAmendmentMessage("Test Reason", "Test Comment"))).SendNctsMessage(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(1, departure.Messages.Count);
				var message = departure.Messages[0];
				var messageInterpretation = message.EM_MessageInterpretation;
				AssertContains("<tr><td>Message code</td><td>IE013</td></tr><tr><td>Message number</td><td>1</td></tr><tr><td>LRN</td><td>NCT001</td></tr><tr><td>Declarant</td><td>NCTS UK TEST LAB HMCE</td></tr><tr><td>Departing</td><td>GB000060</td></tr><tr><td>Arriving</td><td>IT025100</td></tr><tr><td>Amend Reason</td><td>Test Reason</td></tr><tr><td>Amend Comment</td><td>Test Comment</td></tr></table>", messageInterpretation);
				AssertContains("<h3>NCTS Message IE013 OK</h3>", messageInterpretation);
			}
		}

		public void TestMakePrettyForInterpretation_7()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Destination office code is GB00000x
			{
				var arrival = MakeDefaultArrivalForMessagingTest(Factory);
				new NctsMessageManager(arrival, new NctsTransmissionMessageGenerator(new NctsMessageFunctionSet.ArrivalNotificationMessage())).SendNctsMessage(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(1, arrival.Messages.Count);
				var message = arrival.Messages[0];
				var messageInterpretation = message.EM_MessageInterpretation;
				AssertContains("<tr><td>Message code</td><td>IE007</td></tr><tr><td>Message number</td><td>1</td></tr><tr><td>MRN</td><td>15GB000060100C7FA0</td></tr><tr><td>LRN</td><td>NCT001</td></tr><tr><td>Declarant</td><td>NCTS UK TEST LAB HMCE</td></tr><tr><td>Arriving</td><td>GB000060</td></tr></table>", messageInterpretation);
				AssertContains("<h3>Arrival Notification Sent</h3>", messageInterpretation);
			}
		}

		public void TestMakePrettyForInterpretation_44()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Destination office code is GB00000x
			{
				var arrival = MakeDefaultUnloadingRemarksForMessagingTest(Factory);
				arrival.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
				new NctsMessageManager(arrival, new NctsTransmissionMessageGenerator(new NctsMessageFunctionSet.UnloadingRemarksMessage())).SendNctsMessage(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(1, arrival.Messages.Count);
				var message = arrival.Messages[0];
				var messageInterpretation = message.EM_MessageInterpretation;
				AssertContains("<tr><td>Message code</td><td>IE044</td></tr><tr><td>Message number</td><td>1</td></tr><tr><td>MRN</td><td>15GB000060100C7FA0</td></tr><tr><td>LRN</td><td>NCT001</td></tr><tr><td>Declarant</td><td>NCTS UK TEST LAB HMCE</td></tr><tr><td>Arriving</td><td>GB000060</td></tr></table>", messageInterpretation);
				AssertContains("<h3>Unloading Remarks Sent</h3>", messageInterpretation);
			}
		}

		public void TestDemandEoriDuringMessageGeneration()
		{
			var departure = Factory.New<NctsHeader>();
			var generator = new NctsTransmissionMessageGenerator(new NctsMessageFunctionSet.DeclarationDataMessage());
			var result = generator.Generate(departure);
			AssertContains("EORI for declarant is mandatory", result.Errors[0]);
		}

		public void TestFlipOutAllPlaceholders()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Departure office code is GB00000x
			{
				var departure = Factory.New<NctsHeader>();
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				departure = MakeDefaultDepartureForMessagingTest(departure, Factory, addAllMandatoryData: true);
				var office = departure.MovementHeader.CustomsOffices.AddNew();
				office.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				office.CY_Data = "GB000060";
				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", departure.Principal, suffix: "", traderTin: "123456789012");
				var generator = new NctsTransmissionMessageGenerator(new NctsMessageFunctionSet.DeclarationDataMessage());
				var messageManager = new NctsMessageManager(departure, generator);
				var shutUp = new SendsMessagesToCustomsShutterUpperer();
				messageManager.SendNctsMessage(shutUp);
				AssertEquals(1, departure.Messages.Count);
				var message = departure.Messages[0];
				var messageText = message.EM_MessageText;
				var cleanPkOfMessage = NctsTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(message);
				AssertNotContains("<<", messageText);
				AssertContains("<CC015B>", messageText);
				AssertContains(cleanPkOfMessage, messageText);
			}
		}

		public void TestMessageGenerationForDepartureWithoutExpressSave()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var msg = RunTestMessageGenerationForDeparture(false);
				AssertContains("LRN is put into message", "<RefNumHEA4>NCT001</RefNumHEA4>", msg.EM_MessageText);
			}
		}

		public void TestMessageGenerationForDeparture()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var msg = RunTestMessageGenerationForDeparture();
				AssertContains("LRN is put into message", "<RefNumHEA4>NCT001</RefNumHEA4>", msg.EM_MessageText);
			}
		}

		public void TestMessageGenerationForDepartureFromXIOffice()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var msg = RunTestMessageGenerationForDeparture(departureOffice: "XI000060");
				AssertContains("XI Office is put into message", "<RefNumEST1>IT025100</RefNumEST1>", msg.EM_MessageText);
			}
		}

		public void TestMessageGenerationForArrivalToXIOffice()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var arrival = MakeDefaultArrivalForMessagingTest(Factory, destinationOffice: "XI000060");
				Factory.Save();

				var generator = new NctsTransmissionMessageGenerator(new NctsMessageFunctionSet.ArrivalNotificationMessage());
				var messageManager = new NctsMessageManager(arrival, generator);
				var shutUp = new SendsMessagesToCustomsShutterUpperer();
				messageManager.SendNctsMessage(shutUp);
				AssertEquals(1, arrival.Messages.Count);
				var msg = arrival.Messages[0];
				AssertContains("XI Office is put into message", "<RefNumRES1>XI000060</RefNumRES1>", msg.EM_MessageText);
			}
		}

		public void TestMessageGenerationForArrival()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Destination office code is GB00000x
			{
				var arrival = MakeDefaultArrivalForMessagingTest(Factory);
				Factory.Save();

				var generator = new NctsTransmissionMessageGenerator(new NctsMessageFunctionSet.ArrivalNotificationMessage());
				var messageManager = new NctsMessageManager(arrival, generator);
				var shutUp = new SendsMessagesToCustomsShutterUpperer();
				messageManager.SendNctsMessage(shutUp);
				AssertEquals(1, arrival.Messages.Count);
				AssertMessageGeneration(arrival, "007", NctsMessageStatusList.Codes.ArrivalNotificationSent);
			}
		}

		public void TestInterchangeGeneration()
		{
			var messageCollection = new NonDependentEDIMessageCollection(Factory);
			var message = messageCollection.AddNew();
			message.EM_MessageOwner = "GB123456789012";
			var interchangeProvider = new NctsInterchangeProvider(messageCollection);
			var interchange = interchangeProvider.Interchanges[0];
			AssertEquals("NCT", interchange.EI_ApplicationCode);
			AssertEquals("Interchange sender should be message owner", "GB123456789012", interchange.EI_From);
			AssertContains("UNB+UNOC:3+GB123456789012+NCTS+", interchange.EI_HeaderText);
			AssertEquals("NCTS", interchange.EI_To);
		}

		public void TestNewTransactionManagementWhenDepartureMessageIsSaved_WithAmountGreaterThan0()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.Latvia);

			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, Core.Constants.CountryCodes.Latvia);

			var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Number = "19860101";
			guaranteeHeader1.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader1.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader1.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader1.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			var transaction1 = guaranteeHeader1.CusGuaranteeLineTransactions.AddNew();
			transaction1.CPL_Reference = "Entry Number";
			transaction1.CPL_TranValue = 100m;
			transaction1.CPL_TransactionType = PermitTransactionTypeList.Codes.CUS;
			transaction1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			transaction1.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			transaction1.CPL_AppId = "Entry Reference";
			transaction1.CPL_Comment = "Instruction Desc.";
			transaction1.CPL_Procedure = "AAA";

			NCTSTestHelper.SetUpTariff(Factory);

			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Departure office code is GB00000x
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header = MakeDefaultDepartureForMessagingTest(header, Factory, addAllMandatoryData: true);
				header.Principal.E2_OA_Address = org1.MainAddress.PK;
				header.Declarant.E2_OA_Address = org1.MainAddress.PK;

				var guarantee1 = header.Guarantees.AddNew();
				guarantee1.PW_BondAmount = 10m;
				guarantee1.PW_BondNumber = "19860101";

				header.ApportionedAmountToGuaranteesLiabilityAmount();
				AssertEquals(10m, guarantee1.PW_BondAmount);
				Factory.Save();

				var generator = new NctsTransmissionMessageGenerator(new NctsMessageFunctionSet.DeclarationDataMessage());
				var messageManager = new NctsMessageManager(header, generator);
				var shutUp = new SendsMessagesToCustomsShutterUpperer();

				CombineAssertions(() =>
				{
					AssertEquals("Initial", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
					messageManager.SendNctsMessage(shutUp);
					AssertEquals("Ncts Message Sent", 2, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);

					var transactions = guarantee1.CusGuarantee.CusGuaranteeLineTransactions;
					var transaction = transactions[1];

					AssertEquals("Transaction Reference", header.BH_JobReference, transaction.CPL_Reference);
					AssertEquals("Transaction Comment", "NCTS departure " + header.LocalReferenceNumber, transaction.CPL_Comment);
					AssertEquals("Transaction Category", PermitTransactionCategoryList.Codes.CUM, transaction.CPL_TransactionCategory);
					AssertEquals("Transaction Type", PermitTransactionTypeList.Codes.TRA, transaction.CPL_TransactionType);
					AssertEquals("Transaction Value", -guarantee1.PW_BondAmount, transaction.CPL_TranValue);
					AssertEquals("Transaction ID", "1", transaction.CPL_AppId);
					AssertEquals("Transaction Status", PermitTransactionStatusList.Codes.Pending, transaction.CPL_TransactionStatus);
					AssertEquals("Transaction Reference Line No.", 0, transaction.CPL_ReferenceNumberLine);
				});
			}
		}

		public void TestNewTransactionManagementWhenDepartureMessageIsSaved_WithAmountLesserThan0()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.Latvia);

			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Number = "19860101";
			guaranteeHeader1.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader1.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader1.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader1.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			Factory.Save();

			NCTSTestHelper.SetUpTariff(Factory, Core.Constants.CountryCodes.Italy);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Departure office code is GB00000x
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header = MakeDefaultDepartureForMessagingTest(header, Factory);
				header.Principal.E2_OA_Address = org1.MainAddress.PK;
				header.Declarant.E2_OA_Address = org1.MainAddress.PK;

				var guarantee1 = header.Guarantees.AddNew();
				guarantee1.PW_BondAmount = 0m;
				guarantee1.PW_BondNumber = "19860101";

				header.ApportionedAmountToGuaranteesLiabilityAmount();
				AssertEquals(0m, guarantee1.PW_BondAmount);
				Factory.Save();

				var generator = new NctsTransmissionMessageGenerator(new NctsMessageFunctionSet.DeclarationDataMessage());
				var messageManager = new NctsMessageManager(header, generator);
				var shutUp = new SendsMessagesToCustomsShutterUpperer();

				Assert(guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count == 0);
				messageManager.SendNctsMessage(shutUp);
				Assert(guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count == 0);
			}
		}

		public static void SetupGBPassword(BusinessObjectFactory factory)
		{
			var pw = factory.New<MasterFiles.Integration.Customs.GB.IGlbExternalPassword_GB>();
			pw.GP_GC = GlbCompany.CurrentCompany.PK.ToGuid();
			pw.GP_ExpiryDate = ZDateTime.Today.AddDays(7);
			pw.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			pw.IsTokenForNCTS = true;
		}

		public static NctsHeader MakeDefaultDepartureForMessagingTest(NctsHeader departure, BusinessObjectFactory factory, bool initialiseGoodsItem = true, string principalEori = "GB954131533000", bool addAllMandatoryData = false, string departureOffice = "GB000060")
		{
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = departure.MovementHeader;
			if (addAllMandatoryData)
			{
				NCTSTestHelper.CreateJobDocAddressForTest(factory, "CE1", departure.Consignee, "3");
				NCTSTestHelper.CreateJobDocAddressForTest(factory, "CE1", departure.Consignor, "3");
				departure.BH_FTZMove = false;
				movementHeader.BM_AdditionalText = "1234";
			}
			departure.BH_JobReference = "NCT001";
			movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			NCTSTestHelper.CreateJobDocAddressForTest(factory, "PC1", departure.Principal, "", "NCTS UK TEST LAB HMCE", "11TH FLOOR, ALEX HOUSE, VICTORIA AV", "SS99 1AA", "SOUTHEND-ON-SEA, ESSEX", "GBSOU", "GB", principalEori);
			departure.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.UnitedKingdom;
			movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
			NCTSTestHelper.CreateCustomsOfficeForTest(departure, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, departureOffice, ZDateTime.Empty, false);
			NCTSTestHelper.CreateCustomsOfficeForTest(departure, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "IT025100", ZDateTime.Empty, false);
			NCTSTestHelper.CreateGuaranteeForTest(departure, "1", "09GB00000100000M0", "", "AC01", "");
			var goodsItem = movementHeader.GoodsItems.AddNew();
			if (initialiseGoodsItem)
			{
				goodsItem.BY_Description = "XXX";
				goodsItem.BY_GrossWeight = 123;
				goodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
				NCTSTestHelper.SetupPackageForTest(goodsItem, "MARK1", "BX", 30);
			}
			return departure;
		}

		public static NctsHeader MakeDefaultArrivalForMessagingTest(BusinessObjectFactory factory, string destinationOffice = "GB000060")
		{
			var arrival = factory.New<NctsHeader>();
			arrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			arrival.SetMovementType(NctsMovementType.Codes.Arrival);
			arrival.BH_JobReference = "NCT001";
			var mrn = CusEntryNumber.New(arrival, CusEntryNumberTypes.Standard.MovementReferenceNumber, arrival.Branch.Company.GC_RN_NKCountryCode);
			mrn.CE_EntryNum = "MRN123";
			NCTSTestHelper.CreateJobDocAddressForTest(factory, "TRD", arrival.DestinationTrader, "", "NCTS UK TEST LAB HMCE", "11TH FLOOR, ALEX HOUSE, VICTORIA AV", "SS99 1AA", "SOUTHEND-ON-SEA, ESSEX", "GBSOU", "GB", "GB954131533000");
			NCTSTestHelper.CreateCustomsOfficeForTest(arrival, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival, destinationOffice, ZDateTime.Empty);
			arrival.ArrivalMrnFromUser = "15GB000060100C7FA0";
			arrival.ArrivalMovementHeader.GoodsItems.AddNew();
			return arrival;
		}

		public static NctsHeader MakeDefaultUnloadingRemarksForMessagingTest(BusinessObjectFactory factory)
		{
			var arrival = MakeDefaultArrivalForMessagingTest(factory);
			arrival.UnloadingMovementHeader.ResetUnloadedGoodsItem(arrival.ArrivalMovementHeader.GoodsItems);
			arrival.UnloadingRemark.G9_Conform = YesNoList.Codes.Yes;
			arrival.UnloadingRemark.G9_UnloadingCompletion = YesNoList.Codes.Yes;
			arrival.UnloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.Yes;
			arrival.UnloadingRemark.G9_UnloadingDate = ZDateTime.Today;
			var unloadedGoodsItem1 = arrival.UnloadingMovementHeader.GoodsItems.AddNew();
			unloadedGoodsItem1.IsChecked = true;
			return arrival;
		}

		EDIMessage RunTestMessageGenerationForDeparture(bool saveFirst = true, string departureOffice = "GB000060")
		{
			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure = MakeDefaultDepartureForMessagingTest(departure, Factory, departureOffice: departureOffice, addAllMandatoryData: true);
			if (saveFirst)
			{
				Factory.Save();
			}
			var generator = new NctsTransmissionMessageGenerator(new NctsMessageFunctionSet.DeclarationDataMessage());
			var messageManager = new NctsMessageManager(departure, generator);
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			messageManager.SendNctsMessage(shutUp);
			AssertEquals(1, departure.Messages.Count);
			return AssertMessageGeneration(departure, "015", NctsMessageStatusList.Codes.DepartureDeclarationSent);
		}

		EDIMessage AssertMessageGeneration(NctsHeader nctsHeader, string messageId, string messageStatus)
		{
			var message = nctsHeader.Messages[0];
			AssertEquals("NCT", message.EM_ApplicationCode);
			AssertEquals("GB954131533000", message.EM_MessageOwner);
			AssertStartsWith("", "<CC" + messageId, message.EM_MessageText);
			AssertEquals("GB", message.EM_MessageType);
			AssertEquals(messageId, message.EM_MessageSubType);
			AssertEquals("", message.EM_ApplicationReference);
			AssertNotNull((NctsHeader)message.EM_LinkedObject);
			AssertEquals("QUE", message.EM_Status);
			AssertEquals("TRX", message.EM_ReceiveTransmit);
			AssertEquals(messageStatus, nctsHeader.EffectiveMessageStatus);
			return message;
		}
	}
}
