using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class ProxyBuilder
	{
		ProxyBuilder()
		{
			lazyAssemblyBuilder = new Lazy<AssemblyBuilder>(GetAssemblyBuilder);
			lazyModuleBuilder =  new Lazy<ModuleBuilder>(GetModuleBuilder);
		}

		Dictionary<Type, Type> proxyCache;

		#region Instance

		public static ProxyBuilder Instance
		{
			get
			{
				if (lazyInstance == null)
				{
					lazyInstance = new Lazy<ProxyBuilder>(() => new ProxyBuilder());
				}

				return lazyInstance.Value;
			}
		}

		[ThreadStatic]
		static Lazy<ProxyBuilder> lazyInstance;

		#endregion

		#region AssemblyBuilder

		AssemblyBuilder AssemblyBuilder
		{
			get { return lazyAssemblyBuilder.Value; }
		}

		readonly Lazy<AssemblyBuilder> lazyAssemblyBuilder;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending migration")]   // WI00669071 - Do not use System.AppDomain.
		AssemblyBuilder GetAssemblyBuilder()
		{
			var name = new AssemblyName(string.Format(CultureInfo.InvariantCulture, "{0}_{1}", "tmpAsm", Guid.NewGuid().ToString("N")));
#if NETCOREAPP
			return AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
#else
			var domain = AppDomain.CurrentDomain;
			return domain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
#endif
		}

#endregion

		#region ModuleBuilder

		ModuleBuilder ModuleBuilder
		{
			get { return lazyModuleBuilder.Value; }
		}

		readonly Lazy<ModuleBuilder> lazyModuleBuilder;

		ModuleBuilder GetModuleBuilder()
		{
			return AssemblyBuilder.DefineDynamicModule("DynamicProxies");
		}

		#endregion

		#region CreateProxy

		public T CreateProxy<T>()
		{
			var type = typeof(T);
			Type proxyType = null;

			if (proxyCache != null && proxyCache.ContainsKey(type))
			{
				proxyType = proxyCache[type];
			}
			else
			{
				proxyType = EmitProxy(type);

				if (proxyCache == null)
				{
					proxyCache = new Dictionary<Type, Type>();
				}

				proxyCache[type] = proxyType;
			}

			return (T)Activator.CreateInstance(proxyType);
		}

		#endregion

		#region Emit

		Type EmitProxy(Type type)
		{
			if (!type.IsInterface || type.IsNested || !type.IsPublic)
			{
				var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Proxy class can be created for only non nested public interfaces. Type '{0}' does not filfill these conditions.", type);

				throw new InvalidOperationException(message);
			}

			var proxyTypeName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", type.Name, Guid.NewGuid().ToString("N"));

			var typeBuilder = ModuleBuilder.DefineType(proxyTypeName,
				TypeAttributes.Public | TypeAttributes.Class,
				typeof(object),
				new[] { type });

			var baseConstructorInfo = typeof(object).GetConstructor(Array.Empty<Type>());

			var constructorBuilder = typeBuilder.DefineConstructor(
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
				CallingConventions.Standard,
				Type.EmptyTypes);

			var ilGenerator = constructorBuilder.GetILGenerator();

			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Call, baseConstructorInfo);
			ilGenerator.Emit(OpCodes.Ret);

			var properties = type.GetProperties();

			foreach (var property in properties)
			{
				EmitProperty(typeBuilder, property);
			}

			EmitMethods(type, typeBuilder);

			var proxy = typeBuilder.CreateType();

			return proxy;
		}

		void EmitMethods(Type type, TypeBuilder typeBuilder)
		{
			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

			foreach (var method in methods)
			{
				if (!method.IsSpecialName)
				{
					EmitMethod(typeBuilder, method);
				}
			}

			foreach (Type interf in type.GetInterfaces())
			{
				EmitMethods(interf, typeBuilder);
			}
		}

		void EmitProperty(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
		{
			var fieldBuilder = typeBuilder.DefineField(string.Concat("_", propertyInfo.Name),
				propertyInfo.PropertyType,
				FieldAttributes.Private);

			var propertyBuilder = typeBuilder.DefineProperty(propertyInfo.Name,
				PropertyAttributes.HasDefault,
				propertyInfo.PropertyType,
				null);

			const MethodAttributes getSetAttr = MethodAttributes.Public
				| MethodAttributes.Final
				| MethodAttributes.HideBySig
				| MethodAttributes.SpecialName
				| MethodAttributes.NewSlot
				| MethodAttributes.Virtual;

			if (propertyInfo.CanRead)
			{
				var getAccessor = typeBuilder.DefineMethod(string.Concat("get_", propertyInfo.Name),
					getSetAttr,
					propertyInfo.PropertyType,
					Type.EmptyTypes);

				var getterIlGenerator = getAccessor.GetILGenerator();

				getterIlGenerator.Emit(OpCodes.Ldarg_0);
				getterIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
				getterIlGenerator.Emit(OpCodes.Ret);

				propertyBuilder.SetGetMethod(getAccessor);

				typeBuilder.DefineMethodOverride(getAccessor, propertyInfo.GetMethod);
			}

			if (propertyInfo.CanWrite)
			{
				var setAccessor = typeBuilder.DefineMethod(string.Concat("set_", propertyInfo.Name),
					getSetAttr,
					null,
					new[] { propertyInfo.PropertyType });

				var setterIlGenerator = setAccessor.GetILGenerator();

				setterIlGenerator.Emit(OpCodes.Ldarg_0);
				setterIlGenerator.Emit(OpCodes.Ldarg_1);
				setterIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);
				setterIlGenerator.Emit(OpCodes.Ret);

				propertyBuilder.SetSetMethod(setAccessor);

				typeBuilder.DefineMethodOverride(setAccessor, propertyInfo.SetMethod);
			}
		}

		void EmitMethod(TypeBuilder typeBuilder, MethodInfo methodInfo)
		{
			var returnType = methodInfo.ReturnType;

			var methodBuilder = typeBuilder.DefineMethod(
				methodInfo.Name,
				MethodAttributes.Public |
				MethodAttributes.Final |
				MethodAttributes.HideBySig |
				MethodAttributes.NewSlot |
				MethodAttributes.Virtual,
				CallingConventions.HasThis,
				returnType,
				methodInfo.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).ToArray());

			var ilGenerator = methodBuilder.GetILGenerator();

			if (returnType != typeof(void))
			{
				var localBuilder = ilGenerator.DeclareLocal(returnType);
				ilGenerator.Emit(OpCodes.Ldloc, localBuilder);
			}

			ilGenerator.Emit(OpCodes.Ret);

			typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
		}

		#endregion
	}
}
