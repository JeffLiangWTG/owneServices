using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusContainerLookups : EU.Business.Declaration.CusContainerLookups
	{
		public CusContainerLookups(CusContainer parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CO_FCL_LCL_NCT_List
		{
			get
			{
				var isAirTransportModeOrDeclarationIsNull = Parent.Declaration?.IsAir ?? true;
				return Factory.GetCachedValue("DE.CusContainerLookups.CO_FCL_LCL_NCT_List|" + isAirTransportModeOrDeclarationIsNull, () =>
				{
					var result = base.CO_FCL_LCL_NCT_List;
					if (isAirTransportModeOrDeclarationIsNull)
					{
						result.AddPairIfNotExist(Core.Constants.ContainerModes.ULD, Core.Constants.ContainerModeDescriptions.ULD);
					}
					return result;
				});
			}
		}
	}
}
