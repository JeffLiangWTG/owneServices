using System.IO;
using CargoWise.ComponentModel;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	class QueryInvoiceRequestPayloadValidation : IPayloadValidation
	{
		public QueryInvoiceRequestPayloadValidation()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		void IPayloadValidation.Validate(Stream stream, INotifications errorNotifications, INotifications warningNotifications)
		{
			stream.Position = 0;
			if (new StreamReader(stream).ReadToEnd() != @"<?xml version=""1.0"" encoding=""utf-8""?><Empty />")
			{
				errorNotifications.AddError($"Invalid payload content, it should be empty.");
			}
		}
	}
}
