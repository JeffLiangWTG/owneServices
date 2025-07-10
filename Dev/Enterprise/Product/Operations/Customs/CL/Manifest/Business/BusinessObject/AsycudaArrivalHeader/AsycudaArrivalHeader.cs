using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AsycudaArrivalHeader : ASYCUDA.Business.AsycudaArrivalHeader
	{
		public AsycudaArrivalHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("AsycudaArrivalHeader.ATH_ETAAtDischargePort", Caption = "Arrival Date")]
		public override ZDateTime ATH_ETAAtDischargePort { get => base.ATH_ETAAtDischargePort; set => base.ATH_ETAAtDischargePort = value; }

		[ResourceStringData("AsycudaArrivalHeader.ATH_Reference", Caption = "Registration Number")]
		public override ZString ATH_Reference { get => base.ATH_Reference; set => base.ATH_Reference = value; }

		[ResourceStringData("AsycudaArrivalHeader.ATH_ReferenceIssueDate", Caption = "Registration Date")]
		public override ZDate ATH_ReferenceIssueDate { get => base.ATH_ReferenceIssueDate; set => base.ATH_ReferenceIssueDate = value; }

		protected override AsycudaArrivalLineCollection CreateNewArrivalLineCollection() => new AsycudaArrivalLineCollection<AsycudaArrivalLine>(this);

		protected AsycudaBill BillAsociated => ArrivalDetails.Count > 0
			? Factory.Load<AsycudaBill>(ArrivalDetails.FirstOrDefault()?.ATL_ABL_AsycudaBill ?? ZGuid.Empty)
			: null;

		public override bool CanDelete => BillAsociated == null || BillAsociated.CanDelete;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var resultEmpty = (NoResString)string.Empty;

				if (!CanDelete)
				{
					if (BillAsociated.ABL_BillStatus == CustomsStatusList.Codes.CAN)
					{
						return ResString.GetMultilingualString("43F3861C-C228-4F4E-A874-4A059D50987F", "This bill is canceled. Must be kept for history purposes.");
					}
					else if (BillAsociated.ABL_BillStatus == CustomsStatusList.Codes.ACP)
					{
						return ResString.GetMultilingualString("2DDD44EB-2C6B-40AC-B163-6271FAC404B6", "This bill is already sent and accepted.");
					}
					else if (BillAsociated.ABL_BillStatus == CustomsStatusList.Codes.SNT && BillAsociated.ABL_MessageStatus == MessageStatusCodeList.Codes.Awaiting)
					{
						return ResString.GetMultilingualString("CC2D9CED-AEDD-472C-80CE-EFFD44C61234", "This bill is already sent and it is awaiting for a response.");
					}
					else
					{
						return resultEmpty;
					}
				}
				return resultEmpty;
			}
		}
	}
}
