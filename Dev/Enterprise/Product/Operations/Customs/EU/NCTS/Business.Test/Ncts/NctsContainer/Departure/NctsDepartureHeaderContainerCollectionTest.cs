using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader>))]
	class NctsDepartureHeaderContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() =>
			GetContainerCollectionForMovementType().containers;

		protected override System.Type GetExpectedCollectionType()
		{
			return typeof(NctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader>);
		}

		public void TestSetDefaultsForNewChildPhase5()
		{
			CombineAssertions(() =>
			{
				var containers = GetContainerCollectionForMovementType().containers;
				var first = containers.AddNew();
				AssertEquals("First element doesn't have BC_Mode set", ZString.Empty, first.BC_Mode);
				AssertEquals("First element BC_SequenceNumber", (ZShort)1, first.BC_SequenceNumber);
				first.BC_Mode = Constants.ContainerModes.Containerised;

				var second = containers.AddNew();
				AssertEquals("Second element has BC_Mode the same as the first element", first.BC_Mode, second.BC_Mode);
				AssertEquals("First element BC_SequenceNumber after adding second element", (ZShort)1, first.BC_SequenceNumber);
				AssertEquals("Second element BC_SequenceNumber after adding second element", (ZShort)2, second.BC_SequenceNumber);

				var third = containers.AddNew();
				AssertEquals("Third element has BC_Mode the same as the first element", first.BC_Mode, third.BC_Mode);
				AssertEquals("First element BC_SequenceNumber after adding third element", (ZShort)1, first.BC_SequenceNumber);
				AssertEquals("Second element BC_SequenceNumber after adding third element", (ZShort)2, second.BC_SequenceNumber);
				AssertEquals("Third element BC_SequenceNumber after adding third element", (ZShort)3, third.BC_SequenceNumber);
				second.Delete();
				AssertEquals("First element BC_SequenceNumber after deleting second element", (ZShort)1, first.BC_SequenceNumber);
				AssertEquals("Third element BC_SequenceNumber after deleting second element", (ZShort)2, third.BC_SequenceNumber);

				var fourth = containers.AddNew();
				AssertEquals("First element BC_SequenceNumber after adding fourth element", (ZShort)1, first.BC_SequenceNumber);
				AssertEquals("Third element BC_SequenceNumber after adding fourth element", (ZShort)2, third.BC_SequenceNumber);
				AssertEquals("Fourth element BC_SequenceNumber after adding fourth element", (ZShort)3, fourth.BC_SequenceNumber);
			});
		}

		public void TestLoad_Arrival()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var containers = header.ArrivalHeaderContainers;
			var container = containers.AddNew();
			container.BC_ContainerNum = "MSCU1234566";
			Factory.Save();
			var reloadedHeader = new BusinessObjectFactory().Load<NctsHeader>(header.PK);
			var containersD = reloadedHeader.DepartureHeaderContainers;
			containersD.Load();
			AssertEquals("Number of containers should be 0", 0, containersD.Count);
		}

		public void TestAllowNew()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var containers = header.DepartureHeaderContainers;

			AssertEquals(true, containers.AllowNew);

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			containers = header.DepartureHeaderContainers;

			AssertEquals(false, containers.AllowNew);

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			containers = header.DepartureHeaderContainers;

			AssertEquals(true, containers.AllowNew);
		}

		public void TestMaxCountValidation_TR0024()
		{
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0024Active));

					var containers = GetContainerCollectionForMovementType(NctsMovementType.Codes.Departure).containers;
					var maxCountValidator = ((ISupportMaxCountValidation)containers).MaxCountValidator;
					var notification = maxCountValidator.Notification;

					AssertEquals("MaxCount for Departure movement", 9999, maxCountValidator.MaxCount);
					AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
					AssertEquals("Notification Message", "[TR0024] The maximum number of 9999 Containers/Equipments has been exceeded.", notification.Message);
					AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);

					containers = GetContainerCollectionForMovementType(NctsMovementType.Codes.Arrival).containers;
					maxCountValidator = ((ISupportMaxCountValidation)containers).MaxCountValidator;

					AssertEquals("MaxCount for Arrival movement", -1, maxCountValidator.MaxCount);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0024Active));

					containers = GetContainerCollectionForMovementType(NctsMovementType.Codes.Departure).containers;
					maxCountValidator = ((ISupportMaxCountValidation)containers).MaxCountValidator;

					AssertEquals("MaxCount when RuleTR0024 is disabled", maxCountValidator.MaxCount, -1);
				}
			});
		}

		(NctsHeader header, NctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader> containers) GetContainerCollectionForMovementType(string movementType = NctsMovementType.Codes.Departure)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(movementType);
			return (header, (NctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader>)header.DepartureHeaderContainers);
		}
	}
}
