using System.Collections;

using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for CMRMessageTestingRetriever.
	/// </summary>
	public class CMRMessageTestingRetriever : AUCInterchangeRetriever
	{
		public CMRMessageTestingRetriever() : base()
		{
			interchangesToNotAcknowledge[0] = 6;
			interchangesToNotAcknowledge[1] = 4;
			interchangesToNotAcknowledge[2] = 2;

			interchangesNumbersNotAcknowledged[0] = new ArrayList();
			interchangesNumbersNotAcknowledged[1] = new ArrayList();
			interchangesNumbersNotAcknowledged[2] = new ArrayList();
		}

		protected int[] interchangesToNotAcknowledge = new int[3];
		protected ArrayList[] interchangesNumbersNotAcknowledged = new ArrayList[3];
		protected override void CreateAcknowledgementMessage(EDIInterchange interchange)
		{
			int numberOfTimesNotAcknowledged = 0;
			if (interchangesNumbersNotAcknowledged[0].Contains(interchange.EI_InterchangeNum))
			{
				if (interchangesNumbersNotAcknowledged[1].Contains(interchange.EI_InterchangeNum))
				{
					if (!interchangesNumbersNotAcknowledged[2].Contains(interchange.EI_InterchangeNum) && interchangesNumbersNotAcknowledged[2].Count < interchangesToNotAcknowledge[2])
					{
						interchangesNumbersNotAcknowledged[2].Add(interchange.EI_InterchangeNum);
						numberOfTimesNotAcknowledged = 3;
					}
				}
				else if (interchangesNumbersNotAcknowledged[1].Count < interchangesToNotAcknowledge[1])
				{
					interchangesNumbersNotAcknowledged[1].Add(interchange.EI_InterchangeNum);
					numberOfTimesNotAcknowledged = 2;
				}
			}
			else if (interchangesNumbersNotAcknowledged[0].Count < interchangesToNotAcknowledge[0])
			{
				interchangesNumbersNotAcknowledged[0].Add(interchange.EI_InterchangeNum);
				numberOfTimesNotAcknowledged = 1;
			}
			if (numberOfTimesNotAcknowledged == 0)
			{
				base.CreateAcknowledgementMessage(interchange);
			}
			else
			{
				Logger.Log("Not acknowledging interchange #" + interchange.EI_InterchangeNum + " for the " + numberOfTimesNotAcknowledged + " time");
			}
		}
	}
}
