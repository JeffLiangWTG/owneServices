using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceCopiesRegistryDataType))]
	class InvoiceCopiesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<InvoiceCopiesRegistryDataType>
	{
		#region Implementation

		protected override InvoiceCopiesRegistryDataType GetNewDataType()
		{
			return new InvoiceCopiesRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "InvoiceCopiesRegistryItemEditor"; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			base.AssertValuesEqual(message, lhs, rhs);
			AssertEquals("IsOriginal", ((InvoiceCopy)lhs).IsOriginal, ((InvoiceCopy)rhs).IsOriginal);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			InvoiceCopyCollection collection = new InvoiceCopyCollection();
			InvoiceCopy copy = collection.AddNew();

			copy.Name = (NoResString)"Name";
			copy.DeliveryMethod = "ALL";
			copy.IncludeTradingTerms = true;
			copy.IsOriginal = true;
			copy.Order = 1;
			copy.Message = "copy legend";

			byte[] byteArrayValue = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,73,0,110,0,118,0,111,0,105,0,99,0,101,0,67,0,111,0,112,0,121,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,
				0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,
				0,101,0,109,0,97,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,
				0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,62,0,60,0,73,0,110,0,118,0,111,0,105,0,99,
				0,101,0,67,0,111,0,112,0,121,0,62,0,60,0,78,0,97,0,109,0,101,0,62,0,78,0,97,0,109,0,101,0,60,0,47,0,78,0,97,0,109,0,101,0,62,0,60,0,68,0,101,0,108,0,105,0,118,0,101,0,114,0,121,0,77,
				0,101,0,116,0,104,0,111,0,100,0,62,0,65,0,76,0,76,0,60,0,47,0,68,0,101,0,108,0,105,0,118,0,101,0,114,0,121,0,77,0,101,0,116,0,104,0,111,0,100,0,62,0,60,0,73,0,110,0,99,0,108,0,117,0,100,
				0,101,0,84,0,114,0,97,0,100,0,105,0,110,0,103,0,84,0,101,0,114,0,109,0,115,0,62,0,89,0,60,0,47,0,73,0,110,0,99,0,108,0,117,0,100,0,101,0,84,0,114,0,97,0,100,0,105,0,110,0,103,0,84,0,101,
				0,114,0,109,0,115,0,62,0,60,0,73,0,115,0,79,0,114,0,105,0,103,0,105,0,110,0,97,0,108,0,62,0,89,0,60,0,47,0,73,0,115,0,79,0,114,0,105,0,103,0,105,0,110,0,97,0,108,0,62,0,60,0,79,0,114,
				0,100,0,101,0,114,0,62,0,49,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,62,0,99,0,111,0,112,0,121,0,32,0,108,0,101,0,103,0,101,0,110,0,100,
				0,60,0,47,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,62,0,60,0,47,0,73,0,110,0,118,0,111,0,105,0,99,0,101,0,67,0,111,0,112,0,121,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,
				0,73,0,110,0,118,0,111,0,105,0,99,0,101,0,67,0,111,0,112,0,121,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
