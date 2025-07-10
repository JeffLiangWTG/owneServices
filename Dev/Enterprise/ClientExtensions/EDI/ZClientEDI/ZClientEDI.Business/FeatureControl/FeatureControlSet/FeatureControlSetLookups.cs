//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoFeatureControlSetLookups
//
//    This class should be used for overriding collections in AutoFeatureControlSetLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public class FeatureControlSetLookups : AutoFeatureControlSetLookups
	{
		public FeatureControlSetLookups(AutoFeatureControlSet parent) : base(parent)
		{
		}

		public LicenceDatabaseNonDependentActiveCollection ActiveCargoWiseDatabases
		{
			get
			{
				if (activeCargoWiseDatabases == null)
				{
					var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
					query.AddToFilter(LicenceDatabaseSchema.LD_Product, new string[] { ProductTypes.Codes.Enterprise, ProductTypes.Codes.CargoWiseOne, ProductTypes.Codes.CargoWiseNext });
					query.AddToFilter(LicenceDatabaseSchema.LD_IsActive, true);
					activeCargoWiseDatabases = new LicenceDatabaseNonDependentActiveCollection(Parent.Factory, query);
				}
				return activeCargoWiseDatabases;
			}
		}

		LicenceDatabaseNonDependentActiveCollection activeCargoWiseDatabases;
	}
}
