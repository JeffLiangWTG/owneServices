using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;
using CusTempStorageRegLineTransactionTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList;
using CusTempStorageRegPremisesTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public static class TemporaryStorageHelper
{
	public static void SetPremisesAndGuaranteeIntoRegHeader(BusinessObjectFactory factory, CusTempStorageRegHeader regHeader, ZString goodsLocation, CommonGuarantee guarantee, ZBool isMessageTypeLAM)
	{
		var premisesType = isMessageTypeLAM ? CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility : CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		var premise = EU.Business.TemporaryStorageHelper.GetManagedPremises(factory, premisesType, goodsLocation);
		regHeader.SRH_SRP_Premises = premise.PK;

		var regHeaderGuarantee = regHeader.Guarantee;
		if (!isMessageTypeLAM && guarantee != null && guarantee.CusGuarantee != null)
		{
			regHeaderGuarantee.PW_BondAmount = guarantee.PW_BondAmount;
			regHeaderGuarantee.PW_BondNumber = guarantee.PW_BondNumber;
			regHeaderGuarantee.PW_RX_NKCurrency = guarantee.PW_RX_NKCurrency;
			regHeaderGuarantee.PW_CPH_Guarantee = guarantee.CusGuarantee.PK;
		}
	}

	public static (ZInt itemNumber, ZInt regLineSeq, ZGuid transactionPK, ZDecimal calculatedGrossWeight, ZDecimal calculatedBondAmount, ZGuid regLinePK) CreateRegisterDataForPackage
		(BusinessObjectFactory factory, ZString packType, ZInt packQty, ZString packFullMarks, ZDecimal pivotAndTransactionCalculatedGrossWeight, ZDecimal itemGrossWeight, ZDecimal itemLiabilityAmount, ZGuid regHeaderPK, ZDate regHeaderArrivalDate,
		ZString traderId, ZGuid regLineItemPK, ZInt itemNumber, ZInt regLineSeq, ZString referenceNum, ZDateTimeOffset issueDate, ZDateTimeOffset unloadingDate, ZString internalReferenceType, ZGuid packPK, IEnumerable<Tuple<ZGuid, ZGuid>> regLineAndPackTupleList, bool isMessageTypeTSMAndIsUnionGoods, bool shouldCheckExistingPackageInOtherRegLine = true)
	{
		const string lineCustomsStatusOPN = "OPN";
		const string lineUnionStatusTER = "TER";
		const string lineUnionStatusNAT = "NAT";
		const string lineUnionStatusCOM = "COM";
		const int decimalsForCalculatedGrossWeight = 5;
		const int decimalsForCalculatedBondAmount = 2;

		var packMarks = packFullMarks.Left(CusTempStorageRegLine.Schema.SRL_PackageMarksMaxLength);
		pivotAndTransactionCalculatedGrossWeight = pivotAndTransactionCalculatedGrossWeight.Truncate(decimalsForCalculatedGrossWeight);
		var transactionBondAmount = isMessageTypeTSMAndIsUnionGoods ? 0 : ((ZDecimal)(itemLiabilityAmount / itemGrossWeight * pivotAndTransactionCalculatedGrossWeight)).Truncate(decimalsForCalculatedBondAmount);

		var regLineTransactionPK = ZGuid.Empty;
		var regLinePK = ZGuid.Empty;

		if (shouldCheckExistingPackageInOtherRegLine)
		{
			if (internalReferenceType == CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration)
			{
				var queryLine = new ZQuery(CusTempStorageRegLineSchema.SRL_SRH, regHeaderPK);
				_ = queryLine.AddToFilter(CusTempStorageRegLineSchema.SRL_PackageType, packType);
				_ = queryLine.AddToFilter(CusTempStorageRegLineSchema.SRL_PackageMarks, packMarks);
				var existingLine = factory.LoadTop1<CusTempStorageRegLine>(queryLine);
				regLinePK = existingLine?.PK ?? ZGuid.Empty;
			}
			else
			{
				regLinePK = regLineAndPackTupleList.FirstOrDefault(x => x.Item2 == packPK)?.Item1 ?? ZGuid.Empty;
			}

			if (!regLinePK.IsEmpty)
			{
				if (internalReferenceType == CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration)
				{
					var queryLineItemPivot = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLinePK);
					var existingLineItemPivot = factory.LoadTop1<CusTempStorageRegLineItemPivot>(queryLineItemPivot);
					if (existingLineItemPivot != null)
					{
						itemNumber = existingLineItemPivot.RegLineItem.SRI_GoodsItemNumber;
					}
				}
			}
		}

		if (regLinePK.IsEmpty)
		{
			var regLine = factory.New<CusTempStorageRegLine>();
			regLine.SRL_SRH = regHeaderPK;
			regLine.SRL_LineNumber = regLineSeq;
			regLineSeq++;
			regLine.SRL_LimitDate = regHeaderArrivalDate.IsValid && internalReferenceType != CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry ? regHeaderArrivalDate.AddDays(90) : ZDate.Empty;
			regLine.SRL_PackageType = packType;
			regLine.SRL_GrossWeightUQ = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
			regLine.SRL_CustomsStatus = lineCustomsStatusOPN;
			regLine.SRL_GoodsOwnerIdentifier = traderId;
			regLine.SRL_PackageMarks = packMarks;
			regLine.SRL_UnionStatus = internalReferenceType == CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry
				? lineUnionStatusNAT
				: isMessageTypeTSMAndIsUnionGoods ? lineUnionStatusCOM : lineUnionStatusTER;
			regLinePK = regLine.PK;
		}

		var regLineItemPivot = factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot.SRV_SRL_Line = regLinePK;
		regLineItemPivot.SRV_SRI_Item = regLineItemPK;
		regLineItemPivot.SRV_GrossWeight = pivotAndTransactionCalculatedGrossWeight;

		var queryLineTransaction = new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLinePK);
		_ = queryLineTransaction.AddToFilter(CusTempStorageRegLineTransactionSchema.SRT_TransactionType, CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
		var existingLineTransaction = factory.LoadTop1<CusTempStorageRegLineTransaction>(queryLineTransaction);
		if (existingLineTransaction != null)
		{
			existingLineTransaction.SRT_GrossWeight += pivotAndTransactionCalculatedGrossWeight;
			existingLineTransaction.SRT_BondAmount += transactionBondAmount;
			regLineTransactionPK = existingLineTransaction.PK;
		}
		else
		{
			var regLineTransaction = factory.New<CusTempStorageRegLineTransaction>();
			regLineTransaction.SRT_SRL = regLinePK;
			regLineTransaction.SRT_GrossWeight = pivotAndTransactionCalculatedGrossWeight;
			regLineTransaction.SRT_PackageQty = packQty;
			regLineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction.SRT_InternalReferenceType = internalReferenceType;
			regLineTransaction.SRT_InternalReferenceNumber = referenceNum;
			regLineTransaction.SRT_TransactionDate = issueDate;
			regLineTransaction.SRT_PhysicalInOutDate = unloadingDate;
			regLineTransaction.SRT_BondAmount = transactionBondAmount;
			regLineTransactionPK = regLineTransaction.PK;
		}

		return (itemNumber, regLineSeq, regLineTransactionPK, pivotAndTransactionCalculatedGrossWeight, transactionBondAmount, regLinePK);
	}

	public static (decimal diff_amount, ZDecimal value) CorrectDecimalValue(decimal diff_amount, decimal correctionAmount, ZDecimal value)
	{
		if (diff_amount != 0)
		{
			if (diff_amount > 0)
			{
				value -= correctionAmount;
				diff_amount -= correctionAmount;
			}
			else
			{
				value += correctionAmount;
				diff_amount += correctionAmount;
			}
		}

		return (diff_amount, value);
	}

	public static List<CusTempStorageRegLineTransaction> GetLineTransactionsFromListOfPks(BusinessObjectFactory factory, List<ZGuid> transactionPKs)
	{
		var query = new ZQuery();
		transactionPKs.ForEach(t => query.AddToFilter(JoinCondition.Or, CusTempStorageRegLineTransactionSchema.PK, t));
		return factory.Load<CusTempStorageRegLineTransaction>(query).OrderBy(t => t.RegLine.SRL_LineNumber).ToList();
	}

	public static void CorrectBondAmountInLineTransactions(List<CusTempStorageRegLineTransaction> transactions, ZDecimal goodsItemLiabilityAmount, ZDecimal transactionsTotalBondAmount)
	{
		var diff_amount = transactionsTotalBondAmount - goodsItemLiabilityAmount;
		var correctionAmount = (decimal)0.01;
		if (diff_amount != 0)
		{
			transactions.ForEach(x =>
			{
				(diff_amount, x.SRT_BondAmount) = CorrectDecimalValue(diff_amount, correctionAmount, x.SRT_BondAmount);
			});
		}
	}

	public static void CorrectGrossWeightInLineTransactions(List<CusTempStorageRegLineTransaction> transactions, ZDecimal goodsItemGrossWeight, ZDecimal transactionsTotalGrossWeight)
	{
		var diff_amount = transactionsTotalGrossWeight - goodsItemGrossWeight;
		var correctionAmount = (decimal)0.00001;
		if (diff_amount != 0)
		{
			transactions.ForEach(x =>
			{
				(diff_amount, x.SRT_GrossWeight) = CorrectDecimalValue(diff_amount, correctionAmount, x.SRT_GrossWeight);
			});
		}
	}

	public static void CorrectGrossWeightInItemPivots(BusinessObjectFactory factory, ZGuid regLineItemPK, ZDecimal goodsItemGrossWeight, ZDecimal pivotsTotalGrossWeight)
	{
		var diff_amount = pivotsTotalGrossWeight - goodsItemGrossWeight;
		var correctionAmount = (decimal)0.00001;
		if (diff_amount != 0)
		{
			var queryPivot = new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRI_Item, regLineItemPK);
			var pivots = factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(queryPivot).OrderBy(p => p.RegLine.SRL_LineNumber);

			pivots.ForEach(x =>
			{
				(diff_amount, x.SRV_GrossWeight) = CorrectDecimalValue(diff_amount, correctionAmount, x.SRV_GrossWeight);
			});
		}
	}
}
