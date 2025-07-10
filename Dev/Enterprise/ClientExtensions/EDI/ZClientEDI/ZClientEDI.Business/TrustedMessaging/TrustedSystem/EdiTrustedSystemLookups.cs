//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiTrustedSystemLookups
//
//    This class should be used for overriding collections in AutoEdiTrustedSystemLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.TrustedMessaging.Business
{
	public class EdiTrustedSystemLookups : AutoEdiTrustedSystemLookups
	{
		public EdiTrustedSystemLookups(AutoEdiTrustedSystem parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ProductTypeList => GetProductTypeList();

		public static CodeDescriptionPairList GetProductTypeList()
		{
			var productTyps = new ProductTypes(true);
			var trustedServices = EDIDataRegistry.Instance.MyAccountTrustedServices.Value.GetActiveCodeDescriptionPairList();
			productTyps.AddRange(trustedServices);
			return productTyps;
		}
	}
}
