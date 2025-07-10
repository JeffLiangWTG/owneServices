using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Matching;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.UniversalDataBuss.Matching.Testing.ModuleMatcherTest;
using DummyBizo = Enterprise.ZArchitecture.Business.Testing.DummyBizOWithAutoLogs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.UniversalDataBuss.Matching.Testing
{
	public class OrganisationMatchUseDeduplicationTest : TestCaseWithFactory
	{
		public void TestOrganisationMatchUseDeduplication()
		{
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory = new UniversalObjectFactory();

				var org = OrgSetUp(factory);
				var address = OrganizationAddressDataSetUp();

				var dummyBOMatch = factory.New<DummyBizo>();
				dummyBOMatch.HumanReadableNameForTest = "Dummy BO X00001234";
				dummyBOMatch.Z0_Description = "RER321789";
				dummyBOMatch.Z0_Guid = org.PK;

				var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BookingConfirmationReference = "RER321789",
				};
				shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address });
				factory.SaveForTesting();

				var matcher = new ModuleMatcher<IShipmentDataObjectReader, DummyBizo>(factory);
				matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.BookingConfirmationReference, DummyBizoSchema.Z0_Description, MatchableOrganizationType.ConsigneeDocumentaryAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90 });

				var matchResult = matcher.GetBestMatch(new DummyReader(shipment));
				Assert(matchResult.Success);
			}
		}

		OrganizationAddress OrganizationAddressDataSetUp()
		{
			return new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MatchableOrganizationType.ConsigneeDocumentaryAddress),
				Address1 = "HEIDEMANNSTASSE 164",
				City = "MUENCHEN",
				CompanyName = "BMW",
				Country = new Country
				{
					Code = "DE"
				},
				Phone = "02123456789",
				Postcode = "80939",
				State = "BY",
			};
		}

		IOrgHeader OrgSetUp(UniversalObjectFactory factory)
		{
			var org = factory.BOFactory.New<OrgHeader>();
			org.OH_FullName = "BMW";
			org.OH_Category = OrgConstants.Category.Business;
			org.OH_Language = "EN";
			org.OH_Code = "BMWMUC1";
			org.MainAddress.OA_Address1 = "HEIDEMANNSTASSE 164";
			org.MainAddress.OA_City = "MUENCHEN";
			org.MainAddress.OA_State = "BY";
			org.MainAddress.OA_PostCode = "80939";
			org.MainAddress.OA_Phone = "02123456789";
			org.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Germany;
			org.MainAddress.OA_Language = "EN";

			org.OrganisationTypes = OrganisationTypes.Consignor;

			var addressToHash = org.MainAddress.OA_Address1 + org.MainAddress.OA_City + org.MainAddress.OA_State + org.MainAddress.OA_PostCode;
			var patternMatchingAddress = factory.New<PatternMatchingAddress>();
			patternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(addressToHash);
			patternMatchingAddress.PMA_OH = org.PK;
			patternMatchingAddress.PMA_ParentId = org.MainAddress.PK;
			patternMatchingAddress.PMA_RN_NKCountryCode = Constants.CountryCodes.Germany;
			patternMatchingAddress.PMA_ParentTableCode = OrgAddressSchema.Constants.Prefix;

			var patternMatchingName = factory.New<PatternMatchingName>();
			patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(org.OH_FullName);
			patternMatchingName.PMN_OH = org.PK;
			patternMatchingName.PMN_ParentId = org.PK;
			patternMatchingName.PMN_RN_NKCountryCode = Constants.CountryCodes.Germany;
			patternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;

			var patternMatchingPhone = factory.New<PatternMatchingPhone>();
			patternMatchingPhone.PMP_HashedValue = TextStandardizerHelper.ComputeStringHashFast(org.MainAddress.OA_Phone);
			patternMatchingPhone.PMP_OH = org.PK;
			patternMatchingPhone.PMP_ParentId = org.MainAddress.PK;
			patternMatchingPhone.PMP_RN_NKCountryCode = Constants.CountryCodes.Germany;
			patternMatchingPhone.PMP_ParentTableCode = OrgAddressSchema.Constants.Prefix;

			factory.SaveForTesting();

			return org;
		}
	}
}
