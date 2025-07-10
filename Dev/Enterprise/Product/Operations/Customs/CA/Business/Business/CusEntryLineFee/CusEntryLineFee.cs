using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public partial class CusEntryLineFee : AutoCusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool ShouldResetDataOnMergingCore
		{
			get { return false; }
		}
	}
}
