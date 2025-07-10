using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business
{
	public class CusPermitHeader : EU.Business.CusPermitHeader, Integration.Customs.GB.ICusPermitHeader
	{
		public CusPermitHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			CPH_StartDate = ZDate.Today;
			CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;
		}

		public CusPermitEnquiryMessageCollection CusPermitHeaderQueries
		{
			get
			{
				if (cusPermitHeaderQueries == null)
				{
					cusPermitHeaderQueries = CreateNewCusPermitHeaderQueryCollection();
					cusPermitHeaderQueries.Load();
					cusPermitHeaderQueries.Sort(EDIMessageSchema.Constants.EM_SystemCreateTimeUtc, System.ComponentModel.ListSortDirection.Descending);
				}

				return cusPermitHeaderQueries;
			}
		}
		CusPermitEnquiryMessageCollection cusPermitHeaderQueries;

		protected virtual CusPermitEnquiryMessageCollection CreateNewCusPermitHeaderQueryCollection()
		{
			return new CusPermitEnquiryMessageCollection(this);
		}

		public void RefreshQueries() => CusPermitHeaderQueries.Load();
	}
}
