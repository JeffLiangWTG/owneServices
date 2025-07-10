using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderCharges))]
	class CusEntryHeaderChargeTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		#region TestShouldResetDataOnMergingCore
		public void TestShouldResetDataOnMergingCore()
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISServicePaymentAmount;
			AssertEquals("AQISServicePaymentAmount", false, charge.ShouldResetDataOnMerging);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.OtherCharges;
			AssertEquals("OtherCharges", false, charge.ShouldResetDataOnMerging);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.TotalPayableAdmin;
			AssertEquals("TotalPayableAdmin", false, charge.ShouldResetDataOnMerging);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.Woodlevy;
			AssertEquals("Woodlevy", true, charge.ShouldResetDataOnMerging);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISContainerCharges;
			AssertEquals("AQISContainerCharges", true, charge.ShouldResetDataOnMerging);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISProcessingCharge;
			AssertEquals("AQISProcessingCharge", true, charge.ShouldResetDataOnMerging);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.DeclarationProcessingCharge;
			AssertEquals("DeclarationProcessingCharge", true, charge.ShouldResetDataOnMerging);

			charge.EntryHeader.EntryNumber = "TEST001";

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISServicePaymentAmount;
			AssertEquals("AQISServicePaymentAmount", false, charge.ShouldResetDataOnMerging);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.OtherCharges;
			AssertEquals("OtherCharges", false, charge.ShouldResetDataOnMerging);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.TotalPayableAdmin;
			AssertEquals("TotalPayableAdmin", false, charge.ShouldResetDataOnMerging);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.Woodlevy;
			AssertEquals("Woodlevy", true, charge.ShouldResetDataOnMerging);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISContainerCharges;
			AssertEquals("AQISContainerCharges", false, charge.ShouldResetDataOnMerging);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISProcessingCharge;
			AssertEquals("AQISProcessingCharge", false, charge.ShouldResetDataOnMerging);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.DeclarationProcessingCharge;
			AssertEquals("DeclarationProcessingCharge", false, charge.ShouldResetDataOnMerging);
		}

		#endregion

		#region TestIsPayableToCustoms
		public void TestIsPayableToCustoms()
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();
			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.TotalPayableAdmin;
			AssertEquals("IsPayableToCustomsForHeader", true, charge.IsPayableToCustomsForHeader);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.DeclarationProcessingCharge;
			AssertEquals("IsPayableToCustomsForHeader", true, charge.IsPayableToCustomsForHeader);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISProcessingCharge;
			AssertEquals("IsPayableToCustomsForHeader", true, charge.IsPayableToCustomsForHeader);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISContainerCharges;
			AssertEquals("IsPayableToCustomsForHeader", true, charge.IsPayableToCustomsForHeader);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.Woodlevy;
			AssertEquals("IsPayableToCustomsForHeader", true, charge.IsPayableToCustomsForHeader);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.OtherCharges;
			AssertEquals("IsPayableToCustomsForHeader", true, charge.IsPayableToCustomsForHeader);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.DutyAmount;//This is line level
			AssertEquals("IsPayableToCustomsForHeader", false, charge.IsPayableToCustomsForHeader);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISServicePaymentAmount;
			AssertEquals("IsPayableToCustomsForHeader", true, charge.IsPayableToCustomsForHeader);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.EntryFee;
			AssertEquals("IsPayableToCustomsForHeader", true, charge.IsPayableToCustomsForHeader);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.MessageFee;
			AssertEquals("IsPayableToCustomsForHeader", true, charge.IsPayableToCustomsForHeader);

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.TradegateGST;
			AssertEquals("IsPayableToCustomsForHeader", true, charge.IsPayableToCustomsForHeader);
		}
		#endregion

		public void TestIsChargeTypeDeferrable()
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISProcessingCharge;
			AssertEquals("AQISProcessingCharge IsDeferrable", true, charge.IsDeferrable);
			AssertEquals("AQISProcessingCharge", true, CusEntryHeaderCharges.IsChargeTypeDeferrable(CusEntryChargeTypeList.Codes.AQISProcessingCharge));

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.DeclarationProcessingCharge;
			AssertEquals("DeclarationProcessingCharge IsDeferrable", true, charge.IsDeferrable);
			AssertEquals("DeclarationProcessingCharge", true, CusEntryHeaderCharges.IsChargeTypeDeferrable(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge));

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.Woodlevy;
			AssertEquals("Woodlevy IsDeferrable", true, charge.IsDeferrable);
			AssertEquals("Woodlevy", true, CusEntryHeaderCharges.IsChargeTypeDeferrable(CusEntryChargeTypeList.Codes.Woodlevy));

			charge.C1_ChargeType = CusEntryChargeTypeList.Codes.OtherCharges;
			AssertEquals("OtherCharges IsDeferrable", false, charge.IsDeferrable);
			AssertEquals("OtherCharges", false, CusEntryHeaderCharges.IsChargeTypeDeferrable(CusEntryChargeTypeList.Codes.OtherCharges));
		}

		#region Test EntryHeader
		public void TestEntryHeader()
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();
			AssertEquals("Entry Header is AU type", typeof(CusEntryHeader), charge.EntryHeader.GetType());
		}
		#endregion

		public void TestCheckASPChargeCreated()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var charge1 = entry.Charges.AddNew("ASP");
			var charge2 = entry.Charges.AddNew("B00");

			CombineAssertions(() =>
			{
				var charge3 = entry.Charges.AddNew();
				charge3.C1_ChargeType = "ASP";
				AssertEquals("Will not show error when running the test", ZString.Empty, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return entry.Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var charge = entry.Charges.AddNew();
			charge.C1_ChargeAmount = 10m;
			return charge;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { CusEntryHeaderCharges.Schema.C1_Source };
		}
	}
}
