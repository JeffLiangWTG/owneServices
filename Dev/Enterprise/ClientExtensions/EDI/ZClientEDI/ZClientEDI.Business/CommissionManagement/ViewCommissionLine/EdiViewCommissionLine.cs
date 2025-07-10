using System.Data;
using CargoWise.EntityFramework;
using Enterprise.CommissionManagement.Business;

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public class EdiViewCommissionLine : ViewCommissionLine
	{
		public EdiViewCommissionLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new EdiCommissionHeader CommissionHeader
		{
			get { return (EdiCommissionHeader)base.CommissionHeader; }
		}
	}
}

