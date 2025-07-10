using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	class BaseChargeLookupsTest : JobChargeLookupsTest
	{
		public void TestSupplyTypes()
		{
			var testCharge = Factory.New<Charge>();
			AssertEquals("Default value", "LOC, LOX, LOA, INT, INX, INA, DSB", testCharge.Lookups.SupplyTypes.CodesAsString);

			var values = new CodeDescriptionBoolDisallowNewCollection(AccountingMasterFilesConstants.SupplyTypeClassificationList);
			values.Cast<CodeDescriptionBool>().ForEach(x => x.Bool = true);
			values[0].Bool = false;
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, values))
			{
				AssertEquals("LOC was removed from list", "LOX, LOA, INT, INX, INA, DSB", testCharge.Lookups.SupplyTypes.CodesAsString);
			}

			values.Cast<CodeDescriptionBool>().ForEach(x => x.Bool = false);
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, values))
			{
				AssertNullOrEmpty("All removed", testCharge.Lookups.SupplyTypes.CodesAsString);
			}
		}

		public void TestActiveChargeCodes()
		{
			var newChargeCode = Factory.New<AccChargeCode>();
			newChargeCode.AC_IsActive = false;
			newChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			newChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;

			var testCharge = Factory.New<Charge>();
			var chargeCodes = new AccChargeCodeCollection(Factory, testCharge.Lookups.ChargeCodes.CompleteFilter);
			chargeCodes.Load();
			Assert(!chargeCodes.Contains(newChargeCode));

			newChargeCode.AC_IsActive = true;

			var testCharge2 = Factory.New<Charge>();
			var chargeCodes2 = new AccChargeCodeCollection(Factory, testCharge2.Lookups.ChargeCodes.CompleteFilter);
			chargeCodes2.Load();
			Assert(chargeCodes2.Contains(newChargeCode));
		}

		public void TestRelatedJobNumber()
		{
			var creator = new TestObjectCreator(Factory);
			var gatewayConsol = creator.CreateGatewayConsol("AUSYD", "SGSIN", "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);

			using (var consolJobHeader = creator.CreateJob(gatewayConsol))
			{
				var shipment1 = creator.CreateShipment("S0001", "AUSYD", "USLAX", gatewayConsol);
				var shipment2 = creator.CreateShipment("S0002", "AUBNE", "USNYC", gatewayConsol);
				var shipment3 = creator.CreateShipment("S0004", "AUMEL", "USLAX");
				Factory.Save();
				var shipment4 = creator.CreateShipment("S0003", "AUSYD", "SGSIN", gatewayConsol);

				var charge = creator.CreateCharge(consolJobHeader, creator.CreateChargeCode("AAA"), 100, 100);

				foreach (var shipment in new ForwardingShipment[] { shipment1, shipment2, shipment3, shipment4 })
				{
					AssertEquals("Pre-condition: all shipments are saved except for shipment4", shipment != shipment4, shipment.IsInDatabase);
				}

				var expected = new[]
				{
					("S0001", "AUSYD - USLAX"),
					("S0002", "AUBNE - USNYC")
				};

				var actual = charge.Lookups.RelatedJobNumbers.Cast<ICodeDescription>().Select(x => (x.Code, x.Description)).ToArray();
				AssertArrayEqualsByElements(expected, actual);

				Factory.Save();
				expected = expected.Append(("S0003", "AUSYD - SGSIN")).ToArray();

				actual = charge.Lookups.RelatedJobNumbers.Cast<ICodeDescription>().Select(x => (x.Code, x.Description)).ToArray();
				AssertArrayEqualsByElements(expected, actual);
			}
		}

		public virtual void TestChargeCodesChangesDepartmentbyFilterList()
		{
			var testShip = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "TestJob";
			testJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			testJob.JH_ParentID = testShip.PK;

			var myTestCharge = testJob.Charges.AddNew();
			myTestCharge.JR_JH = testJob.PK;
			myTestCharge.JR_GB = GlbBranch.CurrentBranch.PK;

			AccChargeCode chargeCodeAll = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeAll.AC_Code = "CCODE1";
			chargeCodeAll.AC_DepartmentFilterList = "ALL";

			myTestCharge.JR_AC = chargeCodeAll.PK;
			AssertEquals("Charge Code should be set to default", myTestCharge.JR_GE, testJob.Department.PK);

			AccChargeCode chargeCodeMany = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeMany.AC_Code = "CCODE2";
			chargeCodeMany.AC_DepartmentFilterList = "FEA, FIA, FES, FIS, CEA, CIA, CES, CIS";

			myTestCharge.JR_AC = chargeCodeMany.PK;
			AssertEquals("Charge Code should be set to default", myTestCharge.JR_GE, testJob.Department.PK);

			AccChargeCode chargeCodeOne = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeOne.AC_Code = "CCODE3";
			chargeCodeOne.AC_DepartmentFilterList = "CIS";

			GlbDepartment cISDept = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CIS"));

			myTestCharge.JR_AC = chargeCodeOne.PK;
			AssertEquals("Charge Code should be set to that GE_code equal to CIS", myTestCharge.JR_GE, cISDept.PK);
		}

		public void TestRatingBehaviourLookups()
		{
			var newChargeCode = Factory.New<AccChargeCode>();
			newChargeCode.AC_IsActive = false;
			newChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			newChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;

			var testCharge = Factory.New<Charge>();

			Assert(testCharge.Lookups.RatingBehaviors_Cost.ContainsCode(JobChargeLookups.CreateNewCharge));
			Assert(testCharge.Lookups.RatingBehaviors_Cost.ContainsCode(JobChargeLookups.ReAutorateCharge));

			Assert(testCharge.Lookups.RatingBehaviors_Sell.ContainsCode(JobChargeLookups.CreateNewCharge));
			Assert(testCharge.Lookups.RatingBehaviors_Sell.ContainsCode(JobChargeLookups.ReAutorateCharge));

			var tr = Factory.NewWithValidTestData<AccTaxRate>();
			var thCost = Factory.NewWithValidTestData<AccTransactionHeader>();
			var tlCost = Factory.NewWithValidTestData<AccTransactionLines>();
			tlCost.AL_AH = thCost.PK;
			tlCost.AL_LineType = TransactionLineTypes.Cost;
			tlCost.AL_LineAmount = -200;
			tlCost.AL_OSAmount = -200;
			tlCost.AL_RX_NKTransactionCurrency = "AUD";
			tlCost.AL_RevRecognitionType = "IMM";

			testCharge.JR_AL_APLine = tlCost.PK;
			testCharge.JR_AT_SellGSTRate = tr.PK;

			Assert(testCharge.JR_IsCostPosted);
			AssertEquals(2, testCharge.Lookups.RatingBehaviors_Cost.Count);
			Assert(testCharge.Lookups.RatingBehaviors_Cost.ContainsCode(JobChargeLookups.CreateNewCharge));
			Assert(testCharge.Lookups.RatingBehaviors_Cost.ContainsCode(JobChargeLookups.StopFromAutorating));

			var thSell = Factory.NewWithValidTestData<AccTransactionHeader>();
			var tlSell = Factory.NewWithValidTestData<AccTransactionLines>();
			tlSell.AL_AH = thSell.PK;
			tlSell.AL_LineType = TransactionLineTypes.Revenue;
			tlSell.AL_LineAmount = 200;
			tlSell.AL_OSAmount = 200;
			tlSell.AL_RX_NKTransactionCurrency = "AUD";
			tlSell.AL_RevRecognitionType = "IMM";

			testCharge.JR_AL_ARLine = tlSell.PK;

			Assert(testCharge.JR_IsRevenuePosted);
			AssertEquals(2, testCharge.Lookups.RatingBehaviors_Sell.Count);
			Assert(testCharge.Lookups.RatingBehaviors_Sell.ContainsCode(JobChargeLookups.CreateNewCharge));
			Assert(testCharge.Lookups.RatingBehaviors_Sell.ContainsCode(JobChargeLookups.StopFromAutorating));

			var testCharge2 = Factory.New<Charge>();
			var fRTConsolCost = Factory.New<JobConsolCost>();

			testCharge2.JR_E6 = fRTConsolCost.PK;
			testCharge2.JR_AT_SellGSTRate = tr.PK;
			Assert(testCharge2.Lookups.RatingBehaviors_Cost.ContainsCode(JobChargeLookups.CreateNewCharge));
			Assert(testCharge2.Lookups.RatingBehaviors_Cost.ContainsCode(JobChargeLookups.StopFromAutorating));
		}
	}
}
