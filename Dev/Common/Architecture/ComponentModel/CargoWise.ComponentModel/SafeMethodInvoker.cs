using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	public static class SafeMethodInvoker
	{
		#region SafeCreateInstance / SafeMethodInvoke / SafeCreateDelegate

		public const string PrivateTypeAccessNamespace = "PrivateTypeAccess";
		public const string PrivateTypeAccessTypeName = "PrivateTypeAccess";
		public const string PrivateTypeAccessTypeFullName = PrivateTypeAccessNamespace + "." + PrivateTypeAccessTypeName;

		static volatile bool failedReflectionPermission;

		/// <summary>
		/// Constructs an object from a type. A private type may be invoked in
		/// partial trust mode (see SafeMethodInvoke).
		/// </summary>
		[SecurityCritical]
		public static object SafeCreateInstance(Type type)
		{
			Argument.NotNull(type, nameof(type)); // Suggested By ReviewBot 
			var constructor = type.GetConstructor(Array.Empty<Type>());
			if (constructor != null)
			{
				return SafeCreateInstance(constructor);
			}
			else
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type {0} returned no constructor.", type.Name));
			}
		}

		/// <summary>
		/// Constructs an object from a type. A private type may be invoked in
		/// partial trust mode (see SafeMethodInvoke).
		/// </summary>
		[SecurityCritical]
		public static object SafeCreateInstance(ConstructorInfo constructor, params object[] parameters)
		{
			Argument.NotNull(constructor, nameof(constructor)); // Suggested By ReviewBot 
			return SafeMethodInvoke(
					delegate
					{ return constructor.Invoke(parameters); },
					delegate (out bool succeeded)
					{ return SafeConstructorInvokeForPrivateType(constructor, parameters, out succeeded); },
					MemberTypes.Constructor,
					constructor.DeclaringType);
		}

		/// <summary>
		/// Invokes a method on a type. A private method or type may be invoked<br/>
		/// in partial trust mode if the following code exists in the type's assembly:<br/>
		/// <br/>
		/// namespace PrivateTypeAccess<br/>
		/// {<br/>
		///     [EditorBrowsable(EditorBrowsableState.Never)]<br/>
		///     [StrongNameIdentityPermission(SecurityAction.LinkDemand, Name = ""CargoWise.ComponentModel"", PublicKey = ""db7664a33a4355f2"")]<br/>
		///     public static class PrivateTypeAccess<br/>
		///     {<br/>
		///         public static object Invoke(ConstructorInfo constructor, object[] parameters)<br/>
		///         { return constructor.Invoke(parameters); }<br/>
		///     <br/>
		///         public static object Invoke(MethodBase method, object obj, object[] parameters)<br/>
		///         { return method.Invoke(obj, parameters); }<br/>
		/// <br/>
		///         public static Delegate CreateDelegate(Type type, object firstArgument, MethodInfo method, bool throwOnBindFailure)<br/>
		///         { return Delegate.CreateDelegate(type, firstArgument, method, throwOnBindFailure); }<br/>
		///     }<br/>
		/// }<br/>
		/// <br/>
		/// </summary>
		[SecurityCritical]
		public static object SafeMethodInvoke(MethodBase method, object obj, object[] parameters)
		{
			Argument.NotNull(method, nameof(method)); // Suggested By ReviewBot 
			Argument.NotNull(obj, nameof(obj));
			return SafeMethodInvoke(
					delegate
					{ return method.Invoke(obj, parameters); },
					delegate (out bool succeeded)
					{ return SafeMethodInvokeForPrivateType(method, obj, parameters, out succeeded); },
					MemberTypes.Method,
					method.DeclaringType);
		}

		/// <summary>
		/// Constructs a new Delegate object. A private method may be used to construct
		/// the delegate partial trust mode (see SafeMethodInvoke).
		/// </summary>
		[SecurityCritical]
		public static Delegate SafeCreateDelegate(Type type, object firstArgument, MethodInfo method, bool throwOnBindFailure)
		{
			Argument.NotNull(type, nameof(type));
			Argument.NotNull(firstArgument, nameof(firstArgument));
			Argument.NotNull(method, nameof(method)); // Suggested By ReviewBot 
			Argument.NotNull(method.DeclaringType, nameof(method.DeclaringType));
			return (Delegate)SafeMethodInvoke(
					delegate
					{ return Delegate.CreateDelegate(type, firstArgument, method, throwOnBindFailure); },
					delegate (out bool succeeded)
					{ return SafeCreateDelegateForPrivateType(type, firstArgument, method, throwOnBindFailure, out succeeded); },
					MemberTypes.Custom,
					method.DeclaringType);
		}

		static object SafeMethodInvoke(PublicMethodInvoker publicInvoker, PrivateMethodInvoker privateInvoker, MemberTypes memberType, Type declaringType)
		{
			Argument.NotNull(privateInvoker, nameof(privateInvoker));
			Argument.NotNull(publicInvoker, nameof(publicInvoker));
			object result = null;
			var succeeded = false;
			if (failedReflectionPermission)
			{
				try
				{
					result = privateInvoker(out succeeded);
				}
				catch (SecurityException)
				{
					succeeded = false;
				}
				catch (MemberAccessException)
				{
					succeeded = false;
				}
			}
			if (!succeeded)
			{
				try
				{
					result = publicInvoker();
					failedReflectionPermission = false;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (!(ex is MethodAccessException) && !(ex is SecurityException))
					{
						throw;
					}

					failedReflectionPermission = true;
					try
					{
						result = privateInvoker(out succeeded);
					}
					catch (SecurityException)
					{
						succeeded = false;
					}
					catch (MemberAccessException)
					{
						succeeded = false;
					}
					if (!succeeded)
					{
						throw;
					}
				}
			}
			return result;
		}
		delegate object PublicMethodInvoker();
		delegate object PrivateMethodInvoker(out bool succeeded);

		static object SafeConstructorInvokeForPrivateType(ConstructorInfo constructor, object[] parameters, out bool succeeded)
		{
			Argument.NotNull(constructor, nameof(constructor)); // Suggested By ReviewBot 
			Argument.NotNull(constructor.DeclaringType, nameof(constructor.DeclaringType)); // Suggested By ReviewBot 
			var constructorInvoker = privateConstructorInvokers.GetOrAdd(constructor.DeclaringType.Assembly, (Assembly assembly) =>
			{
				var privateTypeAccess = constructor.DeclaringType.Assembly.GetType(PrivateTypeAccessTypeFullName);
				var constructorInvokerMethod = privateTypeAccess == null ? null : privateTypeAccess.GetMethod("Invoke", new Type[] { typeof(ConstructorInfo), typeof(object[]) });
				if (privateTypeAccess != null && constructorInvokerMethod == null)
				{
					throw new InvalidOperationException("Could not find method " + PrivateTypeAccessTypeFullName + ".Invoke(ConstructorInfo, object[])");
				}
				return constructorInvokerMethod == null ? null : (SafeConstructorInvokeForPrivateTypeDelegate)Delegate.CreateDelegate(typeof(SafeConstructorInvokeForPrivateTypeDelegate), constructorInvokerMethod);
			});
			succeeded = constructorInvoker != null;
			return constructorInvoker == null ? null : constructorInvoker(constructor, parameters);
		}

		static readonly ConcurrentDictionary<Assembly, SafeConstructorInvokeForPrivateTypeDelegate> privateConstructorInvokers = new ConcurrentDictionary<Assembly, SafeConstructorInvokeForPrivateTypeDelegate>();

		delegate object SafeConstructorInvokeForPrivateTypeDelegate(ConstructorInfo constructor, object[] args);

		static object SafeMethodInvokeForPrivateType(MethodBase method, object obj, object[] parameters, out bool succeeded)
		{
			var declaringType = obj != null ? obj.GetType() : method.DeclaringType;
			var methodInvoker = privateMethodInvokers.GetOrAdd(declaringType, (Type type) =>
			{
				var componentType = obj == null ? method.DeclaringType : obj.GetType();
				var methodInvokerMethod = componentType.GetMethod("Invoke", new Type[] { typeof(MethodBase), typeof(object), typeof(object[]) });
				if (methodInvokerMethod == null)
				{
					var privateTypeAccess = method.DeclaringType.Assembly.GetType(PrivateTypeAccessTypeFullName);
					methodInvokerMethod = privateTypeAccess == null ? null : privateTypeAccess.GetMethod("Invoke", new Type[] { typeof(MethodBase), typeof(object), typeof(object[]) });
					if (privateTypeAccess != null && methodInvokerMethod == null)
					{
						throw new InvalidOperationException("Could not find method " + PrivateTypeAccessTypeFullName + ".Invoke(MethodBase, object, object[])");
					}
				}
				return methodInvokerMethod == null ? null : (SafeMethodInvokeForPrivateTypeDelegate)Delegate.CreateDelegate(typeof(SafeMethodInvokeForPrivateTypeDelegate), methodInvokerMethod);
			});
			succeeded = methodInvoker != null;
			return methodInvoker == null ? null : methodInvoker(method, obj, parameters);
		}

		static readonly ConcurrentDictionary<Type, SafeMethodInvokeForPrivateTypeDelegate> privateMethodInvokers = new ConcurrentDictionary<Type, SafeMethodInvokeForPrivateTypeDelegate>();

		delegate object SafeMethodInvokeForPrivateTypeDelegate(MethodBase method, object obj, object[] parameters);

		static Delegate SafeCreateDelegateForPrivateType(Type type, object firstArgument, MethodInfo method, bool throwOnBindFailure, out bool succeeded)
		{
			Argument.NotNull(method, nameof(method)); // Suggested By ReviewBot 
			Argument.NotNull(method.DeclaringType, nameof(method.DeclaringType)); // Suggested By ReviewBot 
			var delegateCreator = privateCreateDelegateMethods.GetOrAdd(method.DeclaringType.Assembly, (Assembly assembly) =>
			{
				var privateTypeAccess = method.DeclaringType.Assembly.GetType(PrivateTypeAccessTypeFullName);
				var delegateCreatorMethod = privateTypeAccess == null ? null : privateTypeAccess.GetMethod("CreateDelegate", new Type[] { typeof(Type), typeof(object), typeof(MethodInfo), typeof(bool) });
				if (privateTypeAccess != null && delegateCreatorMethod == null)
				{
					throw new InvalidOperationException("Could not find method " + PrivateTypeAccessTypeFullName + ".CreateDelegate(Type, object, MethodInfo, bool)");
				}
				return (SafeCreateDelegateForPrivateTypeDelegate)Delegate.CreateDelegate(typeof(SafeCreateDelegateForPrivateTypeDelegate), delegateCreatorMethod);
			});
			succeeded = delegateCreator != null;
			return delegateCreator == null ? null : delegateCreator(type, firstArgument, method, throwOnBindFailure);
		}

		static readonly ConcurrentDictionary<Assembly, SafeCreateDelegateForPrivateTypeDelegate> privateCreateDelegateMethods = new ConcurrentDictionary<Assembly, SafeCreateDelegateForPrivateTypeDelegate>();

		delegate Delegate SafeCreateDelegateForPrivateTypeDelegate(Type type, object firstArgument, MethodInfo method, bool throwOnBindFailure);

		#endregion

		#region CreateLightWeightMethodInvoker

		/// <summary>
		/// Create a dynamic method using light-weight code generation to produce a
		/// fast method invoking Delegate object.
		/// </summary>
		public static Delegate CreateLightWeightMethodInvoker(string name, MethodInfo method, Type delegateType, Type delegateReturnType, Type[] delegateParameterTypes)
		{
			Argument.NotNull(method, nameof(method));
			Argument.NotNull(method.DeclaringType, nameof(method.DeclaringType));
			Argument.NotNull(delegateType, nameof(delegateType));
			Argument.NotNull(delegateReturnType, nameof(delegateReturnType));
			Argument.NotNull(delegateParameterTypes, nameof(delegateParameterTypes)); // Suggested By ReviewBot 

			var methodDeclaringType = method.DeclaringType;
			var methodParameters = method.GetParameters();
			CheckCreateLightWeightMethodInvokerArguments(method, delegateParameterTypes/*, methodParameters*/);
			var methodIsStatic = method.IsStatic;

			var result = new DynamicMethod(name, delegateReturnType, delegateParameterTypes, methodDeclaringType.IsInterface ? typeof(object) : methodDeclaringType, true);
			var il = result.GetILGenerator();
			for (int i = 0; i < delegateParameterTypes.Length; i++)
			{
				if (methodIsStatic)
				{
					var methodParameter = methodParameters[i];
					EmitLoadArgument(il, i, delegateParameterTypes[i], methodParameter.ParameterType);
				}
				else
				{
					if (i == 0)
					{
						il.Emit(OpCodes.Ldarg_0);
						il.Emit(OpCodes.Castclass, methodDeclaringType);
					}
					else
					{
						// Q.E.D.
						var methodParameter = methodParameters[i - 1];
						EmitLoadArgument(il, i, delegateParameterTypes[i], methodParameter.ParameterType);
					}
				}
			}

			il.Emit(!methodIsStatic && method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
			BoxOrUnboxIfRequired(il, method.ReturnType, delegateReturnType);
			il.Emit(OpCodes.Ret);

			return result.CreateDelegate(delegateType);
		}

		static void CheckCreateLightWeightMethodInvokerArguments(MethodInfo method, Type[] delegateParameterTypes)
		{
			Argument.NotNull(method, nameof(method)); // Suggested By ReviewBot 
			Argument.NotNull(delegateParameterTypes, nameof(delegateParameterTypes));
			var methodParameters = method.GetParameters();
			foreach (var parameter in methodParameters)
			{
				if (parameter == null)
				{
					throw new ArgumentException("Method's parameters cannot be null.", nameof(method));
				}
				if ((parameter.IsOut || parameter.IsRetval))
				{
					throw new ArgumentException("out and ref parameters are not supported", nameof(method));
				}
			}
			if (delegateParameterTypes.Length != methodParameters.Length + (method.IsStatic ? 0 : 1))
			{
				throw new ArgumentException("Incorrect number of items in delegateParameterTypes array", nameof(delegateParameterTypes));
			}
			for (int i = 0; i < methodParameters.Length; i++)
			{
				var parameter = methodParameters[i] ?? throw new ArgumentException("Method's parameters cannot be null.", nameof(method));

				var parameterType = parameter.ParameterType;
				var delegateParameterType = delegateParameterTypes[i + (method.IsStatic ? 0 : 1)]
					?? throw new ArgumentException("delegateParameterTypes must have all non-null entries.", nameof(delegateParameterTypes));

				if (parameterType.IsValueType != delegateParameterType.IsValueType)
				{
					throw new ArgumentException("Boxing or Unboxing of parameter types is not presently supported", nameof(delegateParameterTypes));
				}
				if (parameterType.IsValueType && parameterType != delegateParameterType)
				{
					throw new ArgumentException("Casting from one value type to another value type is not presently supported", nameof(delegateParameterTypes));
				}
			}
		}

		static void EmitLoadArgument(ILGenerator il, int argumentIndex, Type boxFrom, Type boxTo)
		{
			Argument.NotNull(il, nameof(il)); // Suggested By ReviewBot 
			if (argumentIndex == 0)
			{
				il.Emit(OpCodes.Ldarg_0);
			}
			else if (argumentIndex == 1)
			{
				il.Emit(OpCodes.Ldarg_1);
			}
			else if (argumentIndex == 2)
			{
				il.Emit(OpCodes.Ldarg_2);
			}
			else if (argumentIndex == 3)
			{
				il.Emit(OpCodes.Ldarg_3);
			}
			else
			{
				il.Emit(OpCodes.Ldarg, (short)argumentIndex);
			}
			//BoxOrUnboxIfRequired(il, boxFrom, boxTo); // couldnt get this to work..
		}

		static void BoxOrUnboxIfRequired(ILGenerator il, Type from, Type to)
		{
			Argument.NotNull(from, nameof(from)); // Suggested By ReviewBot 
			Argument.NotNull(to, nameof(to)); // Suggested By ReviewBot 
			Argument.NotNull(il, nameof(il));
			if (!from.IsValueType && to.IsValueType)
			{
				il.Emit(OpCodes.Unbox, to);
			}
			else if (from.IsValueType && !to.IsValueType)
			{
				il.Emit(OpCodes.Box, from);
			}
		}

		#endregion
	}
}
