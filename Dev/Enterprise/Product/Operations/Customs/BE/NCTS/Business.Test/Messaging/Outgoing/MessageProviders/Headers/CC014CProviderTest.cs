using System;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC014CProvider))]
	sealed class CC014CProviderTest : NctsHeaderProviderAbstractTest<CC014CProvider>
	{
		[TestDate(2025, 05, 08, 12, 39, 20, 555)]
		public void TestInvalidationRequestDateAndTime()
		{
			AssertEquals(new DateTime(2025, 05, 08, 12, 39, 20, 0, DateTimeKind.Unspecified), Provider.InvalidationRequestDateAndTime);
		}

		[TestDate(2025, 05, 08, 12, 39, 20, 555)]
		public void TestInvalidationDecisionDateAndTime_NCTS()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection
			{
				new MessageVersionRegistry { DomainCode = Constants.MessageVersionRegistryDomainCodes.NCTSP5, TargetSystemName = "NCTS.BE" }
			};
			using (BECustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertEquals(new DateTime(2025, 05, 08, 12, 39, 20, 0, DateTimeKind.Unspecified), Provider.InvalidationRequestDateAndTime);
			}
		}

		public void TestInvalidationDecisionDateAndTime_NTA()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection
			{
				new MessageVersionRegistry { DomainCode = Constants.MessageVersionRegistryDomainCodes.NCTSP5, TargetSystemName = "NTA" }
			};
			using (BECustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertEquals(DateTime.MinValue, Provider.InvalidationDecisionDateAndTime);
			}
		}

		public void TestInvalidationDecision_NCTS()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection
			{
				new MessageVersionRegistry { DomainCode = Constants.MessageVersionRegistryDomainCodes.NCTSP5, TargetSystemName = "NCTS.BE" }
			};
			using (BECustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertEquals(true, Provider.InvalidationDecision);
			}
		}

		public void TestInvalidationDecision_NTA()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection
			{
				new MessageVersionRegistry { DomainCode = Constants.MessageVersionRegistryDomainCodes.NCTSP5, TargetSystemName = "NTA" }
			};
			using (BECustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertEquals(false, Provider.InvalidationDecision);
			}
		}

		public void TestInvalidationInitiatedByCustoms()
		{
			AssertEquals(false, Provider.InvalidationInitiatedByCustoms);
		}

		public void TestInvalidationJustification()
		{
			action.Justification = "K=V*InvalidationJustification=InvalidationJustificationText*O=V";
			AssertEquals("K=V*InvalidationJustification=InvalidationJustificationText*O=V", Provider.InvalidationJustification);
		}

		public void TestHolderOfTheTransitProcedure()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("Not Null", Provider.HolderOfTheTransitProcedure);
				AssertType<HolderOfTheTransitProcedureProvider>("Type", Provider.HolderOfTheTransitProcedure);
			});
		}

		public void TestLRN()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ArrivalMrnFromUser = "MRNOfTheNCTS";
				nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRNOfTheNCT";
				AssertNull("When MRN is filled, LRN must be null", Provider.LRN);
				nctsHeader.ArrivalMrnFromUser = ZString.Empty;
				AssertEquals("When MRN is empty, LRN must be filled", "LRNOfTheNCT", Provider.LRN);
			});
		}

		protected override bool HasSendingActionParameter => true;

		protected override string MessageType => Constants.MessageTypes.CC014C;

		protected override string MovementType => NctsMovementType.Codes.Departure;
	}
}
