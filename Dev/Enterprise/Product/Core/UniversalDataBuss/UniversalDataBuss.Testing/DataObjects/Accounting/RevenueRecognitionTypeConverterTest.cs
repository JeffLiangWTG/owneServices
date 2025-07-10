using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(RevenueRecognitionTypeConverter))]
	class RevenueRecognitionTypeConverterTest : EnumConverterTestCase<RevenueRecognitionTypeConverter, RevenueRecognitionType>
	{
		protected override IEnumerable<ZString> GetAllPossibleEnterpriseValues()
		{
			return typeof(MasterFiles.Business.RevenueRecognitionLookups.RecognitionDateOptionCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
				.Select(fieldInfo => (string)fieldInfo.GetValue(null)).ToList().ConvertAll(x => (ZString)x);
		}
	}
}

