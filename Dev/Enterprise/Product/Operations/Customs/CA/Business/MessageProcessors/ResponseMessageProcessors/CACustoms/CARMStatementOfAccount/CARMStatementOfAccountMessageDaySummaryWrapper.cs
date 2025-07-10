using System.Collections.Generic;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMSOA;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class CARMStatementOfAccountMessageDaySummaryWrapper : ITableInterpretation
	{
		public CARMStatementOfAccountMessageDaySummaryWrapper(ZString account, ZcarmsoaDetailsProgramAccountDaySummary daySummary)
		{
			this.account = account;
			this.daySummary = daySummary;
		}

		readonly ZString account;
		readonly ZcarmsoaDetailsProgramAccountDaySummary daySummary;

		#region Properties

		[ColumnName(0, "ID")]
		public ZString ID => daySummary.Id;

		[ColumnName(1, "Release Date")]
		public ZDate ReleaseDate => daySummary.ReleaseDate.HasValue ? new ZDate(daySummary.ReleaseDate.Value) : ZDate.Empty;

		[ColumnName(2, "Accounting Date")]
		public ZDateTime AccountingDate => daySummary.AccountingDate;

		[ColumnName(3, "Duties")]
		public ZDecimal Duties => daySummary.Lineitem.Duties;

		[ColumnName(4, "Excise")]
		public ZDecimal Excise => daySummary.Lineitem.Excise;

		[ColumnName(5, "Excise Duties")]
		public ZDecimal ExciseDuties => daySummary.Lineitem.Exciseduties;

		[ColumnName(6, "SIMA")]
		public ZDecimal SIMA => daySummary.Lineitem.Sima;

		[ColumnName(7, "GST")]
		public ZDecimal GST => daySummary.Lineitem.Gst;

		[ColumnName(8, "HST")]
		public ZDecimal HST => daySummary.Lineitem.Hst;

		[ColumnName(9, "PST")]
		public ZDecimal PST => daySummary.Lineitem.Pst;

		[ColumnName(10, "Interest")]
		public ZDecimal Interest => daySummary.Lineitem.Interest;

		[ColumnName(11, "Penalties")]
		public ZDecimal Penalties => daySummary.Lineitem.Penalties;

		[ColumnName(12, "Payments")]
		public ZDecimal Payments => daySummary.Lineitem.Payments;

		[ColumnName(13, "Others")]
		public ZDecimal Others => daySummary.Lineitem.Others;

		[ColumnName(14, "Totals")]
		public ZDecimal Totals => daySummary.Lineitem.Totals;

		[ColumnName(15, "Payment Due Date")]
		public ZDateTime? PaymentDueDate => daySummary.Lineitem.PaymentDueDate.HasValue ? new ZDate(daySummary.Lineitem.PaymentDueDate) : ZDate.Empty;

		#endregion

		#region ITableInterpretation
		public string Caption => account;

		public IEnumerable<string> Titles => PropertyNameProvider.GetColumnTitles<CARMStatementOfAccountMessageDaySummaryWrapper>();

		public IEnumerable<object> Values
		{
			get
			{
				yield return ID;
				yield return ReleaseDate;
				yield return AccountingDate;
				yield return InterpretationHelper.FormatAmount(Duties);
				yield return InterpretationHelper.FormatAmount(Excise);
				yield return InterpretationHelper.FormatAmount(ExciseDuties);
				yield return InterpretationHelper.FormatAmount(SIMA);
				yield return InterpretationHelper.FormatAmount(GST);
				yield return InterpretationHelper.FormatAmount(HST);
				yield return InterpretationHelper.FormatAmount(PST);
				yield return InterpretationHelper.FormatAmount(Interest);
				yield return InterpretationHelper.FormatAmount(Penalties);
				yield return InterpretationHelper.FormatAmount(Payments);
				yield return InterpretationHelper.FormatAmount(Others);
				yield return InterpretationHelper.FormatAmount(Totals);
				yield return PaymentDueDate;
			}
		}

		#endregion
	}
}
