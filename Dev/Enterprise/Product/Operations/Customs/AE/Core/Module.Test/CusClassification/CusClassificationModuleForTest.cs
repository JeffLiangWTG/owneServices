using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.Module.Testing;

public class CusClassificationModuleForTest : CusClassificationModule
{
	public CusClassificationModuleForTest()
	{
	}

	public IFilterControl NewFilterControl
	{
		get
		{
			return GetNewFilterControl();
		}
	}

	public IBusinessObjectCollection NewGridCollection
	{
		get
		{
			return GetNewGridCollection();
		}
	}

	public FilterBusinessObject NewFilterBusinessObject
	{
		get
		{
			return GetNewFilterBusinessObject();
		}
	}
}
