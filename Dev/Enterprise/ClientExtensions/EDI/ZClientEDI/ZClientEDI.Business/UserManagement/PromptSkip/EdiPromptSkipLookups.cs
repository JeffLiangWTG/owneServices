//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPromptSkipLookups
//
//    This class should be used for overriding collections in AutoEdiPromptSkipLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiPromptSkipLookups : AutoEdiPromptSkipLookups
	{
		public EdiPromptSkipLookups(AutoEdiPromptSkip parent) : base(parent)
		{
		}

		#region Types

		public ReadOnlyCodeDescriptionPairList Types => Factory.GetCachedValue("EdiPromptSkipLookups.Types", () => new EdiPromptSkipTypes());

		public static ReadOnlyCodeDescriptionPairList PromptTypes => new EdiPromptSkipTypes();

		#endregion
	}
}