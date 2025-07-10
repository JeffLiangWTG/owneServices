using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromDetentionAdvice : FreightWrapper
	{
		public FreightWrapperFromDetentionAdvice(DetentionAdviceHeader detention, BusinessObjectFactory factory)
			: base(detention, factory)
		{
			this.detentionBO = detention ?? Factory.GetNull<DetentionAdviceHeader>();
		}

		readonly DetentionAdviceHeader detentionBO;

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(detentionBO, Factory);
		}

		protected override OrganisationWrapper GetClient()
		{
			return new OrganisationWrapper(OrganisationUsageType.Client, detentionBO.Client, ContactType.Receivables, Factory);
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("d520300e-6088-4c05-9fbd-a5e30704387c", "Job Number");
		}

		#region IBODocDataProvider Members

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get { return detentionBO.Client; }
		}

		protected override BusinessObject ParentBusinessObject
		{
			get { return detentionBO.Client; }
		}

		#endregion
	}
}
