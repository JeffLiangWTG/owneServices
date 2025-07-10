using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(TransactionTypeConverter))]
	class TransactionTypeConverterTest : EnumConverterTestCase<TransactionTypeConverter, TransactionType>
	{
		protected override IEnumerable<ZString> GetAllPossibleEnterpriseValues()
		{
			var transactionTypes = typeof(TransactionTypes).GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
				.Select(fieldInfo => (string)fieldInfo.GetValue(null)).ToList().ConvertAll(x => (ZString)x);

			var lineTypes = typeof(TransactionLineTypes).GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
				.Select(fieldInfo => (string)fieldInfo.GetValue(null)).ToList().ConvertAll(x => (ZString)x);

			transactionTypes.AddRange(lineTypes);

			return transactionTypes;
		}

		protected override IEnumerable<ZString> GetEnterpriseValuesExcludedFromMapping()
		{
			return new ZString[] { "WAJ", "RCB", "DDB", "INB", "UAC", "UAI", "IPA", "CPA", "INI", "INC", "INA", "UCT" }; //These transaction types and lines types are not possible to export in Universal Export
		}
	}
}

