using System;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public class ZBlobParser : ZTypeBaseParser<ZBlob>
	{
		protected override bool TryParse(string sourceValue, out ZBlob typedResult)
		{
			if (!string.IsNullOrWhiteSpace(sourceValue))
			{
				try
				{
					typedResult = new ZBlob(Convert.FromBase64String(sourceValue));
					return true;
				}
				catch (FormatException)
				{
				}
			}

			typedResult = ZBlob.Empty;
			return false;
		}

		protected override string ValidValueDescription
		{
			get { return Res.GetString("d86c7738-059c-4d96-b7ab-62a2e2b148e9", "Blob"); }
		}
	}
}
