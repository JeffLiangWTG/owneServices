using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Warehouse
{
	public class PackageScreeningMethodList
	{
		public CodeDescriptionPairList PackageScreeningMethodCodeDescriptionList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				foreach (ShipmentInspectionType pair in ShipmentInspectionTypesDefaultList.Types)
				{
					list.AddPair(pair.Code, pair.Description);
				}
				return list;
			}
		}

		ShipmentInspectionTypes ShipmentInspectionTypesDefaultList
		{
			get
			{
				if (shipmentInspectionTypesDefaultList == null)
				{
					var originalIATAList = new ShipmentInspectionTypeLists(RegistryFactory.Instance).SystemDefinedList_IATA;
					var uniqueShipmentInsepctionTypesList = new ShipmentInspectionTypeCollection(ZString.Empty);
					foreach (var item in originalIATAList)
					{
						uniqueShipmentInsepctionTypesList.Add(item);
					}

					var systemDefinedListIATACodePairList = originalIATAList.GetCodeDescriptionPairList();
					foreach (ShipmentInspectionType type in new ShipmentInspectionTypeLists(RegistryFactory.Instance).SystemDefinedList_EuropeanUnion)
					{
						if (!systemDefinedListIATACodePairList.ContainsCode(type.Code))
						{
							uniqueShipmentInsepctionTypesList.Add(type);
						}
					}
					shipmentInspectionTypesDefaultList = new ShipmentInspectionTypes(ZString.Empty, uniqueShipmentInsepctionTypesList);
				}
				return shipmentInspectionTypesDefaultList;
			}
		}
		ShipmentInspectionTypes shipmentInspectionTypesDefaultList;
	}
}
