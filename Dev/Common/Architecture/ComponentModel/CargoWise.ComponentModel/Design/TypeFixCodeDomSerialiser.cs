using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Security;
using CargoWise.Common;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// A CodeDomSerializer that fixes serialisation of Type objects. Incorrect Types are deserialised
	/// with fake types. Apply this to a class that has properties that return Type objects.
	/// </summary>
	[SecurityCritical]
	public abstract class TypeFixCodeDomSerializer : KCodeDomSerializer
	{
		protected TypeFixCodeDomSerializer(Type rootType)
		{
			Argument.NotNull(rootType, nameof(rootType));
			RootType = rootType;
		}

		public Type RootType { get; private set; }

		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			if (manager != null)
			{
				var baseClassSerializer = (CodeDomSerializer)manager.GetSerializer(RootType.BaseType, typeof(CodeDomSerializer));
				if (baseClassSerializer != null)
				{
					return baseClassSerializer.Serialize(new MyDesignerSerializationManager(manager), value);
				}
			}
			return null;
		}

		public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
		{
			if (manager != null)
			{
				var baseClassSerializer = (CodeDomSerializer)manager.GetSerializer(RootType.BaseType, typeof(CodeDomSerializer));
				if (baseClassSerializer != null)
				{
					return baseClassSerializer.Deserialize(new MyDesignerSerializationManager(manager), codeObject);
				}
			}
			return null;
		}

		#region CheckAppliedCorrectly

		public static void CheckAppliedCorrectly(Type type)
		{
			Argument.NotNull(type, nameof(type));
			var attributes = TypeDescriptor.GetAttributes(type);
			var attribute = (DesignerSerializerAttribute)attributes[typeof(DesignerSerializerAttribute)];
			Type typeFixSerialiser = null;
			var serialiserType = attribute == null ? null : Type.GetType(attribute.SerializerTypeName);
			var serialiserBaseType = attribute == null ? null : Type.GetType(attribute.SerializerBaseTypeName);
			if (serialiserType != null && typeof(TypeFixCodeDomSerializer).IsAssignableFrom(serialiserType))
			{
				typeFixSerialiser = serialiserType;
			}
			else if (serialiserBaseType != null && typeof(TypeFixCodeDomSerializer).IsAssignableFrom(serialiserBaseType))
			{
				typeFixSerialiser = serialiserBaseType;
			}
			if (typeFixSerialiser == null && type.BaseType != null)
			{
				CheckAppliedCorrectly(type.BaseType);
			}
			if (typeFixSerialiser == null || typeFixSerialiser.IsAbstract)
			{
				throw new ArgumentException(
					"If you apply " + typeof(TypeTypeConverter).FullName + " to a property, you must also apply [" +
					typeof(DesignerAttribute).FullName + "(typeof(" + typeof(TypeFixCodeDomSerializer).FullName + "), typeof(" + typeof(CodeDomSerializer).FullName + "))]" +
					" to the class.");
			}
			var serialiser = (TypeFixCodeDomSerializer)Activator.CreateInstance(typeFixSerialiser);
			var serialiserRootType = type.Assembly.GetType(serialiser.RootType.FullName);
			if (serialiserRootType != null && !serialiserRootType.IsAssignableFrom(type))
			{
				throw new ArgumentException("Serialiser must have the component's type passed into the constructor. Expected type " + type.FullName + " but found " + serialiser.RootType.FullName);
			}
		}

		#endregion

		#region MyDesignerSerializationManager

		class MyDesignerSerializationManager : IDesignerSerializationManager
		{
			readonly IDesignerSerializationManager Inner;

			public MyDesignerSerializationManager(IDesignerSerializationManager inner)
			{
				Argument.NotNull(inner, nameof(inner));
				Inner = inner;
			}

			#region IDesignerSerializationManager Members

			public void ReportError(object errorInformation)
			{
				Inner.ReportError(errorInformation);
			}

			public void RemoveSerializationProvider(IDesignerSerializationProvider provider)
			{
				Inner.RemoveSerializationProvider(provider);
			}

			public event ResolveNameEventHandler ResolveName
			{
				add { Inner.ResolveName += value; }
				remove { Inner.ResolveName -= value; }
			}

			public event EventHandler SerializationComplete
			{
				add { Inner.SerializationComplete += value; }
				remove { Inner.SerializationComplete -= value; }
			}

			public void AddSerializationProvider(IDesignerSerializationProvider provider)
			{
				Inner.AddSerializationProvider(provider);
			}

			public string GetName(object value)
			{
				return Inner.GetName(value);
			}

			public ContextStack Context
			{
				get { return Inner.Context; }
			}

			public void SetName(object instance, string name)
			{
				Inner.SetName(instance, name);
			}

			public object GetSerializer(Type objectType, Type serializerType)
			{
				return Inner.GetSerializer(objectType, serializerType);
			}

			public object CreateInstance(Type type, ICollection arguments, string name, bool addToContainer)
			{
				return Inner.CreateInstance(type, arguments, name, addToContainer);
			}

			public PropertyDescriptorCollection Properties
			{
				get { return PropertyDescriptorCollection.Empty; }
			} //inner.Properties; } }

			public object GetInstance(string name)
			{
				return Inner.GetInstance(name);
			}

			public Type GetType(string typeName)
			{
				var result = Inner.GetType(typeName);
				if (typeName != null && result == null)
				{
					result = new TypeNameHolder(typeName, this);
				}
				return result;
			}

			#endregion

			#region IServiceProvider Members

			public object GetService(Type serviceType)
			{
				return Inner.GetService(serviceType);
			}

			#endregion
		}

		#endregion
	}
}
