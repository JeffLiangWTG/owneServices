using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.Registry.Business;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class RecipientSalutation : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<RecipientSalutation>",
				ResString.GetMultilingualString("5079b8fa-db9a-4f6a-a5d1-733dd2721414", "Returns the Salutation for the Contact the Report or Document is supposed to be sent to."),
				new List<(string example, object expectedResult)> { ("<RecipientSalutation>", (NoResString)"Dear John Doe") });
		}

		[CodeStringFinderHint(typeof(SalutationHelper), "LoadDefaultSalutations")]
		protected override object GetReplacementCore(string macro, Report report)
		{
			string result = "";

			if (report.DeliveryContact != null)
			{
				if (!report.DeliveryContact.Salutation.IsEmpty && report.DeliveryContact.Contact != null)
				{
					result = report.DeliveryContact.Salutation;
					if (result.Contains(Core.Constants.SalutationMacros.Name) || result.Contains(Core.Constants.SalutationMacros.JobCategory))
					{
						result = result.Replace(Core.Constants.SalutationMacros.Name, report.DeliveryContact.Name).
							Replace(Core.Constants.SalutationMacros.JobCategory, report.DeliveryContact.Contact.OC_JobCategory);
					}
				}
				else
				{
					result = (string)new RecipientName().GetReplacement(macro, report);
				}
			}

			return result;
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<\s*recipient\s*salutation\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
