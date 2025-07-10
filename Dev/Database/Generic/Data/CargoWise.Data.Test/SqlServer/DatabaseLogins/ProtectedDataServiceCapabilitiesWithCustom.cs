using System;
using System.Collections.Generic;
using CargoWise.DataProtection;

namespace CargoWise.Data.Testing
{
	class ProtectedDataServiceCapabilitiesWithCustom : ProtectedDataServiceCapabilities
	{
		protected override Dictionary<string, Type> BuildSupportedTypeCollection()
		{
			var result = base.BuildSupportedTypeCollection();
			result.Add("Custom", typeof(CustomCredentials));
			return result;
		}
	}
	public class CustomCredentials : DBCredentials
	{
		public CustomCredentials()
		{ }
		public CustomCredentials(string loginName, string password) : base("NotImportant", "SomeDb", loginName, password) { }
	}
}
