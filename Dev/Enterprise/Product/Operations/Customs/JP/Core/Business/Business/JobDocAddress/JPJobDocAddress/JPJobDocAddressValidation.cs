using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Business
{
	public class JPJobDocAddressValidation(AutoJobDocAddress parent) : JobDocAddressValidation(parent)
	{
		public JPJobDocAddress JobDocAddress => Parent as JPJobDocAddress;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateLocationCode();
		}

		public void ValidateLocationCode()
		{
			ValidateCalculatedProperty(JobDocAddress.LocationCodeInfo);
		}

		protected void CheckLocationCode()
		{
			var jobDocAddress = JobDocAddress;
			if (jobDocAddress.E2_AddressOverride)
			{
				CustomsRegistrationNumberValidation.ValidateCustomsCode(NotificationType.MessageError, OrgCusCode.JapanCodeTypes.AAL, jobDocAddress.LocationCode, jobDocAddress.LocationCodeInfo);
			}
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();

			var parent = Parent;
			CheckEnglishAddress(parent);

			var orgCusCodeCollection = parent.Address?.CustomsCodes;
			switch (parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.CustomsDepotAddress:
					if (orgCusCodeCollection != null && !orgCusCodeCollection.Any(x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Japan && x.OK_CodeType == OrgCusCode.CodeTypes.ControlledPremisesID))
					{
						parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("AE2C763C-BFF4-46D7-B8F9-B29E50F66666", "The selected Customs Depot address does not have a Customs Controlled Premises Code. To add one, press F3 to visit the organization, go to Details > Config > Registration Numbers and add a JP-CCP code with the selected Customs Depot address as the premises address."));
					}
					break;
				case DocAddressTypes.Codes.CustomsWarehouseAddress:
					if (orgCusCodeCollection != null)
					{
						var code = orgCusCodeCollection.FirstOrDefault(x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Japan && x.OK_CodeType == OrgCusCode.CodeTypes.ControlledPremisesID);
						if (code == null)
						{
							parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("AE2C763C-BFF4-46D7-B8F9-B29E50F99999", "The selected Bonded Warehouse address does not have a Customs Controlled Premises Code. To add one, press F3 to visit the organization, go to Details > Config > Registration Numbers and add a JP-CCP code with the selected Bonded Warehouse address as the premises address."));
						}
						else
						{
							var declaration = parent.Parent as JobDeclaration;
							if (declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.CEI_BondedLocationCode != code.OK_CustomsRegNo))
							{
								Parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("5C8C0822-3F6F-4381-8AFE-5384880D50BD", "The Bonded Warehouse organization CCP location code does not match the value entered."));
							}
						}
					}
					break;
				case DocAddressTypes.Codes.InspectionWitness:
				case DocAddressTypes.Codes.ExternalBroker:
				case DocAddressTypes.Codes.Forwarder:
					CheckNACCSUserCode();
					break;
				case DocAddressTypes.Codes.AirCargoAgent:
					CheckNACCSUserCode();
					CheckLocationCode();
					break;
			}

			void CheckNACCSUserCode()
			{
				if (orgCusCodeCollection != null && !orgCusCodeCollection.Any(x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Japan && x.OK_CodeType == OrgCusCode.JapanCodeTypes.NUC))
				{
					parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("1C69D47B-822C-4509-97D5-BD430C830F9A", "The selected {0} address does not have a NACCS User Code. To add one, press F3 to visit the organization, go to Details > Config > Registration Number / Codes and add a new row where Country/Region of Issue is JP, Type is NUC, and Premises Address is the selected {0} address.", parent.AddressDescription));
				}
			}

			void CheckLocationCode()
			{
				if (orgCusCodeCollection != null && !orgCusCodeCollection.Any(x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Japan && x.OK_CodeType == OrgCusCode.JapanCodeTypes.AAL))
				{
					parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("6F05A47B-32A4-4C88-AB5D-BE08A2C58210", "The selected {0} address does not have a Location Code. To add one, press F3 to visit the organization, go to Details > Config > Registration Number / Codes and add a new row where Country/Region of Issue is JP, Type is AAL, and Premises Address is the selected {0} address.", parent.AddressDescription));
				}
			}
		}

		void CheckEnglishAddress(JobDocAddress docAddress)
		{
			if (docAddress.Address is OrgAddress address && !address.IsEnglish)
			{
				docAddress.E2_OA_AddressInfo.AddMessageError(Res.GetString("A962434E-D219-430C-BD5E-AF1F8095D4FA", "NACCS only accepts addresses in English. The entered address is not in English. Please override it to English or add an English translation to the address."));
			}
		}

		protected override void CheckE2_GovRegNumType()
		{
			base.CheckE2_GovRegNumType();

			ListValidation.MessageErrorIfInvalidCode(Parent.E2_GovRegNumTypeInfo);
		}

		protected override void CheckE2_GovRegNum()
		{
			base.CheckE2_GovRegNum();

			var parent = Parent;
			var customsRegNoInfo = parent.E2_GovRegNumInfo;

			MandatoryValidation.MessageErrorIfNotEntered(customsRegNoInfo);
			CustomsRegistrationNumberValidation.ValidateCustomsCode(NotificationType.MessageError, parent.E2_GovRegNumType, parent.E2_GovRegNum, customsRegNoInfo);
		}

		protected override void CheckE2_CompanyName()
		{
			base.CheckE2_CompanyName();
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.E2_CompanyNameInfo);
		}

		protected override void CheckE2_AdditionalAddressInformation()
		{
			base.CheckE2_AdditionalAddressInformation();
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.E2_AdditionalAddressInformationInfo);
		}

		protected override void CheckE2_Address1()
		{
			base.CheckE2_Address1();
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.E2_Address1Info);
		}

		protected override void CheckE2_Address2()
		{
			base.CheckE2_Address2();
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.E2_Address2Info);
		}
	}
}
