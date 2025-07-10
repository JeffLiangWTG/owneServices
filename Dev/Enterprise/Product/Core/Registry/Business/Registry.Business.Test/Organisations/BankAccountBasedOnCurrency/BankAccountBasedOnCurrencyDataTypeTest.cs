using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BankAccountBasedOnCurrencyDataType))]
	sealed class BankAccountBasedOnCurrencyDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BankAccountBasedOnCurrencyDataType>
	{
		#region Implementation

		protected override BankAccountBasedOnCurrencyDataType GetNewDataType()
		{
			return new BankAccountBasedOnCurrencyDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "BankAccountBasedOnCurrencyRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			BankAccountBasedOnCurrencyCollection collection = new BankAccountBasedOnCurrencyCollection();
			BankAccountBasedOnCurrency bankAccountBasedOnCurrency = collection.AddNew();

			bankAccountBasedOnCurrency.Currency = "AUD";
			bankAccountBasedOnCurrency.DoNotPerformListValidationOnBankAccount = true;
			bankAccountBasedOnCurrency.BankAccount = new ZGuid("9a2b1218-2998-4390-9f85-1d42fe69352e");

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,
				49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
				0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,66,0,97,0,
				110,0,107,0,65,0,99,0,99,0,111,0,117,0,110,0,116,0,66,0,97,0,115,0,101,0,100,0,79,0,110,
				0,67,0,117,0,114,0,114,0,101,0,110,0,99,0,121,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,
				0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,
				0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,
				0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,
				0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,
				0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,
				0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,66,0,97,0,110,0,107,0,65,0,99,0,99,
				0,111,0,117,0,110,0,116,0,66,0,97,0,115,0,101,0,100,0,79,0,110,0,67,0,117,0,114,0,114,0,101,
				0,110,0,99,0,121,0,62,0,60,0,67,0,117,0,114,0,114,0,101,0,110,0,99,0,121,0,62,0,65,0,85,0,
				68,0,60,0,47,0,67,0,117,0,114,0,114,0,101,0,110,0,99,0,121,0,62,0,60,0,66,0,97,0,110,0,107,
				0,65,0,99,0,99,0,111,0,117,0,110,0,116,0,62,0,57,0,97,0,50,0,98,0,49,0,50,0,49,0,56,0,45,0,
				50,0,57,0,57,0,56,0,45,0,52,0,51,0,57,0,48,0,45,0,57,0,102,0,56,0,53,0,45,0,49,
				0,100,0,52,0,50,0,102,0,101,0,54,0,57,0,51,0,53,0,50,0,101,0,60,0,47,0,66,0,97,0,110,0,107,
				0,65,0,99,0,99,0,111,0,117,0,110,0,116,0,62,0,60,0,47,0,66,0,97,0,110,0,107,0,65,0,99,
				0,99,0,111,0,117,0,110,0,116,0,66,0,97,0,115,0,101,0,100,0,79,0,110,0,67,0,117,0,114,0,114,0,
				101,0,110,0,99,0,121,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,66,0,97,0,110,
				0,107,0,65,0,99,0,99,0,111,0,117,0,110,0,116,0,66,0,97,0,115,0,101,0,100,0,79,0,110,0,67,0,
				117,0,114,0,114,0,101,0,110,0,99,0,121,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
