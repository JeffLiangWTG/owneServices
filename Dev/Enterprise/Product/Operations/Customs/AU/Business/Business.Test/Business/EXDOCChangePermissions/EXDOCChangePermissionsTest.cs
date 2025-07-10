using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCChangePermissionsTest : TestCaseWithFactory
	{
		public void TestEXDOCChangePermissionsElements()
		{
			eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, ZString.Empty, ".");//Used to load 'ChangePermissionsMatrix' into the CachedValue
			var changePermissionsMatrix = Factory.GetCachedValue<EXDOCChangePermissions.ChangePermissions>("EXDOCChangePermissionsMatrix", null);
			AssertEquals("28 data fields in matrix", 27, changePermissionsMatrix.Count);
			foreach (var statusPermissions in changePermissionsMatrix.Values)
			{
				Assert("At least one status permission", statusPermissions.Count > 0);
				Assert("No more than six status permissions", statusPermissions.Count <= 8);
				foreach (var statusPermission in statusPermissions.Values)
				{
					Assert("At least one product in list", statusPermission.Length > 0);
					Assert("No more than nine products in list", statusPermission.Length <= 9);
					Assert("All are valid products", statusPermission.KeepChars("DEFGHIMSW").Length == statusPermission.Length);
				}
			}
			var statusPermisssions = changePermissionsMatrix[EXDOCDataFields.RFPAuthorisationEstablishmentNumber];
			AssertEquals("3 status permisssions", 3, statusPermisssions.Count);
			AssertEquals("ORDR", "DEFGHIMS", statusPermisssions[EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder]);
			AssertEquals("INIT", "HG", statusPermisssions[EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial]);
			AssertEquals("FINL", "HG", statusPermisssions[EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal]);
			statusPermisssions = changePermissionsMatrix[EXDOCDataFields.AmendedInformation];
			AssertEquals("2 status permisssions", 2, statusPermisssions.Count);
			AssertEquals("INSP", "GH", statusPermisssions[EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected]);
			AssertEquals("HCDR", "GH", statusPermisssions[EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady]);
		}

		public void TestIsChangeAllowed_RFPAuthorisationEstablishmentNumber()
		{
			Assert("Order D is Allowed", eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "D", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder));
			Assert("Order E is Allowed", eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "E", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder));
			Assert("Order F is Allowed", eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "F", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder));
			Assert("Order G is Allowed", eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "G", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder));
			Assert("Order H is Allowed", eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "H", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder));
			Assert("Order I is Allowed", eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "I", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder));
			Assert("Order M is Allowed", eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "M", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder));
			Assert("Order S is Allowed", eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "S", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder));
			Assert("Order W is NOT Allowed", !eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "W", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder));

			Assert("INIT H is Allowed", eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "H", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial));
			Assert("INIT G is Allowed", eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "G", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial));
			Assert("INIT D is NOT Allowed", !eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "D", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial));
			Assert("INIT S is NOT Allowed", !eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "S", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial));

			Assert("FINL H is Allowed", eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "H", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal));
			Assert("FINL G is Allowed", eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "G", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal));
			Assert("FINL D is NOT Allowed", !eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "D", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal));
			Assert("FINL S is NOT Allowed", !eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "S", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal));

			Assert("INSP H is NOT Allowed", !eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "H", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected));
			Assert("HCRD H is NOT Allowed", !eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "H", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady));
			Assert("CTRD H is NOT Allowed", !eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "H", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CtrdCertificateReady));
			Assert("COMP H is NOT Allowed", !eXDOCChangePermissions.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber, "H", EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted));
		}

		public void TestIsChangeAllowed()
		{
			BruteForceAssertIsChangeAllowed(EXDOCDataFields.CommodityType, new ZString[] { EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal }, new ZString[] { "D", "E", "F", "G", "H", "I", "M", "S", "W" });
			BruteForceAssertIsChangeAllowed(EXDOCDataFields.DeclarationOfComplianceIndicator, new ZString[] { EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal }, new ZString[] { "D", "E", "F", "G", "H", "I", "M", "S", "W" });
			BruteForceAssertIsChangeAllowed(EXDOCDataFields.ImportedProductFlag, new ZString[] { EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal }, new ZString[] { "D", "F" });
			BruteForceAssertIsChangeAllowed(EXDOCDataFields.TrueAndCompleteIndicator, new ZString[] { EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial, EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal }, new ZString[] { "D", "E", "F", "G", "H", "I", "M", "S", "W" });
		}

		protected override void SetUp()
		{
			base.SetUp();
			eXDOCChangePermissions = new EXDOCChangePermissions(Factory);
		}
		EXDOCChangePermissions eXDOCChangePermissions;

		void BruteForceAssertIsChangeAllowed(EXDOCDataFields datafield, ZString[] complianceStatusCodes, ZString[] expectedAllowedCommodities) //Only Applicable When All ComplianceCodes have the same list of comoddities i.e "DEFG,DEFG,DEFG"
		{
			var unspecifiedComplianceStatusCodes = new EXDOCComplianceStatusCodesForCusEntryNumber().GetAllCodesZString().Where(x => !complianceStatusCodes.Contains(x));
			var allCommodities = new EXDOCCommodityCodesSingleChar().GetAllCodesZString();
			var notAllowedCommodities = allCommodities.Where(x => !expectedAllowedCommodities.Contains(x));

			CombineAssertions(datafield.ToString(), () =>
			{
				foreach (var comStatusCode in complianceStatusCodes)
				{
					foreach (var allowedCommodity in expectedAllowedCommodities)
					{
						Assert(string.Concat(comStatusCode, "-", allowedCommodity, " should be Allowed"), eXDOCChangePermissions.IsChangeAllowed(datafield, allowedCommodity, comStatusCode));
					}

					foreach (var notAllowedCommodity in notAllowedCommodities)
					{
						Assert(string.Concat(comStatusCode, "-", notAllowedCommodity, " should NOT be Allowed"), !eXDOCChangePermissions.IsChangeAllowed(datafield, notAllowedCommodity, comStatusCode));
					}
				}

				foreach (var unspecifiedComStatusCode in unspecifiedComplianceStatusCodes)
				{
					foreach (var commodity in allCommodities)
					{
						Assert(string.Concat(unspecifiedComStatusCode, "-", commodity, " should NOT be Allowed"), !eXDOCChangePermissions.IsChangeAllowed(datafield, commodity, unspecifiedComStatusCode));
					}
				}
			});
		}
	}
}
