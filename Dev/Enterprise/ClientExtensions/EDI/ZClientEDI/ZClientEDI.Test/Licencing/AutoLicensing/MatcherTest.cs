using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.Client.EDI.Licencing.AutoLicensing.Testing
{
	public class MatcherTest : TestCaseWithFactory
	{
		public void TestGetMatches()
		{
			var licDefault = BillingTestHelper.CreateLicence(Factory, "ENT");

			var licTarget = BillingTestHelper.CreateLicence(Factory, "CW1");
			licTarget.Database.LD_Product = ProductTypes.Codes.Sapphire;
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "ORG1";
			licTarget.Company.LC_OH = org1.PK;
			licTarget.Database.LD_OH_WebAccessOrg = org1.PK;

			var licTarget2 = BillingTestHelper.CreateLicence(Factory, "DDD");
			licTarget2.Database.LD_Product = ProductTypes.Codes.CargoWiseNext;
			licTarget2.Database.LD_LE = licTarget.Database.LD_LE;
			licTarget2.Company.LC_LE = licTarget.Company.LC_LE;
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_Code = "ORG2";
			licTarget2.Company.LC_OH = org2.PK;
			licTarget2.Database.LD_OH_WebAccessOrg = org2.PK;

			Factory.Save();

			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, licDefault.Database.EnterpriseID);

			var ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
			ld1.LD_OH_WebAccessOrg = ZGuid.Empty;
			ld1.LD_LE = licDefault.Database.LicEnterprise.PK;
			ld1.LD_MasterOrgSuggestedUTC = ZDateTime.Empty;
			var info = new LicenceDatabaseRegistrationAdditionalInfo();
			info.EnterpriseCode = licTarget.Database.EnterpriseCode;
			info.EnterpriseID = licTarget.Database.EnterpriseID;
			ld1.Notes.AddNew(false, EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description, info.ToString());

			Factory.Save();

			var matcher = new Matcher();
			var result = matcher.GetMatches(ld1).OrderBy(x => x.TotalScore).ThenBy(x => x.Org.OH_Code).ToList();
			var match1 = result[0];
			AssertEquals(4000, match1.TotalScore);
			AssertEquals(match1.Org.PK, org1.PK);
			var match2 = result[1];
			AssertEquals(4000, match2.TotalScore);
			AssertEquals(match2.Org.PK, org2.PK);

			ld1.LD_Product = ProductTypes.Codes.CargoSphere;
			result = matcher.GetMatches(ld1).OrderBy(x => x.TotalScore).ThenBy(x => x.Org.OH_Code).ToList();
			match1 = result[0];
			AssertEquals(4000, match1.TotalScore);
			AssertEquals(match1.Org.PK, org1.PK);
			match2 = result[1];
			AssertEquals(4100, match2.TotalScore);
			AssertEquals(match2.Org.PK, org2.PK);

			licTarget2.Database.LD_Product = ProductTypes.Codes.CargoWise;
			result = matcher.GetMatches(ld1).OrderBy(x => x.TotalScore).ThenBy(x => x.Org.OH_Code).ToList();
			match1 = result[0];
			AssertEquals(4000, match1.TotalScore);
			AssertEquals(match1.Org.PK, org1.PK);
			match2 = result[1];
			AssertEquals(4100, match2.TotalScore);
			AssertEquals(match2.Org.PK, org2.PK);
		}

		public void TestGetMatchesByAddress()
		{
			var licDefault = BillingTestHelper.CreateLicence(Factory, "ENT");
			var licTarget = BillingTestHelper.CreateLicence(Factory, "CW1");
			Factory.Save();
			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, licDefault.Database.EnterpriseID);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";

			var ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
			ld1.LD_OH_WebAccessOrg = ZGuid.Empty;
			ld1.LD_LE = licDefault.Database.LicEnterprise.PK;
			ld1.LD_MasterOrgSuggestedUTC = ZDateTime.Empty;
			var info = new LicenceDatabaseRegistrationAdditionalInfo();
			info.Address1 = "1 Apple Rd";
			ld1.Notes.AddNew(false, EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description, info.ToString());
			Factory.Save();

			var matcher = new MatcherForTest();
			DeduplicationOrgHeader header = null;
			matcher.Finder = (x) =>
			{
				header = x;
				var scores = new List<ScoringResult>();
				scores.Add(new ScoringResult() { TargetPK = org1.PK.ToGuid(), Score = 0.01 });
				return scores;
			};
			var result = matcher.GetMatches(ld1);
			var match = result.Single();
			AssertEquals(match.Org.PK, org1.PK);
			AssertEquals(1, match.TotalScore);

			AssertEquals("", header.OH_FullName);
			AssertEquals("", header.OH_Code);
			var address = header.OrgAddresses.Single();
			AssertEquals("", address.OA_RL_NKRelatedPortCode);
			AssertEquals("", address.OA_RN_NKCountryCode);
			AssertEquals("", address.OA_AdditionalAddressInformation);
			AssertEquals("1 Apple Rd", address.OA_Address1);
			AssertEquals("", address.OA_Address2);
			AssertEquals("", address.OA_City);
			AssertEquals("", address.OA_Code);
			AssertEquals("", address.OA_PostCode);
			AssertEquals("", address.OA_State);
			AssertEquals("", address.OA_Email);
			AssertEquals("", address.OA_Fax);
			AssertEquals("", address.OA_Phone);
			AssertEquals("", address.OA_Mobile);
			AssertEquals("", address.OA_CompanyNameOverride);
			AssertEquals("NTC", address.OA_ValidationStatus);

			var info2 = new LicenceDatabaseRegistrationAdditionalInfo();
			info2.OrgName = "Org Name1";
			info2.OrgCountry = "AU";
			info2.Address1 = "1 Apple Rd";
			info2.Address2 = "2 River Rd";
			info2.City = "Sydney";
			info2.Postcode = "2000";
			info2.State = "NSW";
			ld1.Notes.AddNew(false, EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description, info2.ToString());
			Factory.Save();

			header = null;
			result = matcher.GetMatches(ld1);
			match = result.Single();
			AssertEquals(match.Org.PK, org1.PK);
			AssertEquals(1, match.TotalScore);
			AssertEquals("Org Name1", header.OH_FullName);
			AssertEquals("", header.OH_Code);
			address = header.OrgAddresses.Single();
			AssertEquals("", address.OA_RL_NKRelatedPortCode);
			AssertEquals("AU", address.OA_RN_NKCountryCode);
			AssertEquals("", address.OA_AdditionalAddressInformation);
			AssertEquals("1 Apple Rd", address.OA_Address1);
			AssertEquals("2 River Rd", address.OA_Address2);
			AssertEquals("Sydney", address.OA_City);
			AssertEquals("", address.OA_Code);
			AssertEquals("2000", address.OA_PostCode);
			AssertEquals("NSW", address.OA_State);
			AssertEquals("", address.OA_Email);
			AssertEquals("", address.OA_Fax);
			AssertEquals("", address.OA_Phone);
			AssertEquals("", address.OA_Mobile);
			AssertEquals("Org Name1", address.OA_CompanyNameOverride);
			AssertEquals("NTC", address.OA_ValidationStatus);
		}

		class MatcherForTest : Matcher
		{
			protected override IReadOnlyList<ScoringResult> FindPotentialDuplicates(DeduplicationOrgHeader header) => Finder(header);
			public Func<DeduplicationOrgHeader, IReadOnlyList<ScoringResult>> Finder { get; set; }
		}
	}
}
