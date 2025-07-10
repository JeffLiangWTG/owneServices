using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageDefinitions;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business;

sealed class A93NumbersManager
{
	public A93NumbersManager(IXmlCustomsLinkedObjectAdapter customsLinkedObjectAdapter)
	{
		this.customsLinkedObjectAdapter = Argument.NotNull(customsLinkedObjectAdapter, nameof(customsLinkedObjectAdapter));
	}

	public void InvalidateNumbers()
	{
		var entryPayInfos = customsLinkedObjectAdapter.GetAllPaymentInfo();
		var entriesToInvalidate = GetEntryPayInfosToInvalidate(entryPayInfos);
		InvalidatePayInfoEntries(entriesToInvalidate);
	}

	public void AddNumbers(ResponseMessageDetailsReadOnlyCollection<IA93NumberInformation> a93Numbers)
	{
		Argument.NotNull(a93Numbers, nameof(a93Numbers));
		if (!a93Numbers.Any())
		{
			return;
		}

		var a93NumberForHeader = a93Numbers.HeaderItem;
		if (a93NumberForHeader != null)
		{
			var fees = customsLinkedObjectAdapter.GetAllFees(null);
			InsertA93Numbers(fees, a93NumberForHeader, true);
			return;
		}

		InsertA93NumbersForLines(a93Numbers);
	}

	#region Implementation

	void InsertA93NumbersForLines(IReadOnlyCollection<IA93NumberInformation> a93Numbers)
	{
		foreach (var a93NumberItemWrapper in a93Numbers)
		{
			var fees = customsLinkedObjectAdapter.GetFeesForLine(a93NumberItemWrapper.ItemNumber.GetValueOrDefault(0), null);
			InsertA93Numbers(fees, a93NumberItemWrapper, false);
		}
	}

	void InsertA93Numbers(IReadOnlyCollection<IFee> fees, IA93NumberInformation a93NumberItem, bool isForHeader)
	{
		var paymentResponseNo = isForHeader
					? a93NumberItem.ReferenceNumber
					: GeneratePaymentResponseNo(a93NumberItem);

		var paymentInfoList = a93NumberItem.GetPaymentInformationItems();

		foreach (var paymentInfo in paymentInfoList)
		{
			var applicableFees = fees.Where(f => string.Equals(f.MethodOfPayment, paymentInfo.PaymentType, StringComparison.OrdinalIgnoreCase)).ToArray();
			var payment = new Ucc6A93NumberPayment(paymentType: paymentInfo.PaymentType,
				paymentDate: paymentInfo.PaymentDate,
				paymentResponseNo: paymentResponseNo,
				applicableFees);
			customsLinkedObjectAdapter.AddA93Number(payment);
		}
	}

	string GeneratePaymentResponseNo(IA93NumberInformation a93NumberItemWrapper)
	{
		return FormattableString.Invariant($"{a93NumberItemWrapper.ReferenceNumber}-{a93NumberItemWrapper.ItemNumber}");
	}

	void InvalidatePayInfoEntries(IReadOnlyCollection<CusEntryPayInfo> entriesToInvalidate)
	{
		foreach (var cusEntryPayInfo in entriesToInvalidate)
		{
			var copiedCusEntryPayInfo = CreateCopy(cusEntryPayInfo);
			InvalidateAmount(copiedCusEntryPayInfo);
		}
	}

	IReadOnlyCollection<CusEntryPayInfo> GetEntryPayInfosToInvalidate(IReadOnlyCollection<CusEntryPayInfo> entryPayInfos)
	{
		var cusEntryPayInfosToInvalidate = entryPayInfos
			.Where(e => !string.IsNullOrWhiteSpace(e.A93Number))
			.Select(e => new { Key = GenerateA93NumberKey(e), Entry = e })
			.GroupBy(e => e.Key)
			.Where(e => e.Count() == 1)
			.Select(e => e.Select(i => i.Entry))
			.SelectMany(e => e)
			.Where(e => e.C9_PaymentAmount > 0)
			.ToCollection();

		return cusEntryPayInfosToInvalidate;
	}

	string GenerateA93NumberKey(CusEntryPayInfo cusEntryPayInfo)
	{
		return FormattableString.Invariant(
			$"{cusEntryPayInfo.A93Number}+{cusEntryPayInfo.C9_PaymentDate:yyyyMMdd}+{cusEntryPayInfo.C9_PaymentParty}+{cusEntryPayInfo.C9_PaymentStatus}");
	}

	void InvalidateAmount(CusEntryPayInfo cusEntryPayInfo)
	{
		cusEntryPayInfo.C9_PaymentAmount *= -1;
	}

	CusEntryPayInfo CreateCopy(CusEntryPayInfo sourceCusEntryPayInfo)
	{
		var cusEntryPayInfo = customsLinkedObjectAdapter.Factory.New<CusEntryPayInfo>();
		cusEntryPayInfo.C9_IncomingPayResponseNo = sourceCusEntryPayInfo.C9_IncomingPayResponseNo;
		cusEntryPayInfo.C9_PaymentAmount = sourceCusEntryPayInfo.C9_PaymentAmount;
		cusEntryPayInfo.C9_PaymentDate = sourceCusEntryPayInfo.C9_PaymentDate;
		cusEntryPayInfo.C9_PaymentParty = sourceCusEntryPayInfo.C9_PaymentParty;
		cusEntryPayInfo.C9_PaymentStatus = sourceCusEntryPayInfo.C9_PaymentStatus;
		cusEntryPayInfo.C9_PaymentReference = sourceCusEntryPayInfo.C9_PaymentReference;
		cusEntryPayInfo.C9_CH = sourceCusEntryPayInfo.C9_CH;
		cusEntryPayInfo.C9_TransactionType = sourceCusEntryPayInfo.C9_TransactionType;
		cusEntryPayInfo.C9_ClusterKey = sourceCusEntryPayInfo.C9_ClusterKey;
		return cusEntryPayInfo;
	}

	#endregion

	readonly IXmlCustomsLinkedObjectAdapter customsLinkedObjectAdapter;
}
