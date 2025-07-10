using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class CACTaxRefNumber : AutoCACTaxRefNumber
	{
		public CACTaxRefNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ZE_ZD_TaxRefNumHeader

		[RelatedBusinessObject("Header")]
		public override ZGuid ZE_ZD_TaxRefNumHeader
		{
			get { return base.ZE_ZD_TaxRefNumHeader; }
			set { base.ZE_ZD_TaxRefNumHeader = value; }
		}

		public CACTaxRefNumHeader Header
		{
			get { return Factory.Load<CACTaxRefNumHeader>(ZE_ZD_TaxRefNumHeader); }
		}

		#endregion
	}
}
