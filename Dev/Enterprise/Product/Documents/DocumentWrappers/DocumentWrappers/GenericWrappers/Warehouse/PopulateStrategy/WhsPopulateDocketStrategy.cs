using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public abstract class WhsPopulateDocketStrategy : WhsPopulateStrategy
	{
		#region Constructor

		protected WhsPopulateDocketStrategy(BusinessObject docket)
			: base(docket)
		{
		}

		#endregion

		#region Properties

		#region JobNumber

		public override ZString JobNumber
		{
			get { return DocketBO.WD_DocketID; }
		}

		#endregion

		#region JobNumberHeading

		public override ZString JobNumberHeading
		{
			get { return Res.GetString("529ccbbd-2194-4fa2-964f-e09c5b7f85ad", "Job Number"); }
		}

		#endregion

		#region SecondaryHeading

		public override ZString SecondaryHeading
		{
			get
			{
				ZString description = DocketBO.SubTypeDesc;
				if (description.IsEmpty)
				{
					description = DocketBO.Description;
				}
				return (Res.GetString("3e332dc3-76ad-4a90-86dd-6a9f5db24c1e", "{0} Details", description)).Trim();
			}
		}

		#endregion

		#region ServiceLevel

		public override CodeAndDescriptionWrapper ServiceLevel
		{
			get { return new CodeAndDescriptionWrapper(DocketBO.WD_PL_NKCarrierServiceLevel, DocketBO.Lookups.CarrierServiceLevels, Factory); }
		}

		#endregion

		#region TransportReference

		public override LabelValuePairWrapper TransportReference
		{
			get { return new LabelValuePairWrapper(Res.GetString("875aca4c-5ce9-4cf5-b1d6-0c55d75acada", "Transport Ref"), DocketBO.WD_TransportReference, Factory); }
		}
		#endregion

		#endregion

		#region Implementation

		WhsDocket DocketBO
		{
			get { return docketBO ?? (docketBO = (WhsDocket)WrappedBO); }
		}
		WhsDocket docketBO;

		#endregion
	}
}
