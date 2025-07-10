using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AE.Business;

public class CusContainerLookups : Customs.Business.CusContainerLookups
{
	public CusContainerLookups(CusContainer parent)
		: base(parent)
	{
	}

	public CusContainer Container
	{
		get { return (CusContainer)Parent; }
	}

	public override CodeDescriptionPairList CO_FCL_LCL_NCT_List
	{
		get
		{
			var declaration = Container.Declaration;
			return Factory.GetCachedValue("AE|CusContainerLookups|ContainerModeList|" + (declaration.IsApplicationCodeDubai ? "AEDeclarationApplicationCodeDubai" : "Other"), () =>
			{
				if (declaration.IsApplicationCodeDubai)
				{
					return new ContainerModeList();
				}
				return base.CO_FCL_LCL_NCT_List;
			});
		}
	}
}
