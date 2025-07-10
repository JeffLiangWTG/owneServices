using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageDirectorForNZCustoms
	{
		readonly INotifications notifier;
		readonly EDIInterchange interchange;

		public EHubMessageDirectorForNZCustoms(EDIInterchange interchange, INotifications notifier)
		{
			this.notifier = notifier;
			this.interchange = interchange;
		}

		public EHubMessageBuilder CreateBuilder()
		{
			if (interchange.EI_ApplicationCode == ApplicationCodeList.Codes.NZMAFeBACCa)
			{
				return new EHubMessageBuilderForNZCustomsEBACCA(interchange, notifier);
			}
			else if (IsWCOMessageType)
			{
				return new EHubMessageBuilderForNZCustomsWCO(interchange, notifier);
			}
			else
			{
				return new EHubMessageBuilderForNZCustomsLegacy(interchange, notifier);
			}
		}

		bool IsWCOMessageType
		{
			get
			{
				return interchange.EI_ApplicationCode == ApplicationCodeList.Codes.NZCustoms && XmlMessageTypeList.Where(t => t == interchange.EI_InterchangeType).Any();
			}
		}

		readonly string[] XmlMessageTypeList = new string[] { "ANA", "AND", "I10", "I11", "I51", "I52", "I53", "IPI", "E40", "E41", "EXC", "ICR", "CRE", "OCR" };
	}
}