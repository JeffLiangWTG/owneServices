using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	public class CustomsReferenceDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestReadIntoDataRowCusReference()
		{
			var mainAddressPK = CreateTestOrgAddress(Factory);
			Factory.SaveForTesting();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var result = new CustomsReferenceDataObjectReader(CreateCustomsReferenceBOForCusReference("FIS", "FR1", "12345", TestOrganizationAddress), logger, entryInstruction.PK, entryInstruction.TablePrefix, Factory).ReadIntoDataRowCusReference();

				AssertNotNull("ResultBOs exists", result);
				CombineAssertions(() =>
				{
					AssertEquals("Code", "FR1", result.GetValue(CusReferenceSchema.CFR_Code));
					AssertEquals("Reference", "12345", result.GetValue(CusReferenceSchema.CFR_Reference));
					AssertEquals("Type", "FIS", result.GetValue(CusReferenceSchema.CFR_Type));
					AssertEquals("Parent PK", entryInstruction.PK, result.GetValue(CusReferenceSchema.CFR_ParentID));
					AssertEquals("Parent TableCode", entryInstruction.TablePrefix, result.GetValue(CusReferenceSchema.CFR_ParentTableCode));
					AssertEquals("Owner", mainAddressPK, result.GetValue(CusReferenceSchema.CFR_OA_Owner));
				});
			}
		}

		public void TestReadIntoDataRowCusReferenceOwner_Null()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var result = new CustomsReferenceDataObjectReader(CreateCustomsReferenceBOForCusReference("FIS", "FR1", "12345", null), logger, entryInstruction.PK, entryInstruction.TablePrefix, Factory).ReadIntoDataRowCusReference();

				AssertNotNull("ResultBOs exists", result);
				CombineAssertions(() =>
				{
					AssertEquals("Code", "FR1", result.GetValue(CusReferenceSchema.CFR_Code));
					AssertEquals("Reference", "12345", result.GetValue(CusReferenceSchema.CFR_Reference));
					AssertEquals("Type", "FIS", result.GetValue(CusReferenceSchema.CFR_Type));
					AssertEquals("Parent PK", entryInstruction.PK, result.GetValue(CusReferenceSchema.CFR_ParentID));
					AssertEquals("Parent TableCode", entryInstruction.TablePrefix, result.GetValue(CusReferenceSchema.CFR_ParentTableCode));
					AssertEquals("Owner", ZGuid.Empty, result.GetValue(CusReferenceSchema.CFR_OA_Owner));
				});
			}
		}

		public void TestReadIntoDataRowCusAuthorizationUsage()
		{
			var orgPK = CreateTestOrg(Factory);
			Factory.SaveForTesting();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var result = new CusAuthorizationUsageDataObjectReader(CreateCustomsReferenceBOForCusReference("AUT", "ABC", "12345", TestOrganizationAddress), logger, entryInstruction.PK, entryInstruction.TablePrefix, Factory).ReadIntoDataRowCusReference();

				AssertNotNull("ResultBOs exists", result);
				CombineAssertions(() =>
				{
					AssertEquals("Code", "ABC", result.GetValue(CusAuthorizationUsageSchema.AGC_Code));
					AssertEquals("Reference", "12345", result.GetValue(CusAuthorizationUsageSchema.AGC_Number));
					AssertEquals("Parent PK", entryInstruction.PK, result.GetValue(CusAuthorizationUsageSchema.AGC_ParentID));
					AssertEquals("Parent TableCode", entryInstruction.TablePrefix, result.GetValue(CusAuthorizationUsageSchema.AGC_ParentTableCode));
					AssertEquals("Owner", orgPK, result.GetValue(CusAuthorizationUsageSchema.AGC_OH_Owner));
				});
			}
		}

		public void TestReadIntoDataRowCusAuthorizationUsageOwner_Null()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var result = new CusAuthorizationUsageDataObjectReader(CreateCustomsReferenceBOForCusReference("AUT", "ABC", "12345", null), logger, entryInstruction.PK, entryInstruction.TablePrefix, Factory).ReadIntoDataRowCusReference();

				AssertNotNull("ResultBOs exists", result);
				CombineAssertions(() =>
				{
					AssertEquals("Code", "ABC", result.GetValue(CusAuthorizationUsageSchema.AGC_Code));
					AssertEquals("Reference", "12345", result.GetValue(CusAuthorizationUsageSchema.AGC_Number));
					AssertEquals("Parent PK", entryInstruction.PK, result.GetValue(CusAuthorizationUsageSchema.AGC_ParentID));
					AssertEquals("Parent TableCode", entryInstruction.TablePrefix, result.GetValue(CusAuthorizationUsageSchema.AGC_ParentTableCode));
					AssertEquals("Owner", OrgHeader.UnmatchedOrganisationPK, result.GetValue(CusAuthorizationUsageSchema.AGC_OH_Owner));
					AssertNoExceptionThrown("No Exception should be thrown when saving", () => Factory.SaveForTesting());
					AssertContains("Owner is missing", "There are not any owner details", logger.Logs);

					var auth = Factory.Load<CusAuthorizationUsage>(result.GetValue(CusAuthorizationUsageSchema.PK));
					AssertNotNull("Loaded Auth", auth);
					AssertEquals("Cluster Key", declaration.JE_ClusterKey, auth?.AGC_ClusterKey);
				});
			}
		}

		public void TestReadIntoDataRowCusAuthorizationUsageOwnerNotExists()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var result = new CusAuthorizationUsageDataObjectReader(CreateCustomsReferenceBOForCusReference("AUT", "ABC", "12345", TestOrganizationAddress), logger, entryInstruction.PK, entryInstruction.TablePrefix, Factory).ReadIntoDataRowCusReference();

				AssertNotNull("ResultBOs exists", result);
				CombineAssertions(() =>
				{
					AssertEquals("Code", "ABC", result.GetValue(CusAuthorizationUsageSchema.AGC_Code));
					AssertEquals("Reference", "12345", result.GetValue(CusAuthorizationUsageSchema.AGC_Number));
					AssertEquals("Parent PK", entryInstruction.PK, result.GetValue(CusAuthorizationUsageSchema.AGC_ParentID));
					AssertEquals("Parent TableCode", entryInstruction.TablePrefix, result.GetValue(CusAuthorizationUsageSchema.AGC_ParentTableCode));
					AssertNotEquals("Owner", ZGuid.Empty, result.GetValue(CusAuthorizationUsageSchema.AGC_OH_Owner));
					AssertNoExceptionThrown("No Exception should be thrown when saving", () => Factory.SaveForTesting());
					AssertContains("Owner cannot be matched", "could not be matched for Authorization", logger.Logs);

					var auth = Factory.Load<CusAuthorizationUsage>(result.GetValue(CusAuthorizationUsageSchema.PK));
					AssertNotNull("Loaded Auth", auth);
					AssertEquals("Cluster Key", declaration.JE_ClusterKey, auth?.AGC_ClusterKey);
				});
			}
		}

		internal static UniversalCustoms.CustomsReference CreateCustomsReferenceBOForCusReference(string type, string subType, string reference, OrganizationAddress owner) => new UniversalCustoms.CustomsReference
		{
			Type = new CodeDescriptionPair() { Code = type },
			SubType = new CodeDescriptionPair35Char() { Code = subType },
			Reference = reference,
			Owner = owner
		};

		internal static OrganizationAddress TestOrganizationAddress => new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
		{
			Address1 = "Teststraße 1",
			CompanyName = "Test Company",
			AddressType = "Supplier"
		};

		internal static ZGuid CreateTestOrgAddress(UniversalObjectFactory factory)
		{
			var testOrg = CreateOrgWithAddress(factory);
			return testOrg.MainAddress.PK;
		}

		internal static ZGuid CreateTestOrg(UniversalObjectFactory factory)
		{
			var testOrg = CreateOrgWithAddress(factory);
			return testOrg.PK;
		}

		static OrgHeader CreateOrgWithAddress(UniversalObjectFactory factory)
		{
			var testOrg = factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = "Test Company";
			testOrg.OH_Code = "TESTORG";
			var mainAddress = testOrg.MainAddress;
			mainAddress.FillWithValidTestData();
			mainAddress.OA_Address1 = "Teststraße 1";
			return testOrg;
		}
	}
}
