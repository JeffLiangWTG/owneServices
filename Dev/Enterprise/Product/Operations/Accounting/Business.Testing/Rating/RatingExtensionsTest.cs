using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	public class RatingExtensionsTest : TestCaseWithFactory
	{
		public void TestFindBestMatches_ShouldReturnTheSameResultParallelizedCalculationOrNot()
		{
			const int ChargesCount = RatingExtensions.ThresholdForFindBestMatchesParallelizedCalculation;
			const int OrderReferencesCount = 5;
			const int ChargeCodesCount = 5;
			const int ContainerNumbersCount = 5;
			const int ContainerCodesCount = 5;
			const int PartiesCount = 5;

			var allChargeCodes = Factory
				.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK))
				.ToArray();

			var chargeCodes = allChargeCodes
				.Take(ChargeCodesCount)
				.ToArray();

			AssertEquals("PRECONDITION", ChargeCodesCount, chargeCodes.Length);

			var containerCodes = new string[ContainerCodesCount];
			for (var i = 0; i < ContainerCodesCount; i++)
			{
				containerCodes[i] = TestObjectCreator.GetRandomString(4);
			}

			var parties = new ZGuid[PartiesCount];
			for (var i = 0; i < PartiesCount; i++)
			{
				parties[i] = ZGuid.NewZGuid();
			}

			var orderReferences = new string[OrderReferencesCount];
			for (var i = 0; i < OrderReferencesCount; i++)
			{
				orderReferences[i] = TestObjectCreator.GetRandomString(10);
			}

			var containerNumbers = new string[ContainerNumbersCount];
			for (var i = 0; i < ContainerNumbersCount; i++)
			{
				containerNumbers[i] = TestObjectCreator.GetRandomString(10);
			}

			var r = new Random();
			var charges = new List<BaseCharge>();
			var infos = new List<AutoRateInfo>();

			// Duplicate infos can cause the result of the test unstable because parallel mode can match
			// any of the infos to charges (which is fine in production) while synchronized mode picks the infos in order.
			// In this test, we expect single matches by asserting descriptions with index numbers so let's make unique combinations.
			var combinations = new HashSet<(ZGuid, ZGuid, ZString, ZString, ZString)>();

			var index = 0;
			while (infos.Count < ChargesCount)
			{
				var chargeCode = chargeCodes[r.Next(0, chargeCodes.Length - 1)];
				var orderReference = orderReferences[r.Next(0, orderReferences.Length - 1)];
				var party = parties[r.Next(0, parties.Length - 1)];
				var containerCode = containerCodes[r.Next(0, containerCodes.Length - 1)];
				var containerNumber = containerNumbers[r.Next(0, containerNumbers.Length - 1)];

				if (combinations.Add((chargeCode.PK, party, orderReference, containerCode, containerNumber)))
				{
					infos.Add
					(
						SetupNewAutoRateInfo(
							chargeCode,
							party,
							description: index.ToString(),
							orderReference: orderReference,
							containerCode: containerCode,
							containerNumber: containerNumber
						)
					);

					index++;
				}
			}

			index = 0;
			combinations.Clear();
			while (charges.Count < ChargesCount)
			{
				var chargeCode = chargeCodes[r.Next(0, chargeCodes.Length - 1)];
				var party = parties[r.Next(0, parties.Length - 1)];
				var orderReference = orderReferences[r.Next(0, orderReferences.Length - 1)];
				var containerCode = containerCodes[r.Next(0, containerCodes.Length - 1)];
				var containerNumber = containerNumbers[r.Next(0, containerNumbers.Length - 1)];

				if (combinations.Add((chargeCode.PK, party, orderReference, containerCode, containerNumber)))
				{
					charges.Add
					(
						SetupNewCharge
						(
							chargeCode,
							party,
							description: index.ToString(),
							orderReference: orderReference,
							containerCode: containerCode,
							containerNumber: containerNumber
						)
					);
					index++;
				}
			}

			var singleThreadResult = infos.FindBestMatches(charges, CostSell.Cost)
				.Select(match => match.rateInfo.Description + " " + match.charge.JR_Desc)
				.ToArray();

			// Should not affect the result as there should not be matching existing charge
			var extraInfo = new AutoRateInfo(Factory) { ChargeCode = allChargeCodes.Last() };
			infos.Add(extraInfo);

			var multiThreadResult = infos.FindBestMatches(charges, CostSell.Cost)
				.Select(match => match.rateInfo.Description + " " + match.charge.JR_Desc)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(singleThreadResult, multiThreadResult);
		}

		public void TestFindBestMatches_ShouldConsiderLocationInformationInMatching()
		{
			var chargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK)).First();
			var party = new ZGuid();

			var rateInfo1 = SetupNewAutoRateInfo(chargeCode, party, location: "LOC1", locationType: "TYPE1");
			var rateInfo2 = SetupNewAutoRateInfo(chargeCode, party);
			var rateInfo3 = SetupNewAutoRateInfo(chargeCode, party, location: "LOC2", locationType: "TYPE2");
			var rateInfo4 = SetupNewAutoRateInfo(chargeCode, party, location: "LOC1", locationType: null);
			var rateInfo5 = SetupNewAutoRateInfo(chargeCode, party, location: null, locationType: "TYPE2");

			var charge1 = SetupNewCharge(chargeCode, party, location: "LOC1", locationType: "TYPE1");
			var charge2 = SetupNewCharge(chargeCode, party);
			var charge3 = SetupNewCharge(chargeCode, party, location: "LOC3", locationType: "TYPE1"); // Location does not match, but LocationType match
			var charge4 = SetupNewCharge(chargeCode, party, location: "LOC1", locationType: "TYPE3	"); // Location match, but LocationType does not match

			var infos = new List<AutoRateInfo>() { rateInfo1, rateInfo2, rateInfo3, rateInfo4, rateInfo5 };
			var charges = new List<BaseCharge>() { charge1, charge2, charge3, charge4 };

			var matchingInfo = infos.FindBestMatches(charges, CostSell.Revenue);

			var rateInfo1Match = matchingInfo.SingleOrDefault(i => i.rateInfo == rateInfo1);
			AssertEquals(rateInfo1Match.charge, charge1);

			var rateInfo2Match = matchingInfo.SingleOrDefault(i => i.rateInfo == rateInfo2);
			AssertEquals(rateInfo2Match.charge, charge2);

			(AutoRateInfo, BaseCharge, bool) defaultValue = (null, null, false);

			var rateInfo3Match = matchingInfo.SingleOrDefault(i => i.rateInfo == rateInfo3);
			AssertEquals(rateInfo3Match, defaultValue); // not matched

			var rateInfo4Match = matchingInfo.SingleOrDefault(i => i.rateInfo == rateInfo4);
			AssertEquals(rateInfo4Match, defaultValue); // not matched

			var rateInfo5Match = matchingInfo.SingleOrDefault(i => i.rateInfo == rateInfo5);
			AssertEquals(rateInfo5Match, defaultValue); // not matched
		}

		public void TestFindBestMatches_ShouldConsiderServiceIDInMatching()
		{
			var chargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK)).First();
			var party1 = new ZGuid();
			var party2 = new ZGuid();

			var charge1 = SetupNewCharge(chargeCode, party1, serviceID: "Service 3");
			var charge2 = SetupNewCharge(chargeCode, party1, serviceID: "Service 1");
			var charge3 = SetupNewCharge(chargeCode, party1, serviceID: "Service 2");
			var charge4 = SetupNewCharge(chargeCode, party1, serviceID: "Service 2");

			var rateInfo1 = SetupNewAutoRateInfo(chargeCode, party2, serviceID: "Service 1");	// Should match charge2
			var rateInfo2 = SetupNewAutoRateInfo(chargeCode, party2, serviceID: "Service 1");	// Should not match
			var rateInfo3 = SetupNewAutoRateInfo(chargeCode, party2, serviceID: "Service 2");	// Should match charge3
			var rateInfo4 = SetupNewAutoRateInfo(chargeCode, party2, serviceID: "Service 2");	// Should match charge4

			var infos = new List<AutoRateInfo>() { rateInfo1, rateInfo2, rateInfo3, rateInfo4 };
			var charges = new List<BaseCharge>() { charge1, charge2, charge3, charge4 };

			var matchingInfo = infos.FindBestMatches(charges, CostSell.Revenue);

			(AutoRateInfo, BaseCharge, bool) defaultValue = (null, null, false);

			var rateInfo1Match = matchingInfo.SingleOrDefault(i => i.rateInfo == rateInfo1);
			AssertEquals(rateInfo1Match.charge, charge2);

			var rateInfo2Match = matchingInfo.SingleOrDefault(i => i.rateInfo == rateInfo2);
			AssertEquals(rateInfo2Match, defaultValue);

			var rateInfo3Match = matchingInfo.SingleOrDefault(i => i.rateInfo == rateInfo3);
			AssertEquals(rateInfo3Match.charge, charge3);

			var rateInfo4Match = matchingInfo.SingleOrDefault(i => i.rateInfo == rateInfo4);
			AssertEquals(rateInfo4Match.charge, charge4);
		}

		public void TestFindBestMatches_ShouldConsiderTransportProviderInMatching()
		{
			var chargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK)).First();
			var party1 = new ZGuid();
			var party2 = new ZGuid();

			var serviceProvider1 = ZGuid.NewZGuid();
			var transportProvider1 = ZGuid.NewZGuid();
			var transportProvider2 = ZGuid.NewZGuid();
			var transportProvider3 = ZGuid.NewZGuid();
			var transportProvider4 = ZGuid.NewZGuid();

			var charge1 = SetupNewCharge(chargeCode, party1, serviceProviderPK: serviceProvider1.ToString(), transportProviderPK: transportProvider1.ToString());
			var charge2 = SetupNewCharge(chargeCode, party1, serviceProviderPK: serviceProvider1.ToString(), transportProviderPK: transportProvider2.ToString());
			var charge3 = SetupNewCharge(chargeCode, party1, serviceProviderPK: serviceProvider1.ToString(), transportProviderPK: transportProvider3.ToString());

			var rateInfo1 = SetupNewAutoRateInfo(chargeCode, party2, transportProviderPK: transportProvider2.ToString());	// Should match charge2
			var rateInfo2 = SetupNewAutoRateInfo(chargeCode, party2, transportProviderPK: transportProvider4.ToString());	// Should not match
			var rateInfo3 = SetupNewAutoRateInfo(chargeCode, party2, transportProviderPK: transportProvider3.ToString());	// Should match charge3

			var infos = new List<AutoRateInfo>() { rateInfo1, rateInfo2, rateInfo3 };
			var charges = new List<BaseCharge>() { charge1, charge2, charge3 };

			var matchingInfo = infos.FindBestMatches(charges, CostSell.Revenue);

			(AutoRateInfo, BaseCharge, bool) defaultValue = (null, null, false);

			var rateInfo1Match = matchingInfo.SingleOrDefault(i => i.rateInfo == rateInfo1);
			AssertEquals(rateInfo1Match.charge, charge2);

			var rateInfo2Match = matchingInfo.SingleOrDefault(i => i.rateInfo == rateInfo2);
			AssertEquals(rateInfo2Match, defaultValue);

			var rateInfo3Match = matchingInfo.SingleOrDefault(i => i.rateInfo == rateInfo3);
			AssertEquals(rateInfo3Match.charge, charge3);
		}

		public void TestFindBestMatches_ShouldConsiderTransportProviderInMatching_EdgeCases()
		{
			var chargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK)).First();
			var party1 = new ZGuid();
			var party2 = new ZGuid();

			void EdgeCaseAssert(ZGuid? costSP, ZGuid? costTP, ZGuid? sellTP, bool shouldMatch, string message)
			{
				var charge = SetupNewCharge(chargeCode, party1, serviceProviderPK: costSP.ToString(), transportProviderPK: costTP.ToString());
				var rateInfo = SetupNewAutoRateInfo(chargeCode, party2, transportProviderPK: sellTP.ToString());

				var infos = new List<AutoRateInfo> { rateInfo };
				var charges = new List<BaseCharge> { charge };

				var matchingInfo = infos.FindBestMatches(charges, CostSell.Revenue);

				var rateInfo1Match = matchingInfo.SingleOrDefault(i => i.rateInfo == rateInfo);
				if (shouldMatch)
				{
					AssertEquals(message, charge, rateInfo1Match.charge);
				}
				else
				{
					(AutoRateInfo, BaseCharge, bool) defaultValue = (null, null, false);
					AssertEquals(message, defaultValue, rateInfo1Match);
				}
			}

			var org1 = ZGuid.NewZGuid();
			var org2 = ZGuid.NewZGuid();
			var org3 = ZGuid.NewZGuid();

			EdgeCaseAssert(costSP: org1, costTP: org2, sellTP: org1, true, "Should match when sell TP matches cost SP");
			EdgeCaseAssert(costSP: org2, costTP: org1, sellTP: org1, true, "Should match when sell TP matches cost TP");
			EdgeCaseAssert(costSP: null, costTP: org2, sellTP: org1, true, "Should match when cost SP is empty");
			EdgeCaseAssert(costSP: org2, costTP: null, sellTP: org1, true, "Should match when cost TP is empty");
			EdgeCaseAssert(costSP: null, costTP: null, sellTP: org1, true, "Should match when cost TP and SP are empty");
			EdgeCaseAssert(costSP: org1, costTP: org2, sellTP: org3, false, "Should not match when cost TP and SP don't match sell TP");
		}

		public void TestFindBestMatches_SameAttributes()
		{
			// Tests scenarios where existing charges or newly autorated charges have same attributes resulting in the
			// same score calculated between all pairs. This is a common scenario for warehouse.
			//
			// So, to avoid this we have an optimization where we autorate the score once.
			// This test basically ensures that everything works.
			//
			// This test tests the functionality. The performance is tested in:
			// https://devops.wisetechglobal.com/wtg/InternalTools/_git/CWBenchmarks?path=%2FBusiness%2FRating%2FMergeChargesBenchmark.cs
			var chargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK)).First();
			var party = new ZGuid();

			var rateInfo1 = SetupNewAutoRateInfo(chargeCode, party, description: "20GP 1", containerCode: "20GP");
			var rateInfo2 = SetupNewAutoRateInfo(chargeCode, party, description: "20GP 2", containerCode: "20GP");
			var rateInfo3 = SetupNewAutoRateInfo(chargeCode, party, description: "40GP 1", containerCode: "40GP");
			var rateInfo4 = SetupNewAutoRateInfo(chargeCode, party, description: "Empty 1", containerCode: null);
			var rateInfo5 = SetupNewAutoRateInfo(chargeCode, party, description: "Empty 2", containerCode: null);
			var rateInfo6 = SetupNewAutoRateInfo(chargeCode, party, description: "45HC", containerCode: "45HC");	// No match

			var charge1 = SetupNewCharge(chargeCode, party, description: "20GP 1", containerCode: "20GP");		// rateInfo1
			var charge2 = SetupNewCharge(chargeCode, party, description: "40GP 1", containerCode: "40GP");		// rateInfo3
			var charge3 = SetupNewCharge(chargeCode, party, description: "40GP 2", containerCode: "40GP");		// no match
			var charge4 = SetupNewCharge(chargeCode, party, description: "Empty 1", containerCode: null);		// rateInfo4
			var charge5 = SetupNewCharge(chargeCode, party, description: "Empty 2", containerCode: null);		// rateInfo5
			var charge6 = SetupNewCharge(chargeCode, party, description: "Empty 3", containerCode: null);		// rateInfo2

			var infos = new List<AutoRateInfo>() { rateInfo1, rateInfo2, rateInfo3, rateInfo4, rateInfo5, rateInfo6 };
			var charges = new List<BaseCharge>() { charge1, charge2, charge3, charge4, charge5, charge6 };

			var matchingInfo = infos.FindBestMatches(charges, CostSell.Revenue);
			var actualMatch = matchingInfo.Select(m => new
			{
				NewCharge = (string)m.rateInfo.Description,
				ExistingCharge = (string)m.charge.JR_Desc
			}).ToList();

			var expectedMatch = new[]
			{
				new { NewCharge = "20GP 1", ExistingCharge = "20GP 1" },
				new { NewCharge = "40GP 1", ExistingCharge = "40GP 1" },
				new { NewCharge = "Empty 1", ExistingCharge = "Empty 1" },
				new { NewCharge = "Empty 2", ExistingCharge = "Empty 2" },
				new { NewCharge = "20GP 2", ExistingCharge = "Empty 3" }
			};

			AssertContainsExactElementsInAnyOrder(expectedMatch, actualMatch);
		}

		#region Implementation
		BaseCharge SetupNewCharge(
			AccChargeCode accChargeCode,
			ZGuid partyRef,
			string description = "Desc",
			string orderReference = "Ref1",
			string containerCode = "20GP",
			string containerNumber = null,
			string location = null,
			string locationType = null,
			string serviceID = null,
			string transportProviderPK = null,
			string serviceProviderPK = null)
		{
			var charge = Factory.New<BaseCharge>();
			charge.JR_AC = accChargeCode.PK;
			charge.JR_OrderReference = orderReference;
			charge.JR_OH_CostAccount = partyRef;
			charge.JR_Desc = description;

			var containerCodeAttribute = charge.JobChargeAttributes.AddNew();
			containerCodeAttribute.EC_Name = JobChargeAttribTypeList.Codes.ContainerCode;
			containerCodeAttribute.EC_Value = containerCode;

			var containerNumberAttribute = charge.JobChargeAttributes.AddNew();
			containerNumberAttribute.EC_Name = JobChargeAttribTypeList.Codes.ContainerNumber;
			containerNumberAttribute.EC_Value = containerNumber;

			if (!string.IsNullOrEmpty(location))
			{
				var locationAttribute = charge.JobChargeAttributes.AddNew();
				locationAttribute.EC_Name = JobChargeAttribTypeList.Codes.LocationDesc;
				locationAttribute.EC_Value = location;
			}

			if (!string.IsNullOrEmpty(locationType))
			{
				var locationTypeAttribute = charge.JobChargeAttributes.AddNew();
				locationTypeAttribute.EC_Name = JobChargeAttribTypeList.Codes.LocationType;
				locationTypeAttribute.EC_Value = locationType;
			}

			if (!string.IsNullOrEmpty(serviceID))
			{
				var serviceIdAttribute = charge.JobChargeAttributes.AddNew();
				serviceIdAttribute.EC_Name = JobChargeAttribTypeList.Codes.ServiceID;
				serviceIdAttribute.EC_Value = serviceID;
			}

			if (!string.IsNullOrEmpty(transportProviderPK))
			{
				var transportProviderIdAttribute = charge.JobChargeAttributes.AddNew();
				transportProviderIdAttribute.EC_Name = JobChargeAttribTypeList.Codes.TransportProviderPK;
				transportProviderIdAttribute.EC_Value = transportProviderPK;
			}

			if (!string.IsNullOrEmpty(serviceProviderPK))
			{
				var serviceProviderIdAttribute = charge.JobChargeAttributes.AddNew();
				serviceProviderIdAttribute.EC_Name = JobChargeAttribTypeList.Codes.ServiceProviderPK;
				serviceProviderIdAttribute.EC_Value = serviceProviderPK;
			}

			return charge;
		}

		AutoRateInfo SetupNewAutoRateInfo(
			AccChargeCode accChargeCode,
			ZGuid partyRef,
			string description = "Desc",
			string orderReference = "Ref1",
			string containerCode = "20GP",
			string containerNumber = null,
			string location = null,
			string locationType = null,
			string serviceID = null,
			string transportProviderPK = null)
		{
			var info = new AutoRateInfo(Factory);
			info.ChargeCode = accChargeCode;
			info.JobRef = orderReference;
			info.ProviderPK = partyRef;
			info.Attributes.Add(JobChargeAttribTypeList.Codes.ContainerCode, containerCode);
			info.Attributes.Add(JobChargeAttribTypeList.Codes.ContainerNumber, containerNumber);
			info.Description = description;

			if (!string.IsNullOrEmpty(location))
			{
				info.Attributes.Add(JobChargeAttribTypeList.Codes.LocationDesc, location);
			}

			if (!string.IsNullOrEmpty(locationType))
			{
				info.Attributes.Add(JobChargeAttribTypeList.Codes.LocationType, locationType);
			}

			if (!string.IsNullOrEmpty(serviceID))
			{
				info.Attributes.Add(JobChargeAttribTypeList.Codes.ServiceID, serviceID);
			}

			if (!string.IsNullOrEmpty(transportProviderPK))
			{
				info.Attributes.Add(JobChargeAttribTypeList.Codes.TransportProviderPK, transportProviderPK);
			}

			return info;
		}
		#endregion
	}
}
