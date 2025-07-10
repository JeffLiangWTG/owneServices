using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusInBondContainerLookups : Customs.Business.CusInBondContainerLookups
	{
		public CusInBondContainerLookups(CusInBondContainer parent)
			: base(parent)
		{
		}

		public new CusInBondContainer Parent => base.Parent as CusInBondContainer;

		public CodeDescriptionPairList CusContainers => Factory.GetCachedValue(
			"CusInBondContainerLookups.CusContainer",
			() =>
			{
				var result = new CodeDescriptionPairList();
				Parent.MoveDetail?.MoveHeader?.EntryInstruction?.JobDeclaration?.CusContainers.Cast<CusContainer>()
					.ForEach(contianer =>
					{
						result.AddPairIfNotExist(contianer.CO_ContainerNumber, contianer.CO_ContainerNumber);
					});
				return result;
			},
			CacheStalenessPolicy.StaleWhenDataTableChanges(CusContainer.Schema.TableName, Factory));
	}
}
