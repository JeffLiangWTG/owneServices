using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	class JobComInvoiceGroupHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceGroupHeaderTest
	{
		public void TestDeclaration()
		{
			AssertType<JobDeclaration>(header.JobDeclaration);
		}

		public void TestJobComInvoiceHeaders()
		{
			AssertType<InvoiceHeaderActiveCollection>(header.JobComInvoiceHeaders);
		}

		public void TestGroupCharges()
		{
			AssertType<EU.Business.Declaration.GroupInvoiceChargeCollection<GroupInvoiceCharge>>(header.Charges);
		}

		public void TestGetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var groupHeader = Factory.New<JobComInvoiceGroupHeaderForTest>();
				groupHeader.JZ_JE = declaration.PK;
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "DEIMP", groupHeader.GetStandaloneIncoTermAndChargeFactoryCountryContext());

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "DE", groupHeader.GetStandaloneIncoTermAndChargeFactoryCountryContext());
			});
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Customs.Business.ICommonInvoice commonInvoice = dec.TopGroupInvoice;

			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));

			var customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.RemoveCode(Common.CustomsChargeTypeList.Codes.OverseasFreight);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.AdditionCharge);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.Commission);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.DeductionCharge);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.ExWorks);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.ForeignInlandFreight);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.LandingCharges);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.OverseasInsurance);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.OtherCharges);
			customsChargeTypeList.RemoveCode(CustomsChargeTypeList.Codes.PackingCost);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._001, ImportChargeCodeList.Descriptions._001);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._002, ImportChargeCodeList.Descriptions._002);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._003, ImportChargeCodeList.Descriptions._003);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._004, ImportChargeCodeList.Descriptions._004);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._005, ImportChargeCodeList.Descriptions._005);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._006, ImportChargeCodeList.Descriptions._006);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._007, ImportChargeCodeList.Descriptions._007);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._008, ImportChargeCodeList.Descriptions._008);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._009, ImportChargeCodeList.Descriptions._009);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._010, ImportChargeCodeList.Descriptions._010);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._011, ImportChargeCodeList.Descriptions._011);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._012, ImportChargeCodeList.Descriptions._012);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._015, ImportChargeCodeList.Descriptions._015);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._016, ImportChargeCodeList.Descriptions._016);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._017, ImportChargeCodeList.Descriptions._017);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._019, ImportChargeCodeList.Descriptions._019);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes.AIR, ImportChargeCodeList.Descriptions.AIR);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes.TCE, ImportChargeCodeList.Descriptions.TCE);
			customsChargeTypeList.AddPair(ImportChargeCodeList.Codes._014, ImportChargeCodeList.Descriptions._014);
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			var exportChargeTypeList = new ChargeCodeList();
			exportChargeTypeList.Sort();
			AssertEquals(exportChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public override void TestChargesToImportForLandedCosting()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var groupHeader = dec.JobComInvoiceGroupHeaders[0];
			var fobInvoice = groupHeader.JobComInvoiceHeaders.AddNew();
			fobInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			fobInvoice.JZ_InvoiceAmount = 10000m;
			fobInvoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;

			var groupCharges = groupHeader.Charges;
			groupCharges.AddNew(OverseasFreightCode, 1000m, dec.LocalCurrencyCode).J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
			var deductionCharge = groupCharges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 100m, dec.LocalCurrencyCode);
			deductionCharge.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
			PerformExtraDeductionChargeInitialisation(deductionCharge);
			var groupCommission = groupCharges.AddNew(CustomsChargeTypeList.Codes.Commission, 100m, dec.LocalCurrencyCode);
			groupCommission.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;

			dec.ResumeApportionment();
			AssertEquals("FOB Invoice has three charges apportioned", 3, fobInvoice.GroupCharges.Count);
			AssertEquals("First row is OFT", OverseasFreightCode, fobInvoice.GroupCharges[0].J7_ChargeType);
			AssertEquals("Apportioned OFT not included in lines", false, fobInvoice.GroupCharges[0].J7_IsIncludedInITOT);

			AssertEquals("Second row is DED", CustomsChargeTypeList.Codes.DeductionCharge, fobInvoice.GroupCharges[1].J7_ChargeType);
			AssertEquals("Apportioned DED not included in lines", false, fobInvoice.GroupCharges[1].J7_IsIncludedInITOT);

			groupCommission.J7_IsIncludedInITOT = true;
			dec.ResumeApportionment();

			var result = new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)groupHeader).ChargesToImportForLandedCosting).ToArray();
			AssertEquals("Two charges in result", 2, result.Length);
			AssertEquals("Overseas freight code", true, result[0].ChargeDescription.Contains(OverseasFreightDescription.ToUpper()));
			AssertEquals("DED should be brought", true, result[1].ChargeDescription.Contains("Deduction (or Discount) from Entry"));
			AssertEquals("DED should be brought as a negative amount", -100m, result[1].AmountToDistribute.Amount);
			AssertEquals("OTH charge should not be brought as this is included in lines", false, result[1].ChargeDescription.Contains(CustomsChargeTypeList.Descriptions.Commission.ToString().ToUpper()));
		}

		protected override string OverseasFreightCode => ChargeCodeList.Codes.OverseasFreight;

		protected override string OverseasFreightDescription => ChargeCodeList.Descriptions.OverseasFreight;

		protected override Type ExpectedTypeOfCharges => typeof(EU.Business.Declaration.GroupInvoiceChargeCollection<GroupInvoiceCharge>);

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			header = declaration.JobComInvoiceGroupHeaders[0];
		}
		JobComInvoiceGroupHeader header;
	}

	class JobComInvoiceGroupHeaderForTest : JobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new string GetStandaloneIncoTermAndChargeFactoryCountryContext() => base.GetStandaloneIncoTermAndChargeFactoryCountryContext();
	}
}
