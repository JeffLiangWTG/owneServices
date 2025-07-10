using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Messaging
{
	public partial class ContainerPackModeCodeList
	{
		public static CodeDescriptionPairList GetSupportedContainerPackModeCodeList(BusinessObjectFactory factory, ZString transportMode)
		{
			return factory.GetCachedValue("ContainerModeCodeList" + transportMode, () =>
			{
				var result = new ContainerPackModeCodeList();
				switch (transportMode)
				{
					case Core.Constants.TransportModes.Sea:
						result.RemoveCode(ContainerPackModeCodeList.Codes.UL);
						break;
					case Core.Constants.TransportModes.Air:
						result.RemoveCode(ContainerPackModeCodeList.Codes.FC);
						result.RemoveCode(ContainerPackModeCodeList.Codes.LC);
						break;
				}
				return result;
			});
		}
	}
}
