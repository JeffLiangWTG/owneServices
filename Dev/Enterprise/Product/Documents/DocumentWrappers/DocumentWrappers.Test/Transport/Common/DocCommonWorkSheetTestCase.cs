using System;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCommonWorkSheet))]
	sealed class DocCommonWorkSheetTestCase : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocCommonWorkSheet.New(WorkSheet, Factory) };
		}

		public void TestContainerLegs()
		{
			DocCommonWorkSheet sheet = DocCommonWorkSheet.New(WorkSheet, Factory);
			AssertEquals(3, sheet.CartageLegs.Count);
		}

		public void TestContainerLegDriverName()
		{
			DocCommonWorkSheet sheet = DocCommonWorkSheet.New(WorkSheet, Factory);
			AssertEquals("wrong driver name", "Speedy Gonzales", sheet.ContainerLegDriverName);

			WorkSheet.EY_DriversName = "Michael Schmock";
			sheet = DocCommonWorkSheet.New(WorkSheet, Factory);
			AssertEquals("wrong rego", "Speedy Gonzales", sheet.ContainerLegDriverName);

			WorkSheet.EY_GS_NKTruckDriver = String.Empty;
			sheet = DocCommonWorkSheet.New(WorkSheet, Factory);
			AssertEquals("Should no longer have the drivers name", "", sheet.ContainerLegDriverName);
		}

		public void TestContainerLegVehicleRego()
		{
			DocCommonWorkSheet sheet = DocCommonWorkSheet.New(WorkSheet, Factory);
			AssertEquals("wrong rego", "Xprd4YrsAg", sheet.ContainerLegVehicleRego);

			WorkSheet.EY_TruckRegistration = "ASS LII";
			sheet = DocCommonWorkSheet.New(WorkSheet, Factory);
			AssertEquals("wrong rego", "Xprd4YrsAg", sheet.ContainerLegVehicleRego);

			WorkSheet.EY_RQ_Truck = ZGuid.Empty;
			sheet = DocCommonWorkSheet.New(WorkSheet, Factory);
			AssertEquals("wrong rego", "", sheet.ContainerLegVehicleRego);
		}

		public void TestWorkSheetDate()
		{
			DocCommonWorkSheet sheet = DocCommonWorkSheet.New(WorkSheet, Factory);
			AssertEquals("incorrect WorkSheet date", StartTime.ToShortDateString() + " / " + EndTime.ToShortDateString(), sheet.WorkSheetDate);
		}

		public void TestWorksheetLegsPrintOrder()
		{
			WorkSheet.CartageLegs[0].JU_PlannedPickupTime = ZDateTime.Now.AddDays(-3);
			WorkSheet.CartageLegs[1].JU_PlannedPickupTime = ZDateTime.Now.AddDays(-1);
			WorkSheet.CartageLegs[2].JU_PlannedPickupTime = ZDateTime.Now.AddDays(-2);

			WorkSheet.CartageLegs[0].JU_RunSheetSequence = 3;
			WorkSheet.CartageLegs[1].JU_RunSheetSequence = 1;
			WorkSheet.CartageLegs[2].JU_RunSheetSequence = 2;

			Factory.Save();

			DocCommonWorkSheet sheet = DocCommonWorkSheet.New(WorkSheet, Factory);

			AssertEquals("legs should be sorted by sequence", WorkSheet.CartageLegs[1], sheet.CartageLegs[0].WrappedObject);
			AssertEquals("legs should be sorted by sequence", WorkSheet.CartageLegs[2], sheet.CartageLegs[1].WrappedObject);
			AssertEquals("legs should be sorted by sequence", WorkSheet.CartageLegs[0], sheet.CartageLegs[2].WrappedObject);
		}

		public void TestRunSheetNumber()
		{
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			AssertEquals("", runSheet.EY_RunSheetNumber);

			runSheet.EY_RunSheetNumber = "Hi";
			AssertEquals("Hi", runSheet.EY_RunSheetNumber);
		}

		#region Implementation

		ZDateTime StartTime
		{
			get
			{
				fStartTime = new ZDateTime(2007, 4, 2, 9, 0, 0);
				return fStartTime;
			}
		}

		ZDateTime EndTime
		{
			get
			{
				fEndTime = new ZDateTime(2008, 8, 1, 5, 30, 0);
				return fEndTime;
			}
		}

		ZDateTime fStartTime;
		ZDateTime fEndTime;

		protected override void SetUp()
		{
			WorkSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			WorkSheet.EY_StartTime = StartTime;
			WorkSheet.EY_EndTime = EndTime;

			RefEquipment truck = Factory.NewWithValidTestData<RefEquipment>();
			truck.RQ_Registration = "Xprd4YrsAg";
			WorkSheet.EY_RQ_Truck = truck.PK;

			GlbStaff driver = Factory.NewWithValidTestData<GlbStaff>();
			driver.GS_FullName = "Speedy Gonzales";
			WorkSheet.EY_GS_NKTruckDriver = driver.GS_Code;

			CommonCartageLeg leg1 = Factory.New<CommonCartageLeg>();
			CommonCartageLeg leg2 = Factory.New<CommonCartageLeg>();
			CommonCartageLeg leg3 = Factory.New<CommonCartageLeg>();

			WorkSheet.CartageLegs.Add(leg1);
			WorkSheet.CartageLegs.Add(leg2);
			WorkSheet.CartageLegs.Add(leg3);

			base.SetUp();
		}

		CommonWorkSheet WorkSheet;

		#endregion
	}
}
