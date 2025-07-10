using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.ServiceTasks
{
	public class NctsIE43ArrivalItemsParser
	{
		public NctsIE43ArrivalItemsParser(NctsHeader nctsHeader, IE43Wrapper ie43Wrapper)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			this.ie43Wrapper = Argument.NotNull(ie43Wrapper, nameof(ie43Wrapper));
		}

		public void Parse()
		{
			if (nctsHeader.IsArrivalMovement)
			{
				nctsHeader.ArrivalMovementHeader.BM_TransportAtDeparture = ie43Wrapper.MeansOfTransportAtDepartureIdentity;
				nctsHeader.ArrivalMovementHeader.BM_RN_NKTransportAtDepartureCountry = ie43Wrapper.MeansOfTransportAtDepartureNationality;
				nctsHeader.ArrivalMovementHeader.BM_GrossWeight = ie43Wrapper.TotalGrossMass;
				nctsHeader.ArrivalMovementHeader.BM_GrossWeightUQ = ie43Wrapper.TotalGrossMassUQ;
				SetSeals();
				SetArrivedGoodsItems();
			}
		}

		void SetArrivedGoodsItems()
		{
			nctsHeader.ArrivalMovementHeader.GoodsItems.DeleteAll();

			if (ie43Wrapper.GoodsItems != null)
			{
				foreach (var goodsItemWrapper in ie43Wrapper.GoodsItems)
				{
					var goodsItem = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
					goodsItem.BY_LineNo = goodsItemWrapper.ItemNumber;
					goodsItem.BY_HarmonisedTariff = goodsItemWrapper.CommodityCode;
					goodsItem.BY_Type = goodsItemWrapper.DeclarationType;
					goodsItem.BY_Description = goodsItemWrapper.DescriptionOfGoods.Left(goodsItem.BY_DescriptionInfo.MaxLength);
					goodsItem.BY_GrossWeight = goodsItemWrapper.GrossWeight;
					goodsItem.BY_GrossWeightUnit = goodsItemWrapper.GrossWeightUQ;
					goodsItem.BY_NetWeight = goodsItemWrapper.NetWeight;
					goodsItem.BY_NetWeightUnit = goodsItemWrapper.NetWeightUQ;
					goodsItem.BY_RN_NKCountryOfDispatch = goodsItemWrapper.CountryOfDispatch;
					goodsItem.BY_RN_NKCountryOfDestination = goodsItemWrapper.CountryOfDestination;

					SetProducedDocumentsCertificates(goodsItem, goodsItemWrapper.ProducedDocumentsCertificates);
					SetSpecialMentions(goodsItem, goodsItemWrapper.SpecialMentions);
					SetContainers(goodsItem, goodsItemWrapper.Containers);
					SetPackages(goodsItem, goodsItemWrapper.Packages);
					SetSgiCodes(goodsItem, goodsItemWrapper.SgiCodes);
				}
			}
		}

		void SetSeals()
		{
			//sealsNumber = cusdecMessage.CNT.GetCNTControlValue("16"); // TODO maybe check count?

			if (ie43Wrapper.Seals != null)
			{
				foreach (var seal in ie43Wrapper.Seals)
				{
					var sealItem = nctsHeader.ArrivalMovementHeader.Seals.AddNew();
					sealItem.CY_Data = seal;
				}
			}
		}

		void SetProducedDocumentsCertificates(NctsCommonCargoDesc goodsItem, IReadOnlyCollection<IE43ProducedDocumentsCertificateWrapper> producedDocumentsCertificates)
		{
			if (producedDocumentsCertificates != null)
			{
				foreach (var sdWrapper in producedDocumentsCertificates)
				{
					var sd = goodsItem.SupportingDocuments.AddNew();
					sd.CSI_Code = sdWrapper.Code;
					sd.CSI_ReferenceNumber = sdWrapper.ReferenceNumber;
					sd.CSI_Description = sdWrapper.Description;
				}
			}
		}

		void SetSpecialMentions(NctsCommonCargoDesc goodsItem, IReadOnlyCollection<IE43SpecialMentionWrapper> specialMentions)
		{
			if (specialMentions != null)
			{
				foreach (var smWrapper in specialMentions)
				{
					var sm = goodsItem.AdditionalInfos.AddNew();
					sm.CSI_Code = smWrapper.Code;
					sm.CSI_Description = smWrapper.Description;
					sm.CSI_NctsExportFromEC = smWrapper.NctsExportFromEC;
					sm.CSI_RN_NKCountryCode = smWrapper.CountryCode;
				}
			}
		}

		void SetContainers(NctsArrivalAndUnloadingCargoDesc goodsItem, IReadOnlyCollection<ZString> containers)
		{
			if (containers != null)
			{
				foreach (var containerNumber in containers)
				{
					var container = goodsItem.Containers.AddNew();
					container.ContainerNumber = containerNumber;
				}
			}
		}

		void SetPackages(NctsCommonCargoDesc goodsItem, IReadOnlyCollection<IE43PackageWrapper> packages)
		{
			if (packages != null)
			{
				foreach (var packageWrapper in packages)
				{
					var package = goodsItem.Packages.AddNew();
					package.B5_MarksAndNumbers = packageWrapper.MarksAndNumbers;
					package.B5_UnitType = packageWrapper.UnitType;
					package.B5_UnitCount = !packageWrapper.UnitCount.IsEmpty ? packageWrapper.UnitCount : packageWrapper.NumberOfPieces;
				}
			}
		}

		void SetSgiCodes(NctsCommonCargoDesc goodsItem, IReadOnlyCollection<IE43SgiCodeWrapper> sgiCodes)
		{
			if (sgiCodes != null)
			{
				foreach (var sgiWrapper in sgiCodes)
				{
					var sgi = goodsItem.AdditionalInfos.AddNew();
					sgi.CSI_Code = sgiWrapper.Code;
					sgi.CSI_Description = sgiWrapper.Description;
				}
			}
		}

		readonly NctsHeader nctsHeader;
		readonly IE43Wrapper ie43Wrapper;
	}
}
