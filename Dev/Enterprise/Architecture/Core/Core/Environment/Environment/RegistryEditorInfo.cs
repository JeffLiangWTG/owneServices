using System;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class RegistryEditorInfo : IRegistryEditorInfo
	{
		public abstract Type BaseDataTypeToBeEdited { get; }
	}
}
