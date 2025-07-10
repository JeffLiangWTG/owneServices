using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ShipmentPrePaidCharges))]
	sealed class ShipmentPrePaidChargesTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing_MacrosIsEmpty_ReturnFalse()
		{
			AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
		}

		public void TestIsResponsibleForReplacing_MacrosHasBadFormat_ReturnFalse()
		{
			AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<ShipmentPrePaidCharge(<Tbl.Fld>)>", Passes.FirstPass));
			AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("ShipmentPrePaidCharges(<Tbl.Fld>)>", Passes.FirstPass));
		}

		public void TestIsResponsibleForReplacing_ParameterIsMacros_ReturnTrue()
		{
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("< ShipmentPrePaidCharges (  <Fld>   )   >", Passes.FirstPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<ShipmentPrePaidCharges(<Tbl.Fld>)>", Passes.FirstPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<ShipmentPrePaidCharges(<Tbl.Fld>)>", Passes.FirstPass));
		}

		public void TestIsResponsibleForReplacing_ParameterIsMacrosWithInnerMacroses_ReturnTrue()
		{
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<ShipmentPrePaidCharges(<Inner1(<Tbl.Fld>)>)>", Passes.FirstPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<ShipmentPrePaidCharges(<Inner1(<Inner2(<Tbl.Fld>)>)>)>", Passes.FirstPass));
		}

		public void TestIsResponsibleForReplacing_ParameterIsGuid_ReturnTrue()
		{
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<ShipmentPrePaidCharges({F3C3E183-C2D1-4259-AB43-BC56ECDD6772})>", Passes.FirstPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<ShipmentPrePaidCharges(0DE6650A-6F7B-4C2B-B8D2-57EA9EDBF0EE)>", Passes.FirstPass));
		}

		public void TestIsResponsibleForReplacing_ParameterHasBadFormat_ReturnFalse()
		{
			AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<ShipmentPrePaidCharges(McLaren)>", Passes.FirstPass));
			AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<ShipmentPrePaidCharges(<Inner1(<Inner2(<Tbl.Fld>)>))>", Passes.FirstPass));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetReplacement_WithIncoTerm_MultipleAccountChargeCodes_Success()
		{
			var ship = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();

			ship[JobShipmentSchema.JS_INCO.Name] = IncoTerms.FreeCarrier;

			var chr1 = Factory.NewWithValidTestData<AccChargeCode>();
			chr1.AC_GC = GlbCompany.CurrentCompany.PK;
			chr1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chr1.AC_Code = "TEST1";

			var chr2 = Factory.NewWithValidTestData<AccChargeCode>();
			chr2.AC_GC = GlbCompany.CurrentCompany.PK;
			chr2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chr2.AC_Code = "TEST2";

			var chr3 = Factory.NewWithValidTestData<AccChargeCode>();
			chr3.AC_GC = GlbCompany.CurrentCompany.PK;
			chr3.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chr3.AC_Code = "TEST3";

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = ship.PK;

			var jobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge1.JR_JH = job.PK;
			jobCharge1.JR_AC = chr1.PK;

			var jobCharge2 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge2.JR_JH = job.PK;
			jobCharge2.JR_AC = chr2.PK;

			var jobCharge3 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge3.JR_JH = job.PK;
			jobCharge3.JR_AC = chr3.PK;

			Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("AllSections.xls", TestFilesSubFolder.ReportTestFiles);

			var stopwatch = new Mock<IStopwatch>();
			stopwatch.Setup(m => m.ElapsedMilliseconds).Returns(long.MaxValue);

			var macro = $"<ShipmentPrePaidCharges({ship.PK})>";
			using (var pack = new DocumentPack())
			using (var report = new Report(pack, excelTemplate))
			using (ObjectFactory.Substitute(stopwatch.Object))
			{
				object result = "";
				AssertNoExceptionThrown(() => { result = ValueProviderToTest.GetReplacement(macro, report); });
				AssertEquals("TEST1, TEST2, TEST3, ", result);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetReplacement_EmptyParameter_Success()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("AllSections.xls", TestFilesSubFolder.ReportTestFiles);

			var macro = $"<ShipmentPrePaidCharges({Guid.Empty})>";
			using (var pack = new DocumentPack())
			using (var report = new Report(pack, excelTemplate))
			{
				Object result = "";
				AssertNoExceptionThrown(() => { result = ValueProviderToTest.GetReplacement(macro, report); });
				AssertEquals("", result);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetReplacement_WithIncoTerm_SingleAccountChargeCode_Success()
		{
			var ship = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();

			ship[JobShipmentSchema.JS_INCO.Name] = IncoTerms.FreeCarrier;

			var chr1 = Factory.NewWithValidTestData<AccChargeCode>();
			chr1.AC_GC = GlbCompany.CurrentCompany.PK;
			chr1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chr1.AC_Code = "TEST1";

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = ship.PK;

			var jobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge1.JR_JH = job.PK;
			jobCharge1.JR_AC = chr1.PK;

			Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("AllSections.xls", TestFilesSubFolder.ReportTestFiles);

			var stopwatch = new Mock<IStopwatch>();
			stopwatch.Setup(m => m.ElapsedMilliseconds).Returns(long.MaxValue);

			var macro = $"<ShipmentPrePaidCharges({ship.PK})>";
			using (var pack = new DocumentPack())
			using (var report = new Report(pack, excelTemplate))
			using (ObjectFactory.Substitute(stopwatch.Object))
			{
				object result = "";
				AssertNoExceptionThrown(() => { result = ValueProviderToTest.GetReplacement(macro, report); });
				AssertEquals("TEST1, ", result);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetReplacement_BadIncoTerm_SingleAccountChargeCode_Success()
		{
			var ship = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();

			ship[JobShipmentSchema.JS_INCO.Name] = "BAD";

			var chr1 = Factory.NewWithValidTestData<AccChargeCode>();
			chr1.AC_GC = GlbCompany.CurrentCompany.PK;
			chr1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chr1.AC_Code = "TEST1";

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = ship.PK;

			var jobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge1.JR_JH = job.PK;
			jobCharge1.JR_AC = chr1.PK;

			Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("AllSections.xls", TestFilesSubFolder.ReportTestFiles);

			var stopwatch = new Mock<IStopwatch>();
			stopwatch.Setup(m => m.ElapsedMilliseconds).Returns(long.MaxValue);

			var macro = $"<ShipmentPrePaidCharges({ship.PK})>";
			using (var pack = new DocumentPack())
			using (var report = new Report(pack, excelTemplate))
			using (ObjectFactory.Substitute(stopwatch.Object))
			{
				object result = "";
				AssertNoExceptionThrown(() => { result = ValueProviderToTest.GetReplacement(macro, report); });
				AssertEquals("", result);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestDocumentation()
		{
			base.TestDocumentation();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var ship = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			ship[JobShipmentSchema.JS_INCO.Name] = IncoTerms.FreeCarrier;

			var chr1 = Factory.NewWithValidTestData<AccChargeCode>();
			chr1.AC_GC = GlbCompany.CurrentCompany.PK;
			chr1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chr1.AC_Code = "CRT";

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = ship.PK;

			var jobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge1.JR_JH = job.PK;
			jobCharge1.JR_AC = chr1.PK;
			Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("AllSections.xls", TestFilesSubFolder.ReportTestFiles);
			Report = new Report(Pack, excelTemplate);
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("JS_PK", ship.PK));
		}

		protected override ValueProvider GetNewValueProvider() => new ShipmentPrePaidCharges();

		protected override List<FieldInfo> FieldCollection
		{
			get
			{
				return new List<FieldInfo>()
				{
					typeof(ShipmentPrePaidCharges).GetField("factory", BindingFlags.Instance | BindingFlags.NonPublic),
					typeof(ShipmentPrePaidCharges).GetField("fNumberOfRunsWithCurrentFactory", BindingFlags.Instance | BindingFlags.NonPublic),
					typeof(ShipmentPrePaidCharges).GetField("MaxNumberOfRunsWithSameFactory", BindingFlags.Instance | BindingFlags.NonPublic),
				};
			}
		}
	}
}
