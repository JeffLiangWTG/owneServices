using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
namespace Enterprise.Customs.JP.Common;

public class ContainerHelper
{
	public ContainerHelper(BusinessObjectFactory factory)
	{
		this.factory = factory;
	}

	readonly BusinessObjectFactory factory;

	const string OtherInJapanese = "その他";

	public IReadOnlyList<ZZRefCusCodeListCombined> ContainerLengthList => factory.GetCachedValue($"JPContainerLengthList-{ZDateTime.Today}", () => ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerLength, ZDateTime.Today));

	public IReadOnlyList<ZZRefCusCodeListCombined> ContainerHeightList => factory.GetCachedValue($"JPContainerHeightList-{ZDateTime.Today}", () => ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerHeight, ZDateTime.Today));

	public IReadOnlyList<ZZRefCusCodeListCombined> ContainerTypeList => factory.GetCachedValue($"JPContainerTypeList-{ZDateTime.Today}", () => ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerType, ZDateTime.Today));

	public ZString MapContainerTypeToNaccsAcceptedValue(ZString code) => ContainerTypeList.Any(x => x.ZZD_Code == code) ? code : OtherTypeCode;

	public ZString MapContainerSizeToNaccsAcceptedValue(ZString code)
	{
		var lengthCode = code.SubstringSafe(0, 1);
		var heightCode = code.SubstringSafe(1, 1);
		var mappedLengthCode = ContainerLengthList.Any(x => x.ZZD_Code == lengthCode) ? lengthCode : OtherLengthCode;
		var mappedHeightCode = ContainerHeightList.Any(x => x.ZZD_Code == heightCode) ? heightCode : OtherHeightCode;
		return mappedLengthCode + mappedHeightCode;
	}

	public ZString OtherTypeCode => GetContainerList(ContainerTypeList);

	public ZString OtherLengthCode => GetContainerList(ContainerLengthList);

	public ZString OtherHeightCode => GetContainerList(ContainerHeightList);

	ZString GetContainerList(IReadOnlyList<ZZRefCusCodeListCombined> readOnlyList) => readOnlyList.FirstOrDefault(x => x.ZZD_Description.Contains(OtherInJapanese, StringComparison.OrdinalIgnoreCase))?.ZZD_Code ?? ZString.Empty;

	public ZString GetNACCSContainerType(RefContainer containerType)
	{
		var result = ZString.Empty;
		if (containerType != null)
		{
			result = containerType.GetCountrySpecificContainerCode(Core.Constants.CountryCodes.Japan).SubstringSafe(2, 2);
			if (result.IsEmpty)
			{
				if (containerType.RC_IsIso && !containerType.RC_ISOType.IsEmpty)
				{
					result = MapContainerTypeToNaccsAcceptedValue(containerType.RC_ISOType.SubstringSafe(2, 2));
				}
				else
				{
					if (containerType.RC_Code.Length == 4)
					{
						result = MapContainerTypeToNaccsAcceptedValue(containerType.RC_Code.SubstringSafe(2, 2));
					}
					else
					{
						result = OtherTypeCode;
					}
				}
			}
		}
		return result;
	}

	public ZString GetNACCSContainerSize(RefContainer containerType)
	{
		var result = ZString.Empty;

		if (containerType != null)
		{
			result = containerType.GetCountrySpecificContainerCode(Core.Constants.CountryCodes.Japan).Left(2);
			if (result.IsEmpty)
			{
				if (containerType.RC_IsIso && !containerType.RC_ISOType.IsEmpty)
				{
					result = MapContainerSizeToNaccsAcceptedValue(containerType.RC_ISOType.Left(2));
				}
				else
				{
					if (containerType.RC_Code.Length == 4)
					{
						result = MapContainerSizeToNaccsAcceptedValue(containerType.RC_Code.Left(2));
					}
					else
					{
						result = OtherLengthCode + OtherHeightCode;
					}
				}
			}
		}

		return result;
	}
}
