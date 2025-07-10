using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	class CFRCollectionSummaryWriter : IUNDGSubstanceCollectionSummaryWriter
	{
		ZString IUNDGSubstanceCollectionSummaryWriter.GetSummary(IReadOnlyCollection<UNDGSubstanceWrapper> collection)
		{
			var cfrUNDGs = collection.Where(IsCFRSubstance).ToList();

			if (cfrUNDGs.Count == 0)
			{
				return ZString.Empty;
			}

			var summaries = GetParentShipments(cfrUNDGs)
				.SelectMany(GetAdditionalHandlingInformationNoteSummariesFromShipment)
				.ToList();

			summaries.Add(StatementOfApproval);

			return string.Join(System.Environment.NewLine, summaries);
		}

		IReadOnlyCollection<ForwardingShipment> GetParentShipments(IEnumerable<UNDGSubstanceWrapper> wrappers)
		{
			var result = new Dictionary<ZGuid, ForwardingShipment>();

			foreach (var wrapper in wrappers)
			{
				if (wrapper?.DGData is ForwardingUNDGDataItem forwardingDataItem
					&& forwardingDataItem.ParentPackLine?.Shipment is ForwardingShipment parentShipment)
				{
					result[parentShipment.PK] = parentShipment;
				}
			}

			return result.Values.ToArray();
		}

		IReadOnlyCollection<ZString> GetAdditionalHandlingInformationNoteSummariesFromShipment(ForwardingShipment shipment)
		{
			return shipment?
				.Notes?
				.FindByDescription(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description)
				.Select(note => note.ST_NoteText)
				.ToArray();
		}

		ZString StatementOfApproval
			=> Res.GetString("d9a71f4f-20dc-9f9f-4790-a755f77222fe", "This is to certify that the above-named/herein-named materials are properly classified, described, packaged, marked and labeled, and are in proper condition for transportation according to the applicable regulations of the Department of Transportation.");

		bool IsCFRSubstance(UNDGSubstanceWrapper wrapper)
		{
			return (wrapper.DGData?.Subs?.DG_Standard ?? ZString.Empty) == UNDGSubstanceStandardTypes.CFR;
		}
	}
}
