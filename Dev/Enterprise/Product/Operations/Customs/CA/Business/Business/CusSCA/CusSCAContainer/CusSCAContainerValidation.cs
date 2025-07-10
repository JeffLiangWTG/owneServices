using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAContainerValidation : Customs.Business.CusSCAContainerValidation
	{
		public CusSCAContainerValidation(CusSCAContainer container)
			: base(container)
		{
		}

		protected new CusSCAContainer Parent
		{
			get { return (CusSCAContainer)base.Parent; }
		}

		protected override void CheckCN_ContainerMode()
		{
			base.CheckCN_ContainerMode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CN_ContainerModeInfo);
			if (Parent.IsNonContainerised)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CN_ContainerModeInfo, Parent.Lookups.SCRNonContainerModes, ResString.GetMultilingualString("01f7139a-e03f-4537-8df4-afb3b26f1a94", "Please enter a Non-containerized mode (AIR or NCT)."));
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CN_ContainerModeInfo, Parent.Lookups.SCRContainerisedModes, ResString.GetMultilingualString("575eb2fe-cea8-4e96-aebb-7744675f8373", "Please enter a Containerized mode (CNT or EMP)."));
			}
		}

		protected override void CheckCN_RC_NKContainerType()
		{
			base.CheckCN_RC_NKContainerType();
			if (!Parent.IsNonContainerised)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CN_RC_NKContainerTypeInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.CN_RC_NKContainerTypeInfo);
			}
		}

		protected override void CheckCN_RN_NKCountryOfRegistration()
		{
			base.CheckCN_RN_NKCountryOfRegistration();
			if (!Parent.IsNonContainerised)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CN_RN_NKCountryOfRegistrationInfo);
			}
		}

		protected override void CheckCN_ContainerSizeOrISOCode()
		{
			base.CheckCN_ContainerSizeOrISOCode();
			if (!Parent.IsNonContainerised && !Parent.CN_ContainerSizeOrISOCode.IsEmpty)
			{
				var isoType = new ContainerISOType(Parent);
				isoType.ISOCode = Parent.CN_ContainerSizeOrISOCode;
				if (Parent.CN_ContainerSizeOrISOCode.Length < 4)
				{
					Parent.CN_ContainerSizeOrISOCodeInfo.AddMessageError(Res.GetString("CE243E4E-AB55-42fe-A6EB-5F9AB67A4DCB", "Container ISO Size must be four characters long."));
				}
				else
				{
					if (!isoType.IsKnown)
					{
						Parent.CN_ContainerSizeOrISOCodeInfo.AddMessageError(Res.GetString("B5726366-42EC-43cf-BE8A-EAFEDCE6C882", "Unknown ISO Code."));
					}
				}
			}
		}
	}
}
