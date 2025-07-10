using System;
using System.Reflection;
using System.Reflection.Emit;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.ComponentModel
{
	public sealed class KReflectPropertyDescriptor : ReflectPropertyDescriptor
	{
		public KReflectPropertyDescriptor(Type componentClass, string name, Type type, PropertyInfo propertyInfo, MethodInfo getMethod, MethodInfo setMethod, Attribute[] attributes)
			: base(componentClass, name, type, propertyInfo, getMethod, setMethod, attributes)
		{
			Argument.NotNull(type, nameof(type));
			Argument.NotNull(componentClass, nameof(componentClass));
		}

		public KReflectPropertyDescriptor(Type componentClass, PropertyInfo info)
			: base(componentClass, info.Name, info.PropertyType, info, info.GetGetMethod(true), info.GetSetMethod(true), null)
		{
			Argument.NotNull(componentClass, nameof(componentClass));
			Argument.NotNull(info, nameof(info)); // Suggested By ReviewBot 
		}

		#region GetValue / SetValue

		public override object GetValue(object component)
		{
			if (component != null)
			{
				try
				{
					return Getter != null ? Getter(component) : null;
				}
				catch (InvalidCastException ex)
				{
					throw new TargetException("Wrong component type.", ex);
				}
				catch (Exception ex)
				{
					ex.Data["KReflectPropertyDescriptor.Component"] = component.GetType().FullName;
					ex.Data["KReflectPropertyDescriptor.Name"] = Name;
					throw;
				}
			}

			return null;
		}

		public override void SetValue(object component, object value)
		{
			if (component != null && SetMethodValue != null)
			{
				SetMethodValue.Invoke(component, new object[] { value });
			}
		}

		#endregion

		#region Lightweight Code Gen

		delegate object GetterDelegate(object component);

		static readonly FieldInfo RelocFixupListInfo = typeof(ILGenerator).GetField("m_RelocFixupList", BindingFlags.Instance | BindingFlags.NonPublic);
		static readonly FieldInfo RVAFixupListInfo = typeof(ILGenerator).GetField("m_RVAFixupList", BindingFlags.Instance | BindingFlags.NonPublic);

		[ThreadSafe]
		static readonly int[] emptyIntArray = Array.Empty<int>();

		GetterDelegate Getter
		{
			get
			{
				if (getter == null)
				{
					var dynamicMethod = new DynamicMethod(String.Empty, typeof(object), new Type[] { typeof(object) }, GetType(), true);
					var ilGen = dynamicMethod.GetILGenerator(16);

					// hack required until .net 4.5
					if (RelocFixupListInfo != null)
					{
						RelocFixupListInfo.SetValue(ilGen, emptyIntArray);
					}
					if (RVAFixupListInfo != null)
					{
						RVAFixupListInfo.SetValue(ilGen, emptyIntArray);
					}

					ilGen.Emit(OpCodes.Ldarg_0);

					var storedGetMethodValue = GetMethodValue;
					var declaringType = storedGetMethodValue.DeclaringType;
					if (declaringType != null)
					{
						if (storedGetMethodValue.IsVirtual && !declaringType.IsGenericType)
						{
							var baseDefinition = storedGetMethodValue.GetBaseDefinition();
							if (baseDefinition != null && baseDefinition.DeclaringType != null)
							{
								ilGen.Emit(OpCodes.Castclass, baseDefinition.DeclaringType);
							}
						}
						else
						{
							ilGen.Emit(OpCodes.Castclass, declaringType);
						}

						if (declaringType.IsValueType)
						{
							ilGen.Emit(OpCodes.Unbox, declaringType);
						}
					}

					ilGen.Emit(OpCodes.Callvirt, storedGetMethodValue);
					if (storedGetMethodValue.ReturnType.IsValueType)
					{
						ilGen.Emit(OpCodes.Box, PropertyType);
					}
					ilGen.Emit(OpCodes.Ret);

					getter = (GetterDelegate)dynamicMethod.CreateDelegate(typeof(GetterDelegate));
				}

				return getter;
			}
		}

		GetterDelegate getter;

		#endregion
	}
}
