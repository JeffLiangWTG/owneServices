using System;

namespace Enterprise.UniversalDataBuss.Integration
{
	public static class UniversalXmlInfo
	{
		public const string Namespace_2011_11 = "http://www.cargowise.com/Schemas/Universal/2011/11";
		public const string Namespace_2012_11 = "http://www.cargowise.com/Schemas/Universal/2012/11";
		public const string Version_2011_11 = "1.1";
		public const string Version_2012_11_DO_NOT_USE = "2.0";

		public const string CommonSchemaName = "UniversalCommon.xsd";
		public const string InterchangeSchemaName = "UniversalInterchange.xsd";
		public const string ResponseSchemaName = "UniversalResponse.xsd";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public const string ZipFileName = "Universal XML";

		public const int MaxStringLength = Int32.MaxValue;
	}
}
