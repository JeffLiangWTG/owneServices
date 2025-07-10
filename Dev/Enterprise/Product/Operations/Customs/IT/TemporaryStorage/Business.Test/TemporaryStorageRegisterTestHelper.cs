using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using static NUnit.Framework.Assertion;
using static NUnit.Framework.AssertionWithHtml;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

static class TemporaryStorageRegisterTestHelper
{
	public static void AssertRegisterHeader(
		string message,
		CusTempStorageRegHeader registerHeader,
		ZString appCode,
		ZString reference,
		ZString internalReference,
		ZString status,
		ZString previousReferenceType,
		ZString movementReferenceNumber,
		ZDateTime presentationDate,
		ZString transportID,
		ZString customsOffice)
	{
		CombineAssertions(message, () =>
		{
			AssertEquals(nameof(registerHeader.SRH_AppCode), appCode, registerHeader.SRH_AppCode);
			AssertEquals(nameof(registerHeader.SRH_Reference), reference, registerHeader.SRH_Reference);
			AssertEquals(nameof(registerHeader.SRH_InternalReference), internalReference, registerHeader.SRH_InternalReference);
			AssertEquals(nameof(registerHeader.SRH_Status), status, registerHeader.SRH_Status);
			AssertEquals(nameof(registerHeader.SRH_PreviousReferenceType), previousReferenceType, registerHeader.SRH_PreviousReferenceType);
			AssertEquals(nameof(registerHeader.SRH_PreviousReference), movementReferenceNumber, registerHeader.SRH_PreviousReference);
			AssertEquals(nameof(registerHeader.SRH_PresentationDate), presentationDate, registerHeader.SRH_PresentationDate);
			AssertEquals(nameof(registerHeader.SRH_TransportID), transportID, registerHeader.SRH_TransportID);
			AssertEquals(nameof(registerHeader.SRH_CustomsOffice), customsOffice, registerHeader.SRH_CustomsOffice);
		});
	}

	public static void AssertRegisterLine(
		string message,
		CusTempStorageRegLine line,
		ZString houseBill,
		ZInt lineNumber,
		ZDate limitDate,
		ZString locationOfGoods,
		ZString ownerReference,
		ZString ownerReferenceType,
		ZString goodsDescription,
		ZString packageType,
		ZString grossWeightUQ,
		ZString[] containers)
	{
		CombineAssertions(message, () =>
		{
			AssertEquals(nameof(line.RegLineItemPivots), 1, line.RegLineItemPivots.Count);

			var item = line.RegLineItemPivots[0];
			AssertNotNull(nameof(item.RegLineItem), item);
			AssertEquals(nameof(item.RegLineItem.SRI_HouseBill), houseBill, item.RegLineItem.SRI_HouseBill);

			AssertEquals(nameof(line.SRL_LineNumber), lineNumber, line.SRL_LineNumber);
			AssertEquals(nameof(line.SRL_LimitDate), limitDate, line.SRL_LimitDate);
			AssertEquals(nameof(line.SRL_LocationOfGoods), locationOfGoods, line.SRL_LocationOfGoods);
			AssertEquals(nameof(line.SRL_OwnerReference), ownerReference, line.SRL_OwnerReference);
			AssertEquals(nameof(line.SRL_OwnerReferenceType), ownerReferenceType, line.SRL_OwnerReferenceType);
			AssertEquals(nameof(line.SRL_GoodsDescription), goodsDescription, line.SRL_GoodsDescription);
			AssertEquals(nameof(line.SRL_PackageType), packageType, line.SRL_PackageType);
			AssertEquals(nameof(line.SRL_GrossWeightUQ), grossWeightUQ, line.SRL_GrossWeightUQ);

			AssertContainsExactElementsInExactOrder(nameof(line.Containers), containers, line.Containers.Select(x => x.CY_Data));
		});
	}

	public static void AssertRegisterLineTransaction(
		string message,
		CusTempStorageRegLineTransaction transaction,
		ZString type,
		ZDecimal grossWeight,
		ZInt packageQty,
		ZString referenceType,
		ZString reference)
	{
		CombineAssertions(message, () =>
		{
			AssertEquals(nameof(transaction.SRT_TransactionType), type, transaction.SRT_TransactionType);
			AssertEquals(nameof(transaction.SRT_GrossWeight), grossWeight, transaction.SRT_GrossWeight);
			AssertEquals(nameof(transaction.SRT_PackageQty), packageQty, transaction.SRT_PackageQty);
			AssertEquals(nameof(transaction.SRT_ReferenceType), referenceType, transaction.SRT_ReferenceType);
			AssertEquals(nameof(transaction.SRT_Reference), reference, transaction.SRT_Reference);
		});
	}
}
