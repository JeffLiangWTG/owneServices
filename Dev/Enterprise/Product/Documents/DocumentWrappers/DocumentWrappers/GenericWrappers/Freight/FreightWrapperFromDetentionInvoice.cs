using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromDetentionInvoice : FreightWrapper
	{
		public FreightWrapperFromDetentionInvoice(ContainerDetention detention, BusinessObjectFactory factory)
			: base(detention, factory)
		{
			this.DetentionBO = detention ?? Factory.GetNull<ContainerDetention>();
		}

		protected override Job GetJob()
		{
			return new Job.Loader(DetentionBO).Load();
		}

		protected override ZString GetJobNumber()
		{
			return DetentionBO.NC_JobNumber;
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("d520300e-6088-4c05-9fbd-a5e30704387c", "Job Number");
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(DetentionBO, Factory);
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return DetentionBO.PK;
		}

		protected override ClientAndAgentBrandingBusinessObject AlternativeBranding
		{
			get { return ((IDocumentSupportable)DetentionBO).DocumentSupporter.GetAlternativeBranding(); }
		}

		#region Wrapper Properties

		protected override OrganisationWrapper GetPrincipal()
		{
			return new OrganisationWrapper(OrganisationUsageType.Principal, DetentionBO.Principal, ContactType.ShippingLine, Factory);
		}

		#endregion

		#region Implementation

		readonly ContainerDetention DetentionBO;

		#endregion
	}
}
