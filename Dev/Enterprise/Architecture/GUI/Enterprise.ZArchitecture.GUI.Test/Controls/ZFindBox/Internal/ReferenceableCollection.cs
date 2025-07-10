using System;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	public class ReferenceableCollection : BusinessObjectCollection<ReferenceableDummy>, ICodePropertyNameProvider
	{
		public ReferenceableCollection(BusinessObjectFactory factory) : base(factory) { }

		public Func<Type, string> GetCodePropertyNameForTesting;
		string ICodePropertyNameProvider.GetCodePropertyName(Type type) => GetCodePropertyNameForTesting?.Invoke(type);
	}
}
