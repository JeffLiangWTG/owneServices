using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE917;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tms;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE917ProviderTest : Business.Testing.DataProviderTestCase<IE917Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE917Provider(null));
		}

		public void TestMrnNumber()
		{
			AssertEquals("20DE41000000001870745", Provider.MrnNumber);
		}

		public void TestMrnNumberSequenceNumber()
		{
			AssertEquals("1", Provider.MrnNumberSequenceNumber);
		}

		public void TestAdministrativeReferenceCode()
		{
			AssertEquals("20DE41000000001870745", Provider.AdministrativeReferenceCode);
		}

		public void TestAdministrativeReferenceCode_AttributesIsNull()
		{
			message.Body.XmlNegativeAcknowledgement.Attributes = null;
			AssertEquals(ZString.Empty, Provider.AdministrativeReferenceCode);
		}

		public void TestErrors()
		{
			CombineAssertions(() =>
			{
				var error = Provider.Errors.Single();
				AssertEquals("Error Column Number", "368", error.ErrorColumnNumber);
				AssertEquals("Error Line Number", "1", error.ErrorLineNumber);
				AssertEquals("Error Reason", "Reason", error.ErrorReason);
				AssertEquals("Error Location", "Location", error.ErrorLocation);
				AssertEquals("Original Attribute Value", "Original Value", error.OriginalAttributeValue);
			});
		}

		protected override IEnumerable<Expression<Func<IE917Provider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Errors;
		}

		protected override IE917Provider GetProvider() => new IE917Provider(message);

		protected override void SetUp()
		{
			base.SetUp();

			message = new Ie917Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 09, 24),
					TimeOfPreparation = "13:29:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new BodyType
				{
					XmlNegativeAcknowledgement = new XmlNegativeAcknowledgementType
					{
						Attributes = new AttributesType
						{
							AdministrativeReferenceCode = "20DE41000000001870745",
							SequenceNumber = "1"
						},
						XmlError = new Collection<XmlErrorType>
						{
							new XmlErrorType
							{
								ErrorColumnNumber = "368",
								ErrorLineNumber = "1",
								ErrorReason = "Reason",
								ErrorLocation = "Location",
								OriginalAttributeValue = "Original Value"
							}
						}
					}
				}
			};
		}

		Ie917Type message;
	}
}
