using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PaymentTwelveLevelAuthorisationSettingsRegistryDataType))]
	sealed class PaymentTwelveLevelAuthorisationSettingsDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PaymentTwelveLevelAuthorisationSettingsRegistryDataType>
	{
		#region Implementation

		protected override PaymentTwelveLevelAuthorisationSettingsRegistryDataType GetNewDataType()
		{
			return new PaymentTwelveLevelAuthorisationSettingsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "PaymentAuthorisationSettingsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			PaymentTwelveLevelAuthorisationSettingsCollection collection = new PaymentTwelveLevelAuthorisationSettingsCollection();
			PaymentTwelveLevelAuthorisationSettings paymentAuthorisationSettings1 = collection.AddNew();
			paymentAuthorisationSettings1.Amount = 100;
			paymentAuthorisationSettings1.Range = paymentAuthorisationSettings1.RangeList[0].Code;
			paymentAuthorisationSettings1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.EleventhApprovalRequiredOnly;
			PaymentTwelveLevelAuthorisationSettings paymentAuthorisationSettings2 = collection.AddNew();
			paymentAuthorisationSettings2.Amount = 100;
			paymentAuthorisationSettings2.Range = paymentAuthorisationSettings2.RangeList[1].Code;
			paymentAuthorisationSettings2.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.TwelfthApprovalRequiredOnly;

			byte[] byteArrayValue = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,80,0,97,0,121,0,109,0,101,0,110,0,116,0,84,0,119,0,101,0,108,0,118,0,101,0,76,0,101,0,118,0,101,0,108,0,65,
0,117,0,116,0,104,0,111,0,114,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,
0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,32,
0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,
0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,62,0,60,0,80,0,97,0,121,0,109,0,101,0,110,0,116,0,84,0,119,0,101,0,108,
0,118,0,101,0,76,0,101,0,118,0,101,0,108,0,65,0,117,0,116,0,104,0,111,0,114,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,0,60,0,65,0,109,0,111,
0,117,0,110,0,116,0,62,0,49,0,48,0,48,0,60,0,47,0,65,0,109,0,111,0,117,0,110,0,116,0,62,0,60,0,82,0,97,0,110,0,103,0,101,0,62,0,85,0,112,0,32,0,116,0,111,0,60,0,47,0,82,0,97,0,110,
0,103,0,101,0,62,0,60,0,65,0,117,0,116,0,104,0,111,0,114,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,82,0,101,0,113,0,117,0,105,0,114,0,101,0,109,0,101,0,110,0,116,0,62,0,49,0,49,0,116,0,104,
0,32,0,76,0,101,0,118,0,101,0,108,0,32,0,79,0,110,0,108,0,121,0,60,0,47,0,65,0,117,0,116,0,104,0,111,0,114,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,82,0,101,0,113,0,117,0,105,0,114,0,101,
0,109,0,101,0,110,0,116,0,62,0,60,0,47,0,80,0,97,0,121,0,109,0,101,0,110,0,116,0,84,0,119,0,101,0,108,0,118,0,101,0,76,0,101,0,118,0,101,0,108,0,65,0,117,0,116,0,104,0,111,0,114,0,105,0,115,
0,97,0,116,0,105,0,111,0,110,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,0,60,0,80,0,97,0,121,0,109,0,101,0,110,0,116,0,84,0,119,0,101,0,108,0,118,0,101,0,76,0,101,0,118,0,101,0,108,
0,65,0,117,0,116,0,104,0,111,0,114,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,0,60,0,65,0,109,0,111,0,117,0,110,0,116,0,62,0,49,0,48,0,48,
0,60,0,47,0,65,0,109,0,111,0,117,0,110,0,116,0,62,0,60,0,82,0,97,0,110,0,103,0,101,0,62,0,65,0,98,0,111,0,118,0,101,0,60,0,47,0,82,0,97,0,110,0,103,0,101,0,62,0,60,0,65,0,117,0,116,
0,104,0,111,0,114,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,82,0,101,0,113,0,117,0,105,0,114,0,101,0,109,0,101,0,110,0,116,0,62,0,49,0,50,0,116,0,104,0,32,0,76,0,101,0,118,0,101,0,108,0,32,
0,79,0,110,0,108,0,121,0,60,0,47,0,65,0,117,0,116,0,104,0,111,0,114,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,82,0,101,0,113,0,117,0,105,0,114,0,101,0,109,0,101,0,110,0,116,0,62,0,60,0,47,
0,80,0,97,0,121,0,109,0,101,0,110,0,116,0,84,0,119,0,101,0,108,0,118,0,101,0,76,0,101,0,118,0,101,0,108,0,65,0,117,0,116,0,104,0,111,0,114,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,83,0,101,
0,116,0,116,0,105,0,110,0,103,0,115,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,80,0,97,0,121,0,109,0,101,0,110,0,116,0,84,0,119,0,101,0,108,0,118,0,101,0,76,0,101,0,118,0,101,
0,108,0,65,0,117,0,116,0,104,0,111,0,114,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
