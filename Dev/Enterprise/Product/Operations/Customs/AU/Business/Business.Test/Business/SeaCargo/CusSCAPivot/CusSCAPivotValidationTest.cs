using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestGoodsDescription()
		{
			pivot.CV_GoodsDescription = "";
			AssertEquals("Should not be empty", true, pivot.CV_GoodsDescriptionInfo.HasMessageErrors());
			pivot.CV_GoodsDescription = "Description of the goods";
			AssertNoMessageErrors("Goods description is set, no message errors expected", pivot.CV_GoodsDescriptionInfo);
			houseBill.CA_HouseBill = HouseBillNumber;
			container.CN_ContainerNumber = ContainerNumber;
			pivot.CV_GoodsDescription = "";
			AssertHasMessageError(pivot.CV_GoodsDescriptionInfo, "Goods description required on house bill: " + HouseBillNumber + " in container: " + ContainerNumber);
		}

		public void TestPackagetTypeListValidation()
		{
			pivot.CV_PackageType = "PKG";
			AssertEquals("Container PackageType should have no Messages Errors but have Errors", true, pivot.CV_PackageTypeInfo.HasMessageErrors());
			houseBill.CA_HouseBill = HouseBillNumber;
			container.CN_ContainerNumber = ContainerNumber;
			pivot.CV_PackageType = "";
			AssertHasMessageError(pivot.CV_PackageTypeInfo, "Package type is required on house bill: " + HouseBillNumber + " in container: " + ContainerNumber);
		}

		public void TestWeightAmountValidation()
		{
			pivot.Validation.ValidateCV_Weight();
			Assert("Pre-Condition, Container Weight has MessageErrors as Weight is Mandatory", pivot.CV_WeightInfo.HasMessageErrors());
			pivot.CV_Weight = 2.012m;
			Assert("Container Weight should have no Messages Errors", !pivot.CV_WeightInfo.HasMessageErrors());

			houseBill.CA_HouseBill = HouseBillNumber;
			container.CN_ContainerNumber = ContainerNumber;
			pivot.CV_Weight = 0.0m;
			AssertHasMessageError(pivot.CV_WeightInfo, "Gross weight is required on house bill: " + HouseBillNumber + " in container: " + ContainerNumber);
		}

		public void TestWeightUQValidation()
		{
			pivot.CV_WeightUQ = "";
			pivot.Validation.ValidateCV_WeightUQ();
			Assert("Pre-Condition, Container Weight Unit Quantity has Message Errors as Weight Unit Quantity is Mandatory", pivot.CV_WeightUQInfo.HasMessageErrors() && !pivot.CV_WeightUQInfo.HasErrors());
			pivot.CV_WeightUQ = Core.Constants.Weight.Kilograms;
			Assert("Container Weight Unit Quantity has no Message Errors", !pivot.CV_WeightUQInfo.HasMessageErrors() && !pivot.CV_WeightUQInfo.HasErrors());
			pivot.CV_WeightUQ = Core.Constants.Weight.ShortTons;
			Assert("Container Weight Unit Quantity has no Message Errors", !pivot.CV_WeightUQInfo.HasMessageErrors() && !pivot.CV_WeightUQInfo.HasErrors());
			pivot.CV_WeightUQ = "ZZ";
			Assert("Container Weight Unit Quantity has Errors", pivot.CV_WeightUQInfo.HasMessageErrors());

			houseBill.CA_HouseBill = HouseBillNumber;
			container.CN_ContainerNumber = ContainerNumber;
			pivot.CV_WeightUQ = "";
			AssertHasMessageError(pivot.CV_WeightUQInfo, "Weight unit of quantity is required on house bill: " + HouseBillNumber + " in container: " + ContainerNumber);
		}

		public void TestVolumeValidation()
		{
			pivot.Validation.ValidateCV_Volume();
			Assert("Pre-Condition, Container Volume has Message Errors as Volume is Mandatory", pivot.CV_VolumeInfo.HasMessageErrors());
			pivot.CV_Volume = 23.4m;
			Assert("Container Volume has no Errors", !pivot.CV_VolumeInfo.HasMessageErrors());

			houseBill.CA_HouseBill = HouseBillNumber;
			container.CN_ContainerNumber = ContainerNumber;
			pivot.CV_Volume = 0.0m;
			AssertHasMessageError(pivot.CV_VolumeInfo, "Volume is required on house bill: " + HouseBillNumber + " in container: " + ContainerNumber);
		}

		[StressTest]
		public void TestCheckCV_AssociatedContainer_Performance()
		{
			var oceanBillPk = ZGuid.NewZGuid();
			var sql = $@"
DECLARE @cnt INTEGER = 0 
DECLARE @oceanBillPK UNIQUEIDENTIFIER = {oceanBillPk.ToSqlGuid()}
DECLARE @containerPK UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.CusSCAOceanBill(CB_PK, CB_ApplicationCode, CB_SystemCreateTimeUtc, CB_SystemCreateUser, CB_SystemLastEditTimeUtc, CB_SystemLastEditUser) VALUES (@oceanBillPK, 'CMR', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.CusSCAContainer(CN_PK, CN_CB, CN_ContainerMode, CN_SystemCreateTimeUtc, CN_SystemCreateUser, CN_SystemLastEditTimeUtc, CN_SystemLastEditUser) VALUES (@containerPK, @oceanBillPK, 'FCL', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
WHILE @cnt < 5000
	BEGIN
		DECLARE @houseBillPK UNIQUEIDENTIFIER = NEWID();
		INSERT INTO dbo.CusSCAHouse(CA_PK, CA_CB, CA_HouseBill, CA_SystemCreateTimeUtc, CA_SystemCreateUser, CA_SystemLastEditTimeUtc, CA_SystemLastEditUser) VALUES (@houseBillPK, @oceanBillPK, CONCAT('HB', @cnt), GetUtcDate(), '~BP', GetUtcDate(), '~BP')
		INSERT INTO dbo.CusSCAPivot(CV_PK, CV_CB, CV_CN, CV_CA, CV_SystemCreateTimeUtc, CV_SystemCreateUser, CV_SystemLastEditTimeUtc, CV_SystemLastEditUser) VALUES (NEWID(), @oceanBillPK, @containerPK, @houseBillPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
		SET @cnt = @cnt + 1
	END
";
			Db.Connection.ExecuteNonQuery(sql);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var oceanBill = newFactory.Load<CusSCAOceanBill>(oceanBillPk);
			var stopWatch = new Stopwatch();
			// This method is called according to the consideration that ZForm (eventually) and MultiMessageManager call LoadChildEditableObjects().
			oceanBill.LoadChildEditableObjects();
			stopWatch.Start();
			// Would have called oceanBill.RunPreSaveValidation() here, but we simulate this so that we don't time other validations from this UT.
			foreach (var pivot in oceanBill.Pivots)
			{
				pivot.Validation.ValidateCV_AssociatedContainer();
			}
			AssertLessThan(stopWatch.ElapsedMilliseconds, 20_000); // 114798ms -> 10300ms
		}

		public void TestValidateCV_AssociatedContainer()
		{
			houseBill.CA_HouseBill = HouseBillNumber;
			houseBill.CA_ConsigneeName = "ABC";
			container.CN_ContainerNumber = ContainerNumber;
			pivot.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
			CusSCAHouse houseBill2 = oceanBill.HouseBills.AddNew();
			CusSCAContainer container2 = oceanBill.Containers.AddNew();
			CusSCAPivot pivot2 = houseBill2.Pivot.AddNew();
			houseBill2.CA_ConsigneeName = "DEF";
			container2.CN_ContainerNumber = "CONT1234565";
			pivot2.CV_AssociatedContainer = container2.CN_ContainerNumber;
			AssertNoMessageError(pivot2.CV_AssociatedContainerInfo, CusSCAPivotValidation.InvalidContainerFCXOrLCL);
			AssertNoMessageError(pivot2.CV_AssociatedContainerInfo, CusSCAPivotValidation.InvalidContainerLCL);

			houseBill2.CA_ConsigneeName = "ABC";
			pivot2.CV_AssociatedContainer = ContainerNumber;
			AssertHasMessageError(pivot2.CV_AssociatedContainerInfo, CusSCAPivotValidation.InvalidContainerFCXOrLCL);
			AssertNoMessageError(pivot2.CV_AssociatedContainerInfo, CusSCAPivotValidation.InvalidContainerLCL);

			pivot.CN_ContainerMode = Core.Constants.ContainerModes.FCLMixedShipper;
			AssertNoMessageError(pivot2.CV_AssociatedContainerInfo, CusSCAPivotValidation.InvalidContainerFCXOrLCL);
			AssertNoMessageError(pivot2.CV_AssociatedContainerInfo, CusSCAPivotValidation.InvalidContainerLCL);

			houseBill2.CA_ConsigneeName = "DEF";
			pivot.CN_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertNoMessageError(pivot2.CV_AssociatedContainerInfo, CusSCAPivotValidation.InvalidContainerFCXOrLCL);
			AssertNoMessageError(pivot2.CV_AssociatedContainerInfo, CusSCAPivotValidation.InvalidContainerLCL);

			pivot.CN_ContainerMode = Core.Constants.ContainerModes.FCLMixedShipper;
			AssertNoMessageError(pivot2.CV_AssociatedContainerInfo, CusSCAPivotValidation.InvalidContainerFCXOrLCL);
			AssertHasMessageError(pivot2.CV_AssociatedContainerInfo, CusSCAPivotValidation.InvalidContainerLCL);
		}

		public void TestValidateCV_AssociatedContainerWithNoHouseBill()
		{
			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = "Maingano";
			CusSCAPivot pivot = container.Pivots.AddNew();
			pivot.CV_AssociatedContainer = container.CN_ContainerNumber;
			AssertEquals(container.CN_ContainerNumber, pivot.CV_AssociatedContainer);
		}

		public void TestValidationIsCleared()
		{
			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = "OLCU1231231";
			CusSCAPivot pivot = container.Pivots.AddNew();

			pivot.Validation.ValidateCN_SealNumber();
			AssertEquals(true, pivot.CN_SealNumberInfo.HasWarnings());
			pivot.CN_SealNumber = "1231231";
			pivot.Validation.ValidateCN_SealNumber();
			AssertEquals(false, pivot.CN_SealNumberInfo.HasWarnings());
		}

		public void TestGoodsDescriptionValidation()
		{
			pivot.Validation.ValidateCV_GoodsDescription();
			Assert("Pre-Condition, Container Goods Description has MessageErrors as Goods Description is Empty", pivot.CV_GoodsDescriptionInfo.HasMessageErrors());
			pivot.CV_GoodsDescription = "SOME GOODS DESCRIPTION FOR ME";
			Assert("Container Goods Description should have no Messages Errors", !pivot.CV_GoodsDescriptionInfo.HasMessageErrors());
			pivot.CV_GoodsDescription = "SOME GOODS DESCRIPTION FOR ME THAT CONTAIN INVALID CHARACTERS #$%^&*)((";
			Assert("Container Goods Description should have Messages Errors as there is invalid characters", !pivot.CV_GoodsDescriptionInfo.HasMessageErrors());
			pivot.CV_GoodsDescription = "SOME GOODS DESCRIPTION FOR ME THAT CONTAIN INVALID CHARACTERS #$%^&*)((";
			Assert("Container Goods Description should have Messages Errors as there is invalid characters", !pivot.CV_GoodsDescriptionInfo.HasMessageErrors());
		}

		public void TestCheckCV_Volumn()
		{
			var ocean = Factory.New<CusSCAOceanBill>();
			var house = ocean.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();

			var msg = string.Format(CusSCAPivotValidation.ValueCannotLessThan001, "Volume");

			pivot.CV_Volume = 0.001;
			AssertHasMessageError(pivot.CV_VolumeInfo, msg);
			pivot.CV_Volume = 0.01;
			AssertNoMessageError(pivot.CV_VolumeInfo, msg);
		}

		public void TestCheckCV_Weight()
		{
			var ocean = Factory.New<CusSCAOceanBill>();
			var house = ocean.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();

			var msg = string.Format(CusSCAPivotValidation.ValueCannotLessThan001, "Gross Weight");

			pivot.CV_Weight = 0.001;
			AssertHasMessageError(pivot.CV_WeightInfo, msg);
			pivot.CV_Weight = 0.01;
			AssertNoMessageError(pivot.CV_WeightInfo, msg);
		}

		public void TestCheckCV_NetWeight()
		{
			var ocean = Factory.New<CusSCAOceanBill>();
			var house = ocean.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();

			var msg = string.Format(CusSCAPivotValidation.ValueCannotLessThan001, "Net Weight");

			pivot.CV_NetWeight = 0.001;
			AssertHasMessageError(pivot.CV_NetWeightInfo, msg);
			pivot.CV_NetWeight = 0.01;
			AssertNoMessageError(pivot.CV_NetWeightInfo, msg);
		}

		public void TestYouCannotHaveTwoPivotsOnTheSameHouseContainerPair()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			CusSCAContainer container = oceanBill.Containers.AddNew();

			var pivot1 = house.Pivot.AddNew();
			pivot1.CV_CN = container.PK;

			pivot1.Validation.ValidateAll();

			AssertNoError(pivot1.CV_AssociatedContainerInfo, "Only one Packing Line is allowed to be linked per each House Bill and Container. Please remove any extra Packing Lines.");

			var pivot2 = house.Pivot.AddNew();
			pivot2.CV_CN = container.PK;

			pivot2.Validation.ValidateAll();

			AssertHasError(pivot2.CV_AssociatedContainerInfo, "Only one Packing Line is allowed to be linked per each House Bill and Container. Please remove any extra Packing Lines.");
		}

		public void TestSACValidation()
		{
			houseBill.CA_IsMasterHouse = true;
			pivot.CV_IsSAC = true;
			AssertEquals("Master house cannot be declared as SAC", true, pivot.CV_IsSACInfo.HasMessageErrors());

			pivot.CV_IsSAC = false;
			AssertEquals("Master house cannot be declared as SAC", false, pivot.CV_IsSACInfo.HasMessageErrors());
		}

		public void TestGoodsDescriptionGreaterThanTwoCharacters()
		{
			pivot.CV_GoodsDescription = "";
			AssertEquals("Should not be empty", true, pivot.CV_GoodsDescriptionInfo.HasMessageErrors());

			pivot.CV_GoodsDescription = "1234%";
			AssertEquals("Should be at least two chars", true, pivot.CV_GoodsDescriptionInfo.HasMessageErrors());

			pivot.CV_GoodsDescription = "1234%v";
			AssertEquals("Should be at least two chars", true, pivot.CV_GoodsDescriptionInfo.HasMessageErrors());

			pivot.CV_GoodsDescription = "1234%vv";
			AssertEquals("Should be at least two chars", false, pivot.CV_GoodsDescriptionInfo.HasMessageErrors());
		}

		public void TestPackageTypeValidation()
		{
			pivot.CV_PackageType = "";
			AssertEquals("Package type required", true, pivot.CV_PackageTypeInfo.HasMessageErrors());

			//Pivot.CV_AssociatedContainer = CusSCAPivot.Bulk;
			container.CN_ContainerMode = CMRImportCargoTypes.Codes.Bulk;
			pivot.CV_PackageType = "";
			AssertEquals("Package type not required for Bulk", false, pivot.CV_PackageTypeInfo.HasMessageErrors());
		}

		public void TestPackageCountValidation()
		{
			pivot.CV_PackageCount = 0;
			AssertEquals("Package count required", true, pivot.CV_PackageCountInfo.HasMessageErrors());

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.Bulk;
			pivot.CV_PackageCount = 0;
			AssertEquals("Package count not required for Bulk", false, pivot.CV_PackageCountInfo.HasMessageErrors());
		}

		public void TestMarksAndNumberRequired()
		{
			pivot.CV_MarksAndNumbers = "";
			AssertEquals("Not required", false, pivot.CV_MarksAndNumbersInfo.HasNotifications());

			container.CN_ContainerMode = Core.Constants.ContainerModes.LCL;
			pivot.Validation.ValidateCV_MarksAndNumbers();
			AssertEquals("Required for LCL", true, pivot.CV_MarksAndNumbersInfo.HasMessageErrors());

			container.CN_ContainerMode = CMRCargoTypes.Codes.BreakBulk;
			container.CN_ContainerNumber = CusSCAPivot.BreakBulk;
			pivot.CV_MarksAndNumbers = "";
			AssertEquals("Required for B/B", true, pivot.CV_MarksAndNumbersInfo.HasMessageErrors());

			pivot.CV_MarksAndNumbers = "Marks and numbers";
			AssertEquals("Required for B/B", false, pivot.CV_MarksAndNumbersInfo.HasMessageErrors());

			container.CN_ContainerMode = Core.Constants.ContainerModes.LCL;
			container.CN_ContainerNumber = ContainerNumber;
			houseBill.CA_HouseBill = HouseBillNumber;
			pivot.CV_MarksAndNumbers = "";
			AssertHasMessageError(pivot.CV_MarksAndNumbersInfo, "Marks and numbers required on house bill: " + HouseBillNumber + " in container: " + ContainerNumber);
		}

		public void TestMasterHouseValidatesSAC()
		{
			pivot.CV_IsSAC = true;
			AssertNoMessageErrors("when master house false, sac true", pivot.CV_IsSACInfo);
			houseBill.CA_IsMasterHouse = true;
			AssertHasMessageErrors("when master house changed to true", pivot.CV_IsSACInfo);
			houseBill.CA_IsMasterHouse = false;
			AssertNoMessageErrors("when master house changed back to false", pivot.CV_IsSACInfo);
		}

		public void TestValidateBulkPackageCount()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			CusSCAPivot pivot = house.Pivot.AddNew();
			pivot.CV_AssociatedContainer = CusSCAPivot.Bulk;
			pivot.CV_PackageCount = 10;
			AssertHasMessageErrors("Package count should not be included on Bulk Entries", pivot.CV_PackageCountInfo);
		}

		#region Implementation

		const string HouseBillNumber = "HB123PIVOT442";
		const string ContainerNumber = "TRCU3392204";

		CusSCAOceanBill oceanBill;
		CusSCAHouse houseBill;
		CusSCAPivot pivot;
		CusSCAContainer container;

		protected override void SetUp()
		{
			base.SetUp();

			oceanBill = Factory.New<CusSCAOceanBill>();
			houseBill = oceanBill.HouseBills.AddNew();
			container = oceanBill.Containers.AddNew();
			pivot = houseBill.Pivot.AddNew();
			pivot.CV_CN = container.PK;
		}

		#endregion
	}
}
