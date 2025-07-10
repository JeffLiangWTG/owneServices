using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(TaxMessageID))]
	class TaxMessageIDTest : DataObjectTestCase<TaxMessageID>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(TaxMessageID.TaxMessageCode), AccInvMsgSchema.A9_Code.MaxLength },
				{ nameof(TaxMessageID.Description), AccInvMsgSchema.A9_Description.MaxLength },
				{ nameof(TaxMessageID.EnglishTaxMessage), AccInvMsgSchema.A9_EnglishMsg.MaxLength }
			};
		}
	}
}

