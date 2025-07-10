//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobChargePostingQueueLookups
//
//    This class should be used for overriding collections in AutoJobChargePostingQueueLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobChargePostingQueueLookups : AutoJobChargePostingQueueLookups
	{
		public JobChargePostingQueueLookups(AutoJobChargePostingQueue parent) : base(parent)
		{
		}

		public const string PostCost = "CST";
		public const string PostRevenue = "REV";

		public CodeDescriptionPairList PostingInstructions
		{
			get
			{
				var postingInstructions = new CodeDescriptionPairList();
				postingInstructions.AddPair(PostCost, Res.GetString("32079C47-8A7A-432d-97E8-DCE86CF6BD55", "Post Cost"));
				postingInstructions.AddPair(PostRevenue, Res.GetString("3C11B3E8-6624-44fa-9FBC-49A403269176", "Post Revenue"));
				return postingInstructions;
			}
		}
	}
}