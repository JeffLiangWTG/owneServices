using System.Collections.Generic;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARM_TYPES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class CARMDailyNoticeMessageLineWrapper : ITableInterpretation
	{
		public CARMDailyNoticeMessageLineWrapper(BusinessObjectFactory factory, DailyNoticeLineItemTypeLineitem lineItem)
		{
			this.factory = factory;
			this.lineItem = lineItem;
		}

		readonly BusinessObjectFactory factory;
		readonly DailyNoticeLineItemTypeLineitem lineItem;

		#region Properties

		public ZString TransactionNumber => lineItem.AtnNum;

		[ColumnName(0, "Doc Type")]
		public ZString DocType => lineItem.DocType;

		[ColumnName(1, "Release Date")]
		public ZDate ReleaseDate => lineItem.RelDate.HasValue ? new ZDate(lineItem.RelDate.Value) : ZDate.Empty;

		[ColumnName(2, "Accounting Date")]
		public ZDateTime AccountingDate => lineItem.AccDate;

		[ColumnName(3, "Port")]
		public ZString Port => lineItem.Port;

		[ColumnName(4, "Duties")]
		public ZDecimal Duties => lineItem.Amounts.Duties;

		[ColumnName(5, "SIMA")]
		public ZDecimal SIMA => lineItem.Amounts.Sima;

		[ColumnName(6, "Excise Tax")]
		public ZDecimal ExciseTax => lineItem.Amounts.Excise;

		public ZString TransactionDescription => lineItem.TransDesc;

		public ZString CADVERSION => lineItem.CadVersion;

		public ZString SubmittedBy => lineItem.SubBy;

		public DailyNoticeLineItemTypeLineitemStatus? AccountingStatus => lineItem.Status;

		[ColumnName(7, "Excise Duties")]
		public ZDecimal ExciseDuties => lineItem.Amounts.Exciseduties;

		[ColumnName(8, "GST/PST/HST")]
		public ZDecimal GSTAndPSTAndHST => lineItem.Amounts.Gst + lineItem.Amounts.Pst + lineItem.Amounts.Hst;

		public ZDecimal GST => lineItem.Amounts.Gst;

		public ZDecimal PST => lineItem.Amounts.Pst;

		public ZDecimal HST => lineItem.Amounts.Hst;

		[ColumnName(8, "Interest")]
		public ZDecimal Interest => lineItem.Amounts.Interest;

		public ZDecimal Penalties => lineItem.Amounts.Penalties;

		public ZDecimal Payments => lineItem.Amounts.Payments;

		[ColumnName(9, "Others")]
		public ZDecimal Others => lineItem.Amounts.Others;

		[ColumnName(10, "Totals")]
		public ZDecimal Totals => lineItem.Amounts.Totals;

		[ColumnName(11, "Transaction Number")]
		public ZString ReleatedDocumentNumber => lineItem.AtnNum;

		public JobDeclaration ReleatedDeclaration
		{
			get
			{
				if (releatedDeclarationCached == null)
				{
					releatedDeclarationCached = new CachedProperty<JobDeclaration>(factory, () =>
					{
						var docType = DocType == JobMessageTypeList.Codes.B2Adjustments ? JobMessageTypeList.Codes.B2Adjustments : string.Empty;
						return ImportLinkedObjectManager.LoadDeclarationWithTransactionNumber(factory, TransactionNumber, docType);
					});
				}

				return releatedDeclarationCached.Value;
			}
		}
		CachedProperty<JobDeclaration> releatedDeclarationCached;

		[ColumnName(12, "Job Number")]
		public ZString JobNumber => ReleatedDeclaration?.JE_DeclarationReference ?? ZString.Empty;

		public ZDateTime? PaymentDueDate => lineItem.Amounts.PaymentDueDate;

		#endregion

		#region ITableInterpretation
		public string Caption => Res.GetString("87149AE1-65AA-4343-822F-8207CE0AB6F1", "Transactions");

		public IEnumerable<string> Titles => PropertyNameProvider.GetColumnTitles<CARMDailyNoticeMessageLineWrapper>();

		public IEnumerable<object> Values
		{
			get
			{
				return new object[]
						{
							DocType,
							InterpretationHelper.FormatDate(ReleaseDate),
							InterpretationHelper.FormatDate(AccountingDate),
							Port,
							InterpretationHelper.FormatAmount(Duties, true),
							InterpretationHelper.FormatAmount(SIMA, true),
							InterpretationHelper.FormatAmount(ExciseTax, true),
							InterpretationHelper.FormatAmount(ExciseDuties, true),
							InterpretationHelper.FormatAmount(GSTAndPSTAndHST, true),
							InterpretationHelper.FormatAmount(Interest, true),
							InterpretationHelper.FormatAmount(Others, true),
							InterpretationHelper.FormatAmount(Totals, true),
							ReleatedDocumentNumber,
							JobNumber
						};
			}
		}

		#endregion
	}
}
