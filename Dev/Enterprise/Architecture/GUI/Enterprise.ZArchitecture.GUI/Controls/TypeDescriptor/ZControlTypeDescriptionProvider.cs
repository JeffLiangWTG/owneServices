using System;
using System.Collections.Generic;
using System.ComponentModel;
using Enterprise.ZArchitecture.Core;

#pragma warning disable SA1003 // Symbols should be spaced correctly

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public class ZControlTypeDescriptionProvider : TypeDescriptionProvider
	{
		public ZControlTypeDescriptionProvider()
			: base(TypeDescriptor.GetProvider(typeof(Component)))
		{
		}

		public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
		{
			ICustomTypeDescriptor result;
			if (DesignModeFinder.IsDesigning)
			{
				result = base.GetTypeDescriptor(objectType, instance);
			}
			else
			{
				if (TypeDescriptorDictionary == null)
				{
					TypeDescriptorDictionary = new Dictionary<Type, ICustomTypeDescriptor>();
				}

				if (!TypeDescriptorDictionary.TryGetValue(objectType, out result))
				{
					result = new ZControlTypeDescriptor(objectType);
					TypeDescriptorDictionary.Add(objectType, result);
				}
			}
			return result;
		}

		[ThreadStatic]
		static Dictionary<Type, ICustomTypeDescriptor> TypeDescriptorDictionary;
	}
}
