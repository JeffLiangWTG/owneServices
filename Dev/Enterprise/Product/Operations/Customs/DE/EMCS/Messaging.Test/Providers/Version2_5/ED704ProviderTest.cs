using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	[TestedType(typeof(ED704Provider))]
	class ED704ProviderTest : InboundDataProviderTestCase<IED704, ED704Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED704Provider(null));
		}
		public void TestMessageGroup()
		{
			AssertEquals("EME", dataProvider.MessageGroup);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", dataProvider.MessageIdentifier);
		}

		public void TestCorrelationIdentifier()
		{
			AssertEquals("00000000000215", dataProvider.CorrelationIdentifier);
		}

		public void TestAdministrativeReferenceCode()
		{
			AssertEquals("MRN98761234", dataProvider.AdministrativeReferenceCode);
		}

		public void TestLocalReferenceNumber()
		{
			AssertEquals("B000222547896254786321", dataProvider.LocalReferenceNumber);
		}

		public void TestAdministrativeReferenceCode_AttributesIsNull()
		{
			message.Body.GenericRefusalMessage.Attributes = null;
			AssertEquals(ZString.Empty, dataProvider.AdministrativeReferenceCode);
		}

		public void TestLocalReferenceNumber_AttributesIsNull()
		{
			message.Body.GenericRefusalMessage.Attributes = null;
			AssertEquals(ZString.Empty, dataProvider.LocalReferenceNumber);
		}

		public void TestErrors()
		{
			message.Body.GenericRefusalMessage.Error = new[]
			{
				new ED704CBodyGenericRefusalMessageError(),
				new ED704CBodyGenericRefusalMessageError()
			};

			CombineAssertions(() =>
			{
				var errors = dataProvider.Errors;
				AssertEquals("Count", 2, errors.Count);
				AssertSame("Cached", errors, dataProvider.Errors);
			});
		}

		public void TestErrorProviderConstructor()
		{
			message.Body.GenericRefusalMessage.Error = new ED704CBodyGenericRefusalMessageError[]
			{
				null
			};
			AssertExceptionThrown<ArgumentException>(() => _ = dataProvider.Errors);
		}

		public void TestErrorProviderValues()
		{
			message.Body.GenericRefusalMessage.Error = new[]
			{
				new ED704CBodyGenericRefusalMessageError { ErrorNumber = "EMCS-30143", LineNumber = "1", ColumnNumber = "2", ErrorType = ED704CBodyGenericRefusalMessageErrorErrorType.Item15, ErrorReason = "Incorrect value" }
			};

			CombineAssertions(() =>
			{
				var error = dataProvider.Errors.Single();
				AssertEquals("Error Number", "EMCS-30143", error.ErrorNumber);
				AssertEquals("Line Number", "1", error.LineNumber);
				AssertEquals("Column Number", "2", error.ColumnNumber);
				AssertEquals("Error Type", "15", error.ErrorType);
				AssertEquals("Error Reason", "Incorrect value", error.ErrorReason);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED704C
			{
				Header = new ED704CHeader
				{
					CorrelationIdentifier = "00000000000215",
					MessageGroup = ED704CHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102"
				},
				Body = new ED704CBody
				{
					GenericRefusalMessage = new ED704CBodyGenericRefusalMessage
					{
						Attributes = new ED704CBodyGenericRefusalMessageAttributes
						{
							AdministrativeReferenceCode = "MRN98761234",
							LocalReferenceNumber = "B000222547896254786321"
						}
					}
				}
			};
			dataProvider = new ED704Provider(message);
		}
		IED704 dataProvider;
		ED704C message;

		protected override ED704Provider GetProvider()
		{
			message.Body.GenericRefusalMessage.Error = new[]
			{
				new ED704CBodyGenericRefusalMessageError(),
				new ED704CBodyGenericRefusalMessageError()
			};
			return (ED704Provider)dataProvider;
		}
	}
}
