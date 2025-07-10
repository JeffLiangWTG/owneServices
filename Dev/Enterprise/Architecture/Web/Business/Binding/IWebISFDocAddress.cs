using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Web.Business
{
	public interface IWebISFDocAddress : IDocAddress
	{
		ZString E2_Contact { get; }
		ZString E2_SocialSecurityNumber { get; set; }
		ZDateTime E2_SocialSecurityNumberDateOfBirth { get; set; }
		ZBool IsSocialSecurityNumberGovRegNumType { get; }
		ZPropertyInfo E2_SocialSecurityNumberInfo { get; }
		ZPropertyInfo E2_SocialSecurityNumberDateOfBirthInfo { get; }
	}
}