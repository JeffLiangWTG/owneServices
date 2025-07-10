using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class JobDocAddressRequirementProvider
	{
		public JobDocAddressRequirementProvider(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		internal JobDocAddressRequirement GoodsOwnerDocAddressRequirement
		{
			get { return fGoodsOwnerDocAddressRequirement ?? (fGoodsOwnerDocAddressRequirement = AddJobDocAddressRequirement(DocAddressType.GoodsOwner)); }
		}
		JobDocAddressRequirement fGoodsOwnerDocAddressRequirement;

		internal JobDocAddressRequirement CarrierAgentDocumentaryAddressRequirement
		{
			get { return fCarrierAgentDocumentaryAddressRequirement ?? (fCarrierAgentDocumentaryAddressRequirement = AddJobDocAddressRequirement(DocAddressType.CarrierAgent)); }
		}
		JobDocAddressRequirement fCarrierAgentDocumentaryAddressRequirement;

		internal JobDocAddressRequirement TransporterDocumentaryAddressRequirement
		{
			get { return fTransporterDocumentaryAddressRequirement ?? (fTransporterDocumentaryAddressRequirement = AddJobDocAddressRequirement(DocAddressType.Transporter)); }
		}
		JobDocAddressRequirement fTransporterDocumentaryAddressRequirement;

		internal JobDocAddressRequirement DispatchWarehouseDocumentaryAddressRequirement
		{
			get { return fDispatchWarehouseDocumentaryAddressRequirement ?? (fDispatchWarehouseDocumentaryAddressRequirement = AddJobDocAddressRequirement(DocAddressType.DispatchWarehouse)); }
		}
		JobDocAddressRequirement fDispatchWarehouseDocumentaryAddressRequirement;

		internal JobDocAddressRequirement DestinationWarehouseDocumentaryAddressRequirement
		{
			get { return fDestinationWarehouseDocumentaryAddressRequirement ?? (fDestinationWarehouseDocumentaryAddressRequirement = AddJobDocAddressRequirement(DocAddressType.DestinationWarehouse)); }
		}
		JobDocAddressRequirement fDestinationWarehouseDocumentaryAddressRequirement;

		JobDocAddressRequirement AddJobDocAddressRequirement(DocAddressType docAddressType)
		{
			var result = new JobDocAddressRequirement(docAddressType);
			SetJobDocAddressRequirement(result);
			return result;
		}

		internal void SetJobDocAddressRequirement(JobDocAddressRequirement jobDocAddressRequirement)
		{
			jobDocAddressRequirement.GetRegistrationNumberResult = GetRegistrationNumberResult;
			jobDocAddressRequirement.LookupsGovRegNumTypes = GetGovRegNumTypeList;
		}

		RegistrationNumberResult GetRegistrationNumberResult(JobDocAddress docAddress)
		{
			return new RegistrationNumberResult(docAddress.Factory, true, () => new RegistrationNumber());
		}

		CodeDescriptionPairList GetGovRegNumTypeList(JobDocAddressLookups lookups)
		{
			return factory.GetCachedValue<GovRegNumTypeList>();
		}
	}
}
