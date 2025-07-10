using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(StampDutyRechargeRegistryDataType))]
	class StampDutyRechargeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<StampDutyRechargeRegistryDataType>
	{
		#region Implementation

		protected override StampDutyRechargeRegistryDataType GetNewDataType()
		{
			return new StampDutyRechargeRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "StampDutyRechargeRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			StampDutyRecharge item = new StampDutyRecharge();
			item.StampDutyRechargeOrganizationType = Enterprise.Core.Constants.StampDutyRechargeOrganizationType.LocalOrganizations;
			item.StampDutyRechargeTransactionType = Enterprise.Core.Constants.StampDutyRechargeTransactionType.ARInvoice;

			byte[] byteArrayValue = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,83,0,116,0,97,0,109,0,112,0,68,0,117,0,116,0,121,0,82,0,101,0,99,0,104,0,97,0,114,0,103,0,101,0,62,0,60,0,83,0,116,0,97,0,109,0,112,0,68,0,117,
0,116,0,121,0,82,0,101,0,99,0,104,0,97,0,114,0,103,0,101,0,79,0,114,0,103,0,97,0,110,0,105,0,122,0,97,0,116,0,105,0,111,0,110,0,84,0,121,0,112,0,101,0,62,0,76,0,79,0,67,0,60,0,47,0,83,
0,116,0,97,0,109,0,112,0,68,0,117,0,116,0,121,0,82,0,101,0,99,0,104,0,97,0,114,0,103,0,101,0,79,0,114,0,103,0,97,0,110,0,105,0,122,0,97,0,116,0,105,0,111,0,110,0,84,0,121,0,112,0,101,0,62,
0,60,0,83,0,116,0,97,0,109,0,112,0,68,0,117,0,116,0,121,0,82,0,101,0,99,0,104,0,97,0,114,0,103,0,101,0,84,0,114,0,97,0,110,0,115,0,97,0,99,0,116,0,105,0,111,0,110,0,84,0,121,0,112,0,101,
0,62,0,73,0,78,0,86,0,60,0,47,0,83,0,116,0,97,0,109,0,112,0,68,0,117,0,116,0,121,0,82,0,101,0,99,0,104,0,97,0,114,0,103,0,101,0,84,0,114,0,97,0,110,0,115,0,97,0,99,0,116,0,105,0,111,
0,110,0,84,0,121,0,112,0,101,0,62,0,60,0,47,0,83,0,116,0,97,0,109,0,112,0,68,0,117,0,116,0,121,0,82,0,101,0,99,0,104,0,97,0,114,0,103,0,101,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(item, byteArrayValue)
			};
		}

		#endregion
	}
}
