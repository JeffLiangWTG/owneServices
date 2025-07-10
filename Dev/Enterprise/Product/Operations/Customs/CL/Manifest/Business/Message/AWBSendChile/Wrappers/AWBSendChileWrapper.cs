using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AWBSendChileWrapper : IAWBRequest
	{
		public AWBSendChileWrapper(AsycudaBill bill, ZString actionType, ZString reasonType, string reason = WrappersConstants.EmptyString)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			header = bill.Header;
			ActionType = actionType;
			this.reason = reason;
			this.reasonType = reasonType;
		}
		readonly AsycudaBill bill;
		readonly AsycudaManifestHeader header;
		readonly string reasonType;
		readonly ZString reason;

		AsycudaArrivalLine ArrivalDetail => bill.Factory.GetValue(ref arrivalDetailCached, () =>
		{
			return CLMessageHelper.GetAsycudaArrivalLine(bill);
		});
		CachedProperty<AsycudaArrivalLine> arrivalDetailCached;

		ASYCUDA.Business.AsycudaArrivalHeader ArrivalHeader => ArrivalDetail?.ArrivalHeader;

		IAWBRequest awbChile => this;

		public string ActionType { get; }

		string IAWBRequest.ReferenceNumber => bill.ABL_BillNumber;

		string IAWBRequest.ItemsAmount => bill.ABL_ManifestQty.ToString("0");

		string IAWBRequest.Weight => CLMessageHelper.WeightConvertion(bill.ABL_GrossWeightUQ, bill.ABL_GrossWeight);

		string IAWBRequest.WeightUQ => CLMessageHelper.WeightUnitCodeCalculator(bill.ABL_GrossWeightUQ);

		string IAWBRequest.Volume => CLMessageHelper.VolumeConvertion(bill.ABL_VolumeUQ, bill.ABL_Volume);

		string IAWBRequest.VolumeUQ => CLMessageHelper.VolumeUnitCodeCalculator(bill.ABL_VolumeUQ);

		string IAWBRequest.ItemsTotal => bill.Packs.Count.ToString();

		bool IAWBRequest.IsPartial => ArrivalDetail != null && ArrivalDetail.ATL_Quantity != 0;

		string IAWBRequest.PartialCorrelative
		{
			get
			{
				var result = ZString.Empty;
				if (awbChile.IsPartial && ArrivalHeader != null)
				{
					result = ArrivalHeader.ATH_ArrivalSequence.ToString();
				}
				return result;
			}
		}

		IDocOpTransport IAWBRequest.DocOpTransport
		{
			get
			{
				var voyageName = header.AMA_Voyage;
				if (awbChile.IsPartial && ArrivalHeader != null)
				{
					voyageName = ArrivalHeader.ATH_VoyageFlightNo;
				}
				return new AWBSendChileWrapperOpTransport(header, voyageName);
			}
		}

		IReadOnlyCollection<IDocDates> IAWBRequest.DocDates
		{
			get
			{
				var fArvFromPartial = header.AMA_E_ARV.ToString(WrappersConstants.DateFormatLong);
				if (awbChile.IsPartial && ArrivalHeader != null)
				{
					fArvFromPartial = ArrivalHeader.ATH_ETAAtDischargePort.ToString(WrappersConstants.DateFormatLong);
				}

				var result = new List<IDocDates>();
				var dateCodes = new List<ZString> { WrappersConstants.DateName.Fem, WrappersConstants.DateName.Fzarpe, WrappersConstants.DateName.Farribo, WrappersConstants.DateName.Fpres };
				foreach (var dateCode in dateCodes)
				{
					result.Add(new AWBSendChileWrapperDate(header, dateCode, fArvFromPartial));
				}
				return result.ToArray();
			}
		}

		IReadOnlyCollection<IDocLocations> IAWBRequest.DocLocations
		{
			get
			{
				var result = new List<IDocLocations>();
				var locationCodes = new List<ZString> { WrappersConstants.LocationName.Pe, WrappersConstants.LocationName.Pd };
				foreach (var locationCode in locationCodes)
				{
					result.Add(new AWBSendChileWrapperLocation(header, locationCode));
				}
				return result.ToArray();
			}
		}

		IReadOnlyCollection<IDocParticipations> IAWBRequest.DocParticipations
		{
			get
			{
				var result = new List<IDocParticipations>();
				var participationCodes = new List<ZString> { WrappersConstants.ParticipationName.Alm, WrappersConstants.ParticipationName.Caer, WrappersConstants.ParticipationName.Emi, WrappersConstants.ParticipationName.Emido, WrappersConstants.ParticipationName.Cons,
					WrappersConstants.ParticipationName.Noti, WrappersConstants.ParticipationName.Cnte };
				foreach (var participationCode in participationCodes)
				{
					result.Add(new AWBSendChileWrapperParticipation(bill, participationCode));
				}
				return result.ToArray();
			}
		}

		IReadOnlyCollection<IDocItems> IAWBRequest.DocItems
		{
			get
			{
				var result = new List<IDocItems>();
				foreach (AsycudaPack pack in bill.Packs)
				{
					result.Add(new AWBSendChileWrapperItem(pack, awbChile.IsPartial, ArrivalDetail));
				}
				return result.ToArray();
			}
		}

		IReadOnlyCollection<IDocCharges> IAWBRequest.DocCharges => new List<IDocCharges>() { new AWBSendChileWrapperCharge(bill) };

		IReadOnlyCollection<IDocReferences> IAWBRequest.DocReferences
		{
			get
			{
				var references = new List<IDocReferences> { new AWBSendChileWrapperReference(bill, WrappersConstants.ReferenceType.Ref, ArrivalDetail) };
				var masterBill = (AsycudaBill)header.MasterBill;
				if (masterBill != null)
				{
					references.Add(new AWBSendChileWrapperReference(masterBill, WrappersConstants.ReferenceType.Madre, ArrivalDetail));
				}
				return references.ToArray();
			}
		}

		IReadOnlyCollection<IDocObservations> IAWBRequest.DocObservations => new List<IDocObservations>() { new AWBSendChileWrapperObservation(reasonType, reason) };
	}
}
