using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ExportAgentOrganisationWrapper : OrganisationWrapper
	{
		public ExportAgentOrganisationWrapper(OrganisationUsageType usageType, OrgHeader organisation, ContactType mainContactType, ZString transportMode, BusinessObjectFactory factory)
			: base(usageType, organisation, mainContactType, transportMode, factory)
		{
		}

		public ExportAgentOrganisationWrapper(OrganisationUsageType usageType, OrgAddress address, ContactType mainContactType, ZString transportMode, BusinessObjectFactory factory)
			: base(usageType, address, mainContactType, transportMode, factory)
		{
		}

		public ExportAgentOrganisationWrapper(OrganisationUsageType usageType, OrgHeader organisation, ContactType mainContactType, BusinessObjectFactory factory)
			: base(usageType, organisation, mainContactType, factory)
		{
		}

		public ExportAgentOrganisationWrapper(OrganisationUsageType usageType, OrgAddress address, ContactType mainContactType, BusinessObjectFactory factory)
			: base(usageType, address, mainContactType, factory)
		{
		}

		public ExportAgentOrganisationWrapper(OrganisationUsageType usageType, JobDocAddress docAddress, BusinessObjectFactory factory)
			: base(usageType, docAddress, factory)
		{
		}

		internal ExportAgentOrganisationWrapper(OrganisationUsageType usageType, OrganisationWrapper wrapperBeingCloned)
			: base(usageType, wrapperBeingCloned)
		{
		}

		public new ZString CompanyNameAndAddress
		{
			get
			{
				ZString result;

				if (base.WrappedAddress == null || base.OrganisationBO == null)
				{
					result = base.CompanyNameAndAddress;
				}
				else
				{
					var codes = GenerateCustomCodesString();

					result = CompanyName;

					if (!string.IsNullOrEmpty(codes))
					{
						result += System.Environment.NewLine + codes;
					}

					if (!base.WrappedAddress.Address.IsEmpty)
					{
						result += System.Environment.NewLine + base.WrappedAddress.Address;
					}
				}

				return result;
			}
		}

		protected virtual string GenerateCustomCodesString()
		{
			var result = string.Empty;

			var fMCcode = base.OrganisationBO.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FederalMaritimeCommissionNumber);
			var cHBcode = base.OrganisationBO.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.CustomsHouseBrokerLicenseNumber);

			if (!fMCcode.IsEmpty && !cHBcode.IsEmpty)
			{
				result = Res.GetString("4e3cce84-f28b-4574-a7cf-8f9825ba68fb", "FMC NO. {0} / CHB NO. {1}", fMCcode, cHBcode);
			}
			else if (!fMCcode.IsEmpty)
			{
				result = Res.GetString("de5b8abf-ab0c-442a-9af7-6a6739840284", "FMC NO. {0}", fMCcode);
			}
			else if (!cHBcode.IsEmpty)
			{
				result = Res.GetString("d509bf70-8955-4e53-be9e-07d9fce65805", "CHB NO. {0}", cHBcode);
			}

			return result;
		}
	}
}
