//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoReleaseBuildLookups
//
//    This class should be used for overriding collections in AutoReleaseBuildLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.ReleaseBuilds.Business
{
	public class ReleaseBuildLookups : AutoReleaseBuildLookups
	{
		public ReleaseBuildLookups(AutoReleaseBuild parent)
			: base(parent)
		{
		}

		#region Product
		public CodeDescriptionPairList ProductTypeList
		{
			get
			{
				CodeDescriptionPairList productType = new CodeDescriptionPairList();
				productType.AddRange(EDIDataRegistry.Instance.SystemProductMappings.Value);
				productType.AddPairIfNotExist(ProductTypes.Codes.CargoWiseNext, ProductTypes.Descriptions.CargoWiseNext);
				productType.AddPairIfNotExist(ProductTypes.Codes.CargoWise, ProductTypes.Descriptions.CargoWise);

				return productType;
			}
		}
		#endregion

		#region Statuses

		public CodeDescriptionPairList Statuses
		{
			get { return new ReleaseRingsList(); }
		}

		#endregion
	}
}

