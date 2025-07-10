using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using RefCusCodeList = Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsCustomsEntryIntegrator
	{
		void CopyCustomsEntry(CusEntryHeader entryHeader);
	}

	public class NctsBillCustomsEntryIntegrator : INctsCustomsEntryIntegrator
	{
		public NctsBillCustomsEntryIntegrator(NctsBill houseConsignment)
		{
			this.houseConsignment = Argument.NotNull(houseConsignment, nameof(houseConsignment));
			nctsHeader = Argument.NotNull(houseConsignment.Header, nameof(houseConsignment.Header));
		}

		void INctsCustomsEntryIntegrator.CopyCustomsEntry(CusEntryHeader entryHeader)
		{
			Argument.NotNull(entryHeader, nameof(entryHeader));
			Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));

			var targetColletion = houseConsignment.GoodsItems;
			var oldValue = targetColletion.CopyLastGoodsItemToNewLines;
			targetColletion.CopyLastGoodsItemToNewLines = false;

			entryHeader
				.MergedLines
				.Cast<CusEntryLine>()
				.OrderBy(entryLine => entryLine.CL_LineNumber)
				.ForEach(entryLine => CopyIntoNewGoodsItem(entryLine));

			if (!IsInPhase5TransitionPeriod)
			{
				AddNewN830MovementReferenceNumberIfExport(entryHeader, houseConsignment.PreviousDocuments);
			}

			targetColletion.CopyLastGoodsItemToNewLines = oldValue;
		}

		void CopyIntoNewGoodsItem(CusEntryLine entryLine)
		{
			var goodsItem = houseConsignment.GoodsItems.AddNew();

			goodsItem.BY_Description = entryLine.GoodsDescription.Left(goodsItem.BY_DescriptionInfo.MaxLength);
			goodsItem.BY_GrossWeight = entryLine.GrossWeight.InKilogramsSafe;
			goodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			goodsItem.BY_NetWeight = entryLine.EffectiveNetWeight.InKilogramsSafe;
			goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			goodsItem.BY_HarmonisedTariff = entryLine.Tariff.Left(goodsItem.BY_HarmonisedTariffInfo.MaxLength);
			CopyCustomsSecondQuantity(entryLine, goodsItem);
			CopyUNDGs(entryLine, goodsItem, houseConsignment.Factory);
			goodsItem.BY_CusC4Number = entryLine.CusNumber;
			CopyCustomsQuantity(entryLine, goodsItem);
			goodsItem.BY_MonetaryValue = entryLine.TotalLinePriceInLocalCurrency;

			if (IsInPhase5TransitionPeriod && AddNewN830MovementReferenceNumberIfExport(entryLine.Header, goodsItem.PreviousDocuments) is PreviousDocument n830PreviousDocument)
			{
				AssignItemNumberToN830PreviousDocument(n830PreviousDocument, entryLine);
			}

			CreatePackagingDetails(entryLine, goodsItem);
		}

		protected virtual void CopyCustomsSecondQuantity(CusEntryLine entryLine, NctsDepartureCargoDesc goodsItem)
		{
			goodsItem.BY_CustomsSecondQuantity = entryLine.SupplementaryQuantity;
			goodsItem.BY_CustomsSecondUnitQty = entryLine.SupplementaryUQ.Left(goodsItem.BY_CustomsSecondUnitQtyInfo.MaxLength);
		}

		static void CopyCustomsQuantity(CusEntryLine entryLine, NctsDepartureCargoDesc goodsItem)
		{
			var areAllInvoiceLinesCustomsUnitQtyKGM = !entryLine
				.InvoiceLines
				.Cast<JobComInvoiceLine>()
				.Any(x => x.JI_CustomsUnitQty != RefCusCodeList.CustomsUq.Weight.Kilogram);

			if (areAllInvoiceLinesCustomsUnitQtyKGM)
			{
				goodsItem.CustomsFirstQuantityInKilograms = entryLine.CustomsQuantity;
			}
		}

		static void CopyUNDGs(CusEntryLine entryLine, NctsDepartureCargoDesc goodsItem, BusinessObjectFactory factoryToCloneIn)
		{
			foreach (var undg in entryLine.UNDGs)
			{
				var clonedUNDG = (UNDGDataItem)new NctsDeepCloneStrategy(undg, goodsItem.PK, factoryToCloneIn).Clone();
				goodsItem.UNDGs.Add(clonedUNDG);
			}
		}

		PreviousDocument AddNewN830MovementReferenceNumberIfExport(
			CusEntryHeader header,
			Customs.Business.ICusSupportingInfoCollection<PreviousDocument> documentCollection)
		{
			if (header is not { IsExport: true, MovementReferenceNumber: { IsEmpty: false } movementReferenceNumber })
			{
				return null;
			}

			var previousDocument = documentCollection.AddNew();
			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			previousDocument.CSI_ReferenceNumber = movementReferenceNumber;

			return previousDocument;
		}

		protected virtual bool ShouldAssignItemNumberToN830PreviousDocument => true;

		void AssignItemNumberToN830PreviousDocument(PreviousDocument prevDoc, CusEntryLine entryLine)
		{
			if (!ShouldAssignItemNumberToN830PreviousDocument || entryLine is null)
			{
				return;
			}

			prevDoc.CSI_ItemNumber = entryLine.CL_LineNumber;
		}

		void CreatePackagingDetails(CusEntryLine entryLine, NctsDepartureCargoDesc goodsItem) => CreatePackagingDetailsCore(entryLine, goodsItem);

		protected virtual void CreatePackagingDetailsCore(CusEntryLine entryLine, NctsDepartureCargoDesc goodsItem)
		{
			var packagingDetailCollection = entryLine.PackagingDetails;

			foreach (var packagingDetail in packagingDetailCollection.Where(x => x.Package != null))
			{
				var basePackage = packagingDetail.Package;

				var goodsItemPackage = goodsItem.Packages.AddNew();
				goodsItemPackage.B5_UnitType = basePackage.CW_PackType;
				goodsItemPackage.B5_UnitCount = new ZLong(packagingDetail.CHC_NumberOfPacks);
				goodsItemPackage.B5_MarksAndNumbers = basePackage.CW_MarksAndNos;

				var container = basePackage.PackingGroup?.Container;
				if (container != null)
				{
					var headerContainer = CreateOrUpdateHeaderContainer(container);
					goodsItemPackage.ContainersPivot.AddPivotFor(headerContainer);
				}
			}
		}

		NctsDepartureHeaderContainer CreateOrUpdateHeaderContainer(Customs.Business.BaseCusContainer container)
		{
			var headerContainer = GetOrCreateNewDepartureHeaderContainer(container.CO_ContainerNumber);
			var sealSetter = new NctsDepartureHeaderContainerSealsSetter(headerContainer);
			sealSetter.SetInFirstAvailableSlot(container.CO_Seal);
			sealSetter.SetInFirstAvailableSlot(container.CO_SecondSeal);
			return headerContainer;
		}

		NctsDepartureHeaderContainer GetOrCreateNewDepartureHeaderContainer(ZString containerNumber)
		{
			var existingContainer = nctsHeader.DepartureHeaderContainers
				.Cast<NctsDepartureHeaderContainer>()
				.FirstOrDefault(x => x.BC_ContainerNum == containerNumber);
			if (existingContainer != null)
			{
				return existingContainer;
			}

			var newContainer = nctsHeader.DepartureHeaderContainers.AddNew();
			newContainer.BC_Mode = Core.Constants.ContainerModes.Containerised;
			newContainer.BC_ContainerNum = containerNumber;
			return newContainer;
		}

		bool IsInPhase5TransitionPeriod => houseConsignment.IsInPhase5TransitionPeriod;

		readonly NctsBill houseConsignment;
		readonly NctsHeader nctsHeader;
	}
}
