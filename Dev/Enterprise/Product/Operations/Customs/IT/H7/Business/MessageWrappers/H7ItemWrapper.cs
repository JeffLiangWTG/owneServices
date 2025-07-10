using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.EntityFramework;
using Enterprise.Core;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class H7ItemWrapper : IH7Item
{
	public H7ItemWrapper(AsycudaPackedItem packedItem)
	{
		this.packedItem = Argument.NotNull(packedItem, nameof(packedItem));
		this.bill = Argument.NotNull(packedItem.Bill, nameof(packedItem.Bill));
	}

	readonly AsycudaBill bill;
	readonly AsycudaPackedItem packedItem;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformation
	{
		get
		{
			if (additionalInfos == null)
			{
				var addInfosList = new List<AdditionalInformationWrapper>();
				packedItem.AdditionalInfos
					.Where(info => info.IsAnAdditionalInformation)
					.ForEach(info => addInfosList.Add(new AdditionalInformationWrapper(info)));

				additionalInfos = addInfosList.AsReadOnly();
			}

			return additionalInfos;
		}
	}

	public IReadOnlyCollection<IAdditionalReference> AdditionalReferences
	{
		get
		{
			if (additionalReferences == null)
			{
				var addRefList = new List<AdditionalReferenceWrapper>();
				packedItem.AdditionalInfos
					.Where(info => info.IsAnAdditionalReference)
					.ForEach(info => addRefList.Add(new AdditionalReferenceWrapper(info)));

				additionalReferences = addRefList.AsReadOnly();
			}

			return additionalReferences;
		}
	}

	public IEoriTrader Exporter => CachedValueHelper.GetValue(ref exporter, () => new EoriTraderWrapper(null, bill.GetShipperAddress()));
	CachedValue<IEoriTrader> exporter;

	public string GoodsDescription => packedItem.API_GoodsDescription;

	public decimal GrossMass => Constants.Weight.Convert(packedItem.API_GrossWeight, packedItem.API_GrossWeightUQ, Constants.Weight.Kilograms);

	public string HSCode => packedItem.API_Tariff.Left(6);

	public ICost IntrinsicValue => CachedValueHelper.GetValue(ref intrinsicValue, () => new CostWrapper(packedItem.API_GoodsValue, packedItem.API_RX_NKGoodsValueCurrency));
	CachedValue<ICost> intrinsicValue;

	public int ItemNumber => packedItem.SequenceNumber;

	public string MethodOfPayment => bill.Header.AMA_PaymentMethod;

	public IReadOnlyCollection<int> NumberOfPacks
	{
		get
		{
			if (numberOfPacks == null)
			{
				var numberOfPacksList = packedItem.AsycudaPackPackedItemLinks
					.Where(p => p.IsLinked)
					.Select(p => (int)p.PackQty)
					.ToList();

				numberOfPacks = numberOfPacksList.AsReadOnly();
			}

			return numberOfPacks;
		}
	}
	ReadOnlyCollection<int> numberOfPacks;

	public IReadOnlyCollection<IPreviousDocument> PreviousDocuments
	{
		get
		{
			if (previousDocuments == null)
			{
				var prevDocList = new List<PreviousDocumentWrapper>();
				packedItem.PreviousDocuments.ForEach(doc => prevDocList.Add(new PreviousDocumentWrapper(doc)));

				previousDocuments = prevDocList.AsReadOnly();
			}

			return previousDocuments;
		}
	}

	public ICustomsProcedure Procedure => CachedValueHelper.GetValue(ref procedure, () => new CustomsProcedureWrapper(bill));
	CachedValue<ICustomsProcedure> procedure;

	public decimal? SupplementaryUnit => packedItem.API_CustomsQty2;

	public IReadOnlyCollection<ISupportingDocument> SupportingDocuments
	{
		get
		{
			if (supportingDocuments == null)
			{
				var supportingDocsList = new List<SupportingDocumentWrapper>();
				packedItem.SupportingDocuments
					.ForEach(doc => supportingDocsList.Add(new SupportingDocumentWrapper(doc)));

				supportingDocuments = supportingDocsList.AsReadOnly();
			}

			return supportingDocuments;
		}
	}

	public ICost TransportCosts => CachedValueHelper.GetValue(ref transportCosts, () =>
	{
		var valuePerItem = (bill.ABL_InsuranceValue + bill.ABL_TransportValue) / bill.PackedItems.Count;
		return new CostWrapper(valuePerItem, bill.ABL_RX_NKTransportValueCurrency);
	});
	CachedValue<ICost> transportCosts;

	public IReadOnlyCollection<ITransportDocument> TransportDocuments
	{
		get
		{
			if (transportDocuments == null)
			{
				var transportDocsList = new List<TransportDocumentWrapper>();
				packedItem.AdditionalInfos
					.Where(doc => doc.IsATransportDocument)
					.ForEach(doc => transportDocsList.Add(new TransportDocumentWrapper(doc)));

				transportDocuments = transportDocsList.AsReadOnly();
			}

			return transportDocuments;
		}
	}

	public string Ucr => bill.ABL_UCRNumber;

	ReadOnlyCollection<AdditionalReferenceWrapper> additionalReferences;
	ReadOnlyCollection<AdditionalInformationWrapper> additionalInfos;
	ReadOnlyCollection<SupportingDocumentWrapper> supportingDocuments;
	ReadOnlyCollection<PreviousDocumentWrapper> previousDocuments;
	ReadOnlyCollection<TransportDocumentWrapper> transportDocuments;
}
