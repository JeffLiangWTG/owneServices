using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using OrgHeader = Enterprise.MasterFiles.Business.OrgHeader;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class HouseConsignmentCustomsMessageWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when bill is null", () => new HouseConsignmentCustomsMessageWrapper(null));
		AssertNoExceptionThrown("When argument is not null", () => new HouseConsignmentCustomsMessageWrapper(bill));
	}

	public void TestSequenceNumber()
	{
		bill.SequenceNumber = 0;
		var wrapper = CreateNewWrapper();
		AssertEquals(nameof(IHouseConsignmentCustomsMessageWrapper.SequenceNumber), 0, wrapper.SequenceNumber);

		bill.SequenceNumber = 5;
		wrapper = CreateNewWrapper();
		AssertEquals(nameof(IHouseConsignmentCustomsMessageWrapper.SequenceNumber), 5, wrapper.SequenceNumber);
	}

	public void TestPreviousDocuments()
	{
		var wrapper = CreateNewWrapper();
		AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.PreviousDocuments), wrapper.PreviousDocuments);
		AssertEquals($"{nameof(IHouseConsignmentCustomsMessageWrapper.PreviousDocuments)}, Count", 0, wrapper.PreviousDocuments.Count);

		bill.PreviousDocuments.AddNew();
		wrapper = CreateNewWrapper();
		var previousDocuments = wrapper.PreviousDocuments;
		AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.PreviousDocuments), previousDocuments);
		AssertEquals($"{nameof(IHouseConsignmentCustomsMessageWrapper.PreviousDocuments)}, Count", 1, previousDocuments.Count);
		AssertType<PreviousDocumentWrapper>($"{nameof(IHouseConsignmentCustomsMessageWrapper.PreviousDocuments)}, Type", previousDocuments.Single());
		AssertSame($"{nameof(IHouseConsignmentCustomsMessageWrapper.PreviousDocuments)}, Cached", previousDocuments, wrapper.PreviousDocuments);
	}

	public void TestSupportingDocuments()
	{
		var wrapper = CreateNewWrapper();
		AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.SupportingDocuments), wrapper.SupportingDocuments);
		AssertEquals($"{nameof(IHouseConsignmentCustomsMessageWrapper.SupportingDocuments)}, Count", 0, wrapper.SupportingDocuments.Count);

		bill.SupportingDocuments.AddNew();
		wrapper = CreateNewWrapper();
		var supportingDocuments = wrapper.SupportingDocuments;
		AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.SupportingDocuments), supportingDocuments);
		AssertEquals($"{nameof(IHouseConsignmentCustomsMessageWrapper.SupportingDocuments)}, Count", 1, supportingDocuments.Count);
		AssertType<SupportingDocumentWrapper>($"{nameof(IHouseConsignmentCustomsMessageWrapper.SupportingDocuments)}, Type", supportingDocuments.Single());
		AssertSame($"{nameof(IHouseConsignmentCustomsMessageWrapper.SupportingDocuments)}, Cached", supportingDocuments, wrapper.SupportingDocuments);
	}

	public void TestAdditionalReferences()
	{
		var wrapper = CreateNewWrapper();
		AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalReferences), wrapper.AdditionalReferences);
		AssertEquals($"{nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalReferences)}, Count", 0, wrapper.AdditionalReferences.Count);

		bill.AdditionalDocuments.AddNew();
		bill.AdditionalDocuments.AddNew().CSI_SubType = "REF";
		wrapper = CreateNewWrapper();
		var additionalReferences = wrapper.AdditionalReferences;
		AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalReferences), additionalReferences);
		AssertEquals($"{nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalReferences)}, Count", 1, additionalReferences.Count);
		AssertType<AdditionalReferenceWrapper>($"{nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalReferences)}, Type", additionalReferences.Single());
		AssertSame($"{nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalReferences)}, Cached", additionalReferences, wrapper.AdditionalReferences);
	}

	public void TestTransportDocuments()
	{
		var wrapper = CreateNewWrapper();
		AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.TransportDocuments), wrapper.TransportDocuments);
		AssertEquals($"{nameof(IHouseConsignmentCustomsMessageWrapper.TransportDocuments)}, Count", 0, wrapper.TransportDocuments.Count);

		bill.AdditionalDocuments.AddNew();
		bill.AdditionalDocuments.AddNew().CSI_SubType = "TRA";
		wrapper = CreateNewWrapper();
		var transportDocuments = wrapper.TransportDocuments;
		AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.TransportDocuments), transportDocuments);
		AssertEquals($"{nameof(IHouseConsignmentCustomsMessageWrapper.TransportDocuments)}, Count", 1, transportDocuments.Count);
		AssertType<TransportDocumentWrapper>($"{nameof(IHouseConsignmentCustomsMessageWrapper.TransportDocuments)}, Type", transportDocuments.Single());
		AssertSame($"{nameof(IHouseConsignmentCustomsMessageWrapper.TransportDocuments)}, Cached", transportDocuments, wrapper.TransportDocuments);

		using (SetNctsTrasitionPeriod(isActive: true))
		{
			CombineAssertions("When Transition period ON", () =>
			{
				wrapper = CreateNewWrapper();
				AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.TransportDocuments), wrapper.TransportDocuments);
				AssertEquals($"{nameof(IHouseConsignmentCustomsMessageWrapper.TransportDocuments)}, Count", 0, wrapper.TransportDocuments.Count);
			});
		}
	}

	public void TestUcr()
	{
		bill.B0_ReferenceID = ZString.Empty;
		var wrapper = CreateNewWrapper();
		AssertNullOrEmpty(nameof(IHouseConsignmentCustomsMessageWrapper.Ucr), wrapper.Ucr);

		bill.B0_ReferenceID = "ABC";
		wrapper = CreateNewWrapper();
		AssertEquals(nameof(IHouseConsignmentCustomsMessageWrapper.Ucr), "ABC", wrapper.Ucr);
	}

	public void TestAdditionalInformation()
	{
		var wrapper = CreateNewWrapper();
		AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalInformation), wrapper.AdditionalInformation);
		AssertEquals($"{nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalInformation)}, Count", 0, wrapper.AdditionalInformation.Count);

		bill.AdditionalDocuments.AddNew();
		bill.AdditionalDocuments.AddNew().CSI_SubType = "INF";
		wrapper = CreateNewWrapper();
		var additionalInformation = wrapper.AdditionalInformation;
		AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalInformation), additionalInformation);
		AssertEquals($"{nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalInformation)}, Count", 1, additionalInformation.Count);
		AssertType<AdditionalInformationWrapper>($"{nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalInformation)}, Type", additionalInformation.Single());
		AssertSame($"{nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalInformation)}, Cached", additionalInformation, wrapper.AdditionalInformation);
	}

	public void TestConsignor()
	{
		var wrapper = CreateNewWrapper();
		AssertNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignor), wrapper.Consignor);

		var consignor = Factory.NewWithValidTestData<OrgHeader>();
		bill.Consignor.OrganisationPK = consignor.PK;
		wrapper = CreateNewWrapper();
		AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignor), wrapper.Consignor);
	}

	public void TestConsignee()
	{
		using (SetNctsTrasitionPeriod(true))
		{
			var wrapper = CreateNewWrapper();
			AssertNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignee), wrapper.Consignee);

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			bill.Consignee.OrganisationPK = consignee.PK;
			wrapper = CreateNewWrapper();
			AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignee), wrapper.Consignee);
		}
	}

	public void TestHouseConsigneeDuringNonTransitionPeriodWithPortIsNotC0009AndSecurityBTH()
	{
		SetupC0009List();
		using (SetNctsTrasitionPeriod(false))
		{
			var wrapper = CreateNewWrapper();
			AssertNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignee), wrapper.Consignee);

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			bill.Consignee.OrganisationPK = consignee.PK;

			var consigneeAddress = consignee.Addresses.AddNew();
			bill.Consignee.E2_OA_Address = consigneeAddress.PK;
			bill.Header.Consignee.E2_OA_Address = consigneeAddress.PK;

			bill.Header.MovementHeader.BM_RL_NKDestinationPort = "IN";
			bill.Header.MovementHeader.BM_TypeOfSecurity = "BTH";

			wrapper = CreateNewWrapper();
			AssertNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignee), wrapper.Consignee);
		}
	}

	public void TestHouseConsigneeDuringNonTransitionPeriodWithPortIsC0009()
	{
		SetupC0009List();
		using (SetNctsTrasitionPeriod(false))
		{
			var header = bill.Header;
			var movementHeader = header.MovementHeader;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			bill.Consignee.OrganisationPK = consignee.PK;
			var consigneeAddress = consignee.Addresses.AddNew();

			movementHeader.BM_RL_NKDestinationPort = "IT";
			var bill1 = header.Bills.AddNew();
			bill1.Consignee.E2_OA_Address = consigneeAddress.PK;

			var wrapper = CreateNewWrapper();
			AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignee), wrapper.Consignee);
			AssertType<EoriOrTcuTraderWrapper>(wrapper.Consignee);
		}
	}

	public void TestHouseConsignee_AgainstAdditionalInfo30600()
	{
		SetupC0009List();
		using (SetNctsTrasitionPeriod(false))
		{
			var header = bill.Header;
			var movementHeader = header.MovementHeader;

			movementHeader.BM_TypeOfSecurity = "BTH";
			movementHeader.BM_RL_NKDestinationPort = "ES";

			var bill1 = header.Bills.AddNew();
			bill1.Consignee.OrganisationPK = ZGuid.Empty;
			bill.Consignee.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var headerAdditionalDocument30600 = header.AdditionalDocuments.AddNew();
			headerAdditionalDocument30600.CSI_Code = "30600";
			headerAdditionalDocument30600.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

			var wrapper = CreateNewWrapper();
			AssertNull("When Header has 30600 Additional Document, Consignee",
				wrapper.Consignee);

			header.AdditionalDocuments.RemoveAndDeleteAll();
			Add30600AdditionalInfo(bill1);
			wrapper = CreateNewWrapper();
			AssertNotNull("When Bill has not 30600 Additional Document, Consignee", wrapper.Consignee);
			AssertType<EoriOrTcuTraderWrapper>(wrapper.Consignee);

			Add30600AdditionalInfo(bill);
			wrapper = CreateNewWrapper();
			AssertNull("When Bill has 30600 Additional Document, Consignee",
				wrapper.Consignee);
		}

		static void Add30600AdditionalInfo(NctsBill bill)
		{
			var additionalDocument30600 = bill.AdditionalDocuments.AddNew();
			additionalDocument30600.CSI_Code = "30600";
			additionalDocument30600.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		}
	}

	public void TestHouseConsigneeWithHouseConsignmentInstancesHavingSameConsignee()
	{
		SetupC0009List();
		using (SetNctsTrasitionPeriod(false))
		{
			var header = bill.Header;
			var movementHeader = header.MovementHeader;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			bill.Consignee.OrganisationPK = consignee.PK;

			movementHeader.BM_RL_NKDestinationPort = "IN";
			movementHeader.BM_TypeOfSecurity = "BTH";
			header.AdditionalDocuments.Clear();
			Add30600AdditionalInfo(bill.AdditionalDocuments);

			var wrapper = CreateNewWrapper();
			AssertNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignee), wrapper.Consignee);

			bill.AdditionalDocuments.RemoveAndDeleteAll();
			var houseConsignment1 = header.Bills.AddNew();
			var commonAddress = consignee.Addresses.AddNew();
			houseConsignment1.Consignee.E2_OA_Address = commonAddress.PK;
			movementHeader.BM_TypeOfSecurity = "NON";
			movementHeader.BM_RL_NKDestinationPort = "IT";
			header.Consignee.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().Addresses.AddNew().PK;

			wrapper = CreateNewWrapper();
			AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignee), wrapper.Consignee);

			var houseConsignment2 = header.Bills.AddNew();
			houseConsignment2.Consignee.E2_OA_Address = commonAddress.PK;

			var houseConsignment3 = header.Bills.AddNew();
			houseConsignment3.Consignee.E2_OA_Address = commonAddress.PK;

			wrapper = CreateNewWrapper();
			AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignee), wrapper.Consignee);

			bill.Consignee.E2_OA_Address = commonAddress.PK;
			wrapper = CreateNewWrapper();
			AssertNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignee), wrapper.Consignee);

			header.Consignee.E2_OA_Address = commonAddress.PK;
			bill.Consignee.E2_OA_Address = ZGuid.Empty;
			wrapper = CreateNewWrapper();
			AssertNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignee), wrapper.Consignee);
		}
	}

	public void TestHouseConsigneeWithHouseConsignmentInstancesHavingDifferentConsignee()
	{
		SetupC0009List();
		using (SetNctsTrasitionPeriod(false))
		{
			var header = bill.Header;
			var movementHeader = header.MovementHeader;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			bill.Consignee.OrganisationPK = consignee.PK;
			movementHeader.BM_RL_NKDestinationPort = "IN";
			movementHeader.BM_TypeOfSecurity = "BTH";
			header.AdditionalDocuments.Clear();

			Add30600AdditionalInfo(bill.AdditionalDocuments);
			var wrapper = CreateNewWrapper();
			AssertNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignee), wrapper.Consignee);

			bill.AdditionalDocuments.RemoveAndDeleteAll();
			var houseConsignment1 = header.Bills.AddNew();
			var commonAddress = consignee.Addresses.AddNew();
			houseConsignment1.Consignee.E2_OA_Address = commonAddress.PK;
			movementHeader.BM_TypeOfSecurity = "NON";
			movementHeader.BM_RL_NKDestinationPort = "IT";

			wrapper = CreateNewWrapper();
			AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignee), wrapper.Consignee);

			var houseConsignment2 = header.Bills.AddNew();
			houseConsignment2.Consignee.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().Addresses.AddNew().PK;
			var houseConsignment3 = header.Bills.AddNew();
			houseConsignment3.Consignee.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().Addresses.AddNew().PK;

			wrapper = CreateNewWrapper();
			AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.Consignee), wrapper.Consignee);
		}
	}

	public void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateNewWrapper();
		AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals($"{nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalSupplyChainActors)}, Count", 0, wrapper.AdditionalSupplyChainActors.Count);

		bill.CusSupplyChainActorReferences.AddNew();
		wrapper = CreateNewWrapper();
		var additionalSupplyChainActors = wrapper.AdditionalSupplyChainActors;
		AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalSupplyChainActors), additionalSupplyChainActors);
		AssertEquals($"{nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalSupplyChainActors)}, Count", 1, additionalSupplyChainActors.Count);
		AssertType<AdditionalSupplyChainActorWrapper>($"{nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalSupplyChainActors)}, Type", additionalSupplyChainActors.Single());
		AssertSame($"{nameof(IHouseConsignmentCustomsMessageWrapper.AdditionalSupplyChainActors)}, Cached", additionalSupplyChainActors, wrapper.AdditionalSupplyChainActors);
	}

	public void TestTransportChargesMethodOfPayment()
	{
		using (SetNctsTrasitionPeriod(isActive: true))
		{
			bill.B0_TransportPaymentMethod = ZString.Empty;
			var wrapper = CreateNewWrapper();
			AssertNullOrEmpty(nameof(IHouseConsignmentCustomsMessageWrapper.TransportChargesMethodOfPayment), wrapper.TransportChargesMethodOfPayment);

			bill.B0_TransportPaymentMethod = "2";
			wrapper = CreateNewWrapper();
			AssertEquals(nameof(IHouseConsignmentCustomsMessageWrapper.TransportChargesMethodOfPayment), "2", wrapper.TransportChargesMethodOfPayment);
		}

		using (SetNctsTrasitionPeriod(isActive: false))
		{
			AssertWrapperFieldWithSingleLine(
				"MethodOfPayment",
				(bill, value) => { bill.B0_TransportPaymentMethod = value; },
				(movementHeader, value) => { movementHeader.BM_MethodOfPayment = value; },
				(wrapper) => wrapper.TransportChargesMethodOfPayment);

			AssertWrapperFieldWithMultiLine(
				"MethodOfPayment",
				(bill, value) => { bill.B0_TransportPaymentMethod = value; },
				(movementHeader, value) => { movementHeader.BM_MethodOfPayment = value; },
				(wrapper) => wrapper.TransportChargesMethodOfPayment);
		}
	}

	public void TestCountryOfDispatch()
	{
		bill.B0_RN_NKCountryOfExport = ZString.Empty;
		var wrapper = CreateNewWrapper();
		AssertNullOrEmpty(nameof(IHouseConsignmentCustomsMessageWrapper.CountryOfDispatch), wrapper.CountryOfDispatch);

		bill.B0_RN_NKCountryOfExport = "IT";
		wrapper = CreateNewWrapper();
		AssertEquals(nameof(IHouseConsignmentCustomsMessageWrapper.CountryOfDispatch), "IT", wrapper.CountryOfDispatch);
	}

	public void TestGrossMass()
	{
		bill.B0_Weight = 0m;
		var wrapper = CreateNewWrapper();
		AssertEquals(nameof(IHouseConsignmentCustomsMessageWrapper.GrossMass), 0m, wrapper.GrossMass);

		bill.B0_Weight = 1.23m;
		wrapper = CreateNewWrapper();
		AssertEquals(nameof(IHouseConsignmentCustomsMessageWrapper.GrossMass), 1.23m, wrapper.GrossMass);
	}

	public void TestDepartureMeansOfTransports_WhenTransitionPeriodIsOff()
	{
		using (SetNctsTrasitionPeriod(isActive: false))
		{
			CombineAssertions("When Transition period Off", () =>
			{
				bill.Header.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
				bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._40;
				bill.TransportAtDeparture = "Air-N Schelde";
				bill.TransportCountryAtDeparture = "IT";

				var wrapper = CreateNewWrapper();
				AssertEquals("When all Transport Departure fields of House Consignments are the same, they should be written at the header level.", 0, wrapper.DepartureMeansOfTransports.Count);

				var bill2 = header.Bills.AddNew();
				bill2.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._41;
				bill.TransportAtDeparture = "Air-N Schelde";
				bill.TransportCountryAtDeparture = "IT";

				wrapper = CreateNewWrapper();

				AssertEquals("When the Transport Departure fields of House Consignments differ, they should be written at the line level.", 1, wrapper.DepartureMeansOfTransports.Count);
				AssertEquals("TypeOfIdentification at line level", 40, wrapper.DepartureMeansOfTransports.First().TypeOfIdentification);
			});
		}
	}

	public void TestDepartureMeansOfTransports_AlwaysEmptyList_WhenTransitionPeriodIsON()
	{
		using (SetNctsTrasitionPeriod(isActive: true))
		{
			CombineAssertions("When Transition period ON", () =>
			{
				bill.Header.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
				bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._40;
				bill.TransportAtDeparture = "Air-N Schelde";
				bill.TransportCountryAtDeparture = "IT";

				var wrapper = CreateNewWrapper();
				AssertNotNull(nameof(IHouseConsignmentCustomsMessageWrapper.DepartureMeansOfTransports), wrapper.DepartureMeansOfTransports);
				AssertEquals($"{nameof(IHouseConsignmentCustomsMessageWrapper.DepartureMeansOfTransports)}, count", 0, wrapper.DepartureMeansOfTransports.Count);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		bill = header.Bills.AddNew();
	}

	void AssertWrapperFieldWithSingleLine(
		string fieldName,
		Action<NctsBill, string> setLineFieldTo,
		Action<NctsDepartureMovementHeader, string> setHeaderTo,
		Func<IHouseConsignmentCustomsMessageWrapper, string> getWrapperFieldValue)
	{
		var header = Factory.NewDepartureNctsHeaderPhase5();
		var movementHeader = header.MovementHeader;
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			setHeaderTo(movementHeader, "X");
			setLineFieldTo(bill, string.Empty);

			var wrapper = CreateNewWrapper(bill);
			AssertEquals($"When {fieldName} is only provided at header level and not at line, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(bill, "X");
			wrapper = CreateNewWrapper(bill);
			AssertEquals($"When line and header have same {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(bill, "Y");
			wrapper = CreateNewWrapper(bill);
			AssertEquals($"When line has {fieldName} different from header, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setHeaderTo(movementHeader, string.Empty);
			setLineFieldTo(bill, "Y");
			wrapper = CreateNewWrapper(bill);
			AssertEquals($"When line has {fieldName} and header is empty, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(bill, string.Empty);
			wrapper = CreateNewWrapper(bill);
			AssertEquals($"When both line and header don't have {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));
		});
	}

	void AssertWrapperFieldWithMultiLine(
		string fieldName,
		Action<NctsBill, string> setLineFieldTo,
		Action<NctsDepartureMovementHeader, string> setHeaderTo,
		Func<IHouseConsignmentCustomsMessageWrapper, string> getWrapperFieldValue)
	{
		var header = Factory.NewDepartureNctsHeaderPhase5();
		var movementHeader = header.MovementHeader;
		var bill1 = header.Bills.AddNew();
		var bill2 = header.Bills.AddNew();

		CombineAssertions($"{fieldName} only in lines", () =>
		{
			setHeaderTo(movementHeader, string.Empty);
			setLineFieldTo(bill1, "X");
			setLineFieldTo(bill2, string.Empty);

			var wrapper1 = CreateNewWrapper(bill1);
			var wrapper2 = CreateNewWrapper(bill2);
			AssertEquals($"When {fieldName} is only available in one line, in line 1", "X", getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is only available in one line, in line 2", string.Empty, getWrapperFieldValue(wrapper2));

			setLineFieldTo(bill1, "Y");
			setLineFieldTo(bill2, "Y");
			wrapper1 = CreateNewWrapper(bill1);
			wrapper2 = CreateNewWrapper(bill2);
			AssertEquals($"When all lines have same {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper1));
			AssertEquals($"When all lines have same {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));

			setLineFieldTo(bill2, "Z");
			wrapper1 = CreateNewWrapper(bill1);
			wrapper2 = CreateNewWrapper(bill2);
			AssertEquals($"When lines have different {fieldName}, {fieldName}", "Y", getWrapperFieldValue(wrapper1));
			AssertEquals($"When lines have different {fieldName}, {fieldName}", "Z", getWrapperFieldValue(wrapper2));

			setLineFieldTo(bill2, string.Empty);
			wrapper1 = CreateNewWrapper(bill1);
			wrapper2 = CreateNewWrapper(bill2);
			AssertEquals($"When lines have different {fieldName} (one does not have value), {fieldName}", "Y", getWrapperFieldValue(wrapper1));
			AssertEquals($"When lines have different {fieldName} (one does not have value), {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));

			setLineFieldTo(bill1, string.Empty);
			wrapper1 = CreateNewWrapper(bill1);
			wrapper2 = CreateNewWrapper(bill2);
			AssertEquals($"When lines have same {fieldName} (all do not have value), {fieldName}", string.Empty, getWrapperFieldValue(wrapper1));
			AssertEquals($"When lines have same {fieldName} (all do not have value), {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));
		});

		CombineAssertions($"{fieldName} both in lines and header", () =>
		{
			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(bill1, "Y");
			setLineFieldTo(bill2, "Y");
			var wrapper1 = CreateNewWrapper(bill1);
			var wrapper2 = CreateNewWrapper(bill2);
			AssertEquals($"When {fieldName} is the same in lines and header, {fieldName}", string.Empty, getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is the same in lines and header, {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(bill1, "X");
			setLineFieldTo(bill2, "Z");
			wrapper1 = CreateNewWrapper(bill1);
			wrapper2 = CreateNewWrapper(bill2);
			AssertEquals($"When {fieldName} is present in lines and header and different in all cases, {fieldName}", "X", getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is present in lines and header and different in all cases, {fieldName}", "Z", getWrapperFieldValue(wrapper2));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(bill1, "X");
			setLineFieldTo(bill2, string.Empty);
			wrapper1 = CreateNewWrapper(bill1);
			wrapper2 = CreateNewWrapper(bill2);
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere, {fieldName}", "X", getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere, {fieldName}", "Y", getWrapperFieldValue(wrapper2));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(bill1, string.Empty);
			setLineFieldTo(bill2, string.Empty);
			wrapper1 = CreateNewWrapper(bill1);
			wrapper2 = CreateNewWrapper(bill2);
			AssertEquals($"When {fieldName} is only available at header level, {fieldName}", string.Empty, getWrapperFieldValue(wrapper1));
			AssertEquals($"When {fieldName} is only available at header level, {fieldName}", string.Empty, getWrapperFieldValue(wrapper2));
		});
	}

	AdditionalInfo Add30600AdditionalInfo(ICusSupportingInfoCollection<AdditionalInfo> additionalInfoCollection)
	{
		var additionalInfo = additionalInfoCollection.AddNew();
		additionalInfo.CSI_Code = AdditionalDocumentTypes._30600;
		additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

		return additionalInfo;
	}

	void SetupC0009List()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "C0009 Desc");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy,
			EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009,
			Core.Constants.CountryCodes.Italy,
			ZDateTime.MinSmallDateTimeValue,
			ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
	}

	IHouseConsignmentCustomsMessageWrapper CreateNewWrapper(NctsBill bill = null)
	{
		return new HouseConsignmentCustomsMessageWrapper(bill ?? this.bill);
	}

	IDisposable SetNctsTrasitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);

	NctsBill bill;
	NctsHeader header;
}
