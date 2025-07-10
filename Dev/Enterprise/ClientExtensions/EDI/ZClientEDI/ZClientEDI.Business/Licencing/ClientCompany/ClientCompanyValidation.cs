//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientCompanyValidation
//
//    This class should be used for overriding validation in AutoClientCompanyValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Licencing.Business
{
	using System.Linq;

	/// <summary>
	/// NOTE: Validation is disabled by default. See ClientCompany.IsValidationEnabledCore.
	/// </summary>
	public class ClientCompanyValidation : AutoClientCompanyValidation
	{
		public ClientCompanyValidation(AutoClientCompany parent) : base(parent)
		{
		}

		protected override void CheckLCC_OH()
		{
			base.CheckLCC_OH();

			var parent = (ClientCompany)Parent;
			if (parent.LCC_OHInfo.HasChanges && !parent.LCC_OH.IsEmpty && parent.Org != null)
			{
				if (parent.Database.ClientCompanies.Cast<ClientCompany>()
					.Where(x => x.PK != parent.PK)
					.Any(x => x.LCC_OH == parent.LCC_OH))
				{
					parent.LCC_OHInfo.AddError("Organization is already set on another company on the database.");
				}
				else if (parent.Org.LicCompany == null)
				{
					parent.LCC_OHInfo.AddError("Organization does not have a licence.");
				}
				else if (parent.Org.LicCompany.LC_LE != parent.Database.LD_LE)
				{
					parent.LCC_OHInfo.AddError("Organization enterprise " + parent.Org.LicCompany.LicEnterprise.LE_EnterpriseCode + " does not match database enterprise " + parent.Database.LicEnterprise.LE_EnterpriseCode + ".");
				}
			}
		}
	}
}

