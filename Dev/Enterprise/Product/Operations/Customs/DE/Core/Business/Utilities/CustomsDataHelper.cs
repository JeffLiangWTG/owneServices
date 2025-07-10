using System;
using System.Collections.Generic;
using System.Xml.Schema;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DE.Business
{
	public static class CustomsDataHelper
	{
		public static TIDataProvider DataProvider<TIDataProvider>(this EDIMessage message)
			where TIDataProvider : IDataProvider
		{
			var pk = message.PK;
			var applicationReference = message.EM_ApplicationReference;
			var factory = message.Factory;
			return factory.GetCachedValue(string.Join("|", "DataProvider", pk, applicationReference), () =>
			{
				TIDataProvider result = default;
				var (success, responseMessageDetails) = ((IInboundMessageDataProvider<TIDataProvider>)message).GetResponseMessageDetails(applicationReference);
				if (success)
				{
					try
					{
						var obj = message.DECustomsData<TIDataProvider>();
						var xmlObject = obj?.GetType().GetProperty("Message")?.GetValue(obj);
						if (xmlObject != null)
						{
							result = (TIDataProvider)Activator.CreateInstance(responseMessageDetails.ProviderType, xmlObject);
						}
					}
					catch (Exception ex) when (ex.InnerException is XmlSchemaValidationException || ex.InnerException is InvalidOperationException)
					{
						message.EM_Status = EDIMessage.Status.Error;
						factory.CreateStmNoteForEdiMessage(pk, ex.InnerException.Message);
					}
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Error;
					factory.CreateStmNoteForEdiMessage(pk, FormattableString.Invariant($"Unable to find Message Details for Application Reference: {applicationReference}")); // Debug Note
				}
				return result;
			});
		}

		static object DECustomsData<TIDataProvider>(this EDIMessage message)
			where TIDataProvider : IDataProvider
		{
			var pk = message.PK;
			var applicationReference = message.EM_ApplicationReference;
			var factory = message.Factory;
			return factory.GetCachedValue(string.Join("|", "DECustomsData", pk, applicationReference), () =>
			{
				object result = null;
				var (success, responseMessageDetails) = ((IInboundMessageDataProvider<TIDataProvider>)message).GetResponseMessageDetails(applicationReference);
				if (success)
				{
					using (var textReader = message.GetEM_MessageTextReader())
					{
						var objType = typeof(DECustomsDataProvider<>).MakeGenericType(responseMessageDetails.XmlObjectType);
						result = Activator.CreateInstance(objType, responseMessageDetails.XsdSchemaEmbeddedResourceName, textReader);
					}
				}

				return result;
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004")]
		public static List<AttachedDocument> AttachedDocuments<TIDataProvider>(this EDIMessage message)
			where TIDataProvider : IDataProvider
		{
			var pk = message.PK;
			var applicationReference = message.EM_ApplicationReference;
			var factory = message.Factory;
			return factory.GetCachedValue(string.Join("|", "AttachedDocuments", pk, applicationReference), () =>
			{
				var attachedDocuments = new List<AttachedDocument>();
				var obj = message.DECustomsData<TIDataProvider>();
				if (obj != null)
				{
					attachedDocuments = (List<AttachedDocument>)obj.GetType().GetProperty("AttachedDocuments")?.GetValue(obj) ?? new List<AttachedDocument>();
				}

				return attachedDocuments;
			});
		}
	}
}
