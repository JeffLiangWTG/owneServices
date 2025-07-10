using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class TypeSafeCusEntryLine : AutoCusEntryLine
	{
		protected TypeSafeCusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		[ChildEditable]
		public new IEUCusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> Fees
		{
			get
			{
				var fees = (IEUCusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)base.Fees;
				RegisterEditableChildObject(fees);
				return fees;
			}
		}

		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		public new JobComInvoiceLine RandomLine => base.RandomLine as JobComInvoiceLine;

		public new CusEntryLineLookups Lookups => (CusEntryLineLookups)base.Lookups;

		public new CusEntryLineValidation Validation => (CusEntryLineValidation)base.Validation;

		#region Implementation

		protected override System.Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

		CusEntryLine EntryLine => (CusEntryLine)this;

		protected override ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection()
		{
			return new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(EntryLine, Factory);
		}

		protected override Customs.Business.CusEntryLineLookups GetNewLookups() => new CusEntryLineLookups(EntryLine);

		protected override Customs.Business.CusEntryLineValidation GetNewValidation() => new CusEntryLineValidation(EntryLine);

		public override void Delete()
		{
			Fees.VatRefresher.Unhook();
			base.Delete();
		}

		#endregion
	}
}
