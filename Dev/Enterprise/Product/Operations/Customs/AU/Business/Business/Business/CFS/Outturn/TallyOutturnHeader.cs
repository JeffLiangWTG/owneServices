using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TallyOutturnHeader : CusOutturnHeader
	{
		public TallyOutturnHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Customs.Business.CusOutturnHeaderCusOutturnCollection GetNewOutturns()
		{
			return new TallyOutturnHeaderOutturnCollection(this);
		}

		[ChildEditable(true)]
		public new TallyOutturnHeaderOutturnCollection Outturns
		{
			get { return (TallyOutturnHeaderOutturnCollection)base.Outturns; }
		}
	}
}
