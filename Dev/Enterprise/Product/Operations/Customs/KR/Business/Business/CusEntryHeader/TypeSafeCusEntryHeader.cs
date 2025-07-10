using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.BusinessObjects.Interfaces;

namespace Enterprise.Customs.KR.Business
{
	public partial class CusEntryHeader
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;

		public new CusEntryHeader Clone() => (CusEntryHeader)base.Clone();

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new JobComInvoiceHeader[] InvoiceHeaders() => (JobComInvoiceHeader[])base.InvoiceHeaders;

		public new Customs.Business.ICusEntryLineCollection<CusEntryLine> MergedLines => (Customs.Business.CusEntryLineCollection<CusEntryLine>)base.MergedLines;

		[ChildEditable(true)]
		public new Customs.Business.IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (Customs.Business.IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

		public new EDIMessageCollection Messages => (EDIMessageCollection)base.Messages;

		public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

		//For PID, it returns a validation class which inherits from Customs.Business.CusEntryHeaderValidation
		//public new CusEntryHeaderValidation Validation => (CusEntryHeaderValidation)base.Validation;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		#endregion

		#region Implementation

		protected override Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new Customs.Business.CusEntryLineCollection<CusEntryLine>(this);

		protected override Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new Customs.Business.AllCusEntryLineCollection<CusEntryLine>(this);

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);

		protected override Customs.Business.CusEntryHeaderValidation GetNewValidation()
		{
			Customs.Business.CusEntryHeaderValidation result = null;
			if (IsMisc)
			{
				result = new MiscCusEntryHeaderValidation(this);
			}
			else
			{
				result = new CusEntryHeaderValidation(this);
			}
			return result;
		}

		protected override Enterprise.Messaging.Business.EDIMessageCollection GetNewMessageCollection() => new EDIMessageCollection(this);

		public new Customs.Business.CusEntrySnapshotCollection<CusEntrySnapshot> Snapshots => (Customs.Business.CusEntrySnapshotCollection<CusEntrySnapshot>)base.Snapshots;

		protected override ICusEntrySnapshotCollection<Customs.Business.CusEntrySnapshot> CreateNewEntrySnapshotsCollection() => new Customs.Business.CusEntrySnapshotCollection<CusEntrySnapshot>(this);
		#endregion
	}
}
