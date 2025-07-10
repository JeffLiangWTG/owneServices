using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class OrganisationTest : TestCaseWithFactory
	{
		public void TestOrganisationSerialisation()
		{
			var organisation = new Organisation(RoleType.Buyer);
			organisation.CompanyName = "(주)동부제철";
			organisation.AddressLine1 = "서울시 강남구 논현동";
			organisation.BuildingNumber = "12";
			organisation.RepresentativeName = "이상규";
			var idNumbers = new IDNumberAndType[]
			{
				new IDNumberAndType()
				{
					Type = IdentificationType.ForeignCompanyID,
					Number = "1111",
					CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth
				}
			};

			organisation.SetRegistrationIDNumbers(idNumbers);

			Stream stream = null;
			try
			{
				AssertNoExceptionThrown("Organisation serialised without problems", () => stream = KRXmlObjectSerializer.Serialize(organisation));
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();
				AssertContains(@"<ForeignCompanyID>1111</ForeignCompanyID>", serialisedXml);
				AssertContains("<CompanyName>(주)동부제철</CompanyName>", serialisedXml);
				AssertContains("<AddressLine1>서울시 강남구 논현동</AddressLine1>", serialisedXml);
				AssertContains("<BuildingNumber>12</BuildingNumber>", serialisedXml);
				AssertContains("<RepresentativeName>이상규</RepresentativeName>", serialisedXml);
			}
			finally
			{
				stream.Dispose();
			}
		}

		public void TestOrganisationIdentificationTypeAttributes()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "레디코리아 Test";
			supplier.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(IdentificationType.RoadNameCode, "RN1", Core.Constants.CountryCodes.KoreaSouth);
			supplier.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(IdentificationType.BuildingNumber, "B1", Core.Constants.CountryCodes.KoreaSouth);
			supplier.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(IdentificationType.OfficeID, "O1", Core.Constants.CountryCodes.KoreaSouth);
			supplier.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(IdentificationType.ControlledPremisesID, "C1", Core.Constants.CountryCodes.KoreaSouth);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(IdentificationType.BusinessRegNo, "B1", Core.Constants.CountryCodes.KoreaSouth);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(IdentificationType.UnipassIDForOrganization, "U1", Core.Constants.CountryCodes.KoreaSouth);

			var organisation = new Organisation(RoleType.Supplier);

			var idNumbers = new IDNumberAndType[]
			{
				new IDNumberAndType()
				{
					Type = IdentificationType.RoadNameCode,
					Number = "RN1",
					CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth
				},
				new IDNumberAndType()
				{
					Type = IdentificationType.BuildingNumber,
					Number = "B1",
					CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth
				},
				new IDNumberAndType()
				{
					Type = IdentificationType.OfficeID,
					Number = "O1",
					CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth
				},
				new IDNumberAndType()
				{
					Type = IdentificationType.ControlledPremisesID,
					Number = "C1",
					CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth
				},
				new IDNumberAndType()
				{
					Type = OrgCusCode.CodeTypes.CarrierCode,
					Number = "C2",
					CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth
				},
				new IDNumberAndType()
				{
					Type = IdentificationType.BusinessRegNo,
					Number = "B1",
					CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth
				},
				new IDNumberAndType()
				{
					Type = IdentificationType.UnipassIDForOrganization,
					Number = "U1",
					CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth
				}
			};

			organisation.SetRegistrationIDNumbers(idNumbers);

			AssertEquals("Business rego number", "B1", organisation.BusinessRegNo);
			AssertEquals("UnipassIDForOrganization", "U1", organisation.UnipassIDForOrganization);
			AssertEquals("RoadNameCode", "RN1", organisation.RoadNameCode);
			AssertEquals("BuildingNumber", "B1", organisation.BuildingNumber);
			AssertEquals("OfficeID", "O1", organisation.OfficeID);
			AssertEquals("ControlledPremisesID", "C1", organisation.ControlledPremisesID);
			AssertEquals("CarrierCode", "C2", organisation.CarrierCode);
		}
	}
}
