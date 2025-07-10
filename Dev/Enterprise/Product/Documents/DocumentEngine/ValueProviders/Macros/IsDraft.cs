using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class IsDraft : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(
				"<IsDraft>",
				ResString.GetMultilingualString("43701432-4805-4b92-ab3f-36bd98c4f16e", "Returns a value that indicates whether this document is a draft."),
				new List<(string example, object expectedResult)> { ("<IsDraft>", "N") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			string result = ZBool.False.ToYN();

			var deliveryInstructions = report?.Parent?.DeliveryInstructions;
			if (deliveryInstructions != null)
			{
				result = deliveryInstructions.IsDraft.ToYN();
			}
			else
			{
				deliveryInstructions = report?.DataProviderList?.AllDataProviders?.OfType<DeliveryInstructions>().FirstOrDefault();
				if (deliveryInstructions != null)
				{
					result = deliveryInstructions.IsDraft.ToYN();
				}
			}

			return result;
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex(@"^<(?:[\s]*)IsDraft(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
