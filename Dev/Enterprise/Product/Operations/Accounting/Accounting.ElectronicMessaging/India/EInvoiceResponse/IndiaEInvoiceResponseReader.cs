using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;

namespace Enterprise.Accounting.ElectronicMessaging.India
{
	// Code will be removed in WI00829724 once we are confident all customers support new style XUE messages

	public class IndiaEInvoiceResponseReader
	{
		public IndiaEInvoiceResponseReader(string payload, INotifications notifications)
		{
			Payload = Argument.NotNullOrEmpty(payload, nameof(payload));
			Notifications = Argument.NotNull(notifications, nameof(notifications));
		}

		string Payload { get; }
		INotifications Notifications { get; }

		const string SchemaResourceName = "Enterprise.Accounting.ElectronicMessaging.India.EInvoiceResponse.IndiaEInvoiceResponseSchema.json";

		public IndiaEInvoiceResponse Read()
		{
			IndiaEInvoiceResponse result = null;
			try
			{
				bool isValid;
				var payload = Payload.ToUTF8FromBase64();

				try
				{
					var schema = JsonSchemaLoader.Load(SchemaResourceName);
					isValid = schema.ValidateJSON(payload, Notifications);
				}
				catch (JsonReaderException)
				{
					AddJsonNotificationError(payload);
					return null;
				}

				var settings = new JsonSerializerSettings
				{
					Error = (s, e) =>
					{
						Notifications.AddError(e.ErrorContext.Error.Message);
						e.ErrorContext.Handled = true;
						isValid = false;
					}
				};
				result = JsonConvert.DeserializeObject<IndiaEInvoiceResponse>(payload, settings);

				if (!isValid)
				{
					AddJsonNotificationError(payload);
					return null;
				}

				isValid &= ValidateDatestamp(result.AckDateTime, "AckDt", payload);               // JSON field in schema
				isValid &= ValidateJwt(result.SignedInvoiceAsJwt, "SignedInvoice", payload);      // JSON field in schema
				isValid &= ValidateJwt(result.SignedQRCodeAsJwt, "SignedQRCode", payload);        // JSON field in schema

				result = isValid ? result : null;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Notifications.AddError(Res.GetString("744723f8-d75b-440e-a042-bed657632ab9", "Reading India E-Invoice Response ended with an exception.\r\nException: {0}\r\nException Message: {1}.", ex.GetType(), ex.Message));
			}

			return result;
		}

		bool ValidateDatestamp(string timestamp, string fieldName, string payload)
		{
			if (!ZDateTime.TryParseExact(timestamp, out var _, (NoResString)"yyyy-MM-dd HH:mm:ss"))      // Date parsing format string based on external schema
			{
				AddFieldNotificationError(fieldName, payload);
				return false;
			}
			return true;
		}

		bool ValidateJwt(string jwtData, string fieldName, string payload)
		{
			if (string.IsNullOrEmpty(jwtData))
			{
				AddFieldNotificationError(fieldName, payload);
				return false;
			}

			var jwtParts = jwtData.Split('.');
			if (jwtParts.Length != 3)
			{
				AddFieldNotificationError(fieldName, payload);
				return false;
			}

			try
			{
				_ = Convert.FromBase64String(Base64UrlUtility.FromUrlSafeBase64(jwtParts[0]));
				_ = Convert.FromBase64String(Base64UrlUtility.FromUrlSafeBase64(jwtParts[1]));
				_ = Convert.FromBase64String(Base64UrlUtility.FromUrlSafeBase64(jwtParts[2]));
			}
			catch (FormatException)
			{
				AddFieldNotificationError(fieldName, payload);
				return false;
			}

			return true;
		}

		void AddFieldNotificationError(string fieldName, string payload)
		{
			Notifications.AddError(Res.GetString("e52e812b-acc6-492f-b480-652ee4816adc", "India E-Invoice Response has invalid '{0}' field:\r\n{1}", fieldName, payload));
		}

		void AddJsonNotificationError(string payload)
		{
			Notifications.AddError(Res.GetString("45d24725-5494-4ec1-a52b-85d2aa890eaf", "Reading the following India E-Invoice Response has failed due to invalid JSON:\r\n{0}", payload));
		}
	}
}
