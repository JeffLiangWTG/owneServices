using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public partial class CusEntryHeader
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobComInvoiceHeader RandomHeader
		{
			get { return (JobComInvoiceHeader)base.RandomHeader; }
		}

		public new CusEntryHeader Clone()
		{
			return (CusEntryHeader)base.Clone();
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		public new JobComInvoiceHeader[] InvoiceHeaders
		{
			get { return (JobComInvoiceHeader[])base.InvoiceHeaders; }
		}

		public new Customs.Business.ICusEntryLineCollection<CusEntryLine> MergedLines
		{
			get { return (Customs.Business.CusEntryLineCollection<CusEntryLine>)base.MergedLines; }
		}

		public new EDIMessageCollection Messages
		{
			get { return (EDIMessageCollection)base.Messages; }
		}

		public new Customs.Business.IAllCusEntryLineCollection<CusEntryLine> AllEntryLines
		{
			get { return (AllCusEntryLineCollection)base.AllEntryLines; }
		}

		BusinessObjectCollection Integration.Customs.CA.ICACusEntryHeader.AllEntryLines => (BusinessObjectCollection)AllEntryLines;

		public new CusEntryHeaderLookups Lookups
		{
			get { return (CusEntryHeaderLookups)base.Lookups; }
		}

		public new CusEntryHeaderValidation Validation
		{
			get { return (CusEntryHeaderValidation)base.Validation; }
		}

		#endregion

		#region AddInfo/Validation/Lookups objects

		protected AddInfoCusEntryHeader AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new AddInfoCusEntryHeader(CH_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		AddInfoCusEntryHeader fAddInfo;

		public AddInfoCusEntryHeaderLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public AddInfoCusEntryHeaderValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}
		#endregion

		#region Implementation

		protected override Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection()
		{
			return new Customs.Business.CusEntryLineCollection<CusEntryLine>(this);
		}

		protected override Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection()
		{
			return new AllCusEntryLineCollection(this);
		}

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups()
		{
			return new CusEntryHeaderLookups(this);
		}

		protected override Customs.Business.CusEntryHeaderValidation GetNewValidation()
		{
			return new CusEntryHeaderValidation(this);
		}

		protected override Enterprise.Messaging.Business.EDIMessageCollection GetNewMessageCollection()
		{
			return new EDIMessageCollection(this);
		}

		#endregion
	}
}
