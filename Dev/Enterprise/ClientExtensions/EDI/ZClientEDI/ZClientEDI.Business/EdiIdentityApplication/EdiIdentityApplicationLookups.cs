//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiIdentityApplicationLookups
//
//    This class should be used for overriding collections in AutoEdiIdentityApplicationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IdentityApplication.Business
{
	public class EdiIdentityApplicationLookups : AutoEdiIdentityApplicationLookups
	{
		public EdiIdentityApplicationLookups(AutoEdiIdentityApplication parent) : base(parent)
		{
		}

		public EdiIdentityApplicationRedirectUrlStatus EdiIdentityApplicationRedirectUrlStatusList => new EdiIdentityApplicationRedirectUrlStatus();

		public CodeDescriptionPairList ProductTypeList
		{
			get
			{
				var productTypes = (CodeDescriptionPairList)new ProductTypes(true);
				productTypes.Sort();
				return productTypes;
			}
		}

		public DatabaseTypes DatabaseTypesList => new DatabaseTypes();

		public EdiIdentityApplicationCollection ParentApplications => new EdiIdentityApplicationCollection(Factory, new ZQuery(EdiIdentityApplicationSchema.IDA_LD, SQLComparisonOperator.IsNotBlank, null));
	}
}
