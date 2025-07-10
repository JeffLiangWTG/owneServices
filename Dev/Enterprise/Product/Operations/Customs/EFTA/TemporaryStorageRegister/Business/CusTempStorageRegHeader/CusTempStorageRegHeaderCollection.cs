using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

[ModuleID(ModuleId.TempStorageRegister)]
public class CusTempStorageRegHeaderCollection<T> : CusTempStorageRegHeaderCollection
	where T : CusTempStorageRegHeader
{
	public CusTempStorageRegHeaderCollection(BusinessObjectFactory factory, params ZString[] appCodes)
		: base(factory, appCodes)
	{
	}

	public new T this[int i] => (T)base[i];

	public new T AddNew() => (T)base.AddNew();
}

public abstract class CusTempStorageRegHeaderCollection : ActiveBusinessObjectCollection<CusTempStorageRegHeader>
{
	protected CusTempStorageRegHeaderCollection(BusinessObjectFactory factory, params ZString[] appCodes)
		: base(factory, new ZQuery(CusTempStorageRegHeaderSchema.SRH_AppCode, appCodes))
	{
		this.appCodes = appCodes;
	}
	readonly ZString[] appCodes;

	protected override void SetDefaultsForNewElementCore(CusTempStorageRegHeader newElement)
	{
		base.SetDefaultsForNewElementCore(newElement);
		if (appCodes.Length == 1)
		{
			newElement.SRH_AppCode = appCodes[0];
		}
	}
}
