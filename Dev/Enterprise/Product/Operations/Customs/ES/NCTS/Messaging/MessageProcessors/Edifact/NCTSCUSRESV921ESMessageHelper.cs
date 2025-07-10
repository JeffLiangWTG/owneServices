using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Edifact.V921ES;
using Enterprise.Edifact.V921ES.Elements;
using Enterprise.Edifact.V921ES.Messages.CUSRES;
using Enterprise.Edifact.V921ES.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors
{
	public class NCTSCUSRESV921ESMessageHelper : CUSRESV921ESMessageHelper, INctsDepartureAndTIRResponseMessageProvider
	{
		protected NCTSCUSRESV921ESMessageHelper(BusinessObjectFactory factory, CUSRESMessage message) : base(factory, message)
		{
		}

		public static new NCTSCUSRESV921ESMessageHelper New(EDIMessage message)
		{
			NCTSCUSRESV921ESMessageHelper result = null;
			if (message != null)
			{
				var d921MessageFactory = new D921ESMessageFactory();
				var esCharSet = new UNOAESCharacterSet();
				var cusresMessage = message.GetAutoEdifactMessageUsingNamedFactory(d921MessageFactory, esCharSet) as CUSRESMessage;
				if (cusresMessage != null)
				{
					result = new NCTSCUSRESV921ESMessageHelper(message.Factory, cusresMessage);
				}
			}
			return result;
		}

		ZString INctsDepartureAndTIRResponseMessageProvider.CustomsClearanceCriteria
		{
			get
			{
				if (!customsClearanceCriteria.HasValue)
				{
					customsClearanceCriteria = GetCustomsClearanceCriteria();
				}
				return customsClearanceCriteria.Value;
			}
		}
		ZString? customsClearanceCriteria;

		ZString GetCustomsClearanceCriteria()
		{
			var result = ZString.Empty;
			foreach (GISSegment gis in cusresMessage.GIS)
			{
				if (gis.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.CustomsPreference)
				{
					result = gis.ProcessingIndicator.ProcessTypeIdentification.ToString();
					break;
				}
			}
			return result;
		}
	}
}
