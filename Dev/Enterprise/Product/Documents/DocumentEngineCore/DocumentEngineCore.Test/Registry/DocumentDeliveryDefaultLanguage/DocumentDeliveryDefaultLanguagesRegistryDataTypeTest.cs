using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentDeliveryDefaultLanguagesRegistryDataType))]
	class DocumentDeliveryDefaultLanguagesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DocumentDeliveryDefaultLanguagesRegistryDataType>
	{
		public void TestDocumentDeliveryDefaultLanguageValidateCore()
		{
			var errorList = new DocumentDeliveryDefaultLanguagesCollection {
				new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address, Order = 3 },
				new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Company, Order = 1 }
			};
			AssertExceptionThrown<RegistryValidationException>("Should thrown an exception", "The Order sequence should equal the number of Fallbacks used.",
				() => DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, errorList));

			var noErrorList = new DocumentDeliveryDefaultLanguagesCollection {
				new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address, Order = 2 },
				new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Company, Order = 1 }
			};
			AssertNoExceptionThrown("Should not thrown an exception", () => DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, noErrorList));
		}

		#region Implementation

		protected override DocumentDeliveryDefaultLanguagesRegistryDataType GetNewDataType()
		{
			return new DocumentDeliveryDefaultLanguagesRegistryDataType();
		}

		protected override string ExpectedEditorName => "DocumentDeliveryDefaultLanguagesRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new DocumentDeliveryDefaultLanguagesCollection();
			var entry1 = collection.AddNew();
			var entry2 = collection.AddNew();
			entry1.Fallback = Enterprise.Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.System;
			entry1.Order = 1;
			entry2.Fallback = Enterprise.Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Company;
			entry2.Order = 2;

			byte[] byteArrayValue = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,68,0,101,0,108,0,105,0,118,0,101,0,114,0,121,0,68,0,101,0,102,
				0,97,0,117,0,108,0,116,0,76,0,97,0,110,0,103,0,117,0,97,0,103,0,101,0,115,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,
				0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,
				0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,
				0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,62,0,60,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,68,0,101,0,108,0,105,0,118,0,101,0,114,0,121,0,68,0,101,
				0,102,0,97,0,117,0,108,0,116,0,76,0,97,0,110,0,103,0,117,0,97,0,103,0,101,0,115,0,62,0,60,0,70,0,97,0,108,0,108,0,98,0,97,0,99,0,107,0,62,0,83,0,121,0,115,0,116,0,101,0,109,0,60,0,47,
				0,70,0,97,0,108,0,108,0,98,0,97,0,99,0,107,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,49,0,60,0,47,0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,68,0,111,0,99,0,117,0,109,0,101,
				0,110,0,116,0,68,0,101,0,108,0,105,0,118,0,101,0,114,0,121,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,76,0,97,0,110,0,103,0,117,0,97,0,103,0,101,0,115,0,62,0,60,0,68,0,111,0,99,0,117,0,109,
				0,101,0,110,0,116,0,68,0,101,0,108,0,105,0,118,0,101,0,114,0,121,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,76,0,97,0,110,0,103,0,117,0,97,0,103,0,101,0,115,0,62,0,60,0,70,0,97,0,108,0,108,
				0,98,0,97,0,99,0,107,0,62,0,67,0,111,0,109,0,112,0,97,0,110,0,121,0,60,0,47,0,70,0,97,0,108,0,108,0,98,0,97,0,99,0,107,0,62,0,60,0,79,0,114,0,100,0,101,0,114,0,62,0,50,0,60,0,47,
				0,79,0,114,0,100,0,101,0,114,0,62,0,60,0,47,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,68,0,101,0,108,0,105,0,118,0,101,0,114,0,121,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,76,0,97,
				0,110,0,103,0,117,0,97,0,103,0,101,0,115,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,68,0,101,0,108,0,105,0,118,0,101,0,114,0,121,
				0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,76,0,97,0,110,0,103,0,117,0,97,0,103,0,101,0,115,0,62,0
			};

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
