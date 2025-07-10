using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class CACRateLine : AutoCACRateLine
	{
		public CACRateLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ZR_ZC_Rate

		[RelatedBusinessObject("Rate")]
		public override ZGuid ZR_ZC_Rate
		{
			get { return base.ZR_ZC_Rate; }
			set { base.ZR_ZC_Rate = value; }
		}

		public CACRate Rate
		{
			get { return Factory.Load<CACRate>(ZR_ZC_Rate); }
		}

		#endregion
	}
}
