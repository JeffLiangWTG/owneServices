using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSAmendmentComparisonEDIMessage : CDSEDIMessage, Integration.Customs.GB.GBCDS.IGbCDSEdiMessage
	{
		public CDSAmendmentComparisonEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.NewAmendment;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			EM_Status = EDIMessageStatusList.Codes.Acknowledged;
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation { get => string.Format(CultureInfo.CurrentCulture, messageInterpretation, MessagePrettierCss.CSS, EM_SystemCreateTimeUtc); set => base.EM_MessageInterpretation = value; }

		const string messageInterpretation = "{0}This is a snapshot of the state of your entry at {1} UTC, which was used to create an amendment request.  The details of what was sent to CDS should be viewed on the AMD message.";
	}
}
