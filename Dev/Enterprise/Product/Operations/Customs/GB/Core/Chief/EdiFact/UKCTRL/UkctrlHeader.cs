namespace Enterprise.Customs.GB.Chief.EdiFact.UKCTRL
{
	public class UkctrlHeader
	{
		public string UNH_SYS_MRN { get; set; }

		public string UCM_MSG_MRN { get; set; }
		public string UCM_MSG_SYS_CAR { get; set; }
		public string UCM_MSG_TYPE { get; set; }

		public string UCM_MSG_VSN { get; set; }
		public string UCM_MSG_REL_NO { get; set; }
		public string UCM_MSG_CNTR_AGNCY { get; set; }
		public string UCM_MSG_ASG_CODE { get; set; }

		public string UCX_ACTION_CODE { get; set; }
		public string UCX_MSG_ERROR_CODE { get; set; }
	}
}
