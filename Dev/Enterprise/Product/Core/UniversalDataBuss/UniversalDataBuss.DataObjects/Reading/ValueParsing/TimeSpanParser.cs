using System;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public class TimeSpanParser : XmlTypeBaseParser<TimeSpan>
	{
		protected override bool TryParse(string sourceValue, out TimeSpan typedResult)
		{
			if (!string.IsNullOrWhiteSpace(sourceValue))
			{
				try
				{
					typedResult = System.Xml.XmlConvert.ToTimeSpan(sourceValue);
					return true;
				}
				catch (FormatException)
				{
				}
			}

			typedResult = TimeSpan.Zero;
			return false;
		}

		protected override string ValidValueDescription
		{
			get { return Res.GetString("48cfa4f0-e0aa-4fbc-8ee7-6bc5b414a111", "Duration"); }
		}
	}
}
