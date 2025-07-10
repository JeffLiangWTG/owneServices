using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Integration
{
	/// <summary>
	/// Constants specific to Data Transfer or Data Export.
	/// </summary>
	public static class DataTransferConstants
	{
		public static class AccTransactionHeaderAuthorisationRecord
		{
			// Database table assumes version 1 EInvoicing data. XUT needs to be more future proof.
			public const string EInvoicingModuleCode = "EINV";
			public static MultilingualString EInvoicingModuleDescription
				=> ResString.GetMultilingualString("e0763ab2-dae4-4f12-b722-cdcd17a5c37d", "Electronic Invoicing");

			public const string Version0 = "0";
			public const string Version1 = "1";

			/*
				Please don't add to this collection of legacy NullPlaceholders
				AccTransactionHeaderAuthorisationRecord now allows empty/null values

				These NullPlaceholders have been inserted in the past and still exist in customer systems
				These values are still used in tests and will remain until here until all customer data has been transformed
			*/
			public const string NullPlaceholderForNVarchar = "_";
			public const string NullPlaceholderForChar3 = "___"; 
			public readonly static ZDateTimeOffset NullPlaceholderForDateTime = new ZDateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
		}
	}
}
