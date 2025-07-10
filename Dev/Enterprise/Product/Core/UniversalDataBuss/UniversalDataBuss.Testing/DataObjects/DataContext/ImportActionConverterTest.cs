using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(ImportActionConverter))]
	sealed class ImportActionConverterTest : EnumConverterTestCase<ImportActionConverter, ImportAction>
	{
		protected override IEnumerable<ZString> GetAllPossibleEnterpriseValues() => Enum.GetNames(typeof(ImportAction)).Select(value => new ZString(value));
	}
}
