using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MY.Business
{
	public class CusEntryHeader : TypeSafeCusEntryHeader, Integration.Customs.MY.ICusEntryHeader
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool HasBeenWithdrawn
		{
			get { return false; } // Not currently use
		}

		protected override Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection()
		{
			return new Customs.Business.CusEntryLineCollection<CusEntryLine>(this);
		}

		protected override Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new Customs.Business.AllCusEntryLineCollection<CusEntryLine>(this);

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups()
		{
			return new CusEntryHeaderLookups(this);
		}

		protected override Customs.Business.CusEntryHeaderValidation GetNewValidation()
		{
			return new CusEntryHeaderValidation(this);
		}
	}
}
