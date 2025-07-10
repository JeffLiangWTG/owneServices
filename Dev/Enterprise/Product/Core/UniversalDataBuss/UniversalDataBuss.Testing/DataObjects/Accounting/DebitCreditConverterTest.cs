using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(DebitCreditConverter))]
	class DebitCreditConverterTest : EnumConverterTestCase<DebitCreditConverter, DebitCredit>
	{
		protected override IEnumerable<ZString> GetAllPossibleEnterpriseValues()
		{
			return typeof(Constants.DebitCredit).GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
				.Select(fieldInfo => (string)fieldInfo.GetValue(null)).ToList().ConvertAll(x => (ZString)x);
		}
	}
}
