using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAContainerDataObjectReader : CusSCAContainerDataObjectReader<CusSCAContainer>
	{
		public CusSCAContainerDataObjectReader(IColumnIndexer oceanBill, HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper, UniversalContainer data, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(oceanBill, hvlvConsolidatorShipmentWrapper, data, logger, factory)
		{ }

		protected override void PopulateCountrySpecificData(CusSCAContainer targetBO, Dictionary<string, ValueSetter> valueSetter)
		{
			base.PopulateCountrySpecificData(targetBO, valueSetter);
			var containerBO = GetColumnIndexer(targetBO);
			SetValue(containerBO, CusSCAContainerSchema.CN_SealNumber, dataObject.Seal, valueSetter);
			if (dataObject.ContainerType != null && containerBO.GetValue(CusSCAContainerSchema.CN_RC_NKContainerType).IsEmpty)
			{
				var containerType = dataObject.ContainerType.GetCodeAsUpperCase();
				SetValue(containerBO, CusSCAContainerSchema.CN_TypeOfContainer, containerType, valueSetter);
				SetValue(containerBO, CusSCAContainerSchema.CN_ContainerSizeOrISOCode, dataObject.ContainerType.ISOCode, valueSetter);
			}
			SetValue(containerBO, CusSCAContainerSchema.CN_ContainerStatus, dataObject.ContainerStatus.GetCodeAsUpperCase(), valueSetter);
			SetValue(containerBO, CusSCAContainerSchema.CN_ShipperOwnedContainer, dataObject.IsShipperOwned, valueSetter);
		}

		public ZString CheckTheFieldsIsChangedAfterCargoReporting()
		{
			CusSCAContainer targetBO = GetExistingBusinessObject();
			if (targetBO == null)
			{
				return ZString.Empty;
			}
			var builder = new ZStringBuilder();

			foreach (var setterInfo in GetValueSetters(targetBO).OfType<IColumnValueSetterInfo>())
			{
				if (fieldsThatShouldNotChangeAfterCargoReporting.Contains(setterInfo.Column.Name))
				{
					var oldValue = setterInfo.Row.GetValue(setterInfo.Column) != null ? setterInfo.Row.GetValue(setterInfo.Column).ToString() : string.Empty;
					var newValue = setterInfo.Value != null ? setterInfo.Value.ToString() : string.Empty;
					var propertyInfo = targetBO.ZPropertyInfoHash.GetPropertySafe(setterInfo.Column.Name);
					if (newValue != null && string.Compare(oldValue, newValue, System.StringComparison.OrdinalIgnoreCase) != 0)
					{
						builder.AppendLine(Res.GetString("68688A4D-E952-4477-A1A7-4DA77829A28D", "There is an attempt to update {0} from '{1}' to '{2}' on this Master/Sub-Master while it has at least one House Bill with an active messaging.",
							propertyInfo != null ? (string)propertyInfo.HumanReadableName : setterInfo.Column.Name, oldValue, newValue));
					}
				}
			}
			return builder.ToString();
		}

		readonly string[] fieldsThatShouldNotChangeAfterCargoReporting = new string[]
		{
			CusSCAContainerSchema.CN_ContainerNumber.Name,
			CusSCAContainerSchema.CN_ContainerMode.Name,
			CusSCAContainerSchema.CN_RC_NKContainerType.Name,
			CusSCAContainerSchema.CN_SealNumber.Name,
			CusSCAContainerSchema.CN_TypeOfContainer.Name,
			CusSCAContainerSchema.CN_ContainerSizeOrISOCode.Name,
		};
	}
}
