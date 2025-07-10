using System.Collections.Generic;
using CargoWise.Integration;

namespace Enterprise.Client.UPE.Business
{
	public class CommercialQueueCodeDescriptionPairList : AutoCommercialQueueCodeDescriptionPairList
	{
		public CommercialQueueCodeDescriptionPairList()
		{
			MoveToLast(Codes.Finance);
			MoveToLast(Codes.Hold);
			MoveToLast(Codes.EIR);
			MoveToLast(Codes.AR);
			MoveToLast(Codes.Chase);
			MoveToLast(Codes.OnFile);
			MoveToLast(Codes.Rebill);
			MoveToLast(Codes.AlternateBroker);
			MoveToLast(Codes.Completed);
		}

		public static IReadOnlyList<string> CompletedQueueNames
		{
			get
			{
				return new string[]
				{
					CommercialQueueCodeDescriptionPairList.Codes.Completed,
					CommercialQueueCodeDescriptionPairList.Codes.OnFile,
					CommercialQueueCodeDescriptionPairList.Codes.Chase,
					CommercialQueueCodeDescriptionPairList.Codes.Rebill,
				};
			}
		}

		void MoveToLast(string code)
		{
			ICodeDescription item = this[code];
			Remove(item);
			Add(item);
		}
	}
}
