using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public class JobDeclarationSynchroniser : Customs.Business.JobDeclarationSynchroniser
	{
		public JobDeclarationSynchroniser(JobDeclaration destination)
			: base(destination)
		{
		}

		public new JobDeclaration Destination => (JobDeclaration)base.Destination;

		protected override Customs.Business.BillsSynchroniser GetBillsSynchroniser()
		{
			return new BillsSynchroniser(Destination, GetHouseBillOfSpecificShipment);
		}

		protected override ZString GetHouseBillOfSpecificShipment(Freight.Forwarding.Business.IBillDetails billDetails)
		{
			var bkgNumberInfo = billDetails.BKGBillNumberInfo;
			return bkgNumberInfo == null ? base.GetHouseBillOfSpecificShipment(billDetails) : (ZString)bkgNumberInfo.Value;
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!SyncChangesDetected)
			{
				Source.JS_TransportModeInfo.ValueChanged += JS_TransportModeInfo_ValueChanged;
			}
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			Source.JS_TransportModeInfo.ValueChanged -= JS_TransportModeInfo_ValueChanged;
		}

		void JS_TransportModeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			PackLineSynchronisers.Synchronise(IsEnabled);
		}

		protected override void HookSupplierAndImporter()
		{
			AddDocAddressSynchroniser(Destination.SupplierDocumentaryAddress, Source.ConsignorDocumentaryAddress);
			AddDocAddressSynchroniser(Destination.ImporterDocumentaryAddress, Source.ConsigneeDocumentaryAddress);
		}

		void AddDocAddressSynchroniser(JobDocAddress destinationDocAddress, JobDocAddress sourceDocAddress)
		{
			Synchronisers.Add(new FieldSynchroniser(destinationDocAddress.E2_OA_AddressInfo,
				() => sourceDocAddress.E2_AddressOverride ? ZGuid.Empty : sourceDocAddress.E2_OA_Address,
				() => new ZPropertyInfo[] { sourceDocAddress.E2_OA_AddressInfo, sourceDocAddress.E2_AddressOverrideInfo, sourceDocAddress.OrganisationPKInfo },
				() => sourceDocAddress != null && (sourceDocAddress.Organisation == null || sourceDocAddress.Organisation.IsMiscellaneous),
				() => !sourceDocAddress.E2_AddressOverride)
			{ JobDocAddressPersistingSyncronizedValue = destinationDocAddress });
		}
	}
}
