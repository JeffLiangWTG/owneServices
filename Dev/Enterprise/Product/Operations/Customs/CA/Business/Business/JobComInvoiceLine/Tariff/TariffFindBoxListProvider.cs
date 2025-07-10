using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class TariffFindBoxListProvider : FindBoxListProvider
	{
		public TariffFindBoxListProvider(IBusinessObjectCollection collection)
			: base(collection)
		{
		}

		#region Implementation

		protected override void AddCodeEqualsFilter(ZQuery query, string code)
		{
			base.AddCodeEqualsFilter(query, TariffFormatter.Format(code));
		}

		protected override void AddCodeStartsWithFilter(ZQuery query, string code)
		{
			base.AddCodeStartsWithFilter(query, TariffFormatter.Format(code));
		}

		#region DescriptionFromCode

		public override string DescriptionFromCode(string code)
		{
			var result = base.DescriptionFromCode(code);
			if (!string.IsNullOrEmpty(result))
			{
				code = TariffFormatter.Format(code);
				var descriptionDataList = new List<string>();
				descriptionDataList.Add(result);
				var spiltIndex = -1;
				for (var length = code.Length - 1; length > 3; length--)
				{
					var description = base.DescriptionFromCode(code.Substring(0, length));
					if (!string.IsNullOrEmpty(description))
					{
						if (!Regex.IsMatch(description, "^Other$", RegexOptions.IgnoreCase))
						{
							AddDotToEndOf(ref description);
						}
						if (length % 2 == 1 && spiltIndex == -1)
						{
							spiltIndex = descriptionDataList.Count;
						}
						descriptionDataList.Add(description);
					}
				}

				spiltIndex = spiltIndex == -1 ? 0 : spiltIndex;

				var details = descriptionDataList.GetRange(0, spiltIndex + 1);
				details.Reverse();
				var summarys = descriptionDataList.GetRange(spiltIndex + 1, descriptionDataList.Count - spiltIndex - 1);
				summarys.Reverse();

				var detailDesc = string.Join(" ", details);
				var summaryDesc = string.Join(" ", summarys);

				if (spiltIndex > 0 || summarys.Count == 0)
				{
					AddDotToEndOf(ref detailDesc);
				}

				if (summarys.Count == 0)
				{
					result = detailDesc;
				}
				else
				{
					AddDotToEndOf(ref summaryDesc);
					result = detailDesc + " (" + summaryDesc + ")";
				}
			}
			return result;
		}

		void AddDotToEndOf(ref string value)
		{
			value = value.Trim();
			if (!char.IsPunctuation(value.Last()))
			{
				value += '.';
			}
		}
		#endregion

		protected TariffFormatter TariffFormatter
		{
			get { return tariffFormatter ?? (tariffFormatter = new TariffFormatter()); }
		}
		TariffFormatter tariffFormatter;

		#endregion
	}
}
