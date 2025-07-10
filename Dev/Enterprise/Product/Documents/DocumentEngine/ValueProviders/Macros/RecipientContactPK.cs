using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class RecipientContactPK : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<RecipientContactPK>",
				ResString.GetMultilingualString("02342508-4e5f-47f5-92e0-298ae9a27928", "Returns the PK of the Contact ({0}) the Report or Document is supposed to be sent to.", "OrgContact"),
				new List<(string example, object expectedResult)> { ("<RecipientContactPK>", "b003d3b0-a36d-4590-a489-4fafdfe541f5") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return (report.DeliveryContact != null && report.DeliveryContact.Contact != null && report.DeliveryContact.Contact.IsInDatabase)
				? report.DeliveryContact.Contact.PK.ToString()
				: ZGuid.Empty.ToString();
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*RecipientContact\s*PK\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
