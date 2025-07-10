using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(PaymentOrReceiptTypeConverter))]
	class PaymentOrReceiptTypeConverterTest : EnumConverterTestCase<PaymentOrReceiptTypeConverter, PaymentOrReceiptType>
	{
		protected override IEnumerable<ZString> GetAllPossibleEnterpriseValues()
		{
			return typeof(ReceiptTypes).GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
				.Select(fieldInfo => (string)fieldInfo.GetValue(null)).ToList().ConvertAll(x => (ZString)x);
		}

		protected override IEnumerable<ZString> GetEnterpriseValuesExcludedFromMapping()
		{
			return new ZString[] { "FCB" }; //FCB is not an actual Receipt Type, rather it is a flag to indicate Foreign Currency Balance Adjustment Journal
		}
	}
}

