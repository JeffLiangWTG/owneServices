using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Macro.UDFToMCRConverter
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
	class ValueProviderConverter
	{
		public static readonly IReadOnlyDictionary<string, Tuple<string, Type>> EnvValueProviders = new Dictionary<string, Tuple<string, Type>>
		{
			{ "Enterprise.DocumentEngine.MacroValueProviders.CompanyCode", new Tuple<string, Type>("Company.Code", typeof(ZString)) },
			{ "Enterprise.DocumentEngine.MacroValueProviders.CompanyCountry", new Tuple<string, Type>("Company.Country.Name", typeof(ZString)) },
			{ "Enterprise.DocumentEngine.MacroValueProviders.CompanyCountryCode", new Tuple<string, Type>("Company.Country.Code", typeof(ZString)) },
			{ "Enterprise.DocumentEngine.MacroValueProviders.CompanyName", new Tuple<string, Type>("Company.Name", typeof(ZString)) },
			{ "Enterprise.DocumentEngine.MacroValueProviders.CompanyCurrencyCode", new Tuple<string, Type>("LocalCurrency", typeof(ZString)) },
			{ "Enterprise.DocumentEngine.MacroValueProviders.CompanyOfficeAddress1", new Tuple<string, Type>("Company.Organization.MainAddress.AddressLine1", typeof(ZString)) },
			{ "Enterprise.DocumentEngine.MacroValueProviders.CompanyOfficeAddress2", new Tuple<string, Type>("Company.Organization.MainAddress.AddressLine2", typeof(ZString)) },
			{ "Enterprise.DocumentEngine.MacroValueProviders.BranchPortName", new Tuple<string, Type>("Branch.HomePort.Code", typeof(ZString)) },
		};

		public static readonly IReadOnlyDictionary<string, Tuple<string, Type>> ParametarizedValueProviders = new Dictionary<string, Tuple<string, Type>>
		{
			{ "Enterprise.DocumentEngine.ValueProviders.Macros.Contains,2", new Tuple<string, Type>("Contains({0}, {1}, false)", typeof(ZBool)) },
			{ "Enterprise.DocumentEngine.MacroValueProviders.SubString,2", new Tuple<string, Type>("{0}.Substring({1})", typeof(ZString)) },
			{ "Enterprise.DocumentEngine.MacroValueProviders.SubString,3", new Tuple<string, Type>("{0}.Substring({1}, {2})", typeof(ZString)) },
		};
	}
}
