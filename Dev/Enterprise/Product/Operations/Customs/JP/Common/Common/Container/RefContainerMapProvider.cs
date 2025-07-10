using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Common;

public class RefContainerMapProvider : IRefContainerMapProvider
{
	public UsageRequirement IsUsageNeeded => UsageRequirement.NotRequire;

	public ICodeDescriptionPairList GetCodeList(BusinessObjectFactory factory, ZString usage)
	{
		return factory.GetCachedValue<ICodeDescriptionPairList>($"JP-RefContainerMapCodeList-{ZDateTime.Today}", () =>
		{
			var helper = new ContainerHelper(factory);

			var containerLengthCodeList = helper.ContainerLengthList;
			var containerHeightCodeList = helper.ContainerHeightList;
			var containerTypeCodeList = helper.ContainerTypeList;

			var combinedContainerCodeList = new CodeDescriptionPairList();

			foreach (var type in containerTypeCodeList)
			{
				foreach (var length in containerLengthCodeList)
				{
					foreach (var height in containerHeightCodeList)
					{
						var length_height_type_code = length.ZZD_Code + height.ZZD_Code + type.ZZD_Code;
						var type_length_height_description = type.ZZD_Description + " - 長さ" + length.ZZD_Description + " - 高さ" + height.ZZD_Description;
						combinedContainerCodeList.AddPairIfNotExist(length_height_type_code, type_length_height_description);
					}
				}
			}

			combinedContainerCodeList.Sort();
			return combinedContainerCodeList;
		});
	}

	public ZString GetDefaultCustomsCode(BusinessObjectFactory factory, ZString containerType) => ZString.Empty;

	public ICodeDescriptionPairList GetUsageList(BusinessObjectFactory factory) => new CodeDescriptionPairList();
}
