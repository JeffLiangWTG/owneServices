using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public interface IAsycudaContainerDataObjectWriter
	{
		Container GetDataObject(AsycudaContainer sourceBO);
	}

	public class AsycudaContainerDataObjectWriter<TContainer> : DataObjectWriter<TContainer, Container>, IAsycudaContainerDataObjectWriter
		where TContainer : AsycudaContainer
	{
		readonly AsycudaManifestHeaderDataObjectWriterHelper helper;
		public AsycudaContainerDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			: base(manager)
		{
			this.helper = helper;
		}

		protected override Container PopulateDataObject(TContainer containerSource)
		{
			var uxml = new Container(writeManager.WriterStrategy)
			{
				ContainerNumber = containerSource.ACN_ContainerNumber,
				Seal = containerSource.ACN_Seal1,
				SecondSeal = containerSource.ACN_Seal2,
				ThirdSeal = containerSource.ACN_Seal3,
				GrossWeight = containerSource.ACN_GoodsWeightInKilos,
				WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms },
				Commodity = new Commodity() { Code = containerSource.ACN_CommodityCode, Description = containerSource.Lookups.CommodityCodes.GetDescriptionFromCode(containerSource.ACN_CommodityCode) },
				StowagePosition = containerSource.ACN_StowageLocation
			};

			var containerType = containerSource.ContainerType;
			if (containerType != null)
			{
				uxml.ContainerType = UniversalDataBuss.DataObjects.Universal.ContainerType.New(containerType);

				var typeCode = uxml.ContainerType.ISOCode;
				var customsCode = typeCode.HasValue ? ZZRefCusMapCombined.MapCW1CodeToCustomsCode(containerSource.Factory, helper.CountryCode, RefCusMapTypeList.Codes.CTYPE, typeCode.Value, ZDateTime.Today) : ZString.Empty;
				if (!customsCode.IsEmpty)
				{
					uxml.ContainerType.ISOCode = customsCode;
				}

				uxml.TotalHeight = containerType.RC_Height;
				uxml.TotalWidth = containerType.RC_Width;
				uxml.TotalLength = containerType.RC_Length;
			}

			uxml.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo() { Key = AsycudaContainer.Schema.ACN_EmptyFullIndicator, Value = containerSource[AsycudaContainer.Schema.ACN_EmptyFullIndicator].ToString() },
				new AddInfo() { Key = AsycudaContainer.Schema.ACN_SealingPartyName, Value = containerSource[AsycudaContainer.Schema.ACN_SealingPartyName].ToString() },
				new AddInfo()
				{
					Key = AsycudaContainer.Schema.ACN_SealingPartyType,
					Value = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(containerSource.Factory, helper.CountryCode, RefCusMapTypeList.Codes.STYPE, containerSource.ACN_SealingPartyType, ZDateTime.Today)
				},
				new AddInfo() { Key = AsycudaContainer.Schema.ACN_NumberOfPackages, Value = containerSource[AsycudaContainer.Schema.ACN_NumberOfPackages].ToString() }
			});

			return uxml;
		}

		Container IAsycudaContainerDataObjectWriter.GetDataObject(AsycudaContainer sourceBO) => GetDataObject(sourceBO as TContainer);
	}
}
