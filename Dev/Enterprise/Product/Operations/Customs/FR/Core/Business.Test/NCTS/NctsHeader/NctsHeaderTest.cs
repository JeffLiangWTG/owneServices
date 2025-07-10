using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs.EU.NCTS;
using NctsEuOfficeCode = Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode;
using NctsMovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsHeader))]
	class NctsHeaderTest : NctsHeaderAbstractTest
	{
		public void TestUpdateDestinationOffice()
		{
			CombineAssertions(() =>
			{
				AssertUpdatingDestinationOffice("No defaulting is expected when Declarant and Destination Trader are empty.",
					ZString.Empty, isPhase4: true, isSimplifiedProcedure: true, declarantExists: false, destinationExists: false, hasDeclarantAceAuthorisation: false, authorisationMatchesDestinationAddress: false, authorisationHasOfficeRule: false);
				AssertUpdatingDestinationOffice("No defaulting is expected when Destination Trader is Empty.",
					ZString.Empty, isPhase4: true, isSimplifiedProcedure: true, declarantExists: true, destinationExists: false, hasDeclarantAceAuthorisation: false, authorisationMatchesDestinationAddress: false, authorisationHasOfficeRule: false);
				AssertUpdatingDestinationOffice("No defaulting is expected when Declarant is empty.",
					ZString.Empty, isPhase4: true, isSimplifiedProcedure: true, declarantExists: false, destinationExists: true, hasDeclarantAceAuthorisation: false, authorisationMatchesDestinationAddress: false, authorisationHasOfficeRule: false);
				AssertUpdatingDestinationOffice("No defaulting is expected when Declarant has no ACE authorisation.",
					ZString.Empty, isPhase4: true, isSimplifiedProcedure: true, declarantExists: true, destinationExists: true, hasDeclarantAceAuthorisation: false, authorisationMatchesDestinationAddress: false, authorisationHasOfficeRule: false);
				AssertUpdatingDestinationOffice("No defaulting is expected when Declarant has an ACE authorisation but it doesn't match Destination Trader Address.",
					ZString.Empty, isPhase4: true, isSimplifiedProcedure: true, declarantExists: true, destinationExists: true, hasDeclarantAceAuthorisation: true, authorisationMatchesDestinationAddress: false, authorisationHasOfficeRule: false);
				AssertUpdatingDestinationOffice("No defaulting is expected when Declarant has an ACE authorisation matching Destination Trader address, but it doesn't have an OFC rule.",
					ZString.Empty, isPhase4: true, isSimplifiedProcedure: true, declarantExists: true, destinationExists: true, hasDeclarantAceAuthorisation: true, authorisationMatchesDestinationAddress: true, authorisationHasOfficeRule: false);
				AssertUpdatingDestinationOffice("Departure Customs office should be defaulted with OFC rule value of Declarant ACE authorisation matching Destination Trader address.", "FR003333",
					isPhase4: true, isSimplifiedProcedure: true, declarantExists: true, destinationExists: true, hasDeclarantAceAuthorisation: true, authorisationMatchesDestinationAddress: true, authorisationHasOfficeRule: true);
				AssertUpdatingDestinationOffice("No defaulting is expected when declaration is not Phase4.",
					ZString.Empty, isPhase4: false, isSimplifiedProcedure: true, declarantExists: true, destinationExists: true, hasDeclarantAceAuthorisation: true, authorisationMatchesDestinationAddress: true, authorisationHasOfficeRule: true);
				AssertUpdatingDestinationOffice("No defaulting is expected when declaration is not simplified.",
					ZString.Empty, isPhase4: true, isSimplifiedProcedure: false, declarantExists: true, destinationExists: true, hasDeclarantAceAuthorisation: true, authorisationMatchesDestinationAddress: true, authorisationHasOfficeRule: true);
			});

			void AssertUpdatingDestinationOffice(string comment, string expectedDestinationOffice, bool isPhase4, bool isSimplifiedProcedure, bool declarantExists, bool destinationExists, bool hasDeclarantAceAuthorisation, bool authorisationMatchesDestinationAddress, bool authorisationHasOfficeRule)
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.BH_ApplicationCode = isPhase4 ? CusInBondApplicationCodeList.Codes.NCTS4 : CusInBondApplicationCodeList.Codes.NCTS5;
				header.ArrivalMovementHeader.IsSimplifiedNctsProcedure = isSimplifiedProcedure;

				if (destinationExists)
				{
					var destination = Factory.NewWithValidTestData<OrgHeader>();
					header.DestinationTrader.E2_OA_Address = destination.MainAddress.PK;
				}

				if (declarantExists)
				{
					var declarant = Factory.NewWithValidTestData<OrgHeader>();
					if (hasDeclarantAceAuthorisation)
					{
						var address = !destinationExists || !authorisationMatchesDestinationAddress ? declarant.MainAddress : header.DestinationTrader.Address;
						var authorisation = declarant.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit).WithNumber("DECLARANT_AUT").WithCountry(header.CountryCode);
						authorisation.CPH_OA_AppliesTo = address.PK;
						authorisation.CPH_StartDate = ZDate.Today.AddDays(-1);

						if (authorisationHasOfficeRule)
						{
							var declarantAuthorisationRule = authorisation.CusAuthorisationRules.AddNew();
							declarantAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.OFC;
							declarantAuthorisationRule.CPR_ValueFrom = "FR003333";
						}
					}

					header.Declarant.E2_OA_Address = declarant.MainAddress.PK;
				}

				AssertEquals(comment, expectedDestinationOffice, header.DestinationCustomsOfficeCodeForArrival);
			}
		}

		public void TestDefaultingOfDepartureCustomsOffice()
		{
			CombineAssertions(() =>
			{
				AssertDefaultingOfDepartureCustomsOffice("No defaulting is expected when Declarant and Consignor are empty.", ZString.Empty, isPhase4: true, isSimplifiedProcedure: true, isDeclarantEmpty: true, isConsignorEmpty: true);
				AssertDefaultingOfDepartureCustomsOffice("No defaulting is expected when Consignor is Empty.", ZString.Empty, isPhase4: true, isSimplifiedProcedure: true, isDeclarantEmpty: false, isConsignorEmpty: true);
				AssertDefaultingOfDepartureCustomsOffice("In case Declarant is empty, departure Customs office should be defaulted to Consignor CTR number if able.", "FR001111", isPhase4: true, isSimplifiedProcedure: true, isDeclarantEmpty: true, isConsignorEmpty: false);
				AssertDefaultingOfDepartureCustomsOffice("Departure Customs office should be defaulted to Consignor CTR number if able when Declarant has no ACR authorisation.", "FR001111", isPhase4: true, isSimplifiedProcedure: true, isDeclarantEmpty: false, isConsignorEmpty: false);
				AssertDefaultingOfDepartureCustomsOffice("Departure Customs office should be defaulted to Consignor CTR number if able when Declarant has an ACR authorisation but it doesn't match Consignor Address.", "FR001111", isPhase4: true, isSimplifiedProcedure: true, isDeclarantEmpty: false, isConsignorEmpty: false, hasDeclarantAcrAuthorisation: true, authorisationMatchesConsignorAddress: false);
				AssertDefaultingOfDepartureCustomsOffice("Departure Customs office should be defaulted to Consignor CTR number if able when Declarant has an ACR authorisation matching Consignor address, but it doesn't have an OFC rule.", "FR001111", isPhase4: true, isSimplifiedProcedure: true, isDeclarantEmpty: false, isConsignorEmpty: false, hasDeclarantAcrAuthorisation: true, authorisationMatchesConsignorAddress: true, authorisationHasOfficeRule: false);
				AssertDefaultingOfDepartureCustomsOffice("Departure Customs office should be defaulted with OFC rule value of Declarant ACR authorisation matching Consignor address.", "FR003333", isPhase4: true, isSimplifiedProcedure: true, isDeclarantEmpty: false, isConsignorEmpty: false, hasDeclarantAcrAuthorisation: true, authorisationMatchesConsignorAddress: true, authorisationHasOfficeRule: true);
				AssertDefaultingOfDepartureCustomsOffice("No defaulting is expected when declaration is not Phase4.", ZString.Empty, isPhase4: false, isSimplifiedProcedure: true, isDeclarantEmpty: false, isConsignorEmpty: false, hasDeclarantAcrAuthorisation: true, authorisationMatchesConsignorAddress: true, authorisationHasOfficeRule: true);
				AssertDefaultingOfDepartureCustomsOffice("Departure Customs office should be defaulted to Consignor CTR number if able when declaration is not simplified.", "FR001111", isPhase4: true, isSimplifiedProcedure: false, isDeclarantEmpty: false, isConsignorEmpty: false, hasDeclarantAcrAuthorisation: true, authorisationMatchesConsignorAddress: true, authorisationHasOfficeRule: true);
			});

			void AssertDefaultingOfDepartureCustomsOffice(string comment, string expectedDepartureCustomsoffice, bool isPhase4, bool isSimplifiedProcedure, bool isDeclarantEmpty, bool isConsignorEmpty, bool hasDeclarantAcrAuthorisation = false, bool authorisationMatchesConsignorAddress = false, bool authorisationHasOfficeRule = false)
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				header.BH_ApplicationCode = isPhase4 ? CusInBondApplicationCodeList.Codes.NCTS4 : CusInBondApplicationCodeList.Codes.NCTS5;
				if (isPhase4)
				{
					header.CustomsOfficesForDeparture.AddNew("DEP");
				}

				header.MovementHeader.BM_GONumber = isSimplifiedProcedure ? "A3" : ZString.Empty;

				if (!isConsignorEmpty)
				{
					var consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsOfficeForTransit, "FR001111", Enterprise.Core.Constants.CountryCodes.France);
					header.Consignor.E2_OA_Address = consignor.MainAddress.PK;
				}

				if (!isDeclarantEmpty)
				{
					var declarant = Factory.NewWithValidTestData<OrgHeader>();
					if (hasDeclarantAcrAuthorisation)
					{
						var address = isConsignorEmpty || !authorisationMatchesConsignorAddress ? declarant.MainAddress : header.Consignor.Address;
						var authorisation = declarant.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit).WithNumber("DECLARANT_AUT").WithCountry(header.CountryCode);
						authorisation.CPH_OA_AppliesTo = address.PK;
						authorisation.CPH_StartDate = ZDate.Today.AddDays(-1);

						if (authorisationHasOfficeRule)
						{
							var declarantAuthorisationRule = authorisation.CusAuthorisationRules.AddNew();
							declarantAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.OFC;
							declarantAuthorisationRule.CPR_ValueFrom = "FR003333";
						}
					}

					header.Declarant.E2_OA_Address = declarant.MainAddress.PK;
				}

				AssertEquals(comment, expectedDepartureCustomsoffice, header.DepartureCustomsOfficeCode);
			}
		}

		public void TestIsArrivalTabReadOnly_ArrivalDetailedStatus()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.DetailedArrivalStatusCode = ZString.Empty;
			AssertEquals($"Movement should be editable when its Customs Status allows for it as long as arrival detailed status is not PNR.", false, header.IsArrivalTabReadOnly);

			header.DetailedArrivalStatusCode = NctsDetailedStatusList.Codes.PreArrivalNotificationRequest;
			AssertEquals($"Movement should not be editable when arrival detailed status is PNR, even when its Customs Status would normaly not allow for it.", true, header.IsArrivalTabReadOnly);
		}

		public void TestLocalReferenceNumberReadOnly()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			CombineAssertions("When ncts header type is DA, and Customs Status is not DRJ", () =>
			{
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;

				AssertEquals("Pre-requisite: BH_HeaderType", "DA", nctsHeader.BH_HeaderType);
				AssertEquals("Pre-requisite: MessageCount", 0, nctsHeader.Messages.Count);
				AssertEquals("Pre-requisite: MovementReferenceNumber", "", nctsHeader.MovementReferenceNumber);
				AssertNotEquals("Pre-requisite: CustomsStatus", "DRJ", nctsHeader.MovementHeader.BM_CustomsStatus);

				Assert("NctsHeader LRN is readonly", nctsHeader.LocalReferenceNumberReadOnly);
			});

			CombineAssertions("When ncts header Messages count greater than 0, and Customs Status is not DRJ", () =>
			{
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
				nctsHeader.Messages.AddNew();

				AssertEquals("Pre-requisite: BH_HeaderType", "D", nctsHeader.BH_HeaderType);
				AssertEquals("Pre-requisite: MessageCount", 1, nctsHeader.Messages.Count);
				AssertEquals("Pre-requisite: MovementReferenceNumber", "", nctsHeader.MovementReferenceNumber);
				AssertNotEquals("Pre-requisite: CustomsStatus", "DRJ", nctsHeader.MovementHeader.BM_CustomsStatus);

				Assert("NctsHeader LRN is readonly", nctsHeader.LocalReferenceNumberReadOnly);
			});

			CombineAssertions("When ncts header type is D, and MovementReferenceNumber is not empty, and Customs Status is not DRJ", () =>
			{
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
				nctsHeader.Messages.RemoveAndDeleteAll();
				var mrn = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);
				mrn.CE_EntryNum = "MRN123";

				AssertEquals("Pre-requisite: BH_HeaderType", "D", nctsHeader.BH_HeaderType);
				AssertEquals("Pre-requisite: MessageCount", 0, nctsHeader.Messages.Count);
				AssertEquals("Pre-requisite: MovementReferenceNumber", "MRN123", nctsHeader.MovementReferenceNumber);
				AssertNotEquals("Pre-requisite: CustomsStatus", "DRJ", nctsHeader.MovementHeader.BM_CustomsStatus);

				Assert("NctsHeader LRN is readonly", nctsHeader.LocalReferenceNumberReadOnly);
			});

			CombineAssertions("When ncts header Customs Status is DRJ", () =>
			{
				nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationRejected;
				AssertEquals("Pre-requisite: CustomsStatus", "DRJ", nctsHeader.MovementHeader.BM_CustomsStatus);

				Assert("NctsHeader LRN is not readonly", !nctsHeader.LocalReferenceNumberReadOnly);
			});
		}

		public void TestShipmentSynchroniser_Phase4()
		{
			var nctsHeader = Factory.New<NctsHeaderforTest>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var shipmentParent = Factory.New<ForwardingShipment>();
			nctsHeader.BH_ParentID = shipmentParent.PK;
			nctsHeader.BH_ParentTableCode = shipmentParent.TablePrefix;
			AssertType<NctsShipmentSynchroniser>(nctsHeader.Synchroniser);
		}

		public void TestConsolSynchroniser_Phase4()
		{
			var nctsHeader = Factory.New<NctsHeaderforTest>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var consolParent = Factory.New<ForwardingConsol>();
			nctsHeader.BH_ParentID = consolParent.PK;
			nctsHeader.BH_ParentTableCode = consolParent.TablePrefix;
			AssertType<NctsConsolSynchroniser>(nctsHeader.Synchroniser);
		}

		public void TestGetValueSetStrategy()
		{
			var nctsHeader = Factory.New<NctsHeaderforTest>();
			AssertType<NctsHeaderPhase4ValueSetStrategy>("GetValueSetStrategy()", nctsHeader.GetValueSetStrategyExposed());
		}

		public void TestCreateNCTSArrivalNotificationMessageProcessor_Phase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var supporter = nctsHeader as INCTSAutoSendingMessageSupporter;
			AssertType<FRSendNCTSArrivalNotificationMessageProcessor>(supporter.CreateNCTSArrivalNotificationMessageProcessor());
		}

		public void TestCreateNCTSMessageProcessor_Phase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var supporter = nctsHeader as INCTSAutoSendingMessageSupporter;
			AssertType<FRSendNCTSMessageProcessor>(supporter.CreateNCTSMessageProcessor());
		}

		public void TestCreateNCTSArrivalNotificationMessageProcessor_Phase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var supporter = nctsHeader as INCTSAutoSendingMessageSupporter;
			AssertType<AutoSendNCTSP5MessageProcessor>(supporter.CreateNCTSArrivalNotificationMessageProcessor());
		}

		public void TestCreateNCTSMessageProcessor_Phase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var supporter = nctsHeader as INCTSAutoSendingMessageSupporter;
			AssertType<AutoSendNCTSP5MessageProcessor>(supporter.CreateNCTSMessageProcessor());
		}

		public void TestDocumentSupporter()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			AssertNotNull(nameof(NctsHeader.DocumentSupporter), nctsHeader.DocumentSupporter);
			AssertType<NctsHeaderDocumentSupporter>($"{nameof(NctsHeader.DocumentSupporter)} type", nctsHeader.DocumentSupporter);
		}

		public void TestVisualizableDocumentsSupportable()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			var supporter = nctsHeader.GetSupporter();
			AssertType<NctsHeaderVisualizableDocumentSupporter>(supporter);
			AssertEquals("Enterprise.Customs.FR.Business.Documents.NctsHeaderVisualizableDocumentSupporter", supporter.GetType().FullName);
		}

		public void TestMessages()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			AssertType<FREDIMessageCollection>((nctsHeader as IFRMessagesOwner).Messages);
		}

		[TestDate(2021, 06, 12, 14, 26, 00)]
		public void TestFallbackInformation()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			AssertEquals(ZString.Empty, nctsHeader.FallbackInformation);

			var fallbackSetting = new FallbackSettings
			{
				Start = ZDateTime.Today.AddDays(-1),
				End = ZDateTime.Today.AddDays(1)
			};
			using (FRCustomsDataRegistry.Instance.DeltaTMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting))
			{
				nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = false;
				AssertEquals(@"PLAN DE CONTINUITÉ DES OPÉRATIONS
TRANSIT DE L’UNION/TRANSIT COMMUN
AUCUNE DONNÉE DISPONIBLE DANS LE 
SYSTÈME
ENGAGÉE LE 2021-06-12/14:26", nctsHeader.FallbackInformation);

				nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
				nctsHeader.BH_JobReference = "NCT000028";
				var consignor = Factory.New<OrgHeader>();
				consignor.OH_FullName = "Winterfell";
				consignor.SetupCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789");
				consignor.SetupCusCode(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001");
				nctsHeader.Consignor.E2_OA_Address = consignor.MainAddress.PK;

				var customOffice = nctsHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture);
				customOffice.CY_Data = "FR002300";

				AssertEquals(@"PLAN DE CONTINUITÉ DES OPÉRATIONS
TRANSIT DE L’UNION/TRANSIT COMMUN
AUCUNE DONNÉE DISPONIBLE DANS LE 
SYSTÈME
ENGAGÉE LE 2021-06-12/14:26", nctsHeader.FallbackInformation);
			}
		}

		public void TestGuarantees_Phase5Departure()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.Guarantees.AddNew();
				var result = nctsHeader.MovementHeader.Guarantees;
				AssertEquals("Parent", nctsHeader.MovementHeader.PK, result[0].PW_ParentID);
				AssertType<NctsGuaranteeCollection<FRNctsGuarantee>>("Type", result);
			});
		}

		public void TestGuarantees_Phase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
			CombineAssertions(() =>
			{
				nctsHeader.Guarantees.AddNew();
				var result = nctsHeader.Guarantees;
				AssertEquals("Parent", nctsHeader.PK, result[0].PW_ParentID);
				AssertType<NctsGuaranteeCollection<FRNctsGuarantee>>("Type", result);
			});
		}

		public void TestHeaderContainers()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			AssertType<NctsDepartureHeaderContainerCollection<FRNctsDepartureHeaderContainer, NctsHeader>>(nctsHeader.DepartureHeaderContainers);
		}

		public void TestIsDepartureAmendmentAllowed()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			Assert(!nctsHeader.IsDepartureAmendmentAllowed);

			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			nctsHeader.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.Anticipated;
			Assert(nctsHeader.IsDepartureAmendmentAllowed);

			nctsHeader.MovementHeader.BM_CustomsStatus = ZString.Empty;
			Assert(!nctsHeader.IsDepartureAmendmentAllowed);

			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			nctsHeader.DetailedDepartureStatusCode = ZString.Empty;
			Assert(!nctsHeader.IsDepartureAmendmentAllowed);

			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
			Assert(nctsHeader.IsDepartureAmendmentAllowed);

			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
			Assert(nctsHeader.IsDepartureAmendmentAllowed);

			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsNotReleasedForTransit;
			Assert(nctsHeader.IsDepartureAmendmentAllowed);

			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationCancelled;
			Assert(!nctsHeader.IsDepartureAmendmentAllowed);
		}

		public void TestIsDepartureCancellationAllowed()
		{
			var nctsHeader = GetNewBusinessObjectPhase5(Factory);
			var departureMovement = nctsHeader.MovementHeader;

			var fallbackSettings = new FallbackSettings();
			fallbackSettings.Start = ZDateTime.Empty;
			Assert("Prerequisite: Fallback procedure is inactive", !FRCustomsDataRegistry.DeltaTFallbackIsActive);

			departureMovement.BM_CustomsStatus = ZString.Empty;
			Assert(!nctsHeader.IsDepartureCancellationAllowed);

			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			Assert(nctsHeader.IsDepartureCancellationAllowed);

			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
			Assert(nctsHeader.IsDepartureCancellationAllowed);

			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
			Assert(nctsHeader.IsDepartureCancellationAllowed);

			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
			Assert(nctsHeader.IsDepartureCancellationAllowed);

			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsNotReleasedForTransit;
			Assert(nctsHeader.IsDepartureCancellationAllowed);

			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
			Assert(nctsHeader.IsDepartureCancellationAllowed);

			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
			Assert(nctsHeader.IsDepartureCancellationAllowed);

			fallbackSettings.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaTMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSettings);
			Assert("Prerequisite: Fallback procedure is active", FRCustomsDataRegistry.DeltaTFallbackIsActive);

			Assert("Cancellation is not allowed for fallback procedure.", !nctsHeader.IsDepartureCancellationAllowed);
		}

		public void TestGetOutgoingMessage()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			var interchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			interchange1.EI_InterchangeNum = "423";
			var message1 = nctsHeader.Messages.AddNew();
			message1.EM_EI = interchange1.PK;
			message1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;

			var interchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			interchange2.EI_InterchangeNum = "424";
			var message2 = nctsHeader.Messages.AddNew();
			message2.EM_EI = interchange2.PK;
			message2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;

			var interchange3 = Factory.NewWithValidTestData<EDIInterchange>();
			interchange3.EI_InterchangeNum = "424.";
			var message3 = nctsHeader.Messages.AddNew();
			message3.EM_EI = interchange3.PK;
			message3.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			AssertEquals("Prerequisite: Declaration should be Phase4.", true, nctsHeader.IsPhase4);
			AssertSame("Message should be linked to declaration header.", message2.EM_LinkedObject, nctsHeader);
			AssertEquals("GetOutgoingMessage should return the message (message2) matching the interchange number 424.", message2, nctsHeader.GetOutgoingMessage(message3));

			nctsHeader = GetNewBusinessObjectPhase5(Factory);
			var movementHeader = nctsHeader.MovementHeader;

			var interchange4 = Factory.NewWithValidTestData<EDIInterchange>();
			interchange4.EI_InterchangeNum = "425";

			var message4 = movementHeader.Messages.AddNew();
			message4.EM_EI = interchange4.PK;
			message4.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;

			var message5 = movementHeader.Messages.AddNew();
			message5.EM_EI = interchange4.PK;
			message5.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			AssertEquals("Prerequisite: Declaration should not be Phase4.", false, nctsHeader.IsPhase4);
			AssertSame("Message should be linked to movement header.", message5.EM_LinkedObject, movementHeader);
			AssertEquals("GetOutgoingMessage should return the message (message4) matching the interchange number 425.", message4, nctsHeader.GetOutgoingMessage(message5));

			var interchange5 = Factory.NewWithValidTestData<EDIInterchange>();
			interchange5.EI_InterchangeNum = "999";

			var message6 = movementHeader.Messages.AddNew();
			message6.EM_EI = interchange5.PK;
			message6.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			AssertEquals("GetOutgoingMessage should return the message (message4) when no message matches the interchange number 999.", message4, nctsHeader.GetOutgoingMessage(message6));
		}

		public void TestDetailedDepartureStatusCodeGetter()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			nctsHeader.FRNctsHeader.CFN_DetailedDepartureStatusCode = "ZZZ";
			AssertEquals("ZZZ", nctsHeader.DetailedDepartureStatusCode);
		}

		public void TestDetailedDepartureStatusCodeSetter()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			nctsHeader.DetailedDepartureStatusCode = "ZZZ";
			AssertEquals("ZZZ", nctsHeader.FRNctsHeader.CFN_DetailedDepartureStatusCode);
		}

		public void TestIsPrelodgedMovement()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = GetNewBusinessObject(Factory);
				nctsHeader.MovementHeader.PreLodgedForAgreedLocationOfGoodsCode = true;
				AssertEquals("Is Pre Lodge", true, nctsHeader.IsPrelodgedMovement);

				nctsHeader.MovementHeader.PreLodgedForAgreedLocationOfGoodsCode = false;
				AssertEquals("Is not Pre Lodge", false, nctsHeader.IsPrelodgedMovement);
			});
		}

		public void TestGetCustomsOfficeRequirementHelper()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			AssertType<NctsHeaderCustomsOfficeRequirementHelper>(nctsHeader.CustomsOfficeRequirementHelper);
		}

		public void TestHasDepartureReachedGoodsReleasedForTransitStatusreferenceDRL()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			Assert(!nctsHeader.HasDepartureReachedGoodsReleasedForTransitStatus);
			var log = nctsHeader.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				log.SL_Reference = NctsTransitStatusList.Codes.DeclarationRejected;
				log.SL_EventTime = new ZDateTime(2021, 3, 12);
			}

			Assert(!nctsHeader.HasDepartureReachedGoodsReleasedForTransitStatus);

			log = nctsHeader.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.CustomsManifestStatus.Code;
				log.SL_Reference = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
				log.SL_EventTime = new ZDateTime(2021, 3, 12);
			}

			Assert(!nctsHeader.HasDepartureReachedGoodsReleasedForTransitStatus);

			log = nctsHeader.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				log.SL_Reference = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
				log.SL_EventTime = new ZDateTime(2021, 3, 12);
			}

			Assert(nctsHeader.HasDepartureReachedGoodsReleasedForTransitStatus);
		}

		public void TestMakeDepartureAmendment()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			var departureMovement = nctsHeader.MovementHeader;
			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
			nctsHeader.LocalReferenceNumber = "NCTIA00000001";
			nctsHeader.EffectiveMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

			CombineAssertions(() =>
			{
				AssertEquals("Amendment not allowed", "Amendment is not allowed currently.", nctsHeader.MakeDepartureAmendment());

				departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
				AssertEquals("Amendment allowed", "Status reset. Please save, close and reopen this declaration to amend and retransmit.", nctsHeader.MakeDepartureAmendment());
				AssertEquals("BM_CustomsStatus after amendment", NctsTransitStatusList.Codes.ReadyForAmendment, departureMovement.BM_CustomsStatus);
				AssertEquals("LocalReferenceNumber has not changed", "NCTIA00000001", nctsHeader.LocalReferenceNumber);
			});
		}

		public void TestDeltaTFallbackAnnouncedButNotActiveDeltaTFallbackIsAnnouncedFallBackNotActive()
		{
			var nctsheader = Factory.New<NctsHeader>();
			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Today.AddDays(1);
			fallbackSetting.Start = ZDateTime.Today.AddDays(1);
			FRCustomsDataRegistry.Instance.DeltaTMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			AssertEquals(false, FRCustomsDataRegistry.DeltaTFallbackIsActive);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, "Fallback");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, NctsHeader.Schema.DeltaT, ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(2));
			Factory.Save();

			AssertEquals(true, nctsheader.DeltaTFallbackAnnounced);
			AssertEquals(true, nctsheader.DeltaTFallbackAnnouncedButNotActive);
		}
		public void TestDeltaTFallbackAnnouncedButNotActiveDeltaTFallbackIsAnnouncedFallBackActive()
		{
			var nctsheader = Factory.New<NctsHeader>();
			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaTMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			AssertEquals(true, FRCustomsDataRegistry.DeltaTFallbackIsActive);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, "Fallback");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, NctsHeader.Schema.DeltaT, ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(2));
			Factory.Save();
			AssertEquals(true, nctsheader.DeltaTFallbackAnnounced);
			AssertEquals(false, nctsheader.DeltaTFallbackAnnouncedButNotActive);
		}

		public void TestDeltaTFallbackAnnouncedButNotActiveDeltaTFallbackIsNotAnnouncedFAllBackNotActive()
		{
			var nctsheader = Factory.New<NctsHeader>();
			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Today.AddDays(1);
			fallbackSetting.Start = ZDateTime.Today.AddDays(1);
			FRCustomsDataRegistry.Instance.DeltaTMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			AssertEquals(false, FRCustomsDataRegistry.DeltaTFallbackIsActive);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, "Fallback");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, NctsHeader.Schema.DeltaT, ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(2));
			Factory.Save();
			AssertEquals(false, nctsheader.DeltaTFallbackAnnounced);
			AssertEquals(false, nctsheader.DeltaTFallbackAnnouncedButNotActive);
		}

		public void TestDeltaTFallbackAnnouncedButNotActiveDeltaTFallbackIsNotAnnouncedFAllBackIsActive()
		{
			var nctsheader = Factory.New<NctsHeader>();
			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaTMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			AssertEquals(true, FRCustomsDataRegistry.DeltaTFallbackIsActive);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, "Fallback");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, NctsHeader.Schema.DeltaT, ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(2));
			Factory.Save();
			AssertEquals(false, nctsheader.DeltaTFallbackAnnounced);
			AssertEquals(false, nctsheader.DeltaTFallbackAnnouncedButNotActive);
		}

		public void TestAdditionalClausesForWhenDepartureAmendmentIsAllowed_ARFACC()
		{
			var nctsheader = Factory.New<NctsHeaderforTest>();
			nctsheader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsheader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsNotReleasedForTransit;

			nctsheader.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.ArrivalNotification;
			Assert(!nctsheader.AdditionalClausesForWhenDepartureAmendmentIsAllowed);

			nctsheader.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.AmendmentRefused;
			Assert(nctsheader.AdditionalClausesForWhenDepartureAmendmentIsAllowed);

			nctsheader.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.AmendmentAccepted;
			Assert(nctsheader.AdditionalClausesForWhenDepartureAmendmentIsAllowed);
		}

		public void TestGetTemporaryStorageRegisterTransactionData()
		{
			var nctsheader = Factory.New<NctsHeaderforTest>();
			nctsheader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsheader.BH_JobReference = "NCT0000002";
			nctsheader.MovementReferenceEntryNumber.CE_EntryNum = "NCTS0001";

			var goods = nctsheader.MovementHeader.GoodsItems.AddNew();
			goods.BY_GrossWeight = 20m;
			var package1 = goods.Packages.AddNew();
			package1.B5_UnitCount = 40;
			var doc = goods.PreviousDocuments.AddNew();
			doc.CSI_Code = PreviousDocumentCodeList.Codes._337;
			doc.CSI_LineNo = 1;

			var goods2 = nctsheader.MovementHeader.GoodsItems.AddNew();
			goods2.BY_GrossWeight = 50m;
			var package2 = goods2.Packages.AddNew();
			package2.B5_UnitCount = 10;
			var doc2 = goods2.PreviousDocuments.AddNew();
			doc2.CSI_Code = PreviousDocumentCodeList.Codes._337;
			doc2.CSI_LineNo = 2;

			var transactionData1 = goods.TemporaryStorageRegisterTransactionDataProvider.GetTemporaryStorageRegisterTransactionData().ToList();
			var transaction1 = transactionData1.Single();
			CombineAssertions("Transaction values", () =>
			{
				AssertEquals("NCTS0001", transaction1.CustomsReferenceNumber);
				AssertEquals("NCT0000002", transaction1.InternalReferenceNumber);
				AssertEquals(40, transaction1.PackageQuantity);
				AssertEquals(TempStorageTransactionRefTypeList.Codes.NctsHeader, transaction1.ReferenceType);
				AssertEquals(20m, transaction1.GrossMass);
				AssertEquals("", transaction1.Comments);
				AssertEquals(1, transaction1.RegisterLineNo);
			});

			var transactionData2 = goods2.TemporaryStorageRegisterTransactionDataProvider.GetTemporaryStorageRegisterTransactionData().ToList();
			var transaction2 = transactionData2.Single();
			CombineAssertions("Transaction values", () =>
			{
				AssertEquals("NCTS0001", transaction2.CustomsReferenceNumber);
				AssertEquals("NCT0000002", transaction2.InternalReferenceNumber);
				AssertEquals(10, transaction2.PackageQuantity);
				AssertEquals(TempStorageTransactionRefTypeList.Codes.NctsHeader, transaction2.ReferenceType);
				AssertEquals(50m, transaction2.GrossMass);
				AssertEquals("", transaction2.Comments);
				AssertEquals(2, transaction2.RegisterLineNo);
			});
		}

		public void TestCloneResetsDetailedStatus()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			nctsHeader.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.ReleasedAtDestination;
			var clone = (NctsHeader)nctsHeader.TemplateCopy();
			AssertEquals("DetailedDepartureStatusCode should be ''", ZString.Empty, clone.DetailedDepartureStatusCode);

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.ArrivalNotification;
			clone = (NctsHeader)nctsHeader.TemplateCopy();
			AssertEquals("DetailedDepartureStatusCode should be ''", ZString.Empty, clone.DetailedDepartureStatusCode);
		}

		public void TestConsignor_BothEmpty()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			var orgHeader = Factory.New<OrgHeader>();
			nctsHeader.Consignor.OrganisationPK = orgHeader.PK;
			AssertNoWarningContaining(nctsHeader.Consignor.OrganisationPKInfo, "Please specify the Consignor, either in Departure Declaration or Goods Items tab.");

			nctsHeader.Consignor.OrganisationPK = ZGuid.Empty;
			AssertHasWarningContaining(nctsHeader.Consignor.OrganisationPKInfo, "Please specify the Consignor, either in Departure Declaration or Goods Items tab.");

			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem.Consignor.OrganisationPK = orgHeader.PK;
			AssertNoWarningContaining(goodsItem.Consignor.OrganisationPKInfo, "Please specify the Consignor, either in Departure Declaration or Goods Items tab.");

			goodsItem.Consignor.OrganisationPK = ZGuid.Empty;
			AssertHasWarningContaining(goodsItem.Consignor.OrganisationPKInfo, "Please specify the Consignor, either in Departure Declaration or Goods Items tab.");
		}

		public void TestConsignor_BothFilled()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			var orgHeader = Factory.New<OrgHeader>();
			nctsHeader.Consignor.OrganisationPK = orgHeader.PK;
			AssertNoWarningContaining(nctsHeader.Consignor.OrganisationPKInfo, "You can't fill in the Consignor in both Departure Declaration and Goods Items tab. Please note that only the Consignor in Departure Declaration tab will be sent to customs.");

			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem.Consignor.OrganisationPK = orgHeader.PK;
			AssertHasWarningContaining(goodsItem.Consignor.OrganisationPKInfo, "You can't fill in the Consignor in both Departure Declaration and Goods Items tab. Please note that only the Consignor in Departure Declaration tab will be sent to customs.");

			nctsHeader.Consignor.OrganisationPK = ZGuid.Empty;
			AssertNoWarningContaining(nctsHeader.Consignor.OrganisationPKInfo, "You can't fill in the Consignor in both Departure Declaration and Goods Items tab. Please note that only the Consignor in Departure Declaration tab will be sent to customs.");
		}

		public void TestConsignee_BothEmpty()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			var orgHeader = Factory.New<OrgHeader>();
			nctsHeader.Consignee.OrganisationPK = orgHeader.PK;
			AssertNoWarningContaining(nctsHeader.Consignee.OrganisationPKInfo, "Please specify the Consignee, either in Departure Declaration or Goods Items tab.");

			nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
			AssertHasWarningContaining(nctsHeader.Consignee.OrganisationPKInfo, "Please specify the Consignee, either in Departure Declaration or Goods Items tab.");

			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem.Consignee.OrganisationPK = orgHeader.PK;
			AssertNoWarningContaining(goodsItem.Consignee.OrganisationPKInfo, "Please specify the Consignee, either in Departure Declaration or Goods Items tab.");

			goodsItem.Consignee.OrganisationPK = ZGuid.Empty;
			AssertHasWarningContaining(goodsItem.Consignee.OrganisationPKInfo, "Please specify the Consignee, either in Departure Declaration or Goods Items tab.");
		}

		public void TestConsignee_BothFilled()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			var orgHeader = Factory.New<OrgHeader>();
			nctsHeader.Consignee.OrganisationPK = orgHeader.PK;
			AssertNoWarningContaining(nctsHeader.Consignee.OrganisationPKInfo, "You can't fill in the Consignee in both Departure Declaration and Goods Items tab. Please note that only the Consignee in Departure Declaration tab will be sent to customs.");

			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem.Consignee.OrganisationPK = orgHeader.PK;
			AssertHasWarningContaining(goodsItem.Consignee.OrganisationPKInfo, "You can't fill in the Consignee in both Departure Declaration and Goods Items tab. Please note that only the Consignee in Departure Declaration tab will be sent to customs.");

			nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
			AssertNoWarningContaining(nctsHeader.Consignee.OrganisationPKInfo, "You can't fill in the Consignee in both Departure Declaration and Goods Items tab. Please note that only the Consignee in Departure Declaration tab will be sent to customs.");
		}

		public void TestDeclarant_Empty()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "CONOK";
			declarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("EOR", "1232141", Core.Constants.CountryCodes.France);

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.Declarant.OrganisationPK = ZGuid.Empty;
			nctsHeader.Declarant.Validation.ValidateAll();
			AssertHasMessageError(nctsHeader.Declarant.OrganisationPKInfo, "Please enter a Declarant trader with an EORI or enter full address details.");
			nctsHeader.Declarant.OrganisationPK = declarant.PK;
			AssertNoMessageError(nctsHeader.Declarant.OrganisationPKInfo, "Please enter a Declarant trader with an EORI or enter full address details.");

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
			nctsHeader.Declarant.OrganisationPK = ZGuid.Empty;
			nctsHeader.Declarant.Validation.ValidateAll();
			AssertHasMessageError(nctsHeader.Declarant.OrganisationPKInfo, "Please enter a Declarant trader with an EORI or enter full address details.");
			nctsHeader.Declarant.OrganisationPK = declarant.PK;
			AssertNoMessageError(nctsHeader.Declarant.OrganisationPKInfo, "Please enter a Declarant trader with an EORI or enter full address details.");

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.Declarant.OrganisationPK = ZGuid.Empty;
			nctsHeader.Declarant.Validation.ValidateAll();
			AssertHasMessageError(nctsHeader.Declarant.OrganisationPKInfo, "Please enter a Declarant trader with an EORI or enter full address details.");
			nctsHeader.Declarant.OrganisationPK = declarant.PK;
			AssertNoMessageError(nctsHeader.Declarant.OrganisationPKInfo, "Please enter a Declarant trader with an EORI or enter full address details.");
		}

		public void TestDeclarantJobDocAddressChangedTriggerChangeValueInBMLocationOfGoodsCode()
		{
			var declarantWithOkAuthorisation = Factory.NewWithValidTestData<OrgHeader>();
			declarantWithOkAuthorisation.OH_Code = "DECOK";

			var declarantAddressWithOkAuthorisation = declarantWithOkAuthorisation.Addresses.AddNew();
			declarantAddressWithOkAuthorisation.AddressCode = "DeclarantMatchAddress";
			declarantAddressWithOkAuthorisation.Address1 = "DeclarantMatchAddress";

			var consignorWithOkAuthorisation = Factory.NewWithValidTestData<OrgHeader>();
			consignorWithOkAuthorisation.OH_Code = "CONOK";

			var consignorAddressWithOKaddress = consignorWithOkAuthorisation.Addresses.AddNew();
			consignorAddressWithOKaddress.AddressCode = "consignorMatchAddress";
			consignorAddressWithOKaddress.Address1 = "consignorMatchAddress";

			var authorisation2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisation2.CPH_OH_PermitHolder = declarantWithOkAuthorisation.PK;
			authorisation2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authorisation2.CPH_Number = "OKAddress";
			authorisation2.CPH_OA_AppliesTo = consignorAddressWithOKaddress.PK;
			authorisation2.CPH_Type = "ACR";
			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var departureMovement = nctsHeader.MovementHeader;
			Factory.Save();

			nctsHeader.Consignor.OrganisationPK = consignorWithOkAuthorisation.PK;
			nctsHeader.Consignor.E2_OA_Address = consignorAddressWithOKaddress.PK;
			departureMovement.IsSimplifiedNctsProcedure = true;

			CombineAssertions(() =>
			{
				departureMovement.BM_LocationOfGoodsCode = ZString.Empty;
				nctsHeader.Declarant.OrganisationPK = declarantWithOkAuthorisation.PK;
				AssertEquals("Change declarantJobDocAddress should trigger ChangeValueInBMLocationOfGoodsCode()", "OKAddress", departureMovement.BM_LocationOfGoodsCode);

				nctsHeader.Consignor.E2_OA_Address = ZGuid.Empty;
				departureMovement.BM_LocationOfGoodsCode = ZString.Empty;
				nctsHeader.Consignor.E2_OA_Address = consignorAddressWithOKaddress.PK;
				AssertEquals("Change consignorJobDocAddress should trigger ChangeValueInBMLocationOfGoodsCode()", "OKAddress", departureMovement.BM_LocationOfGoodsCode);
			});
		}

		public void TestDeclarantJobDocAddressChangedTriggerUpdateDestinationOffice()
		{
			var header = Factory.New<NctsHeader>();

			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.ArrivalMovementHeader.IsSimplifiedNctsProcedure = true;

			var destinationTrader = Factory.NewWithValidTestData<OrgHeader>();
			header.DestinationTrader.E2_OA_Address = destinationTrader.MainAddress.PK;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();

			var address = header.DestinationTrader.Address;
			var authorisation = declarant.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit).WithNumber("DECLARANT_AUT").WithCountry(header.CountryCode);
			authorisation.CPH_OA_AppliesTo = address.PK;
			authorisation.CPH_StartDate = ZDate.Today.AddDays(-1);

			var declarationAuthorisationRule = authorisation.CusAuthorisationRules.AddNew();
			declarationAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.OFC;
			declarationAuthorisationRule.CPR_ValueFrom = "FR003333";

			AssertEquals("Verify DestinationCustomsOfficeCodeForArrival is not set before a change is triggered by DeclarantJobDocAddressChanged method.", ZString.Empty, header.DestinationCustomsOfficeCodeForArrival);
			header.Declarant.E2_OA_Address = declarant.MainAddress.PK;
			AssertEquals("Change in Declarant address should trigger UpdateDestinationOffice method which updates DestinationCustomsOfficeCodeForArrival.", "FR003333", header.DestinationCustomsOfficeCodeForArrival);
		}

		public void TestCorrelationIDCanBeUniquelySetWhenSaving()
		{
			var nctsHeader1 = GetNewBusinessObject(Factory);
			var nctsHeader2 = GetNewBusinessObject(Factory);
			var nctsHeader3 = GetNewBusinessObject(Factory);

			Factory.Save();

			AssertType<ZString>(nctsHeader1.CorrelationID);
			AssertNotEquals(nctsHeader1.CorrelationID, nctsHeader2.CorrelationID);
			AssertEquals("NctsHeader CorrelationID starts from 0000000001", "0000000001", nctsHeader1.CorrelationID);
			AssertEquals("NctsHeader CorrelationID auto-increments by 1", "0000000002", nctsHeader2.CorrelationID);
			AssertEquals("NctsHeader CorrelationID auto-increments by 1", "0000000003", nctsHeader3.CorrelationID);
		}

		public void TestCorrelationIDPrefix()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			AssertEquals(ZString.Empty, nctsHeader.CorrelationIDPrefix);
		}

		public void TestCorrelationIdShouldBeRevertedToEmptyForNewlyCreatedHeaderButFailsToBeSaved()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();
			var nctsHeader1 = factory1.New<NctsHeader>();
			var nctsHeader2 = factory2.New<NctsHeader>();
			nctsHeader1.BH_ParentTableCode = "XX";
			try
			{
				factory1.Save();
			}
			catch (ZSaveException)
			{
			}

			factory2.Save();
			Assert("For newly created headers that fail to be saved, the Correlation ID should be reverted to empty.", nctsHeader1.CorrelationID.IsEmpty);

			nctsHeader1.BH_ParentTableCode = ZString.Empty;
			factory1.Save();
			Assert("In the second attempt to save, nctsHeader1 is able to be assigned a new CorrelationID.", nctsHeader1.CorrelationID != nctsHeader2.CorrelationID);
		}

		public void TestCustomsOfficeUpdatedOnConsignorOrConsigneeChanged()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.CustomsOfficesForDeparture.AddNew("DES");
			header.CustomsOfficesForDeparture.AddNew("DEP");
			AssertNullOrEmpty(header.DepartureCustomsOfficeCode);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Address1 = "Address1";
			address1.AddressCode = "add1";
			address1.OA_OH = org1.PK;
			var cusCode1 = org1.CustomsCodes.AddNew();
			cusCode1.OK_RN_NKCodeCountry = "FR";
			cusCode1.OK_CodeType = "CTR";
			cusCode1.OK_CustomsRegNo = "FR001";
			cusCode1.OK_OA_PremisesAddress = address1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_Address1 = "Address2";
			address2.AddressCode = "add2";
			address2.OA_OH = org2.PK;
			var cusCode2 = org2.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = "FR";
			cusCode2.OK_CodeType = "CTR";
			cusCode2.OK_CustomsRegNo = "FR002";
			cusCode2.OK_OA_PremisesAddress = address2.PK;

			header.Consignee.E2_OA_Address = address1.PK;
			header.Consignor.E2_OA_Address = address2.PK;
			Factory.Save();

			AssertEquals("FR001", header.DestinationCustomsOfficeCode);
			AssertEquals("FR002", header.DepartureCustomsOfficeCode);
		}

		public void TestAutomatisationOfTraCustomsOfficeWithLoadPortOfDispatchAndCountryDestinationPort()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			var tradeGroup = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionForCustoms, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

			CombineAssertions("Prerequisite: Country IsCountryEuOrCtCountry  and IsMemberOfEU.", () =>
			{
				Assert("Switzerland is IsCountryEuOrCtCountry.", Factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Switzerland));
				Assert("Switzerland is not IsMemberOfEU.", !Factory.IsMemberOfEU(Core.Constants.CountryCodes.Switzerland));

				Assert("Switzerland is IsCountryEuOrCtCountry.", Factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Italy));
				Assert("Italy is IsMemberOfEU.", Factory.IsMemberOfEU(Core.Constants.CountryCodes.Italy));

				Assert("UnitedKingdom is not IsCountryEuOrCtCountry.", !Factory.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.UnitedKingdom));
				Assert("UnitedKingdom is not IsMemberOfEU.", !Factory.IsMemberOfEU(Core.Constants.CountryCodes.UnitedKingdom));
			});

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var office = header.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			office.CY_Data = "FR002300";

			header.PortOfDispatch = "ITPOR";
			header.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;

			Assert("There should be no TRA customs office as IsCountryEuOrCtCountry and IsMemberOfEU are true for italy", !header.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit));

			header.PortOfDispatch = "GBPOR";
			header.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.UnitedKingdom;
			Assert("There should be no TRA customs office as IsCountryEuOrCtCountry and IsMemberOfEU are false for united kingdom", !header.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit));

			header.PortOfDispatch = "CHPOR";
			Assert("There should be TRA customs office as IsCountryEuOrCtCountry is true and IsMemberOfEU is false for Switzerland", header.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit));

			header.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Switzerland;
			Assert("Tra office should have the same data as DES office as Tra office exist and BM_RL_NKDestinationPort &&  PortOfDispatch has same country.", header.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit && x.CY_Data == "FR002300"));

			var header2 = Factory.New<NctsHeader>();
			header2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header2.SetMovementType(NctsMovementType.Codes.Departure);
			var office2 = header2.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			office2.CY_Data = "FR002300";
			header2.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Switzerland;

			Assert("There should be no TRA customs office as PortOfDispatch has not been set yet.", !header2.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit));

			header2.PortOfDispatch = "CHPOR";
			Assert("Tra office should have been created with the same data as DES office as BM_RL_NKDestinationPort &&  PortOfDispatch has same country.", header2.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit && x.CY_Data == "FR002300"));

			var header3 = Factory.New<NctsHeader>();
			header3.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header3.SetMovementType(NctsMovementType.Codes.Departure);
			var office3 = header3.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			office3.CY_Data = "FR002300";
			header3.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;

			Assert("There should be no TRA customs office as PortOfDispatch has not been set yet.", !header3.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit));

			header3.PortOfDispatch = "CHPOR";
			Assert("Tra office should have been created with empty value as BM_RL_NKDestinationPort &&  PortOfDispatch has different country.", header3.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit && x.CY_Data == ""));

			header3.PortOfDispatch = "ITPOR";
			Assert("Tra office should have been valorised with NCTSOfficeOfDestination cy data as BM_RL_NKDestinationPort &&  PortOfDispatch has same country.", header3.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit && x.CY_Data == "FR002300"));
		}

		public void TestIsDepartureTabReadOnly_MessageStatus()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsWrittenOff;
			AssertEquals($"Movement should not be editable when its Customs Status doesn't allow for it as long as Message Status is not REJ.", true, header.IsDepartureTabReadOnly);

			header.EffectiveMessageStatus = EDIMessageStatusList.Codes.Rejected;
			AssertEquals($"Movement should always be editable when Message Status is REJ, even when its Customs Status would normaly not allow for that.", false, header.IsDepartureTabReadOnly);
		}

		public void TestIsDepartureTabReadOnly_CustomsStatus()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var departureMovement = header.MovementHeader;

			CombineAssertions(() =>
			{
				departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
				AssertEquals($"When BM_CustomsStatus == '{NctsTransitStatusList.Codes.Unknown}', IsDepartureTabReadOnly", false, header.IsDepartureTabReadOnly);

				departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationRejected;
				AssertEquals($"When BM_CustomsStatus == '{NctsTransitStatusList.Codes.DeclarationRejected}', IsDepartureTabReadOnly", false, header.IsDepartureTabReadOnly);

				departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsWrittenOff;
				AssertEquals($"When BM_CustomsStatus == '{NctsTransitStatusList.Codes.GoodsWrittenOff}', IsDepartureTabReadOnly", true, header.IsDepartureTabReadOnly);
			});
		}

		public void TestShouldTransportDetailsSyncDependsOnTransportMode()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			Assert("ShouldTransportDetailsSyncDependsOnTransportMode should be equal to false for FR.", !nctsHeader.ShouldTransportDetailsSyncDependsOnTransportMode);
		}

		public override void TestICusInBondContainerTypeSupporter()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			AssertEquals(typeof(FRNctsDepartureHeaderContainer), (header as ICusInBondContainerTypeSupporter).ContainerType);
		}

		public void TestBillCollectionType()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			AssertType<NctsBillCollection<NctsBill>>(nctsHeader.Bills);
		}

		public void TestCusAuthorizationUsageType()
		{
			var movementHeader = Factory.New<NctsHeader>();
			var cusAuthorizationUsage = movementHeader.CusAuthorizationUsages.AddNew();
			AssertType<NctsCusAuthorizationUsage>(cusAuthorizationUsage);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		NctsHeader GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader;
		}

		NctsHeader GetNewBusinessObjectPhase5(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			return nctsHeader;
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new LightValidationTesterExcludingJobDocAddress(bizObjToTest);
		}

		protected override ZString ExpectedDefaultBM_GS_NKCusAgent => GlbStaff.CurrentUser.GS_Code;
	}

	public class NctsHeaderforTest : NctsHeader
	{
		public NctsHeaderforTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString BH_ApplicationCode => CusInBondApplicationCodeList.Codes.NCTS4;

		public IValueSetStrategy GetValueSetStrategyExposed() => base.GetValueSetStrategy();

		public new bool AdditionalClausesForWhenDepartureAmendmentIsAllowed => base.AdditionalClausesForWhenDepartureAmendmentIsAllowed;
	}

	class LightValidationTesterExcludingJobDocAddress : LightValidationTester
	{
		public LightValidationTesterExcludingJobDocAddress(BusinessObject bo) : base(bo)
		{
		}

		protected override bool ShouldTestProperty(ZPropertyInfo info)
		{
			// DAT asks me to call MarkAsNeedingValidation when changing E2_ParentID, E2_ParentTableCode and E2_AddressType of Consignor/ee (JobDocAddress),
			// even though I never change them directly. I did try to call MarkAsNeedingValidation in their setters but DAT didn't recognize it.
			// So I suppressed the JobDocAddress here.
			if (info.BizObj.GetType() == typeof(JobDocAddress))
			{
				return false;
			}
			return base.ShouldTestProperty(info);
		}
	}
}
