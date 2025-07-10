using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.MY.Business
{
	public class CusEntryLine : TypeSafeCusEntryLine, Integration.Customs.MY.ICusEntryLine
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		#region protected override

		protected override System.Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

		protected override ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection()
		{
			return new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(this, Factory);
		}

		protected override Customs.Business.CusEntryLineLookups GetNewLookups()
		{
			return new CusEntryLineLookups(this);
		}

		protected override Customs.Business.CusEntryLineValidation GetNewValidation()
		{
			return new CusEntryLineValidation(this);
		}

		#endregion

		#endregion
	}
}
