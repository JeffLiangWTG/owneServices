using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
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
			error.Code = "101";
			NUnit.Framework.Assert.That(dataProvider.Code, Is.EqualTo("101"));
		}

		[ExpectNoExceptions]
		public void TestPointer()
		{
			error.Pointer = "101";
			NUnit.Framework.Assert.That(dataProvider.Pointer, Is.EqualTo("101"));
		}

		[ExpectNoExceptions]
		public void TestText()
		{
			error.Text = "TEXT101";
			NUnit.Framework.Assert.That(dataProvider.Text, Is.EqualTo("TEXT101"));
		}

		[ExpectNoExceptions]
		public void TestOriginalValue()
		{
			error.OriginalValue = "OriginalValue here";
			NUnit.Framework.Assert.That(dataProvider.OriginalValue, Is.EqualTo("OriginalValue here"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			error = new DEERRFError();
			dataProvider = new ERRNCKErrorProvider(error);
		}
		DEERRFError error;
		IERRNCKError dataProvider;

		protected override ERRNCKErrorProvider GetProvider() => (ERRNCKErrorProvider)dataProvider;
	}
}
