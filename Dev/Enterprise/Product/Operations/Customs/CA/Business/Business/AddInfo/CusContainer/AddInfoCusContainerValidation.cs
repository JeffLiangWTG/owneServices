using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class AddInfoCusContainerValidation : CAAddInfoValidation
	{
		public AddInfoCusContainerValidation(AddInfoCusContainer cusContainerAddInfo)
			: base(cusContainerAddInfo)
		{
		}

		protected override void CheckCA_ContainerSizeOrISOCode()
		{
			base.CheckCA_ContainerSizeOrISOCode();
			if (!Parent.CA_ContainerSizeOrISOCode.IsEmpty)
			{
				ContainerISOType isoType = new ContainerISOType(Parent);
				isoType.ISOCode = Parent.CA_ContainerSizeOrISOCode;
				if (Parent.CA_ContainerSizeOrISOCode.Length < 4)
				{
					Parent.CA_ContainerSizeOrISOCodeInfo.AddMessageError(Res.GetString("6cf2b6aa-1a2b-41d0-9a41-feabcaa2ae84", "Container ISO Size must be four characters long."));
				}
				else
				{
					if (!isoType.IsKnown)
					{
						Parent.CA_ContainerSizeOrISOCodeInfo.AddMessageError(Res.GetString("fc6e5de8-76c9-4059-b094-da748e53b9d9", "Unknown ISO Code."));
					}
				}
			}
		}

		protected override void CheckCA_RN_NKCountryOfRegistration()
		{
			base.CheckCA_RN_NKCountryOfRegistration();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_RN_NKCountryOfRegistrationInfo, Parent.Lookups.CountryOfRegistrations);
		}
	}
}
