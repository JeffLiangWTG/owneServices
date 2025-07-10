using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManArrivalPortLookupsTest : Customs.Business.Testing.CusSeaManArrivalPortLookupsTest
	{
		public void TestActualArrivalStatusList()
		{
			var port = Factory.New<CusSeaManArrivalPort>();
			AssertNotNull("AcceptedRejectedList shouldn't be null", port.Lookups.ActualArrivalStatusList);
			AssertEquals("AcceptedRejectedList should be of type CMRAcceptedRejectedList", typeof(CMRBaseStatuses), port.Lookups.ActualArrivalStatusList.GetType());
		}

		public void TestCTOAddressOrgs()
		{
			var port = Factory.New<CusSeaManArrivalPort>();
			AssertNotNull("shouldn't be null.", port.Lookups.CTOAddressOrgs);
			AssertEquals("should be a SeaCTOCollection", typeof(SeaCTOCollection), port.Lookups.CTOAddressOrgs.GetType());
		}

		public void TestCTOAddressOrgsRelationshipFilter()
		{
			var org1 = OrgHeader.New(Factory);
			org1.OH_RL_NKClosestPort = "AAAAA";
			org1.OH_IsSeaCTO = true;
			org1.OH_IsMiscFreightServices = true;

			var org2 = OrgHeader.New(Factory);
			org2.OH_RL_NKClosestPort = "BBBBB";
			org2.OH_IsSeaCTO = true;
			org2.OH_IsMiscFreightServices = true;

			var port = Factory.New<CusSeaManArrivalPort>();

			var coll = port.Lookups.CTOAddressOrgs;
			coll.Load();
			AssertEquals(true, coll.Count >= 2);

			port.BA_RL_NKArrivalPort = "AAAAA";
			coll = port.Lookups.CTOAddressOrgs;
			coll.Load();
			AssertEquals(2, coll.Count);
			AssertEquals("AAAAA", coll[0].OH_RL_NKClosestPort);
		}

		public void TestCTOAddressOrgsDefaultsFilterControl()
		{
			var port = Factory.New<CusSeaManArrivalPort>();
			port.BA_RL_NKArrivalPort = "AAAAA";
			var coll = port.Lookups.CTOAddressOrgs;
			AssertEquals("AAAAA", coll.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
		}

		[NUnit.Framework.TestDate(2005, 10, 01)]
		public void TestBerthCodes()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var berthCode1 = CMRBerthCode.New(Factory);
				berthCode1.BC_PortCode = "PPPPP";
				berthCode1.BC_BerthCode = "111";
				berthCode1.BC_BerthCodeStartDate = new ZDateTime(2005, 01, 20);
				berthCode1.BC_BerthCodeEndDate = new ZDateTime(2010, 01, 20);

				var berthCode2 = CMRBerthCode.New(Factory);
				berthCode2.BC_PortCode = "PPPPP";
				berthCode2.BC_BerthCode = "222";
				berthCode2.BC_BerthCodeEndDate = new ZDateTime(2010, 01, 20);

				var berthCode3 = CMRBerthCode.New(Factory);
				berthCode3.BC_PortCode = "DDDDD";
				berthCode3.BC_BerthCode = "333";
				berthCode3.BC_BerthCodeStartDate = new ZDateTime(2004, 05, 20);
				berthCode3.BC_BerthCodeEndDate = new ZDateTime(2016, 04, 11);

				Factory.Save();

				var header = Factory.New<CusSeaManTranHead>();
				var port = header.Arrivals.AddNew();

				port.BA_ArrivalPortATA = new ZDateTime(2005, 10, 01, 12, 10, 20);
				port.BA_ArrivalPortETA = ZDateTime.Empty;
				port.BA_RL_NKArrivalPort = "PPPPP";
				AssertEquals(2, port.Lookups.BerthCodeList.Count);

				port.BA_ArrivalPortATA = ZDateTime.Empty;
				port.BA_ArrivalPortETA = new ZDateTime(2005, 10, 01, 12, 10, 20);
				port.BA_RL_NKArrivalPort = "PPPPP";
				AssertEquals(2, port.Lookups.BerthCodeList.Count);

				port.BA_ArrivalPortATA = ZDateTime.Empty;
				port.BA_ArrivalPortETA = ZDateTime.Empty;
				port.BA_RL_NKArrivalPort = "PPPPP";
				AssertEquals(2, port.Lookups.BerthCodeList.Count);

				port.BA_RL_NKArrivalPort = "DDDDD";
				port.BA_ArrivalPortATA = new ZDateTime(2003, 01, 01);
				AssertEquals(0, port.Lookups.BerthCodeList.Count);
			}

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var universalHelper = new UniversalReferenceTestDataHelper(Factory);
				universalHelper.CreateNewOrGetExistingCusCodeType(AUConstants.RefCusCodeTypeCodes.CMRBC, "Berth Code Type", Core.Constants.CountryCodes.Australia);
				universalHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRBC, "ACODE", "ADescription", ZDateTime.MinSmallDateTimeValue, new ZDateTime(2003, 1, 1), AUConstants.RefCusCodeAttributesNames.BerthPortCode, "AUSYD");
				universalHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRBC, "BCODE", "BDescription", ZDateTime.MinSmallDateTimeValue, new ZDateTime(2004, 1, 1), AUConstants.RefCusCodeAttributesNames.BerthPortCode, "AUSYD");
				universalHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRBC, "CCODE", "CDescription", ZDateTime.MinSmallDateTimeValue, new ZDateTime(2005, 1, 1), AUConstants.RefCusCodeAttributesNames.BerthPortCode, "AUSYD");
				universalHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRBC, "DCODE", "DDescription", ZDateTime.MinSmallDateTimeValue, new ZDateTime(2006, 1, 1), AUConstants.RefCusCodeAttributesNames.BerthPortCode, "AUSYD");
				universalHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRBC, "ECODE", "EDescription", ZDateTime.MinSmallDateTimeValue, new ZDateTime(2006, 1, 1), AUConstants.RefCusCodeAttributesNames.BerthPortCode, "AUBNE");
				Factory.Save();

				var header = Factory.New<CusSeaManTranHead>();
				var port = header.Arrivals.AddNew();

				port.BA_ArrivalPortATA = new ZDateTime(2003, 5, 1);
				port.BA_ArrivalPortETA = new ZDateTime(2004, 5, 1);
				port.BA_RL_NKArrivalPort = "AUSYD";
				AssertEquals("Elements are filtered by BA_ArrivalPortATA and BA_RL_NKArrivalPort", "BCODE, CCODE, DCODE", port.Lookups.BerthCodeList.CodesAsString);

				port.BA_ArrivalPortATA = ZDateTime.Empty;
				port.BA_ArrivalPortETA = new ZDateTime(2004, 5, 1);
				port.BA_RL_NKArrivalPort = "AUSYD";
				AssertEquals("Elements are filtered by BA_ArrivalPortETA and BA_RL_NKArrivalPort", "CCODE, DCODE", port.Lookups.BerthCodeList.CodesAsString);

				port.BA_ArrivalPortATA = ZDateTime.Empty;
				port.BA_ArrivalPortETA = ZDateTime.Empty;
				port.BA_RL_NKArrivalPort = "AUSYD";
				AssertEquals("Elements are filtered by ZDatetime.Today and BA_RL_NKArrivalPort", "DCODE", port.Lookups.BerthCodeList.CodesAsString);

				port.BA_RL_NKArrivalPort = "AUBNE";
				AssertEquals("Elements are filtered by ZDatetime.Today and BA_RL_NKArrivalPort", "ECODE", port.Lookups.BerthCodeList.CodesAsString);
			}
		}
	}
}
