using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public class AddressDocWrapper : AddressWrapper
	{
		public AddressDocWrapper(OrganisationUsageType usageType, OrgAddress address, BusinessObjectFactory factory) : base(usageType, address, ContactType.NoContactType, factory)
		{
			SetupCompanyNameAndAddressInChinese();
		}

		public AddressDocWrapper(OrganisationUsageType usageType, JobDocAddress docAddress, BusinessObjectFactory factory) : base(usageType, docAddress, factory)
		{
			SetupCompanyNameAndAddressInChinese();
		}

		void SetupCompanyNameAndAddressInChinese()
		{
			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.ChineseSimplified))
			{
				CompanyNameInChinese = CompanyName;
				CompanyNameAndAddressInChinese = CompanyNameAndAddress;
				AddressAsASingleLineInChinese = AddressAsASingleLine;
			}
		}

		public ZString CompanyNameInChinese { get; private set; }
		public ZString CompanyNameAndAddressInChinese { get; private set; }
		public ZString AddressAsASingleLineInChinese { get; private set; }
	}
}
