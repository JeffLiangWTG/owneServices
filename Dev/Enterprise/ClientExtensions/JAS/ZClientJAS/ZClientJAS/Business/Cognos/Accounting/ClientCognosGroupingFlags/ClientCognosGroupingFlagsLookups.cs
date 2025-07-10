//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientCognosGroupingFlagsLookups
//
//    This class should be used for overriding collections in AutoClientCognosGroupingFlagsLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class ClientCognosGroupingFlagsLookups : AutoClientCognosGroupingFlagsLookups
	{
		#region IntercompanyCodes

		public static class IntercompanyCodes
		{
			public const string Exclude = "NO";
			public const string Include = "I/A";
			public const string IncludeWithCurrencyAndAmount = "J";
			public const string ICTOTA = "ICT";
		}

		#endregion

		public ClientCognosGroupingFlagsLookups(AutoClientCognosGroupingFlags parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList IntercompanyCodeList
		{
			get
			{
				if (fIntercompanyCodeList == null)
				{
					fIntercompanyCodeList = new CodeDescriptionPairList();
					fIntercompanyCodeList.AddPair(IntercompanyCodes.Exclude, "Exclude Intercompany Code");
					fIntercompanyCodeList.AddPair(IntercompanyCodes.Include, "Include Intercompany Code");
					fIntercompanyCodeList.AddPair(IntercompanyCodes.IncludeWithCurrencyAndAmount, "Include Intercompany Code with Transaction Currency and Amount");
					fIntercompanyCodeList.AddPair(IntercompanyCodes.ICTOTA, "Specify ICTOTA as Intercompany Code");
				}
				return fIntercompanyCodeList;
			}
		}

		CodeDescriptionPairList fIntercompanyCodeList;
	}
}
