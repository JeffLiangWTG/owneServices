using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsPackageCollection<NctsPackage, NctsCommonCargoDesc>))]
	class NctsPackageCollectionTests : BusinessObjectCollectionTestCase
	{
		public void TestAddCloneFrom()
		{
			var collection = GetCollection(isPhase5: false, NctsMovementType.Codes.Arrival);
			var package = collection.AddNew();
			package.B5_MarksAndNumbers = "AAA";

			var goodsItem2 = Factory.New<NctsArrivalCargoDesc>();
			var collection2 = new NctsPackageCollection<NctsPackage, NctsCommonCargoDesc>(goodsItem2);
			CombineAssertions(() =>
			{
				AssertEquals("Count is 0", 0, collection2.Count);

				AssertNoExceptionThrown("BY_ParentID is not ready, no exception when calling CusInBondCargoDescTypeDecider.GetTypeForLoad()", () => collection2.AddCloneFrom(collection, new BusinessObjectCloneArgs()));
				AssertEquals("Count is 1", 1, collection2.Count);
			});
		}

		public void TestSetDefaultsForNewChild()
		{
			var collection = (NctsPackageCollection<NctsPackage, NctsCommonCargoDesc>)Collection;
			var package1 = collection.AddNew();
			package1.Parent.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var package2 = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertNotEquals("B5_TypeOfDifference not phase5", NctsUnloadedStateList.Codes.NEW, package1.B5_TypeOfDifference);
				AssertEquals("B5_TypeOfDifference phase5", NctsUnloadedStateList.Codes.NEW, package2.B5_TypeOfDifference);
			});
		}

		public void TestSingleContainerNotSelecedForPhase4Arrival()
		{
			var packages = GetCollection(isPhase5: false, NctsMovementType.Codes.Arrival);
			var nctsHeader = packages.Master.Header;
			_ = nctsHeader.ArrivalHeaderContainers.AddNew();
			var package1 = packages.AddNew();
			AssertEquals("Not linked to single container", 0, package1.ContainersPivot.Count);
		}

		public void TestSingleContainerNotSelecedForPhase4Departure()
		{
			var packages = GetCollection(isPhase5: false, NctsMovementType.Codes.Departure);
			var nctsHeader = packages.Master.Header;
			_ = nctsHeader.DepartureHeaderContainers.AddNew();
			var package1 = packages.AddNew();
			AssertEquals("Not linked to single container", 0, package1.ContainersPivot.Count);
		}

		public void TestSingleContainerNotSelecedForPhase5Arrival()
		{
			var packages = GetCollection(isPhase5: true, NctsMovementType.Codes.Arrival);
			var nctsHeader = packages.Master.Header;
			_ = nctsHeader.ArrivalHeaderContainers.AddNew();
			var package1 = packages.AddNew();
			AssertEquals("Linked to single container", 0, package1.ContainersPivot.Count);
		}

		public void TestSingleContainerSelecedForPhase5Departure()
		{
			var packages = GetCollection(isPhase5: true, NctsMovementType.Codes.Departure);
			var nctsHeader = packages.Master.Header;
			_ = nctsHeader.DepartureHeaderContainers.AddNew();
			var package1 = packages.AddNew();
			AssertEquals("Linked to single container", 1, package1.ContainersPivot.Count);
			_ = nctsHeader.DepartureHeaderContainers.AddNew();
			var package2 = packages.AddNew();
			AssertEquals("Not linked to single container", 0, package2.ContainersPivot.Count);
		}

		public void TestRelationShipFilter()
		{
			var collection = GetCollection(isPhase5: false, NctsMovementType.Codes.Arrival);
			var package = collection.AddNew();
			var packageWithParent = Factory.New<NctsPackage>();
			packageWithParent.B5_B5_ParentPackage = package.PK;
			packageWithParent.B5_ParentID = package.B5_ParentID;
			packageWithParent.B5_ParentTableCode = package.B5_ParentTableCode;

			CombineAssertions(() =>
			{
				AssertEquals("There should be 1 row in the collection", 1, collection.Count);
				AssertEquals("The row in the collection should match the package without parent package", package.PK, collection[0].PK);
			});
		}

		public void TestMaxCountValidation_TR0026()
		{
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0026Active));

					var packages = GetPackageCollectionForMovementType(NctsMovementType.Codes.Departure);
					var maxCountValidator = ((ISupportMaxCountValidation)packages).MaxCountValidator;
					var notification = maxCountValidator.Notification;

					AssertEquals("MaxCount for Departure movement", 99, maxCountValidator.MaxCount);
					AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
					AssertEquals("Notification Message", "[TR0026] The maximum number of 99 Package Lines has been exceeded.", notification.Message);
					AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);

					packages = GetPackageCollectionForMovementType(NctsMovementType.Codes.Arrival);
					maxCountValidator = ((ISupportMaxCountValidation)packages).MaxCountValidator;

					AssertEquals("MaxCount for Arrival movement", -1, maxCountValidator.MaxCount);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0026Active));

					packages = GetPackageCollectionForMovementType(NctsMovementType.Codes.Departure);
					maxCountValidator = ((ISupportMaxCountValidation)packages).MaxCountValidator;

					AssertEquals("MaxCount when RuleTR0026 is disabled", maxCountValidator.MaxCount, -1);
				}
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => GetCollection(isPhase5: false, NctsMovementType.Codes.Arrival);

		NctsPackageCollection<NctsPackage, NctsCommonCargoDesc> GetCollection(bool isPhase5, string movementType)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = isPhase5 ? Common.CusInBondApplicationCodeList.Codes.NCTS5 : Common.CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(movementType);
			NctsCommonCargoDesc goodsItem;
			if (isPhase5)
			{
				if (movementType == NctsMovementType.Codes.Arrival)
				{
					goodsItem = header.Bills.AddNew().ArrivalGoodsItems.AddNew();
				}
				else
				{
					goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
				}
			}
			else
			{
				if (movementType == NctsMovementType.Codes.Arrival)
				{
					goodsItem = header.ArrivalMovementHeader.GoodsItems.AddNew();
				}
				else
				{
					goodsItem = header.MovementHeader.GoodsItems.AddNew();
				}
			}
			return new NctsPackageCollection<NctsPackage, NctsCommonCargoDesc>(goodsItem);
		}

		INctsPackageCollection<NctsPackage, NctsCommonCargoDesc> GetPackageCollectionForMovementType(string movementType)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(movementType);

			var bill = header.Bills.AddNew();
			switch (movementType)
			{
				case NctsMovementType.Codes.Arrival:
					return bill.ArrivalGoodsItems.AddNew().Packages;
				case NctsMovementType.Codes.Departure:
				default:
					return bill.GoodsItems.AddNew().Packages;
			}
		}
	}
}
