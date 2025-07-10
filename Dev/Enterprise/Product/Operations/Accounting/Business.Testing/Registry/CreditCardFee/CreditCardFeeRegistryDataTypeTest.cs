using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CreditCardFeeRegistryDataType))]
	class CreditCardFeeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CreditCardFeeRegistryDataType>
	{
		#region Implementation

		protected override CreditCardFeeRegistryDataType GetNewDataType()
		{
			return new CreditCardFeeRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CreditCardFeeRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			BusinessObject chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, new ZGuid("0A4A1216-BBC5-4674-8EFC-F556578F5F7D")));

			CreditCardFeeCollection collection = new CreditCardFeeCollection();

			CreditCardFee creditCardFee = collection.AddNew();
			creditCardFee.ChargeCodePK = chargeCode.PK;
			creditCardFee.Percentage = 10.10;

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
				0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,114,0,101,0,100,0,105,0,116,0,67,0,97,0,114,0,100,0,70,0,101,0,101,0,32,0,120,0,109,0,108,0,110,
				0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,
				0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,
				0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,67,0,114,0,101,
				0,100,0,105,0,116,0,67,0,97,0,114,0,100,0,70,0,101,0,101,0,62,0,60,0,67,0,104,0,97,0,114,0,103,0,101,0,67,0,111,0,100,0,101,0,80,0,75,0,62,0,48,0,97,0,52,0,97,0,49,0,50,0,49,0,54,
				0,45,0,98,0,98,0,99,0,53,0,45,0,52,0,54,0,55,0,52,0,45,0,56,0,101,0,102,0,99,0,45,0,102,0,53,0,53,0,54,0,53,0,55,0,56,0,102,0,53,0,102,0,55,0,100,0,60,0,47,0,67,0,104,0,97,
				0,114,0,103,0,101,0,67,0,111,0,100,0,101,0,80,0,75,0,62,0,60,0,80,0,101,0,114,0,99,0,101,0,110,0,116,0,97,0,103,0,101,0,62,0,49,0,48,0,46,0,49,0,60,0,47,0,80,0,101,0,114,0,99,0,101,
				0,110,0,116,0,97,0,103,0,101,0,62,0,60,0,47,0,67,0,114,0,101,0,100,0,105,0,116,0,67,0,97,0,114,0,100,0,70,0,101,0,101,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,114,
				0,101,0,100,0,105,0,116,0,67,0,97,0,114,0,100,0,70,0,101,0,101,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion

		#region Factory

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}

				return factory;
			}
		}

		#endregion
	}
}
