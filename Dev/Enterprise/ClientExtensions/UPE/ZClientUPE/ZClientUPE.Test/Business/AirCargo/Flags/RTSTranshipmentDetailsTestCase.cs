using CargoWise.ComponentModel;
using Enterprise.Customs.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal abstract class RTSTranshipmentDetailsTestCase : UPECusHAWBFlagDetailsTestCase
	{
		public void TestOriginPort()
		{
			FlagDetails.OriginPort = "";
			AssertMandatoryValidationError(FlagDetails.OriginPortInfo, true);
			FlagDetails.OriginPort = "10298";
			AssertMandatoryValidationError(FlagDetails.OriginPortInfo, false);
			AssertListValidationInvalidCodeError(FlagDetails.OriginPortInfo, true);
			FlagDetails.OriginPort = "AUSYD";
			AssertListValidationInvalidCodeError(FlagDetails.OriginPortInfo, false);
			AssertEquals(0, FlagDetails.OriginPortInfo.GetErrors().Count());
		}

		public void TestDestinationPort()
		{
			FlagDetails.DestinationPort = "";
			AssertMandatoryValidationError(FlagDetails.DestinationPortInfo, true);
			FlagDetails.DestinationPort = "10298";
			AssertMandatoryValidationError(FlagDetails.DestinationPortInfo, false);
			AssertListValidationInvalidCodeError(FlagDetails.DestinationPortInfo, true);
			FlagDetails.DestinationPort = "AUSYD";
			AssertListValidationInvalidCodeError(FlagDetails.DestinationPortInfo, false);
			AssertEquals(0, FlagDetails.DestinationPortInfo.GetErrors().Count());
		}

		public void TestName()
		{
			FlagDetails.Name = "";
			AssertMandatoryValidationError(FlagDetails.NameInfo, true);
			FlagDetails.Name = "MEH";
			AssertMandatoryValidationError(FlagDetails.NameInfo, false);
			AssertEquals(0, FlagDetails.NameInfo.GetErrors().Count());
		}

		public void TestStreet()
		{
			FlagDetails.Street = "";
			AssertMandatoryValidationError(FlagDetails.StreetInfo, true);
			FlagDetails.Street = "MEH";
			AssertMandatoryValidationError(FlagDetails.StreetInfo, false);
			AssertEquals(0, FlagDetails.StreetInfo.GetErrors().Count());
		}

		public void TestCity()
		{
			FlagDetails.City = "";
			AssertMandatoryValidationError(FlagDetails.CityInfo, true);
			FlagDetails.City = "MEH";
			AssertMandatoryValidationError(FlagDetails.CityInfo, false);
			AssertEquals(0, FlagDetails.CityInfo.GetErrors().Count());
		}

		public void TestCountry()
		{
			FlagDetails.Country = "";
			AssertMandatoryValidationError(FlagDetails.CountryInfo, true);
			FlagDetails.Country = "__";
			AssertMandatoryValidationError(FlagDetails.CountryInfo, false);
			AssertListValidationInvalidCodeError(FlagDetails.CountryInfo, true);
			FlagDetails.Country = Core.Constants.CountryCodes.Australia;
			AssertListValidationInvalidCodeError(FlagDetails.CountryInfo, false);
			AssertEquals(0, FlagDetails.CountryInfo.GetErrors().Count());
		}

		public void TestPostCode()
		{
			FlagDetails.PostCode = "";
			AssertMandatoryValidationError(FlagDetails.PostCodeInfo, true);
			FlagDetails.PostCode = "MEH";
			AssertMandatoryValidationError(FlagDetails.PostCodeInfo, false);
			AssertEquals(0, FlagDetails.PostCodeInfo.GetErrors().Count());
		}

		public void TestUpdateHAWBDetails()
		{
			FlagDetails.UpdateCusHAWBDetails();
			AssertEquals("USCOL", UPECusHAWB.CS_RL_NKOrigin);
			AssertEquals("AUMEL", UPECusHAWB.CS_RL_NKDestination);
			AssertEquals("JOohn", UPECusHAWB.CS_ConsigneeName);
			AssertEquals("Botany Street", UPECusHAWB.CS_ConsigneeStreet);
			AssertEquals("Mural Lane", UPECusHAWB.CS_ConsigneeStreet2);
			AssertEquals("SYDNEY", UPECusHAWB.CS_ConsigneeCity);
			AssertEquals("NSW", UPECusHAWB.CS_ConsigneeState);
			AssertEquals("AU", UPECusHAWB.CS_RN_NKConsigneeCountry);
			AssertEquals("2100", UPECusHAWB.CS_ConsigneePostcode);
		}

		public void TestValidateAll()
		{
			FlagDetails.OriginPort = "";
			FlagDetails.DestinationPort = "";
			FlagDetails.Name = "";
			FlagDetails.Street = "";
			FlagDetails.Street2 = "";
			FlagDetails.City = "";
			FlagDetails.Country = "";
			FlagDetails.PostCode = "";
			FlagDetails.ValidateAll();
			AssertEquals("should have errors now", 9, FlagDetails.Notifications.GetErrors().Count());
		}

		public abstract void TestPopulatePropertiesInConstructor();
		protected override void SetUp()
		{
			base.SetUp();
			UPECusHAWB.CS_RL_NKOrigin = "AUSYD";
			UPECusHAWB.CS_RL_NKDestination = "ITMIL";
			FlagDetails.OriginPort = "USCOL";
			FlagDetails.DestinationPort = "AUMEL";
			FlagDetails.Name = "JOohn";
			FlagDetails.Street = "Botany Street";
			FlagDetails.Street2 = "Mural Lane";
			FlagDetails.City = "SYDNEY";
			FlagDetails.State = "NSW";
			FlagDetails.Country = "AU";
			FlagDetails.PostCode = "2100";
		}

		protected new RTSTranshipmentDetails FlagDetails
		{
			get
			{
				return (RTSTranshipmentDetails)base.FlagDetails;
			}
		}

		protected override sealed UPECusHAWBFlagDetails GetNewUPECusHAWBFlagDetails(UPECusHAWB uPECusHAWB)
		{
			return GetNewRTSTranshipmentDetails(uPECusHAWB);
		}

		protected abstract RTSTranshipmentDetails GetNewRTSTranshipmentDetails(UPECusHAWB uPECusHAWB);
	}
}
