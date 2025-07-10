using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromSundryCharges : FreightWrapper
	{
		public FreightWrapperFromSundryCharges(SundryCharges sundryCharges, BusinessObjectFactory factory)
			: base(sundryCharges, factory)
		{
			SundryChargesBO = sundryCharges ?? Factory.GetNull<SundryCharges>();
		}

		protected override Job GetJob()
		{
			return new Job.Loader(SundryChargesBO).Load();
		}

		protected override ZString GetJobNumber()
		{
			return SundryChargesBO.D4_JobNumber;
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("8cdd4a94-4279-49bd-a386-18c2271abcb6", "Job Number");
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return SundryChargesBO.PK;
		}

		protected override SundryCharges GetSundryCharges()
		{
			return SundryChargesBO;
		}

		#region Wrapper Properties

		protected override OrganisationWrapper GetPrincipal()
		{
			return new OrganisationWrapper(OrganisationUsageType.BillToParty, SundryChargesBO.BillToParty, ContactType.ShippingLine, Factory);
		}

		#endregion

		#region Implementation

		readonly SundryCharges SundryChargesBO;

		#endregion
	}
}
