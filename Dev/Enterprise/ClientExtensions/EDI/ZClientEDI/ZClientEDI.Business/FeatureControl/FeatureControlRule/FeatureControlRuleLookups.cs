//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoFeatureControlRuleLookups
//
//    This class should be used for overriding collections in AutoFeatureControlRuleLookups
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
	public class FeatureControlRuleLookups : AutoFeatureControlRuleLookups
	{
		public FeatureControlRuleLookups(AutoFeatureControlRule parent) : base(parent)
		{
		}

		public FeatureControlRuleLicenceDatabaseCollection LicenceDatabaseNotLinked
		{
			get
			{
				if (licenceDatabaseNotLinked == null)
				{
					var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
					query.AddToFilter(LicenceDatabaseSchema.LD_Product, new string[] { ProductTypes.Codes.Enterprise, ProductTypes.Codes.CargoWiseOne, ProductTypes.Codes.CargoWiseNext, ProductTypes.Codes.CargoWise });
					var subPivotQuery = new ZDBOnlySubQuery(typeof(FeatureControlRuleLicenceDatabasePivot), FeatureControlRuleLicenceDatabasePivotSchema.FCD_LD_LicenceDatabase, notIn: true);
					subPivotQuery.AddToFilter(FeatureControlRuleLicenceDatabasePivotSchema.FCD_FCR_FeatureControlRule, Parent.PK);
					query.AddSubQuery(LicenceDatabaseSchema.PK, subPivotQuery, JoinCondition.And);

					licenceDatabaseNotLinked = new FeatureControlRuleLicenceDatabaseCollection((FeatureControlRule)Parent, query);
				}
				return licenceDatabaseNotLinked;
			}
		}

		FeatureControlRuleLicenceDatabaseCollection licenceDatabaseNotLinked;

		public FeatureControlSetCollection FeatureSets
		{
			get
			{
				if (featureSets == null)
				{
					featureSets = new FeatureControlSetCollection(Factory);
				}
				return featureSets;
			}
		}
		FeatureControlSetCollection featureSets;
	}
}

