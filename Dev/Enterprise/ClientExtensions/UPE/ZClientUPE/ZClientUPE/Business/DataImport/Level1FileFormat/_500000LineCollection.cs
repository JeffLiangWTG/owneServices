using System.Collections;
using System.Text.RegularExpressions;

using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public class _500000LineCollection : CollectionBase
	{
		public _500000Line this[int i]
		{
			get { return (_500000Line)List[i]; }
		}

		public int Add(_500000Line value)
		{
			return List.Add(value);
		}

		public void AddRange(_500000LineCollection value)
		{
			foreach (_500000Line item in value)
			{
				List.Add(item);
			}
		}

		public ZString GoodsDescription
		{
			get
			{
				foreach (_500000Line invoiceLine in this)
				{
					if (invoiceLine.Description != "")
					{
						string result = Regex.Replace(invoiceLine.Description, @"\bsamples\b|\bsample\b|\bNCV\b|\bconsol\b", "", RegexOptions.IgnoreCase);
						return Regex.Replace(result, @"[ ]+", " ").Trim().ToUpper();
					}
				}
				return "";
			}
		}
	}
}
