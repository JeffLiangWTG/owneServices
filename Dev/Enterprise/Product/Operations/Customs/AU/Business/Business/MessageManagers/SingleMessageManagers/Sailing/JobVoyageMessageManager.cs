using System.Collections;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobVoyageMessageManager : Customs.Business.MultiMessageManager
	{
		public JobVoyageMessageManager(CustomsJobVoyageWrapper voyageWrapper)
			: base()
		{
			this.voyageWrapper = voyageWrapper;
		}

		public override bool AllowManualAmendments
		{
			get { return true; }
		}

		#region Implementation

		public override IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return voyageWrapper; }
		}

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return false; }
		}

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			ArrayList result = new ArrayList();
			result.Add(new JobVoyageAIRIARManager(voyageWrapper));
			foreach (CustomsVoyageDestinationWrapper destinationWrapper in voyageWrapper.Destinations)
			{
				result.Add(new VoyageDestinationAIRAARManager(destinationWrapper));
			}
			return (SingleMessageManager[])result.ToArray(typeof(SingleMessageManager));
		}
		readonly CustomsJobVoyageWrapper voyageWrapper;

		#endregion
	}
}
