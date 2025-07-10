using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.Core.Testing
{
	[TestedType(typeof(LegTypeConverter))]
	class LegTypeConverterTest : EnumConverterTestCase<LegTypeConverter, LegType>
	{
		protected override IEnumerable<ZString> GetAllPossibleEnterpriseValues()
		{
			var legTypes = typeof(Constants.TransportPlanningType).GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
				.Select(fieldInfo => (string)fieldInfo.GetValue(null)).ToList().ConvertAll(x => (ZString)x);

			legTypes.Add("LTL"); //used for local transport booking

			return legTypes;
		}

		protected override IEnumerable<ZString> GetEnterpriseValuesExcludedFromMapping()
		{
			return new ZString[] { "OBC", "UNA" };
		}
	}
}
