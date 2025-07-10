using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using static Enterprise.Customs.DataTransfer.Universal.Constants;
using BondedWarehousingHelper = Enterprise.Customs.EU.Business.BondedWarehousingHelper;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class WarehouseCustomsLineDetails : WarehouseCustomsLineDetailsWithEntryInstruction
	{
		public WarehouseCustomsLineDetails(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, Customs.DataTransfer.Universal.WarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail, UniversalDataBuss.DataObjects.Universal.Shipment shipment)
			: base(factory, invoiceLine, fallbackDetail, shipment)
		{
		}

		public new WarehouseCustomsFallbackDetailWithEntryInstruction FallbackDetail => (WarehouseCustomsFallbackDetailWithEntryInstruction)base.FallbackDetail;

		protected override IEnumerable<IWarehouseCustomsLineAddInfo> GetAdditionalAddInfos()
		{
			var commercialChargeCollection = InvoiceLine.CommercialChargeCollection;
			if (commercialChargeCollection != null)
			{
				foreach (var commercialCharge in commercialChargeCollection)
				{
					yield return new WarehouseCustomsLineAddInfoCommercialCharge(commercialCharge);
				}
			}

			var customsReferenceCollection = InvoiceLine.CustomsReferenceCollection;
			if (customsReferenceCollection != null)
			{
				foreach (var customsReference in customsReferenceCollection)
				{
					if (customsReference.Type?.Code?.ToString() == CusCodeDataTypeList.Codes.SupplementaryCode)
					{
						yield return new WarehouseCustomsLineAddInfoSupplementaryCode(customsReference);
					}
				}
			}

			var documents = InvoiceLine.CustomsSupportingInformationCollection?.Where(x => x.Category.GetCodeAsUpperCase() == CusSupportingInfoTypeList.Codes.PreviousDocument && x.Type?.GetCodeAsUpperCase().ToString() == PreviousDocumentCodeList.Codes.IM);
			if (documents != null)
			{
				foreach (var document in documents)
				{
					yield return new WarehouseCustomsLineAddInfoPreviousDocument(document);
				}
			}

			foreach (var authorisation in GetAuthorizations())
			{
				yield return authorisation;
			}
		}

		protected virtual IEnumerable<IWarehouseCustomsLineAddInfo> GetAuthorizations()
		{
			var entryInstruction = RelatedEntryInstruction;
			if (entryInstruction != null)
			{
				var authorisations = entryInstruction.CustomsReferenceCollection?.Where(x => BondedWarehousingHelper.Constants.EntryInstructionAuthorisationAddInfo.Type.Equals(x.Type.GetCodeAsUpperCase()));
				if (authorisations != null)
				{
					foreach (var authorisation in authorisations)
					{
						var subType = authorisation.SubType.GetCodeAsUpperCase();
						switch (subType)
						{
							case CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1:
							case CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2:
							case CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP:
								yield return new WarehouseCustomsLineAddInfoAuthorization(authorisation);
								break;
						}
					}
				}
			}
		}

		EntryInstruction RelatedEntryInstruction => CachedValueHelper.GetValue(ref relatedEntryInstructionCached, () =>
		{
			var entryInstructionId = InvoiceLine.EntryInstructionLink.GetValueOrDefault();
			return entryInstructionId.IsEmpty ? null : shipment?
				.EntryInstructionCollection
				.FirstOrDefault(h => h.Link == entryInstructionId);
		});
		CachedValue<EntryInstruction> relatedEntryInstructionCached;

		protected override ZString? GetStyle() => EntryStyle + RelatedEntryInstruction?.SubStyle.GetCodeAsUpperCase();

		ZString EntryStyle
		{
			get
			{
				if (!entryStyleCached.HasValue)
				{
					entryStyleCached = shipment?.MessageSubType.GetCodeAsUpperCase();
				}
				return entryStyleCached.Value;
			}
		}
		ZString? entryStyleCached;

		protected override List<UniversalAddInfo> GetAddInfosApplicableForWarehousing()
		{
			var addInfos = base.GetAddInfosApplicableForWarehousing();

			addInfos.AddRange(new List<UniversalAddInfo>
			{
				UniversalAddInfo.New(BondedWarehousingHelper.Constants.LinePrice, InvoiceLine.LinePrice?.ToString()),
				UniversalAddInfo.New(BondedWarehousingHelper.Constants.LinePriceCurrency, FallbackDetail.LinePriceCurrency),
				UniversalAddInfo.New(BondedWarehousingHelper.Constants.CountryOfSupply, InvoiceLine.AddInfoCollection?.FirstOrDefault(x => x.Key.GetValueOrDefault() == BondedWarehousingHelper.Constants.CountryOfSupply)?.Value?.ToString()),
				UniversalAddInfo.New(BondedWarehousingHelper.Constants.ValuationCode, InvoiceLine.ValuationCode?.Code?.ToString())
			});

			var grossWeight = InvoiceLine.Weight;
			if (grossWeight.HasValue)
			{
				addInfos.Add(UniversalAddInfo.New(BondedWarehousingHelper.Constants.LineGrossWeight, grossWeight.ToString()));
			}

			var grossWeightUnit = InvoiceLine.WeightUnit?.Code;
			if (!string.IsNullOrWhiteSpace(grossWeightUnit))
			{
				addInfos.Add(UniversalAddInfo.New(BondedWarehousingHelper.Constants.LineGrossWeightUnit, grossWeightUnit.ToString()));
			}

			return addInfos;
		}

		protected override List<UniversalAddInfo> GetThirdQuantityAsAddInfosForWarehousing()
		{
			return new List<UniversalAddInfo>();
		}

		public override ZString? Procedure => base.Procedure.HasValue ? base.Procedure.Value.Left(7) : null;

		public override ZInt? OrderLineNo => IsOutward ? InvoiceLine?.BondedWHSOrderLineNumber : null;

		public override ZString? OrderNumber => IsOutward ? InvoiceLine?.BondedWHSOrderNumber : null;

		public override ZString? CountryOfDestination => InvoiceLine?.AddInfoCollection?.FirstOrDefault(x => (string)x.Key == AddInfoKeys.InvoiceLine.CountryOfDestination)?.Value ?? shipment.GoodsDestination;

		protected override string CountryCode => FallbackDetail.CountryCode;
	}
}
