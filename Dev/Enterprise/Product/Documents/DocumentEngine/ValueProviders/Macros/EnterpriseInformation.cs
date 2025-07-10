using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class EnterpriseInformation : ValueProvider
	{
		public EnterpriseInformation() : base() { }

		protected override ValueProviderDocumenter GetDocumentation()
		{
			var retriever = new EnterpriseInformationRetriever();
			var propertyNames = new List<string>(retriever.GetType().GetProperties().Select(p => p.Name));
			return new ValueProviderDocumenter("<EnterpriseInformation.{Property})>",
				ResString.GetMultilingualString("061ad40b-5d40-4841-bd30-1815e50383ac", @"This macro is used to access various information about the {0} application. 
Available properties are {1}.", Core.Constants.ProductName, string.Join(",", propertyNames)),
				new List<(string example, object expectedResult)> { ("<EnterpriseInformation.DBServerName>", retriever.DBServerName) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			string result;
			string propertyName = fRegex.Match(macro).Groups["Property"].Value;
			var retriever = new EnterpriseInformationRetriever();
			var retrieverType = typeof(EnterpriseInformationRetriever);

			var propertyInfo = retrieverType.GetProperty(propertyName);
			if (propertyInfo != null)
			{
				using (Culture.SetTemporarily(CultureInfo.InvariantCulture))
				{
					result = (string)propertyInfo.GetValue(retriever, null);
				}
			}
			else
			{
				result = (NoResString)"Invalid property name: " + macro;
			}

			return result;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^\s*<\s*EnterpriseInformation\s*\.\s*(?<Property>\S+)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
