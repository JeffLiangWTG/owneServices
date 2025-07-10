using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase4;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public abstract class XmlMessageBuilder<T, M> : Interfaces.IXmlMessageBuilder
		where T : IDeclaration
		where M : INctsXmlMessage, new()
	{
		protected XmlMessageBuilder(T wrapper, ErrorCollector errorCollector)
		{
			this.wrapper = Argument.NotNull(wrapper, nameof(wrapper));
			this.errorCollector = Argument.NotNull(errorCollector, nameof(errorCollector));
		}

		protected T Wrapper => wrapper;

		protected ErrorCollector ErrorCollector => errorCollector;

		protected M GenerateMessage()
		{
			var message = new M();
			PopulateMessageHeader(message);
			PopulateMessageBody(message);
			return message;
		}

		protected virtual void PopulateMessageHeader(M message)
		{
			var dateTimeNow = ZDateTime.Now;
			message.SynIdeMes1 = "UNOC";
			message.SynVerNumMes2 = "3";
			message.MesRecMes6 = "NCTS";
			message.DatOfPreMes9 = dateTimeNow.ToString("yyMMdd");
			message.TimOfPreMes10 = dateTimeNow.ToString("HHmm");
			message.IntConRefMes11 = NctsTransmissionMessageGenerator.XML_INTERCHANGEID_PLACEHOLDER;
			message.AppRefMes14 = ApplicatonReference_NCTS;
			message.MesIdeMes19 = NctsTransmissionMessageGenerator.XML_MESSAGEID_PLACEHOLDER;
			message.ComAccRefMes21 = NctsTransmissionMessageGenerator.XML_SYSCAR_PLACEHOLDER;
		}
		const string ApplicatonReference_NCTS = "NCTS";

		protected virtual void PopulateMessageBody(M message)
		{
		}

		public ZString GetXMLMessage()
		{
			var message = GenerateMessage();
			return message == null ? ZString.Empty : CTCExtensions.Serialize(message);
		}

		public ZString GetXMLMessageWithoutNamespaces()
		{
			var message = GetXMLMessage();
			return RemoveAllNamespaces(message);
		}

		static ZString RemoveAllNamespaces(ZString xmlDocument)
		{
			var element = XElement.Parse(xmlDocument);
			if (element != null)
			{
				var xmlDocumentWithoutNs = RemoveAllNamespaces(element);
				return xmlDocumentWithoutNs.ToString();
			}
			return xmlDocument;
		}

		//Recursive function
		static XElement RemoveAllNamespaces(XElement e)
		{
			return new XElement(e.Name.LocalName,
			  (from n in e.Nodes()
			   let e1 = n as XElement
			   select (e1 != null ? RemoveAllNamespaces(e1) : n)),
				  (e.HasAttributes) ?
					(from a in e.Attributes()
					 where (!a.IsNamespaceDeclaration)
					 select new XAttribute(a.Name.LocalName, a.Value)) : null);
		}

		protected readonly ErrorCollector errorCollector;
		protected readonly T wrapper;
	}
}
