using System;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public class ZBoolParser : ZTypeBaseParser<ZBool>
	{
		protected override bool TryParse(string sourceValue, out ZBool typedResult)
		{
			return ZBool.TryParse(sourceValue, out typedResult);
		}

		protected override string ValidValueDescription
		{
			get { return Res.GetString("a93a7cc5-a50a-4f3e-93dc-698449ef8b06", "Boolean (true or false)"); }
		}

		protected override bool IsValidWhenEmpty(Type targetType)
		{
			return false;
		}
	}
}
