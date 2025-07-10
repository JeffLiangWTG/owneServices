using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class FormatJobDocAddress : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<FormatJobDocAddress({JobDocAddressPK} [,{HideCountryIfSame}])>",
				ResString.GetMultilingualString("7532a6d1-423a-409b-91e7-364b756c5e33", @"Returns a formatted address for the specified address including the company name.
{0} is the PK of a Doc Address ({1}) you want to get the address from.

Optional Parameters:
{2} is a Microsoft JScript statement that if is evaluated to True will hide the Country in the address if it's the same with the Sender's Company Country.",
"{JobDocAddressPK}", "JobDocAddress", "{HideCountryIfSame}"),
				new List<(string example, object expectedResult)> {
					((NoResString)"<FormatJobDocAddress(Header.JobDocAddressID)>", (NoResString)"UNIT 3, 72 O'RIORDAN STREET\nALEXANDRIA NSW\nSYDNEY NSW 2015\nAUSTRALIA"),
					((NoResString)"<FormatJobDocAddress(Header.JobDocAddressID , '<TransportMode>'=='SEA')>", (NoResString)"UNIT 3, 72 O'RIORDAN STREET\nALEXANDRIA NSW\nSYDNEY NSW 2015") });
		}

		protected override bool NeedCheckForScientificNotation => true;

		#region GetReplacement
		protected override object GetReplacementCore(string macro, Report report)
		{
			Guid jobDocAddressPK = Guid.Empty;
			bool hideCountryIfSame = false;
			Match match = Regex.Match(macro);

			object parsedValue = ValueProviderHelper.ParseGuid(report, match.Groups[1].Value);

			if (parsedValue is Guid)
			{
				jobDocAddressPK = (Guid)parsedValue;
			}
			else
			{
				return parsedValue;
			}

			if (match.Groups.Count > 3 && !string.IsNullOrEmpty(match.Groups[3].Value))
			{
				hideCountryIfSame = ExpressionEvaluator.Evaluate(match.Groups[3].Value, report.UseJsEvaluator);
			}

			if (jobDocAddressPK != Guid.Empty)
			{
				JobDocAddress docAddress = GetDocAddress(jobDocAddressPK);
				if (docAddress != null)
				{
					return new AddressFormatter(docAddress.Factory, docAddress, GlbCompany.CurrentCompany, !hideCountryIfSame).PostalAddress();
				}
				return "";
			}

			return "";
		}

		JobDocAddress GetDocAddress(Guid jobDocAddressPK)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			return factory.Load<JobDocAddress>(jobDocAddressPK);
		}

		#endregion

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)format(?:[\s]*)job(?:[\s]*)doc(?:[\s]*)address(?:[\s]*)\((?:[\s]*)([^\s]+)(?:[\s]*)(,(.+))?(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
