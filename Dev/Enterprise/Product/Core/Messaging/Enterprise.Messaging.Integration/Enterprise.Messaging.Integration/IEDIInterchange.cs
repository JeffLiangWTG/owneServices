using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Messaging.Integration
{
	public interface IEDIInterchange : IBusiness
	{
		ZGuid PK { get; }
		ZString EI_BodyText { get; set; }
		ZString EI_HeaderText { get; set; }
		ZString EI_FooterText { get; set; }
		ZString EI_ApplicationCode { get; set; }
		ZString EI_InterchangeType { get; set; }
		ZString EI_ReceiveTransmit { get; set; }
		ZString EI_InterchangeNum { get; } // get only - assigned in Interchange.OnSaving()
		ZString EI_To { get; set; }
		ZString EI_From { get; set; }
		ZString EI_TransportType { get; set; }
		ZString EI_Status { get; set; }
		ZBool EI_IsActive { get; set; }
		ZGuid EI_GB { get; set; }
		ZGuid EI_SessionGUID { get; set; }
		ZGuid EI_ECC_CommunicationPartyConfig { get; set; }

		void SetEI_BodyTextOrDataSource(Stream source);
		TextReader GetEI_BodyTextReader(bool closeReaderBetweenReads = false);

		IEnumerable<string> MessageProcessWarnings { get; }
		void AssignInterchangeNumber();

		void AddMessageProcessWarning(string warning);
	}
}
