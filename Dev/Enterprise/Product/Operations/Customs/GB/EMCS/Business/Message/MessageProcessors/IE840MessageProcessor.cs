using System;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE840MessageProcessor : EMCSMessageProcessor<IIE840>
	{
		public IE840MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("6446F728-7DA3-4072-A13E-A3DCE470A9F9", "EMCS IE840 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE840 provider)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.EVT;
			linkedEMCSDeclaration = emcsDeclaration;
			SendEmailNotification(message, Res.GetString("07CBB86D-3281-4AE7-84A9-1F620425B9E0", "EMCS Event Report"), false, provider, null, GetEmailBody);
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE840)dataProvider;
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("245036DF-5E4A-4190-8F5E-C7B5138D70FF", "Your EMCS Declaration for Job {0} has received an Event Report. For details please follow the Link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("A7241152-6F1A-4218-99C5-4A1F7DD440B3", "ARC: {0}", provider.ExciseMovementEad.AdministrativeReferenceCode));

			return htmlBody.ToString();
		}
	}
}
