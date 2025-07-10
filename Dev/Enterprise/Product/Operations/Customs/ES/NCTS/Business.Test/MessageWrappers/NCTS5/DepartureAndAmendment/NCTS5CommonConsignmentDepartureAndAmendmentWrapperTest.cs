using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonConsignmentDepartureAndAmendmentWrapperTest : WrapperHelperTest<NCTS5CommonConsignmentDepartureAndAmendmentWrapper>
	{
		public void TestCommonConsignmentData()
		{
			CombineAssertions(() =>
			{
				var commonConsignmentData = wrapper.CommonConsignmentData;
				AssertNotNull("Expected filled CommonConsignmentData", commonConsignmentData);
				AssertSame("Cached CommonConsignmentData", wrapper.CommonConsignmentData, commonConsignmentData);
			});
		}

		public void TestCountryOfDispatch()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;

					departureMovement.BM_RN_NKCountryOfDispatch = "ES";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDispatch in Consignment when there are no goodsItems", "ES", wrapper.CommonConsignmentData.CountryOfDispatch);

					var nctsBill = nctsHeader.Bills.AddNew();
					var goodsItem1 = nctsBill.GoodsItems.AddNew();
					goodsItem1.BY_RN_NKCountryOfDispatch = "FR";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDispatch in Consignment when there is only one goodsItem and code different than in consignment", ZString.Empty, wrapper.CommonConsignmentData.CountryOfDispatch);
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDispatch in ConsignmentItem when there is only one goodsItem and code different than in consignment", "FR", wrapper.HouseConsignment.First().ConsignmentItem.First().CountryOfDispatch);

					goodsItem1.BY_RN_NKCountryOfDispatch = "ES";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDispatch in Consignment when there is only one goodsItem and code the same as in consignment", "ES", wrapper.CommonConsignmentData.CountryOfDispatch);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDispatch in ConsignmentItem when there is only one goodsItem and code the same as in consignment", ZString.Empty, wrapper.HouseConsignment.First().ConsignmentItem.First().CountryOfDispatch);

					nctsBill.B0_RN_NKCountryOfExport = "IT";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDispatch in House when there is only one goodsItem and code the same as in consignment but house consignment has code different than in consignment", ZString.Empty, wrapper.HouseConsignment.First().CountryOfDispatch);

					nctsBill.B0_RN_NKCountryOfExport = "FR";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDispatch in Consignment when there is only one goodsItem and code the same as in consignment but house consignment has code different than in consignment", ZString.Empty, wrapper.CommonConsignmentData.CountryOfDispatch);
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDispatch in ConsignmentItem when there is only one goodsItem and code the same as in consignment but house consignment has code different than in consignment", "ES", wrapper.HouseConsignment.First().ConsignmentItem.First().CountryOfDispatch);

					nctsBill.B0_RN_NKCountryOfExport = "ES";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDispatch in Consignment when there is only one goodsItem and code the same as in consignment and house consignment has code the same as in consignment", "ES", wrapper.CommonConsignmentData.CountryOfDispatch);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDispatch in ConsignmentItem when there is only one goodsItem and code the same as in consignment and house consignment has code the same as in consignment", ZString.Empty, wrapper.HouseConsignment.First().ConsignmentItem.First().CountryOfDispatch);

					var goodsItem2 = nctsBill.GoodsItems.AddNew();
					goodsItem2.BY_RN_NKCountryOfDispatch = "DE";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDispatch in Consignment when there are multiple goodsItems at least one different code than in consignment", ZString.Empty, wrapper.CommonConsignmentData.CountryOfDispatch);
					AssertContainsExactElementsInAnyOrder("TransitionalPeriod: Expected filled CountryOfDispatch in ConsignmentItem when there are multiple goodsItems at least one different code than in consignment", new ZString[] { "ES", "DE" }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.CountryOfDispatch).ToArray());

					goodsItem2.BY_RN_NKCountryOfDispatch = "ES";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDispatch in Consignment when there are multiple goodsItems with same codes as in consignment", "ES", wrapper.CommonConsignmentData.CountryOfDispatch);
					AssertContainsExactElementsInAnyOrder("TransitionalPeriod: Expected empty CountryOfDispatch in ConsignmentItem when there are multiple goodsItems with same codes as in consignment", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.CountryOfDispatch).ToArray());

					nctsBill.B0_RN_NKCountryOfExport = "FR";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDispatch in Consignment when there are multiple goodsItems with same codes as in consignment but house consignment has code different than in consignment", ZString.Empty, wrapper.CommonConsignmentData.CountryOfDispatch);
					AssertContainsExactElementsInAnyOrder("TransitionalPeriod: Expected filled CountryOfDispatch in ConsignmentItem when there are multiple goodsItems with same codes as in consignment but house consignment has code different than in consignment", new ZString[] { "ES", "ES" }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.CountryOfDispatch).ToArray());

					nctsBill.B0_RN_NKCountryOfExport = "ES";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDispatch in Consignment when there are multiple goodsItems with same codes as in consignment and house consignment has code the same as in consignment", "ES", wrapper.CommonConsignmentData.CountryOfDispatch);
					AssertContainsExactElementsInAnyOrder("TransitionalPeriod: Expected empty CountryOfDispatch in ConsignmentItem when there are multiple goodsItems with same codes as in consignment and house consignment has code the same as in consignment", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.CountryOfDispatch).ToArray());

					departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;

					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDispatch in Consignment when there are multiple goodsItems with same codes but BM_InBondEntryType is not TIR", ZString.Empty, wrapper.CommonConsignmentData.CountryOfDispatch);

					goodsItem2.BY_RN_NKCountryOfDispatch = "DE";
					wrapper = GetWrapper(nctsHeader);
					AssertContainsExactElementsInAnyOrder("TransitionalPeriod: Expected empty CountryOfDispatch in ConsignmentItem when there are multiple goodsItems at least one different code but BM_InBondEntryType is not TIR", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.CountryOfDispatch).ToArray());
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					nctsHeader.Bills.DeleteAll();
					departureMovement.BM_RN_NKCountryOfDispatch = "ES";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected filled CountryOfDispatch in Consignment when there are no goodsItems or Bills", "ES", wrapper.CommonConsignmentData.CountryOfDispatch);

					var nctsBill = nctsHeader.Bills.AddNew();
					nctsBill.B0_RN_NKCountryOfExport = "ES";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected filled CountryOfDispatch in Consignment when there are no goodsItems and is the same as in Bills", "ES", wrapper.CommonConsignmentData.CountryOfDispatch);
					AssertEquals("FinalPeriod: Expected empty CountryOfDispatch in House when code is the same as in consignment", ZString.Empty, wrapper.HouseConsignment.First().CountryOfDispatch);

					nctsBill.B0_RN_NKCountryOfExport = "FR";
					var goodsItem1 = nctsBill.GoodsItems.AddNew();
					goodsItem1.BY_RN_NKCountryOfDispatch = "FR";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected empty CountryOfDispatch in Consignment when there are no goodsItems but is not the same as in Bills", ZString.Empty, wrapper.CommonConsignmentData.CountryOfDispatch);
					AssertEquals("FinalPeriod: Expected filled CountryOfDispatch in House when code is the not same as in consignment but same as in goodItem", "FR", wrapper.HouseConsignment.First().CountryOfDispatch);

					goodsItem1.BY_RN_NKCountryOfDispatch = "IT";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected empty CountryOfDispatch in Consignment when code is not the same as in goodsItems", ZString.Empty, wrapper.CommonConsignmentData.CountryOfDispatch);
					AssertEquals("FinalPeriod: Expected empty CountryOfDispatch in House when code is the not same as in goodItem", ZString.Empty, wrapper.HouseConsignment.First().CountryOfDispatch);
					AssertEquals("FinalPeriod: Expected filled CountryOfDispatch in House when code is the not same as in consignment and as in House and is not TIR", "IT", wrapper.HouseConsignment.First().ConsignmentItem.First().CountryOfDispatch);
				}
			});
		}

		public void TestCountryOfDestination()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					departureMovement.BM_RL_NKDestinationPort = "ES";
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDestination in Consignment when there are no goodsItems", "ES", wrapper.CommonConsignmentData.CountryOfDestination);

					var nctsBill = nctsHeader.Bills.AddNew();
					var goodsItem1 = nctsBill.GoodsItems.AddNew();
					goodsItem1.BY_RN_NKCountryOfDestination = "FR";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDestination in Consignment when there is only one goodsItem and code different than in consignment", ZString.Empty, wrapper.CommonConsignmentData.CountryOfDestination);
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDestination in ConsignmentItem when there is only one goodsItem and code different than in consignment", "FR", wrapper.HouseConsignment.First().ConsignmentItem.First().CountryOfDestination);

					goodsItem1.BY_RN_NKCountryOfDestination = "ES";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDestination in Consignment when there is only one goodsItem and code the same as in consignment", "ES", wrapper.CommonConsignmentData.CountryOfDestination);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDestination in ConsignmentItem when there is only one goodsItem and code the same as in consignment", ZString.Empty, wrapper.HouseConsignment.First().ConsignmentItem.First().CountryOfDestination);

					nctsBill.B0_RN_NKCountryOfExport = "IT";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDestination in House when there is only one goodsItem and code the same as in consignment but house consignment has code different than in consignment", ZString.Empty, wrapper.HouseConsignment.First().CountryOfDestination);

					nctsBill.B0_RN_NKCountryOfDestination = "FR";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDestination in Consignment when there is only one goodsItem and code the same as in consignment but house consignment has code different than in consignment", ZString.Empty, wrapper.CommonConsignmentData.CountryOfDestination);
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDestination in ConsignmentItem when there is only one goodsItem and code the same as in consignment but house consignment has code different than in consignment", "ES", wrapper.HouseConsignment.First().ConsignmentItem.First().CountryOfDestination);

					nctsBill.B0_RN_NKCountryOfDestination = "ES";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDestination in Consignment when there is only one goodsItem and code the same as in consignment and house consignment has code the same as in consignment", "ES", wrapper.CommonConsignmentData.CountryOfDestination);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDestination in ConsignmentItem when there is only one goodsItem and code the same as in consignment and house consignment has code the same as in consignment", ZString.Empty, wrapper.HouseConsignment.First().ConsignmentItem.First().CountryOfDestination);

					var goodsItem2 = nctsBill.GoodsItems.AddNew();
					goodsItem2.BY_RN_NKCountryOfDestination = "DE";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDestination in Consignment when there are multiple goodsItems with at least one different code than in consignment", ZString.Empty, wrapper.CommonConsignmentData.CountryOfDestination);
					AssertContainsExactElementsInAnyOrder("TransitionalPeriod: Expected filled CountryOfDestination in ConsignmentItem when there are multiple goodsItems at least one different code than in consignment", new ZString[] { "ES", "DE" }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.CountryOfDestination).ToArray());

					goodsItem2.BY_RN_NKCountryOfDestination = "ES";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDestination in Consignment when there are multiple goodsItems with same codes as in consignment", "ES", wrapper.CommonConsignmentData.CountryOfDestination);
					AssertContainsExactElementsInAnyOrder("TransitionalPeriod: Expected empty CountryOfDestination in ConsignmentItem when there are multiple goodsItems with same codes as in consignment", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.CountryOfDestination).ToArray());

					nctsBill.B0_RN_NKCountryOfDestination = "FR";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty CountryOfDestination in Consignment when there are multiple goodsItems with same codes as in consignment but house consignment has code different than in consignment", ZString.Empty, wrapper.CommonConsignmentData.CountryOfDestination);
					AssertContainsExactElementsInAnyOrder("TransitionalPeriod: Expected filled CountryOfDestination in ConsignmentItem when there are multiple goodsItems with same codes as in consignment but house consignment has code different than in consignment", new ZString[] { "ES", "ES" }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.CountryOfDestination).ToArray());

					nctsBill.B0_RN_NKCountryOfDestination = "ES";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected filled CountryOfDestination in Consignment when there are multiple goodsItems with same codes as in consignment and house consignment has code the same as in consignment", "ES", wrapper.CommonConsignmentData.CountryOfDestination);
					AssertContainsExactElementsInAnyOrder("TransitionalPeriod: Expected empty CountryOfDestination in ConsignmentItem when there are multiple goodsItems with same codes as in consignment and house consignment has code the same as in consignment", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.CountryOfDestination).ToArray());
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					nctsHeader.Bills.DeleteAll();
					departureMovement.BM_RL_NKDestinationPort = "ES";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected filled CountryOfDestination in Consignment when there are no goodsItems or Bills", "ES", wrapper.CommonConsignmentData.CountryOfDestination);

					var nctsBill = nctsHeader.Bills.AddNew();
					nctsBill.B0_RN_NKCountryOfDestination = "ES";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected filled CountryOfDestination in Consignment when there are no goodsItems and is the same as in Bills", "ES", wrapper.CommonConsignmentData.CountryOfDestination);
					AssertEquals("FinalPeriod: Expected empty CountryOfDestination in House when code is the same as in consignment", ZString.Empty, wrapper.HouseConsignment.First().CountryOfDestination);

					nctsBill.B0_RN_NKCountryOfDestination = "FR";
					var goodsItem1 = nctsBill.GoodsItems.AddNew();
					goodsItem1.BY_RN_NKCountryOfDestination = "FR";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected empty CountryOfDestination in Consignment when there are no goodsItems but is not the same as in Bills", ZString.Empty, wrapper.CommonConsignmentData.CountryOfDestination);
					AssertEquals("FinalPeriod: Expected filled CountryOfDestination in House when code is the not same as in consignment but same as in goodItem", "FR", wrapper.HouseConsignment.First().CountryOfDestination);

					goodsItem1.BY_RN_NKCountryOfDestination = "IT";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected empty CountryOfDestination in Consignment when code is not the same as in goodsItems", ZString.Empty, wrapper.CommonConsignmentData.CountryOfDestination);
					AssertEquals("FinalPeriod: Expected empty CountryOfDestination in House when code is the not same as in goodItem", ZString.Empty, wrapper.HouseConsignment.First().CountryOfDestination);
					AssertEquals("FinalPeriod: Expected filled CountryOfDestination in House when code is the not same as in consignment and as in House", "IT", wrapper.HouseConsignment.First().ConsignmentItem.First().CountryOfDestination);
				}
			});
		}

		public void TestReferenceNumberUCR()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					departureMovement.BM_UniqueConsignmentReference = "reference";
					AssertEquals("TransitionalPeriod: Expected filled ReferenceNumberUCR in Consignment when there are no goodsItems", "reference", wrapper.CommonConsignmentData.ReferenceNumberUCR);

					var nctsBill = nctsHeader.Bills.AddNew();
					var goodsItem1 = nctsBill.GoodsItems.AddNew();
					goodsItem1.BY_CommercialReferenceNumber = "reference1";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty ReferenceNumberUCR in Consignment when there is only one goodsItem and code different than in consignment", ZString.Empty, wrapper.CommonConsignmentData.ReferenceNumberUCR);
					AssertEquals("TransitionalPeriod: Expected filled ReferenceNumberUCR in ConsignmentItem when there is only one goodsItem and code different than in consignment", "reference1", wrapper.HouseConsignment.First().ConsignmentItem.First().ReferenceNumberUCR);

					goodsItem1.BY_CommercialReferenceNumber = "reference";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected filled ReferenceNumberUCR in Consignment when there is only one goodsItem and code the same as in consignment", "reference", wrapper.CommonConsignmentData.ReferenceNumberUCR);
					AssertEquals("TransitionalPeriod: Expected empty ReferenceNumberUCR in ConsignmentItem when there is only one goodsItem and code the same as in consignment", ZString.Empty, wrapper.HouseConsignment.First().ConsignmentItem.First().ReferenceNumberUCR);

					nctsBill.B0_ReferenceID = "reference2";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty ReferenceNumberUCR in House when there is only one goodsItem and code the same as in consignment but house consignment has code different than in consignment", ZString.Empty, wrapper.HouseConsignment.First().ReferenceNumberUCR);

					nctsBill.B0_ReferenceID = "reference1";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty ReferenceNumberUCR in Consignment when there is only one goodsItem and code the same as in consignment but house consignment has code different than in consignment", ZString.Empty, wrapper.CommonConsignmentData.ReferenceNumberUCR);
					AssertEquals("TransitionalPeriod: Expected filled ReferenceNumberUCR in ConsignmentItem when there is only one goodsItem and code the same as in consignment but house consignment has code different than in consignment", "reference", wrapper.HouseConsignment.First().ConsignmentItem.First().ReferenceNumberUCR);

					nctsBill.B0_ReferenceID = "reference";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected filled ReferenceNumberUCR in Consignment when there is only one goodsItem and code the same as in consignment and house consignment has code the same as in consignment", "reference", wrapper.CommonConsignmentData.ReferenceNumberUCR);
					AssertEquals("TransitionalPeriod: Expected empty ReferenceNumberUCR in ConsignmentItem when there is only one goodsItem and code the same as in consignment and house consignment has code the same as in consignment", ZString.Empty, wrapper.HouseConsignment.First().ConsignmentItem.First().ReferenceNumberUCR);

					var goodsItem2 = nctsBill.GoodsItems.AddNew();
					goodsItem2.BY_CommercialReferenceNumber = "reference2";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty ReferenceNumberUCR in Consignment when there are multiple goodsItems at least one different code than in consignment", ZString.Empty, wrapper.CommonConsignmentData.ReferenceNumberUCR);
					AssertContainsExactElementsInAnyOrder("TransitionalPeriod: Expected filled ReferenceNumberUCR in ConsignmentItem when there are multiple goodsItems at least one different code than in consignment", new ZString[] { "reference", "reference2" }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.ReferenceNumberUCR).ToArray());

					goodsItem2.BY_CommercialReferenceNumber = "reference";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected filled ReferenceNumberUCR in Consignment when there are multiple goodsItems with same codes as in consignment", "reference", wrapper.CommonConsignmentData.ReferenceNumberUCR);
					AssertContainsExactElementsInAnyOrder("TransitionalPeriod: Expected empty ReferenceNumberUCR in ConsignmentItem when there are multiple goodsItems with same codes as in consignment", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.ReferenceNumberUCR).ToArray());

					nctsBill.B0_ReferenceID = "reference1";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected empty ReferenceNumberUCR in Consignment when there are multiple goodsItems with same codes as in consignment but house consignment has code different than in consignment", ZString.Empty, wrapper.CommonConsignmentData.ReferenceNumberUCR);
					AssertContainsExactElementsInAnyOrder("TransitionalPeriod: Expected filled ReferenceNumberUCR in ConsignmentItem when there are multiple goodsItems with same codes as in consignment but house consignment has code different than in consignment", new ZString[] { "reference", "reference" }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.ReferenceNumberUCR).ToArray());

					nctsBill.B0_ReferenceID = "reference";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected filled ReferenceNumberUCR in Consignment when there are multiple goodsItems with same codes as in consignment and house consignment has code the same as in consignment", "reference", wrapper.CommonConsignmentData.ReferenceNumberUCR);
					AssertContainsExactElementsInAnyOrder("TransitionalPeriod: Expected empty ReferenceNumberUCR in ConsignmentItem when there are multiple goodsItems with same codes as in consignment and house consignment has code the same as in consignment", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.ReferenceNumberUCR).ToArray());
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					nctsHeader.Bills.DeleteAll();
					departureMovement.BM_UniqueConsignmentReference = "reference";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected filled ReferenceNumberUCR in Consignment when there are no goodsItems or Bills", "reference", wrapper.CommonConsignmentData.ReferenceNumberUCR);

					var nctsBill = nctsHeader.Bills.AddNew();
					nctsBill.B0_ReferenceID = "reference";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected filled ReferenceNumberUCR in Consignment when there are no goodsItems and is the same as in Bills", "reference", wrapper.CommonConsignmentData.ReferenceNumberUCR);
					AssertEquals("FinalPeriod: Expected empty ReferenceNumberUCR in House when code is the same as in consignment", ZString.Empty, wrapper.HouseConsignment.First().ReferenceNumberUCR);

					nctsBill.B0_ReferenceID = "reference2";
					var goodsItem1 = nctsBill.GoodsItems.AddNew();
					goodsItem1.BY_CommercialReferenceNumber = "reference2";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected empty ReferenceNumberUCR in Consignment when there are no goodsItems but is not the same as in Bills", ZString.Empty, wrapper.CommonConsignmentData.ReferenceNumberUCR);
					AssertEquals("FinalPeriod: Expected filled ReferenceNumberUCR in House when code is the not same as in consignment but same as in goodItem", "reference2", wrapper.HouseConsignment.First().ReferenceNumberUCR);

					goodsItem1.BY_CommercialReferenceNumber = "reference3";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected empty ReferenceNumberUCR in Consignment when code is not the same as in goodsItems", ZString.Empty, wrapper.CommonConsignmentData.ReferenceNumberUCR);
					AssertEquals("FinalPeriod: Expected empty ReferenceNumberUCR in House when code is the not same as in goodItem", ZString.Empty, wrapper.HouseConsignment.First().ReferenceNumberUCR);
					AssertEquals("FinalPeriod: Expected filled ReferenceNumberUCR in House when code is the not same as in consignment and as in House", "reference3", wrapper.HouseConsignment.First().ConsignmentItem.First().ReferenceNumberUCR);
				}
			});
		}

		public void TestNullCarrier()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Carrier.ToString());
		}

		public void TestCarrier()
		{
			CombineAssertions(() =>
			{
				var orgAddress = Factory.New<OrgAddress>();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgAddress.OA_OH = orgHeader.PK;
				departureMovement.Carrier.E2_OA_Address = orgAddress.PK;

				var carrier = wrapper.Carrier;
				AssertNotNull("Expected filled Carrier", carrier);
				AssertSame("Cached Carrier", wrapper.Carrier, carrier);

				nctsHeader.Principal.E2_OA_Address = orgAddress.PK;
				wrapper = GetWrapper(nctsHeader);
				AssertNull("Expected empty Carrier when it is the same as the Principal", wrapper.Carrier);
			});
		}

		public void TestNullConsignor()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Consignor.ToString());
		}

		public void TestConsignor()
		{
			CombineAssertions(() =>
			{
				var orgAddress = Factory.New<OrgAddress>();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgAddress.OA_OH = orgHeader.PK;
				nctsHeader.Consignor.E2_OA_Address = orgAddress.PK;

				var orgAddress2 = Factory.New<OrgAddress>();
				var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader2.OH_FullName = "Name2";
				orgAddress2.OA_OH = orgHeader2.PK;
				var nctsBill = nctsHeader.Bills.AddNew();
				nctsBill.Consignor.E2_OA_Address = orgAddress2.PK;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					departureMovement.BM_TypeOfSecurity = "NON";
					wrapper = GetWrapper(nctsHeader);
					AssertNull("TransitionalPeriod: Expected empty Consignor when securityType is NON", wrapper.Consignor);

					departureMovement.BM_TypeOfSecurity = "EXI";
					wrapper = GetWrapper(nctsHeader);
					var consignor = wrapper.Consignor;
					AssertNotNull("TransitionalPeriod: Expected filled Consignor when securityType is EXI (not NON)", consignor);
					AssertNull("TransitionalPeriod: Expected empty Consignor for House", wrapper.HouseConsignment.First().Consignor);
					AssertSame("TransitionalPeriod: Cached Consignor", wrapper.Consignor, consignor);

					nctsHeader.Principal.E2_OA_Address = orgAddress.PK;
					wrapper = GetWrapper(nctsHeader);
					AssertNull("TransitionalPeriod: Expected empty Consignor when it is the same as the Principal", wrapper.Consignor);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					nctsHeader.Consignor.E2_OA_Address = orgAddress.PK;
					nctsBill.Consignor.E2_OA_Address = orgAddress.PK;
					nctsHeader.Principal.E2_OA_Address = ZGuid.Empty;
					departureMovement.BM_TypeOfSecurity = "EXI";
					wrapper = GetWrapper(nctsHeader);
					AssertNotNull("FinalPeriod: Expected Consignor when Bill Consignor is the same", wrapper.Consignor);
					AssertNull("FinalPeriod: Expected empty Consignor for House when Bill Consignor is the same", wrapper.HouseConsignment.First().Consignor);

					nctsBill.Consignor.E2_OA_Address = ZGuid.Empty;
					wrapper = GetWrapper(nctsHeader);
					AssertNotNull("FinalPeriod: Expected Consignor when Bill Consignor is empty", wrapper.Consignor);
					AssertNull("FinalPeriod: Expected empty Consignor for House when Bill Consignor is empty", wrapper.HouseConsignment.First().Consignor);

					nctsBill.Consignor.E2_OA_Address = orgAddress2.PK;
					wrapper = GetWrapper(nctsHeader);
					AssertNull("FinalPeriod: Expected empty Consignor when Bill Consignor is different", wrapper.Consignor);
					AssertNotNull("FinalPeriod: Expected Consignor for House when Bill Consignor is different", wrapper.HouseConsignment.First().Consignor);
				}
			});
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				var orgAddress = Factory.New<OrgAddress>();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgAddress.OA_OH = orgHeader.PK;
				nctsHeader.Consignee.E2_OA_Address = orgAddress.PK;

				var orgAddress2 = Factory.New<OrgAddress>();
				var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader2.OH_FullName = "Name2";
				orgAddress2.OA_OH = orgHeader2.PK;

				var orgAddress3 = Factory.New<OrgAddress>();
				var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader3.OH_FullName = "Name3";
				orgAddress3.OA_OH = orgHeader3.PK;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(nctsHeader);
					var consignee = wrapper.CommonConsignmentData.Consignee;
					AssertNotNull("TransitionalPeriod: Expected filled Consignee in Consignment when there are no goodsItems", consignee);
					AssertSame("TransitionalPeriod: Cached Consignee", wrapper.CommonConsignmentData.Consignee, consignee);

					var nctsBill = nctsHeader.Bills.AddNew();
					var goodsItem1 = nctsBill.GoodsItems.AddNew();

					var orgAddressItem1 = Factory.New<OrgAddress>();
					var orgHeaderItem1 = Factory.NewWithValidTestData<OrgHeader>();
					orgAddressItem1.OA_OH = orgHeaderItem1.PK;
					goodsItem1.Consignee.E2_OA_Address = orgAddressItem1.PK;
					wrapper = GetWrapper(nctsHeader);
					AssertNull("TransitionalPeriod: Expected empty Consignee in Consignment when there is only one goodsItem and code different than in consignment", wrapper.CommonConsignmentData.Consignee);
					AssertNotNull("TransitionalPeriod: Expected filled Consignee in ConsignmentItem when there is only one goodsItem and code different than in consignment", wrapper.HouseConsignment.First().ConsignmentItem.First().Consignee);

					goodsItem1.Consignee.E2_OA_Address = orgAddress.PK;
					wrapper = GetWrapper(nctsHeader);
					AssertNotNull("TransitionalPeriod: Expected filled Consignee in Consignment when there is only one goodsItem and code the same as in consignment", wrapper.CommonConsignmentData.Consignee);
					AssertNull("TransitionalPeriod: Expected empty Consignee in ConsignmentItem when there is only one goodsItem and code the same as in consignment", wrapper.HouseConsignment.First().ConsignmentItem.First().Consignee);

					nctsBill.Consignee.E2_OA_Address = orgAddressItem1.PK;
					wrapper = GetWrapper(nctsHeader);
					AssertNull("TransitionalPeriod: Expected empty Consignee in Consignment when there is only one goodsItem and code the same as in consignment but house consignment has code different than in consignment", wrapper.CommonConsignmentData.Consignee);
					AssertNotNull("TransitionalPeriod: Expected filled Consignee in ConsignmentItem when there is only one goodsItem and code the same as in consignment but house consignment has code different than in consignment", wrapper.HouseConsignment.First().ConsignmentItem.First().Consignee);

					nctsBill.Consignee.E2_OA_Address = orgAddress.PK;
					wrapper = GetWrapper(nctsHeader);
					AssertNotNull("TransitionalPeriod: Expected filled Consignee in Consignment when there is only one goodsItem and code the same as in consignment and house consignment has code the same as in consignment", wrapper.CommonConsignmentData.Consignee);
					AssertNull("TransitionalPeriod: Expected empty Consignee in ConsignmentItem when there is only one goodsItem and code the same as in consignment and house consignment has code the same as in consignment", wrapper.HouseConsignment.First().ConsignmentItem.First().Consignee);

					var goodsItem2 = nctsBill.GoodsItems.AddNew();
					var orgAddressItem2 = Factory.New<OrgAddress>();
					var orgHeaderItem2 = Factory.NewWithValidTestData<OrgHeader>();
					orgAddressItem2.OA_OH = orgHeaderItem2.PK;
					goodsItem2.Consignee.E2_OA_Address = orgAddressItem2.PK;
					wrapper = GetWrapper(nctsHeader);
					var consignmentItems = wrapper.HouseConsignment.First().ConsignmentItem.ToList();
					AssertNull("TransitionalPeriod: Expected empty Consignee in Consignment when there are multiple goodsItems at least one different code than in consignment", wrapper.CommonConsignmentData.Consignee);
					AssertNotNull("TransitionalPeriod: Expected filled Consignee in ConsignmentItem when there are multiple goodsItems with same codes, first item", consignmentItems[0].Consignee);
					AssertNotNull("TransitionalPeriod: Expected filled Consignee in ConsignmentItem when there are multiple goodsItems with same codes, second item", consignmentItems[1].Consignee);

					goodsItem2.Consignee.E2_OA_Address = orgAddress.PK;
					wrapper = GetWrapper(nctsHeader);
					consignmentItems = wrapper.HouseConsignment.First().ConsignmentItem.ToList();
					AssertNotNull("TransitionalPeriod: Expected filled Consignee in Consignment when there are multiple goodsItems with same codes as in consignment", wrapper.CommonConsignmentData.Consignee);
					AssertNull("TransitionalPeriod: Expected empty Consignee in ConsignmentItem when there are multiple goodsItems with same codes, first item", consignmentItems[0].Consignee);
					AssertNull("TransitionalPeriod: Expected empty Consignee in ConsignmentItem when there are multiple goodsItems with same codes, second item", consignmentItems[1].Consignee);

					nctsBill.Consignee.E2_OA_Address = orgAddressItem1.PK;
					wrapper = GetWrapper(nctsHeader);
					consignmentItems = wrapper.HouseConsignment.First().ConsignmentItem.ToList();
					AssertNull("TransitionalPeriod: Expected empty Consignee in Consignment when there are multiple goodsItems with same codes as in consignment but house consignment has code different than in consignment", wrapper.CommonConsignmentData.Consignee);
					AssertNotNull("TransitionalPeriod: Expected filled Consignee in ConsignmentItem when there are multiple goodsItems with same codes, first item", consignmentItems[0].Consignee);
					AssertNotNull("TransitionalPeriod: Expected filled Consignee in ConsignmentItem when there are multiple goodsItems with same codes, second item", consignmentItems[1].Consignee);

					nctsBill.Consignee.E2_OA_Address = orgAddress.PK;
					wrapper = GetWrapper(nctsHeader);
					consignmentItems = wrapper.HouseConsignment.First().ConsignmentItem.ToList();
					AssertNotNull("TransitionalPeriod: Expected filled Consignee in Consignment when there are multiple goodsItems with same codes as in consignment and house consignment has code the same as in consignment", wrapper.CommonConsignmentData.Consignee);
					AssertNull("TransitionalPeriod: Expected empty Consignee in ConsignmentItem when there are multiple goodsItems with same codes, first item", consignmentItems[0].Consignee);
					AssertNull("TransitionalPeriod: Expected empty Consignee in ConsignmentItem when there are multiple goodsItems with same codes, second item", consignmentItems[1].Consignee);

					var addInfo = goodsItem1.AdditionalInfos.AddNew();
					addInfo.CSI_Code = "30600";
					addInfo.CSI_SubType = "INF";
					wrapper = GetWrapper(nctsHeader);
					AssertNull("TransitionalPeriod: Expected empty Consignee in Consignment when there is at least one goodsItem with AdditionalInfo INF with code 30600", wrapper.CommonConsignmentData.Consignee);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					nctsHeader.Bills.DeleteAll();
					nctsHeader.Consignee.E2_OA_Address = orgAddress.PK;
					wrapper = GetWrapper(nctsHeader);
					AssertNotNull("FinalPeriod: Expected filled Consignee in Consignment when there are no goodsItems or Bills", wrapper.CommonConsignmentData.Consignee);

					var nctsBill = nctsHeader.Bills.AddNew();
					nctsBill.Consignee.E2_OA_Address = orgAddress.PK;
					wrapper = GetWrapper(nctsHeader);
					AssertNotNull("FinalPeriod: Expected filled Consignee in Consignment when there are no goodsItems and is the same as in Bills", wrapper.CommonConsignmentData.Consignee);
					AssertNull("FinalPeriod: Expected empty Consignee in House when code is the same as in consignment", wrapper.HouseConsignment.First().Consignee);

					nctsBill.Consignee.E2_OA_Address = orgAddress2.PK;
					var goodsItem1 = nctsBill.GoodsItems.AddNew();
					goodsItem1.Consignee.E2_OA_Address = orgAddress2.PK;
					wrapper = GetWrapper(nctsHeader);
					AssertNull("FinalPeriod: Expected empty Consignee in Consignment when there are no goodsItems but is not the same as in Bills", wrapper.CommonConsignmentData.Consignee);
					AssertNotNull("FinalPeriod: Expected filled Consignee in House when code is the not same as in consignment but same as in goodItem", wrapper.HouseConsignment.First().Consignee);

					goodsItem1.Consignee.E2_OA_Address = orgAddress3.PK;
					wrapper = GetWrapper(nctsHeader);
					AssertNull("FinalPeriod: Expected empty Consignee in Consignment when code is not the same as in goodsItems", wrapper.CommonConsignmentData.Consignee);
					AssertNull("FinalPeriod: Expected empty Consignee in House when code is the not same as in goodItem", wrapper.HouseConsignment.First().Consignee);
					AssertNull("FinalPeriod: Expected empty Consignee in Item when is final period", wrapper.HouseConsignment.First().ConsignmentItem.First().Consignee);
				}
			});
		}

		public void TestPlaceOfUnloading()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					AssertNull("Expected empty PlaceOfUnloading when not declared", wrapper.PlaceOfUnloading);

					departureMovement.BM_PlaceOfUnloading = "ESMAD";
					departureMovement.BM_TypeOfSecurity = "EXI";
					wrapper = GetWrapper(nctsHeader);
					AssertNotNull("TransitionalPeriod: Expected filled PlaceOfUnloading when securityType is not NON (is EXI)", wrapper.PlaceOfUnloading);

					departureMovement.BM_SpecificCircumstance = NctsSpecificCircumstanceIndicatorList.Codes.XXX;
					wrapper = GetWrapper(nctsHeader);
					AssertNull("TransitionalPeriod: Expected not filled PlaceOfUnloading when securityType is not NON (is EXI) and specific_circumstance is XXX", wrapper.PlaceOfUnloading);

					departureMovement.BM_SpecificCircumstance = NctsSpecificCircumstanceIndicatorList.Codes.A20;
					wrapper = GetWrapper(nctsHeader);
					AssertNotNull("TransitionalPeriod: Expected filled PlaceOfUnloading when securityType is not NON (is EXI) and specific_circumstance is not XXX", wrapper.PlaceOfUnloading);

					departureMovement.BM_TypeOfSecurity = "NON";
					wrapper = GetWrapper(nctsHeader);
					AssertNull("TransitionalPeriod: Expected empty PlaceOfUnloading when securityType is NON", wrapper.PlaceOfUnloading);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					departureMovement.BM_PlaceOfUnloading = "ESMAD";
					departureMovement.BM_TypeOfSecurity = "EXI";
					wrapper = GetWrapper(nctsHeader);
					AssertNotNull("FinalPeriod: Expected filled PlaceOfUnloading when securityType is not NON (is EXI)", wrapper.PlaceOfUnloading);

					departureMovement.BM_TypeOfSecurity = "NON";
					wrapper = GetWrapper(nctsHeader);
					AssertNull("FinalPeriod: Expected empty PlaceOfUnloading when securityType is NON", wrapper.PlaceOfUnloading);
				}
			});
		}

		public void TestMethodOfPayment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty MethodOfPayment when not declared", ZString.Empty, wrapper.CommonConsignmentData.MethodOfPayment);

				departureMovement.BM_MethodOfPayment = "A";
				departureMovement.BM_TypeOfSecurity = "EXI";
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected filled MethodOfPayment when securityType is not NON (is EXI)", "A", wrapper.CommonConsignmentData.MethodOfPayment);

				departureMovement.BM_TypeOfSecurity = "NON";
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected empty MethodOfPayment when securityType is NON", ZString.Empty, wrapper.CommonConsignmentData.MethodOfPayment);
			});
		}

		public void TestAdditionalSupplyChainActor()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalSupplyChainActor when no data declared", 0, wrapper.AdditionalSupplyChainActor.Count);

				nctsHeader.MovementHeader.CusSupplyChainActors.AddNew();
				wrapper = GetWrapper(nctsHeader);
				var additionalSupplyChainActor = wrapper.AdditionalSupplyChainActor;
				AssertEquals("Expected filled AdditionalSupplyChainActor", 1, additionalSupplyChainActor.Count);
				AssertSame("Cached AdditionalSupplyChainActor", wrapper.AdditionalSupplyChainActor, additionalSupplyChainActor);
			});
		}

		public void TestHouseConsignment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty HouseConsignment when no data declared", 0, wrapper.HouseConsignment.Count);

				nctsHeader.Bills.AddNew();
				nctsHeader.Bills.AddNew();
				wrapper = GetWrapper(nctsHeader);
				var houseConsignment = wrapper.HouseConsignment;
				AssertEquals("Expected filled HouseConsignment", 2, houseConsignment.Count);
				AssertSame("Cached HouseConsignment", wrapper.HouseConsignment, houseConsignment);
			});
		}

		public void TestDepartureTransportMeans()
		{
			CombineAssertions(() =>
			{
				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._1_SeaTransport;
				departureMovement.TransportTypeAtDeparture = "11";
				departureMovement.VesselNameAtDeparture = "Vessel";
				departureMovement.VesselCountryAtDeparture = "ES";
				var nctsBill = nctsHeader.Bills.AddNew();
				nctsBill.TransportTypeAtDeparture = "11";
				nctsBill.VesselNameAtDeparture = "Vessel";
				nctsBill.VesselCountryAtDeparture = "ES";

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("TransitionalPeriod: Expected filled DepartureTransportMeans with 1 element ignoring house data", 1, wrapper.DepartureTransportMeans.Count);
					AssertEquals("TransitionalPeriod: Expected not filled DepartureTransportMeans in House when Transitional Period", 0, wrapper.HouseConsignment.First().DepartureTransportMeans.Count);
				}
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected filled DepartureTransportMeans when house data are the same", 1, wrapper.DepartureTransportMeans.Count);
					AssertEquals("FinalPeriod: Expected not filled DepartureTransportMeans in House when house data are the same", 0, wrapper.HouseConsignment.First().DepartureTransportMeans.Count);

					nctsBill.TransportTypeAtDeparture = "10";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected not filled DepartureTransportMeans when house data TransportTypeAtDeparture is different", 0, wrapper.DepartureTransportMeans.Count);
					AssertEquals("FinalPeriod: Expected filled DepartureTransportMeans in House when house data TransportTypeAtDeparture is different", 1, wrapper.HouseConsignment.First().DepartureTransportMeans.Count);

					nctsBill.TransportTypeAtDeparture = ZString.Empty;
					nctsBill.VesselNameAtDeparture = ZString.Empty;
					nctsBill.VesselCountryAtDeparture = ZString.Empty;
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected filled DepartureTransportMeans when house data are empty", 1, wrapper.DepartureTransportMeans.Count);
					AssertEquals("FinalPeriod: Expected not filled DepartureTransportMeans in House when house data are empty", 0, wrapper.HouseConsignment.First().DepartureTransportMeans.Count);

					nctsBill.TransportAtDeparture = "ID";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected not filled DepartureTransportMeans when house data TransportAtDeparture is different", 0, wrapper.DepartureTransportMeans.Count);
					AssertEquals("FinalPeriod: Expected filled DepartureTransportMeans in House when house data TransportAtDeparture is different", 1, wrapper.HouseConsignment.First().DepartureTransportMeans.Count);

					nctsBill.TransportAtDeparture = ZString.Empty;
					nctsBill.TransportCountryAtDeparture = "PT";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected not filled DepartureTransportMeans when house data TransportCountryAtDeparture is different", 0, wrapper.DepartureTransportMeans.Count);
					AssertEquals("FinalPeriod: Expected filled DepartureTransportMeans in House when house data TransportCountryAtDeparture is different", 1, wrapper.HouseConsignment.First().DepartureTransportMeans.Count);

					nctsBill.TransportCountryAtDeparture = ZString.Empty;
					nctsBill.VesselNameAtDeparture = "Vessel2";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected not filled DepartureTransportMeans when house data VesselNameAtDeparture is different", 0, wrapper.DepartureTransportMeans.Count);
					AssertEquals("FinalPeriod: Expected filled DepartureTransportMeans in House when house data VesselNameAtDeparture is different", 1, wrapper.HouseConsignment.First().DepartureTransportMeans.Count);

					nctsBill.VesselNameAtDeparture = ZString.Empty;
					nctsBill.VesselCountryAtDeparture = "PT";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected not filled DepartureTransportMeans when house data VesselCountryAtDeparture is different", 0, wrapper.DepartureTransportMeans.Count);
					AssertEquals("FinalPeriod: Expected filled DepartureTransportMeans in House when house data VesselCountryAtDeparture is different", 1, wrapper.HouseConsignment.First().DepartureTransportMeans.Count);

					departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._3_RoadTransport;
					nctsBill.VesselCountryAtDeparture = ZString.Empty;
					nctsBill.Trailer1IDAtDeparture = "31";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected not filled DepartureTransportMeans when house data Trailer1IDAtDeparture is different", 0, wrapper.DepartureTransportMeans.Count);
					AssertEquals("FinalPeriod: Expected filled DepartureTransportMeans in House when house data Trailer1IDAtDeparture is different", 1, wrapper.HouseConsignment.First().DepartureTransportMeans.Count);

					departureMovement.Trailer1IDAtDeparture = "31";
					nctsBill.Trailer1NationalityAtDeparture = "PT";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected not filled DepartureTransportMeans when house data Trailer1NationalityAtDeparture is different", 0, wrapper.DepartureTransportMeans.Count);
					AssertEquals("FinalPeriod: Expected filled DepartureTransportMeans in House when house data Trailer1NationalityAtDeparture is different", 1, wrapper.HouseConsignment.First().DepartureTransportMeans.Count);

					nctsBill.Trailer1IDAtDeparture = ZString.Empty;
					nctsBill.Trailer1NationalityAtDeparture = ZString.Empty;
					nctsBill.Trailer2IDAtDeparture = "31";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected not filled DepartureTransportMeans when house data Trailer2IDAtDeparture is different", 0, wrapper.DepartureTransportMeans.Count);
					AssertEquals("FinalPeriod: Expected filled DepartureTransportMeans in House when house data Trailer2IDAtDeparture is different", 1, wrapper.HouseConsignment.First().DepartureTransportMeans.Count);

					departureMovement.Trailer2IDAtDeparture = "31";
					nctsBill.Trailer2NationalityAtDeparture = "PT";
					wrapper = GetWrapper(nctsHeader);
					AssertEquals("FinalPeriod: Expected not filled DepartureTransportMeans when house data Trailer2NationalityAtDeparture is different", 0, wrapper.DepartureTransportMeans.Count);
					AssertEquals("FinalPeriod: Expected filled DepartureTransportMeans in House when house data Trailer2NationalityAtDeparture is different", 1, wrapper.HouseConsignment.First().DepartureTransportMeans.Count);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;

			wrapper = GetWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
		NCTS5CommonConsignmentDepartureAndAmendmentWrapper wrapper;

		NCTS5CommonConsignmentDepartureAndAmendmentWrapper GetWrapper(NctsHeader header, string messageType = "") => new NCTS5CommonConsignmentDepartureAndAmendmentWrapper(header, messageType);

		protected override NCTS5CommonConsignmentDepartureAndAmendmentWrapper GetProvider() => wrapper;
	}
}
