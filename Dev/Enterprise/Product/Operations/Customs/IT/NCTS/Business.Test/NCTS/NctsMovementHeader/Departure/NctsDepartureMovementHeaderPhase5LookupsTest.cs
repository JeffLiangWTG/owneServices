using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase5LookupsTest : BusinessObjectLookupsTestCase
{
	public void TestPaymentPartyList()
	{
		AssertEquals("PaymentPartyList Count", 0, departureMovement.ITLookups.PaymentPartyList.Count);
	}

	public void TestDefermentApprovalNumberList()
	{
		AssertEquals("DefermentApprovalNumberList Count", 0, departureMovement.ITLookups.DefermentApprovalNumberList.Count);
	}

	public void TestBondedWarehouseCollection()
	{
		AssertType<BondedWarehouseCollection>("BondedWarehouseCollection Type", departureMovement.ITLookups.BondedWarehouseCollection);
	}

	public void TestCustomsChannelCodeList()
	{
		var expectedCustomsChannels = new string[]
		{
			"CA - Automatic Control",
			"CD - Document Control",
			"VM - Inspection",
			"CS - Scanner Control",
		};

		var customsChannels = departureMovement.ITLookups.CustomsChannelCodeList.ElementsAsString
			.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);

		AssertContainsExactElementsInExactOrder("CustomsChannelCodeList", expectedCustomsChannels, customsChannels);
	}

	public void TestNctsParticipantTypeList()
	{
		AssertEquals("NctsParticipantTypeList Count", 0, departureMovement.ITLookups.NctsParticipantTypeList.Count);
	}

	public void TestTransportAtBorderTypeOfIdList()
	{
		CombineAssertions(() =>
		{
			TestCase("1", "10, 11, 99");
			TestCase("2", "21, 99");
			TestCase("3", "30, 99");
			TestCase("4", "40, 41, 99");
			TestCase("5", "");
			TestCase("6", "");
			TestCase("7", "10, 11, 21, 30, 40, 41, 80, 81, 99");
			TestCase("8", "80, 81, 99");
			TestCase("9", "10, 11, 21, 30, 40, 41, 80, 81, 99");
		});

		void TestCase(string exportTransportMode, string expectedLookupsList)
		{
			using (TemporarilySetTransitionPeriod(true))
			{
				header.MovementHeader.BM_ExportTransportMode = exportTransportMode;
				var list = departureMovement.Lookups.TransportAtBorderTypeOfIdList;
				AssertEquals($"exportTransportMode = {exportTransportMode}", expectedLookupsList, list.CodesAsString);
				AssertSame($"Cached for exportTransportMode = {exportTransportMode}", list, departureMovement.Lookups.TransportAtBorderTypeOfIdList);
			}

			using (TemporarilySetTransitionPeriod(false))
			{
				var list = departureMovement.Lookups.TransportAtBorderTypeOfIdList;
				AssertCollectionNotContains("code 99 should not be present", "99", list.GetAllCodes());
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.NewDepartureNctsHeader();
		header.BH_ApplicationCode = "NC5";
		departureMovement = header.MovementHeader;
	}

	IDisposable TemporarilySetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);

	NctsHeader header;
	NctsDepartureMovementHeader departureMovement;
}
