#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class StampDutyRechargeRegistryControl
	{
		public ZArchitecture.GUI.ZDropEdit StampDutyRechargeOrganizationTypeDropEdit_ForTestOnly
		{
			get { return StampDutyRechargeOrganizationTypeDropEdit; }
			set { StampDutyRechargeOrganizationTypeDropEdit = value; }
		}

		public ZArchitecture.GUI.ZDropEdit StampDutyRechargeTransactionTypeDropEdit_ForTestOnly
		{
			get { return StampDutyRechargeTransactionTypeDropEdit; }
			set { StampDutyRechargeTransactionTypeDropEdit = value; }
		}
	}
}

#endif
