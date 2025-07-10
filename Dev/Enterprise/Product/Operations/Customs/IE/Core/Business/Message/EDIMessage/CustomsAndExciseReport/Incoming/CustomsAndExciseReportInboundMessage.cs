using System;
using System.Data;
using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class CustomsAndExciseReportInboundMessage : CustomsAndExciseReportMessage
	{
		public CustomsAndExciseReportInboundMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public TDataProvider GetDataProvider<TDataProvider>(Type objectType, Action exceptionHandling = null)
		{
			TDataProvider result = default;
			try
			{
				if (typeof(TDataProvider) == typeof(CustomsAndExciseReportErrorProvider))
				{
					using (var reader = GetEM_MessageTextReader())
					{
						var xmlObject = IEXmlObjectSerializer.Deserialize<MessageAcknowledgement>(reader);
						result = (TDataProvider)Activator.CreateInstance(typeof(TDataProvider), xmlObject);
					}
				}
				else
				{
					var jsonObject = JsonSerializer.Deserialize(EM_MessageText, objectType);
					if (jsonObject != null)
					{
						result = (TDataProvider)Activator.CreateInstance(typeof(TDataProvider), jsonObject);
					}
				}
			}
			catch
			{
				exceptionHandling?.Invoke();
			}
			return result;
		}

		public override ZString EM_MessageInterpretation
		{
			get => MessageInterpretationNoteManager.Value;
			set
			{
				var oldValue = EM_MessageInterpretation;
				base.EM_MessageInterpretation = value;
				EM_MessageInterpretationInfo.RefreshBinding(oldValue);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}
	}
}
