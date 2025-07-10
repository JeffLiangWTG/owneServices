using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderSailingSynchroniser))]
	sealed class AsycudaManifestHeaderSailingSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchroniser()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Singapore;
			vessel.RV_Name = "ABCSDEWEQE23";
			vessel.RV_RadioCallSign = "CALLME";
			vessel.RV_LloydsNumber = "9832343";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "QS0903";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_OH_Line = carrier.PK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2018, 1, 1);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = new ZDateTime(2018, 9, 1);

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var propertyInfo = new[]
			{
				header.AMA_ParentTableCodeInfo,
				header.AMA_ParentIdInfo,
				header.AMA_VoyageInfo,
				header.AMA_VesselNameInfo,
				header.AMA_LloydsNumberInfo,
				header.AMA_RadioCallSignInfo,
				header.AMA_RN_NKConveyanceNationalityInfo,
			};

			CombineAssertions(() =>
			{
				Assert("Should default to empty.", !header.Sailings.Any());
				AssertNull("Should default to null.", header.Sailing);

				foreach (var info in propertyInfo)
				{
					Assert($"Should default to empty on {info.Name}.", info.Value.IsEmpty);
				}
			});

			header.ChangeSailing(sailing.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Should has one sailing from the ChangeSailing.", 1, header.Sailings.Count);
				AssertEquals("Should be equal to the target.", sailing.PK, header.Sailing.PK);

				AssertEquals("Should attach the sailing.", JobSailingSchema.Constants.Prefix, header.AMA_ParentTableCode);
				AssertEquals("Should attach the sailing.", sailing.PK, header.AMA_ParentId);

				AssertEquals("Should sync the value from JX_JA_RL_NKPortOfLoading.", sailing.JX_JA_RL_NKPortOfLoading, header.AMA_RL_NKPortOfLoading);
				AssertEquals("Should sync the value from JX_JB_RL_NKPortOfDischarge.", sailing.JX_JB_RL_NKPortOfDischarge, header.AMA_RL_NKPortOfDischarge);
				AssertEquals("Should sync the value from JA_E_DEP.", sailing.JX_JA_E_DEP, header.AMA_E_DEP);
				AssertEquals("Should sync the value from JX_JB_E_ARV.", sailing.JX_JB_E_ARV, header.AMA_E_ARV);
				AssertEquals("Should sync the value from RV_RN_NKCountryOfReg.", vessel.RV_RN_NKCountryOfReg, header.AMA_RN_NKConveyanceNationality);
				AssertEquals("Should sync the value from JV_VoyageFlight.", voyage.JV_VoyageFlight, header.AMA_Voyage);
				AssertEquals("Should sync the value from vessel code on voyage.", vessel.RV_Code, header.AMA_VesselName);
				AssertEquals("Should sync the value from vessel lloyds number on voyage.", vessel.RV_LloydsNumber, header.AMA_LloydsNumber);
				AssertEquals("Should sync the value from vessel radio call sign on voyage.", vessel.RV_RadioCallSign, header.AMA_RadioCallSign);
				AssertEquals("Should sync the value from carrier mainAddress on voyage.", carrier.MainAddress.PK, header.AMA_OA_Carrier);
			});

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			CombineAssertions(() =>
			{
				Assert("Should clear sailing as the transport mode is changed and the original value is sea.", !header.Sailings.Any());
				AssertNull("Should clear sailing as the transport mode is changed and the original value is sea.", header.Sailing);

				foreach (var info in propertyInfo)
				{
					Assert($"Should clear the value of {info.Name}.", info.Value.IsEmpty);
				}

				AssertEquals("Should keep the value of AMA_RL_NKPortOfLoading.", sailing.JX_JA_RL_NKPortOfLoading, header.AMA_RL_NKPortOfLoading);
				AssertEquals("Should keep the value of AMA_RL_NKPortOfDischarge.", sailing.JX_JB_RL_NKPortOfDischarge, header.AMA_RL_NKPortOfDischarge);
				AssertEquals("Should keep the value of AMA_E_DEP.", sailing.JX_JA_E_DEP, header.AMA_E_DEP);
				AssertEquals("Should keep the value of AMA_E_ARV.", sailing.JX_JB_E_ARV, header.AMA_E_ARV);
				AssertEquals("Should keep the value of AMA_OA_Carrier.", carrier.MainAddress.PK, header.AMA_OA_Carrier);
			});
		}
	}
}
