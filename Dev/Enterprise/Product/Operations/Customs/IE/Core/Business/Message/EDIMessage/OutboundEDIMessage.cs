using System;
using System.Data;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public abstract class OutboundEDIMessage : EDIMessage
	{
		protected OutboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public TDataProvider GetDataProvider<TDataProvider>() where TDataProvider : IXMLMessageObject
		{
			return GetDataProviderCore<TDataProvider>();
		}

		protected virtual TDataProvider GetDataProviderCore<TDataProvider>()
		{
			TDataProvider result = default;
			try
			{
				using (var reader = GetEM_MessageTextReader())
				{
					result = IEXmlObjectSerializer.Deserialize<TDataProvider>(reader, withXSDValidation: false);
				}
			}
			catch (Exception ex) when (ex.InnerException is XmlSchemaValidationException || ex.InnerException is InvalidOperationException || ex.InnerException is XmlException)
			{ }

			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}

		public override void ResetToQueuedStatus() { }
	}
}
