using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU;
using CusEntryLineFee = Enterprise.Customs.IT.Business.Declaration.CusEntryLineFee;
using ITCusEntryLine = Enterprise.Customs.IT.Business.Declaration.CusEntryLine;
using ITCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.IT.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.IT.Business;

[AllowNoStaticNew]
public class ITDocSADHPage : DocSADHPage
{
	public static ITDocSADHPage New(BusinessObjectFactory factory, ITCusEntryLine entryLine1)
	{
		return new ITDocSADHPage(factory, ITDocSADHLine.New(entryLine1, factory), null, null, null);
	}

	public static ITDocSADHPage New(BusinessObjectFactory factory, ITCusEntryLineCollection entryLines, int startFrom)
	{
		return new ITDocSADHPage(factory
			, GetElementSafe(entryLines, startFrom, factory)
			, GetElementSafe(entryLines, startFrom + 1, factory)
			, GetElementSafe(entryLines, startFrom + 2, factory)
			, entryLines
		);
	}

	ITDocSADHPage(BusinessObjectFactory factory, ITDocSADHLine line1, ITDocSADHLine line2, ITDocSADHLine line3, ITCusEntryLineCollection entryLines)
		: base(factory, line1, line2, line3)
	{
		this.entryLines = entryLines;
	}

	public new ITDocSADHLine Line1 => (ITDocSADHLine)base.Line1;
	public new ITDocSADHLine Line2 => (ITDocSADHLine)base.Line2;
	public new ITDocSADHLine Line3 => (ITDocSADHLine)base.Line3;

	protected override ZString BISCaptionCore => "BIS";

	protected override DocSADHLineTaxCollection Box47TaxesTotalsCore => GetBox47TaxesTotals();

	protected override ZString Box47TotalAmountCore => GetBox47TotalAmountCore();

	ZString GetBox47TotalAmountCore()
	{
		var result = ZString.Empty;
		if (entryLines != null && IsLastPage(entryLines))
		{
			var totalAmount = entryLines
				.SelectMany(el => el.Fees).Cast<CusEntryLineFee>()
				.Where(fee => !ITDocSADHLineTaxHelper.ExcludeDutiesInTotals(fee.CF_MethodOfPayment))
				.Sum(fee => fee.CF_ChargeAmount);
			result = totalAmount.ToString(DecimalsFormat);
		}
		return result;
	}

	DocSADHLineTaxCollection GetBox47TaxesTotals()
	{
		var factory = Factory;
		if (entryLines == null || !IsLastPage(entryLines))
		{
			return new DocSADHLineTaxCollection(new List<NonPersistentFee>().Cast<IDocSADHLineTaxBoxSupporter>(), factory);
		}

		var sortedFeeList = entryLines
			.SelectMany(el => el.Fees).Cast<CusEntryLineFee>()
			.Where(fee => !ITDocSADHLineTaxHelper.ExcludeDutiesInTotals(fee.CF_MethodOfPayment))
			.OrderBy((CusEntryLineFee x) => x, new CusEntryLineFeeComparer())
			.ThenBy(x => x.CF_MethodOfPayment);

		var feeGrantTotalDictionary = new Dictionary<Tuple<string, string>, NonPersistentFee>();
		foreach (var fee in sortedFeeList)
		{
			var keyOfGrantTotal = new Tuple<string, string>(fee.G4_Type, fee.G4_MethodOfPayment);
			if (!feeGrantTotalDictionary.ContainsKey(keyOfGrantTotal))
			{
				feeGrantTotalDictionary.Add(keyOfGrantTotal, new NonPersistentFee(fee, factory));
			}
			else
			{
				feeGrantTotalDictionary[keyOfGrantTotal].AddAmountToTax(fee.CF_ChargeAmount);
			}
		}

		return new DocSADHLineTaxCollection(feeGrantTotalDictionary.Values.Cast<IDocSADHLineTaxBoxSupporter>(), factory);
	}

	static ITDocSADHLine GetElementSafe(ITCusEntryLineCollection entryLines, int index, BusinessObjectFactory factory)
	{
		var entryLine = index < entryLines.Count ? entryLines[index] : null;
		return ITDocSADHLine.New(entryLine, factory);
	}

	ZBool IsLastPage(ITCusEntryLineCollection entryLines) => Line2 == null || Line3 == null || Line3.EntryLine.PK == entryLines.Last().PK;

	readonly ITCusEntryLineCollection entryLines;

	const string DecimalsFormat = "0.00";
}
