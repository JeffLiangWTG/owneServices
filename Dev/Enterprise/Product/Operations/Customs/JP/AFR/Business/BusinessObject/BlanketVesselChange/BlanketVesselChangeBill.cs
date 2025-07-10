using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class BlanketVesselChangeBill : AutoBlanketVesselChangeBill
	{
		public BlanketVesselChangeBill(JPAFRBills bill, BlanketVesselChange blanketVesselChange) : base(bill.Factory)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			BlanketVesselChange = Argument.NotNull(blanketVesselChange, nameof(blanketVesselChange));
			base.JPM_BillOfLadingNumber = this.bill.JPB_BillNumber;
			base.JPM_ReleaseStatus = this.bill.JPB_ReleaseStatus;
			base.JPM_MessageStatus = this.bill.JPB_MessageStatus;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.JPM_Send = ZBool.True;
		}

		public override ZBool JPM_Send
		{
			get { return base.JPM_Send; }
			set
			{
				base.JPM_Send = value;
				if (BlanketVesselChange != null)
				{
					var vesselChangeBills = BlanketVesselChange.BlanketVesselChangeBills.Cast<BlanketVesselChangeBill>().ToArray();
					BlanketVesselChange.SetJPM_AreAllBillsCheckedInternally(vesselChangeBills.All(x => x.JPM_Send));
					vesselChangeBills.Where(x => x.PK != PK).ForEach(x => x.Validation.ValidateJPM_Send());
				}
			}
		}

		protected override ZString GetJPM_ReleaseStatusDescription()
		{
			return Factory.GetCachedValue<AFRBillCustomsStatusList>().GetDescriptionFromCode(JPM_ReleaseStatus) ?? ZString.Empty;
		}

		protected override ZString GetJPM_MessageStatusDescription()
		{
			return Factory.GetCachedValue<MessageStatusList>().GetDescriptionFromCode(JPM_MessageStatus) ?? ZString.Empty;
		}

		public override SchemaGuidColumn PKSchemaColumn => JPAFRBillsSchema.PK;

		protected override void AddToFactoryCache()
		{
			// should be called after MoveDetail is set
		}

		protected override ZGuid GetPK() => bill.PK;

		readonly JPAFRBills bill;
		internal readonly BlanketVesselChange BlanketVesselChange;
	}
}
