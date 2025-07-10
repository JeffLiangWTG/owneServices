//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEDIMessageLookups
//
//    This class should be used for overriding collections in AutoEDIMessageLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Messaging.Business
{
	public class EDIMessageLookups : AutoEDIMessageLookups
	{
		public EDIMessageLookups(AutoEDIMessage parent)
			: base(parent)
		{
		}

		public virtual CodeDescriptionPairList MessageTypeList
		{
			get { return Factory.GetCachedValue<EDIMessageTypeList>(); }
		}

		public EDIMessageStatusList StatusList
		{
			get { return Factory.GetCachedValue<EDIMessageStatusList>(); }
		}

		public ApplicationCodeList ApplicationList
		{
			get { return Factory.GetCachedValue<ApplicationCodeList>(); }
		}

		#region Companies

		public virtual GlbCompanyCollection Companies
		{
			get { return new GlbCompanyCollection(Factory); }
		}

		#endregion
	}
}
