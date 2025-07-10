using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CustomsCode : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CustomsCode({orgheaderpk},{countrycode},{typecode})>",
				ResString.GetMultilingualString("292ea1de-aa02-49b1-90dd-122c8776160d", @"Returns a specific customs code for a given organization."),
				new List<(string example, object expectedResult)> { ((NoResString)"<CustomsCode(<OrgPK>, AU, GST)>", (NoResString)"Blaticus") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			ZGuid orgheader;
			ZString result = "";

			try
			{
				orgheader = new ZGuid(match.Groups[1].Value.Trim());
			}
			catch (FormatException)
			{
				return result;
			}
			ZString countryCode = match.Groups[2].Value.Trim();
			ZString typeCode = match.Groups[3].Value.Trim();

			RefCountry country = new RefCountry.Loader(Factory).LoadForCountry(countryCode);

			if (country != null)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(OrgCusCodeSchema.OK_OH, orgheader);
				query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, country.Code);
				query.AddToFilter(OrgCusCodeSchema.OK_CodeType, typeCode);

				OrgCusCode code = Factory.LoadTop1<OrgCusCode>(query);
				if (code != null)
				{
					result = code.OK_CustomsRegNo;
				}
			}

			return result;
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<(?:[\s]*)CustomsCode(?:[\s]*)\((?:[\s]*)([^,]*)(?:[\s]*),(?:[\s]*)([^,]*)(?:[\s]*),(?:[\s]*)([^,]*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
