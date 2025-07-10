using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Integration.Customs.EU.NCTS;
using static Enterprise.MasterFiles.Business.WorkflowDescriptor;
using PermitRuleCodeList = Enterprise.Customs.EU.Business.PermitRuleCodeList;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeader))]
	sealed partial class NctsHeaderBaseOnlyTest : NctsHeaderAbstractTest
	{
		[ExpectNoExceptions]
		public void TestSetMovementType_SetInitialTraderAtDestination_Arrival()
		{
			var defaultTraderAtDestinationManagerMock = new Mock<INctsDefaultTraderAtDestinationManager>();
			defaultTraderAtDestinationManagerMock.Setup(mock => mock.ApplyDefaultingIfEnabled());

			var nctsHeaderMock = Factory.NewMoq<NctsHeaderForTest>();
			nctsHeaderMock
				.Protected()
				.Setup<INctsDefaultTraderAtDestinationManager>("GetNewDefaultTraderAtDestinationManager")
				.Returns(defaultTraderAtDestinationManagerMock.Object);
			nctsHeaderMock.Object.SetMovementType(NctsMovementType.Codes.Arrival);
			defaultTraderAtDestinationManagerMock.Verify(mock => mock.ApplyDefaultingIfEnabled(), Times.Once());
		}

		[ExpectNoExceptions]
		public void TestSetMovementType_SetInitialTraderAtDestination_Departure()
		{
			var defaultTraderAtDestinationManagerMock = new Mock<INctsDefaultTraderAtDestinationManager>();
			defaultTraderAtDestinationManagerMock.Setup(mock => mock.ApplyDefaultingIfEnabled());

			var nctsHeaderMock = Factory.NewMoq<NctsHeaderForTest>();
			nctsHeaderMock
				.Protected()
				.Setup<INctsDefaultTraderAtDestinationManager>("GetNewDefaultTraderAtDestinationManager")
				.Returns(defaultTraderAtDestinationManagerMock.Object);
			nctsHeaderMock.Object.SetMovementType(NctsMovementType.Codes.Departure);
			defaultTraderAtDestinationManagerMock.Verify(mock => mock.ApplyDefaultingIfEnabled(), Times.Never());
		}

		public void TestValidationDeciderPhase4()
		{
			AssertNull(header.ValidationDecider);
		}

		public void TestDepartureMovementHeaders()
		{
			AssertType<NctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader>>(header.DepartureMovementHeaders);
		}

		public void TestITypeDeciderContext()
		{
			var frCompany = Factory.New<GlbCompany>();
			frCompany.GC_Code = "CFR";
			frCompany.GC_Name = "FR Company";
			frCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var frBranch = frCompany.Branches.AddNew();
			frBranch.GB_Code = "BFR";

			var header = Factory.New<NctsHeader>();
			AssertEquals("Precondition", "LV", header.Branch.Country.Code);
			AssertEquals("LV", (header as ITypeDeciderContext).Country);

			header.BH_GB = frBranch.PK;
			AssertEquals("FR", (header as ITypeDeciderContext).Country);
		}

		public void TestCommunicationLanguage_List()
		{
			AssertEquals("Lookups.CommunicationLanguageList", Factory.New<NctsHeader>().BH_CommunicationLanguageInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestCommunicationLanguage_MaxLength()
		{
			AssertEquals("JE_DeclarationLanguage MaxLength", 2, Factory.New<NctsHeader>().BH_CommunicationLanguageInfo.MaxLength);
		}

		public void TestCommunicationLanguage_Caption()
		{
			AssertEquals("Language", Factory.New<NctsHeader>().BH_CommunicationLanguageInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestIsDepartureCancellationAllowed()
		{
			var departureHeader = CreatePhase5DepartureHeader();
			var departureMovement = departureHeader.MovementHeader;
			var cancellationAllowedStatuses = new HashSet<string>
			{
				NctsTransitStatusList.Codes.DeclarationMrnAllocated,
				NctsTransitStatusList.Codes.GoodsUnderCustomsControl,
				NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid,
				NctsTransitStatusList.Codes.GoodsNotReleasedForTransit
			};

			CombineAssertions("Cancellation should be allowed only when the departure declaration is DMA, DCC, DGN or DNR", () =>
			{
				AssertEquals(false, departureHeader.IsDepartureCancellationAllowed);
				foreach (var status in departureMovement.Lookups.NctsTransitStatusList.GetAllCodes())
				{
					departureMovement.BM_CustomsStatus = status;
					AssertEquals(cancellationAllowedStatuses.Contains(status), departureHeader.IsDepartureCancellationAllowed);
				}
			});
		}

		public void TestDestinationCustomsOfficeCodeForArrival()
		{
			CombineAssertions(() =>
			{
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				Factory.Save();

				AssertEquals("Before adding new customs office", false, header.HasChanges);
				header.DestinationCustomsOfficeCodeForArrival = "ABC";
				AssertEquals("After adding new customs office", true, header.HasChanges);
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var reloadedHeader = newFactory.Load<NctsHeader>(header.PK);
				AssertEquals("Before updating existing customs office", false, reloadedHeader.HasChanges);
				reloadedHeader.DestinationCustomsOfficeCodeForArrival = "DEF";
				AssertEquals("After updating existing customs office", true, reloadedHeader.HasChanges);
			});
		}

		public void TestCusSupplyChainActors()
		{
			var actors = header.CusSupplyChainActors;
			CombineAssertions(() =>
			{
				AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(actors);
				AssertEquals("IsRegisteredEditableChildObject", true, header.IsRegisteredEditableChildObject(actors));
				AssertSame("Cached", actors, header.CusSupplyChainActors);
				AssertEquals("IsLoaded", true, actors.IsLoaded);
			});
		}

		public void TestGetNewValidation()
		{
			var arrival = Factory.New<NctsHeader>();
			arrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			arrival.SetMovementType(NctsMovementType.Codes.Arrival);
			CombineAssertions(() =>
			{
				AssertType<NctsHeaderValidation>("NCTS4", arrival.Validation);

				arrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertType<NctsHeaderPhase5Validation>("NCTS5", arrival.Validation);
			});
		}

		public void TestHasMultipleContainerisedContainers_Arrival()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalContainer1 = header.ArrivalHeaderContainers.AddNew();
			arrivalContainer1.BC_Mode = Core.Constants.ContainerModes.Containerised;
			var arrivalContainer2 = header.ArrivalHeaderContainers.AddNew();
			arrivalContainer2.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			var arrivalContainer3 = header.ArrivalHeaderContainers.AddNew();
			arrivalContainer3.BC_Mode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("Arrival NCTS4", false, header.HasMultipleContainerisedContainers);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("Arrival NCTS5", true, header.HasMultipleContainerisedContainers);
			arrivalContainer3.Delete();
			AssertEquals("Arrival NCTS5 - no multiple CNT", false, header.HasMultipleContainerisedContainers);
		}

		public void TestHasMultipleContainerisedContainers_Departure()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_Mode = Core.Constants.ContainerModes.Containerised;
			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			var container3 = header.DepartureHeaderContainers.AddNew();
			container3.BC_Mode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("Multiple CNT", true, header.HasMultipleContainerisedContainers);
			container3.Delete();
			AssertEquals("No Multiple CNT", false, header.HasMultipleContainerisedContainers);
		}

		public void TestMovementReferenceNumber_WillNotCreateCusEntryNumber()
		{
			var arrival = Factory.New<NctsHeader>();
			arrival.SetMovementType(NctsMovementType.Codes.Arrival);

			_ = arrival.MovementReferenceNumber;
			AssertNull(CusEntryNumber.Load(arrival, CusEntryNumberTypes.Standard.MovementReferenceNumber, arrival.CountryCode));
		}

		public void TestDepartureGoodsItems_DepartureMovement_Phase4()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.BH_HeaderType = NctsMovementType.Codes.Departure;

			var goodsItem1 = header.MovementHeader.GoodsItems.AddNew();
			var goodsItem2 = header.MovementHeader.GoodsItems.AddNew();
			AssertContainsExactElementsInExactOrder(new[] { goodsItem1.PK, goodsItem2.PK }, header.DepartureGoodsItems.Select(x => x.PK));
		}

		public void TestDepartureGoodsItems_DepartureMovement_Phase5()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_HeaderType = NctsMovementType.Codes.Departure;

			var bill = header.Bills.AddNew();
			var goodsItem1 = bill.GoodsItems.AddNew();
			var goodsItem2 = bill.GoodsItems.AddNew();
			AssertContainsExactElementsInExactOrder(new[] { goodsItem1.PK, goodsItem2.PK }, header.DepartureGoodsItems.Select(x => x.PK));
		}

		public void TestValidateDestinationTraderForSpecificCountry()
		{
			string message = "Please enter a Destination trader with an EORI. Required for a simplified arrival or Normal arrival to an EU member state.  N.B. the EORI is mandatory for native NCTS messaging. ";
			var header = Factory.New<NctsHeaderForTest>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.DestinationCustomsOfficeCodeForArrival = "BE101000";

			CombineAssertions(() =>
			{
				header.DestinationTrader.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("When Destination Trader is empty there should be an error(Phase5)", header.DestinationTrader.OrganisationPKInfo, message);

				header = Factory.New<NctsHeaderForTest>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.DestinationCustomsOfficeCodeForArrival = "BE101000";
				header.DestinationTrader.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining("When Destination Trader is empty there should be an error(Phase4)", header.DestinationTrader.OrganisationPKInfo, message);
			});
		}

		public void TestDepartureGoodsItems_ArrivalMovement()
		{
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			header.ArrivalMovementHeader.GoodsItems.AddNew();
			AssertEquals(0, header.DepartureGoodsItems.Count);
		}

		public void TestCusSeals()
		{
			var cusSeals = header.CusSeals;
			CombineAssertions(() =>
			{
				AssertType<CusSealCollection>("Type", cusSeals);
				AssertEquals("IsRegisteredEditableChildObject", true, header.IsRegisteredEditableChildObject(cusSeals));
				AssertSame("Cached", cusSeals, header.CusSeals);
				AssertEquals("IsLoaded", true, cusSeals.IsLoaded);
			});
		}

		public void TestINCTSAutoSendingMessageSupporter()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var supporter = nctsHeader as INCTSAutoSendingMessageSupporter;
			AssertEquals("The Send NCTS Message trigger is not supported to be sent for LV.", supporter.GetReasonForNotSupportNCTSMessage);
			AssertType<LogAction>(supporter.CreateNCTSMessageProcessor());
			AssertType<LogAction>(supporter.CreateNCTSArrivalNotificationMessageProcessor());
			AssertNull(supporter.CreateStmProcessQueueProcessor(null, ""));
		}

		public void TestDocDataObject()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var dataObject = nctsHeader.GetDocDataObject(ZString.Empty, null);
			AssertNull(dataObject);
		}

		public void TestSourceID()
		{
			var header = Factory.New<NctsHeaderForTest>();
			header.BH_JobReference = "NCTS12345";
			AssertEquals("SourceID should match BH_JobReference", "NCTS12345", header.SourceID);
		}

		public void TestSourceType()
		{
			var header = Factory.New<NctsHeader>();
			AssertEquals("SourceType should be DataContextType.NctsHeader", "NctsHeader", header.SourceType);
		}

		public void TestFallbackInformation()
		{
			AssertEquals(ZString.Empty, header.FallbackInformation);
		}

		public void TestJobDocAddressCanOverrideFlag()
		{
			var header = Factory.New<NctsHeaderForTest>();
			AssertEquals(false, header.PrincipalJobDocAddressRequirement.CanOverride);
			AssertEquals(false, header.ConsignorJobDocAddressRequirement.CanOverride);
			AssertEquals(false, header.ConsigneeJobDocAddressRequirement.CanOverride);
			AssertEquals(false, header.SecurityConsignorJobDocAddressRequirement.CanOverride);
			AssertEquals(false, header.SecurityConsigneeJobDocAddressRequirement.CanOverride);
		}

		public void TestCustomsOfficeRequirementHelper_ShouldNotShareBetweenJobs()
		{
			var header2 = Factory.New<NctsHeader>();
			AssertNotEquals("CustomsOfficeRequirementHelpers from two jobs should not reference to the same one, even they are in the same factory.", header.CustomsOfficeRequirementHelper, header2.CustomsOfficeRequirementHelper);
		}

		public void TestMovementReferenceEntryNumber()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("MovementReferenceEntryNumber->Not Null", header.MovementReferenceEntryNumber);
				AssertEquals("MovementReferenceEntryNumber.CE_RN_NKCountryCode", header.CountryCode, header.MovementReferenceEntryNumber.CE_RN_NKCountryCode);
				AssertEquals("MovementReferenceEntryNumber.CE_EntryType", CusEntryNumberTypes.Standard.MovementReferenceNumber, header.MovementReferenceEntryNumber.CE_EntryType);
			});
		}

		public void TestBrokerageCountryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var headerDE = Factory.New<NctsHeader>();
				AssertEquals("Header created under a DE company.", Core.Constants.CountryCodes.Germany, headerDE.BrokerageCountryCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Guadeloupe))
			{
				var headerGP = Factory.New<NctsHeader>();
				AssertEquals("Header created under a GP company.", Core.Constants.CountryCodes.France, headerGP.BrokerageCountryCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var headerFR = Factory.New<NctsHeader>();
				AssertEquals("Header created under a FR company.", Core.Constants.CountryCodes.France, headerFR.BrokerageCountryCode);
			}
		}

		public void TestLocalCurrency()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Core.Constants.CurrencyCodes.CzechRepublic))
			{
				var header = Factory.New<NctsHeader>();
				AssertEquals(Core.Constants.CurrencyCodes.CzechRepublic, header.LocalCurrency);
			}
		}

		public void TestUnloadingMovementHeaderChildEditable()
		{
			AssertEquals(true, header.IsRegisteredEditableChildObject(header.UnloadingMovementHeader));
		}

		public void TestReasonForCannotRequestCancellation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No MRN", "Cannot cancel as no MRN is allocated", header.ReasonForCannotRequestCancellation);
				header.MovementReferenceEntryNumber.CE_EntryNum = "123";
				AssertEquals("Has MRN", ZString.Empty, header.ReasonForCannotRequestCancellation);
			});
		}

		public void TestExplanationToCustomsForWhyCancelling()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No ExplanationToCustomsForWhyCancelling", ZString.Empty, header.ExplanationToCustomsForWhyCancelling);
				header.ExplanationToCustomsForWhyCancelling = "CANCELLATION REASON";
				AssertEquals("Has ExplanationToCustomsForWhyCancelling", "CANCELLATION REASON", header.ExplanationToCustomsForWhyCancelling);
			});
		}

		public void TestDescriptionProperty()
		{
			header.BH_JobReference = "NCT0000110";
			AssertEquals("NCTS Transit Movement NCT0000110", DescriptionPropertyAttribute.DescriptionFromBusinessObject(header));
		}

		public void TestDeclarantId()
		{
			const string eoriCode1 = "123456789000";
			const string eoriCode2 = "987654321000";
			var org1 = Factory.New<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var org2 = Factory.New<OrgHeader>();
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode2, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var combined = Factory.New<NctsHeader>();
			combined.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			combined.Declarant.E2_OA_Address = org1.MainAddress.PK;
			combined.DestinationTrader.E2_OA_Address = org2.MainAddress.PK;
			AssertContains("DepartureAndArrival Header", eoriCode2, combined.DeclarantId);
		}

		public void TestGetCusCodeDataType()
		{
			var dictionary = ((Integration.Customs.ICusCodeDataTypeSupporter)header).GetCusCodeDataTypes();
			AssertEquals(typeof(NctsEuOfficeCode), dictionary[EU.Business.CusCodeDataTypeList.Codes.OfficeCode]);
		}

		public void TestShouldSynchronise_ActiveMessaging()
		{
			CombineAssertions(() =>
			{
				var consol = Factory.New<ForwardingConsol>();
				header.BH_ParentID = consol.PK;
				header.BH_ParentTableCode = consol.TablePrefix;
				header.BH_OverrideFreightDefaults = false;
				AssertEquals("No messages", true, header.ShouldSynchronise);
				header.Messages.AddNew();
				AssertEquals("Has message", false, header.ShouldSynchronise);
			});
		}

		public void TestShouldSynchronise_PluggedIn()
		{
			CombineAssertions(() =>
			{
				var consol = Factory.New<ForwardingConsol>();
				AssertEquals("Not plugged In", false, header.ShouldSynchronise);
				header.BH_ParentID = consol.PK;
				header.BH_ParentTableCode = consol.TablePrefix;
				header.BH_OverrideFreightDefaults = false;
				AssertEquals("Consol plugged in", true, header.ShouldSynchronise);

				var shipment = Factory.New<ForwardingShipment>();
				header.BH_ParentID = shipment.PK;
				header.BH_ParentTableCode = shipment.TablePrefix;
				AssertEquals("Shipment plugged in", true, header.ShouldSynchronise);
			});
		}

		public void TestShouldSynchronise_OverrideFreightDefaults()
		{
			CombineAssertions(() =>
			{
				var consol = Factory.New<ForwardingConsol>();
				header.BH_ParentID = consol.PK;
				header.BH_ParentTableCode = consol.TablePrefix;
				header.BH_OverrideFreightDefaults = false;
				AssertEquals("No freight override", true, header.ShouldSynchronise);
				header.BH_OverrideFreightDefaults = true;
				AssertEquals("Freight override", false, header.ShouldSynchronise);
			});
		}

		public void TestShouldTransportDetailsSyncDependsOnTransportMode()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			Assert("ShouldTransportDetailsSyncDependsOnTransportMode should be equal to true for EU.", nctsHeader.ShouldTransportDetailsSyncDependsOnTransportMode);
		}

		public void TestOverrideFreightDefaultsWhenSycroniserIsNull()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var dummyCusInBondParent = Factory.New<CusInBondParentDummyBusinessObject>();

			nctsHeader.BH_ParentID = dummyCusInBondParent.PK;
			nctsHeader.BH_ParentTableCode = dummyCusInBondParent.TablePrefix;

			AssertNull("[PRE-CONDITION], Synchroniser", nctsHeader.Synchroniser);
			nctsHeader.BH_OverrideFreightDefaults = true;
			CombineAssertions("When Synchroniser is null", () =>
			{
				AssertEquals("ShouldSynchronise", false, header.ShouldSynchronise);
				AssertNoExceptionThrown(() => nctsHeader.BH_OverrideFreightDefaults = false);
			});
		}

		public void TestIsPluggedIn()
		{
			var headerWithShipment = Factory.New<NctsHeader>();
			headerWithShipment.SetMovementType(NctsMovementType.Codes.Departure);
			headerWithShipment.BH_ParentID = Factory.New<ForwardingShipment>().PK;
			var consol = Factory.New<ForwardingConsol>();
			var headerWithConsol = Factory.New<NctsHeader>();
			headerWithConsol.SetMovementType(NctsMovementType.Codes.Departure);
			headerWithConsol.BH_ParentID = consol.PK;
			headerWithConsol.BH_ParentTableCode = consol.TablePrefix;

			CombineAssertions(() =>
			{
				AssertEquals("No shipment and consol", false, header.IsPluggedIn);
				AssertEquals("Has shipment", true, headerWithShipment.IsPluggedIn);
				AssertEquals("Has consol", true, headerWithConsol.IsPluggedIn);
			});
		}

		public void TestShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			CombineAssertions(() =>
			{
				AssertNull("No shipment", header.Shipment);

				header.BH_ParentID = shipment.PK;
				AssertEquals("Has shipment", shipment, header.Shipment);
			});
		}

		public void TestConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			CombineAssertions(() =>
			{
				AssertNull("No consol", header.Consol);

				header.BH_ParentID = consol.PK;
				header.BH_ParentTableCode = consol.TablePrefix;
				AssertEquals("Has consol", consol, header.Consol);
			});
		}

		public void TestNctsEdiMessageDocumentSupporterAndInfo()
		{
			var message = Factory.New<NctsEdiMessage>();
			var supporter = message.DocumentSupporter;
			var info = message.DocManagerInfo;
			CombineAssertions(() =>
			{
				AssertType<NctsEdiMessageDocManagerInfo>("DocManagerInfo", info);
				AssertType<NctsEdiMessageDocumentSupporter>("DocumentSupporter", supporter);
			});
		}

		public void TestHumanReadableNameCore()
		{
			header.BH_JobReference = "DEF";
			AssertEquals("NCTS Transit Movement DEF", header.HumanReadableName);
		}

		public void TestCarrier()
		{
			OrgAddress carrierAddress = NCTSTestHelper.SetupCarrierForTest(header);
			AssertEquals(carrierAddress.PK, header.CarrierOrgAddress.PK);
		}

		public void TestBH_RL_NKImportLoadPort()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(header.BH_RL_NKImportLoadPortInfo, NctsHeader.Phase4CaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[15A] Country of Dispatch", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Disp.", captionResourceString.ShortCaption);
				AssertEquals("FullDescription", "Country of Dispatch", captionResourceString.FullDescription);
			});
		}

		public void TestDeclarationPlace()
		{
			AssertEquals("Brisbane", header.DeclarationPlace);
		}

		public void TestItinerary()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No itinerary countries", ZString.Empty, header.ItineraryCountries);

				NCTSTestHelper.AddItineraryCountryForTest(header, "AA");
				NCTSTestHelper.AddItineraryCountryForTest(header, "BB");
				NCTSTestHelper.AddItineraryCountryForTest(header, "CC");
				AssertEquals("Added 3 itineraries", "AA BB CC", header.ItineraryCountries);
				AssertEquals("Sequence# of itinerary1", 1, header.Itinerary[0].Sequence);
				AssertEquals("Sequence# of itinerary2", 2, header.Itinerary[1].Sequence);
				AssertEquals("Sequence# of itinerary3", 3, header.Itinerary[2].Sequence);

				header.Itinerary.RemoveAndDelete(header.Itinerary[2]);
				AssertEquals("Removed last itinerary", "AA BB", header.ItineraryCountries);

				header.Itinerary[0].Sequence = 10;
				NCTSTestHelper.AddItineraryCountryForTest(header, "HH");
				AssertEquals("Added itinerary HH", "AA BB HH", header.ItineraryCountries);
				AssertEquals("Updated sequence# of existing itinerary", 11, header.Itinerary[2].Sequence);
			});
		}

		public void TestBH_UniqueVoyageIdentifier()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No ItineraryCountries", 0, header.Itinerary.Count);

				header.BH_UniqueVoyageIdentifier = "AABB";
				AssertEquals("ItineraryCountries from BH_UniqueVoyageIdentifier", "AA BB", header.ItineraryCountries);
				AssertEquals("Itinerary.HasChanges", false, header.Itinerary.HasChanges);
			});
		}

		public void TestLocalReferenceNumberReadOnly_DepartureAndArrival()
		{
			header.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
			AssertEquals(true, header.LocalReferenceNumberReadOnly);
		}

		public void TestLocalReferenceNumber_MaxLength()
		{
			AssertEquals(22, header.LocalReferenceNumberInfo.MaxLength);
		}

		public void TestIsNctsCountryImplementedNatively()
		{
			CombineAssertions(() =>
			{
				AssertEquals("GB", true, header.IsNctsCountryImplementedNatively(Core.Constants.CountryCodes.UnitedKingdom));
				AssertEquals("FR", true, header.IsNctsCountryImplementedNatively(Core.Constants.CountryCodes.France));
				AssertEquals("ES", false, header.IsNctsCountryImplementedNatively(Core.Constants.CountryCodes.Spain));
				AssertEquals("IT", false, header.IsNctsCountryImplementedNatively(Core.Constants.CountryCodes.Italy));
				AssertEquals("DE", false, header.IsNctsCountryImplementedNatively(Core.Constants.CountryCodes.Germany));
			});
		}

		public void TestMovementReferenceNumber()
		{
			var mrn = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
			mrn.CE_EntryNum = "MRN123";
			AssertEquals("MRN123", header.MovementReferenceNumber);
		}

		public void TestMovementReferenceIssueDate()
		{
			NCTSTestHelper.AssertCaptions(header.MovementReferenceIssueDateInfo, "Release Date", string.Empty, string.Empty);

			AssertEquals("No MRN", ZDateTime.Empty, header.MovementReferenceIssueDate);

			var mrn = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
			mrn.CE_IssueDate = new ZDateTime(2023, 12, 28, 17, 51, 0);
			AssertEquals("With value", new ZDateTime(2023, 12, 28, 17, 51, 0), header.MovementReferenceIssueDate);
		}

		public void TestMovementReferenceExpiryDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No MRN", ZDateTime.Empty, header.MovementReferenceExpiryDate);

				var mrn = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
				mrn.CE_ExpiryDate = new ZDateTime(2023, 12, 28, 17, 51, 0);
				AssertEquals("With value", new ZDateTime(2023, 12, 28, 17, 51, 0), header.MovementReferenceExpiryDate);
			});
		}

		public void TestIsDepartureMovement()
		{
			CombineAssertions(() =>
			{
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				AssertEquals("Departure", true, header.IsDepartureMovement);
				header.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
				AssertEquals("DepartureAndArrival", true, header.IsDepartureMovement);
			});
		}

		public void TestIsArrivalMovement()
		{
			CombineAssertions(() =>
			{
				header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				AssertEquals("Arrival", true, header.IsArrivalMovement);
				header.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
				AssertEquals("DepartureAndArrival", true, header.IsArrivalMovement);
			});
		}

		public void TestIsDepartureAndArrivalMovement()
		{
			header.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
			AssertEquals("IsDepartureAndArrivalMovement", true, header.IsDepartureAndArrivalMovement);
		}

		public void TestCommonMovementHeader_DepartureMovement() => CombineAssertions(() =>
		{
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			var movementHeader = header.MovementHeader;
			AssertSame("Departure", movementHeader, header.CommonMovementHeader);
			header.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
			AssertSame("DepartureAndArrival", movementHeader, header.CommonMovementHeader);
		});

		public void TestCommonMovementHeader_ArrivalMovement()
		{
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			AssertSame("Arrival", header.ArrivalMovementHeader, header.CommonMovementHeader);
		}

		public void TestCommonMovementHeader_UnloadingMovementHeader()
		{
			header.BH_HeaderType = NctsMoveHeaderType.Codes.Unloading;
			AssertSame("Unloading", header.UnloadingMovementHeader, header.CommonMovementHeader);
		}

		public void TestLocalReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Before saved, LocalReferenceNumber empty", ZString.Empty, header.LocalReferenceNumber);
				Factory.Save();
				AssertNotEquals("After saved, LocalReferenceNumber not empty", "NCT000000001", header.LocalReferenceNumber);
				header.LocalReferenceNumber = "1234";
				AssertEquals("Overwrite LocalReferenceNumber", "1234", header.LocalReferenceNumber);
			});
		}

		public void TestLocalReferenceNumber_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(header.LocalReferenceNumberInfo, NctsHeader.Phase4CaptionKey, "[7] Customer Reference Number", "Customer Ref. Num.", "CRN");
		}

		public void TestLocalReferenceNumber_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(header.LocalReferenceNumberInfo, NctsHeader.Phase5CaptionKey, "Customer Reference", "Customer Ref.", "LRN");
		}

		public void TestLocalReferenceNumberForDisplay()
		{
			CombineAssertions(() =>
			{
				header.LocalReferenceNumber = "1234";
				AssertEquals("Get from LocalReferenceNumber", "1234", header.LocalReferenceNumberForDisplay);

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.SetMovementType(NctsMovementType.Codes.Departure);
				header.MovementHeader.BM_PaperlessInbondNum = "5678";
				AssertEquals("NC5 and Departure, get from BM_PaperlessInbondNum", "5678", header.LocalReferenceNumberForDisplay);

				var arrivalHeader = Factory.New<NctsHeader>();
				arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				arrivalHeader.LocalReferenceNumber = "ABCD";
				arrivalHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = "EFGH";
				AssertEquals("NC5 and Arrival, get from LocalReferenceNumber", "EFGH", arrivalHeader.LocalReferenceNumberForDisplay);
			});
		}

		public void TestLocalReferenceNumberForDisplay_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(typeof(NctsHeader), NctsHeader.Schema.LocalReferenceNumberForDisplay, NctsHeader.Phase4CaptionKey, "[7] Customer Reference Number", "Customer Ref. Num.", "CRN");
		}

		public void TestLocalReferenceNumberForDisplay_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(typeof(NctsHeader), NctsHeader.Schema.LocalReferenceNumberForDisplay, NctsHeader.Phase5CaptionKey, "Customer Reference", "Customer Ref.", "LRN");
		}

		public void TestArrivalMrnFromUser_Modified_TriggeringHasChanges()
		{
			header.HasChanges = false;
			CombineAssertions(() =>
			{
				header.ArrivalMrnFromUser = ZString.Empty;
				AssertEquals("Setting with same value", false, header.HasChanges);
				header.ArrivalMrnFromUser = "XXX";
				AssertEquals("Setting with different value", true, header.HasChanges);
			});
		}

		public void TestArrivalMrnFromUser_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(header.ArrivalMrnFromUserInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "MRN", captionResourceString.Caption);
				AssertEquals("ShortCaption", "MRN", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "MRN", captionResourceString.MediumCaption);
				AssertEquals("FullDescription", "Movement Reference Number", captionResourceString.FullDescription);
			});
		}

		public void TestArrivalMrnFromUser_MaxLength()
		{
			CombineAssertions(() =>
			{
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals(21, header.ArrivalMrnFromUserInfo.MaxLength);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals(18, header.ArrivalMrnFromUserInfo.MaxLength);
			});
		}

		public void TestDestinationCustomsOfficeCodeForDeparture_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(header.DestinationCustomsOfficeCodeForDepartureInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Actual Office of Destination", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Dest. Office", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "Destination Office", captionResourceString.MediumCaption);
				AssertEquals("FullDescription", "Customs Office of Destination", captionResourceString.FullDescription);
			});
		}

		public void TestDestinationCustomsOfficeCodeForArrival_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(header.DestinationCustomsOfficeCodeForArrivalInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Actual Office of Destination for Arrival", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Dest. Office (Arrival)", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "Destination Office for Arrival", captionResourceString.MediumCaption);
				AssertEquals("FullDescription", "Customs Office of Destination for Arrival", captionResourceString.FullDescription);
			});
		}

		public void TestDestinationTrader()
		{
			var oldDestinationTrader = header.DestinationTrader;
			oldDestinationTrader.Delete();

			CombineAssertions(() =>
			{
				var destinationTrader1 = header.DestinationTrader;
				AssertNotEquals("New DestinationTrader created", oldDestinationTrader.PK, destinationTrader1.PK);
				AssertSame("Cached", destinationTrader1, header.DestinationTrader);

				destinationTrader1.Delete();
				var destinationTrader2 = header.DocAddresses.CreateWithRequirement(header.DestinationTraderJobDocAddressRequirement);
				AssertEquals("DestinationTrader from DocAddresses", destinationTrader2.PK, header.DestinationTrader.PK);
			});
		}

		public void TestFindRelevantDepartureForCombinedMessage()
		{
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var departure = Factory.New<NctsHeader>();
			departure.BH_HeaderType = NctsMovementType.Codes.Departure;
			departure.MovementReferenceEntryNumber.CE_EntryNum = "1234567890";

			Factory.Save();

			CombineAssertions(() =>
			{
				var matchingDepartureResults = departure.FindRelevantDepartureRecordForCombinedDepartureAndArrival();
				AssertEquals($"Should return {NctsHeader.DepartureRecordFindResult.Unknown} when the declaration is not arrival or departure and arrival", NctsHeader.DepartureRecordFindResult.Unknown, matchingDepartureResults.result);
				AssertEquals("Should return a null object as it hasn't found a matching departure", null, matchingDepartureResults.departureHeaderFound);

				header.MovementReferenceEntryNumber.CE_EntryNum = ZString.Empty;
				matchingDepartureResults = header.FindRelevantDepartureRecordForCombinedDepartureAndArrival();
				AssertEquals($"Should return {NctsHeader.DepartureRecordFindResult.NothingFound} when there are no departures with matching MRN (empty in this case)", NctsHeader.DepartureRecordFindResult.NothingFound, matchingDepartureResults.result);
				AssertEquals("Should return a null object as it hasn't found a matching departure", null, matchingDepartureResults.departureHeaderFound);

				header.MovementReferenceEntryNumber.CE_EntryNum = "1234567890";
				matchingDepartureResults = header.FindRelevantDepartureRecordForCombinedDepartureAndArrival();
				AssertEquals($"Should return {NctsHeader.DepartureRecordFindResult.FoundByMatchingMrn} when there is a departure with matching MRN", NctsHeader.DepartureRecordFindResult.FoundByMatchingMrn, matchingDepartureResults.result);
				AssertEquals("Should return the matching departure object found", departure, matchingDepartureResults.departureHeaderFound);

				header.MovementReferenceEntryNumber.CE_EntryNum = ZString.Empty;
				header.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
				matchingDepartureResults = header.FindRelevantDepartureRecordForCombinedDepartureAndArrival();
				AssertEquals($"Should return {NctsHeader.DepartureRecordFindResult.AlreadyCombinedDepartureAndArrival} when the actual header is a combined departure and arrival", NctsHeader.DepartureRecordFindResult.AlreadyCombinedDepartureAndArrival
					, matchingDepartureResults.result);
				AssertEquals("Should return the actual departure and arrival object", header, matchingDepartureResults.departureHeaderFound);
			});
		}

		public void TestSupportsCombinedArrivalAndDepartureMessage()
		{
			AssertEquals(false, header.SupportsCombinedArrivalAndDepartureMessage);
		}

		public void TestUnloadingRemarksAllowedOverride()
		{
			AssertEquals(false, header.UnloadingRemarksAllowedOverride);
		}

		public void TestPlaceOfUnloadingCodeEmptyHeaderType()
		{
			header.BH_HeaderType = ZString.Empty;
			AssertNull("[PRE-CONDITION] MovementHeader", header.MovementHeader);

			AssertEquals("PlaceOfUnloadingCode", "", header.PlaceOfUnloadingCode);
			AssertExceptionThrown<InvalidOperationException>("Exception expected setting PlaceOfUnloadingCode when BH_HeaderType is empty", () => header.PlaceOfUnloadingCode = "DEDEK");
		}

		public void TestNctsGuaranteeIsCreatedWhenPrincipalIsSet2GuaranteediffADD()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "adresse ok";
			address.AddressCode = "adok";
			address.OA_OH = org.PK;

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "19860101";
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;
			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.ADD;
			rule.CPR_ValueFrom = "#1";

			var guaranteeHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader2.CPH_Number = "19860102";
			guaranteeHeader2.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader2.CPH_OH_PermitHolder = org.PK;
			rule = guaranteeHeader2.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.ADD;
			rule.CPR_ValueFrom = "#2";

			Factory.Save();

			var header = Factory.New<NctsHeaderForTest>();
			AssertEquals(0, header.Guarantees.Count);
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			header.Principal.Address.AddressCode = "adok";
			header.Principal.OrganisationPK = org.PK;
			AssertEquals(1, header.Guarantees.Count);
		}

		public void TestNctsGuaranteeHoldersOrder()
		{
			var principal = CreateOrgHeader("PRINCIPAL");
			var declarant = CreateOrgHeader("DECLARANT");
			var branchOrgProxy = CreateOrgHeader("BRANCH");
			var companyOrgProxy = CreateOrgHeader("COMPANY");

			var guaranteeHeader1 = CreateGuaranteeHeader("19860101");
			var guaranteeHeader2 = CreateGuaranteeHeader("19860102");
			var guaranteeHeader3 = CreateGuaranteeHeader("19860103");
			var guaranteeHeader4 = CreateGuaranteeHeader("19860104");

			Factory.Save();

			foreach (var useDeclarantFallback in new[] { true, false })
			{
				foreach (var useBranchProxyFallback in new[] { true, false })
				{
					foreach (var useCompanyOrgProxyFallback in new[] { true, false })
					{
						AssertUnderDifferentConfiguration(useDeclarantFallback, useBranchProxyFallback, useCompanyOrgProxyFallback);
					}
				}
			}

			void AssertUnderDifferentConfiguration(bool useDeclarantFallback, bool useBranchProxyFallback, bool useCompanyOrgProxyFallback)
			{
				guaranteeHeader1.CPH_OH_PermitHolder = Guid.Empty;
				guaranteeHeader2.CPH_OH_PermitHolder = Guid.Empty;
				guaranteeHeader3.CPH_OH_PermitHolder = Guid.Empty;
				guaranteeHeader4.CPH_OH_PermitHolder = Guid.Empty;

				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
				nctsHeader.Declarant.E2_OA_Address = declarant.MainAddress.PK;
				nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;
				nctsHeader.Company.GC_OH_OrgProxy = companyOrgProxy.PK;
				nctsHeader.Branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				var refresher = nctsHeader.GuaranteeRefresher;
				var movementHeader = nctsHeader.MovementHeader;
				using (NctsConfigurationTestHelper.TemporarilySetAllGuaranteeFallbackConfigurations(Factory, useDeclarantFallBack: useDeclarantFallback, useBranchProxyFallBack: useBranchProxyFallback, useOrgProxyFallBack: useCompanyOrgProxyFallback))
				{
					movementHeader.Guarantees.RemoveAndDeleteAll();
					guaranteeHeader1.CPH_OH_PermitHolder = companyOrgProxy.PK;
					refresher.PopulateGuaranteeWithFallbacks();
					AssertResultFallbackToCompanyOrgProxy();

					movementHeader.Guarantees.RemoveAndDeleteAll();
					guaranteeHeader2.CPH_OH_PermitHolder = branchOrgProxy.PK;
					refresher.PopulateGuaranteeWithFallbacks();
					AssertResultFallbackToBranchOrgProxy();

					movementHeader.Guarantees.RemoveAndDeleteAll();
					guaranteeHeader3.CPH_OH_PermitHolder = declarant.PK;
					refresher.PopulateGuaranteeWithFallbacks();
					AssertResultFallbackToDeclarant();

					movementHeader.Guarantees.RemoveAndDeleteAll();
					guaranteeHeader4.CPH_OH_PermitHolder = principal.PK;
					refresher.PopulateGuaranteeWithFallbacks();
					AssertEquals("Guarantee for principal should always be retrieved.", "19860104", movementHeader.Guarantees.Single().PW_BondNumber);
				}

				void AssertResultFallbackToCompanyOrgProxy()
				{
					if (useCompanyOrgProxyFallback)
					{
						AssertEquals("Guarantee for companyOrgProxy should be retrieved if useCompanyOrgProxyFallBack is true.", "19860101", movementHeader.Guarantees.Single().PW_BondNumber);
					}
					else
					{
						AssertEquals("Guarantee for companyOrgProxy would not be retrieved if useCompanyOrgProxyFallBack is false.", 0, movementHeader.Guarantees.Count);
					}
				}
				void AssertResultFallbackToBranchOrgProxy()
				{
					if (useBranchProxyFallback)
					{
						AssertEquals("Guarantee for branchOrgProxy should be retrieved if useBranchProxyFallback is true.", "19860102", movementHeader.Guarantees.Single().PW_BondNumber);
					}
					else
					{
						AssertResultFallbackToCompanyOrgProxy();
					}
				}
				void AssertResultFallbackToDeclarant()
				{
					if (useDeclarantFallback)
					{
						AssertEquals("Guarantee for declarant should be retrieved if useDeclarantFallback is true.", "19860103", movementHeader.Guarantees.Single().PW_BondNumber);
					}
					else
					{
						AssertResultFallbackToBranchOrgProxy();
					}
				}
			}

			OrgHeader CreateOrgHeader(ZString code)
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = code;
				return orgHeader;
			}

			CusGuaranteeHeader CreateGuaranteeHeader(ZString guaranteeNumber)
			{
				var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guaranteeHeader.CPH_Number = guaranteeNumber;
				guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.COD;
				return guaranteeHeader;
			}
		}

		public void TestNctsGuaranteeIsCreatedWhenPrincipalIsSet2GuaranteediffADD_OrgProxyFallback()
		{
			var companyOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyOrgProxy.PK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "19860101";
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_OH_PermitHolder = companyOrgProxy.PK;
			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.ADD;
			rule.CPR_ValueFrom = "#1";

			var guaranteeHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader2.CPH_Number = "19860102";
			guaranteeHeader2.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader2.CPH_OH_PermitHolder = companyOrgProxy.PK;
			rule = guaranteeHeader2.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.ADD;
			rule.CPR_ValueFrom = "#2";

			Factory.Save();

			var header = Factory.New<NctsHeaderForTest>();
			header.Company.GC_OH_OrgProxy = companyOrgProxy.PK;
			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfigurationUseOrgProxyFallback(Factory, useOrgProxyFallBack: false))
			{
				AssertEquals(0, header.Guarantees.Count);
				header.Principal.E2_OA_Address = org.MainAddress.PK;
				AssertEquals(0, header.Guarantees.Count);
			}

			header = Factory.New<NctsHeaderForTest>();
			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfigurationUseOrgProxyFallback(Factory, useOrgProxyFallBack: true))
			{
				AssertEquals(0, header.Guarantees.Count);
				header.Principal.E2_OA_Address = org.MainAddress.PK;
				AssertEquals(1, header.Guarantees.Count);
			}
		}

		public void TestNctsGuaranteeIsCreatedWhenPrincipalIsSet2GuaranteesameADD()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "adresse ok";
			address.AddressCode = "adok";
			address.OA_OH = org.PK;

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "19860101";
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;
			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.ADD;
			rule.CPR_ValueFrom = "#1";

			var guaranteeHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader2.CPH_Number = "19860102";
			guaranteeHeader2.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader2.CPH_OH_PermitHolder = org.PK;
			rule = guaranteeHeader2.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.ADD;
			rule.CPR_ValueFrom = "#1";

			Factory.Save();

			var header = Factory.New<NctsHeaderForTest>();
			AssertEquals(0, header.Guarantees.Count);
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			header.Principal.Address.AddressCode = "adok";
			header.Principal.OrganisationPK = org.PK;
			AssertEquals(0, header.Guarantees.Count);
		}

		public void TestGetC0009CountryCodes()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.Spain, Core.Constants.CountryCodes.France);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0063, "C0063 Desc");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0063, Core.Constants.CountryCodes.Germany, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { Core.Constants.CountryCodes.Spain, Core.Constants.CountryCodes.France }, header.GetC0009CountryCodes());
		}

		public void TestGetPermitRecords_ShouldNotAddTransactionsIfGuaranteeIsNull()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var principal = Factory.New<OrgHeader>();
			var importer = Factory.New<OrgHeader>();

			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Number = "20210101";
			guarantee.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guarantee.CPH_OH_PermitHolder = importer.PK;

			header.Principal.E2_OA_Address = principal.MainAddress.PK;
			header.BH_OA_Importer = importer.PK;
			var nctsGuarantee = header.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "20210101";
			nctsGuarantee.PW_BondAmount = 12m;
			AssertNull(nctsGuarantee.CusGuarantee);

			var permitRecords = header.GetPermitRecords();
			AssertEquals(0, permitRecords.Count);

			guarantee.CPH_OH_PermitHolder = principal.PK;
			AssertNotNull(nctsGuarantee.CusGuarantee);
			permitRecords = header.GetPermitRecords();
			AssertEquals(1, permitRecords.Count);
			AssertEquals(guarantee.PK, permitRecords[0].PermitHeader.PK);
		}

		public void TestJobReferenceNumberOnSaving()
		{
			CombineAssertions(() =>
			{
				header.BH_JobReference = ZString.Empty;
				AssertNoExceptionThrown("Save when BH_JobReference is empty", () => Factory.Save());

				var referenceNumber = header.BH_JobReference;
				AssertEquals("BH_JobReference is set", "NCT00000001", referenceNumber);

				AssertNoExceptionThrown("Save again", () => Factory.Save());
				AssertEquals("BH_JobReference is not overwritten", referenceNumber, header.BH_JobReference);
			});
		}

		[UseSnapshotProtection]
		public void TestJobReferenceNumberOnSaving_NoDuplicateReferenceException()
		{
			var dbConnection = ((CargoWise.Data.IDbConnected)Factory).Connection;
			header.OnSaving();
			var jobReference = header.BH_JobReference;
			dbConnection.RollbackTransaction();

			var newFactory = new BusinessObjectFactory();
			var header2 = newFactory.New<NctsHeader>();
			header2.SetMovementType(NctsMovementType.Codes.Departure);
			header2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("Saving header2", () => newFactory.Save());
				AssertEquals("header and header2 have the same BH_JobReference", jobReference, header2.BH_JobReference);

				dbConnection.BeginTransaction();
				AssertEquals("Before saved, header has duplicate BH_JobReference", jobReference, header.BH_JobReference);
				AssertNoExceptionThrown("Saving header", () => Factory.Save());
				AssertNotEquals("After saved, header has unique BH_JobReference", jobReference, header.BH_JobReference);
			});
		}

		public void TestJobReferenceNumberOnSaving_NoNullReferenceException()
		{
			var customisation = new BillOfLadingNumberCustomisation();
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.Direction].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.TransportMode].Include = true;
			using (CustomsDataRegistry.Instance.NctsLocalReferenceNumberCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customisation))
			{
				AssertNoExceptionThrown("Saving header", () => header.OnSaving());
			}
		}

		public void TestICommonGoodsItemsIntegratorProvider_CommonGoodsItemsIntegrator()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			AssertType<NctsCommonGoodsItemsIntegrator>(nctsHeader.CommonGoodsItemsIntegrator);
		}

		public void TestBH_FTZMove_DefaultOnTypeOfSecurity()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMoveHeaderType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var movementHeader = header.MovementHeader;
			header.BH_FTZMove = false;
			AssertEquals("Phase 4", false, header.BH_FTZMove);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			movementHeader.BM_TypeOfSecurity = "BTH";
			AssertEquals("Phase 5", true, header.BH_FTZMove);
		}

		public void TestBH_FTZMove_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(header.BH_FTZMoveInfo, NctsHeader.Phase5CaptionKey, "Safety and Security", "Safety/Security", "Safety");
		}

		public void TestBH_FTZMove_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(header.BH_FTZMoveInfo, NctsHeader.Phase4CaptionKey, "Safety and Security:", string.Empty, string.Empty);
		}

		public void TestEffectiveMessageStatus_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(header.EffectiveMessageStatusInfo, NctsHeader.Phase5CaptionKey, "Message Status", "Msg. Status", "Msg. Stat.");
		}

		public void TestEffectiveMessageStatus_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(header.EffectiveMessageStatusInfo, NctsHeader.Phase4CaptionKey, "Messaging Status", string.Empty, "Msg. St.");
		}

		public void TestMovementReferenceNumber_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(header.MovementReferenceNumberInfo, NctsHeader.Phase5CaptionKey, "Movement Reference Number", "Movement Reference", "MRN");
		}

		public void TestMovementReferenceNumber_Phase4Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(header.MovementReferenceNumberInfo, NctsHeader.Phase4CaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "MRN", captionResourceString.Caption);
				AssertEquals("FullDescription", "Movement Reference Number", captionResourceString.FullDescription);
			});
		}

		public void TestBH_ExportFlag_Phase4Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(header.BH_ExportFlagInfo, NctsHeader.Phase4CaptionKey);
			AssertEquals("Caption", "Event Flag", captionResourceString.Caption);
		}

		public void TestBH_ExportFlag_Phase5Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(header.BH_ExportFlagInfo, NctsHeader.Phase5CaptionKey);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Incident Flag", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Incident", captionResourceString.MediumCaption);
			});
		}

		public void TestCusInBondPersonType()
		{
			AssertEquals(typeof(CusInBondPerson), header.CusInBondPersonType);
		}

		public void TestLocationContact()
		{
			var locationContact = header.LocationContact;
			CombineAssertions(() =>
			{
				AssertSame("Same", locationContact, Customs.Business.CusInBondPerson.LoadOrCreate<CusInBondPerson>(header, CusInBondPerson.LocationContactType));
				AssertEquals("IsRegisteredEditableChildObject", true, header.IsRegisteredEditableChildObject(locationContact));

				locationContact.Delete();
				AssertNotEquals("Not same", locationContact.PK, header.LocationContact.PK);
			});
		}

		public void TestContactFullName()
		{
			header.LocationContact.CP_FullName = "AAA";
			AssertEquals("Get value", "AAA", header.ContactFullName);
		}

		public void TestContactFullName_Caption()
		{
			NCTSTestHelper.AssertCaptions(header.ContactFullNameInfo, "Contact", string.Empty, string.Empty);
		}

		public void TestContactFullName_MaxLength()
		{
			AssertEquals(70, header.ContactFullNameInfo.MaxLength);
		}

		public void TestContactPhone()
		{
			header.LocationContact.CP_Phone = "111111";
			AssertEquals("Get value", "111111", header.ContactPhone);
		}

		public void TestContactPhone_Caption()
		{
			NCTSTestHelper.AssertCaptions(header.ContactPhoneInfo, "Telephone", "Phone", "TEL");
		}

		public void TestContactPhone_MaxLength()
		{
			AssertEquals(35, header.ContactPhoneInfo.MaxLength);
		}

		public void TestContactEmail()
		{
			header.LocationContact.CP_Email = "111@abc.com";
			AssertEquals("Get value", "111@abc.com", header.ContactEmail);
		}

		public void TestContactEmail_Caption()
		{
			NCTSTestHelper.AssertCaptions(header.ContactEmailInfo, "Email", string.Empty, "EML");
		}

		public void TestContactEmail_MaxLength()
		{
			AssertEquals(256, header.ContactEmailInfo.MaxLength);
		}

		public void TestSettingFtzMoveToFalseWipesRelatedFields()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_FTZMove = true;
			var departureMovement = header.MovementHeader;
			var goodsItem = departureMovement.GoodsItems.AddNew();
			departureMovement.BM_AdditionalText = "12345";
			goodsItem.BY_CommercialReferenceNumber = "12345";
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "ABC", header.SecurityConsignor);
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "ABC", header.SecurityConsignee);
			CombineAssertions(() =>
			{
				AssertNotNull("Security Consignor is populated", header.SecurityConsignor);
				AssertNotNull("Security Consignee is populated", header.SecurityConsignee);
				AssertEquals("Check Commercial Reference Number", "12345", departureMovement.BM_AdditionalText);
				AssertEquals("Check Commercial Reference Number", "12345", goodsItem.BY_CommercialReferenceNumber);
			});

			header.BH_FTZMove = false;
			CombineAssertions(() =>
			{
				AssertEquals("Security Consignor should have been wiped", ZGuid.Empty, header.SecurityConsignor.OrganisationPK);
				AssertEquals("Security Consignee should have been wiped", ZGuid.Empty, header.SecurityConsignee.OrganisationPK);
				AssertEquals("Commercial Reference Number should have been wiped", string.Empty, departureMovement.BM_AdditionalText);
				AssertEquals("Commercial Reference Number should have been wiped", string.Empty, goodsItem.BY_CommercialReferenceNumber);
			});
		}

		public void TestIsPhase5Declaration()
		{
			CombineAssertions(() =>
			{
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
				AssertEquals("Set Phase 4", false, header.IsPhase5);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("Set Phase 5", true, header.IsPhase5);
			});
		}

		public void TestIsPhase4Declaration()
		{
			CombineAssertions(() =>
			{
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
				AssertEquals("Set Phase 4", true, header.IsPhase4);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("Set Phase 5", false, header.IsPhase4);
			});
		}

		public void TestMultipleKeysToUse_Phase4()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertSequencesEqual(new[] { NctsHeader.Phase4CaptionKey }, header.MultipleKeysToUse);
		}

		public void TestMultipleKeysToUse_Phase5Arrival()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			AssertSequencesEqual(new[] { NctsHeader.Phase5CaptionKey }, header.MultipleKeysToUse);
		}

		public void TestMultipleKeysToUse_Phase5Departure()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			AssertSequencesEqual(new[] { NctsHeader.Phase5DepartureCaptionKey, NctsHeader.Phase5CaptionKey }, header.MultipleKeysToUse);
		}

		public void TestGetIE29CusdecParser()
		{
			var message = Factory.New<NctsEdiMessage>();
			var result = header.GetIE29CusdecParser(message);
			AssertNull(result);
		}

		[ExpectNoExceptions]
		public void TestApportionedAmountToGuaranteesLiabilityAmount_ShouldCallCore()
		{
			CombineAssertions("Phase5Arrival", () =>
			{
				var headerMock = Factory.NewMoq<NctsHeader>();
				headerMock.Object.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				headerMock.Object.SetMovementType(NctsMovementType.Codes.Arrival);
				headerMock.Object.ApportionedAmountToGuaranteesLiabilityAmount();
				headerMock.Protected().Verify("ApportionedAmountToGuaranteesLiabilityAmountCore", Times.Never());
				headerMock.Protected().Verify("ApportionedAmountToGuaranteesLiabilityAmountCore_Phase5Arrival", Times.Once());
			});

			CombineAssertions("Departure", () =>
			{
				var headerMock = Factory.NewMoq<NctsHeader>();
				headerMock.Object.SetMovementType(NctsMovementType.Codes.Departure);
				headerMock.Object.ApportionedAmountToGuaranteesLiabilityAmount();
				headerMock.Protected().Verify("ApportionedAmountToGuaranteesLiabilityAmountCore", Times.Once());
				headerMock.Protected().Verify("ApportionedAmountToGuaranteesLiabilityAmountCore_Phase5Arrival", Times.Never());
			});
		}

		public void TestDefaultApplicationCode()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);
			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				var header = Factory.New<NctsHeader>();
				AssertEquals("Phase 5", CusInBondApplicationCodeList.Codes.NCTS5, header.BH_ApplicationCode);
			}

			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);
			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				var header = Factory.New<NctsHeader>();
				AssertEquals("Not phase 5", CusInBondApplicationCodeList.Codes.NCTS4, header.BH_ApplicationCode);
			}
		}

		public void TestDefaultCountryOfDispatchWhenIsEnabled()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = "IT";
			AssertEquals("[PRE-CONDITION] Country of Dispatch read only", false, header.BH_RL_NKImportLoadPortInfo.ReadOnly);

			header.Consignor.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("When Country of Dispatch is editable, Country of Dispatch", "IT", header.BH_RL_NKImportLoadPort);
			AssertEquals("Country of Dispatch read only", false, header.BH_RL_NKImportLoadPortInfo.ReadOnly);
		}

		public void TestDefaultCountryOfDispatchWhenIsDisabled()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.MovementHeader.GoodsItems.AddNew();
			AssertEquals("[PRE-CONDITION] Country of Dispatch read only", false, header.BH_RL_NKImportLoadPortInfo.ReadOnly);

			goodsItem.BY_RN_NKCountryOfDispatch = "GB";
			AssertEquals("Country of Dispatch read only", true, header.BH_RL_NKImportLoadPortInfo.ReadOnly);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = "IT";
			header.Consignor.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("When Country of Dispatch is disabled, Country of Dispatch", ZString.Empty, header.BH_RL_NKImportLoadPort);
			AssertEquals("Country of Dispatch read only", true, header.BH_RL_NKImportLoadPortInfo.ReadOnly);
		}

		public void TestDefaultCountryOfDestinationWhenIsEnabled()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			AssertEquals("[PRE-CONDITION] Country of Destination read only", false, header.MovementHeader.BM_RL_NKDestinationPortInfo.ReadOnly);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = "DE";
			header.Consignee.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("When Country of Destination is editable, Country of Destination", "DE", header.MovementHeader.BM_RL_NKDestinationPort);
			AssertEquals("Country of Destination read only", false, header.MovementHeader.BM_RL_NKDestinationPortInfo.ReadOnly);
		}

		public void TestDefaultCountryOfDestinationWhenIsDisabled()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.MovementHeader.GoodsItems.AddNew();
			AssertEquals("[PRE-CONDITION] Country of Destination read only", false, header.MovementHeader.BM_RL_NKDestinationPortInfo.ReadOnly);
			goodsItem.BY_RN_NKCountryOfDestination = "GB";
			AssertEquals("Country of Destination read only", true, header.MovementHeader.BM_RL_NKDestinationPortInfo.ReadOnly);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = "DE";
			header.Consignee.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("When Country of Destination is disabled, Country of Destination", ZString.Empty, header.MovementHeader.BM_RL_NKDestinationPort);
			AssertEquals("Country of Destination read only", true, header.MovementHeader.BM_RL_NKDestinationPortInfo.ReadOnly);
		}

		public void TestDefaultTraderAtDestinationManager()
		{
			AssertType<NctsDefaultTraderAtDestinationManager>("DefaultTraderAtDestinationManager Type", header.DefaultTraderAtDestinationManager);
		}

		public void TestDefaultPrincipalRegistryManager()
		{
			AssertType<NctsDefaultPrincipalRegistryManager>("DefaultPrincipalRegistryManager Type", header.DefaultPrincipalRegistryManager);
		}

		[ExpectNoExceptions]
		public void TestSetMovementType_SetInitialPrincipal_Departure()
		{
			var defaultPrincipalRegistryManagerMock = new Mock<INctsDefaultPrincipalRegistryManager>();
			defaultPrincipalRegistryManagerMock.Setup(mock => mock.ApplyDefaultingIfEnabled());

			var nctsHeaderMock = Factory.NewMoq<NctsHeaderForTest>();
			nctsHeaderMock
				.Protected()
				.Setup<INctsDefaultPrincipalRegistryManager>("GetNewDefaultPrincipalRegistryManager")
				.Returns(defaultPrincipalRegistryManagerMock.Object);
			nctsHeaderMock.Object.ResetDefaultPrincipalRegistryManagerExposed();
			nctsHeaderMock.Object.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderMock.VerifyAll();
			defaultPrincipalRegistryManagerMock.Verify(mock => mock.ApplyDefaultingIfEnabled(), Times.Once());
		}

		[ExpectNoExceptions]
		public void TestSetMovementType_SetInitialPrincipal_Arrival()
		{
			var defaultPrincipalRegistryManagerMock = new Mock<INctsDefaultPrincipalRegistryManager>();
			defaultPrincipalRegistryManagerMock.Setup(mock => mock.ApplyDefaultingIfEnabled());

			var nctsHeaderMock = Factory.NewMoq<NctsHeaderForTest>();
			nctsHeaderMock
				.Protected()
				.Setup<INctsDefaultPrincipalRegistryManager>("GetNewDefaultPrincipalRegistryManager")
				.Returns(defaultPrincipalRegistryManagerMock.Object);
			nctsHeaderMock.Object.ResetDefaultPrincipalRegistryManagerExposed();
			nctsHeaderMock.Object.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderMock.Object.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			defaultPrincipalRegistryManagerMock.Verify(mock => mock.ApplyDefaultingIfEnabled(), Times.Never());
		}

		public void TestBills_SequenceNumberSortedByB0_SystemCreateTimeUtc()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<NctsHeader>();
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var nctsBill = header.Bills.AddNew();
				var nctsBill2 = header.Bills.AddNew();
				Factory.Save();

				var anotherFactory = new BusinessObjectFactory();
				var nctsHeaderInAnotherFactory = anotherFactory.Load<NctsHeader>(header.PK);
				AssertEquals("Reloaded, SequenceNumber of nctsBill is 1", (ZShort)1, nctsHeaderInAnotherFactory.Bills.First(x => x.PK == nctsBill.PK).SequenceNumber);
				AssertEquals("Reloaded, SequenceNumber of nctsBill2 is 2", (ZShort)2, nctsHeaderInAnotherFactory.Bills.First(x => x.PK == nctsBill2.PK).SequenceNumber);
			});
		}

		public void TestTotalNumberOfPackages_Caption()
		{
			NCTSTestHelper.AssertCaptions(typeof(NctsHeader), nameof(header.TotalNumberOfPackages), string.Empty, "[6] Total Packages", string.Empty, "Packs");
		}

		public void TestTotalNumberOfItems_Caption()
		{
			NCTSTestHelper.AssertCaptions(typeof(NctsHeader), nameof(header.TotalNumberOfItems), string.Empty, "[5] No. of Items", string.Empty, "Items");
		}

		public void TestTotalGrossMassInKilograms_Caption()
		{
			NCTSTestHelper.AssertCaptions(typeof(NctsHeader), nameof(header.TotalGrossMassInKilograms), string.Empty, "[35] Total Gross Weight (kg)", string.Empty, "Total Gross");
		}

		public void TestCountryOfDispatch()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.UnitedKingdom;
			header.MovementHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.France;
			CombineAssertions(() =>
			{
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("NCTS4", Core.Constants.CountryCodes.UnitedKingdom, header.CountryOfDispatch);

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("NCTS5", Core.Constants.CountryCodes.France, header.CountryOfDispatch);
			});
		}

		public void TestCountryOfDispatch_Caption()
		{
			NCTSTestHelper.AssertCaptions(header.CountryOfDispatchInfo, "Country of Dispatch", "Ctry. of Dispatch", "Dispatch");
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)header).GetCusSupportingInfoTypes();
			CombineAssertions(() =>
			{
				AssertEquals("#CusSupportingInfoTypes", 3, cusSupportingInfoTypes.Count);
				AssertEquals("PRE", typeof(CommonPreviousDocument), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
				AssertEquals("OTH", typeof(NctsAdditionalInfo), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
				AssertEquals("IRD", typeof(RequestedDocument), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument]);
			});
		}

		public void TestPreviousDocuments_ReadOnly()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Phase 5 Arrival", false, bill.PreviousDocuments.ReadOnly);
				AssertEquals("Phase 5 Arrival Header", true, header.PreviousDocuments.ReadOnly);
			});
		}

		public void TestCountriesOfRouting()
		{
			var countriesOfRouting = header.CountriesOfRouting;
			CombineAssertions(() =>
			{
				AssertType<CountryOfRoutingCollection<CountryOfRouting>>(countriesOfRouting);
				AssertEquals("IsRegisteredEditableChildObject", true, header.IsRegisteredEditableChildObject(countriesOfRouting));
				AssertSame("Cached", countriesOfRouting, header.CountriesOfRouting);
			});
		}

		public void TestAdditionalDocuments()
		{
			var additionalDocuments = header.AdditionalDocuments;
			CombineAssertions(() =>
			{
				AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>(additionalDocuments);
				AssertEquals("IsRegisteredEditableChildObject", true, header.IsRegisteredEditableChildObject(additionalDocuments));
				AssertSame("Cached", additionalDocuments, header.AdditionalDocuments);

				var doc = additionalDocuments.AddNew();
				AssertType<NctsAdditionalInfo>(Factory.Load<CusSupportingInfo>(doc.PK));
			});
		}

		public void TestRequestedDocuments()
		{
			var requestedDocuments = header.RequestedDocuments;
			CombineAssertions(() =>
			{
				AssertType<RequestedDocumentCollection>(requestedDocuments);
				AssertEquals("IsRegisteredEditableChildObject", true, header.IsRegisteredEditableChildObject(requestedDocuments));
				AssertEquals("requestedDocuments.ReadOnly", true, requestedDocuments.ReadOnly);
				AssertSame("Cached", requestedDocuments, header.RequestedDocuments);

				var doc = requestedDocuments.AddNew();
				AssertType<RequestedDocument>(Factory.Load<CusSupportingInfo>(doc.PK));
			});
		}

		public void TestCusAuthorizationUsages()
		{
			AssertType<CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader>>(header.CusAuthorizationUsages);
		}

		public void TestDelete_CusAuthorizationUsages()
		{
			CombineAssertions(() =>
			{
				var authorizationUsage1 = header.CusAuthorizationUsages.AddNew();
				var authorizationUsage2 = header.CusAuthorizationUsages.AddNew();
				header.Delete();
				AssertEquals("authorizationUsage1 deleted", true, authorizationUsage1.IsDeleted);
				AssertEquals("authorizationUsage2 deleted", true, authorizationUsage2.IsDeleted);
			});
		}

		public void TestSettingBH_ExportFlagSetsReadOnlyOnIncidentGoodsLocation()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var incident = header.EnRouteIncidents.AddNew();
			incident.BN_CustomsStatus = "ONA";

			CombineAssertions(() =>
			{
				header.BH_ExportFlag = EventFlagList.Codes.Yes;
				AssertEquals("Export flag = 'Y': The goods location on the incident should not be read only", false, incident.GoodsLocation.ReadOnly);
				header.BH_ExportFlag = EventFlagList.Codes.No;
				incident = header.EnRouteIncidents.AddNew();
				incident.BN_CustomsStatus = "ONA";
				AssertEquals("Export flag = 'N': The goods location on the incident should be read only", true, incident.GoodsLocation.ReadOnly);
			});
		}

		public void TestIsArrivalEventAvailable()
		{
			CombineAssertions(() =>
			{
				header.BH_ExportFlag = "N";
				AssertEquals("Export flag = 'N': The IsArrivalEventAvailable flag should be false", false, header.IsArrivalEventAvailable);
				header.BH_ExportFlag = "Y";
				AssertEquals("Export flag = 'Y': The IsArrivalEventAvailable flag should be true", true, header.IsArrivalEventAvailable);
			});
		}

		public void TestCustomsOfficeUpdatedOnConsignorOrConsigneeChanged()
		{
			var currentCountryCode = Env.CurrentCompany.Country.Code;
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
			cusCode1.OK_RN_NKCodeCountry = currentCountryCode;
			cusCode1.OK_CodeType = "CTR";
			cusCode1.OK_CustomsRegNo = "LV001";
			cusCode1.OK_OA_PremisesAddress = address1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_Address1 = "Address2";
			address2.AddressCode = "add2";
			address2.OA_OH = org2.PK;
			var address3 = Factory.NewWithValidTestData<OrgAddress>();
			address3.OA_Address1 = "Address3";
			address3.AddressCode = "add3";
			address3.OA_OH = org2.PK;
			var cusCode2 = org2.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = currentCountryCode;
			cusCode2.OK_CodeType = "CTR";
			cusCode2.OK_CustomsRegNo = "LV002";
			cusCode2.OK_OA_PremisesAddress = address2.PK;
			var cusCode3 = org2.CustomsCodes.AddNew();
			cusCode3.OK_RN_NKCodeCountry = currentCountryCode;
			cusCode3.OK_CodeType = "CTR";
			cusCode3.OK_CustomsRegNo = "LV003";
			cusCode3.OK_OA_PremisesAddress = address3.PK;

			header.Consignee.E2_OA_Address = address1.PK;
			header.Consignor.E2_OA_Address = address2.PK;
			Factory.Save();

			AssertEquals("LV001", header.DestinationCustomsOfficeCode);
			AssertEquals("LV002", header.DepartureCustomsOfficeCode);

			header.Consignee.E2_OA_Address = address3.PK;
			header.Consignor.E2_OA_Address = address3.PK;

			AssertEquals("LV003", header.DestinationCustomsOfficeCode);
			AssertEquals("LV003", header.DepartureCustomsOfficeCode);
		}

		public void TestCustomsOfficesUpdatedOnCustomsOfficesForDepartureChanged()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;
			var officeCount = movementHeader.CustomsOffices.Count;
			var office = movementHeader.CustomsOfficesForDeparture.AddNew("DES");
			AssertEquals("add", officeCount + 1, movementHeader.CustomsOffices.Count);

			office.CY_Code = "DEP";
			AssertEquals("change", "DEP", movementHeader.CustomsOffices[officeCount].CY_Code);

			movementHeader.CustomsOfficesForDeparture.RemoveAndDelete(office);
			AssertEquals("delete", officeCount, movementHeader.CustomsOffices.Count);
		}

		public void TestJobReferenceNumber()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_JobReference = "NCTS001";
			AssertEquals("JobNumber returns BH_JobReference on standalone NCTS Job.", "NCTS001", header.JobNumber);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0000001";
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			AssertEquals("JobNumber returns JS_UniqueConsignRef when attached on a Shipment.", "S0000001", header.JobReferenceNumber);
		}

		public void TestCustomsOfficesForDepartureUpdatedOnCustomsOfficesChanged()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;
			var officeForDepartureCount = movementHeader.CustomsOfficesForDeparture.Count;
			var office = movementHeader.CustomsOffices.AddNew("DES");
			AssertEquals("add", officeForDepartureCount + 1, movementHeader.CustomsOfficesForDeparture.Count);

			office.CY_Code = "DEP";
			AssertEquals("change", "DEP", movementHeader.CustomsOffices[officeForDepartureCount].CY_Code);

			movementHeader.CustomsOffices.RemoveAndDelete(office);
			AssertEquals("delete", officeForDepartureCount, movementHeader.CustomsOfficesForDeparture.Count);
		}

		public void TestJob_WhenNotLinkedToShipment()
		{
			AssertNull("When the JobHeader is not created yet, Job", header.Job);

			new JobHeader.Loader(header).TryLoadOrCreate();
			AssertNotNull("When the JobHeader is created, Job", header.Job);
			AssertEquals("Job ParentID", header.PK, header.Job.JH_ParentID);
		}

		public void TestJob_WhenLinkedToShipment()
		{
			AssertNull("When the JobHeader is not created yet, Job", header.Job);

			var shipment = Factory.New<CommonShipment>();
			header.BH_ParentID = shipment.PK;
			AssertNull("When the linked shipment does not have a JobHeader, Job", header.Job);

			new JobHeader.Loader(shipment).TryLoadOrCreate();
			AssertNotNull("When the linked shipment has a JobHeader, Job", header.Job);
			AssertSame("Shipment Job and NctsHeader Job are the same", shipment.Job, header.Job);
			AssertEquals("Job ParentID", shipment.PK, header.Job.JH_ParentID);
		}

		public void TestGetFetchStrategy()
		{
			CombineAssertions(() =>
			{
				AssertType<NctsHeaderFetchStrategy>(header.FetchStrategy);

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertType<NctsHeaderPhase5FetchStrategy>(header.FetchStrategy);
			});
		}

		public void TestSetInitialCustomsOffice()
		{
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			header.SetInitialCustomsOffice();

			Assert("There should be a DEP customs office", header.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture));
			Assert("There should be a DES customs office", header.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination));

			var header2 = Factory.New<NctsHeader>();
			header2.BH_HeaderType = NctsMovementType.Codes.Arrival;
			header2.SetInitialCustomsOffice();

			Assert("There should be no DEP customs office", !header2.ArrivalMovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture));
			Assert("There should be no DES customs office", !header2.ArrivalMovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination));
		}

		public void TestSetMovementType_InitialiseCustomsOffice()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			Assert("There should be a DEP customs office", header.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture));
			Assert("There should be a DES customs office", header.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination));

			var header2 = Factory.New<NctsHeader>();
			header2.SetMovementType(NctsMovementType.Codes.Arrival);

			Assert("There should be no DEP customs office", !header2.ArrivalMovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture));
			Assert("There should be no DES customs office", !header2.ArrivalMovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination));
		}

		public void TestAutomatisationOfTraCustomsOfficeWithLoadCountryofDispatchAndDestinationPort()
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

			var customsOfficesForDeparture = header.CustomsOfficesForDeparture;
			AssertEquals("There should be NO TRA customs office in CustomsOfficesForDeparture", 0, customsOfficesForDeparture.GetElementsHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).Length);

			header.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Italy;
			header.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;

			AssertEquals("There should be no TRA customs office as IsCountryEuOrCtCountry and IsMemberOfEU are true for italy", 0, customsOfficesForDeparture.GetElementsHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).Length);

			header.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.UnitedKingdom;
			header.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals("There should be no TRA customs office as IsCountryEuOrCtCountry and IsMemberOfEU are false for united kingdom", 0, customsOfficesForDeparture.GetElementsHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).Length);

			header.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Switzerland;
			AssertEquals("There should be TRA customs office as IsCountryEuOrCtCountry is true and IsMemberOfEU is false for Switzerland", 1, header.CustomsOffices.GetElementsHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).Length);
			AssertEquals("CustomsOfficesForDeparture should reloaded and get a TRA customs office", 1, customsOfficesForDeparture.GetElementsHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).Length);

			header.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Switzerland;
			AssertEquals("TRA office should have the same data as DES office as Tra office exist and BM_RL_NKDestinationPort &&  BH_RL_NKImportLoadPort has same country.", "FR002300", header.CustomsOffices.GetFirstElementHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).CY_Data);

			var header2 = Factory.New<NctsHeader>();
			header2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header2.SetMovementType(NctsMovementType.Codes.Departure);
			var office2 = header2.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			office2.CY_Data = "FR002300";
			header2.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Switzerland;

			AssertEquals("There should be no TRA customs office as BH_RL_NKImportLoadPort has not been set yet.", 0, header2.CustomsOffices.GetElementsHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).Length);

			header2.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Switzerland;
			AssertEquals("TRA office should have been created with the same data as DES office as BM_RL_NKDestinationPort &&  BH_RL_NKImportLoadPort has same country.", "FR002300", header2.CustomsOffices.GetFirstElementHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).CY_Data);

			var header3 = Factory.New<NctsHeader>();
			header3.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header3.SetMovementType(NctsMovementType.Codes.Departure);
			var office3 = header3.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			office3.CY_Data = "FR002300";
			header3.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;

			AssertEquals("There should be no TRA customs office as BH_RL_NKImportLoadPort has not been set yet.", 0, header3.CustomsOffices.GetElementsHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).Length);

			header3.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Switzerland;
			AssertEquals("TRA office should have been created with empty value as BM_RL_NKDestinationPort &&  BH_RL_NKImportLoadPort has different country.", "", header3.CustomsOffices.GetFirstElementHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).CY_Data);

			header3.BH_RL_NKImportLoadPort = ZString.Empty;
			header3.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Switzerland;
			AssertEquals("TRA office should have not been created if TRA office exists.", 1, header3.CustomsOffices.GetElementsHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).Length);

			header3.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Italy;
			AssertEquals("TRA office should have been valorised with NCTSOfficeOfDestination cy data as BM_RL_NKDestinationPort &&  BH_RL_NKImportLoadPort has same country.", "FR002300", header3.CustomsOffices.GetFirstElementHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).CY_Data);
		}

		public void TestConsigneeRequirementValidation()
		{
			var consignee = header.Consignee;
			consignee.E2_AddressOverride = true;
			JobDocAddressValidationHelperTest.AssertOverrideValidations(consignee);
		}

		public void TestConsignorRequirementValidation()
		{
			var consignor = header.Consignor;
			consignor.E2_AddressOverride = true;
			JobDocAddressValidationHelperTest.AssertOverrideValidations(consignor);
		}

		public void TestPrincipalRequirementValidation()
		{
			var principal = header.Principal;
			principal.E2_AddressOverride = true;
			JobDocAddressValidationHelperTest.AssertOverrideValidations(principal);
		}

		public void TestPrincipalRequirementValidation_WorkPhoneTR0079()
		{
			var header = CreatePhase5DepartureHeader();
			var principal = header.Principal;

			JobDocAddressValidationHelperTest.AssertTR0079Validation(principal, header);
		}

		public void TestConsigneeRequirementValidation_WorkPhoneTR0079()
		{
			var header = CreatePhase5DepartureHeader();
			var consignee = header.Consignee;

			JobDocAddressValidationHelperTest.AssertTR0079Validation(consignee, header);
		}

		public void TestConsignorRequirementValidation_WorkPhoneTR0079()
		{
			var header = CreatePhase5DepartureHeader();
			var consignor = header.Consignor;

			JobDocAddressValidationHelperTest.AssertTR0079Validation(consignor, header);
		}

		public void TestConsigneeAdditionalValidation()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsHeaderConsigneeJobDocAddressValidation>(nctsHeader.Consignee.AdditionalValidation);
		}

		public void TestAdditionalInfoSequenceGenerator()
		{
			AssertType<ShortSequenceNumberGenerator>("Sequence generator for Additional Documents type REF", header.RefSequenceNumberGenerator);
			AssertType<ShortSequenceNumberGenerator>("Sequence generator for Additional Documents type INF", header.InfSequenceNumberGenerator);
			AssertType<ShortSequenceNumberGenerator>("Sequence generator for Additional Documents type TRA", header.TraSequenceNumberGenerator);
		}

		public void TestArrivalMrnFromUser_ReadOnly()
		{
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(header.ArrivalMrnFromUserInfo, header);
		}

		public void TestBH_ExportFlag_ReadOnly()
		{
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(header.BH_ExportFlagInfo, header);
		}

		public void TestLocalReferenceNumber_ReadOnly()
		{
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(header.LocalReferenceNumberInfo, header);
		}

		public void TestDestinationCustomsOfficeCodeForArrival_ReadOnly()
		{
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(header.DestinationCustomsOfficeCodeForArrivalInfo, header);
		}

		public void TestBH_CommunicationLanguage_ReadOnly()
		{
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(header.BH_CommunicationLanguageInfo, header);
		}

		public void TestConfiguration_Cached()
		{
			var configuration = header.Configuration;
			AssertSame(configuration, Factory.GetCachedValue("NctsConfiguration_LV", () => new NctsConfiguration()));
		}

		public void TestSetMovementType_SetInitialStatus()
		{
			CombineAssertions(() =>
			{
				header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(NctsMovementType.Codes.Departure);
				AssertEquals("Departure", NctsMessageStatusList.Codes.DepartureDeclarationNotSent, header.EffectiveMessageStatus);

				var arrivalHeader = Factory.New<NctsHeader>();
				arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				AssertEquals("Phase4 Arrival", NctsMessageStatusList.Codes.ArrivalNotificationNotSent, arrivalHeader.EffectiveMessageStatus);

				var arrivalHeader_NCTS5 = Factory.New<NctsHeader>();
				arrivalHeader_NCTS5.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				arrivalHeader_NCTS5.SetMovementType(NctsMovementType.Codes.Arrival);
				AssertEquals("Phase5 Arrival", NctsMessageStatusList.Codes.Unknown, arrivalHeader_NCTS5.ArrivalMovementHeader.BM_MessageStatus);
			});
		}

		public void TestPrincipal_ShouldAddTRDAuthorization()
		{
			// Arrange
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			var cusAuthorizationHeader = CreateCusAuthorizationHeader(CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, organization);
			var movementHeader = header.MovementHeader;
			movementHeader.BM_ReducedDatasetIndicator = true;

			AssertCollectionNotContains("Precondition", movementHeader.CusAuthorizationUsages, cau => cau.AGC_Code == CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset);

			// Act
			header.Principal.OrganisationPK = organization.PK;

			// Assert
			AssertCollectionContains(movementHeader.CusAuthorizationUsages,
				cau => cau.AGC_Code == cusAuthorizationHeader.CPH_Type &&
													cau.AGC_Number == cusAuthorizationHeader.CPH_Number &&
													cau.AGC_OH_Owner == cusAuthorizationHeader.CPH_OH_PermitHolder);
		}

		public void TestPrincipal_ShouldDeleteTRDAuthorization_WhenPrincipalDeleted()
		{
			// Arrange
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			CreateCusAuthorizationHeader(CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, organization);
			var movementHeader = header.MovementHeader;
			movementHeader.BM_ReducedDatasetIndicator = true;
			header.Principal.OrganisationPK = organization.PK;

			AssertCollectionContains("Precondition", movementHeader.CusAuthorizationUsages, cau => cau.AGC_Code == CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset);

			// Act
			header.Principal.OrganisationPK = ZGuid.Empty;

			// Assert
			AssertCollectionNotContains(movementHeader.CusAuthorizationUsages, cau => cau.AGC_Code == CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset);
		}

		public void TestPrincipal_ShouldDeleteTRDAuthorization_WhenPrincipalChangedAndHasNoAuthorization()
		{
			// Arrange
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();

			CreateCusAuthorizationHeader(CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, organization1);
			var movementHeader = header.MovementHeader;
			movementHeader.BM_ReducedDatasetIndicator = true;
			header.Principal.OrganisationPK = organization1.PK;

			AssertCollectionContains("Precondition", movementHeader.CusAuthorizationUsages, cau => cau.AGC_Code == CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset);

			// Act
			header.Principal.OrganisationPK = organization2.PK;

			// Assert
			AssertCollectionNotContains(movementHeader.CusAuthorizationUsages, cau => cau.AGC_Code == CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset);
		}

		public void TestPrincipal_ShouldUpdateTRDAuthorization()
		{
			// Arrange
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();

			var authorizationHeader1 = CreateCusAuthorizationHeader(CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, organization1);
			var authorizationHeader2 = CreateCusAuthorizationHeader(CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, organization2);
			var movementHeader = header.MovementHeader;
			movementHeader.BM_ReducedDatasetIndicator = true;
			header.Principal.OrganisationPK = organization1.PK;

			AssertCollectionContains("Precondition", movementHeader.CusAuthorizationUsages,
				cau => cau.AGC_Code == authorizationHeader1.CPH_Type &&
													cau.AGC_Number == authorizationHeader1.CPH_Number &&
													cau.AGC_OH_Owner == authorizationHeader1.CPH_OH_PermitHolder);

			// Act
			header.Principal.OrganisationPK = organization2.PK;

			// Assert
			AssertCollectionNotContains(movementHeader.CusAuthorizationUsages,
				cau => cau.AGC_Code == authorizationHeader1.CPH_Type &&
													cau.AGC_Number == authorizationHeader1.CPH_Number &&
													cau.AGC_OH_Owner == authorizationHeader1.CPH_OH_PermitHolder);
			AssertCollectionContains("Precondition", movementHeader.CusAuthorizationUsages,
				cau => cau.AGC_Code == authorizationHeader2.CPH_Type &&
													cau.AGC_Number == authorizationHeader2.CPH_Number &&
													cau.AGC_OH_Owner == authorizationHeader2.CPH_OH_PermitHolder);
		}

		public void TestPrincipal_ShouldAddACRAuthorization()
		{
			AssertPrincipal_ShouldAddAuthorization(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
		}

		public void TestPrincipal_ShouldAddACRAuthorization_OrgProxyFallback()
		{
			AssertPrincipal_ShouldAddAuthorization_OrgProxyFallback(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
		}

		public void TestPrincipal_ShouldDeleteACRAuthorization_WhenPrincipalDeleted()
		{
			AssertPrincipal_ShouldDeleteAuthorization_WhenPrincipalDeleted(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
		}

		public void TestPrincipal_ShouldDeleteACRAuthorization_WhenPrincipalChangedAndHasNoAuthorization()
		{
			AssertPrincipal_ShouldDeleteAuthorization_WhenPrincipalChangedAndHasNoAuthorization(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
		}

		public void TestPrincipal_ShouldUpdateACRAuthorization()
		{
			AssertPrincipal_ShouldUpdateAuthorization(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
		}

		public void TestPrincipal_ShouldAddSSEAuthorization()
		{
			AssertPrincipal_ShouldAddAuthorization(CusAuthorizationHeaderTypeList.Codes.SpecialSeals);
		}

		public void TestPrincipal_ShouldAddSSEAuthorization_OrgProxyFallback()
		{
			AssertPrincipal_ShouldAddAuthorization_OrgProxyFallback(CusAuthorizationHeaderTypeList.Codes.SpecialSeals);
		}

		public void TestPrincipal_ShouldDeleteSSEAuthorization_WhenPrincipalDeleted()
		{
			AssertPrincipal_ShouldDeleteAuthorization_WhenPrincipalDeleted(CusAuthorizationHeaderTypeList.Codes.SpecialSeals);
		}

		public void TestPrincipal_ShouldDeleteSSEAuthorization_WhenPrincipalChangedAndHasNoAuthorization()
		{
			AssertPrincipal_ShouldDeleteAuthorization_WhenPrincipalChangedAndHasNoAuthorization(CusAuthorizationHeaderTypeList.Codes.SpecialSeals);
		}

		public void TestPrincipal_ShouldUpdateSSEAuthorization()
		{
			AssertPrincipal_ShouldUpdateAuthorization(CusAuthorizationHeaderTypeList.Codes.SpecialSeals);
		}

		public void TestSentToCustoms()
		{
			var sentToCustoms = new[] { LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent };
			var messageStatusList = new NctsMessageStatusList();
			foreach (var status in messageStatusList.GetAllCodes())
			{
				header.EffectiveMessageStatus = status;
				var expected = sentToCustoms.Contains(status);
				AssertEquals($"Status {status} ", expected, header.MessageHasBeenSent);
			}
		}

		public void TestBranchPk()
		{
			AssertEquals("BranchPk should match Primary Key of Branch (BH_GB)", header.BH_GB, header.BranchPk);
		}

		public void TestDeclarationType()
		{
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			AssertEquals("Declaration Type should be NctsArrivalNotification", EUJobMessageTypeList.Codes.NctsArrivalNotification, header.DeclarationType);

			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertEquals("Declaration Type should be NctsDeparture", EUJobMessageTypeList.Codes.NctsDeparture, departureHeader.DeclarationType);
		}

		public void TestLockFile()
		{
			header.LockFile("Test lock reference");
			var lckEventlog = header.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.LockForEditCode).First();

			CombineAssertions(() =>
			{
				AssertEquals("LCK-event active", false, lckEventlog.IsCancelled);
				AssertEquals("EventReference", "Test lock reference", lckEventlog.SL_Reference);
			});
		}

		public void TestUnlockFile()
		{
			header.UnlockFile("Test unlock reference");
			var uckEventLog = header.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.UnlockForEditCode).First();

			CombineAssertions(() =>
			{
				AssertEquals("UCK-event should be active", false, uckEventLog.IsCancelled);
				AssertEquals("EventReference", "Test unlock reference", uckEventLog.SL_Reference);
			});
		}

		public void TestGetNewNctsShipmentSynchroniser()
		{
			var sourceShipment = Factory.New<ForwardingShipment>();
			header.BH_ParentID = sourceShipment.PK;
			header.BH_ParentTableCode = sourceShipment.TablePrefix;

			CombineAssertions(() =>
			{
				AssertType<NctsShipmentSynchroniser>("Phase4", header.Synchroniser);

				var phase5ArrivalHeader = Factory.New<NctsHeader>();
				phase5ArrivalHeader.BH_ParentID = sourceShipment.PK;
				phase5ArrivalHeader.BH_ParentTableCode = sourceShipment.TablePrefix;
				phase5ArrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				phase5ArrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

				AssertType<NctsPhase5ArrivalShipmentSynchroniser>("Phase5A", phase5ArrivalHeader.Synchroniser);

				var phase5DepartureHeader = Factory.New<NctsHeader>();
				phase5DepartureHeader.BH_ParentID = sourceShipment.PK;
				phase5DepartureHeader.BH_ParentTableCode = sourceShipment.TablePrefix;
				phase5DepartureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				phase5DepartureHeader.SetMovementType(NctsMovementType.Codes.Departure);

				AssertType<NctsPhase5DepartureShipmentSynchroniser>("Phase5D", phase5DepartureHeader.Synchroniser);
			});
		}

		public void TestGetNewNctsConsolSynchroniser()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			header.BH_ParentID = sourceConsol.PK;
			header.BH_ParentTableCode = sourceConsol.TablePrefix;

			CombineAssertions(() =>
			{
				AssertType<NctsConsolSynchroniser>("Phase4", header.Synchroniser);

				var phase5ArrivalHeader = Factory.New<NctsHeader>();
				phase5ArrivalHeader.BH_ParentID = sourceConsol.PK;
				phase5ArrivalHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
				phase5ArrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				phase5ArrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

				AssertType<NctsPhase5ArrivalConsolSynchroniser>("Phase5A", phase5ArrivalHeader.Synchroniser);

				var phase5DepartureHeader = Factory.New<NctsHeader>();
				phase5DepartureHeader.BH_ParentID = sourceConsol.PK;
				phase5DepartureHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
				phase5DepartureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				phase5DepartureHeader.SetMovementType(NctsMovementType.Codes.Departure);

				AssertType<NctsPhase5DepartureConsolSynchroniser>("Phase5D", phase5DepartureHeader.Synchroniser);
			});
		}

		public void TestIsLocked()
		{
			CombineAssertions(() =>
			{
				header.LockFile("Test Lock Reference");
				AssertEquals("IsLocked - true", true, header.IsLocked);
				header.UnlockFile("Test Unlock Reference");
				AssertEquals("IsLocked - false", false, header.IsLocked);
			});
		}

		public void TestCanLockUnlockDeclaration_Phase4()
		{
			AssertCanLockUnlockDeclaration(CusInBondApplicationCodeList.Codes.NCTS4);
		}

		public void TestCanLockUnlockDeclaration_Phase5()
		{
			AssertCanLockUnlockDeclaration(CusInBondApplicationCodeList.Codes.NCTS5);
		}

		void AssertCanLockUnlockDeclaration(string applicationCode)
		{
			CombineAssertions(() =>
			{
				header.BH_ApplicationCode = applicationCode;
				header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				AssertEquals("No registry > no possibility to lock declaration", false, header.CanLockUnlockDeclaration(EUJobMessageTypeList.Codes.NctsArrivalNotification));
				var declarationConfig = new DeclarationLockConfig() { DeclarationType = EUJobMessageTypeList.Codes.NctsArrivalNotification, };
				var tabInfo = declarationConfig.TabInfos.AddNew();
				tabInfo.TabPage = "ARN";
				var eventInfo = declarationConfig.EventInfos.AddNew();
				eventInfo.EventReference = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				eventInfo.EventType = Events.CustomsEntryStatusCode;
				eventInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader;

				using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DeclarationLockConfigCollection() { declarationConfig }))
				{
					Factory.Save();
					AssertEquals("Registry exists, no event with correct reference > no possibility to lock declaration", false, header.CanLockUnlockDeclaration(EUJobMessageTypeList.Codes.NctsArrivalNotification));
					header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
					Factory.Save();
					AssertEquals("Registry and event with correct reference exists > possible to lock declaration", true, header.CanLockUnlockDeclaration(EUJobMessageTypeList.Codes.NctsArrivalNotification));
				}
			});
		}

		public void TestLockFileIfEnabledByConfiguration()
		{
			SetCustomsStatusAndLogCESEvent();
			header.LockFileIfEnabledByConfiguration("Test lock reference", EUJobMessageTypeList.Codes.NctsArrivalNotification);

			CombineAssertions(() =>
			{
				var lckEventlog = header.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.LockForEditCode).FirstOrDefault();
				AssertNull("LCK-event not created", lckEventlog);

				var declarationConfig = new DeclarationLockConfig() { DeclarationType = EUJobMessageTypeList.Codes.NctsArrivalNotification, };
				var tabInfo = declarationConfig.TabInfos.AddNew();
				tabInfo.TabPage = "ARN";
				var eventInfo = declarationConfig.EventInfos.AddNew();
				eventInfo.EventReference = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				eventInfo.EventType = Events.CustomsEntryStatusCode;
				eventInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader;
				using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DeclarationLockConfigCollection() { declarationConfig }))
				{
					header.LockFileIfEnabledByConfiguration("Test lock reference", EUJobMessageTypeList.Codes.NctsArrivalNotification);
				}
				lckEventlog = header.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.LockForEditCode).First();
				AssertEquals("LCK-event active", false, lckEventlog.IsCancelled);
				AssertEquals("EventReference", "Test lock reference", lckEventlog.SL_Reference);
			});
		}

		public void TestUnlockFileIfEnabledByConfiguration()
		{
			SetCustomsStatusAndLogCESEvent();
			header.UnlockFileIfEnabledByConfiguration("Test unlock reference", EUJobMessageTypeList.Codes.NctsArrivalNotification);

			CombineAssertions(() =>
			{
				var uckEventLog = header.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.UnlockForEditCode).FirstOrDefault();
				AssertNull("UCK-event should not be created", uckEventLog);

				var declarationConfig = new DeclarationLockConfig() { DeclarationType = EUJobMessageTypeList.Codes.NctsArrivalNotification, };
				var tabInfo = declarationConfig.TabInfos.AddNew();
				tabInfo.TabPage = "ARN";
				var eventInfo = declarationConfig.EventInfos.AddNew();
				eventInfo.EventReference = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				eventInfo.EventType = Events.CustomsEntryStatusCode;
				eventInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader;
				using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DeclarationLockConfigCollection() { declarationConfig }))
				{
					header.UnlockFileIfEnabledByConfiguration("Test unlock reference", EUJobMessageTypeList.Codes.NctsArrivalNotification);
				}
				uckEventLog = header.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.UnlockForEditCode).First();
				AssertEquals("UCK-event should be active", false, uckEventLog.IsCancelled);
				AssertEquals("EventReference", "Test unlock reference", uckEventLog.SL_Reference);
			});
		}

		void SetCustomsStatusAndLogCESEvent()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			header.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
			Factory.Save();
			header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
			Factory.Save();
		}

		public void TestRecalculateGuaranteeAmount_DutyAndVAT()
		{
			var phase5DepartureHeader = Factory.NewWithValidTestData<NctsHeader>();
			phase5DepartureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			phase5DepartureHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var guarantee = CreateNctsGuarantee(phase5DepartureHeader);
			var goodsItem = phase5DepartureHeader.Bills.AddNew().GoodsItems.AddNew();

			NCTSTestHelper.SetUpTariff(Factory);

			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_MonetaryValue = 1_000m;

			CombineAssertions(() =>
			{
				AssertEquals("Duty Amount", 120m, goodsItem.DutyAmount);
				AssertEquals("Vat Amount", 224m, goodsItem.VatAmount);

				guarantee.PW_SuretyCode = "ZER";
				Factory.Save();
				AssertEquals("Liability Amount for fraction ZER", 0m, guarantee.PW_BondAmount);
				guarantee.PW_SuretyCode = "THI";
				Factory.Save();
				AssertEquals("Liability Amount for fraction THI", new ZDecimal(103.20), guarantee.PW_BondAmount);
				guarantee.PW_SuretyCode = "HAL";
				Factory.Save();
				AssertEquals("Liability Amount for fraction HAL", 172m, guarantee.PW_BondAmount);
				guarantee.PW_SuretyCode = "FUL";
				Factory.Save();
				AssertEquals("Liability Amount for fraction FUL", 344m, guarantee.PW_BondAmount);

				goodsItem.Delete();
				Factory.Save();
				Assert("Goods items collection InitialApportionedAmount should be greater than 0", phase5DepartureHeader.Bills.FirstOrDefault().GoodsItems.InitialApportionedAmount > 0.00m);
				AssertEquals("Liability Amount when there is no amount on the goods items", 0.00m, guarantee.PW_BondAmount);
			});
		}

		public void TestRecalculateGuaranteeAmount_AntiDumpingAndCountervailing()
		{
			var phase5DepartureHeader = Factory.NewWithValidTestData<NctsHeader>();
			phase5DepartureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			phase5DepartureHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var guarantee = CreateNctsGuarantee(phase5DepartureHeader);
			var goodsItem1 = phase5DepartureHeader.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem2 = phase5DepartureHeader.Bills.AddNew().GoodsItems.AddNew();

			SetupDataForGetAdditionalCodesForRateType(Customs.Universal.Constants.RateTypes.AntiDumping, Customs.Universal.Constants.RateTypes.Countervailing);
			CreateCountervailingDutyAmount(goodsItem1);
			CreateAntiDumpingDutyAmount(goodsItem2);

			CombineAssertions(() =>
			{
				AssertEquals("Duty Amount 1", 0m, goodsItem1.DutyAmount);
				AssertEquals("Vat Amount 1", 0m, goodsItem1.VatAmount);
				AssertEquals("Duty Amount 2", 0m, goodsItem2.DutyAmount);
				AssertEquals("Vat Amount 2", 0m, goodsItem2.VatAmount);
				AssertEquals("Anti Dumping Amount 1", 4.3m, goodsItem1.AntiDumpingDutyAmount);
				AssertEquals("Anti Dumping Amount 2", 4.3m, goodsItem2.AntiDumpingDutyAmount);
				AssertEquals("Countervailing Amount 1", 0m, goodsItem1.CountervailingDutyAmount);
				AssertEquals("Countervailing Amount 2", 0m, goodsItem2.CountervailingDutyAmount);

				guarantee.PW_SuretyCode = "ZER";
				Factory.Save();
				AssertEquals("Liability Amount for fraction ZER", 0m, guarantee.PW_BondAmount);
				guarantee.PW_SuretyCode = "THI";
				Factory.Save();
				AssertEquals("Liability Amount for fraction THI", new ZDecimal(2.58), guarantee.PW_BondAmount);
				guarantee.PW_SuretyCode = "HAL";
				Factory.Save();
				AssertEquals("Liability Amount for fraction HAL", 4.3m, guarantee.PW_BondAmount);
				guarantee.PW_SuretyCode = "FUL";
				Factory.Save();
				AssertEquals("Liability Amount for fraction FUL", 8.60m, guarantee.PW_BondAmount);

				goodsItem1.Delete();
				goodsItem2.Delete();
				Factory.Save();
				Assert("Goods items collection InitialApportionedAmount should be greater than 0", phase5DepartureHeader.Bills.FirstOrDefault().GoodsItems.InitialApportionedAmount > 0.00m);
				AssertEquals("Liability Amount when there is no amount on the goods items", 0.00m, guarantee.PW_BondAmount);
			});
		}

		public void TestIWarehouseIntegrationSupporter()
		{
			var phase5DepartureHeader = Factory.NewWithValidTestData<NctsHeader>();
			phase5DepartureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			phase5DepartureHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			phase5DepartureHeader.MovementReferenceEntryNumber.CE_EntryNum = "NCTS00001";
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			phase5DepartureHeader.BH_OA_Importer = orgAddress.PK;
			phase5DepartureHeader.MovementHeader.BM_OA_WarehouseAddress = orgAddress.PK;
			phase5DepartureHeader.MovementHeader.BM_WarehouseTransactionStatus = "ICT";
			var supporter = (IWarehouseIntegrationSupporter)phase5DepartureHeader;
			CombineAssertions(() =>
			{
				AssertEquals("GetMessageErrorOfRequiredFieldsForBondedWarehousing", ZString.Empty, supporter.GetMessageErrorOfRequiredFieldsForBondedWarehousing(true, true, true));
				AssertEquals("EntryNumber", "NCTS00001", supporter.EntryNumber);
				AssertEquals("ClientPK", orgHeader.PK, supporter.ClientPK);
				AssertEquals("WarehouseAddress", orgAddress, supporter.WarehouseAddress);
				AssertEquals("WarehouseTransactionStatus", "ICT", supporter.WarehouseTransactionStatus);
				AssertEquals("IsActive", false, supporter.IsActive);
				AssertEquals("SupportsBondedWarehousing", false, supporter.SupportsBondedWarehousing);
				AssertEquals("SupportModificationState", false, supporter.SupportModificationState);
				AssertEquals("IsBondedWarehousingDisabled", false, supporter.IsBondedWarehousingDisabled);
				AssertEquals("IsOutwardBondedWarehousingEnabled", false, supporter.IsOutwardBondedWarehousingEnabled);
				AssertEquals("IsInwardBondedWarehousingEnabled", false, supporter.IsInwardBondedWarehousingEnabled);
				AssertEquals("IsChangeOfOwnershipBondedWarehousingEnabled", false, supporter.IsChangeOfOwnershipBondedWarehousingEnabled);
				AssertEquals("IsChangeOfRegimeWarehousingEnabled", false, supporter.IsChangeOfRegimeWarehousingEnabled);
				AssertEquals("HasManualWhsUpdate", false, supporter.HasManualWhsUpdate);
				AssertEquals("IsIntoTemporaryImportEnabled", false, supporter.IsIntoTemporaryImportEnabled);
				AssertEquals("IsOutOfTemporaryImportEnabled", false, supporter.IsOutOfTemporaryImportEnabled);
				AssertEquals("IsIntoTemporaryExportEnabled", false, supporter.IsIntoTemporaryExportEnabled);
				AssertEquals("IsOutOfTemporaryExportEnabled", false, supporter.IsOutOfTemporaryExportEnabled);
				AssertEquals("IsIntoInwardProcessingEnabled", false, supporter.IsIntoInwardProcessingEnabled);
				AssertEquals("IsOutOfInwardProcessingEnabled", false, supporter.IsOutOfInwardProcessingEnabled);
				AssertEquals("IsIntoOutwardProcessingEnabled", false, supporter.IsIntoOutwardProcessingEnabled);
				AssertEquals("IsOutOfOutwardProcessingEnabled", false, supporter.IsOutOfOutwardProcessingEnabled);
			});
		}

		public void TestISendMessageToCustoms()
		{
			var phase5DepartureHeader = Factory.NewWithValidTestData<NctsHeader>();
			phase5DepartureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			phase5DepartureHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			AssertExceptionThrown<ApplicationException>("When not set",
				"You can't perform this action that results in a message being sent because you have not hooked up a ISendsMessagesToCustoms to the NctsHeader",
				() =>
				{
					_ = phase5DepartureHeader.MessageInitiator;
				});

			phase5DepartureHeader.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			AssertNoExceptionThrown("When set", () =>
			{
				_ = phase5DepartureHeader.MessageInitiator;
			});
		}

		public void TestHeaderContainersLineNumberGenerator()
		{
			AssertType<ShortSequenceNumberGenerator>("Sequence generator for HeaderContainers", header.HeaderContainersLineNumberGenerator);
		}

		public void CreateCountervailingDutyAmount(NctsDepartureCargoDesc goodsItem)
		{
			goodsItem.BY_HarmonisedTariff = "DUMMYTRF1";
			goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;

			var supplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
			supplementaryCode1.CY_Code = "AC02";
		}

		public void CreateAntiDumpingDutyAmount(NctsDepartureCargoDesc goodsItem)
		{
			goodsItem.BY_HarmonisedTariff = "DUMMYTRF1";
			goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;

			var supplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
			supplementaryCode1.CY_Code = "AC02";
		}

		public void TestNotAllSealStateAreDEC()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			CombineAssertions(() =>
			{
				AssertEquals(true, header.AllArrivalSealStateAreDEC);

				var container1 = header.ArrivalHeaderContainers.AddNew();
				AssertEquals(true, header.AllArrivalSealStateAreDEC);

				container1.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				var seal1 = container1.Seals.AddNew();
				seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
				AssertEquals(false, header.AllArrivalSealStateAreDEC);

				seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals(true, header.AllArrivalSealStateAreDEC);
			});
		}

		public void TestSetUpUnloadingStateOfTargetArrivalSeals()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);

			var container1 = header.ArrivalHeaderContainers.AddNew();
			container1.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			var seal1 = container1.Seals.AddNew();
			var seal2 = container1.Seals.AddNew();

			CombineAssertions(() =>
			{
				header.SetUpUnloadingStateOfTargetArrivalSeals();
				AssertEquals("When unloadedState is NEW, seal will not be changed", "NEW", seal1.BK_UnloadingState);
				AssertEquals("When unloadedState is NEW, seal will not be changed", "NEW", seal2.BK_UnloadingState);

				seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
				header.SetUpUnloadingStateOfTargetArrivalSeals();
				AssertEquals("unloadedState has been changed from DIF to DEC", "DEC", seal1.BK_UnloadingState);
				AssertEquals("When unloadedState is NEW, seal will not be changed", "NEW", seal2.BK_UnloadingState);
			});
		}

		public void TestExistNonDECEntry()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill = header.Bills.AddNew();
			var movementDetail = bill.MovementDetail;
			CombineAssertions(() =>
			{
				movementDetail.B9_UnloadedState = "DIF";
				AssertEquals(true, header.ExistNonDECEntry);

				movementDetail.B9_UnloadedState = "DEC";
				var supportingDocument1 = bill.SupportingDocuments.AddNew();
				supportingDocument1.CSI_Status = "DEC";
				AssertEquals(false, header.ExistNonDECEntry);

				var additionalDocument1 = bill.AdditionalDocuments.AddNew();
				additionalDocument1.CSI_Status = "DIF";
				AssertEquals(true, header.ExistNonDECEntry);

				additionalDocument1.CSI_Status = "DEC";
				AssertEquals(false, header.ExistNonDECEntry);

				var goodsItem = bill.ArrivalGoodsItems.AddNew();
				goodsItem.BY_UnloadedState = "DEC";

				var package = goodsItem.Packages.AddNew();
				package.B5_TypeOfDifference = "DIF";
				AssertEquals(true, header.ExistNonDECEntry);
			});
		}

		public void TestSetAllUnloadedStateToDEC_whenDIF()
		{
			AssertSetAllUnloadedStateToDEC("DIF");
		}

		public void TestSetAllUnloadedStateToDEC_whenMIS()
		{
			AssertSetAllUnloadedStateToDEC("MIS");
		}

		public void TestSetAllUnloadedStateToDEC_whenDEC()
		{
			AssertSetAllUnloadedStateToDEC("DEC");
		}

		void AssertSetAllUnloadedStateToDEC(string unloadedState)
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill = header.Bills.AddNew();
			var movementDetail = bill.MovementDetail;
			movementDetail.B9_UnloadedState = unloadedState;
			var supportingDocument = bill.SupportingDocuments.AddNew();
			supportingDocument.CSI_Status = "MIS";
			var additionalDocument = bill.AdditionalDocuments.AddNew();

			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = "DIF";
			var supportingDoc_GoodsItem = goodsItem.SupportingDocuments.AddNew();
			supportingDoc_GoodsItem.CSI_Status = "DIF";

			var bill2 = header.Bills.AddNew();

			header.SetAllUnloadedStateToDEC();
			CombineAssertions(() =>
			{
				AssertEquals("UnloadedState of bill has been changed to or is DEC", "DEC", movementDetail.B9_UnloadedState);
				AssertEquals("UnloadedState of supportingDocument has been changed to DEC", "DEC", supportingDocument.CSI_Status);
				AssertEquals("When UnloadedState is NEW, additionalDocument will be delete", 0, bill.AdditionalDocuments.Count);
				AssertEquals("UnloadedState of goodsItem has been changed to DEC", "DEC", goodsItem.BY_UnloadedState);
				AssertEquals("UnloadedState of supportingDocument under goodsItem has been changed to DEC", "DEC", supportingDoc_GoodsItem.CSI_Status);
				AssertEquals("When UnloadedState is NEW, bill will be delete", 1, header.Bills.Count);
			});
		}

		NctsGuarantee CreateNctsGuarantee(NctsHeader nctsHeader)
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TRA, "1");

			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var guarantee = nctsHeader.MovementHeader.Guarantees[0];
			guarantee.PW_Override = false;

			return guarantee;
		}

		CusGuaranteeHeader CreateCusGuarantee(ZString number, OrgHeader permitHolder, ZString type, ZString subType)
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Number = number;
			guarantee.CPH_OH_PermitHolder = permitHolder.PK;
			guarantee.CPH_Type = type;
			guarantee.CPH_SubType = subType;
			guarantee.CPH_StartDate = ZDate.BrettsBirthday;
			return guarantee;
		}

		void SetupDataForGetAdditionalCodesForRateType(string rateTypeCode, string rateTypeCode2)
		{
			(var helper, var testRate1, var testRate2, var testRate11, var testRate12, var tradeGroup) = SetupRatesForGetAdditionalCodes(rateTypeCode, rateTypeCode2);

			helper.CreateCusApplicability(testRate1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC01");
			helper.CreateCusApplicability(testRate2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC02");

			helper.CreateCusApplicability(testRate11, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC01");
			helper.CreateCusApplicability(testRate12, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC02");

			SetupCusCodeForGetAdditionalCodes(helper);
		}

		(Universal.Testing.UniversalReferenceTestDataHelper, RateView, RateView, RateView, RateView, CusRefTradeGroupView) SetupRatesForGetAdditionalCodes(string rateTypeCode1, string rateTypeCode2)
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			helper.CreateRefCusTaxOrFeeType("VAT");
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", euGrouping);
			var s1p1TariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Latvia, Customs.Universal.Constants.TariffTypes.Import, nomenclatureGroupType: "LV", ensureDataGroupingExists: false);
			Factory.Save();

			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Latvia, s1p1TariffType.PK, "DUMMYTRF1", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "EU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, ensureDataGroupingExists: false);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Latvia);

			var rateType1 = helper.CreateCusRateType(Core.Constants.CountryCodes.Latvia, rateTypeCode1, ensureDataGroupingExists: false);
			var rateCode1 = helper.CreateCusRateCode(Factory, "RC1", rateType1.PK);
			var testRate1 = helper.CreateRate(cusTariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "24.3 * [FLAT]");
			var testRate2 = helper.CreateRate(cusTariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "4.3 * [FLAT]");

			var rateType2 = helper.CreateCusRateType(Core.Constants.CountryCodes.Latvia, rateTypeCode2, ensureDataGroupingExists: false);
			var rateCode2 = helper.CreateCusRateCode(Factory, "RC2", rateType2.PK);
			var testRate11 = helper.CreateRate(cusTariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "24.3 * [FLAT]");
			var testRate12 = helper.CreateRate(cusTariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "4.3 * [FLAT]");

			return (helper, testRate1, testRate2, testRate11, testRate12, tradeGroup);
		}

		void SetupCusCodeForGetAdditionalCodes(Universal.Testing.UniversalReferenceTestDataHelper helper)
		{
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "Additional Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, "Default Rate", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var lastMonth = ZDateTime.Today.AddMonths(-1);
			var nextMonth = ZDateTime.Today.AddMonths(1);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "AC01", "EU AC01", lastMonth, nextMonth)
				.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "AC03", "EU AC03", lastMonth, nextMonth)
				.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);

			Factory.Save();
		}

		void AssertPrincipal_ShouldAddAuthorization(string authorizationCode)
		{
			// Arrange
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			var cusAuthorizationHeader = CreateCusAuthorizationHeader(authorizationCode, organization);
			var movementHeader = header.MovementHeader;
			movementHeader.IsSimplifiedNctsProcedure = true;

			AssertCollectionNotContains("Precondition", movementHeader.CusAuthorizationUsages, cau => cau.AGC_Code == authorizationCode);

			// Act
			header.Principal.OrganisationPK = organization.PK;

			// Assert
			AssertCollectionContains(movementHeader.CusAuthorizationUsages,
				cau => cau.AGC_Code == cusAuthorizationHeader.CPH_Type &&
													cau.AGC_Number == cusAuthorizationHeader.CPH_Number &&
													cau.AGC_OH_Owner == cusAuthorizationHeader.CPH_OH_PermitHolder);
		}

		void AssertPrincipal_ShouldAddAuthorization_OrgProxyFallback(string authorizationCode)
		{
			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfigurationUseOrgProxyFallback(Factory, useOrgProxyFallBack: true))
			{
				// Arrange
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.SetMovementType(NctsMovementType.Codes.Departure);
				var companyOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
				var cusAuthorizationHeader = CreateCusAuthorizationHeader(authorizationCode, companyOrgProxy);

				GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyOrgProxy.PK;

				var organization = Factory.NewWithValidTestData<OrgHeader>();
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = organization.PK;

				Factory.Save();

				var movementHeader = header.MovementHeader;
				movementHeader.IsSimplifiedNctsProcedure = true;

				AssertCollectionNotContains("Precondition", movementHeader.CusAuthorizationUsages, cau => cau.AGC_Code == authorizationCode);
				// Act
				header.Principal.OrganisationPK = organization.PK;

				// Assert
				AssertCollectionContains(movementHeader.CusAuthorizationUsages,
					cau => cau.AGC_Code == cusAuthorizationHeader.CPH_Type &&
														cau.AGC_Number == cusAuthorizationHeader.CPH_Number &&
														cau.AGC_OH_Owner == cusAuthorizationHeader.CPH_OH_PermitHolder);
			}
		}

		void AssertPrincipal_ShouldDeleteAuthorization_WhenPrincipalDeleted(string authorizationCode)
		{
			// Arrange
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			CreateCusAuthorizationHeader(authorizationCode, organization);
			var movementHeader = header.MovementHeader;
			movementHeader.IsSimplifiedNctsProcedure = true;
			header.Principal.OrganisationPK = organization.PK;

			AssertCollectionContains("Precondition", movementHeader.CusAuthorizationUsages, cau => cau.AGC_Code == authorizationCode);

			// Act
			header.Principal.OrganisationPK = ZGuid.Empty;

			// Assert
			AssertCollectionNotContains(movementHeader.CusAuthorizationUsages, cau => cau.AGC_Code == authorizationCode);
		}

		void AssertPrincipal_ShouldDeleteAuthorization_WhenPrincipalChangedAndHasNoAuthorization(string authorizationCode)
		{
			// Arrange
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();

			CreateCusAuthorizationHeader(authorizationCode, organization1);
			var movementHeader = header.MovementHeader;
			movementHeader.IsSimplifiedNctsProcedure = true;
			header.Principal.OrganisationPK = organization1.PK;

			AssertCollectionContains("Precondition", movementHeader.CusAuthorizationUsages, cau => cau.AGC_Code == authorizationCode);

			// Act
			header.Principal.OrganisationPK = organization2.PK;

			// Assert
			AssertCollectionNotContains(movementHeader.CusAuthorizationUsages, cau => cau.AGC_Code == authorizationCode);
		}

		void AssertPrincipal_ShouldUpdateAuthorization(string authorizationCode)
		{
			// Arrange
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();

			var authorizationHeader1 = CreateCusAuthorizationHeader(authorizationCode, organization1);
			var authorizationHeader2 = CreateCusAuthorizationHeader(authorizationCode, organization2);
			var movementHeader = header.MovementHeader;
			movementHeader.IsSimplifiedNctsProcedure = true;
			header.Principal.OrganisationPK = organization1.PK;

			AssertCollectionContains("Precondition", movementHeader.CusAuthorizationUsages,
				cau => cau.AGC_Code == authorizationHeader1.CPH_Type &&
													cau.AGC_Number == authorizationHeader1.CPH_Number &&
													cau.AGC_OH_Owner == authorizationHeader1.CPH_OH_PermitHolder);

			// Act
			header.Principal.OrganisationPK = organization2.PK;

			// Assert
			AssertCollectionNotContains(movementHeader.CusAuthorizationUsages,
				cau => cau.AGC_Code == authorizationHeader1.CPH_Type &&
													cau.AGC_Number == authorizationHeader1.CPH_Number &&
													cau.AGC_OH_Owner == authorizationHeader1.CPH_OH_PermitHolder);
			AssertCollectionContains(movementHeader.CusAuthorizationUsages,
				cau => cau.AGC_Code == authorizationHeader2.CPH_Type &&
													cau.AGC_Number == authorizationHeader2.CPH_Number &&
													cau.AGC_OH_Owner == authorizationHeader2.CPH_OH_PermitHolder);
		}

		CusAuthorisationHeader CreateCusAuthorizationHeader(string type, OrgHeader organization)
		{
			var cusAuthorisationHeader = Factory.New<CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_Type = type;
			cusAuthorisationHeader.CPH_OH_PermitHolder = organization.PK;
			cusAuthorisationHeader.CPH_Number = "123";
			cusAuthorisationHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			cusAuthorisationHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			return cusAuthorisationHeader;
		}

		public void TestLogLogicalStatus()
		{
			header.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();
			var mscLogs = header.Logs.GetAllLogs().Find(x => x.Event.SE_Code == AutoEvents.MessageStatusChangeCode);
			AssertEquals("Status not set. No event logs", 0, mscLogs.Count());

			header.EffectiveMessageStatus = LogicalStatusList.Codes.Invalid;
			AssertEquals("Status not saved. No event logs should be logged yet", 0, mscLogs.Count());
			Factory.Save();
			AssertEquals("Status saved. Event log should be created", 1, mscLogs.Count());
			var log1 = mscLogs.First();
			AssertEquals(LogicalStatusList.Codes.Invalid, log1.SL_Reference);

			Factory.Save();
			AssertEquals("Status not changed, no further log created", 1, mscLogs.Count());

			header.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			Factory.Save();
			AssertEquals("Status changed, another log created", 2, mscLogs.Count());
			var log2 = mscLogs.Last();
			AssertEquals(LogicalStatusList.Codes.Accepted, log2.SL_Reference);
		}

		public void TestIsConditionR0520_UserShouldNotSaveAmendmentsToGuarantees()
		{
			AssertEquals("IsConditionR0520_UserShouldNotSaveAmendmentsToGuarantees is false by default in EU.", false, header.IsConditionR0520_UserShouldNotSaveAmendmentsToGuarantees);
		}

		public void TestApplicationCode_Caption()
		{
			NCTSTestHelper.AssertCaptions(header.BH_ApplicationCodeInfo, "Application Code", "", "App Code");
		}

		public void TestUpdateDestinationCustomsOfficeWhenDestinationTraderChanged_Phase4()
		{
			const string regNo1 = "12345";
			const string regNo2 = "98765";

			var org1 = Factory.New<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, regNo1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var org2 = Factory.New<OrgHeader>();
			var cusCode2 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsOfficeForTransit, regNo2, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusCode2.OK_OA_PremisesAddress = org2.MainAddress.PK;

			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.DestinationTrader.E2_OA_Address = org2.MainAddress.PK;
			AssertEquals("phase4Arrival, DestinationCustomsOfficeCodeForArrival won't be updated", ZString.Empty, header.DestinationCustomsOfficeCodeForArrival);
		}

		public void TestUpdateDestinationCustomsOfficeWhenDestinationTraderChanged_Phase5()
		{
			const string regNo1 = "12345";
			const string regNo2 = "98765";
			const string regNo3 = "87654";

			var org1 = Factory.New<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, regNo1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var org2 = Factory.New<OrgHeader>();
			var cusCode2 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsOfficeForTransit, regNo2, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusCode2.OK_OA_PremisesAddress = org2.MainAddress.PK;

			var org3 = Factory.New<OrgHeader>();
			var cusCode3 = org3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsOfficeForTransit, regNo3, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			CombineAssertions(() =>
			{
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				header.DestinationTrader.E2_OA_Address = org1.MainAddress.PK;
				AssertEquals("DestinationTrader doesn't contain CTR orgCusCode", ZString.Empty, header.CommonMovementHeader.DestinationCustomsOfficeCodeForArrival);

				header.DestinationTrader.E2_OA_Address = org2.MainAddress.PK;
				AssertEquals("DestinationTrader contains CTR orgCusCode", regNo2, header.CommonMovementHeader.DestinationCustomsOfficeCodeForArrival);

				header.DestinationTrader.E2_OA_Address = org3.MainAddress.PK;
				AssertEquals("DestinationTrader contains CTR orgCusCode and premises address is empty", regNo3, header.CommonMovementHeader.DestinationCustomsOfficeCodeForArrival);
			});
		}

		public void TestUpdateAuthorizationDataWhenDestinationTraderChanged()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			CombineAssertions(() =>
			{
				header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				header.DestinationTrader.E2_OA_Address = org2.MainAddress.PK;
				AssertEquals("phase4Arrival, ArrivalMovementHeader.AuthorizationOwner won't be updated", ZGuid.Empty, header.ArrivalMovementHeader.AuthorizationOwner);

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.DestinationTrader.E2_OA_Address = org1.MainAddress.PK;
				AssertEquals("When phase5Arrival and flag true (EU), ArrivalMovementHeader.AuthorizationOwner is NOT set with DestinationTrader's org (org1) when ArrivalMovementHeader.AuthorizationNumber and ArrivalMovementHeader.AuthorizationCode are empty", ZGuid.Empty, header.ArrivalMovementHeader.AuthorizationOwner);

				header.ArrivalMovementHeader.AuthorizationCode = "ACE";
				header.DestinationTrader.E2_OA_Address = org2.MainAddress.PK;
				AssertEquals("When phase5Arrival and flag true (EU), ArrivalMovementHeader.AuthorizationOwner is set with DestinationTrader's org (org2)", org2.PK, header.ArrivalMovementHeader.AuthorizationOwner);

				header.DestinationTrader.E2_OA_Address = ZGuid.Empty;
				AssertEquals("When phase5Arrival and flag true (EU), ArrivalMovementHeader.AuthorizationOwner is set with DestinationTrader's org (empty)", ZGuid.Empty, header.ArrivalMovementHeader.AuthorizationOwner);
			});
		}

		public void TestNCTSPreviousDocumentsCount() => CombineAssertions(() =>
		{
			AssertEquals("No documents", 0, header.NCTSPreviousDocumentsCount);

			header.PreviousDocuments.AddNew().CSI_Code = "N1";
			AssertEquals("1 NCTS documents", 1, header.NCTSPreviousDocumentsCount);

			header.PreviousDocuments.AddNew().CSI_Code = "X1";
			AssertEquals("1 NCTS documents and 1 other document", 1, header.NCTSPreviousDocumentsCount);

			header.PreviousDocuments.AddNew().CSI_Code = "N2";
			AssertEquals("2 NCTS documents", 2, header.NCTSPreviousDocumentsCount);
		});

		public void TestMaxNCTSPreviousDocumentsCount()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_HeaderType = NctsMovementType.Codes.Departure;

			header.PreviousDocuments.AddNew().CSI_Code = "N1";
			header.PreviousDocuments.AddNew().CSI_Code = "X1";
			var bill1 = header.Bills.AddNew();
			bill1.PreviousDocuments.AddNew().CSI_Code = "N0";
			var goodsItem11 = bill1.GoodsItems.AddNew();
			goodsItem11.PreviousDocuments.AddNew().CSI_Code = "N0";
			var bill2 = header.Bills.AddNew();
			bill2.PreviousDocuments.AddNew().CSI_Code = "N2";
			bill2.PreviousDocuments.AddNew().CSI_Code = "X2";
			var goodsItem21 = bill2.GoodsItems.AddNew();
			goodsItem21.PreviousDocuments.AddNew().CSI_Code = "N0";
			var goodsItem22 = bill2.GoodsItems.AddNew();
			goodsItem22.PreviousDocuments.AddNew().CSI_Code = "N3";
			goodsItem22.PreviousDocuments.AddNew().CSI_Code = "N4";
			goodsItem22.PreviousDocuments.AddNew().CSI_Code = "X3";
			AssertEquals(4, header.MaxNCTSPreviousDocumentsCount);
		}

		public void TestGetGoodsItem_WhenIsDeparture()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var houseGoodsItem1 = bill1.GoodsItems.AddNew();
			var houseGoodsItem2 = bill2.GoodsItems.AddNew();

			AssertArrayEqualsByElements("When Is Phase5 Departure Movement, GetGoodsItems()", new[] { houseGoodsItem1, houseGoodsItem2 }, header.GetGoodsItems().ToArray());

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertArrayEqualsByElements("When Is Phase4 Departure Movement, GetGoodsItems()", Array.Empty<NctsCommonCargoDesc>(), header.GetGoodsItems().ToArray());

			var movementHeader = header.MovementHeader;
			var godsItem1 = movementHeader.GoodsItems.AddNew();
			var goodsItem2 = movementHeader.GoodsItems.AddNew();
			AssertArrayEqualsByElements("When Is Phase4 Departure Movement, GetGoodsItems()", new[] { godsItem1, goodsItem2 }, header.GetGoodsItems().ToArray());

			header.BH_HeaderType = "X";
			AssertArrayEqualsByElements("When the movement is neither Arrival nor Departure, GetGoodsItems()", Array.Empty<NctsCommonCargoDesc>(), header.GetGoodsItems().ToArray());
		}

		public void TestGetGoodsItem_WhenIsArrival()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			AssertArrayEqualsByElements("When Is Phase5 Arrival Movement, GetGoodsItems()", Array.Empty<NctsCommonCargoDesc>(), header.GetGoodsItems().ToArray());

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var houseGoodsItem1 = bill1.ArrivalGoodsItems.AddNew();
			var houseGoodsItem2 = bill2.ArrivalGoodsItems.AddNew();
			AssertArrayEqualsByElements("When Is Phase5 Arrival Movement, GetGoodsItems()", new[] { houseGoodsItem1, houseGoodsItem2 }, header.GetGoodsItems().ToArray());

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertArrayEqualsByElements("When Is Phase4 Arrival Movement, GetGoodsItems()", Array.Empty<NctsCommonCargoDesc>(), header.GetGoodsItems().ToArray());

			var arrivalGoodsItem1 = header.ArrivalMovementHeader.GoodsItems.AddNew();
			var arrivalGoodsItem2 = header.ArrivalMovementHeader.GoodsItems.AddNew();
			AssertArrayEqualsByElements("When Is Phase4 Arrival Movement, GetGoodsItems()", new[] { arrivalGoodsItem1, arrivalGoodsItem2 }, header.GetGoodsItems().ToArray());
		}

		public void TestIsAnyArrivalContainerSealDiscrepancy() => CombineAssertions(() =>
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);

			AssertEquals("No container", false, header.IsAnyArrivalContainerSealDiscrepancy);

			var container1 = header.ArrivalHeaderContainers.AddNew();
			AssertEquals("No seal", false, header.IsAnyArrivalContainerSealDiscrepancy);

			var seal1 = container1.Seals.AddNew();
			seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("UnloadedState=NEW", true, header.IsAnyArrivalContainerSealDiscrepancy);
			seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
			AssertEquals("UnloadedState=MIS", true, header.IsAnyArrivalContainerSealDiscrepancy);
			seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals("UnloadedState=DIF", true, header.IsAnyArrivalContainerSealDiscrepancy);
			seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
			AssertEquals("UnloadedState=DEC", false, header.IsAnyArrivalContainerSealDiscrepancy);

			var seal2 = container1.Seals.AddNew();
			seal2.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("2nd seal discrepancy", true, header.IsAnyArrivalContainerSealDiscrepancy);
			seal2.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;

			var container2 = header.ArrivalHeaderContainers.AddNew();
			var seal3 = container2.Seals.AddNew();
			seal3.BK_UnloadingState = ZString.Empty;
			AssertEquals("2nd container no discrepancy", false, header.IsAnyArrivalContainerSealDiscrepancy);
			seal3.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("2nd container discrepancy", true, header.IsAnyArrivalContainerSealDiscrepancy);
		});

		public void TestIsAnyArrivalGoodsItemDiscrepancy() => CombineAssertions(() =>
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);

			AssertEquals("No bill", false, header.IsAnyArrivalGoodsItemDiscrepancy);

			var bill1 = header.Bills.AddNew();
			AssertEquals("No goods item", false, header.IsAnyArrivalGoodsItemDiscrepancy);

			var goodsitem1 = bill1.ArrivalGoodsItems.AddNew();
			goodsitem1.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("UnloadedState=NEW", true, header.IsAnyArrivalGoodsItemDiscrepancy);
			goodsitem1.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			AssertEquals("UnloadedState=MIS", true, header.IsAnyArrivalGoodsItemDiscrepancy);
			goodsitem1.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals("UnloadedState=DIF", true, header.IsAnyArrivalGoodsItemDiscrepancy);
			goodsitem1.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			AssertEquals("UnloadedState=DEC", false, header.IsAnyArrivalGoodsItemDiscrepancy);

			var goodsitem2 = bill1.ArrivalGoodsItems.AddNew();
			goodsitem2.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("2nd goodsitem discrepancy", true, header.IsAnyArrivalGoodsItemDiscrepancy);
			goodsitem2.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;

			var bill2 = header.Bills.AddNew();
			var goodsitem3 = bill2.ArrivalGoodsItems.AddNew();
			goodsitem3.BY_UnloadedState = ZString.Empty;
			AssertEquals("2nd bill no discrepancy", false, header.IsAnyArrivalGoodsItemDiscrepancy);
			goodsitem3.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("2nd bill discrepancy", true, header.IsAnyArrivalGoodsItemDiscrepancy);
		});

		public void TestIsAnyArrivalHouseConsignmentDiscrepancy() => CombineAssertions(() =>
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);

			AssertEquals("No bill", false, header.IsAnyArrivalHouseConsignmentDiscrepancy);

			var bill1 = header.Bills.AddNew();
			bill1.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("UnloadedState=NEW", true, header.IsAnyArrivalHouseConsignmentDiscrepancy);
			bill1.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			AssertEquals("UnloadedState=MIS", true, header.IsAnyArrivalHouseConsignmentDiscrepancy);
			bill1.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals("UnloadedState=DIF", true, header.IsAnyArrivalHouseConsignmentDiscrepancy);
			bill1.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			AssertEquals("UnloadedState=DEC", false, header.IsAnyArrivalHouseConsignmentDiscrepancy);

			var bill2 = header.Bills.AddNew();
			bill2.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("2nd MovementDetail discrepancy", true, header.IsAnyArrivalHouseConsignmentDiscrepancy);
			bill2.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			AssertEquals("2nd MovementDetail no discrepancy", false, header.IsAnyArrivalHouseConsignmentDiscrepancy);
		});

		public void TestEffectiveMessageStatus_Phase4()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

			AssertNullOrEmpty("Precondition", header.BH_MessageStatus);

			header.EffectiveMessageStatus = "SNT";
			AssertEquals("SNT", header.BH_MessageStatus);

			AssertEquals("Precondition", "SNT", header.EffectiveMessageStatus);
			header.BH_MessageStatus = "ERR";
			AssertEquals("ERR", header.EffectiveMessageStatus);
		}

		public void TestEffectiveMessageStatus_Phase5()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			AssertNullOrEmpty("Precondition", header.CommonMovementHeader.BM_MessageStatus);

			header.EffectiveMessageStatus = "SNT";
			AssertEquals("SNT", header.CommonMovementHeader.BM_MessageStatus);

			AssertEquals("Precondition", "SNT", header.EffectiveMessageStatus);
			header.CommonMovementHeader.BM_MessageStatus = "ERR";
			AssertEquals("ERR", header.EffectiveMessageStatus);
		}

		public void TestGetNewMovementHeader() => CombineAssertions(() =>
		{
			var nctsHeader = Factory.New<NctsHeaderPhase5ForTest>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			AssertNull("Should not create departure header for arrival", nctsHeader.GetNewMovementHeaderExposed());
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			AssertNotNull("Should create departure header for departure", nctsHeader.GetNewMovementHeaderExposed());
		});

		public void TestHas30600AdditionalInformation()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				AssertEquals("When additionalDocument is null.", expected: false, nctsHeader.AdditionalDocuments.Has30600AdditionalInformation());

				var additionalDocument = nctsHeader.AdditionalDocuments.AddNew();
				additionalDocument.CSI_Code = "30601";
				additionalDocument.CSI_SubType = "INF";
				AssertEquals("When additionalDocument is not null and Doc Kind = INF and Doc.Type != 30600.", expected: false, nctsHeader.AdditionalDocuments.Has30600AdditionalInformation());

				additionalDocument.CSI_Code = "30600";
				AssertEquals("When additionalDocument is not null and Doc Kind = INF and Doc.Type = 30600.", expected: true, nctsHeader.AdditionalDocuments.Has30600AdditionalInformation());
			});
		}

		public void TestJobDocAddressRequirementDefaultContactType_Consignee()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			AssertEquals(ContactType.NoContactType, nctsHeader.ConsigneeJobDocAddressRequirement.DefaultContactType);
		}

		public void TestJobDocAddressRequirementDefaultContactType_Consignor()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			AssertEquals(ContactType.NoContactType, nctsHeader.ConsignorJobDocAddressRequirement.DefaultContactType);
		}

		public void TestJobDocAddressRequirementDefaultContactType_Principal()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			AssertEquals(ContactType.NoContactType, nctsHeader.PrincipalJobDocAddressRequirement.DefaultContactType);
		}

		public void TestCancelWarehouseIfNeeded_ShouldSendCancelEvent_WhenWarehouseTransactionStatusIsOutward()
		{
			foreach (var transactionStatus in new[] { WarehouseTransactionStatusList.Codes.OutwardCreated, WarehouseTransactionStatusList.Codes.OutwardCreatedPending, WarehouseTransactionStatusList.Codes.OutwardUpdated, WarehouseTransactionStatusList.Codes.OutwardUpdatedPending })
			{
				var bwhHeader = Factory.New<NctsHeader>();
				bwhHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
				((IWarehouseIntegrationSupporter)bwhHeader).WarehouseTransactionStatus = transactionStatus;

				bwhHeader.CancelWarehouseIfNeeded();
				Factory.Save();

				AssertWarehouseCancelEvent(bwhHeader);
			}
		}

		public void TestCancelWarehouseIfNeeded_ShouldNotSendCancelEvent_WhenWarehouseTransactionStatusIsNotOutward()
		{
			foreach (var transactionStatus in new[] { string.Empty, WarehouseTransactionStatusList.Codes.OutwardHolding })
			{
				((IWarehouseIntegrationSupporter)header).WarehouseTransactionStatus = transactionStatus;

				header.CancelWarehouseIfNeeded();
				Factory.Save();

				AssertWarehouseCancelEvent(header, false);
			}
		}

		public void TestCancelWarehouseIfNeeded_ShouldExecuteErrorAction_WhenEventSendingErrorOccurs()
		{
			((IWarehouseIntegrationSupporter)header).WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardUpdatedPending;

			PublishToUniversalResult sendingErrorResult = null;

			header.CancelWarehouseIfNeeded(r => sendingErrorResult = r);
			Factory.Save();

			AssertNotNull(sendingErrorResult);
		}

		public void TestIEuOfficeCodeProviderCustomsOffices()
		{
			var euOfficeCodeProvider = (IEuOfficeCodeProvider)header;

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var customsOffices = euOfficeCodeProvider.CustomsOffices;
			AssertNotNull(customsOffices);
			AssertEquals("Access to IEuOfficeCodeProvider.CustomsOffices for Phase5", ErrorReporter.LastKeyReported);

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			ErrorReporter.Clear();
			AssertSame(header.CustomsOffices, euOfficeCodeProvider.CustomsOffices);
			AssertNullOrEmpty(ErrorReporter.LastKeyReported);
		}

		public void TestIsInPhase5TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, true))
			{
				Assert(header.IsInPhase5TransitionPeriod);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, false))
			{
				Assert(!header.IsInPhase5TransitionPeriod);
			}
		}

		public void TestCusGoodsLocation()
		{
			CombineAssertions(() =>
			{
				AssertSame(header.MovementHeader.GoodsLocation, header.CusGoodsLocation);
				header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				AssertSame(header.ArrivalMovementHeader.GoodsLocation, header.CusGoodsLocation);
			});
		}

		public void TestJobServiceBranch()
		{
			var header = Factory.New<NctsHeader>();
			var iHaveServices = (IHaveServices)header;
			AssertEquals("Service branch is defaulted", Env.CurrentBranch.PK, iHaveServices.ServiceBranch.PK);

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CFR";
			company.GC_Name = "FR Company";
			var branch = company.Branches.AddNew();
			header.BH_GB = branch.PK;
			AssertEquals("Service branch", branch.PK, iHaveServices.ServiceBranch.PK);
		}

		public void TestResetGoodsItemNumbersWhenPhase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var bill1 = nctsHeader.Bills.AddNew();
			var goodItem11 = bill1.GoodsItems.AddNew();
			goodItem11.BY_DeclarationGoodsItemNumber = 1;
			var goodItem12 = bill1.GoodsItems.AddNew();
			goodItem12.BY_DeclarationGoodsItemNumber = 2;

			var bill2 = nctsHeader.Bills.AddNew();
			var goodItem21 = bill2.GoodsItems.AddNew();
			goodItem21.BY_DeclarationGoodsItemNumber = 3;
			var goodItem22 = bill2.GoodsItems.AddNew();
			goodItem22.BY_DeclarationGoodsItemNumber = 4;

			var bill3 = nctsHeader.Bills.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("goodItem11.BY_DeclarationGoodsItemNumber is not 0 before calling method", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 before calling method", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 before calling method", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 before calling method", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				nctsHeader.ResetGoodsItemNumbersWhenPhase5();
				AssertEquals("goodItem11.BY_DeclarationGoodsItemNumber is not 0 after calling method when phase4", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 after calling method when phase4", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 after calling method when phase4", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 after calling method when phase4", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.ShouldResetGoodsItemNumbers = false;
				nctsHeader.ResetGoodsItemNumbersWhenPhase5();
				AssertEquals("goodItem11.BY_DeclarationGoodsItemNumber is not 0 after calling method when phase5 but UpdatePreDeclaration is true", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 after calling method when phase5 but UpdatePreDeclaration is true", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 after calling method when phase5 but UpdatePreDeclaration is true", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 after calling method when phase5 but UpdatePreDeclaration is true", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				nctsHeader.ShouldResetGoodsItemNumbers = true;
				nctsHeader.ResetGoodsItemNumbersWhenPhase5();
				AssertEquals("goodItem11.BY_DeclarationGoodsItemNumber is 0 after calling method when phase5 and UpdatePreDeclaration is dalse", 0, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is 0 after calling method when phase5 and UpdatePreDeclaration is dalse", 0, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is 0 after calling method when phase5 and UpdatePreDeclaration is dalse", 0, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is 0 after calling method when phase5 and UpdatePreDeclaration is dalse", 0, goodItem22.BY_DeclarationGoodsItemNumber);
			});
		}

		public void TestCreateNctsMessageProcessor()
		{
			CombineAssertions(() =>
			{
				AssertCreateNctsMessageProcessor(Core.Constants.CountryCodes.Germany, true, true);
				AssertCreateNctsMessageProcessor(Core.Constants.CountryCodes.France, true, true);
				AssertCreateNctsMessageProcessor(Core.Constants.CountryCodes.UnitedKingdom, true, true);
				AssertCreateNctsMessageProcessor(Core.Constants.CountryCodes.Ireland, true, true);
				AssertCreateNctsMessageProcessor(Core.Constants.CountryCodes.Netherlands, true, true);
				AssertCreateNctsMessageProcessor(Core.Constants.CountryCodes.Norway, true, false);

				AssertCreateNctsMessageProcessor(Core.Constants.CountryCodes.Poland, false, false);
				AssertCreateNctsMessageProcessor(Core.Constants.CountryCodes.Belgium, false, false);
				AssertCreateNctsMessageProcessor(Core.Constants.CountryCodes.Switzerland, false, false);
				AssertCreateNctsMessageProcessor(Core.Constants.CountryCodes.Turkey, false, false);
				AssertCreateNctsMessageProcessor(Core.Constants.CountryCodes.Spain, false, false);
			});
		}

		public void TestSupportsValidateCustomsMessaging()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			using (header.Company.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				AssertEquals("Country not supported for auto sending.", false, header.SupportValidateCustomsMessaging);
			}

			using (header.Company.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				AssertEquals("Country supported for auto sending.", true, header.SupportValidateCustomsMessaging);
			}
		}

		public void TestReloadNctsWhenBillMissRelatedMoveDetail()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_HeaderType = NctsMovementType.Codes.Departure;

			var bill = header.Bills.AddNew();
			AssertNotNull("[PRE-REQ] Bill.MovementDetail", bill.MovementDetail);

			header.MovementHeader.Delete();
			Factory.Save();

			var reloadedHeader = new BusinessObjectFactory().Load<NctsHeader>(header.PK);
			AssertNoExceptionThrown("No StackOverflow", () => _ = reloadedHeader.Bills[0]);
		}

		public void TestForbidMovementHeaderDeletion()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_HeaderType = NctsMovementType.Codes.Departure;

			CombineAssertions("PRE-CONDITIONS", () =>
			{
				AssertEquals("DepartureMovementHeaders Collection Count", 1, header.DepartureMovementHeaders.Count);
				AssertSame("MovementHeader and the first element of DepartureMovementHeaders Collection are the same instance", header.MovementHeader, header.DepartureMovementHeaders[0]);
			});

			AssertEquals("First MovementHeader, CanDelete", false, header.MovementHeader.CanDelete);

			var additionalMovementHeader = header.DepartureMovementHeaders.AddNew();
			AssertEquals("Additional MovementHeader, CanDelete", true, additionalMovementHeader.CanDelete);
		}

		void AssertCreateNctsMessageProcessor(string countryCode, bool canSendArrival, bool canSendDeparture)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				var sendingSupporter = (INCTSAutoSendingMessageSupporter)header;

				var arrivalProcessor = sendingSupporter.CreateNCTSArrivalNotificationMessageProcessor();
				if (canSendArrival)
				{
					AssertType<AutoSendNCTSP5MessageProcessor>(arrivalProcessor);
				}
				else
				{
					AssertType<LogAction>($"The Send NCTS Message trigger is not supported in an Arrival Context for {countryCode}", arrivalProcessor);
				}

				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				var departureProcessor = sendingSupporter.CreateNCTSMessageProcessor();
				if (canSendDeparture)
				{
					AssertType<AutoSendNCTSP5MessageProcessor>(departureProcessor);
				}
				else
				{
					AssertType<LogAction>($"The Send NCTS Message trigger is not supported in a Departure Context for {countryCode}", departureProcessor);
				}
			}
		}

		static void AssertWarehouseCancelEvent(NctsHeader header, bool expectEventExists = true)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CancelTheWarehouseJobCode);
			var dataTransferEvents = header.Logs.Find(query);
			var expectedLogNumber = expectEventExists ? 1 : 0;
			AssertEquals(expectedLogNumber, dataTransferEvents.Length);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			header.MovementHeader.CustomsOffices.AddNew();
			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
		}

		NctsHeader header;

		#region Custom Fields Test
		[TestedType(typeof(NctsHeader))]
		sealed class NctsHeaderCustomFieldsTest : MasterFiles.Business.Testing.TestICustomFieldProvider
		{
		}
		#endregion

		#region CusInBondParentDummyBusinessObject

		sealed class CusInBondParentDummyBusinessObject : NctsHeader, ICusInBondParent
		{
			public CusInBondParentDummyBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			ZGuid ICusInBondParent.PK => PK;

			string ICusInBondParent.TablePrefix => TablePrefix;

			ZString ICusInBondParent.ParentType => nameof(CusInBondParentDummyBusinessObject);

			ZString ICusInBondParent.JobNumber => "";

			ZString ICusInBondParent.HouseBill => "";

			bool ICusInBondParent.IsVisible => true;

			bool ICusInBondParent.IsInternalBrokerage => false;

			event EventHandler ICusInBondParent.VisibilityChanged
			{
				add { }
				remove { }
			}

			ZGuid ICusInBondParent.GetDeclarationPK(ZGuid companyPK) => PK;

			void ICusInBondParent.PopulateJobNumberIfNeeded()
			{
			}
		}

		#endregion
	}
}
