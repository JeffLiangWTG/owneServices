using System;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC509C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC509CProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN509", Provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN509", Provider.MovementReferenceNumber);
		}
		public void TestInvalidationDecisionDateAndTime()
		{
			AssertEquals(new ZDateTime(2022, 02, 07, 15, 10, 30), Provider.InvalidationDecisionDateAndTime);
		}
		public void TestInvalidationRequestDateAndTime()
		{
			AssertEquals(new ZDateTime(2022, 02, 07, 15, 09, 59), Provider.InvalidationRequestDateAndTime);
		}
		public void TestIsInvalidationInitiatedByCustoms()
		{
			AssertEquals(ZBool.True, Provider.IsInvalidationInitiatedByCustoms);
		}
		public void TestInvalidationJustification()
		{
			AssertEquals("Invalidation Justification", Provider.InvalidationJustification);
		}

		CC509CProvider Provider
		{
			get
			{
				if (fProvider == null)
				{
					fProvider = new CC509CProvider(
						new Cc509C
						{
							ExportOperation = new ExportOperationType05
							{
								Lrn = "LRN509",
								Mrn = "MRN509",
								InvalidationDecisionDateAndTime = new DateTime(2022, 02, 07, 15, 10, 30),
								InvalidationRequestDateAndTime = new DateTime(2022, 02, 07, 15, 09, 59),
								InvalidationInitiatedByCustoms = "1",
								InvalidationJustification = "Invalidation Justification"
							}
						});
				}
				return fProvider;
			}
		}
		CC509CProvider fProvider;
	}
}
