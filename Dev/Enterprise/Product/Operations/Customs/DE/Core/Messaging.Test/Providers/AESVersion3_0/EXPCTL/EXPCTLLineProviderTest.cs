using System;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPCTLLineProvider))]
	sealed class EXPCTLLineProviderTest : InboundDataProviderTestCase<IEXPCTLLine, EXPCTLLineProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPCTLLineProvider(null));
		}

		[ExpectNoExceptions]
		public void TestSequenceNumber()
		{
			typeOfControls.sequenceNumber = "23";
			NUnit.Framework.Assert.That(dataProvider.SequenceNumber, Is.EqualTo("23"));
		}

		[ExpectNoExceptions]
		public void TestControlMeasureType()
		{
			typeOfControls.type = DEXPLDTypeOfControlsType.Item10;
			NUnit.Framework.Assert.That(dataProvider.ControlMeasureType, Is.EqualTo("10"));
		}

		[ExpectNoExceptions]
		public void TestAnnotation()
		{
			typeOfControls.text = "Description of the error";
			NUnit.Framework.Assert.That(dataProvider.Annotation, Is.EqualTo("Description of the error"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			typeOfControls = new DEXPLDTypeOfControls();
			dataProvider = new EXPCTLLineProvider(typeOfControls);
		}
		DEXPLDTypeOfControls typeOfControls;
		IEXPCTLLine dataProvider;

		protected override EXPCTLLineProvider GetProvider() => (EXPCTLLineProvider)dataProvider;
	}
}
