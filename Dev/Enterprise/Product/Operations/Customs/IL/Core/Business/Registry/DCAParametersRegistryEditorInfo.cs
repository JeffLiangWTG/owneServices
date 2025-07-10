using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IL.Business
{
	[RegistryEditor("Enterprise.Customs.IL.GUI.DCAParametersRegistryEditor, Enterprise.Customs.IL.GUI")]
	public class DCAParametersRegistryEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited => typeof(DCAParametersRegistryDataType);
	}
}
