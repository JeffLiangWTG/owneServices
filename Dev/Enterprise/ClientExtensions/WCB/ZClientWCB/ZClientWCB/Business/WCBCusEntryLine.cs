using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.WCB
{
	public class WCBCusEntryLine : CusEntryLine
	{
		public WCBCusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString DescriptionInternal
		{
			get
			{
				ZString attribute1 = RandomLine.JI_PartAttrib1;
				return (attribute1.IsEmpty) ? base.DescriptionInternal : new ZString(attribute1 + " " + base.DescriptionInternal);
			}
		}
	}
}
