using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPECalloutQueue : UPECargoReportQueue
	{
		public UPECalloutQueue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ProcessQueueValidation GetNewValidation()
		{
			return new UPECalloutQueueValidation(this);
		}

		protected override void RaiseEIRProcessing()
		{
			if (P4_QueueName == CommercialQueueCodeDescriptionPairList.Codes.EIR)
			{
				base.RaiseEIRProcessing();
			}
		}

		protected override void SetCommercialReleasedDate()
		{
			if (P4_QueueName != CommercialQueueCodeDescriptionPairList.Codes.EIR)
			{
				base.SetCommercialReleasedDate();
			}
		}

		public ZString AccountNumberAccountClass
		{
			get
			{
				ZString result = "";
				UPEOrgHeader uPEOrgHeader = (UPEOrgHeader)OrgHeader.FindByOrgCusCode(Factory, UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber, P4_CustomAttrib8);
				if (uPEOrgHeader != null)
				{
					result = uPEOrgHeader.AccountClass;
				}

				return result;
			}
		}
	}
}
