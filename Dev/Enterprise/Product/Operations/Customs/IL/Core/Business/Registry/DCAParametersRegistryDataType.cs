using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IL.Business
{
	[RegistryEditor("Enterprise.Customs.IL.GUI.DCAParametersRegistryEditor, Enterprise.Customs.IL.GUI")]
	public class DCAParametersRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DCAParameters>
	{
		protected override DCAParameters CloneValue(DCAParameters value)
		{
			return (DCAParameters)value.Clone(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), value.Factory);
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo() => new DCAParametersRegistryEditorInfo();
	}
}
