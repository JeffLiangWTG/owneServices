
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("RegistrationNumberOrCode")]
	public class RegistrationNumberCodeWrapper : GenericWrapper
	{
		public RegistrationNumberCodeWrapper(OrgCusCode customsCodeBO, BusinessObjectFactory factory)
			: base(customsCodeBO, factory)
		{
			CustomsCodeBO = customsCodeBO ?? factory.GetNull<OrgCusCode>();
		}
		readonly OrgCusCode CustomsCodeBO;

		public RegistrationNumberCodeWrapper(ZString type, ZString code, BusinessObjectFactory factory)
			: base(factory.GetNull<OrgCusCode>(), factory)
		{
			fType = new CodeAndDescriptionWrapper(type, new CodeDescriptionPairList(), factory);
			registrationNumberOrCode = code;
		}

		public CodeAndDescriptionWrapper Type
		{
			get { return fType ?? (fType = new CodeAndDescriptionWrapper(CustomsCodeBO.OK_CodeType, CustomsCodeBO.Lookups.OK_CodeType_List, Factory)); }
		}
		CodeAndDescriptionWrapper fType;

		public CountryWrapper CountryOfIssue
		{
			get { return fCountryOfIssue ?? (fCountryOfIssue = new CountryWrapper(CustomsCodeBO.CodeCountry, Factory)); }
		}
		CountryWrapper fCountryOfIssue;

		public AddressWrapper PremisesAddress
		{
			get { return fPremisesAddress ?? (fPremisesAddress = new AddressWrapper(CustomsCodeBO.OK_OA_PremisesAddress.IsEmpty ? null : Factory.Load<OrgAddress>(CustomsCodeBO.OK_OA_PremisesAddress), ContactType.All, Factory)); }
		}
		AddressWrapper fPremisesAddress;

		public ZString RegistrationNumberOrCode
		{
			get
			{
				if (registrationNumberOrCode == ZString.Empty)
				{
					registrationNumberOrCode = CustomsCodeBO == null ? ZString.Empty : CustomsCodeBO.OK_CustomsRegNo;
				}

				return registrationNumberOrCode;
			}
		}
		ZString registrationNumberOrCode;
	}
}
