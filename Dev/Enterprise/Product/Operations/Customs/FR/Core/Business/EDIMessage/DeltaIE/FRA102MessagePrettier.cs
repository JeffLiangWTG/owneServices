using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA102;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class FRA102MessagePrettier : DeltaIEMessagePrettier<FRA102AType>
	{
		public FRA102MessagePrettier(FRA102MessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(FRA102AType messageObject)
		{
			var operation = messageObject.Operation.FirstOrDefault();
			var request = messageObject.Request;
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("LRN", operation?.LRN ?? ZString.Empty),
				("CRN", operation?.CustomsRegistrationNumber ?? ZString.Empty),
				("MRN", operation?.MRN ?? ZString.Empty),
				((NoResString)"Operator request reference", request?.OperatorRequestReference ?? ZString.Empty),
				((NoResString)"Customs request reference", request?.CustomsRequestReference ?? ZString.Empty),
				((NoResString)"Request Registration date and time", request?.RequestRegistrationDateTime ?? ZString.Empty),
			});
		}
	}
}
