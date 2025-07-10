using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class MultilinePrePaidCharges : MultiLineIncoBasedConverter
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<MultilinePrePaidCharges({isimport},{incoterm},{chargegroup})>",
				ResString.GetMultilingualString("eef9e097-54b5-4a9c-a550-193cb31f35ec",
				@"For each charge group specified, (you can specify multiple charge groups separated by line feeds) this macro will return either 'Prepaid' or 'Collect' depending on whether the charge group is payable at the Origin or the Destination according to the Incoterm supplied. 
If you specify multiple charge groups separated by line feeds, multiple payment types will be returned separated by line feeds. 
See also: {0} - has full list of available charge groups.",
			 "MultilinePrePaidCharges"),
				new List<(string example, object expectedResult)> { ((NoResString)"<MultilinePrePaidCharges(Y, <Inco Term>, DST\nORG)>", (NoResString)"Collect\nCollect") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			bool isImport = match.Groups[1].Value == "Y";
			ZString incoTermString = match.Groups[2].Value;
			ZString chargeGroupsString = match.Groups[3].Value.Trim();

			return MultiLineConvertByIncoTerm(isImport, chargeGroupsString, incoTermString, (NoResString)"Prepaid", (NoResString)"Collect");
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)MultilinePrePaidCharges(?:[\s]*)\((?:[\s]*)([YN])(?:[\s]*),(?:[\s]*)([^,]*)(?:[\s]*),(?:[\s]*)([^,]*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
