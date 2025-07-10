using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class BLSendChileWrapper : IBLRequest
	{
		public BLSendChileWrapper(AsycudaBill bill, ZString actionType, string reasonType = WrappersConstants.EmptyString, string reason = WrappersConstants.EmptyString)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			header = this.bill.Header;
			ActionType = actionType;
			this.reasonType = reasonType;
			this.reason = reason;
		}
		readonly AsycudaBill bill;
		readonly AsycudaManifestHeader header;
		readonly ZString reasonType;
		readonly ZString reason;

		public string ActionType { get; }

		string IBLRequest.ReferenceNumber => bill.ABL_BillNumber;

		string IBLRequest.Date => header.AMA_DateAtCustomsOffice.ToString(WrappersConstants.DateFormatLong);

		string IBLRequest.Service => header.AMA_IsTramp ? BLSendChileConstants.ServiceDocument.Tramp : BLSendChileConstants.ServiceDocument.Liner;

		string IBLRequest.ServiceType
		{
			get
			{
				var result = BLSendChileConstants.ServiceTypeDocument.Empty;
				var containerMode = header.AMA_ContainerMode;
				if (bill.ABL_RoRo)
				{
					result = BLSendChileConstants.ServiceTypeDocument.RoRo;
				}
				else
				{
					if (containerMode == Core.Constants.ContainerModes.BreakBulk || containerMode == Core.Constants.ContainerModes.Bulk)
					{
						result = BLSendChileConstants.ServiceTypeDocument.Bb;
					}
					else if (containerMode == Core.Constants.ContainerModes.Containerised)
					{
						var containers = header.Containers.Cast<AsycudaContainer>();
						var count = containers.Count();
						if (count > 0 && containers.All(x => x.ACN_EmptyFullIndicator == EmptyFullIndicatorList.Codes.FullContainerLoad))
						{
							result = BLSendChileConstants.ServiceTypeDocument.FclFcl;
						}
						else
						{
							result = BLSendChileConstants.ServiceTypeDocument.LclLcl;
						}
					}
				}
				return result;
			}
		}

		string IBLRequest.ItemsAmount => bill.ABL_ManifestQty.ToString("0");

		string IBLRequest.Weight => CLMessageHelper.WeightConvertion(bill.ABL_GrossWeightUQ, bill.ABL_GrossWeight);

		string IBLRequest.WeightUQ => CLMessageHelper.WeightUnitCodeCalculator(bill.ABL_GrossWeightUQ);

		string IBLRequest.Volume => CLMessageHelper.VolumeConvertion(bill.ABL_VolumeUQ, bill.ABL_Volume);

		string IBLRequest.VolumeUQ => CLMessageHelper.VolumeUnitCodeCalculator(bill.ABL_VolumeUQ);

		string IBLRequest.ItemsTotal => bill.Packs?.Count.ToString();

		IDocumentOpTransport IBLRequest.DocumentOpTransport => documentOpTransport ?? (documentOpTransport = new BLSendChileWrapperOpTransport(header));
		IDocumentOpTransport documentOpTransport;

		IDocumentFreight IBLRequest.DocumentFreight => documentFreight ?? (documentFreight = new BLSendChileWrapperDocumentFreight(bill));
		IDocumentFreight documentFreight;

		IReadOnlyCollection<IDocumentDate> IBLRequest.Dates
		{
			get
			{
				var result = new List<IDocumentDate>();
				var dateCodes = new List<ZString> { WrappersConstants.DateName.Fpres, WrappersConstants.DateName.Fem, WrappersConstants.DateName.Fzarpe, WrappersConstants.DateName.Femb };
				foreach (var dateCode in dateCodes)
				{
					result.Add(new BLSendChileWrapperDate(header, dateCode));
				}
				return result.ToArray();
			}
		}

		IReadOnlyCollection<IDocumentLocation> IBLRequest.Locations
		{
			get
			{
				var result = new List<IDocumentLocation>();
				var locationCodes = new List<ZString> { WrappersConstants.LocationName.Le, WrappersConstants.LocationName.Pe, WrappersConstants.LocationName.Pd, WrappersConstants.LocationName.Ld, WrappersConstants.LocationName.Lem, WrappersConstants.LocationName.Lrm };
				foreach (var locationCode in locationCodes)
				{
					result.Add(new BLSendChileWrapperLocation(bill, locationCode));
				}
				return result.ToArray();
			}
		}

		IReadOnlyCollection<IParticipationDocument> IBLRequest.ParticipationDocuments
		{
			get
			{
				var result = new List<IParticipationDocument>();
				var participationCodes = new List<ZString> { WrappersConstants.ParticipationName.Alm, WrappersConstants.ParticipationName.Emi, WrappersConstants.ParticipationName.Rep, WrappersConstants.ParticipationName.Emido,
					WrappersConstants.ParticipationName.Emb, WrappersConstants.ParticipationName.Cons, WrappersConstants.ParticipationName.Noti };
				foreach (var participationCode in participationCodes)
				{
					result.Add(new BLSendChileWrapperParticipation(bill, participationCode));
				}
				return result.ToArray();
			}
		}

		IReadOnlyCollection<IDocumentItem> IBLRequest.Items
		{
			get
			{
				var result = new List<IDocumentItem>();
				foreach (AsycudaPack pack in bill.Packs)
				{
					result.Add(new BLSendChileWrapperItem(pack));
				}
				return result.ToArray();
			}
		}

		IReadOnlyCollection<IReference> IBLRequest.References
		{
			get
			{
				var references = new List<IReference>() { new BLSendChileWrapperReference(bill, WrappersConstants.ReferenceType.Ref) };
				var masterBill = (AsycudaBill)header.MasterBill;
				if (masterBill != null)
				{
					references.Add(new BLSendChileWrapperReference(masterBill, WrappersConstants.ReferenceType.Madre));
				}
				return references.ToArray();
			}
		}

		IReadOnlyCollection<IDocObservation> IBLRequest.DocumentObservations => new List<IDocObservation>() { new BLSendChileWrapperObservation(reasonType, reason) };
	}
}
