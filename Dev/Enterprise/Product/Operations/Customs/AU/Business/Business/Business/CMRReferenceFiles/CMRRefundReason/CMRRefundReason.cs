
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRRefundReason : AutoCMRRefundReason, ICodeDescription
	{
		public CMRRefundReason(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRRefundReason New(BusinessObjectFactory factory)
		{
			return factory.New<CMRRefundReason>();
		}

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		public string Description
		{
			get { return CR_RefundReasonDescription; }
		}

		public string Code
		{
			get { return CR_RefundReasonType; }
		}

		#endregion
	}
}
