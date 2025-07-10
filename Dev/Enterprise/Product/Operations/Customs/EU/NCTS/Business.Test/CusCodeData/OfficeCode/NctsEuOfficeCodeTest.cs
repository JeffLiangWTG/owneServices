using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsEuOfficeCode))]
	sealed class NctsEuOfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<NctsEuOfficeCode>
	{
		public void TestICustomsOfficeMembers()
		{
			var nctsEuOfficeCode = header.MovementHeader.CustomsOffices.AddNew();
			var customsOffice = nctsEuOfficeCode as ICustomsOffice;
			AssertNotNull($"{nameof(NctsEuOfficeCode)} as {nameof(ICustomsOffice)}", customsOffice);

			var now = ZDateTime.Now;

			nctsEuOfficeCode.CY_Data = "EU123456";
			nctsEuOfficeCode.CY_Date = now;
			CombineAssertions("", () =>
			{
				AssertEquals(nameof(ICustomsOffice.OfficeCode), "EU123456", customsOffice.OfficeCode);
				AssertEquals(nameof(ICustomsOffice.ArrivalTime), now, customsOffice.ArrivalTime);
			});
		}

		public void TestIsOfficeOfTransit()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty CY_Code", false, nctsEuOfficeCode.IsOfficeOfTransit);

				nctsEuOfficeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
				AssertEquals("CY_Code is NCTSOfficeOfTransit", true, nctsEuOfficeCode.IsOfficeOfTransit);

				nctsEuOfficeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
				AssertEquals("CY_Code is not NCTSOfficeOfTransit", false, nctsEuOfficeCode.IsOfficeOfTransit);
			});
		}

		public void TestIsOfficeCountryConsideredInEuForSafetyAndSecurity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDate.Today.AddMonths(-1);
			var endDate = ZDate.Today.AddMonths(1);

			var tradeGroup = helper.LoadOrCreateTradeGroup("EUN", "EUC", startDate, endDate);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.France, startDate, endDate);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedKingdom, startDate, endDate);

			var tradeGroupCUAM = helper.CreateTradeGroup("EUN", "EUCTP", startDate, endDate);
			helper.AddCountry(tradeGroupCUAM, Core.Constants.CountryCodes.UnitedKingdom, startDate, endDate);

			var tradeGroupEUSEC = helper.CreateTradeGroup("EUN", "EUSEC", startDate, endDate);
			helper.AddCountry(tradeGroupEUSEC, Core.Constants.CountryCodes.Austria, startDate, endDate);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Empty CY_Data, IsOfficeCountryConsideredInEuForSafetyAndSecurity", false, nctsEuOfficeCode.IsOfficeCountryConsideredInEuForSafetyAndSecurity);

				nctsEuOfficeCode.CY_Data = "AT00110";
				AssertEquals("IsOfficeCountryConsideredInEuForSafetyAndSecurity", true, nctsEuOfficeCode.IsOfficeCountryConsideredInEuForSafetyAndSecurity);

				nctsEuOfficeCode.CY_Data = "DE00370";
				AssertEquals("IsOfficeCountryConsideredInEuForSafetyAndSecurity", false, nctsEuOfficeCode.IsOfficeCountryConsideredInEuForSafetyAndSecurity);
			});
		}

		public void TestSequenceNumber_DifferentSubTypes()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var movementHeader = header.MovementHeader;
			movementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();

			CombineAssertions(() =>
			{
				var customsOffice = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				AssertEquals("SequenceNumber of customsOffice (DEP) is 1", 1, customsOffice.CY_Order.ToZInt());

				var customsOffice2 = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice2.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
				AssertEquals("SequenceNumber of customsOffice2 (DES) is 1", 1, customsOffice2.CY_Order.ToZInt());

				var customsOffice3 = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice3.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
				AssertEquals("SequenceNumber of customsOffice3 (DSA) is 1", 1, customsOffice3.CY_Order.ToZInt());

				var customsOffice4 = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice4.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry;
				AssertEquals("SequenceNumber of customsOffice4 (ENQ) is 1", 1, customsOffice4.CY_Order.ToZInt());

				var customsOffice5 = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice5.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
				AssertEquals("SequenceNumber of customsOffice5 (TXT) is 1", 1, customsOffice5.CY_Order.ToZInt());

				var customsOffice6 = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice6.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfLocation;
				AssertEquals("SequenceNumber of customsOffice6 (LOC) is 1", 1, customsOffice6.CY_Order.ToZInt());

				var customsOffice7 = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice7.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
				AssertEquals("SequenceNumber of customsOffice7 (TRA) is 1", 1, customsOffice7.CY_Order.ToZInt());

				var customsOffice8 = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice8.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				AssertEquals("SequenceNumber of customsOffice8 (DEP) is 2", 2, customsOffice8.CY_Order.ToZInt());

				var customsOffice9 = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice9.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
				AssertEquals("SequenceNumber of customsOffice9 (DES) is 2", 2, customsOffice9.CY_Order.ToZInt());

				var customsOffice10 = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice10.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
				AssertEquals("SequenceNumber of customsOffice10 (DSA) is 2", 2, customsOffice10.CY_Order.ToZInt());

				var customsOffice11 = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice11.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry;
				AssertEquals("SequenceNumber of customsOffice11 (ENQ) is 2", 2, customsOffice11.CY_Order.ToZInt());

				var customsOffice12 = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice12.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
				AssertEquals("SequenceNumber of customsOffice12 (TXT) is 2", 2, customsOffice12.CY_Order.ToZInt());

				var customsOffice13 = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice13.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfLocation;
				AssertEquals("SequenceNumber of customsOffice13 (LOC) is 2", 2, customsOffice13.CY_Order.ToZInt());

				var customsOffice14 = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice14.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
				AssertEquals("SequenceNumber of customsOffice14 (TRA) is 2", 2, customsOffice14.CY_Order.ToZInt());

				customsOffice.Delete();

				AssertEquals("customsOffice (DEP) is deleted, SequenceNumber of customsOffice2 (DES) is 1", 1, customsOffice2.CY_Order.ToZInt());
				AssertEquals("customsOffice (DEP) is deleted, SequenceNumber of customsOffice3 (DSA) is 1", 1, customsOffice3.CY_Order.ToZInt());
				AssertEquals("customsOffice (DEP) is deleted, SequenceNumber of customsOffice4 (ENQ) is 1", 1, customsOffice4.CY_Order.ToZInt());
				AssertEquals("customsOffice (DEP) is deleted, SequenceNumber of customsOffice5 (TXT) is 1", 1, customsOffice5.CY_Order.ToZInt());
				AssertEquals("customsOffice (DEP) is deleted, SequenceNumber of customsOffice6 (LOC) is 1", 1, customsOffice6.CY_Order.ToZInt());
				AssertEquals("customsOffice (DEP) is deleted, SequenceNumber of customsOffice7 (TRA) is 1", 1, customsOffice7.CY_Order.ToZInt());
				AssertEquals("customsOffice (DEP) is deleted, SequenceNumber of customsOffice8 (DEP) is 1", 1, customsOffice8.CY_Order.ToZInt());
				AssertEquals("customsOffice (DEP) is deleted, SequenceNumber of customsOffice9 (DES) is 2", 2, customsOffice9.CY_Order.ToZInt());
				AssertEquals("customsOffice (DEP) is deleted, SequenceNumber of customsOffice10 (DSA) is 2", 2, customsOffice10.CY_Order.ToZInt());
				AssertEquals("customsOffice (DEP) is deleted, SequenceNumber of customsOffice11 (ENQ) is 2", 2, customsOffice11.CY_Order.ToZInt());
				AssertEquals("customsOffice (DEP) is deleted, SequenceNumber of customsOffice12 (TXT) is 2", 2, customsOffice12.CY_Order.ToZInt());
				AssertEquals("customsOffice (DEP) is deleted, SequenceNumber of customsOffice13 (LOC) is 2", 2, customsOffice13.CY_Order.ToZInt());
				AssertEquals("customsOffice (DEP) is deleted, SequenceNumber of customsOffice14 (TRA) is 2", 2, customsOffice14.CY_Order.ToZInt());
			});
		}

		public void TestAutomaticSequenceNumberEnabled()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var movementHeader = header.MovementHeader;
			movementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();

			CombineAssertions(() =>
			{
				var customsOffice = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				AssertEquals("First Line", 1, customsOffice.CY_Order.ToZInt());

				var secondLine = movementHeader.CustomsOfficesForDeparture.AddNew();
				secondLine.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				AssertEquals("Second Line", 2, secondLine.CY_Order.ToZInt());

				var thirdLine = movementHeader.CustomsOfficesForDeparture.AddNew();
				thirdLine.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				AssertEquals("Third Line", 3, thirdLine.CY_Order.ToZInt());

				secondLine.Delete();
				AssertEquals("First Line same as second deleted", 1, customsOffice.CY_Order.ToZInt());
				AssertEquals("Third Line renumbered as second deleted", 2, thirdLine.CY_Order.ToZInt());

				var newThirdLine = movementHeader.CustomsOfficesForDeparture.AddNew();
				newThirdLine.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				AssertEquals("New Third added", 3, newThirdLine.CY_Order.ToZInt());
			});
		}

		public void TestAutomaticSequenceNumberDisabled()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			using (NctsConfigurationTestHelper.TemporarilySetOfficeCodeAutomaticSequenceNumberEnabled(Factory, false))
			{
				CombineAssertions(() =>
				{
					var firstLine = Factory.New<NctsEuOfficeCodeForTest>();
					firstLine.AttachToParent(header.MovementHeader);
					AssertEquals("First Line", ZShort.Zero, firstLine.CY_Order);

					firstLine.CY_Order = 20;
					AssertEquals("First Line new sequence number", 20, firstLine.CY_Order.ToZInt());

					var secondLine = Factory.New<NctsEuOfficeCodeForTest>();
					secondLine.AttachToParent(header.MovementHeader);
					AssertEquals("Second Line", ZShort.Zero, secondLine.CY_Order);

					secondLine.CY_Order = 35;
					AssertEquals("Second Line new sequence number", 35, secondLine.CY_Order.ToZInt());
					firstLine.Delete();
					AssertEquals("Second Line remains the same as the first line is deleted", 35, (int)secondLine.CY_Order);
				});
			}
		}

		public void TestAutomaticSequenceNumberDisabled_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

			using (NctsConfigurationTestHelper.TemporarilySetOfficeCodeAutomaticSequenceNumberEnabled(Factory, false))
			{
				CombineAssertions(() =>
				{
					var firstLine = Factory.New<NctsEuOfficeCodeForTest>();
					firstLine.AttachToParent(header.MovementHeader);
					AssertEquals("First Line", ZShort.Zero, firstLine.CY_Order);

					firstLine.CY_Order = 20;
					AssertEquals("First Line new sequence number", 20, firstLine.CY_Order.ToZInt());

					var secondLine = Factory.New<NctsEuOfficeCodeForTest>();
					secondLine.AttachToParent(header);
					AssertEquals("Second Line", ZShort.Zero, secondLine.CY_Order);

					secondLine.CY_Order = 35;
					AssertEquals("Second Line new sequence number", 35, secondLine.CY_Order.ToZInt());
					firstLine.Delete();
					AssertEquals("Second Line remains the same as the first line is deleted", 35, (int)secondLine.CY_Order);
				});
			}
		}

		public void TestCY_OrderCaption()
		{
			var customsOffice = (NctsEuOfficeCode)GetNewBusinessObject();
			NCTSTestHelper.AssertCaptions(customsOffice.CY_OrderInfo, "Sequence Number", string.Empty, "Seq. No.");
		}

		public void TestCY_Order_ReadOnly()
		{
			var customsOffice = (NctsEuOfficeCode)GetNewBusinessObject();
			AssertEquals("The sequence should always be disabled", true, customsOffice.CY_OrderInfo.ReadOnly);
		}

		public void TestValidation_Departure()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsEuOfficeCode = Factory.New<NctsEuOfficeCodeForTest>();
			nctsEuOfficeCode.AttachToParent(header);

			CombineAssertions(() =>
			{
				AssertType("NCTS4", typeof(NctsEuOfficeCodeValidation), nctsEuOfficeCode.Validation);

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

				nctsEuOfficeCode.AttachToParent(header.MovementHeader);
				AssertType("NCTS5", typeof(NctsEuOfficeCodePhase5DepartureValidation), nctsEuOfficeCode.Validation);
			});
		}

		public void TestValidation_Arrival()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var nctsEuOfficeCode = Factory.New<NctsEuOfficeCodeForTest>();
			nctsEuOfficeCode.AttachToParent(header);

			CombineAssertions(() =>
			{
				AssertType("NCTS4", typeof(NctsEuOfficeCodeValidation), nctsEuOfficeCode.Validation);

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

				AssertType("NCTS5", typeof(NctsEuOfficeCodePhase5Validation), nctsEuOfficeCode.Validation);
			});
		}

		public void TestCY_ParentIdMarksAsNeedingValidationOnlyOnChange()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsMovementHeader = nctsHeader.MovementHeader;
			var item = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var officeCode = nctsMovementHeader.CustomsOffices.AddNew();
			item.MarkLightValidationAsValidForTesting();
			officeCode.CY_ParentID = nctsMovementHeader.PK;
			AssertEquals("CY_ParentID is not changed", true, item.LightValidationIsValid);

			officeCode.CY_ParentID = ZGuid.Empty;
			officeCode.CY_ParentID = nctsMovementHeader.PK;

			AssertEquals("CY_ParentID is changed", false, item.LightValidationIsValid);
		}

		protected override IEnumerable<NctsEuOfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var customsOffice = (NctsEuOfficeCode)GetNewBusinessObjectForDeleteTest(factory);
			customsOffice.CY_Data = "D";
			yield return customsOffice;
		}

		public void TestParent()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var nctsMovementHeader = nctsHeader.MovementHeader;
				var officeCode = nctsMovementHeader.CustomsOffices.AddNew();
				AssertType<NctsDepartureMovementHeader>("Office Code Parent is NctsDepartureMovementHeader", officeCode.Parent);

				nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				officeCode = nctsHeader.CustomsOffices.AddNew();
				AssertType<NctsHeader>("Office Code Parent is NctsHeader", officeCode.Parent);
			});
		}
		public void TestGetHeaderEquivalentPropertyInfo()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

				var nctsMovementHeader = nctsHeader.MovementHeader;
				nctsMovementHeader.CustomsOffices.RemoveAndDeleteAll();

				var officeCode = nctsMovementHeader.CustomsOffices.AddNew();
				officeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry;
				AssertEquals("EnquiryCustomsOfficeCode is empty", ZString.Empty, nctsMovementHeader.EnquiryCustomsOfficeCode);
				officeCode.CY_Data = "DE12345";
				AssertEquals("EnquiryCustomsOfficeCode is updated with value from office code", "DE12345", nctsMovementHeader.EnquiryCustomsOfficeCode);

				officeCode.CY_Data = ZString.Empty;
				officeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				AssertEquals("NCTSOfficeOfDeparture is empty", ZString.Empty, nctsMovementHeader.DepartureCustomsOfficeCode);
				officeCode.CY_Data = "IE12345";
				AssertEquals("NCTSOfficeOfDeparture is updated with value from office code", "IE12345", nctsMovementHeader.DepartureCustomsOfficeCode);

				nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
				officeCode = arrivalMovementHeader.CustomsOffices.AddNew();
				officeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
				AssertEquals("NCTSOfficeOfDestinationForArrival is empty", ZString.Empty, arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival);
				officeCode.CY_Data = "IT12345";
				AssertEquals("NCTSOfficeOfDestinationForArrival is updated with value from office code", "IT12345", arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival);
			});
		}

		public void TestGetHeaderEquivalentPropertyInfo_Phase4()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.CustomsOffices.RemoveAndDeleteAll();

				var officeCode = nctsHeader.CustomsOffices.AddNew();
				officeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry;
				AssertEquals("EnquiryCustomsOfficeCode is empty", ZString.Empty, nctsHeader.EnquiryCustomsOfficeCode);
				officeCode.CY_Data = "DE12345";
				AssertEquals("EnquiryCustomsOfficeCode is updated with value from office code", "DE12345", nctsHeader.EnquiryCustomsOfficeCode);

				officeCode.CY_Data = ZString.Empty;
				officeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				AssertEquals("NCTSOfficeOfDeparture is empty", ZString.Empty, nctsHeader.DepartureCustomsOfficeCode);
				officeCode.CY_Data = "IE12345";
				AssertEquals("NCTSOfficeOfDeparture is updated with value from office code", "IE12345", nctsHeader.DepartureCustomsOfficeCode);
				nctsHeader.CustomsOffices.RemoveAndDeleteAll();

				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				officeCode = nctsHeader.CustomsOffices.AddNew();
				officeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
				AssertEquals("NCTSOfficeOfDestinationForArrival is empty", ZString.Empty, nctsHeader.DestinationCustomsOfficeCodeForArrival);
				officeCode.CY_Data = "IT12345";
				AssertEquals("NCTSOfficeOfDestinationForArrival is updated with value from office code", "IT12345", nctsHeader.DestinationCustomsOfficeCodeForArrival);
			});
		}

		public void TestGetHeaderEquivalentPropertyInfo_ErrorReporter()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();

				var customsOffice = Factory.New<NctsEuOfficeCode>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				customsOffice.Parent = nctsHeader;
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				customsOffice.CY_Data = "DE0004";
				AssertEquals("Incorrect Parent for Phase5", ErrorReporter.LastKeyReported);

				ErrorReporter.Clear();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				customsOffice.CY_Data = "DE0003";
				AssertNullOrEmpty(ErrorReporter.LastKeyReported);

				ErrorReporter.Clear();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				customsOffice.Parent = nctsHeader.MovementHeader;
				customsOffice.CY_Data = "DE0002";
				AssertNullOrEmpty(ErrorReporter.LastKeyReported);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.MovementHeader.CustomsOfficesForDeparture.AddNew();
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new LightValidationTesterNctsEuOfficeCode(bizObjToTest);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			nctsEuOfficeCode = header.MovementHeader.CustomsOffices.AddNew();
		}
		NctsHeader header;
		NctsEuOfficeCode nctsEuOfficeCode;
	}

	public class LightValidationTesterNctsEuOfficeCode : LightValidationTester
	{
		public LightValidationTesterNctsEuOfficeCode(BusinessObject bo) : base(bo)
		{
		}

		protected override bool ShouldTestProperty(ZPropertyInfo info)
		{
			// The validation do have a relationship with JobDocAddress, see NctsEuOfficeCodeValidation.CheckCY_Data, here we do reference header.DestinationTrader/SecurityConsignor/Principal...
			// But it can be overweight if we mark NctsEuOfficeCode as needing validation when we set properties in JobDocAddress,
			// So I suppressed the JobDocAddress here.
			if (info.BizObj.GetType() == typeof(JobDocAddress) || (info.BizObj is NctsEuOfficeCode))
			{
				return false;
			}
			return base.ShouldTestProperty(info);
		}
	}

	sealed class NctsEuOfficeCodeForTest : NctsEuOfficeCode
	{
		public NctsEuOfficeCodeForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void AttachToParent(NctsHeader parent)
		{
			CY_ParentTableCode = parent.TablePrefix;
			CY_ParentID = parent.PK;
		}

		public void AttachToParent(NctsDepartureMovementHeader parent)
		{
			CY_ParentTableCode = parent.TablePrefix;
			CY_ParentID = parent.PK;
		}
	}
}
