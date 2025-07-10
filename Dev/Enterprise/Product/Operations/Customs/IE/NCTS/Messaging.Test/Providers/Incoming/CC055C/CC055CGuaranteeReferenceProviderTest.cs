using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public class CC055CGuaranteeReferenceProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("GuaranteeReferenceType missing", () => new CC055CGuaranteeReferenceProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber", 1, provider.SequenceNumber);
		}

		public void TestGRN()
		{
			AssertEquals("GRN", "12GRNCC055C012345A678901", provider.GRN);
		}

		public void TestInvalidGuaranteeReason()
		{
			AssertEquals("InvalidGuaranteeReasons is empty", 0, GetEmptyProvider().InvalidGuaranteeReasons.Count());

			var invalidGuaranteeReason1 = provider.InvalidGuaranteeReasons.ElementAt(0);
			AssertEquals("SequenceNumber", 1, invalidGuaranteeReason1.SequenceNumber);
			AssertEquals("Code", "AB", invalidGuaranteeReason1.InvalidGuaranteeReasonCode);
			AssertEquals("Text", "Some text", invalidGuaranteeReason1.InvalidGuaranteeReasonText);

			var invalidGuaranteeReason2 = provider.InvalidGuaranteeReasons.ElementAt(1);
			AssertEquals("SequenceNumber", 2, invalidGuaranteeReason2.SequenceNumber);
			AssertEquals("Code", "CD", invalidGuaranteeReason2.InvalidGuaranteeReasonCode);
			AssertEquals("Text", "Another text", invalidGuaranteeReason2.InvalidGuaranteeReasonText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC055CGuaranteeReferenceProvider(new GuaranteeReferenceType08
			{
				SequenceNumber = "1",
				Grn = "12GRNCC055C012345A678901",
				InvalidGuaranteeReason = new Collection<InvalidGuaranteeReasonType01>
						{
							new InvalidGuaranteeReasonType01
							{
								SequenceNumber = "1",
								Code = "AB",
								Text = "Some text",
							},
							new InvalidGuaranteeReasonType01
							{
								SequenceNumber = "2",
								Code = "CD",
								Text = "Another text",
							}
						},
			});
		}
		CC055CGuaranteeReferenceProvider provider;

		CC055CGuaranteeReferenceProvider GetEmptyProvider()
		{
			return new CC055CGuaranteeReferenceProvider(new GuaranteeReferenceType08
			{
			});
		}
	}
}
