using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaTransportDocumentCollectionSynchroniser : ASYCUDA.Business.AsycudaTransportDocumentCollectionSynchroniser
	{
		public AsycudaTransportDocumentCollectionSynchroniser(ForwardingConsol sourceConsol, ASYCUDA.Business.AsycudaBill destination)
			: base(sourceConsol, destination)
		{
		}

		protected new AsycudaBill Destination => (AsycudaBill)base.Destination;

		protected new ForwardingConsol Source => base.Source;

		protected override void OnDetectEnabledChanged()
		{
			if (DetectEnabled)
			{
				Destination.TransportDocuments.Load();
			}
			base.OnDetectEnabledChanged();
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.Header.AMA_OverrideFreightDefaults)
			{
				if (Source.IsSea)
				{
					SynchroniseTransportDocuments(GetSourceNumbersToSynchronise());
				}
			}
		}

		IEnumerable<CusEntryNumber> GetSourceNumbersToSynchronise()
		{
			var sourceNumbers = new List<CusEntryNumber>();
			{
				foreach (var sourceNumber in Source.Numbers.OfType<CusEntryNumber>())
				{
					if (sourceNumber != null && sourceNumber.CE_EntryType == IsraelConsolAdditionalReferenceNumberTypes.Codes.ParentDealNumber && !sourceNumber.CE_EntryNum.IsEmpty)
					{
						sourceNumbers.Add(sourceNumber);
					}
				}
				return sourceNumbers;
			}
		}

		void SynchroniseTransportDocuments(IEnumerable<CusEntryNumber> sourceNumbers)
		{
			DeleteTransportDocuments(sourceNumbers);
			if (!SyncChangesDetected)
			{
				var existingTransportDocuments = Destination.TransportDocuments.ToList();
				foreach (var sourceTransportDocument in sourceNumbers)
				{
					var cusTransportDocument = GetOrCreateTransportDocument(existingTransportDocuments);
					if (SyncChangesDetected)
					{
						return;
					}
					if (cusTransportDocument != null && cusTransportDocument.CSI_ReferenceNumber != sourceTransportDocument.CE_EntryNum)
					{
						ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(new AsycudaTransportDocumentSynchroniser(cusTransportDocument, sourceTransportDocument), IsEnabled, DetectEnabled);
					}
				}
			}
		}

		AsycudaTransportDocumentInfo GetOrCreateTransportDocument(List<AsycudaTransportDocumentInfo> existingTransportDocuments)
		{
			var result = existingTransportDocuments.FirstOrDefault(td => td.CSI_Code == Constants.IsraeliCustoms.ManifestTransportContractDocumentId && !td.IsDeleted);
			if (result != null)
			{
				if (DetectEnabled)
				{
					SyncChangesDetected = true;
					return null;
				}
			}
			else
			{
				if (DetectEnabled)
				{
					SyncChangesDetected = true;
					return null;
				}
				using (Destination.TransportDocuments.SuspendListChanged())
				{
					result = Destination.TransportDocuments.AddNew();
				}
			}
			return result;
		}

		void DeleteTransportDocuments(IEnumerable<CusEntryNumber> sourceCollection)
		{
			if (sourceCollection != null && sourceCollection.Any())
			{
				foreach (var transportDocument in Destination.TransportDocuments.Where(td => td.CSI_Code == Constants.IsraeliCustoms.ManifestTransportContractDocumentId).OfType<AsycudaTransportDocumentInfo>().ToArray().Skip(1))
				{
					if (DetectEnabled)
					{
						SyncChangesDetected = true;
						return;
					}
					transportDocument.Delete();
				}
			}
			else
			{
				foreach (var transportDocument in Destination.TransportDocuments.Where(td => td.CSI_Code == Constants.IsraeliCustoms.ManifestTransportContractDocumentId).OfType<AsycudaTransportDocumentInfo>().ToArray())
				{
					if (DetectEnabled)
					{
						SyncChangesDetected = true;
						return;
					}
					transportDocument.Delete();
				}
			}
		}
	}
}
