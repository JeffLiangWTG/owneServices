using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestedType(typeof(ERRNCKErrorProvider))]
	sealed class ERRNCKErrorProviderTest : InboundDataProviderTestCase<IERRNCKError, ERRNCKErrorProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new ERRNCKErrorProvider(null));
		}

		[ExpectNoExceptions]
		public void TestCode()
		{
			error.errorCode = "AAA00000";
			NUnit.Framework.Assert.That(dataProvider.Code, Is.EqualTo("AAA00000"));
		}

		[ExpectNoExceptions]
		public void TestPointer()
		{
			error.errorPointer = "A101";
			NUnit.Framework.Assert.That(dataProvider.Pointer, Is.EqualTo("A101"));
		}

		[ExpectNoExceptions]
		public void TestText()
		{
			error.errorText = "TEXT101";
			NUnit.Framework.Assert.That(dataProvider.Text, Is.EqualTo("TEXT101"));
		}

		[ExpectNoExceptions]
		public void TestOriginalValue()
		{
			error.originalAttributeValue = "Original12345";
			NUnit.Framework.Assert.That(dataProvider.OriginalValue, Is.EqualTo("Original12345"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			error = new DEERRGError();
			dataProvider = new ERRNCKErrorProvider(error);
		}
		DEERRGError error;
		IERRNCKError dataProvider;

		protected override ERRNCKErrorProvider GetProvider() => (ERRNCKErrorProvider)dataProvider;
	}
}
