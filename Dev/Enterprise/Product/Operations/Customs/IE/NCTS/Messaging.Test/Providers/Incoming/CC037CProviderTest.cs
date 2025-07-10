using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC037C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC037CProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("CC037C xmlObject missing", () => new CC037CProvider(null));
			});
		}

		public void TestNullRequester()
		{
			CombineAssertions(() =>
			{
				var emptyProvider = new CC037CProvider(new Cc037CType());
				AssertEquals("Null value should return empty", ZString.Empty, emptyProvider.RequesterIdentificationNumber);
				AssertEquals("Null value should return empty", ZString.Empty, emptyProvider.RequesterIdentificationRole);
			});
		}

		public void TestRequesterIdentificationNumber()
		{
			AssertEquals("RequesterIdentificationNumber", "IN037", provider.RequesterIdentificationNumber);
		}

		public void TestRequesterIdentificationRole()
		{
			AssertEquals("RequesterIdentificationRole", "1", provider.RequesterIdentificationRole);
		}

		public void TestGuaranteeReferences()
		{
			var emptyProvider = new CC037CProvider(new Cc037CType
			{
				GuaranteeReference = null,
			});
			AssertEquals("ValidityLimitation is null", 0, emptyProvider.GuaranteeReferences.Count);

			var guaranteeReference1 = provider.GuaranteeReferences;
			AssertType<CC037CGuaranteeReferenceProvider>(guaranteeReference1.ElementAt(0));
			var guaranteeReference2 = provider.GuaranteeReferences;
			AssertSame("Is cached", guaranteeReference1, guaranteeReference2);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC037CProvider(new Cc037CType
			{
				MessageType = MessageTypes.Cc037C,
				Requester = new RequesterType02
				{
					IdentificationNumber = "IN037",
					Role = "1",
				},
				CustomsOfficeOfGuarantee = new CustomsOfficeOfGuaranteeType02
				{
					ReferenceNumber = "RNCC037C",
				},
				GuaranteeReference = new Collection<GuaranteeReferenceType07>
				{
					new GuaranteeReferenceType07
					{
						SequenceNumber = "1",
						Grn =  "12GRNCC055C012345A678901",
						AcceptanceDateAndTime = new DateTime(2023, 01, 31, 10, 22, 11),
						GuaranteeMonitoringCode = "7",
						GuaranteeQuery = new GuaranteeQueryType
						{
							QueryIdentifier = "1",
							PeriodFromDate = new DateTime(2023, 01, 31, 10, 22, 11),
							PeriodToDate = new DateTime(2023, 02, 28, 9, 8, 7),
						},
					},
				},
			});
		}
		CC037CProvider provider;
	}
}
