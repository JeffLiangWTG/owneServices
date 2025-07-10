using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(RegistrationNumber))]
	class RegistrationNumberTest : DataObjectTestCase<RegistrationNumber>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>()
		{
			{ nameof(RegistrationNumber.Value), OrgCusCodeSchema.OK_CustomsRegNo.MaxLength }
		};
	}
}
