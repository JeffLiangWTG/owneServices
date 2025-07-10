using System.Collections.Generic;

namespace Enterprise.Client.UPE.Business
{
	public class CustomsQueueCodeDescriptionPairList : AutoCustomsQueueCodeDescriptionPairList
	{
		public CustomsQueueCodeDescriptionPairList()
		{
		}

		public static IReadOnlyList<string> CompletedQueueNames
		{
			get
			{
				return new string[]
				{
					DefaultQueueCodeDescriptionPairList.Codes.Completed,
					DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding
				};
			}
		}
	}
}
