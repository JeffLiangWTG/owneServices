using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CusSealCollection))]
	sealed class CusSealCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetCollectionRelationships()
		{
			var collection = (CusSealCollection)Collection;
			var seal1 = Factory.New<CusSeal>();
			collection.Add(seal1);
			var seal2 = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("seal1.BK_ParentID", collection.Master.PK, seal1.BK_ParentID);
				AssertEquals("seal1.BK_ParentTableCode", CusInBondHeaderSchema.Constants.Prefix, seal1.BK_ParentTableCode);
				AssertEquals(typeof(NctsHeader), seal1.ParentType);

				AssertEquals("seal2.BK_ParentID", collection.Master.PK, seal2.BK_ParentID);
				AssertEquals("seal2.BK_ParentTableCode", CusInBondHeaderSchema.Constants.Prefix, seal2.BK_ParentTableCode);
				AssertEquals(typeof(NctsHeader), seal2.ParentType);
			});
		}

		public void TestSetDefaultsForNewChild()
		{
			var collection = (CusSealCollection)Collection;
			var seal1 = collection.AddNew();
			var seal2 = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("seal1.BK_SequenceNumber", (ZShort)1, seal1.BK_SequenceNumber);
				AssertEquals("seal2.BK_SequenceNumber", (ZShort)2, seal2.BK_SequenceNumber);

				seal2.Delete();
				var seal3 = collection.AddNew();
				AssertEquals("seal3.BK_SequenceNumber", (ZShort)2, seal3.BK_SequenceNumber);
			});
		}

		public void TestSetDefaultsForNewChild_NctsDepartureHeaderContainer()
		{
			TestSetDefaultsForNewChild(GetCusSealCollectionForMovementType(NctsMovementType.Codes.Departure), recalculateWhenDeleted: true);
		}

		public void TestSetDefaultsForNewChild_NctsArrivalHeaderContainer()
		{
			TestSetDefaultsForNewChild(GetCusSealCollectionForMovementType(NctsMovementType.Codes.Arrival), recalculateWhenDeleted: true);
		}

		public void TestSetDefaultsForNewChild_NctsHeader()
		{
			TestSetDefaultsForNewChild((CusSealCollection)Collection, recalculateWhenDeleted: false);
		}

		public void TestSetDefaultsForNewChild_EnRouteIncident()
		{
			var enRouteIncident = Factory.New<EnRouteIncident>();
			var collection = new CusSealCollection(enRouteIncident);
			TestSetDefaultsForNewChild(collection, recalculateWhenDeleted: false);
		}

		public void TestSetDefaultsForNewChild_NctsContainer()
		{
			var nctsContainer = Factory.New<NctsContainer>();
			var collection = new CusSealCollection(nctsContainer);
			TestSetDefaultsForNewChild(collection, recalculateWhenDeleted: false);
		}

		void TestSetDefaultsForNewChild(CusSealCollection collection, bool recalculateWhenDeleted)
		{
			var seal1 = collection.AddNew();
			var seal2 = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Sequence of seal1 should be 1", (ZShort)1, seal1.BK_SequenceNumber);
				AssertEquals("Sequence of seal2 should be 2", (ZShort)2, seal2.BK_SequenceNumber);

				collection.Sort(new SortInfo(nameof(CusSeal.BK_SequenceNumber), ListSortDirection.Descending));
				AssertEquals("First collection element should be seal2", seal2.BK_SequenceNumber, collection[0].BK_SequenceNumber);
				AssertEquals("Second collection element should be seal1", seal1.BK_SequenceNumber, collection[1].BK_SequenceNumber);

				var seal3 = collection.AddNew();
				AssertEquals("Sequence of seal3 should be 3", (ZShort)3, seal3.BK_SequenceNumber);

				seal1.Delete();
				if (recalculateWhenDeleted)
				{
					AssertEquals("Sequence of seal2 should recalculate to 1", (ZShort)1, seal2.BK_SequenceNumber);
					AssertEquals("Sequence of seal3 should recalculate to 2", (ZShort)2, seal3.BK_SequenceNumber);
				}

				collection.Sort(new SortInfo(nameof(CusSeal.BK_SequenceNumber), ListSortDirection.Descending));
				AssertEquals("First collection element should be seal3", seal3.BK_SequenceNumber, collection[0].BK_SequenceNumber);
				AssertEquals("Second collection element should be seal2", seal2.BK_SequenceNumber, collection[1].BK_SequenceNumber);

				var seal4 = collection.AddNew();
				if (recalculateWhenDeleted)
				{
					AssertEquals("Sequence of seal4 should be 3", (ZShort)3, seal4.BK_SequenceNumber);
				}
				else
				{
					AssertEquals("Sequence of seal4 should be 4", (ZShort)4, seal4.BK_SequenceNumber);
				}
			});
		}

		public void TestOnRemoved_NctsDepartureHeaderContainer()
		{
			TestOnRemoved(GetCusSealCollectionForMovementType(NctsMovementType.Codes.Departure));
		}

		public void TestOnRemoved_NctsArrivalHeaderContainer()
		{
			TestOnRemoved(GetCusSealCollectionForMovementType(NctsMovementType.Codes.Arrival));
		}

		void TestOnRemoved(CusSealCollection collection)
		{
			var seal1 = collection.AddNew();
			var seal2 = collection.AddNew();
			var seal3 = collection.AddNew();
			var seal4 = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Sequence of seal1 should be 1", (ZShort)1, seal1.BK_SequenceNumber);
				AssertEquals("Sequence of seal2 should be 2", (ZShort)2, seal2.BK_SequenceNumber);
				AssertEquals("Sequence of seal3 should be 3", (ZShort)3, seal3.BK_SequenceNumber);
				AssertEquals("Sequence of seal4 should be 4", (ZShort)4, seal4.BK_SequenceNumber);

				seal2.Delete();
				AssertEquals("Sequence of seal1 should remain as 1", (ZShort)1, seal1.BK_SequenceNumber);
				AssertEquals("Sequence of seal3 should recalculate to 2", (ZShort)2, seal3.BK_SequenceNumber);
				AssertEquals("Sequence of seal4 should recalculate to 3", (ZShort)3, seal4.BK_SequenceNumber);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return new CusSealCollection(header);
		}

		public void TestParentTableForNctsHeaderContainer() => CombineAssertions(() =>
		{
			var container = Factory.New<NctsDepartureHeaderContainer>();
			var collection = new CusSealCollection(container);
			var seal = collection.AddNew();
			AssertEquals("Prefix", CusInBondContainerSchema.Constants.Prefix, seal.BK_ParentTableCode);
			AssertEquals("ParentType", typeof(NctsDepartureHeaderContainer), seal.ParentType);
		});

		public void TestParentTableForEnRouteIncident() => CombineAssertions(() =>
		{
			var enRouteIncident = Factory.New<EnRouteIncident>();
			var collection = new CusSealCollection(enRouteIncident);
			var seal = collection.AddNew();
			AssertEquals("Prefix", CusInBondEventSchema.Constants.Prefix, seal.BK_ParentTableCode);
			AssertEquals("ParentType", typeof(EnRouteIncident), seal.ParentType);
		});

		public void TestParentTableForNctsContainer() => CombineAssertions(() =>
		{
			var nctsContainer = Factory.New<NctsContainer>();
			var collection = new CusSealCollection(nctsContainer);
			var seal = collection.AddNew();
			AssertEquals("Prefix", CusInBondContainerSchema.Constants.Prefix, seal.BK_ParentTableCode);
			AssertEquals("ParentType", typeof(NctsContainer), seal.ParentType);
		});

		public void TestParentTableForNctsArrivalHeaderContainer() => CombineAssertions(() =>
		{
			var container = Factory.New<NctsArrivalHeaderContainer>();
			var collection = new CusSealCollection(container);
			var seal = collection.AddNew();
			AssertEquals("Prefix", CusInBondContainerSchema.Constants.Prefix, seal.BK_ParentTableCode);
			AssertEquals("ParentType", typeof(NctsArrivalHeaderContainer), seal.ParentType);
		});

		public void TestMaxCountValidation_TR0016()
		{
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0016Active));

					var header = Factory.New<NctsHeader>();
					header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					header.SetMovementType(NctsMovementType.Codes.Arrival);

					var incident = header.EnRouteIncidents.AddNew();
					var container = incident.IncidentContainers.AddNew();

					var sealCollection = container.Seals;

					var maxCountValidator = ((ISupportMaxCountValidation)sealCollection).MaxCountValidator;
					var notification = maxCountValidator.Notification;

					AssertEquals("MaxCount", 97, maxCountValidator.MaxCount);
					AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
					AssertEquals("Notification Message", "[TR0016] Max. Number of Seals per Container/Equipment is limited to 99.", notification.Message);
					AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0016Active));

					sealCollection = new CusSealCollection(container);

					maxCountValidator = ((ISupportMaxCountValidation)sealCollection).MaxCountValidator;
					AssertEquals(maxCountValidator.MaxCount, -1);
				}
			});
		}

		public void TestMaxCountValidation_TR0025()
		{
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0025Active));

					var sealCollection = GetCusSealCollectionForMovementType(NctsMovementType.Codes.Departure);
					var maxCountValidator = ((ISupportMaxCountValidation)sealCollection).MaxCountValidator;
					var notification = maxCountValidator.Notification;

					AssertEquals("MaxCount for Departure movement", 97, maxCountValidator.MaxCount);
					AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
					AssertEquals("Notification Message", "[TR0025] The maximum number of 99 Additional Seals has been exceeded.", notification.Message);
					AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);

					sealCollection = GetCusSealCollectionForMovementType(NctsMovementType.Codes.Arrival);
					maxCountValidator = ((ISupportMaxCountValidation)sealCollection).MaxCountValidator;

					AssertEquals("MaxCount for Arrival movement", -1, maxCountValidator.MaxCount);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0025Active));

					sealCollection = GetCusSealCollectionForMovementType(NctsMovementType.Codes.Departure);
					maxCountValidator = ((ISupportMaxCountValidation)sealCollection).MaxCountValidator;

					AssertEquals("MaxCount when RuleTR0025 is disabled", maxCountValidator.MaxCount, -1);
				}
			});
		}

		public void TestCanBeAddedOnlyIfSea1l1AndSeal2Filled()
		{
			var container = Factory.New<NctsDepartureHeaderContainer>();
			var collection = container.AdditionalSeals;
			container.MarkLightValidationAsValidForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Initially container.LightValidationIsValid = True", true, container.LightValidationIsValid);

				collection.AddNew();
				AssertEquals("After adding AdditionalSeals, container.LightValidationIsValid = False", false, container.LightValidationIsValid);
			});
		}

		CusSealCollection GetCusSealCollectionForMovementType(string movementType)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(movementType);
			return movementType == NctsMovementType.Codes.Departure
				? header.DepartureHeaderContainers.AddNew().AdditionalSeals
				: header.ArrivalHeaderContainers.AddNew().Seals;
		}
	}
}
