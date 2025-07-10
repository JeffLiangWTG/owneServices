using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaContainerDataObjectReader : BaseContainerDataObjectReader<AsycudaContainer>
	{
		public AsycudaContainerDataObjectReader(Container containerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaManifestHeader header)
			: base(containerDataObject, logger, factory)
		{
			this.header = Argument.NotNull(header, "header");
		}
		readonly AsycudaManifestHeader header;

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(AsycudaContainer containerBO)
		{
			var builder = new ZStringBuilder();
			if (!dataObject.ContainerNumber.HasValue || dataObject.ContainerNumber.Value.IsEmpty)
			{
				builder.Append(Res.GetString("30F86112-7767-4940-A335-F9E833C3D773", "{0} must not be empty.", "ContainerNumber"));
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}

		protected override AsycudaContainer GetExistingBusinessObject()
		{
			AsycudaContainer result = null;
			if (dataObject.ContainerNumber.HasValue)
			{
				var query = new ZQuery(AsycudaContainerSchema.ACN_AMA_Manifest, header.PK);
				query.AddToFilter(AsycudaContainerSchema.ACN_ContainerNumber, dataObject.ContainerNumber.Value);
				query.FetchOnlyFromLocalCache = !header.IsInDatabase;
				result = factory.Load<AsycudaContainer>(query).OrderBy(x => x.PK).FirstOrDefault();
			}
			return result;
		}

		protected override AsycudaContainer GetNewBusinessObject()
		{
			return header.Containers.AddNew();
		}

		protected override void PopulateBusinessObject(AsycudaContainer containerBO)
		{
			var containerRow = GetColumnIndexer(containerBO);
			SetValue(containerRow, AsycudaContainerSchema.ACN_ContainerNumber, dataObject.ContainerNumber);
			SetValue(containerRow, AsycudaContainerSchema.ACN_Seal1, dataObject.Seal);
			SetValue(containerRow, AsycudaContainerSchema.ACN_Seal2, dataObject.SecondSeal);
			SetValue(containerRow, AsycudaContainerSchema.ACN_Seal3, dataObject.ThirdSeal);
			if (dataObject.GrossWeight.HasValue)
			{
				SetValue(containerRow, AsycudaContainerSchema.ACN_GoodsWeight, dataObject.GrossWeight);
				SetValue(containerRow, AsycudaContainerSchema.ACN_GoodsWeightUQ, Core.Constants.Weight.Kilograms);
			}
			SetValue(containerRow, AsycudaContainerSchema.ACN_CommodityCode, dataObject.Commodity);
			SetValue(containerRow, AsycudaContainerSchema.ACN_StowageLocation, dataObject.StowagePosition);
			FillContainerType(containerRow);
			SetValue(containerRow, AsycudaContainerSchema.ACN_EmptyFullIndicator, dataObject.AddInfoCollection.GetZStringValue(AsycudaContainer.Schema.ACN_EmptyFullIndicator));
			SetValue(containerRow, AsycudaContainerSchema.ACN_SealingPartyName, dataObject.AddInfoCollection.GetZStringValue(AsycudaContainer.Schema.ACN_SealingPartyName));
			SetValue(containerRow, AsycudaContainerSchema.ACN_SealingPartyType, dataObject.AddInfoCollection.GetZStringValue(AsycudaContainer.Schema.ACN_SealingPartyType));
			SetValue(containerRow, AsycudaContainerSchema.ACN_NumberOfPackages, dataObject.AddInfoCollection.GetZIntValue(AsycudaContainer.Schema.ACN_NumberOfPackages));
		}

		void FillContainerType(IColumnIndexer containerRow)
		{
			if (dataObject.ContainerType != null)
			{
				var containerTypeCode = dataObject.ContainerType.Code;
				RefContainer containerType = null;
				if (containerTypeCode.HasValue)
				{
					var queryContainerTypeCode = new ZQuery();
					queryContainerTypeCode.AddToFilter(RefContainerSchema.RC_Code, containerTypeCode);
					containerType = factory.Load<RefContainer>(queryContainerTypeCode).OrderBy(x => x.RC_Code).FirstOrDefault();
				}
				if (containerType == null &&
					(dataObject.ContainerType.ISOCode.HasValue || dataObject.TotalHeight.HasValue || dataObject.TotalWidth.HasValue || dataObject.TotalLength.HasValue))
				{
					var query = new ZQuery();
					if (dataObject.ContainerType.ISOCode.HasValue)
					{
						query.AddToFilter(RefContainerSchema.RC_ISOType, dataObject.ContainerType.ISOCode);
					}
					if (dataObject.TotalHeight.HasValue)
					{
						query.AddToFilter(RefContainerSchema.RC_Height, dataObject.TotalHeight);
					}
					if (dataObject.TotalWidth.HasValue)
					{
						query.AddToFilter(RefContainerSchema.RC_Width, dataObject.TotalWidth);
					}
					if (dataObject.TotalLength.HasValue)
					{
						query.AddToFilter(RefContainerSchema.RC_Length, dataObject.TotalLength);
					}
					containerType = factory.Load<RefContainer>(query).OrderBy(x => x.RC_Code).FirstOrDefault();
				}
				if (containerType != null)
				{
					SetValue(containerRow, AsycudaContainerSchema.ACN_RC_ContainerType, containerType.PK);
				}
			}
		}
	}
}
