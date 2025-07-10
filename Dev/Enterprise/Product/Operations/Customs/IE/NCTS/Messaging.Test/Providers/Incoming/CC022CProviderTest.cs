using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC022C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC022CProviderTest : TestCaseWithFactory
	{
		public void TestMRN()
		{
			AssertEquals("MRN022", provider.MRN);
		}

		public void TestAmendNotificationDateAndTime()
		{
			AssertEquals(new DateTime(2023, 02, 22, 2, 3, 6), provider.AmendmentNotificationDateAndTime);
		}

		public void TestFunctionalErrors()
		{
			AssertEquals("FunctionalErrors is empty", 0, GetEmptyProvider().FunctionalErrors.Count);

			var functionalErrors1 = provider.FunctionalErrors;
			AssertType<CC022CFunctionalErrorProvider>(functionalErrors1.ElementAt(0));
			var functionalErrors2 = provider.FunctionalErrors;
			AssertSame("Is cached", functionalErrors1, functionalErrors2);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC022CProvider(new Cc022CType
			{
				MessageType = MessageTypes.Cc022C,
				TransitOperation = new TransitOperationType09
				{
					Mrn = "MRN022",
					AmendmentNotificationDateAndTime = new DateTime(2023, 02, 22, 2, 3, 6),
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "DEPNUM01"
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType15
				{
					IdentificationNumber = "IDNUMABC000012345",
					TirHolderIdentificationNumber = "IDNUMABC123450000",
					Name = "BOB THE BUILDER",
					Address = new AddressType15
					{
						City = "CITY",
						Country = "ES",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					},
				},
				FunctionalError = new Collection<FunctionalErrorType01>
				{
					new FunctionalErrorType01
					{
						SequenceNumber = "1",
						ErrorPointer = "EP01",
						ErrorCode = AesNctsP5FunctionalErrorCodes.Item26,
						ErrorReason = "ER0001",
						OriginalAttributeValue = "Original value 1"
					},
					new FunctionalErrorType01
					{
						SequenceNumber = "1",
						ErrorPointer = "EP02",
						ErrorCode = AesNctsP5FunctionalErrorCodes.Item12,
						ErrorReason = "ER0022",
						OriginalAttributeValue = "Previous value 2"
					},
				}
			});
		}
		CC022CProvider provider;

		CC022CProvider GetEmptyProvider()
		{
			return new CC022CProvider(new Cc022CType
			{
				TransitOperation = new TransitOperationType09 { },
			});
		}
	}
}
