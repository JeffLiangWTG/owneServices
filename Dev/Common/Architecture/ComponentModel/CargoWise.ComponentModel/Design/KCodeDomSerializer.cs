using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Security;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// A versatile version of the CodeDomSerializer that handles the same assembly being loaded from two different
	/// code bases at design time problem.
	/// </summary>
	[SecurityCritical]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class KCodeDomSerializerBase : CodeDomSerializer
	{
		public sealed override object Serialize(IDesignerSerializationManager manager, object value)
		{
			KCodeDomSerializer serializerInThisAssembly = GetSerializerInSameAssemblyAsThisIfRequired(value);
			if (serializerInThisAssembly != null)
			{
				return serializerInThisAssembly.Serialize(manager, value);
			}
			else
			{
				return SerializeCore(manager, value);
			}
		}
		protected abstract object SerializeCore(IDesignerSerializationManager manager, object value);

		KCodeDomSerializer GetSerializerInSameAssemblyAsThisIfRequired(object value)
		{
			if (value != null &&
				value.GetType().Assembly.FullName == GetType().Assembly.FullName &&
				value.GetType().Assembly.Location != GetType().Assembly.Location)
			{
				Type newSerializerType = value.GetType().Assembly.GetType(GetType().FullName);
				if (newSerializerType != null)
				{
					KCodeDomSerializer newSerializer = (KCodeDomSerializer)Activator.CreateInstance(newSerializerType);
					return newSerializer;
				}
			}
			return null;
		}
	}

	[SecurityCritical]
	public abstract class KCodeDomSerializer : KCodeDomSerializerBase
	{
		public new abstract object Serialize(IDesignerSerializationManager manager, object value);

		protected override object SerializeCore(IDesignerSerializationManager manager, object value)
		{ return Serialize(manager, value); }
	}
}
