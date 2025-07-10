//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiTrustedMessagingConfigLookups
//
//    This class should be used for overriding collections in AutoEdiTrustedMessagingConfigLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.TrustedMessaging.Business
{
	public class EdiTrustedMessagingConfigLookups : AutoEdiTrustedMessagingConfigLookups
	{
		public EdiTrustedMessagingConfigLookups(AutoEdiTrustedMessagingConfig parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ProductTypeList => EdiTrustedSystemLookups.GetProductTypeList();

		public CodeDescriptionPairList CertificateTypeList => new CertificateTypeList();
	}
}
