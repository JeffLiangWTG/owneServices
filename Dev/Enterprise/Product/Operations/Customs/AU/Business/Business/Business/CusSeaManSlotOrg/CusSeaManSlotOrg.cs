using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManSlotOrg : Customs.Business.CusSeaManSlotOrg
	{
		public CusSeaManSlotOrg(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : Customs.Business.AutoCusSeaManSlotOrg.Schema
		{
			public const string ABNOrCCID = "ABNOrCCID";
		}

		#endregion

		#region Proxied Properties

		#region ABNOrCCID

		public ZString ABNOrCCID
		{
			get
			{
				ZString result = ZString.Empty;

				if (SlotCharterer != null)
				{
					result = !SlotCharterer.LocalBusinessRegNo.IsEmpty ? SlotCharterer.LocalBusinessRegNo : SlotCharterer.LocalCustomsClientCode;
				}

				return result;
			}
		}

		public ZPropertyInfo ABNOrCCIDInfo
		{
			get { return GetZPropertyInfo(Schema.ABNOrCCID); }
		}

		#endregion

		#region SlotChartererWrapper

		public OrgHeaderWrapper SlotChartererWrapper
		{
			get
			{
				OrgHeaderWrapper result = null;

				if (SlotCharterer != null)
				{
					result = new OrgHeaderWrapper(SlotCharterer);
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Overrides

		public new CusSeaManTranHead Header
		{
			get { return (CusSeaManTranHead)base.Header; }
		}

		protected override Customs.Business.CusSeaManSlotOrgValidation GetNewValidation()
		{
			return new CusSeaManSlotOrgValidation(this);
		}

		public new CusSeaManSlotOrgValidation Validation
		{
			get { return (CusSeaManSlotOrgValidation)base.Validation; }
		}

		#endregion
	}
}
