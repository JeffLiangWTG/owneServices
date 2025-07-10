using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(ChargeCode))]
	class ChargeCodeTest : DataObjectTestCase<ChargeCode>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(ChargeCode.Code), AccChargeCodeSchema.AC_Code.MaxLength },
				{ nameof(ChargeCode.Description), AccChargeCodeSchema.AC_Desc.MaxLength }
			};
		}
	}
}

