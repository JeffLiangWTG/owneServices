using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public partial class CusEntryHeader : Customs.Business.CusEntryHeader, Integration.Customs.CN.ICusEntryHeader
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;

		public new CusEntryHeader Clone() => (CusEntryHeader)base.Clone();

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new JobComInvoiceHeader[] InvoiceHeaders => (JobComInvoiceHeader[])base.InvoiceHeaders;

		public new Customs.Business.ICusEntryLineCollection<CusEntryLine> MergedLines => (Customs.Business.CusEntryLineCollection<CusEntryLine>)base.MergedLines;

		[ChildEditable(true)]
		public new Customs.Business.IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (Customs.Business.IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

		public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

		[ChildEditable(true)]
		public new Customs.Business.ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> Charges => (Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>)base.Charges;

		public new Customs.Business.CusEntryHeaderValidation Validation => base.Validation;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		protected override Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new Customs.Business.CusEntryLineCollection<CusEntryLine>(this);

		protected override Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new Customs.Business.AllCusEntryLineCollection<CusEntryLine>(this);

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);

		protected override Customs.Business.ICusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

		protected override Customs.Business.CusEntryHeaderValidation GetNewValidation() => new Customs.Business.CusEntryHeaderValidation(this);

		protected override CusEntryNumber LoadCusEntryNumber()
		{
			var result = LoadCusEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber);
			if (result == null)
			{
				var declaration = Declaration;
				var entryType = declaration == null ? Customs.Business.JobMessageTypeList.Codes.Import : (string)declaration.JE_MessageType;
				result = LoadCusEntryNumber(entryType);
			}
			return result;
		}

		CusEntryNumber LoadCusEntryNumber(ZString entryType)
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, CountryCode);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			query.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;
			return Factory.LoadTop1<CusEntryNumber>(query);
		}

		protected override ZString EntryNumberType => CusEntryNumberTypes.Standard.MovementReferenceNumber;
	}
}
