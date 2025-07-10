using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAContainerDataObjectWriter : CusSCAContainerDataObjectWriter<CusSCAContainer>
	{
		public CusSCAContainerDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{ }

		protected override void PopulateCountrySpecificData(CusSCAContainer bizObj, UniversalXml.Container data, bool keepExistingData)
		{
			base.PopulateCountrySpecificData(bizObj, data, keepExistingData);

			var bizObjRow = (IColumnIndexer)bizObj;

			data.Seal = PopulateValue(data.Seal, keepExistingData, () => bizObjRow.GetValue(CusSCAContainerSchema.CN_SealNumber));
			data.IsShipperOwned = PopulateValue(data.IsShipperOwned, keepExistingData, () => bizObjRow.GetValue(CusSCAContainerSchema.CN_ShipperOwnedContainer));
			data.ContainerStatus = PopulateValue(data.ContainerStatus, keepExistingData,
				() => !bizObjRow.GetValue(CusSCAContainerSchema.CN_ContainerStatus).IsEmpty ? new UniversalXml.CodeDescriptionPair() { Code = bizObjRow.GetValue(CusSCAContainerSchema.CN_ContainerStatus) } : null);
		}

		protected override UniversalXml.ContainerType GetContainerType(CusSCAContainer bizObj)
		{
			var result = base.GetContainerType(bizObj);
			if (result == null)
			{
				var bizObjRow = (IColumnIndexer)bizObj;
				result = ListHelper.GetWithDescription<UniversalXml.ContainerType>(bizObjRow.GetValue(CusSCAContainerSchema.CN_TypeOfContainer), Factory.GetCachedValue<CMRContainerTypesForDataTransfer>());
				result.ISOCode = bizObjRow.GetValue(CusSCAContainerSchema.CN_ContainerSizeOrISOCode);
			}
			return result;
		}

		protected override CargoWise.Integration.ICodeDescriptionPairList GetContainerModes(CusSCAContainer bizObj)
		{
			return bizObj.Lookups.CargoTypes;
		}
	}
}
