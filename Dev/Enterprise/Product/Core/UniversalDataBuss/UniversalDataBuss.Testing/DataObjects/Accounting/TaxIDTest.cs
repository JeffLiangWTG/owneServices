using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(TaxID))]
	class TaxIDTest : DataObjectTestCase<TaxID>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(TaxID.TaxCode), AccTaxRateSchema.AT_Code.MaxLength },
				{ nameof(TaxID.Description), AccTaxRateSchema.AT_Description.MaxLength }
			};
		}
	}
}

