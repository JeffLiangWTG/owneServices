//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiIdentityCertificateLookups
//
//    This class should be used for overriding collections in AutoEdiIdentityCertificateLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.IdentityCertificate.Business
{
	public class EdiIdentityCertificateLookups : AutoEdiIdentityCertificateLookups
	{
		public EdiIdentityCertificateLookups(AutoEdiIdentityCertificate parent) : base(parent)
		{
		}

		public static class Statuses
		{
			public const string Queued = "QUE";
			public const string Processing = "PRC";
			public const string Completed = "COM";
		}

		public static CodeDescriptionPairList ProcessingStatusList
		{
			get
			{
				CodeDescriptionPairList codeDescriptionPairList = new CodeDescriptionPairList();
				codeDescriptionPairList.AddPair(Statuses.Queued, Res.GetString("5E9EC9AE-2AFD-4979-9405-3EB62478AEC1", "Queued Only"));
				codeDescriptionPairList.AddPair(Statuses.Processing, Res.GetString("1BE26D10-C15E-4C11-AA8D-0D8A808DA148", "Processing Only"));
				codeDescriptionPairList.AddPair(Statuses.Completed, Res.GetString("D5929169-A6B1-4617-B231-8188943B2F71", "Completed Only"));
				return codeDescriptionPairList;
			}
		}
	}
}
