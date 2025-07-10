using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonHouseConsignmentDepartureAndAmendmentWrapperTest : WrapperHelperTest<NCTS5CommonHouseConsignmentDepartureAndAmendmentWrapper>
	{
		public void TestAdditionalSupplyChainActor()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalSupplyChainActor when no data declared", 0, wrapper.AdditionalSupplyChainActor.Count);

				nctsBill.CusSupplyChainActorReferences.AddNew();
				wrapper = GetWrapper(nctsBill);
				var additionalSupplyChainActor = wrapper.AdditionalSupplyChainActor;
				AssertEquals("Expected filled AdditionalSupplyChainActor", 1, additionalSupplyChainActor.Count);
				AssertSame("Cached AdditionalSupplyChainActor", wrapper.AdditionalSupplyChainActor, additionalSupplyChainActor);
			});
		}

		public void TestConsignmentItem()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty ConsignmentItem when no goodsItem declared", 0, wrapper.ConsignmentItem.Count);

				nctsBill.GoodsItems.AddNew();
				nctsBill.GoodsItems.AddNew();

				wrapper = GetWrapper(nctsBill);
				var consignmentItem = wrapper.ConsignmentItem;
				AssertEquals("Expected filled with 2 ConsignmentItem", 2, consignmentItem.Count);
				AssertSame("Cached ConsignmentItem", wrapper.ConsignmentItem, consignmentItem);
			});
		}

		public void TestDeclarationTypeInLines()
		{
			CombineAssertions(() =>
			{
				var goodsItem1 = nctsBill.GoodsItems.AddNew();
				goodsItem1.BY_Type = NctsPhase5DeclarationTypeList.Codes.T1;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
				wrapper = GetWrapper(nctsBill);
				AssertEquals("Expected empty DeclarationType in ConsignmentItems when header's declaration type is not T", ZString.Empty, wrapper.ConsignmentItem.ToArray()[0].DeclarationType);

				nctsHeader.MovementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
				wrapper = GetWrapper(nctsBill);
				AssertEquals("Expected empty DeclarationType in ConsignmentItems when header's declaration type is T but there is only one goodsItem", ZString.Empty, wrapper.ConsignmentItem.ToArray()[0].DeclarationType);

				var goodsItem2 = nctsBill.GoodsItems.AddNew();
				goodsItem2.BY_Type = NctsPhase5DeclarationTypeList.Codes.T2;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
				wrapper = GetWrapper(nctsBill);
				AssertContainsExactElementsInAnyOrder("Expected filled DeclarationType in ConsignmentItem when header's declaration type is T and there are multiple goodsItems with different codes", new ZString[] { "T1", "T2" }, wrapper.ConsignmentItem.Select(x => x.DeclarationType).ToArray());

				goodsItem2.BY_Type = NctsPhase5DeclarationTypeList.Codes.T1;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
				wrapper = GetWrapper(nctsBill);
				AssertContainsExactElementsInAnyOrder("Expected empty DeclarationType in ConsignmentItem when header's declaration type is T and there are multiple goodsItems with same codes", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.ConsignmentItem.Select(x => x.DeclarationType).ToArray());
			});
		}

		public void TestCountryOfDispatch()
		{
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch = "PT";
				nctsBill.B0_RN_NKCountryOfExport = "ES";
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					AssertEquals("TransitionalPeriod: Expected not filled CountryOfDispatch", ZString.Empty, wrapper.CountryOfDispatch);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					AssertEquals("FinalPeriod: Expected not filled CountryOfDispatch when shouldDeclareCountryOfDispatchInHouseOrItem flag is false", ZString.Empty, wrapper.CountryOfDispatch);

					var goodsItem1 = nctsBill.GoodsItems.AddNew();
					goodsItem1.BY_RN_NKCountryOfDispatch = "ES";
					wrapper = GetWrapper(nctsBill, shouldDeclareCountryOfDispatchInHouseOrItem: true);
					AssertEquals("FinalPeriod: Expected filled CountryOfDispatch when shouldDeclareCountryOfDispatchInHouseOrItem flag is true and Item Country of Dispatch is the same", "ES", wrapper.CountryOfDispatch);

					goodsItem1.BY_RN_NKCountryOfDispatch = "FR";
					wrapper = GetWrapper(nctsBill, shouldDeclareCountryOfDispatchInHouseOrItem: true);
					AssertEquals("FinalPeriod: Expected not filled CountryOfDispatch when shouldDeclareCountryOfDispatchInHouseOrItem flag is true but Item Country of Dispatch is not the same", ZString.Empty, wrapper.CountryOfDispatch);

					nctsBill.B0_RN_NKCountryOfExport = ZString.Empty;
					goodsItem1.BY_RN_NKCountryOfDispatch = ZString.Empty;
					wrapper = GetWrapper(nctsBill, shouldDeclareCountryOfDispatchInHouseOrItem: true);
					AssertEquals("FinalPeriod: Expected filled CountryOfDispatch from Header when shouldDeclareCountryOfDispatchInHouseOrItem flag is true and Bill Country of Dispatch is Empty", "PT", wrapper.CountryOfDispatch);
				}
			});
		}

		public void TestCountryOfDispatchInLines()
		{
			CombineAssertions(() =>
			{
				nctsBill.B0_RN_NKCountryOfExport = "ES";

				var goodsItem1 = nctsBill.GoodsItems.AddNew();
				goodsItem1.BY_RN_NKCountryOfDispatch = "FR";
				AssertEquals("Expected empty CountryOfDispatch when shouldDeclareCountryOfDispatchInHouseOrItem flag is false", ZString.Empty, wrapper.ConsignmentItem.ToArray()[0].CountryOfDispatch);

				wrapper = GetWrapper(nctsBill, shouldDeclareCountryOfDispatchInHouseOrItem: true);
				AssertEquals("Expected filled CountryOfDispatch when shouldDeclareCountryOfDispatchInHouseOrItem flag is true and there is only one goodsItem and code different than in bill", "FR", wrapper.ConsignmentItem.ToArray()[0].CountryOfDispatch);

				goodsItem1.BY_RN_NKCountryOfDispatch = "ES";
				wrapper = GetWrapper(nctsBill, shouldDeclareCountryOfDispatchInHouseOrItem: true);
				AssertEquals("Expected empty CountryOfDispatch when shouldDeclareCountryOfDispatchInHouseOrItem flag is true and there is only one goodsItem and code the same as in bill", ZString.Empty, wrapper.ConsignmentItem.ToArray()[0].CountryOfDispatch);

				var goodsItem2 = nctsBill.GoodsItems.AddNew();
				goodsItem2.BY_RN_NKCountryOfDispatch = "DE";
				wrapper = GetWrapper(nctsBill, shouldDeclareCountryOfDispatchInHouseOrItem: true);
				AssertContainsExactElementsInAnyOrder("Expected filled CountryOfDispatch when shouldDeclareCountryOfDispatchInHouseOrItem flag is true and there are multiple goodsItems at least one different code than in bill", new ZString[] { "ES", "DE" }, wrapper.ConsignmentItem.Select(x => x.CountryOfDispatch).ToArray());

				goodsItem2.BY_RN_NKCountryOfDispatch = ZString.Empty;
				wrapper = GetWrapper(nctsBill, shouldDeclareCountryOfDispatchInHouseOrItem: true);
				AssertContainsExactElementsInAnyOrder("Expected empty CountryOfDispatch when shouldDeclareCountryOfDispatchInHouseOrItem flag is true and there are multiple goodsItems where values are empty or the same as in bill", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.ConsignmentItem.Select(x => x.CountryOfDispatch).ToArray());
			});
		}

		public void TestCountryOfDestination()
		{
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_RL_NKDestinationPort = "PT";
				nctsBill.B0_RN_NKCountryOfDestination = "ES";
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					AssertEquals("TransitionalPeriod: Expected not filled CountryOfDestination", ZString.Empty, wrapper.CountryOfDestination);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					AssertEquals("FinalPeriod: Expected not filled CountryOfDestination when shouldDeclareCountryOfDestinationInHouseOrItem flag is false", ZString.Empty, wrapper.CountryOfDestination);

					var goodsItem1 = nctsBill.GoodsItems.AddNew();
					goodsItem1.BY_RN_NKCountryOfDestination = "ES";
					wrapper = GetWrapper(nctsBill, shouldDeclareCountryOfDestinationInHouseOrItem: true);
					AssertEquals("FinalPeriod: Expected filled CountryOfDestination when shouldDeclareCountryOfDestinationInHouseOrItem flag is true and Item Country of Destination is the same", "ES", wrapper.CountryOfDestination);

					goodsItem1.BY_RN_NKCountryOfDestination = "FR";
					wrapper = GetWrapper(nctsBill, shouldDeclareCountryOfDestinationInHouseOrItem: true);
					AssertEquals("FinalPeriod: Expected not filled CountryOfDestination when shouldDeclareCountryOfDestinationInHouseOrItem flag is true but Item Country of Destination is not the same", ZString.Empty, wrapper.CountryOfDestination);

					nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;
					goodsItem1.BY_RN_NKCountryOfDestination = ZString.Empty;
					wrapper = GetWrapper(nctsBill, shouldDeclareCountryOfDestinationInHouseOrItem: true);
					AssertEquals("FinalPeriod: Expected filled CountryOfDestination from Header when shouldDeclareCountryOfDestinationInHouseOrItem flag is true and Bill Country of Destination is Empty", "PT", wrapper.CountryOfDestination);
				}
			});
		}

		public void TestCountryOfDestinationInLines()
		{
			CombineAssertions(() =>
			{
				nctsBill.B0_RN_NKCountryOfDestination = "ES";

				var goodsItem1 = nctsBill.GoodsItems.AddNew();
				goodsItem1.BY_RN_NKCountryOfDestination = "FR";
				AssertEquals("Expected empty CountryOfDestination when shouldDeclareCountryOfDestinationInHouseOrItem flag is false", ZString.Empty, wrapper.ConsignmentItem.ToArray()[0].CountryOfDestination);

				wrapper = GetWrapper(nctsBill, shouldDeclareCountryOfDestinationInHouseOrItem: true);
				AssertEquals("Expected filled CountryOfDestination when shouldDeclareCountryOfDestinationInHouseOrItem flag is true and there is only one goodsItem and code different than in bill", "FR", wrapper.ConsignmentItem.ToArray()[0].CountryOfDestination);

				goodsItem1.BY_RN_NKCountryOfDestination = "ES";
				wrapper = GetWrapper(nctsBill, shouldDeclareCountryOfDestinationInHouseOrItem: true);
				AssertEquals("Expected empty CountryOfDestination when shouldDeclareCountryOfDestinationInHouseOrItem flag is true and there is only one goodsItem and code the same as in bill", ZString.Empty, wrapper.ConsignmentItem.ToArray()[0].CountryOfDestination);

				var goodsItem2 = nctsBill.GoodsItems.AddNew();
				goodsItem2.BY_RN_NKCountryOfDestination = "DE";
				wrapper = GetWrapper(nctsBill, shouldDeclareCountryOfDestinationInHouseOrItem: true);
				AssertContainsExactElementsInAnyOrder("Expected filled CountryOfDestination when shouldDeclareCountryOfDestinationInHouseOrItem flag is true and there are multiple goodsItems at least one different code than in bill", new ZString[] { "ES", "DE" }, wrapper.ConsignmentItem.Select(x => x.CountryOfDestination).ToArray());

				goodsItem2.BY_RN_NKCountryOfDestination = ZString.Empty;
				wrapper = GetWrapper(nctsBill, shouldDeclareCountryOfDestinationInHouseOrItem: true);
				AssertContainsExactElementsInAnyOrder("Expected empty CountryOfDestination when shouldDeclareCountryOfDestinationInHouseOrItem flag is true and there are multiple goodsItems where values are empty or the same as in bill", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.ConsignmentItem.Select(x => x.CountryOfDestination).ToArray());
			});
		}

		public void TestReferenceNumberUCRInLines()
		{
			CombineAssertions(() =>
			{
				nctsBill.B0_ReferenceID = "reference";

				var goodsItem1 = nctsBill.GoodsItems.AddNew();
				goodsItem1.BY_CommercialReferenceNumber = "reference2";
				AssertEquals("Expected empty ReferenceNumberUCR when shouldDeclareReferenceNumberUCRInItem flag is false", ZString.Empty, wrapper.ConsignmentItem.ToArray()[0].ReferenceNumberUCR);

				wrapper = GetWrapper(nctsBill, shouldDeclareReferenceNumberUCRInItem: true);
				AssertEquals("Expected filled ReferenceNumberUCR when shouldDeclareReferenceNumberUCRInItem flag is true and there is only one goodsItem and code different than in bill", "reference2", wrapper.ConsignmentItem.ToArray()[0].ReferenceNumberUCR);

				goodsItem1.BY_CommercialReferenceNumber = "reference";
				wrapper = GetWrapper(nctsBill, shouldDeclareReferenceNumberUCRInItem: true);
				AssertEquals("Expected empty ReferenceNumberUCR when shouldDeclareReferenceNumberUCRInItem flag is true and there is only one goodsItem and code the same as in bill", ZString.Empty, wrapper.ConsignmentItem.ToArray()[0].ReferenceNumberUCR);

				var goodsItem2 = nctsBill.GoodsItems.AddNew();
				goodsItem2.BY_CommercialReferenceNumber = "reference3";
				wrapper = GetWrapper(nctsBill, shouldDeclareReferenceNumberUCRInItem: true);
				AssertContainsExactElementsInAnyOrder("Expected filled ReferenceNumberUCR when shouldDeclareReferenceNumberUCRInItem flag is true and there are multiple goodsItems at least one different code than in bill", new ZString[] { "reference", "reference3" }, wrapper.ConsignmentItem.Select(x => x.ReferenceNumberUCR).ToArray());

				goodsItem2.BY_CommercialReferenceNumber = ZString.Empty;
				wrapper = GetWrapper(nctsBill, shouldDeclareReferenceNumberUCRInItem: true);
				AssertContainsExactElementsInAnyOrder("Expected empty ReferenceNumberUCR when shouldDeclareReferenceNumberUCRInItem flag is true and there are multiple goodsItems where values are empty or the same as in bill", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.ConsignmentItem.Select(x => x.ReferenceNumberUCR).ToArray());
			});
		}

		public void TestConsignor()
		{
			CombineAssertions(() =>
			{
				var orgAddress1 = Factory.New<OrgAddress>();
				var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader1.OH_FullName = "Name1";
				orgAddress1.OA_OH = orgHeader1.PK;
				nctsHeader.Consignor.E2_OA_Address = orgAddress1.PK;

				var orgAddress2 = Factory.New<OrgAddress>();
				var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader2.OH_FullName = "Name2";
				orgAddress2.OA_OH = orgHeader2.PK;
				nctsBill.Consignor.E2_OA_Address = orgAddress2.PK;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					AssertNull("TransitionalPeriod: Expected not filled Consignor", wrapper.Consignor);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					AssertNull("FinalPeriod: Expected not filled Consignor when shouldDeclareConsignorInHouse flag is false", wrapper.Consignor);

					wrapper = GetWrapper(nctsBill, shouldDeclareConsignorInHouse: true);
					AssertNotNull("FinalPeriod: Expected filled Consignor from Bill when shouldDeclareConsignorInHouse flag is true", wrapper.Consignor);
					AssertEquals("FinalPeriod: Expected Consignor from Bill when shouldDeclareConsignorInHouse flag is true", "Name2", wrapper.Consignor.Name);

					nctsBill.Consignor.E2_OA_Address = ZGuid.Empty;
					wrapper = GetWrapper(nctsBill, shouldDeclareConsignorInHouse: true);
					AssertNotNull("FinalPeriod: Expected filled Consignor from Header when shouldDeclareConsignorInHouse flag is true and Bill Consignor is Empty", wrapper.Consignor);
					AssertEquals("FinalPeriod: Expected Consignor from Header when shouldDeclareConsignorInHouse flag is true and Bill Consignor is Empty", "Name1", wrapper.Consignor.Name);
				}
			});
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				var orgAddress1 = Factory.New<OrgAddress>();
				var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader1.OH_FullName = "Name1";
				orgAddress1.OA_OH = orgHeader1.PK;
				nctsHeader.Consignee.E2_OA_Address = orgAddress1.PK;

				var orgAddress2 = Factory.New<OrgAddress>();
				var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader2.OH_FullName = "Name2";
				orgAddress2.OA_OH = orgHeader2.PK;
				nctsBill.Consignee.E2_OA_Address = orgAddress2.PK;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					AssertNull("TransitionalPeriod: Expected not filled Consignee", wrapper.Consignee);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					AssertNull("FinalPeriod: Expected not filled Consignee when shouldDeclareConsigneeInHouseOrItem flag is false", wrapper.Consignee);

					var goodsItem1 = nctsBill.GoodsItems.AddNew();
					goodsItem1.Consignee.E2_OA_Address = orgAddress2.PK;
					wrapper = GetWrapper(nctsBill, shouldDeclareConsigneeInHouseOrItem: true);
					AssertNotNull("FinalPeriod: Expected filled Consignee when shouldDeclareConsigneeInHouseOrItem flag is true and Consignee is the same", wrapper.Consignee);
					AssertEquals("FinalPeriod: Expected Consignee from Bill when shouldDeclareConsigneeInHouseOrItem flag is true", "Name2", wrapper.Consignee.Name);

					goodsItem1.Consignee.E2_OA_Address = orgAddress1.PK;
					wrapper = GetWrapper(nctsBill, shouldDeclareConsigneeInHouseOrItem: true);
					AssertNull("FinalPeriod: Expected not filled Consignee when shouldDeclareConsigneeInHouseOrItem flag is true but Consignee is not the same", wrapper.Consignee);

					nctsBill.Consignee.E2_OA_Address = ZGuid.Empty;
					goodsItem1.Consignee.E2_OA_Address = ZGuid.Empty;
					wrapper = GetWrapper(nctsBill, shouldDeclareConsigneeInHouseOrItem: true);
					AssertNotNull("FinalPeriod: Expected filled Consignee from Header when shouldDeclareConsigneeInHouseOrItem flag is true and Bill Consignee is Empty", wrapper.Consignee);
					AssertEquals("FinalPeriod: Expected Consignee from Header when shouldDeclareConsigneeInHouseOrItem flag is true and Bill Consignee is Empty", "Name1", wrapper.Consignee.Name);
				}
			});
		}

		public void TestConsigneeInLines()
		{
			CombineAssertions(() =>
			{
				var orgAddress1 = Factory.New<OrgAddress>();
				var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader1.OH_FullName = "Name1";
				orgAddress1.OA_OH = orgHeader1.PK;
				nctsBill.Consignee.E2_OA_Address = orgAddress1.PK;

				var orgAddress2 = Factory.New<OrgAddress>();
				var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader2.OH_FullName = "Name2";
				orgAddress2.OA_OH = orgHeader2.PK;

				var orgAddress3 = Factory.New<OrgAddress>();
				var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader3.OH_FullName = "Name3";
				orgAddress3.OA_OH = orgHeader3.PK;

				var goodsItem1 = nctsBill.GoodsItems.AddNew();
				goodsItem1.Consignee.E2_OA_Address = orgAddress2.PK;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					AssertNull("TransitionalPeriod: Expected empty Consignee when shouldDeclareConsigneeInHouseOrItem flag is false", wrapper.ConsignmentItem.ToArray()[0].Consignee);

					wrapper = GetWrapper(nctsBill, shouldDeclareConsigneeInHouseOrItem: true);
					AssertEquals("TransitionalPeriod: Expected filled Consignee when shouldDeclareConsigneeInHouseOrItem flag is true and there is only one goodsItem and code different than in bill", "Name2", wrapper.ConsignmentItem.ToArray()[0].Consignee.Name);

					goodsItem1.Consignee.E2_OA_Address = orgAddress1.PK;
					wrapper = GetWrapper(nctsBill, shouldDeclareConsigneeInHouseOrItem: true);
					AssertNull("TransitionalPeriod: Expected empty Consignee when shouldDeclareConsigneeInHouseOrItem flag is true and there is only one goodsItem and code the same as in bill", wrapper.ConsignmentItem.ToArray()[0].Consignee);

					var goodsItem2 = nctsBill.GoodsItems.AddNew();
					goodsItem2.Consignee.E2_OA_Address = orgAddress3.PK;
					wrapper = GetWrapper(nctsBill, shouldDeclareConsigneeInHouseOrItem: true);
					AssertContainsExactElementsInAnyOrder("TransitionalPeriod: Expected filled Consignee when shouldDeclareConsigneeInHouseOrItem flag is true and there are multiple goodsItems at least one different code than in bill", new ZString[] { "Name1", "Name3" }, wrapper.ConsignmentItem.Select(x => x.Consignee.Name).ToArray());

					goodsItem2.Consignee.E2_OA_Address = ZGuid.Empty;
					wrapper = GetWrapper(nctsBill, shouldDeclareConsigneeInHouseOrItem: true);
					var consignmentItems = wrapper.ConsignmentItem.ToList();
					AssertNull("TransitionalPeriod: Expected empty Consignee when shouldDeclareConsigneeInHouseOrItem flag is true and there are multiple goodsItems where values are empty or the same as in bill, first item", consignmentItems[0].Consignee);
					AssertNull("TransitionalPeriod: Expected empty Consignee when shouldDeclareConsigneeInHouseOrItem flag is true and there are multiple goodsItems where values are empty or the same as in bill, second item", consignmentItems[1].Consignee);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapper = GetWrapper(nctsBill, shouldDeclareConsigneeInHouseOrItem: true);
					AssertNull("FinalPeriod: Expected not filled Consignee when shouldDeclareConsigneeInHouseOrItem flag is true but is final period", wrapper.ConsignmentItem.ToArray()[0].Consignee);
				}
			});
		}

		public void TestDepartureTransportMeans()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: false);
					AssertEquals("FinalPeriod: Expected empty DepartureTransportMeans when shouldDeclareTransportMeansInHouse flag is true", 0, wrapper.DepartureTransportMeans.Count);

					var departureMovement = nctsHeader.MovementHeader;
					departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._1_SeaTransport;
					departureMovement.TransportTypeAtDeparture = "12";
					departureMovement.VesselNameAtDeparture = "Vessel2";
					departureMovement.VesselCountryAtDeparture = "FR";

					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: true);
					var departureTransportMeans = wrapper.DepartureTransportMeans;

					AssertEquals("Expected filled DepartureTransportMeans with 1 element when inland mode is not 3", 1, departureTransportMeans.Count);
					AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

					var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
					AssertEquals("Expected filled with Consigment data when House empty DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
					AssertEquals("Expected filled with Consigment data when House empty DepartureTransportMeans[0].TransportMode", "12", departureTransportMeansElement.TransportMode);
					AssertEquals("Expected filled with Consigment data when House empty DepartureTransportMeans[0].TransportId", "Vessel2", departureTransportMeansElement.TransportId);
					AssertEquals("Expected filled with Consigment data when House empty DepartureTransportMeans[0].TransportNationality", "FR", departureTransportMeansElement.TransportNationality);

					nctsBill.TransportTypeAtDeparture = "11";
					nctsBill.VesselNameAtDeparture = "Vessel";
					nctsBill.VesselCountryAtDeparture = "ES";
					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: false);
					AssertEquals("FinalPeriod: Expected empty DepartureTransportMeans  when shouldDeclareTransportMeansInHouse flag is false", 0, wrapper.DepartureTransportMeans.Count);

					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: true);
					departureTransportMeans = wrapper.DepartureTransportMeans;

					AssertEquals("Expected filled DepartureTransportMeans with 1 element when inland mode is not 3", 1, departureTransportMeans.Count);
					AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

					departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
					AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "11", departureTransportMeansElement.TransportMode);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "Vessel", departureTransportMeansElement.TransportId);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					nctsHeader.MovementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._1_SeaTransport;
					nctsBill.TransportTypeAtDeparture = "11";
					nctsBill.VesselNameAtDeparture = "Vessel";
					nctsBill.VesselCountryAtDeparture = "ES";
					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: true);

					AssertEquals("TransitionalPeriod: Expected empty DepartureTransportMeans when data declared", 0, wrapper.DepartureTransportMeans.Count);
				}
			});
		}

		public void TestDepartureTransportMeans_MOT1()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					nctsHeader.MovementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._1_SeaTransport;
					nctsBill.TransportTypeAtDeparture = "10";
					nctsBill.VesselNameAtDeparture = "Vessel";
					nctsBill.VesselCountryAtDeparture = "ES";
					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: true);
					var departureTransportMeans = wrapper.DepartureTransportMeans;
					AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);
					AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);
					var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
					AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "10", departureTransportMeansElement.TransportMode);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "Vessel", departureTransportMeansElement.TransportId);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
				}
			});
		}

		public void TestDepartureTransportMeans_MOT2()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					nctsHeader.MovementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._2_RailTransport;
					nctsBill.TransportTypeAtDeparture = "20";
					nctsBill.TransportAtDeparture = "wagon";
					nctsBill.TransportCountryAtDeparture = "GB";
					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: true);
					var departureTransportMeans = wrapper.DepartureTransportMeans;
					AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);
					AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);
					var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
					AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "20", departureTransportMeansElement.TransportMode);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "wagon", departureTransportMeansElement.TransportId);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "GB", departureTransportMeansElement.TransportNationality);
				}
			});
		}

		public void TestDepartureTransportMeans_MOT3()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					nctsHeader.MovementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._3_RoadTransport;
					nctsBill.Trailer2IDAtDeparture = "trailer2";
					nctsBill.Trailer2NationalityAtDeparture = "ES";
					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: true);
					var departureTransportMeans = wrapper.DepartureTransportMeans;
					AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);
					AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);
					var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
					AssertEquals("When only trailer 2 ID declared expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
					AssertEquals("When only trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportMode", "31", departureTransportMeansElement.TransportMode);
					AssertEquals("When only trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportId", "trailer2", departureTransportMeansElement.TransportId);
					AssertEquals("When only trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);

					nctsBill.Trailer1IDAtDeparture = "trailer1";
					nctsBill.Trailer1NationalityAtDeparture = "FR";
					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: true);
					departureTransportMeans = wrapper.DepartureTransportMeans;
					AssertEquals("Expected filled DepartureTransportMeans with 2 elements", 2, departureTransportMeans.Count);
					var departureTransportMeansArray = departureTransportMeans.ToArray();
					AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansArray[0].SequenceNumber);
					AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportMode", "31", departureTransportMeansArray[0].TransportMode);
					AssertEquals("When trailer 1 and trailer 2 ID declared expected filled with trailer 1 ID DepartureTransportMeans[0].TransportId", "trailer1", departureTransportMeansArray[0].TransportId);
					AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportNationality", "FR", departureTransportMeansArray[0].TransportNationality);

					AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].SequenceNumber", "2", departureTransportMeansArray[1].SequenceNumber);
					AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].TransportMode", "31", departureTransportMeansArray[1].TransportMode);
					AssertEquals("When trailer 1 and trailer 2 ID declared expected filled with trailer 2 ID DepartureTransportMeans[1].TransportId", "trailer2", departureTransportMeansArray[1].TransportId);
					AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].TransportNationality", "ES", departureTransportMeansArray[1].TransportNationality);

					nctsBill.TransportAtDeparture = "transportID";
					nctsBill.TransportCountryAtDeparture = "GB";
					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: true);
					departureTransportMeans = wrapper.DepartureTransportMeans;
					AssertEquals("Expected filled DepartureTransportMeans with 3 elements", 3, departureTransportMeans.Count);
					departureTransportMeansArray = departureTransportMeans.ToArray();
					AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansArray[0].SequenceNumber);
					AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportMode", "30", departureTransportMeansArray[0].TransportMode);
					AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled with transport ID DepartureTransportMeans[0].TransportId", "transportID", departureTransportMeansArray[0].TransportId);
					AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportNationality", "GB", departureTransportMeansArray[0].TransportNationality);

					AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].SequenceNumber", "2", departureTransportMeansArray[1].SequenceNumber);
					AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].TransportMode", "31", departureTransportMeansArray[1].TransportMode);
					AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled with trailer 1 ID DepartureTransportMeans[1].TransportId", "trailer1", departureTransportMeansArray[1].TransportId);
					AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].TransportNationality", "FR", departureTransportMeansArray[1].TransportNationality);

					AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[2].SequenceNumber", "3", departureTransportMeansArray[2].SequenceNumber);
					AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[2].TransportMode", "31", departureTransportMeansArray[2].TransportMode);
					AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled with trailer 2 ID DepartureTransportMeans[2].TransportId", "trailer2", departureTransportMeansArray[2].TransportId);
					AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[2].TransportNationality", "ES", departureTransportMeansArray[2].TransportNationality);
				}
			});
		}

		public void TestDepartureTransportMeans_MOT4()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					nctsHeader.MovementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._4_AirTransport;
					nctsBill.TransportTypeAtDeparture = "40";
					nctsBill.TransportAtDeparture = "aircraftID";
					nctsBill.TransportCountryAtDeparture = "ES";
					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: true);
					var departureTransportMeans = wrapper.DepartureTransportMeans;
					AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);
					AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);
					var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
					AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "40", departureTransportMeansElement.TransportMode);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "aircraftID", departureTransportMeansElement.TransportId);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
				}
			});
		}

		public void TestDepartureTransportMeans_MOT5()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					nctsHeader.MovementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._5_PostalConsignment;
					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: true);
					AssertEquals("Expected empty DepartureTransportMeans when MOT is 5", 0, wrapper.DepartureTransportMeans.Count);
				}
			});
		}

		public void TestDepartureTransportMeans_MOT7()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					nctsHeader.MovementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._7_FixedTransportInstallations;
					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: true);
					AssertEquals("Expected empty DepartureTransportMeans when MOT is 7", 0, wrapper.DepartureTransportMeans.Count);
				}
			});
		}

		public void TestDepartureTransportMeans_MOT8()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					nctsHeader.MovementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
					nctsBill.TransportTypeAtDeparture = "80";
					nctsBill.TransportAtDeparture = "Vessel";
					nctsBill.TransportCountryAtDeparture = "ES";
					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: true);
					var departureTransportMeans = wrapper.DepartureTransportMeans;
					AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);
					AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);
					var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
					AssertEquals("When Vessel has no Lloyds/IMO number expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
					AssertEquals("When Vessel has no Lloyds/IMO number expected filled DepartureTransportMeans[0].TransportMode", "80", departureTransportMeansElement.TransportMode);
					AssertEquals("When Vessel has no Lloyds/IMO number expected filled DepartureTransportMeans[0].TransportId", "Vessel", departureTransportMeansElement.TransportId);
					AssertEquals("When Vessel has no Lloyds/IMO number expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
				}
			});
		}

		public void TestDepartureTransportMeans_MOT9()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					nctsHeader.MovementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._9_OwnPropulsion;
					nctsBill.TransportTypeAtDeparture = "81";
					nctsBill.TransportAtDeparture = "transportID";
					nctsBill.TransportCountryAtDeparture = "ES";
					wrapper = GetWrapper(nctsBill, shouldDeclareTransportMeansInHouse: true);
					var departureTransportMeans = wrapper.DepartureTransportMeans;
					AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);
					AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);
					var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
					AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "81", departureTransportMeansElement.TransportMode);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "transportID", departureTransportMeansElement.TransportId);
					AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
				}
			});
		}

		public void TestPreviousDocument()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					AssertEquals("FinalPeriod: Expected empty PreviousDocument list", 0, wrapper.PreviousDocument.Count);

					var prevdoc1 = nctsBill.PreviousDocuments.AddNew();
					prevdoc1.CSI_Code = "9001";

					var prevdoc2 = nctsBill.PreviousDocuments.AddNew();
					prevdoc2.CSI_Code = "9002";

					var prevdoc3 = nctsHeader.PreviousDocuments.AddNew();
					prevdoc3.CSI_Code = "9003";

					var prevdoc4 = nctsBill.GoodsItems.AddNew().PreviousDocuments.AddNew();
					prevdoc4.CSI_Code = "9004";

					wrapper = GetWrapper(nctsBill);
					var documents = wrapper.PreviousDocument;

					AssertEquals("FinalPeriod: Expected filled PreviousDocument with only 1", 1, documents.Count);
					AssertSame("FinalPeriod: Cached PreviousDocument", wrapper.PreviousDocument, documents);

					wrapper = GetWrapper(nctsBill, messageType: DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration);
					documents = wrapper.PreviousDocument;
					AssertEquals("FinalPeriod: Expected not filled PreviousDocument", 0, documents.Count);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					AssertEquals("TransitionalPeriod: Expected not filled PreviousDocument", 0, wrapper.PreviousDocument.Count);

					wrapper = GetWrapper(nctsBill, messageType: DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration);
					var documents = wrapper.PreviousDocument;
					AssertEquals("TransitionalPeriod: Expected not filled PreviousDocument", 0, documents.Count);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsBill = nctsHeader.Bills.AddNew();
			wrapper = GetWrapper(nctsBill);
		}

		NctsHeader nctsHeader;
		NctsBill nctsBill;
		NCTS5CommonHouseConsignmentDepartureAndAmendmentWrapper wrapper;

		NCTS5CommonHouseConsignmentDepartureAndAmendmentWrapper GetWrapper(NctsBill bill, bool shouldDeclareCountryOfDispatchInHouseOrItem = false, bool shouldDeclareCountryOfDestinationInHouseOrItem = false, bool shouldDeclareReferenceNumberUCRInItem = false, bool shouldDeclareConsigneeInHouseOrItem = false, bool shouldDeclareConsignorInHouse = false, bool shouldDeclareTransportMeansInHouse = false, string messageType = "") => new NCTS5CommonHouseConsignmentDepartureAndAmendmentWrapper(bill, shouldDeclareCountryOfDispatchInHouseOrItem, shouldDeclareCountryOfDestinationInHouseOrItem, shouldDeclareReferenceNumberUCRInItem, shouldDeclareConsigneeInHouseOrItem, shouldDeclareConsignorInHouse, shouldDeclareTransportMeansInHouse, messageType);

		protected override NCTS5CommonHouseConsignmentDepartureAndAmendmentWrapper GetProvider() => wrapper;
	}
}
