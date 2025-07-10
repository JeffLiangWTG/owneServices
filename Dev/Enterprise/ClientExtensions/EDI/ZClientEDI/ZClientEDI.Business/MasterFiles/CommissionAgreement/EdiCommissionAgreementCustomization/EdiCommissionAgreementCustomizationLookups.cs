//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiCommissionAgreementCustomizationLookups
//
//    This class should be used for overriding collections in AutoEdiCommissionAgreementCustomizationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCustomizationLookups : AutoEdiCommissionAgreementCustomizationLookups
	{
		public EdiCommissionAgreementCustomizationLookups(AutoEdiCommissionAgreementCustomization parent) : base(parent)
		{
		}

		protected new EdiCommissionAgreementCustomization Parent
		{
			get { return (EdiCommissionAgreementCustomization)base.Parent; }
		}

		#region Countries

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion

		#region Databases

		public BusinessObjectCollection Databases
		{
			get
			{
				var customer = Parent.Customer as EDIOrgHeader;
				if (customer == null || customer.LicCompany == null || customer.LicCompany.LicEnterprise == null)
				{
					return new LicenceDatabaseNonDependentCollection(Factory, ZQuery.NoResultQuery);
				}
				else
				{
					return new LicenceDatabaseCollection(customer.LicCompany.LicEnterprise, Factory);
				}
			}
		}

		#endregion
	}
}

