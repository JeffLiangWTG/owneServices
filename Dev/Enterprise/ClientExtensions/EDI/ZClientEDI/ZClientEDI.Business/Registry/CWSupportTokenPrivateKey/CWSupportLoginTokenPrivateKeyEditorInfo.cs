using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace ZClientEDI.Business.Registry
{
	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.CWSupportLoginTokenPrivateKeyRegistryEditor, ZClientEDI")]
	public class CWSupportLoginTokenPrivateKeyEditorInfo : IRegistryEditorInfo
	{
		public Type BaseDataTypeToBeEdited => typeof(BinaryRegistryDataType);
	}
}
