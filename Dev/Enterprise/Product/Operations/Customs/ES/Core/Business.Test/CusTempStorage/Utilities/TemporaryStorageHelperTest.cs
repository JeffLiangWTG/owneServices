using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusTempStorageRegLineItem = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItem;
using CusTempStorageRegLineTransactionTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList;
using CusTempStorageRegPremises = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageHelper))]
sealed class TemporaryStorageHelperTest : TestCaseWithFactory
{
	public void TestSetPremisesAndGuaranteeIntoRegHeader()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		var premises1 = Factory.New<CusTempStorageRegPremises>();
		premises1.SRP_Type = "ADT";
		premises1.SRP_Code = "COD";
		premises1.SRP_Description = "Desc";
		premises1.SRP_OA_PremisesAddress = orgAddress.PK;
		premises1.SRP_CustomsLocation = "ES009999000002";

		var premises2 = Factory.New<CusTempStorageRegPremises>();
		premises2.SRP_Type = "LAM";
		premises2.SRP_Code = "CO2";
		premises2.SRP_Description = "Desc2";
		premises2.SRP_OA_PremisesAddress = orgAddress.PK;
		premises2.SRP_CustomsLocation = "ES009999000002";

		var premises3 = Factory.New<CusTempStorageRegPremises>();
		premises3.SRP_Type = "LAM";
		premises3.SRP_Code = "CO3";
		premises3.SRP_Description = "Desc3";
		premises3.SRP_OA_PremisesAddress = orgAddress.PK;
		premises3.SRP_CustomsLocation = "ES009999000003";

		var cusGuarantee = Factory.New<CusGuaranteeHeader>();
		cusGuarantee.CPH_Number = "Test1";
		cusGuarantee.CPH_OH_PermitHolder = orgHeader.PK;
		cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		cusGuarantee.CPH_SubType = "1";
		cusGuarantee.CPH_StartDate = ZDate.BrettsBirthday;
		cusGuarantee.CPH_Balance = 1000.0m;

		Factory.Save();

		var regHeader1 = Factory.New<CusTempStorageRegHeader>();
		var regHeader2 = Factory.New<CusTempStorageRegHeader>();
		var regHeader3 = Factory.New<CusTempStorageRegHeader>();

		var guarantee = Factory.New<CommonGuarantee>();
		guarantee.Parent = regHeader3;
		guarantee.PW_BondAmount = 5.0m;
		guarantee.PW_Override = true;
		guarantee.PW_BondNumber = "Test1";
		guarantee.PW_RX_NKCurrency = "EUR";
		guarantee.PW_CPH_Guarantee = cusGuarantee.PK;

		CombineAssertions(() =>
		{
			TemporaryStorageHelper.SetPremisesAndGuaranteeIntoRegHeader(Factory, regHeader1, "ES009999000002", guarantee, false);

			AssertEquals("RegHeader Premises is set for not LAM", premises1.PK, regHeader1.SRH_SRP_Premises);
			AssertEquals("RegHeader Guarantee PW_BondAmount is set for not LAM", guarantee.PW_BondAmount, regHeader1.Guarantee.PW_BondAmount);
			AssertEquals("RegHeader Guarantee PW_BondNumber is set for not LAM", guarantee.PW_BondNumber, regHeader1.Guarantee.PW_BondNumber);
			AssertEquals("RegHeader Guarantee PW_RX_NKCurrency is set for not LAM", guarantee.PW_RX_NKCurrency, regHeader1.Guarantee.PW_RX_NKCurrency);
			AssertEquals("RegHeader Guarantee PW_CPH_Guarantee is set for not LAM", guarantee.PW_CPH_Guarantee, regHeader1.Guarantee.PW_CPH_Guarantee);

			TemporaryStorageHelper.SetPremisesAndGuaranteeIntoRegHeader(Factory, regHeader2, "ES009999000002", guarantee, true);

			AssertEquals("RegHeader Premises is set for LAM", premises2.PK, regHeader2.SRH_SRP_Premises);
			AssertEquals("RegHeader Guarantee PW_BondAmount is not set for LAM", ZDecimal.Zero, regHeader2.Guarantee.PW_BondAmount);
			AssertEquals("RegHeader Guarantee PW_BondNumber is not set for LAM", ZString.Empty, regHeader2.Guarantee.PW_BondNumber);
			AssertEquals("RegHeader Guarantee PW_RX_NKCurrency is not set for LAM", ZString.Empty, regHeader2.Guarantee.PW_RX_NKCurrency);
			AssertEquals("RegHeader Guarantee PW_CPH_Guarantee is not set for LAM", ZGuid.Empty, regHeader2.Guarantee.PW_CPH_Guarantee);
		});
	}

	public void TestCreateRegisterDataForPackage_G5()
	{
		var packPK = new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6");
		var packType = "BX";
		var packQty = 5;
		var packMarks = "marks";
		var pivotAndTransactionGrossWeightCalculated = 3.000085m;
		var itemGrossWeight = 10.0m;
		var itemLiabilityAmount = 2.0m;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		var jobReference = "Reference";
		var arrivalDate = new ZDate(2024, 02, 16);
		var issueDate = new ZDateTimeOffset(2024, 02, 16);
		var unloadingDate = new ZDateTimeOffset(2024, 02, 17);
		var traderId = "GB555555555";
		var transactionReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;

		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		var itemNumber = 1;
		var regLineSeq = 1;
		var regLineAndPackTupleList = new List<Tuple<ZGuid, ZGuid>>();

		(itemNumber, regLineSeq, var transactionPK, var transactionAndPivotGrossWeight, var transactionBondAmount, var regLinePK) = TemporaryStorageHelper.CreateRegisterDataForPackage(Factory, packType, packQty, packMarks, pivotAndTransactionGrossWeightCalculated, itemGrossWeight, itemLiabilityAmount, regHeader.PK, arrivalDate, traderId, regLineItem.PK, itemNumber, regLineSeq, jobReference, issueDate, unloadingDate, transactionReferenceType, packPK, regLineAndPackTupleList, false, false);

		var queryLine = new ZQuery(CusTempStorageRegLineSchema.SRL_SRH, regHeader.PK);
		var regLineCreated = Factory.LoadTop1<CusTempStorageRegLine>(queryLine);

		var queryPivot = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLineCreated.PK);
		var pivotCreated = Factory.LoadTop1<CusTempStorageRegLineItemPivot>(queryPivot);

		var queryLineTransaction = new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLineCreated.PK);
		var lineTransactionCreated = Factory.LoadTop1<CusTempStorageRegLineTransaction>(queryLineTransaction);

		CombineAssertions(() =>
		{
			AssertEquals("itemNumber should not change", 1, itemNumber);
			AssertEquals("regLineSeq should increment", 2, regLineSeq);
			AssertEquals("transactionPK should be transaction created", lineTransactionCreated.PK, transactionPK);
			AssertEquals("transactionAndPivotGrossWeight should be truncated", 3.00008m, transactionAndPivotGrossWeight);
			AssertEquals("transactionBondAmount should be calculated", 0.6m, transactionBondAmount);
			AssertEquals("regLinePK should be regLine created", regLineCreated.PK, regLinePK);

			AssertEquals("regLine is associated to regHeader", regHeader.PK, regLineCreated.SRL_SRH);
			AssertEquals("regLine SRL_LineNumber", 1, regLineCreated.SRL_LineNumber);
			AssertEquals("regLine SRL_LimitDate", new ZDate(2024, 05, 16), regLineCreated.SRL_LimitDate);
			AssertEquals("regLine SRL_PackageType", packType, regLineCreated.SRL_PackageType);
			AssertEquals("regLine SRL_GrossWeightUQ", Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, regLineCreated.SRL_GrossWeightUQ);
			AssertEquals("regLine SRL_CustomsStatus", "OPN", regLineCreated.SRL_CustomsStatus);
			AssertEquals("regLine SRL_GoodsOwnerIdentifier", traderId, regLineCreated.SRL_GoodsOwnerIdentifier);
			AssertEquals("regLine SRL_PackageMarks", packMarks, regLineCreated.SRL_PackageMarks);
			AssertEquals("regLine SRL_UnionStatus", "TER", regLineCreated.SRL_UnionStatus);

			AssertEquals("pivot is associated to regLine", regLineCreated.PK, pivotCreated.SRV_SRL_Line);
			AssertEquals("pivot is associated to regItem", regLineItem.PK, pivotCreated.SRV_SRI_Item);
			AssertEquals("pivot SRV_GrossWeight", 3.00008m, pivotCreated.SRV_GrossWeight);

			AssertEquals("lineTransactionCreated is associated to regLine", regLineCreated.PK, lineTransactionCreated.SRT_SRL);
			AssertEquals("lineTransactionCreated SRT_GrossWeight", 3.00008m, lineTransactionCreated.SRT_GrossWeight);
			AssertEquals("lineTransactionCreated SRT_PackageQty", packQty, lineTransactionCreated.SRT_PackageQty);
			AssertEquals("lineTransactionCreated SRT_TransactionType", CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, lineTransactionCreated.SRT_TransactionType);
			AssertEquals("lineTransactionCreated SRT_InternalReferenceType", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, lineTransactionCreated.SRT_InternalReferenceType);
			AssertEquals("lineTransactionCreated SRT_InternalReferenceNumber", "Reference", lineTransactionCreated.SRT_InternalReferenceNumber);
			AssertEquals("lineTransactionCreated SRT_TransactionDate", new ZDateTimeOffset(2024, 02, 16), lineTransactionCreated.SRT_TransactionDate);
			AssertEquals("lineTransactionCreated SRT_PhysicalInOutDate", new ZDateTimeOffset(2024, 02, 17), lineTransactionCreated.SRT_PhysicalInOutDate);
			AssertEquals("lineTransactionCreated SRT_BondAmount", 0.6m, lineTransactionCreated.SRT_BondAmount);

			regLineAndPackTupleList.Add(new Tuple<ZGuid, ZGuid>(regLinePK, packPK));
			regLineItem.SRI_GoodsItemNumber = itemNumber;
			pivotAndTransactionGrossWeightCalculated = 5.0m;
			itemGrossWeight = 25.0m;
			itemLiabilityAmount = 30.0m;
			itemNumber = 2;
			(itemNumber, regLineSeq, transactionPK, transactionAndPivotGrossWeight, transactionBondAmount, regLinePK) = TemporaryStorageHelper.CreateRegisterDataForPackage(Factory, packType, packQty, packMarks, pivotAndTransactionGrossWeightCalculated, itemGrossWeight, itemLiabilityAmount, regHeader.PK, arrivalDate, traderId, regLineItem.PK, itemNumber, regLineSeq, jobReference, issueDate, unloadingDate, transactionReferenceType, packPK, regLineAndPackTupleList, false);

			var regLinesCreated = Factory.Load<CusTempStorageRegLine>(queryLine).Length;
			queryPivot = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLineCreated.PK);
			_ = queryPivot.AddToFilter(CusTempStorageRegLineItemPivotSchema.PK, SQLComparisonOperator.NotEqual, pivotCreated.PK);
			var pivot2Created = Factory.LoadTop1<CusTempStorageRegLineItemPivot>(queryPivot);
			var lineTransactionsCreated = Factory.Load<CusTempStorageRegLineTransaction>(queryLineTransaction).Length;
			lineTransactionCreated = Factory.LoadTop1<CusTempStorageRegLineTransaction>(queryLineTransaction);

			AssertEquals("itemNumber should change to exisiting one", 2, itemNumber);
			AssertEquals("regLineSeq should not increment", 2, regLineSeq);
			AssertEquals("transactionBondAmount should be calculated", 6m, transactionBondAmount);
			AssertEquals("No new regLine is created", 1, regLinesCreated);

			AssertEquals("pivot is associated to regLine", regLineCreated.PK, pivot2Created.SRV_SRL_Line);
			AssertEquals("pivot is associated to regItem", regLineItem.PK, pivot2Created.SRV_SRI_Item);
			AssertEquals("pivot SRV_GrossWeight", 5.0m, pivot2Created.SRV_GrossWeight);

			AssertEquals("No new transaction is created", 1, lineTransactionsCreated);
			AssertEquals("lineTransactionCreated SRT_GrossWeight is added new qty", 8.00008m, lineTransactionCreated.SRT_GrossWeight);
			AssertEquals("lineTransactionCreated SRT_BondAmount is added new qty", 6.6m, lineTransactionCreated.SRT_BondAmount);

			var packPK2 = new ZGuid("C766C272-6EF8-4961-94B0-0D276C23974A");
			pivotAndTransactionGrossWeightCalculated = 6.0m;
			itemGrossWeight = 30.0m;
			itemLiabilityAmount = 20.0m;
			itemNumber = 5;
			(itemNumber, regLineSeq, transactionPK, transactionAndPivotGrossWeight, transactionBondAmount, regLinePK) = TemporaryStorageHelper.CreateRegisterDataForPackage(Factory, packType, packQty, packMarks, pivotAndTransactionGrossWeightCalculated, itemGrossWeight, itemLiabilityAmount, regHeader.PK, arrivalDate, traderId, regLineItem.PK, itemNumber, regLineSeq, jobReference, issueDate, unloadingDate, transactionReferenceType, packPK2, regLineAndPackTupleList, false);

			var regLinesCreated2 = Factory.Load<CusTempStorageRegLine>(queryLine);
			AssertEquals("New regLine is created", 2, regLinesCreated2.Length);
			var regLineCreated2 = regLinesCreated2.First(l => l.PK == regLinePK);

			var queryPivot2 = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLineCreated2.PK);
			var pivot3Created = Factory.LoadTop1<CusTempStorageRegLineItemPivot>(queryPivot2);

			var queryLineTransaction2 = new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLineCreated2.PK);
			var lineTransaction2Created = Factory.LoadTop1<CusTempStorageRegLineTransaction>(queryLineTransaction2);

			AssertEquals("itemNumber should not change for second pack", 5, itemNumber);
			AssertEquals("regLineSeq should increment for second pack", 3, regLineSeq);
			AssertEquals("transactionPK should be transaction created for second pack", lineTransaction2Created.PK, transactionPK);
			AssertEquals("transactionAndPivotGrossWeight should be truncated for second pack", 6m, transactionAndPivotGrossWeight);
			AssertEquals("transactionBondAmount should be calculated for second pack", 4m, transactionBondAmount);

			AssertEquals("regLine is associated to regHeader for second pack", regHeader.PK, regLineCreated2.SRL_SRH);
			AssertEquals("regLine SRL_LineNumber for second pack", 2, regLineCreated2.SRL_LineNumber);
			AssertEquals("regLine SRL_LimitDate for second pack", new ZDate(2024, 05, 16), regLineCreated2.SRL_LimitDate);
			AssertEquals("regLine SRL_PackageType for second pack", packType, regLineCreated2.SRL_PackageType);
			AssertEquals("regLine SRL_GrossWeightUQ for second pack", Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, regLineCreated2.SRL_GrossWeightUQ);
			AssertEquals("regLine SRL_CustomsStatus for second pack", "OPN", regLineCreated2.SRL_CustomsStatus);
			AssertEquals("regLine SRL_GoodsOwnerIdentifier for second pack", traderId, regLineCreated2.SRL_GoodsOwnerIdentifier);
			AssertEquals("regLine SRL_PackageMarks for second pack", packMarks, regLineCreated2.SRL_PackageMarks);
			AssertEquals("regLine SRL_UnionStatus for second pack", "TER", regLineCreated2.SRL_UnionStatus);

			AssertEquals("pivot is associated to regLine for second pack", regLineCreated2.PK, pivot3Created.SRV_SRL_Line);
			AssertEquals("pivot is associated to regItem for second pack", regLineItem.PK, pivot3Created.SRV_SRI_Item);
			AssertEquals("pivot SRV_GrossWeight for second pack", 6m, pivot3Created.SRV_GrossWeight);

			AssertEquals("lineTransactionCreated is associated to regLine for second pack", regLineCreated2.PK, lineTransaction2Created.SRT_SRL);
			AssertEquals("lineTransactionCreated SRT_GrossWeight for second pack", 6m, lineTransaction2Created.SRT_GrossWeight);
			AssertEquals("lineTransactionCreated SRT_PackageQty for second pack", packQty, lineTransaction2Created.SRT_PackageQty);
			AssertEquals("lineTransactionCreated SRT_TransactionType for second pack", CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, lineTransaction2Created.SRT_TransactionType);
			AssertEquals("lineTransactionCreated SRT_InternalReferenceType for second pack", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, lineTransaction2Created.SRT_InternalReferenceType);
			AssertEquals("lineTransactionCreated SRT_InternalReferenceNumber for second pack", "Reference", lineTransaction2Created.SRT_InternalReferenceNumber);
			AssertEquals("lineTransactionCreated SRT_TransactionDate for second pack", new ZDateTimeOffset(2024, 02, 16), lineTransaction2Created.SRT_TransactionDate);
			AssertEquals("lineTransactionCreated SRT_PhysicalInOutDate for second pack", new ZDateTimeOffset(2024, 02, 17), lineTransaction2Created.SRT_PhysicalInOutDate);
			AssertEquals("lineTransactionCreated SRT_BondAmount for second pack", 4m, lineTransaction2Created.SRT_BondAmount);
		});
	}

	public void TestCreateRegisterDataForPackage_TSM()
	{
		var packPK = new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6");
		var packType = "BX";
		var packQty = 5;
		var packMarks = "marks";
		var pivotAndTransactionGrossWeightCalculated = 3.000085m;
		var itemGrossWeight = 10.0m;
		var itemLiabilityAmount = 2.0m;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		var jobReference = "Reference";
		var arrivalDate = new ZDate(2024, 02, 16);
		var issueDate = new ZDateTimeOffset(2024, 02, 16);
		var unloadingDate = new ZDateTimeOffset(2024, 02, 17);
		var traderId = "GB555555555";
		var transactionReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry;

		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		var itemNumber = 1;
		var regLineSeq = 1;
		var regLineAndPackTupleList = new List<Tuple<ZGuid, ZGuid>>();

		(itemNumber, regLineSeq, var transactionPK, var transactionAndPivotGrossWeight, var transactionBondAmount, var regLinePK) = TemporaryStorageHelper.CreateRegisterDataForPackage(Factory, packType, packQty, packMarks, pivotAndTransactionGrossWeightCalculated, itemGrossWeight, itemLiabilityAmount, regHeader.PK, arrivalDate, traderId, regLineItem.PK, itemNumber, regLineSeq, jobReference, issueDate, unloadingDate, transactionReferenceType, packPK, regLineAndPackTupleList, false, false);

		var queryLine = new ZQuery(CusTempStorageRegLineSchema.SRL_SRH, regHeader.PK);
		var regLineCreated = Factory.LoadTop1<CusTempStorageRegLine>(queryLine);

		var queryPivot = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLineCreated.PK);
		var pivotCreated = Factory.LoadTop1<CusTempStorageRegLineItemPivot>(queryPivot);

		var queryLineTransaction = new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLineCreated.PK);
		var lineTransactionCreated = Factory.LoadTop1<CusTempStorageRegLineTransaction>(queryLineTransaction);

		AssertCreateRegisterDataForPackage_TSM(itemNumber, regLineSeq, lineTransactionCreated, transactionPK, transactionAndPivotGrossWeight, transactionBondAmount, 0.6m, regLineCreated, regLinePK, regHeader, packType,
			traderId, packMarks, pivotCreated, regLineItem, packQty, "TER");

		CombineAssertions(() =>
		{
			regLineAndPackTupleList.Add(new Tuple<ZGuid, ZGuid>(regLinePK, packPK));
			regLineItem.SRI_GoodsItemNumber = itemNumber;
			pivotAndTransactionGrossWeightCalculated = 5.0m;
			itemGrossWeight = 25.0m;
			itemLiabilityAmount = 30.0m;
			itemNumber = 2;
			(itemNumber, regLineSeq, transactionPK, transactionAndPivotGrossWeight, transactionBondAmount, regLinePK) = TemporaryStorageHelper.CreateRegisterDataForPackage(Factory, packType, packQty, packMarks, pivotAndTransactionGrossWeightCalculated, itemGrossWeight, itemLiabilityAmount, regHeader.PK, arrivalDate, traderId, regLineItem.PK, itemNumber, regLineSeq, jobReference, issueDate, unloadingDate, transactionReferenceType, packPK, regLineAndPackTupleList, false);

			var regLinesCreated = Factory.Load<CusTempStorageRegLine>(queryLine).Length;
			queryPivot = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLineCreated.PK);
			_ = queryPivot.AddToFilter(CusTempStorageRegLineItemPivotSchema.PK, SQLComparisonOperator.NotEqual, pivotCreated.PK);
			var pivot2Created = Factory.LoadTop1<CusTempStorageRegLineItemPivot>(queryPivot);
			var lineTransactionsCreated = Factory.Load<CusTempStorageRegLineTransaction>(queryLineTransaction).Length;
			lineTransactionCreated = Factory.LoadTop1<CusTempStorageRegLineTransaction>(queryLineTransaction);

			AssertEquals("itemNumber should change to exisiting one", 2, itemNumber);
			AssertEquals("regLineSeq should not increment", 2, regLineSeq);
			AssertEquals("transactionBondAmount should be calculated", 6m, transactionBondAmount);
			AssertEquals("No new regLine is created", 1, regLinesCreated);

			AssertEquals("pivot is associated to regLine", regLineCreated.PK, pivot2Created.SRV_SRL_Line);
			AssertEquals("pivot is associated to regItem", regLineItem.PK, pivot2Created.SRV_SRI_Item);
			AssertEquals("pivot SRV_GrossWeight", 5.0m, pivot2Created.SRV_GrossWeight);

			AssertEquals("No new transaction is created", 1, lineTransactionsCreated);
			AssertEquals("lineTransactionCreated SRT_GrossWeight is added new qty", 8.00008m, lineTransactionCreated.SRT_GrossWeight);
			AssertEquals("lineTransactionCreated SRT_BondAmount is added new qty", 6.6m, lineTransactionCreated.SRT_BondAmount);

			var packPK2 = new ZGuid("C766C272-6EF8-4961-94B0-0D276C23974A");
			pivotAndTransactionGrossWeightCalculated = 6.0m;
			itemGrossWeight = 30.0m;
			itemLiabilityAmount = 20.0m;
			itemNumber = 5;
			(itemNumber, regLineSeq, transactionPK, transactionAndPivotGrossWeight, transactionBondAmount, regLinePK) = TemporaryStorageHelper.CreateRegisterDataForPackage(Factory, packType, packQty, packMarks, pivotAndTransactionGrossWeightCalculated, itemGrossWeight, itemLiabilityAmount, regHeader.PK, arrivalDate, traderId, regLineItem.PK, itemNumber, regLineSeq, jobReference, issueDate, unloadingDate, transactionReferenceType, packPK2, regLineAndPackTupleList, false);

			var regLinesCreated2 = Factory.Load<CusTempStorageRegLine>(queryLine);
			AssertEquals("New regLine is created", 2, regLinesCreated2.Length);
			var regLineCreated2 = regLinesCreated2.First(l => l.PK == regLinePK);

			var queryPivot2 = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLineCreated2.PK);
			var pivot3Created = Factory.LoadTop1<CusTempStorageRegLineItemPivot>(queryPivot2);

			var queryLineTransaction2 = new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLineCreated2.PK);
			var lineTransaction2Created = Factory.LoadTop1<CusTempStorageRegLineTransaction>(queryLineTransaction2);

			AssertEquals("itemNumber should not change for second pack", 5, itemNumber);
			AssertEquals("regLineSeq should increment for second pack", 3, regLineSeq);
			AssertEquals("transactionPK should be transaction created for second pack", lineTransaction2Created.PK, transactionPK);
			AssertEquals("transactionAndPivotGrossWeight should be truncated for second pack", 6m, transactionAndPivotGrossWeight);
			AssertEquals("transactionBondAmount should be calculated for second pack", 4m, transactionBondAmount);

			AssertEquals("regLine is associated to regHeader for second pack", regHeader.PK, regLineCreated2.SRL_SRH);
			AssertEquals("regLine SRL_LineNumber for second pack", 2, regLineCreated2.SRL_LineNumber);
			AssertEquals("regLine SRL_LimitDate for second pack", new ZDate(2024, 05, 16), regLineCreated2.SRL_LimitDate);
			AssertEquals("regLine SRL_PackageType for second pack", packType, regLineCreated2.SRL_PackageType);
			AssertEquals("regLine SRL_GrossWeightUQ for second pack", Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, regLineCreated2.SRL_GrossWeightUQ);
			AssertEquals("regLine SRL_CustomsStatus for second pack", "OPN", regLineCreated2.SRL_CustomsStatus);
			AssertEquals("regLine SRL_GoodsOwnerIdentifier for second pack", traderId, regLineCreated2.SRL_GoodsOwnerIdentifier);
			AssertEquals("regLine SRL_PackageMarks for second pack", packMarks, regLineCreated2.SRL_PackageMarks);
			AssertEquals("regLine SRL_UnionStatus for second pack", "TER", regLineCreated2.SRL_UnionStatus);

			AssertEquals("pivot is associated to regLine for second pack", regLineCreated2.PK, pivot3Created.SRV_SRL_Line);
			AssertEquals("pivot is associated to regItem for second pack", regLineItem.PK, pivot3Created.SRV_SRI_Item);
			AssertEquals("pivot SRV_GrossWeight for second pack", 6m, pivot3Created.SRV_GrossWeight);

			AssertEquals("lineTransactionCreated is associated to regLine for second pack", regLineCreated2.PK, lineTransaction2Created.SRT_SRL);
			AssertEquals("lineTransactionCreated SRT_GrossWeight for second pack", 6m, lineTransaction2Created.SRT_GrossWeight);
			AssertEquals("lineTransactionCreated SRT_PackageQty for second pack", packQty, lineTransaction2Created.SRT_PackageQty);
			AssertEquals("lineTransactionCreated SRT_TransactionType for second pack", CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, lineTransaction2Created.SRT_TransactionType);
			AssertEquals("lineTransactionCreated SRT_InternalReferenceType for second pack", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, lineTransaction2Created.SRT_InternalReferenceType);
			AssertEquals("lineTransactionCreated SRT_InternalReferenceNumber for second pack", "Reference", lineTransaction2Created.SRT_InternalReferenceNumber);
			AssertEquals("lineTransactionCreated SRT_TransactionDate for second pack", new ZDateTimeOffset(2024, 02, 16), lineTransaction2Created.SRT_TransactionDate);
			AssertEquals("lineTransactionCreated SRT_PhysicalInOutDate for second pack", new ZDateTimeOffset(2024, 02, 17), lineTransaction2Created.SRT_PhysicalInOutDate);
			AssertEquals("lineTransactionCreated SRT_BondAmount for second pack", 4m, lineTransaction2Created.SRT_BondAmount);
		});
	}

	public void TestCreateRegisterDataForPackage_TSM_UnionGoods()
	{
		var packPK = new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6");
		var packType = "BX";
		var packQty = 5;
		var packMarks = "marks";
		var pivotAndTransactionGrossWeightCalculated = 3.000085m;
		var itemGrossWeight = 10.0m;
		var itemLiabilityAmount = 2.0m;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		var jobReference = "Reference";
		var arrivalDate = new ZDate(2024, 02, 16);
		var issueDate = new ZDateTimeOffset(2024, 02, 16);
		var unloadingDate = new ZDateTimeOffset(2024, 02, 17);
		var traderId = "GB555555555";
		var transactionReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry;

		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		var itemNumber = 1;
		var regLineSeq = 1;
		var regLineAndPackTupleList = new List<Tuple<ZGuid, ZGuid>>();

		(itemNumber, regLineSeq, var transactionPK, var transactionAndPivotGrossWeight, var transactionBondAmount, var regLinePK) = TemporaryStorageHelper.CreateRegisterDataForPackage(Factory, packType, packQty, packMarks, pivotAndTransactionGrossWeightCalculated, itemGrossWeight, itemLiabilityAmount, regHeader.PK, arrivalDate, traderId, regLineItem.PK, itemNumber, regLineSeq, jobReference, issueDate, unloadingDate, transactionReferenceType, packPK, regLineAndPackTupleList, true, false);

		var queryLine = new ZQuery(CusTempStorageRegLineSchema.SRL_SRH, regHeader.PK);
		var regLineCreated = Factory.LoadTop1<CusTempStorageRegLine>(queryLine);

		var queryPivot = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLineCreated.PK);
		var pivotCreated = Factory.LoadTop1<CusTempStorageRegLineItemPivot>(queryPivot);

		var queryLineTransaction = new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLineCreated.PK);
		var lineTransactionCreated = Factory.LoadTop1<CusTempStorageRegLineTransaction>(queryLineTransaction);

		AssertCreateRegisterDataForPackage_TSM(itemNumber, regLineSeq, lineTransactionCreated, transactionPK, transactionAndPivotGrossWeight, transactionBondAmount, ZDecimal.Zero, regLineCreated, regLinePK, regHeader, packType,
			traderId, packMarks, pivotCreated, regLineItem, packQty, "COM");
	}

	void AssertCreateRegisterDataForPackage_TSM(int itemNumber, int regLineSeq, CusTempStorageRegLineTransaction lineTransactionCreated, ZGuid transactionPK, ZDecimal transactionAndPivotGrossWeight, ZDecimal transactionBondAmount,
		ZDecimal expectedTransactionBondAmount, CusTempStorageRegLine regLineCreated, ZGuid regLinePK, CusTempStorageRegHeader regHeader, ZString packType, ZString traderId, ZString packMarks, CusTempStorageRegLineItemPivot pivotCreated,
		CusTempStorageRegLineItem regLineItem, int packQty, ZString expectedUnionStatus)
	{
		CombineAssertions(() =>
		{
			AssertEquals("itemNumber should not change", 1, itemNumber);
			AssertEquals("regLineSeq should increment", 2, regLineSeq);
			AssertEquals("transactionPK should be transaction created", lineTransactionCreated.PK, transactionPK);
			AssertEquals("transactionAndPivotGrossWeight should be truncated", 3.00008m, transactionAndPivotGrossWeight);
			AssertEquals("transactionBondAmount should be calculated", expectedTransactionBondAmount, transactionBondAmount);
			AssertEquals("regLinePK should be regLine created", regLineCreated.PK, regLinePK);

			AssertEquals("regLine is associated to regHeader", regHeader.PK, regLineCreated.SRL_SRH);
			AssertEquals("regLine SRL_LineNumber", 1, regLineCreated.SRL_LineNumber);
			AssertEquals("regLine SRL_LimitDate", new ZDate(2024, 05, 16), regLineCreated.SRL_LimitDate);
			AssertEquals("regLine SRL_PackageType", packType, regLineCreated.SRL_PackageType);
			AssertEquals("regLine SRL_GrossWeightUQ", Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, regLineCreated.SRL_GrossWeightUQ);
			AssertEquals("regLine SRL_CustomsStatus", "OPN", regLineCreated.SRL_CustomsStatus);
			AssertEquals("regLine SRL_GoodsOwnerIdentifier", traderId, regLineCreated.SRL_GoodsOwnerIdentifier);
			AssertEquals("regLine SRL_PackageMarks", packMarks, regLineCreated.SRL_PackageMarks);
			AssertEquals("regLine SRL_UnionStatus", expectedUnionStatus, regLineCreated.SRL_UnionStatus);

			AssertEquals("pivot is associated to regLine", regLineCreated.PK, pivotCreated.SRV_SRL_Line);
			AssertEquals("pivot is associated to regItem", regLineItem.PK, pivotCreated.SRV_SRI_Item);
			AssertEquals("pivot SRV_GrossWeight", 3.00008m, pivotCreated.SRV_GrossWeight);

			AssertEquals("lineTransactionCreated is associated to regLine", regLineCreated.PK, lineTransactionCreated.SRT_SRL);
			AssertEquals("lineTransactionCreated SRT_GrossWeight", 3.00008m, lineTransactionCreated.SRT_GrossWeight);
			AssertEquals("lineTransactionCreated SRT_PackageQty", packQty, lineTransactionCreated.SRT_PackageQty);
			AssertEquals("lineTransactionCreated SRT_TransactionType", CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, lineTransactionCreated.SRT_TransactionType);
			AssertEquals("lineTransactionCreated SRT_InternalReferenceType", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, lineTransactionCreated.SRT_InternalReferenceType);
			AssertEquals("lineTransactionCreated SRT_InternalReferenceNumber", "Reference", lineTransactionCreated.SRT_InternalReferenceNumber);
			AssertEquals("lineTransactionCreated SRT_TransactionDate", new ZDateTimeOffset(2024, 02, 16), lineTransactionCreated.SRT_TransactionDate);
			AssertEquals("lineTransactionCreated SRT_PhysicalInOutDate", new ZDateTimeOffset(2024, 02, 17), lineTransactionCreated.SRT_PhysicalInOutDate);
			AssertEquals("lineTransactionCreated SRT_BondAmount", expectedTransactionBondAmount, lineTransactionCreated.SRT_BondAmount);
		});
	}

	public void TestCreateRegisterDataForPackage_LAM()
	{
		var packPK = new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6");
		var packType = "BX";
		var packQty = 5;
		var packMarks = "marks";
		var pivotAndTransactionGrossWeightCalculated = 3.000085m;
		var itemGrossWeight = 10.0m;
		var itemLiabilityAmount = 0m;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		var jobReference = "Reference";
		var arrivalDate = new ZDate(2024, 02, 16);
		var issueDate = new ZDateTimeOffset(2024, 02, 16);
		var unloadingDate = new ZDateTimeOffset(2024, 02, 17);
		var traderId = "GB555555555";
		var transactionReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry;

		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		var itemNumber = 1;
		var regLineSeq = 1;
		var regLineAndPackTupleList = new List<Tuple<ZGuid, ZGuid>>();

		(itemNumber, regLineSeq, var transactionPK, var transactionAndPivotGrossWeight, var transactionBondAmount, var regLinePK) = TemporaryStorageHelper.CreateRegisterDataForPackage(Factory, packType, packQty, packMarks, pivotAndTransactionGrossWeightCalculated, itemGrossWeight, itemLiabilityAmount, regHeader.PK, arrivalDate, traderId, regLineItem.PK, itemNumber, regLineSeq, jobReference, issueDate, unloadingDate, transactionReferenceType, packPK, regLineAndPackTupleList, false, false);

		var queryLine = new ZQuery(CusTempStorageRegLineSchema.SRL_SRH, regHeader.PK);
		var regLineCreated = Factory.LoadTop1<CusTempStorageRegLine>(queryLine);

		var queryPivot = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLineCreated.PK);
		var pivotCreated = Factory.LoadTop1<CusTempStorageRegLineItemPivot>(queryPivot);

		var queryLineTransaction = new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLineCreated.PK);
		var lineTransactionCreated = Factory.LoadTop1<CusTempStorageRegLineTransaction>(queryLineTransaction);

		CombineAssertions(() =>
		{
			AssertEquals("itemNumber should not change", 1, itemNumber);
			AssertEquals("regLineSeq should increment", 2, regLineSeq);
			AssertEquals("transactionPK should be transaction created", lineTransactionCreated.PK, transactionPK);
			AssertEquals("transactionAndPivotGrossWeight should be truncated", 3.00008m, transactionAndPivotGrossWeight);
			AssertEquals("transactionBondAmount should be calculated", 0m, transactionBondAmount);
			AssertEquals("regLinePK should be regLine created", regLineCreated.PK, regLinePK);

			AssertEquals("regLine is associated to regHeader", regHeader.PK, regLineCreated.SRL_SRH);
			AssertEquals("regLine SRL_LineNumber", 1, regLineCreated.SRL_LineNumber);
			AssertEquals("regLine SRL_LimitDate", ZDate.Empty, regLineCreated.SRL_LimitDate);
			AssertEquals("regLine SRL_PackageType", packType, regLineCreated.SRL_PackageType);
			AssertEquals("regLine SRL_GrossWeightUQ", Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, regLineCreated.SRL_GrossWeightUQ);
			AssertEquals("regLine SRL_CustomsStatus", "OPN", regLineCreated.SRL_CustomsStatus);
			AssertEquals("regLine SRL_GoodsOwnerIdentifier", traderId, regLineCreated.SRL_GoodsOwnerIdentifier);
			AssertEquals("regLine SRL_PackageMarks", packMarks, regLineCreated.SRL_PackageMarks);
			AssertEquals("regLine SRL_UnionStatus", "NAT", regLineCreated.SRL_UnionStatus);

			AssertEquals("pivot is associated to regLine", regLineCreated.PK, pivotCreated.SRV_SRL_Line);
			AssertEquals("pivot is associated to regItem", regLineItem.PK, pivotCreated.SRV_SRI_Item);
			AssertEquals("pivot SRV_GrossWeight", 3.00008m, pivotCreated.SRV_GrossWeight);

			AssertEquals("lineTransactionCreated is associated to regLine", regLineCreated.PK, lineTransactionCreated.SRT_SRL);
			AssertEquals("lineTransactionCreated SRT_GrossWeight", 3.00008m, lineTransactionCreated.SRT_GrossWeight);
			AssertEquals("lineTransactionCreated SRT_PackageQty", packQty, lineTransactionCreated.SRT_PackageQty);
			AssertEquals("lineTransactionCreated SRT_TransactionType", CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, lineTransactionCreated.SRT_TransactionType);
			AssertEquals("lineTransactionCreated SRT_InternalReferenceType", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry, lineTransactionCreated.SRT_InternalReferenceType);
			AssertEquals("lineTransactionCreated SRT_InternalReferenceNumber", "Reference", lineTransactionCreated.SRT_InternalReferenceNumber);
			AssertEquals("lineTransactionCreated SRT_TransactionDate", new ZDateTimeOffset(2024, 02, 16), lineTransactionCreated.SRT_TransactionDate);
			AssertEquals("lineTransactionCreated SRT_PhysicalInOutDate", new ZDateTimeOffset(2024, 02, 17), lineTransactionCreated.SRT_PhysicalInOutDate);
			AssertEquals("lineTransactionCreated SRT_BondAmount", 0m, lineTransactionCreated.SRT_BondAmount);

			regLineAndPackTupleList.Add(new Tuple<ZGuid, ZGuid>(regLinePK, packPK));
			regLineItem.SRI_GoodsItemNumber = itemNumber;
			pivotAndTransactionGrossWeightCalculated = 5.0m;
			itemGrossWeight = 25.0m;
			itemLiabilityAmount = 0m;
			itemNumber = 2;
			(itemNumber, regLineSeq, transactionPK, transactionAndPivotGrossWeight, transactionBondAmount, regLinePK) = TemporaryStorageHelper.CreateRegisterDataForPackage(Factory, packType, packQty, packMarks, pivotAndTransactionGrossWeightCalculated, itemGrossWeight, itemLiabilityAmount, regHeader.PK, arrivalDate, traderId, regLineItem.PK, itemNumber, regLineSeq, jobReference, issueDate, unloadingDate, transactionReferenceType, packPK, regLineAndPackTupleList, false);

			var regLinesCreated = Factory.Load<CusTempStorageRegLine>(queryLine).Length;
			queryPivot = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLineCreated.PK);
			_ = queryPivot.AddToFilter(CusTempStorageRegLineItemPivotSchema.PK, SQLComparisonOperator.NotEqual, pivotCreated.PK);
			var pivot2Created = Factory.LoadTop1<CusTempStorageRegLineItemPivot>(queryPivot);
			var lineTransactionsCreated = Factory.Load<CusTempStorageRegLineTransaction>(queryLineTransaction).Length;
			lineTransactionCreated = Factory.LoadTop1<CusTempStorageRegLineTransaction>(queryLineTransaction);

			AssertEquals("itemNumber should change to exisiting one", 2, itemNumber);
			AssertEquals("regLineSeq should not increment", 2, regLineSeq);
			AssertEquals("transactionBondAmount should be zero", 0m, transactionBondAmount);
			AssertEquals("No new regLine is created", 1, regLinesCreated);

			AssertEquals("pivot is associated to regLine", regLineCreated.PK, pivot2Created.SRV_SRL_Line);
			AssertEquals("pivot is associated to regItem", regLineItem.PK, pivot2Created.SRV_SRI_Item);
			AssertEquals("pivot SRV_GrossWeight", 5.0m, pivot2Created.SRV_GrossWeight);

			AssertEquals("No new transaction is created", 1, lineTransactionsCreated);
			AssertEquals("lineTransactionCreated SRT_GrossWeight is added new qty", 8.00008m, lineTransactionCreated.SRT_GrossWeight);
			AssertEquals("lineTransactionCreated SRT_BondAmount is still 0", 0m, lineTransactionCreated.SRT_BondAmount);

			var packPK2 = new ZGuid("C766C272-6EF8-4961-94B0-0D276C23974A");
			pivotAndTransactionGrossWeightCalculated = 6.0m;
			itemGrossWeight = 30.0m;
			itemLiabilityAmount = 20.0m;
			itemNumber = 5;
			(itemNumber, regLineSeq, transactionPK, transactionAndPivotGrossWeight, transactionBondAmount, regLinePK) = TemporaryStorageHelper.CreateRegisterDataForPackage(Factory, packType, packQty, packMarks, pivotAndTransactionGrossWeightCalculated, itemGrossWeight, itemLiabilityAmount, regHeader.PK, arrivalDate, traderId, regLineItem.PK, itemNumber, regLineSeq, jobReference, issueDate, unloadingDate, transactionReferenceType, packPK2, regLineAndPackTupleList, false);

			var regLinesCreated2 = Factory.Load<CusTempStorageRegLine>(queryLine);
			AssertEquals("New regLine is created", 2, regLinesCreated2.Length);
			var regLineCreated2 = regLinesCreated2.First(l => l.PK == regLinePK);

			var queryPivot2 = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLineCreated2.PK);
			var pivot3Created = Factory.LoadTop1<CusTempStorageRegLineItemPivot>(queryPivot2);

			var queryLineTransaction2 = new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLineCreated2.PK);
			var lineTransaction2Created = Factory.LoadTop1<CusTempStorageRegLineTransaction>(queryLineTransaction2);

			AssertEquals("itemNumber should not change for second pack", 5, itemNumber);
			AssertEquals("regLineSeq should increment for second pack", 3, regLineSeq);
			AssertEquals("transactionPK should be transaction created for second pack", lineTransaction2Created.PK, transactionPK);
			AssertEquals("transactionAndPivotGrossWeight should be truncated for second pack", 6m, transactionAndPivotGrossWeight);
			AssertEquals("transactionBondAmount should be calculated for second pack", 4m, transactionBondAmount);

			AssertEquals("regLine is associated to regHeader for second pack", regHeader.PK, regLineCreated2.SRL_SRH);
			AssertEquals("regLine SRL_LineNumber for second pack", 2, regLineCreated2.SRL_LineNumber);
			AssertEquals("regLine SRL_LimitDate for second pack", ZDate.Empty, regLineCreated2.SRL_LimitDate);
			AssertEquals("regLine SRL_PackageType for second pack", packType, regLineCreated2.SRL_PackageType);
			AssertEquals("regLine SRL_GrossWeightUQ for second pack", Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, regLineCreated2.SRL_GrossWeightUQ);
			AssertEquals("regLine SRL_CustomsStatus for second pack", "OPN", regLineCreated2.SRL_CustomsStatus);
			AssertEquals("regLine SRL_GoodsOwnerIdentifier for second pack", traderId, regLineCreated2.SRL_GoodsOwnerIdentifier);
			AssertEquals("regLine SRL_PackageMarks for second pack", packMarks, regLineCreated2.SRL_PackageMarks);
			AssertEquals("regLine SRL_UnionStatus for second pack", "NAT", regLineCreated2.SRL_UnionStatus);

			AssertEquals("pivot is associated to regLine for second pack", regLineCreated2.PK, pivot3Created.SRV_SRL_Line);
			AssertEquals("pivot is associated to regItem for second pack", regLineItem.PK, pivot3Created.SRV_SRI_Item);
			AssertEquals("pivot SRV_GrossWeight for second pack", 6m, pivot3Created.SRV_GrossWeight);

			AssertEquals("lineTransactionCreated is associated to regLine for second pack", regLineCreated2.PK, lineTransaction2Created.SRT_SRL);
			AssertEquals("lineTransactionCreated SRT_GrossWeight for second pack", 6m, lineTransaction2Created.SRT_GrossWeight);
			AssertEquals("lineTransactionCreated SRT_PackageQty for second pack", packQty, lineTransaction2Created.SRT_PackageQty);
			AssertEquals("lineTransactionCreated SRT_TransactionType for second pack", CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, lineTransaction2Created.SRT_TransactionType);
			AssertEquals("lineTransactionCreated SRT_InternalReferenceType for second pack", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry, lineTransaction2Created.SRT_InternalReferenceType);
			AssertEquals("lineTransactionCreated SRT_InternalReferenceNumber for second pack", "Reference", lineTransaction2Created.SRT_InternalReferenceNumber);
			AssertEquals("lineTransactionCreated SRT_TransactionDate for second pack", new ZDateTimeOffset(2024, 02, 16), lineTransaction2Created.SRT_TransactionDate);
			AssertEquals("lineTransactionCreated SRT_PhysicalInOutDate for second pack", new ZDateTimeOffset(2024, 02, 17), lineTransaction2Created.SRT_PhysicalInOutDate);
			AssertEquals("lineTransactionCreated SRT_BondAmount for second pack", 4m, lineTransaction2Created.SRT_BondAmount);
		});
	}

	public void TestCreateRegisterDataForPackage_TRA()
	{
		var packPK = new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6");
		var packType = "BX";
		var packQty = 5;
		var packMarks = "marks";
		var pivotAndTransactionGrossWeightCalculated = 3.000085m;
		var itemGrossWeight = 10.0m;
		var itemLiabilityAmount = 2.0m;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		var jobReference = "Reference";
		var arrivalDate = new ZDate(2024, 02, 16);
		var issueDate = new ZDateTimeOffset(2024, 02, 16);
		var unloadingDate = new ZDateTimeOffset(2024, 02, 17);
		var traderId = "GB555555555";
		var transactionReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;

		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		var itemNumber = 1;
		var regLineSeq = 1;
		var regLineAndPackTupleList = new List<Tuple<ZGuid, ZGuid>>();

		(itemNumber, regLineSeq, var transactionPK, var transactionAndPivotGrossWeight, var transactionBondAmount, var regLinePK) = TemporaryStorageHelper.CreateRegisterDataForPackage(Factory, packType, packQty, packMarks, pivotAndTransactionGrossWeightCalculated, itemGrossWeight, itemLiabilityAmount, regHeader.PK, arrivalDate, traderId, regLineItem.PK, itemNumber, regLineSeq, jobReference, issueDate, unloadingDate, transactionReferenceType, packPK, regLineAndPackTupleList, false, false);

		var queryLine = new ZQuery(CusTempStorageRegLineSchema.SRL_SRH, regHeader.PK);
		var regLineCreated = Factory.LoadTop1<CusTempStorageRegLine>(queryLine);

		var queryPivot = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLineCreated.PK);
		var pivotCreated = Factory.LoadTop1<CusTempStorageRegLineItemPivot>(queryPivot);

		var queryLineTransaction = new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLineCreated.PK);
		var lineTransactionCreated = Factory.LoadTop1<CusTempStorageRegLineTransaction>(queryLineTransaction);

		CombineAssertions(() =>
		{
			AssertEquals("itemNumber should not change", 1, itemNumber);
			AssertEquals("regLineSeq should increment", 2, regLineSeq);
			AssertEquals("transactionPK should be transaction created", lineTransactionCreated.PK, transactionPK);
			AssertEquals("transactionAndPivotGrossWeight should be truncated", 3.00008m, transactionAndPivotGrossWeight);
			AssertEquals("transactionBondAmount should be calculated", 0.6m, transactionBondAmount);
			AssertEquals("regLinePK should be regLine created", regLineCreated.PK, regLinePK);

			AssertEquals("regLine is associated to regHeader", regHeader.PK, regLineCreated.SRL_SRH);
			AssertEquals("regLine SRL_LineNumber", 1, regLineCreated.SRL_LineNumber);
			AssertEquals("regLine SRL_LimitDate", new ZDate(2024, 05, 16), regLineCreated.SRL_LimitDate);
			AssertEquals("regLine SRL_PackageType", packType, regLineCreated.SRL_PackageType);
			AssertEquals("regLine SRL_GrossWeightUQ", Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, regLineCreated.SRL_GrossWeightUQ);
			AssertEquals("regLine SRL_CustomsStatus", "OPN", regLineCreated.SRL_CustomsStatus);
			AssertEquals("regLine SRL_GoodsOwnerIdentifier", traderId, regLineCreated.SRL_GoodsOwnerIdentifier);
			AssertEquals("regLine SRL_PackageMarks", packMarks, regLineCreated.SRL_PackageMarks);
			AssertEquals("regLine SRL_UnionStatus", "TER", regLineCreated.SRL_UnionStatus);

			AssertEquals("pivot is associated to regLine", regLineCreated.PK, pivotCreated.SRV_SRL_Line);
			AssertEquals("pivot is associated to regItem", regLineItem.PK, pivotCreated.SRV_SRI_Item);
			AssertEquals("pivot SRV_GrossWeight", 3.00008m, pivotCreated.SRV_GrossWeight);

			AssertEquals("lineTransactionCreated is associated to regLine", regLineCreated.PK, lineTransactionCreated.SRT_SRL);
			AssertEquals("lineTransactionCreated SRT_GrossWeight", 3.00008m, lineTransactionCreated.SRT_GrossWeight);
			AssertEquals("lineTransactionCreated SRT_PackageQty", packQty, lineTransactionCreated.SRT_PackageQty);
			AssertEquals("lineTransactionCreated SRT_TransactionType", CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, lineTransactionCreated.SRT_TransactionType);
			AssertEquals("lineTransactionCreated SRT_InternalReferenceType", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration, lineTransactionCreated.SRT_InternalReferenceType);
			AssertEquals("lineTransactionCreated SRT_InternalReferenceNumber", "Reference", lineTransactionCreated.SRT_InternalReferenceNumber);
			AssertEquals("lineTransactionCreated SRT_TransactionDate", new ZDateTimeOffset(2024, 02, 16), lineTransactionCreated.SRT_TransactionDate);
			AssertEquals("lineTransactionCreated SRT_PhysicalInOutDate", new ZDateTimeOffset(2024, 02, 17), lineTransactionCreated.SRT_PhysicalInOutDate);
			AssertEquals("lineTransactionCreated SRT_BondAmount", 0.6m, lineTransactionCreated.SRT_BondAmount);

			regLineAndPackTupleList.Add(new Tuple<ZGuid, ZGuid>(regLinePK, packPK));
			regLineItem.SRI_GoodsItemNumber = itemNumber;
			pivotAndTransactionGrossWeightCalculated = 5.0m;
			itemGrossWeight = 25.0m;
			itemLiabilityAmount = 30.0m;
			itemNumber = 2;
			(itemNumber, regLineSeq, transactionPK, transactionAndPivotGrossWeight, transactionBondAmount, regLinePK) = TemporaryStorageHelper.CreateRegisterDataForPackage(Factory, packType, packQty, packMarks, pivotAndTransactionGrossWeightCalculated, itemGrossWeight, itemLiabilityAmount, regHeader.PK, arrivalDate, traderId, regLineItem.PK, itemNumber, regLineSeq, jobReference, issueDate, unloadingDate, transactionReferenceType, packPK, regLineAndPackTupleList, false);

			var regLinesCreated = Factory.Load<CusTempStorageRegLine>(queryLine).Length;
			queryPivot = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLineCreated.PK);
			_ = queryPivot.AddToFilter(CusTempStorageRegLineItemPivotSchema.PK, SQLComparisonOperator.NotEqual, pivotCreated.PK);
			var pivot2Created = Factory.LoadTop1<CusTempStorageRegLineItemPivot>(queryPivot);
			var lineTransactionsCreated = Factory.Load<CusTempStorageRegLineTransaction>(queryLineTransaction).Length;
			lineTransactionCreated = Factory.LoadTop1<CusTempStorageRegLineTransaction>(queryLineTransaction);

			AssertEquals("itemNumber should stay with the existing line's number", 1, itemNumber);
			AssertEquals("regLineSeq should not increment", 2, regLineSeq);
			AssertEquals("transactionBondAmount should be calculated", 6m, transactionBondAmount);
			AssertEquals("No new regLine is created", 1, regLinesCreated);

			AssertEquals("pivot is associated to regLine", regLineCreated.PK, pivot2Created.SRV_SRL_Line);
			AssertEquals("pivot is associated to regItem", regLineItem.PK, pivot2Created.SRV_SRI_Item);
			AssertEquals("pivot SRV_GrossWeight", 5.0m, pivot2Created.SRV_GrossWeight);

			AssertEquals("No new transaction is created", 1, lineTransactionsCreated);
			AssertEquals("lineTransactionCreated SRT_GrossWeight is added new qty", 8.00008m, lineTransactionCreated.SRT_GrossWeight);
			AssertEquals("lineTransactionCreated SRT_BondAmount is added new qty", 6.6m, lineTransactionCreated.SRT_BondAmount);

			var packPK2 = new ZGuid("C766C272-6EF8-4961-94B0-0D276C23974A");
			pivotAndTransactionGrossWeightCalculated = 6.0m;
			itemGrossWeight = 30.0m;
			itemLiabilityAmount = 20.0m;
			itemNumber = 5;
			(itemNumber, regLineSeq, transactionPK, transactionAndPivotGrossWeight, transactionBondAmount, regLinePK) = TemporaryStorageHelper.CreateRegisterDataForPackage(Factory, packType, packQty, packMarks, pivotAndTransactionGrossWeightCalculated, itemGrossWeight, itemLiabilityAmount, regHeader.PK, arrivalDate, traderId, regLineItem.PK, itemNumber, regLineSeq, jobReference, issueDate, unloadingDate, transactionReferenceType, packPK2, regLineAndPackTupleList, false);

			var regLinesCreated2 = Factory.Load<CusTempStorageRegLine>(queryLine);
			AssertEquals("No new regLine is created", 1, regLinesCreated2.Length);
			var regLineCreated2 = regLinesCreated2.First(l => l.PK == regLinePK);

			queryPivot = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLineCreated.PK);
			_ = queryPivot.AddToFilter(CusTempStorageRegLineItemPivotSchema.PK, SQLComparisonOperator.NotEqual, pivotCreated.PK);
			_ = queryPivot.AddToFilter(CusTempStorageRegLineItemPivotSchema.PK, SQLComparisonOperator.NotEqual, pivot2Created.PK);
			var pivot3Created = Factory.LoadTop1<CusTempStorageRegLineItemPivot>(queryPivot);

			lineTransactionsCreated = Factory.Load<CusTempStorageRegLineTransaction>(queryLineTransaction).Length;
			AssertEquals("No new transaction is created", 1, lineTransactionsCreated);
			lineTransactionCreated = Factory.LoadTop1<CusTempStorageRegLineTransaction>(queryLineTransaction);

			AssertEquals("itemNumber should stay with the existing line's number for second pack", 1, itemNumber);
			AssertEquals("regLineSeq should not increment for second pack", 2, regLineSeq);
			AssertEquals("transactionAndPivotGrossWeight should be truncated for second pack", 6m, transactionAndPivotGrossWeight);
			AssertEquals("transactionBondAmount should be calculated for second pack", 4m, transactionBondAmount);

			AssertEquals("pivot is associated to regLine for second pack", regLineCreated2.PK, pivot3Created.SRV_SRL_Line);
			AssertEquals("pivot is associated to regItem for second pack", regLineItem.PK, pivot3Created.SRV_SRI_Item);
			AssertEquals("pivot SRV_GrossWeight for second pack", 6m, pivot3Created.SRV_GrossWeight);

			AssertEquals("lineTransactionCreated SRT_GrossWeight is added new qty for second pack", 14.00008m, lineTransactionCreated.SRT_GrossWeight);
			AssertEquals("lineTransactionCreated SRT_BondAmount is added new qty for second pack", 10.6m, lineTransactionCreated.SRT_BondAmount);
		});
	}

	public void TestCorrectDecimalValue()
	{
		CombineAssertions(() =>
		{
			var correctionAmount = 0.01M;
			var (diffAmount, value) = TemporaryStorageHelper.CorrectDecimalValue(0.00m, correctionAmount, 5.55m);
			AssertEquals("diffAmount is 0 when there is no difference", 0.0m, diffAmount);
			AssertEquals("value is not change when there is no difference", 5.55m, value);

			(diffAmount, value) = TemporaryStorageHelper.CorrectDecimalValue(0.01m, correctionAmount, 5.55m);
			AssertEquals("diffAmount should be 0 when there is difference", 0.0m, diffAmount);
			AssertEquals("value is substracted correctionAmount if diffAmount is greater than 0", 5.54m, value);

			(diffAmount, value) = TemporaryStorageHelper.CorrectDecimalValue(-0.01m, correctionAmount, 5.55m);
			AssertEquals("diffAmount should be 0 when there is difference", 0.0m, diffAmount);
			AssertEquals("value is added correctionAmount if diffAmount is less than 0", 5.56m, value);
		});
	}

	public void TestGetLineTransactionsFromListOfPks()
	{
		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "TST";
		regHeader.SRH_Reference = "Reference";
		var regLine1 = regHeader.CusTempStorageRegLines.AddNew();
		regLine1.SRL_LineNumber = 1;
		var transaction1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		transaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;

		var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
		regLine2.SRL_LineNumber = 2;
		var transaction2 = regLine2.CusTempStorageRegLineTransactions.AddNew();
		transaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		Factory.Save();

		var expectedTransactions = new List<CusTempStorageRegLineTransaction>() { transaction1, transaction2 };
		var transactionPKs = new List<ZGuid> { transaction1.PK, transaction2.PK };

		var transactions = TemporaryStorageHelper.GetLineTransactionsFromListOfPks(Factory, transactionPKs);

		AssertContainsExactElementsInAnyOrder("Transactions should be the same", expectedTransactions, transactions);
	}

	public void TestCorrectBondAmountInLineTransactions()
	{
		var transaction1 = Factory.New<CusTempStorageRegLineTransaction>();
		transaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		transaction1.SRT_BondAmount = 5.55m;
		var transaction2 = Factory.New<CusTempStorageRegLineTransaction>();
		transaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		transaction2.SRT_BondAmount = 6.66m;
		var transactions = new List<CusTempStorageRegLineTransaction> { transaction1, transaction2 };

		CombineAssertions(() =>
		{
			TemporaryStorageHelper.CorrectBondAmountInLineTransactions(transactions, 5.04m, 5.04m);
			AssertEquals("Transaction1: SRT_BondAmount is not change when there is no difference", 5.55m, transaction1.SRT_BondAmount);
			AssertEquals("Transaction2: SRT_BondAmount is not change when there is no difference", 6.66m, transaction2.SRT_BondAmount);

			TemporaryStorageHelper.CorrectBondAmountInLineTransactions(transactions, 5.02m, 5.04m);
			AssertEquals("Transaction1: SRT_BondAmount is substracted correctionAmount if diffAmount is greater than 0", 5.54m, transaction1.SRT_BondAmount);
			AssertEquals("Transaction2: SRT_BondAmount is substracted correctionAmount if diffAmount is greater than 0", 6.65m, transaction2.SRT_BondAmount);

			TemporaryStorageHelper.CorrectBondAmountInLineTransactions(transactions, 5.04m, 5.02m);
			AssertEquals("Transaction1: SRT_BondAmount is added correctionAmount if diffAmount is greater than 0", 5.55m, transaction1.SRT_BondAmount);
			AssertEquals("Transaction2: SRT_BondAmount is added correctionAmount if diffAmount is greater than 0", 6.66m, transaction2.SRT_BondAmount);
		});
	}

	public void TestCorrectGrossWeightInLineTransactions()
	{
		var regLine = Factory.New<CusTempStorageRegLine>();
		var transaction1 = regLine.CusTempStorageRegLineTransactions.AddNew();
		transaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		transaction1.SRT_GrossWeight = 5.00055m;
		var transaction2 = regLine.CusTempStorageRegLineTransactions.AddNew();
		transaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		transaction2.SRT_GrossWeight = 6.00066m;
		var transactions = new List<CusTempStorageRegLineTransaction> { transaction1, transaction2 };

		CombineAssertions(() =>
		{
			TemporaryStorageHelper.CorrectGrossWeightInLineTransactions(transactions, 5.00004m, 5.00004m);
			AssertEquals("Transaction1: SRT_GrossWeight is not change when there is no difference", 5.00055m, transaction1.SRT_GrossWeight);
			AssertEquals("Transaction2: SRT_GrossWeight is not change when there is no difference", 6.00066m, transaction2.SRT_GrossWeight);

			TemporaryStorageHelper.CorrectGrossWeightInLineTransactions(transactions, 5.00002m, 5.00004m);
			AssertEquals("Transaction1: SRT_GrossWeight is substracted correctionAmount if diffAmount is greater than 0", 5.00054m, transaction1.SRT_GrossWeight);
			AssertEquals("Transaction2: SRT_GrossWeight is substracted correctionAmount if diffAmount is greater than 0", 6.00065m, transaction2.SRT_GrossWeight);

			TemporaryStorageHelper.CorrectGrossWeightInLineTransactions(transactions, 5.00004m, 5.00002m);
			AssertEquals("Transaction1: SRT_GrossWeight is added correctionAmount if diffAmount is greater than 0", 5.00055m, transaction1.SRT_GrossWeight);
			AssertEquals("Transaction2: SRT_GrossWeight is added correctionAmount if diffAmount is greater than 0", 6.00066m, transaction2.SRT_GrossWeight);
		});
	}

	public void TestCorrectGrossWeightInItemPivots()
	{
		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "TST";
		regHeader.SRH_Reference = "Reference";
		var regLine1 = regHeader.CusTempStorageRegLines.AddNew();
		regLine1.SRL_LineNumber = 1;
		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		var pivot = regLine1.RegLineItemPivots.AddNew();
		pivot.SRV_SRI_Item = regLineItem.PK;
		pivot.SRV_GrossWeight = 5.00055m;

		Factory.Save();

		CombineAssertions(() =>
		{
			TemporaryStorageHelper.CorrectGrossWeightInItemPivots(Factory, regLineItem.PK, 5.00004m, 5.00004m);
			AssertEquals("SRV_GrossWeight is not change when there is no difference", 5.00055m, pivot.SRV_GrossWeight);

			TemporaryStorageHelper.CorrectGrossWeightInItemPivots(Factory, regLineItem.PK, 5.00002m, 5.00004m);
			AssertEquals("SRV_GrossWeight is substracted correctionAmount if diffAmount is greater than 0", 5.00054m, pivot.SRV_GrossWeight);

			TemporaryStorageHelper.CorrectGrossWeightInItemPivots(Factory, regLineItem.PK, 5.00004m, 5.00002m);
			AssertEquals("SRV_GrossWeight is added correctionAmount if diffAmount is greater than 0", 5.00055m, pivot.SRV_GrossWeight);
		});
	}
}
