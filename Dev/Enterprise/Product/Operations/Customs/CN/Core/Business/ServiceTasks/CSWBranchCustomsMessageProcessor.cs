using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CN.Business
{
	public class CSWBranchCustomsMessageProcessor : BranchCustomsMessageProcessor
	{
		static IEnumerable<ZString> ApplicationCodes
		{
			get
			{
				yield return GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow;
			}
		}

		public CSWBranchCustomsMessageProcessor() : base(applicationCodes: ApplicationCodes, messageTypes: null) { }

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessorsCore()
		{
			return new List<ApplicationTypeMessageProcessor> { new AcdAgrResponseMessageProcessor(Logger) };
		}
	}
}
