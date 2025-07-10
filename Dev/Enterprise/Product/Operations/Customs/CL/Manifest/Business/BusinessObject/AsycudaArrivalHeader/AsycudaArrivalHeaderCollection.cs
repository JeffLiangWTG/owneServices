namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AsycudaArrivalHeaderCollection : ASYCUDA.Business.AsycudaArrivalHeaderCollection<AsycudaArrivalHeader>
	{
		public AsycudaArrivalHeaderCollection(AsycudaManifestHeader master)
			: base(master)
		{
			header = master;
		}
		readonly AsycudaManifestHeader header;

		protected override void SetDefaultsForNewElementCore(AsycudaArrivalHeader newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (header != null && !newElement.ATH_AMA_ManifestHeader.IsEmpty)
			{
				newElement.ATH_VoyageFlightNo = header.AMA_Voyage;
				newElement.ATH_Reference = header.RegistrationNumber;
				newElement.ATH_ETAAtDischargePort = header.AMA_E_ARV;
				newElement.ATH_ReferenceIssueDate = header.RegistrationDate.Date;

				foreach (AsycudaBill bill in header.Bills)
				{
					bill.SetCachedNullForSendOriginalMessage();
				}
			}
		}
	}
}
