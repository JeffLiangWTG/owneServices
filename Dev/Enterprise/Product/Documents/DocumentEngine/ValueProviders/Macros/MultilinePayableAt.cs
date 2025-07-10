using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class MultilinePayableAt : MultiLineIncoBasedConverter
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<MultilinePayableAt({loadport},{dischargeport},{incoterm},{chargegroup})>",
				ResString.GetMultilingualString("ce63cfc5-95a9-4362-972a-1b792099aa44",
				@"For each charge group specified, (you can specify multiple charge groups separated by line feeds) this macro will return either the Load or Discharge port depending on whether the charge group is payable at the Origin or the Destination according to the Incoterm supplied. If you specify multiple charge groups separated by line feeds, multiple UNLOCO's will be returned separated by line feeds.
See also: {0}.
Available Charge Groups:
{1}.", "MultilinePrePaidCharges", new ChargeCodeGroupList().GetHumanReadableListOfElements()),
				new List<(string example, object expectedResult)> { ((NoResString)"<MultilinePayableAt(<Shipment.Origin>, <Shipment.Destination>, EXW, DST\nORG)>", "AUBNE\nAUBNE") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			ZString loadPort = match.Groups[1].Value;
			ZString dischargePort = match.Groups[2].Value;
			ZString incoTermString = match.Groups[3].Value;
			ZString chargeGroupsString = match.Groups[4].Value.Trim();
			string countryCode = GlbBranch.CurrentBranch.Country.RN_Code;
			bool isImport = !loadPort.StartsWith(countryCode) && dischargePort.StartsWith(countryCode);

			return MultiLineConvertByIncoTerm(isImport, chargeGroupsString, incoTermString, loadPort, dischargePort);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)MultilinePayableAt(?:[\s]*)\((?:[\s]*)([^,]*)(?:[\s]*),(?:[\s]*)([^,]*)(?:[\s]*),(?:[\s]*)([^,]*)(?:[\s]*),(?:[\s]*)([^,]*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
