using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class G4AdditionalDataProvider : ITemporaneaCustodiaG4AdditionalDataProvider
{
	IReadOnlyCollection<ContainerTypeDataProviderAbstractClass> ITemporaneaCustodiaG4AdditionalDataProvider.GetContainer(TemporaryStorageBill bill)
	{
		if (bill is null)
		{
			return null;
		}

		var linkPackages = bill.PackedItems.SelectMany(p => p.TemporaryStorageLinkPackages);
		var billContainers = DistinctLinkedPackages(linkPackages);
		return billContainers.Count == 0
			? [new PlaceholderContainerTypeDataProvider()]
			: billContainers.Select(p => p.ContainerTypeDataProvider).ToList();
	}

	DepositoTypeDataProviderAbstractClass ITemporaneaCustodiaG4AdditionalDataProvider.GetDeposito(TemporaryStorageBill businessObject)
	{
		var additionalIdentifier = businessObject?.Header?.GoodsLocation?.CGL_AdditionalIdentifier ?? ZString.Empty;
		return !additionalIdentifier.IsEmpty
			? new DepositoTypeDataProvider(additionalIdentifier.Left(6))
			: null;
	}

	IReadOnlyCollection<MenzioniSpecialiTypeDataProviderAbstractClass> ITemporaneaCustodiaG4AdditionalDataProvider.GetMenzioniSpeciali(TemporaryStoragePackedItem businessObject)
	{
		if (businessObject is null)
		{
			return null;
		}

		var additionalInfoItems = businessObject
			.AdditionalInfos
			.Where(x => x.IsAnAdditionalInformation);

		return additionalInfoItems.Any()
			? additionalInfoItems.Select(x => new MenzioniSpecialiTypeDataProvider(x)).ToList()
			: [new PlaceholderMenzioniSpecialiTypeDataProvider()];
	}

	IReadOnlyCollection<ContainerDettaglioTypeDataProviderAbstractClass> ITemporaneaCustodiaG4AdditionalDataProvider.GetContainerDettaglio(TemporaryStoragePackedItem packedItem)
	{
		if (packedItem is null)
		{
			return null;
		}

		var linkedContainers = DistinctLinkedPackages(packedItem.TemporaryStorageLinkPackages);
		return linkedContainers.Count == 0
			? [new PlaceholderContainerDettaglioTypeDataProvider()]
			: linkedContainers.Select(p => p.ContainerDettaglioTypeDataProvider).ToList();
	}

	List<MergedContainer> DistinctLinkedPackages(IEnumerable<EU.Business.CusTempStorage.TemporaryStorageLinkPackage> linkPackages)
	{
		var containers = new Dictionary<string, MergedContainer>();
		linkPackages
			.Where(x => x.IsLinked && x.Package?.Container is not null)
			.Select(x => x.Package.Container)
			.Distinct()
			.ForEach(x =>
			{
				if (!containers.TryGetValue(x.ACN_ContainerNumber, out var mergedContainer))
				{
					mergedContainer = new MergedContainer(x.ACN_ContainerNumber);
					containers.Add(x.ACN_ContainerNumber, mergedContainer);
				}
				mergedContainer.TryAddContainer(x as TemporaryStorageContainer);
			});
		return containers.Values.ToList();
	}

	string RetrievePaese(CusGoodsLocation businessObject)
	{
		var additionalIdentifier = businessObject?.CGL_AdditionalIdentifier ?? ZString.Empty;
		return !additionalIdentifier.IsEmpty ? Core.Constants.CountryCodes.Italy : null;
	}

	string RetrieveQualificaRappresentante(TemporaryStorageHeader businessObject)
	{
		if (businessObject is null)
		{
			return null;
		}

		return (string)businessObject.AMA_AgentType switch
		{
			RepresentationTypeList.Codes._2Direct => "2",
			RepresentationTypeList.Codes._3Indirect => "3",
			_ => null,
		};
	}

	string ITemporaneaCustodiaG4AdditionalDataProvider.GetPaese(CusGoodsLocation businessObject) =>
		RetrievePaese(businessObject);

	string ITemporaneaCustodiaG4AdditionalDataProvider.GetAmendmentPaese(CusGoodsLocation businessObject) =>
		RetrievePaese(businessObject);

	string ITemporaneaCustodiaG4AdditionalDataProvider.GetQualificaRappresentante(TemporaryStorageHeader businessObject) =>
		RetrieveQualificaRappresentante(businessObject);

	string ITemporaneaCustodiaG4AdditionalDataProvider.GetAmendmentQualificaRappresentante(TemporaryStorageHeader businessObject) =>
		RetrieveQualificaRappresentante(businessObject);

	IReadOnlyCollection<DichiarazioneSemplificataTypeDataProviderAbstractClass> ITemporaneaCustodiaG4AdditionalDataProvider.GetDichiarazioneSemplificata(TemporaryStorageBill businessObject)
	{
		if (businessObject is null)
		{
			return null;
		}

		var distinctPreviousDocuments = businessObject.PreviousDocuments
					.Concat(businessObject.PackedItems.SelectMany(packedItem => packedItem.PreviousDocuments))
					.GroupBy(pd => new
					{
						pd.CSI_Code,
						pd.CSI_ReferenceNumber,
						pd.CSI_LineNo,
						pd.CSI_PackType,
						pd.CSI_UnitOfQuantity,
					})
					.Select(g => new DichiarazioneSemplificataTypeDataProvider(g.First(), g.Sum(x => x.CSI_PackQty), g.Sum(x => x.CSI_Quantity)))
					.ToList();

		return distinctPreviousDocuments;
	}

	#region Implementation

	sealed class DepositoTypeDataProvider(ZString additionalIdentifier) : DepositoTypeDataProviderAbstractClass
	{
		public override string TipoDep => CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;

		public override string IdentificativoDep => additionalIdentifier;

		readonly ZString additionalIdentifier = additionalIdentifier;
	}

	sealed class PlaceholderMenzioniSpecialiTypeDataProvider : MenzioniSpecialiTypeDataProviderAbstractClass
	{
		public override string Descrizione => (NoResString)"Nessuna delle precedenti.";
	}

	sealed class MenzioniSpecialiTypeDataProvider(TemporaryStorageAdditionalInfo additionalInfo) : MenzioniSpecialiTypeDataProviderAbstractClass
	{
		public override string Codice => additionalInfo.CSI_Code;

		public override string Descrizione => additionalInfo.CSI_Description;

		readonly TemporaryStorageAdditionalInfo additionalInfo = additionalInfo;
	}

	sealed class PlaceholderContainerDettaglioTypeDataProvider : ContainerDettaglioTypeDataProviderAbstractClass
	{
		public override string IdentificativoContainerDettaglio => "0";

		public override IReadOnlyCollection<SigilloTypeDataProviderAbstractClass> SigilloDettaglio => [new PlaceholderSigilloTypeDataProvider()];
	}

	sealed class PlaceholderContainerTypeDataProvider : ContainerTypeDataProviderAbstractClass
	{
		public override string IdentificativoContainer => "0";

		public override IReadOnlyCollection<SigilloTypeDataProviderAbstractClass> Sigillo => [new PlaceholderSigilloTypeDataProvider()];
	}

	sealed class PlaceholderSigilloTypeDataProvider : SigilloTypeDataProviderAbstractClass
	{
		public override string IdentificativoSigillo => "0";

		public override string NumeroSigilli => "0000";
	}

	sealed class SigilloTypeDataProvider(string sealId, string sequenceNumber) : SigilloTypeDataProviderAbstractClass
	{
		public override string IdentificativoSigillo => sealId;

		public override string NumeroSigilli => sequenceNumber;
	}

	sealed class MergedContainer
	{
		public MergedContainer(string containerNumber)
		{
			ContainerNumber = Argument.NotNullOrEmpty(containerNumber, nameof(containerNumber));
		}

		public bool TryAddContainer(TemporaryStorageContainer container)
		{
			if (container is null
				|| ContainerNumber != container.ACN_ContainerNumber)
			{
				return false;
			}

			return containers.Add(container);
		}

		public ContainerTypeDataProviderAbstractClass ContainerTypeDataProvider => new MergedContainerTypeDataProvider(this);
		public ContainerDettaglioTypeDataProviderAbstractClass ContainerDettaglioTypeDataProvider => new MergedContainerDettaglioTypeDataProvider(this);

		readonly HashSet<TemporaryStorageContainer> containers = new();
		string ContainerNumber { get; }
		IReadOnlyCollection<SigilloTypeDataProviderAbstractClass> Seals => ContainerHelpers.GetSigilloDettaglio(containers.ToList());

		sealed class MergedContainerTypeDataProvider(MergedContainer mergedContainer) : ContainerTypeDataProviderAbstractClass
		{
			public override string IdentificativoContainer => mergedContainer.ContainerNumber;
			public override IReadOnlyCollection<SigilloTypeDataProviderAbstractClass> Sigillo => mergedContainer.Seals;
		}

		sealed class MergedContainerDettaglioTypeDataProvider(MergedContainer mergedContainer) : ContainerDettaglioTypeDataProviderAbstractClass
		{
			public override string IdentificativoContainerDettaglio => mergedContainer.ContainerNumber;
			public override IReadOnlyCollection<SigilloTypeDataProviderAbstractClass> SigilloDettaglio => mergedContainer.Seals;
		}
	}

	static class ContainerHelpers
	{
		public static IReadOnlyCollection<SigilloTypeDataProviderAbstractClass> GetSigilloDettaglio(List<TemporaryStorageContainer> containers)
		{
			var seals = containers.SelectMany(c => GetSeals(c)).Distinct().ToList();

			return seals.Count == 0
				? [new PlaceholderSigilloTypeDataProvider()]
				: seals.Select((x, index) => new SigilloTypeDataProvider(x, (index + 1).ToString("0000"))).ToList();
		}

		static HashSet<ZString> GetSeals(TemporaryStorageContainer container)
		{
			var seals = new HashSet<ZString>();

			if (!container.ACN_Seal1.IsEmpty)
			{
				seals.Add(container.ACN_Seal1);
			}

			if (!container.ACN_Seal2.IsEmpty)
			{
				seals.Add(container.ACN_Seal2);
			}

			if (!container.ACN_Seal3.IsEmpty)
			{
				seals.Add(container.ACN_Seal3);
			}

			seals.UnionWith(container.AdditionalSeals.Where(x => !x.BK_SealNumber.IsEmpty).Select(x => x.BK_SealNumber));
			return seals;
		}
	}

	sealed class DichiarazioneSemplificataTypeDataProvider(TemporaryStoragePreviousDocument previousDocument, ZInt packQty, ZDecimal quantity) : DichiarazioneSemplificataTypeDataProviderAbstractClass
	{
		public override string DichSempTipoDocumento => previousDocument.CSI_Code;

		public override string DichSempDocumentoPrecedente => previousDocument.CSI_ReferenceNumber;

		public override string DichSempArticoli => previousDocument.CSI_LineNo.ToString();

		public override string DichSempTipoImballaggi => previousDocument.CSI_PackType;

		public override string DichSempNumeroImballaggi => packQty.ToString();

		public override string DichSempUnitaMisura => previousDocument.CSI_UnitOfQuantity;

		public override decimal? DichSempQuantita => quantity;
	}

	#endregion
}
