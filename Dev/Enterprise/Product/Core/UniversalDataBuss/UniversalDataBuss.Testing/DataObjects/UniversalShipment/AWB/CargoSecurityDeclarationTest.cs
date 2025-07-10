using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.AWB.Testing
{
	[TestedType(typeof(CargoSecurityDeclaration))]
	sealed class CargoSecurityDeclarationTest : DataObjectTestCase<CargoSecurityDeclaration>
	{
		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(CargoSecurityDeclaration.TSASecurityStatement),
			nameof(CargoSecurityDeclaration.AdditionalSecurityInformation)
		};
	}
}
