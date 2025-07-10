using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class TransportCoTrackingURL : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<TransportCoTrackingURL({CarrierCode},{CarrierReference})>",
											   ResString.GetMultilingualString("1484dd7f-40d3-4f98-9e3a-2339694438ad",
@"Presents a URL or Hyperlink in the resulting output to the carrier's cartage web site for a job."),
											   new List<(string example, object expectedResult)> { ("<TransportCoTrackingURL(<CarrierCode>, <CarrierRef>)>", new ExcelHyperlink(THyperLinkType.URL, @"http://carrier.com?v=Hello")) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			object result = string.Empty;

			var regMatch = Regex.Match(macro);
			var carrierURL = GetCartageURLFromOHCode(GetTrimmedMatch(regMatch, 1));
			if (!carrierURL.IsEmpty)
			{
				var transportRef = GetTrimmedMatch(regMatch, 2);

				const string urlInsert = Core.Constants.TransportCoHotlinkOpener.CargoWiseREF;
				if (carrierURL.Contains(urlInsert, StringComparison.OrdinalIgnoreCase))
				{
					var urlWithRef = carrierURL.ReplaceIgnoringCase(urlInsert, transportRef);
					if (UrlValidation.IsValidUrl(urlWithRef))
					{
						result = new ExcelHyperlink(THyperLinkType.URL, urlWithRef);
					}
				}
			}

			return result;
		}

		static string GetTrimmedMatch(Match regMatch, int index) => regMatch.Groups[index].Value.Trim();

		static ZString GetCartageURLFromOHCode(string orgCode)
		{
			ZString result = "";

			var carrier = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, orgCode);
			if (carrier != null)
			{
				result = carrier.OrgWebURLs
					.Cast<OrgWebURL>()
					.FirstOrDefault(u => u.PU_Type == OrgWebUrlList.Codes.CartageTracking)?.PU_URL ?? ZString.Empty;
			}

			return result;
		}

		public override Regex Regex => reg;

		static readonly Regex reg = new Regex(@"^<(?:[\s]*)TransportCoTrackingURL(?:[\s]*)\((?:[\s]*)([^\s]*)(?:[\s]*),(?:[\s]*)(.*)(?:[\s]*)\)(?:[\s]*)>$",
											  RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
