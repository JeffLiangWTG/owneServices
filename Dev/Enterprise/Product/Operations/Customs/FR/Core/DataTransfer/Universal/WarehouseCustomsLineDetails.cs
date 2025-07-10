using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using BondedWarehousingHelper = Enterprise.Customs.EU.Business.BondedWarehousingHelper;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.FR.DataTransfer.Universal
{
	public class WarehouseCustomsLineDetails : EU.DataTransfer.Universal.WarehouseCustomsLineDetails
	{
		public WarehouseCustomsLineDetails(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, WarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail, UniversalDataBuss.DataObjects.Universal.Shipment shipment) : base(factory, invoiceLine, fallbackDetail, shipment)
		{
		}

		public ZString? DeclarationEntryStyle => shipment?.MessageSubType?.Code;

		public ZString? EntryInstructionSubStyle => shipment?.EntryInstructionCollection?.FirstOrDefault(x => x.Link != null && x.Link == InvoiceLine?.EntryInstructionLink)?.SubStyle?.Code;

		public ZString? PreviousDocumentNature
		{
			get
			{
				var previousDocs = InvoiceLine?.CustomsSupportingInformationCollection?.Where(doc => (doc.Category?.Code ?? ZString.Empty) == CusSupportingInfoTypeList.Codes.PreviousDocument) ?? Enumerable.Empty<CustomsSupportingInformation>();
				return previousDocs.Any() ? (previousDocs.Count() == 1 ? previousDocs.Single().Type?.Code : DeclarationEntryStyle) : null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Reference")]
		public ZString? PreviousDocumentReference
		{
			get
			{
				var previousDocs = InvoiceLine?.CustomsSupportingInformationCollection?.Where(doc => (doc.Category?.Code ?? ZString.Empty) == CusSupportingInfoTypeList.Codes.PreviousDocument) ?? Enumerable.Empty<CustomsSupportingInformation>();
				return previousDocs.Any() ? (previousDocs.Count() == 1 ? previousDocs.Single().ReferenceNumber : new ZString("REFERENCES MULTIPLES")) : null;
			}
		}

		protected override bool IsOutward
		{
			get
			{
				if (isOutward == null)
				{
					isOutward = new RefCusProcedure.Loader(factory).LoadTop1FromFullCodeCurrentPlusPreviousPlusConcession(InvoiceLine.Procedure.GetValueOrDefault(), CountryCode, ZDateTime.Today)?.IsOutOfWarehouse() ?? false;
				}
				return isOutward.Value;
			}
		}
		bool? isOutward;

		protected override List<UniversalAddInfo> GetAddInfosApplicableForWarehousing()
		{
			var addInfos = base.GetAddInfosApplicableForWarehousing();

			var estimatedDutyBreakdownAddInfoValue = InvoiceLine.AddInfoCollection?.FirstOrDefault(x => x.Key.GetValueOrDefault() == BondedWarehousingHelper.Constants.EstimatedDutyBreakdown)?.Value?.ToString();
			if (!string.IsNullOrEmpty(estimatedDutyBreakdownAddInfoValue))
			{
				addInfos.Add(UniversalAddInfo.New(BondedWarehousingHelper.Constants.EstimatedDutyBreakdown, estimatedDutyBreakdownAddInfoValue));
			}

			var estimatedVATBreakdownAddInfoValue = InvoiceLine.AddInfoCollection?.FirstOrDefault(x => x.Key.GetValueOrDefault() == BondedWarehousingHelper.Constants.EstimatedVATBreakdown)?.Value?.ToString();
			if (!string.IsNullOrEmpty(estimatedVATBreakdownAddInfoValue))
			{
				addInfos.Add(UniversalAddInfo.New(BondedWarehousingHelper.Constants.EstimatedVATBreakdown, estimatedVATBreakdownAddInfoValue));
			}

			var estimatedOtherTaxesBreakdownAddInfoValue = InvoiceLine.AddInfoCollection?.FirstOrDefault(x => x.Key.GetValueOrDefault() == BondedWarehousingHelper.Constants.EstimatedOtherTaxesBreakdown)?.Value?.ToString();
			if (!string.IsNullOrEmpty(estimatedOtherTaxesBreakdownAddInfoValue))
			{
				addInfos.Add(UniversalAddInfo.New(BondedWarehousingHelper.Constants.EstimatedOtherTaxesBreakdown, estimatedOtherTaxesBreakdownAddInfoValue));
			}

			var previousDoc = InvoiceLine.CustomsSupportingInformationCollection?.FirstOrDefault(s => s.Category != null && s.Category.Code.HasValue && s.Category.Code.Value == "PRE");
			if (previousDoc is not null)
			{
				if (previousDoc.Type != null)
				{
					addInfos.Add(UniversalAddInfo.New(BondedWarehousingHelper.Constants.PreviousDocCode, previousDoc.Type.Code.Value));
				}

				addInfos.Add(UniversalAddInfo.New(BondedWarehousingHelper.Constants.PreviousDocReference, previousDoc.ReferenceNumber.Value));
			}

			addInfos.Add(UniversalAddInfo.New(BondedWarehousingHelper.Constants.CountryOfDestination, shipment.GoodsDestination ?? ZString.Empty));

			var entryHeader = shipment.EntryHeaderCollection?.FirstOrDefault(s => s.EntryInstructionLink.HasValue && s.EntryInstructionLink.Value == base.InvoiceLine.EntryInstructionLink);
			if (entryHeader != null)
			{
				var guaranteedAmount = entryHeader.AddInfoCollection?.FirstOrDefault(s => s.Key.HasValue && s.Key.Value == "GuaranteedAmount");
				if (guaranteedAmount is not null)
				{
					addInfos.Add(UniversalAddInfo.New(BondedWarehousingHelper.Constants.GuaranteedAmount, guaranteedAmount.Value.Value));
				}

				var exitDate = entryHeader.AddInfoCollection?.FirstOrDefault(s => s.Key.HasValue && s.Key.Value == BondedWarehousingHelper.Constants.ExitDate);
				if (exitDate is not null)
				{
					addInfos.Add(UniversalAddInfo.New(BondedWarehousingHelper.Constants.ExitDate, exitDate.Value.Value));
				}

				if (entryHeader.EntryStatus != null && entryHeader.EntryStatus.Code.HasValue && !entryHeader.EntryStatus.Code.Value.IsEmpty)
				{
					addInfos.Add(UniversalAddInfo.New(BondedWarehousingHelper.Constants.EntryStatus, entryHeader.EntryStatus.Description ?? entryHeader.EntryStatus.Code.Value));
				}

				var entryDescription = entryHeader.AddInfoCollection?.FirstOrDefault(s => s.Key.HasValue && s.Key.Value == BondedWarehousingHelper.Constants.EntryStatusDescription);
				if (entryDescription is not null && !entryDescription.Value.Value.IsEmpty)
				{
					addInfos.Add(UniversalAddInfo.New(BondedWarehousingHelper.Constants.EntryStatusDescription, entryDescription.Value.Value));
				}
			}

			var entryInstruction = shipment.EntryInstructionCollection?.FirstOrDefault(s => s.Link == base.InvoiceLine.EntryInstructionLink);
			if (entryInstruction is not null)
			{
				var authorization = entryInstruction.CustomsReferenceCollection?.FirstOrDefault(s => s.Type != null && s.Type.Code.HasValue && s.Type.Code.Value == "AUT");
				if (authorization is not null)
				{
					addInfos.Add(UniversalAddInfo.New(BondedWarehousingHelper.Constants.Authorization, authorization.Reference.Value));
				}
			}

			return addInfos;
		}
	}
}
