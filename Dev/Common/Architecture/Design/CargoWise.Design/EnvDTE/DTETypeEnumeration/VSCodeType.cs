using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Common.Design;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI.Interop;
using Microsoft.VisualStudio.VCCodeModel;

namespace CargoWise.Design.DTE
{
	/// <summary>
	/// A that implements its members from the meta-data on a EnvDTE.CodeType object.
	/// </summary>

	public class VSCodeType : TypeDelegator
	{
		public EnvDTE.Project ContainingProject { get; private set; }
		public EnvDTE.CodeType CodeType { get; private set; }

		#region FromCodeType

		protected VSCodeType(IServiceProvider serviceProvider, EnvDTE.Project containingProject, EnvDTE.CodeType codeType)
		{
			this.serviceProvider = serviceProvider;
			CodeType = codeType;
			ContainingProject = containingProject;
			creatingThread = Thread.CurrentThread;
		}

		public static VSCodeType FromCodeType(IServiceProvider serviceProvider, EnvDTE.Project containingProject, EnvDTE.CodeType codeType)
		{ return FromCodeType(codeType, delegate { return new VSCodeType(serviceProvider, containingProject, codeType); }); }

		protected delegate VSCodeType CodeTypeCreator();
		protected static VSCodeType FromCodeType(EnvDTE.CodeType codeType, CodeTypeCreator constructor)
		{
			WeakReference resultRef = Instances[codeType];
			VSCodeType result = (resultRef == null) ? null : resultRef.Target as VSCodeType;
			if (result == null)
			{
				result = constructor();
				Instances[codeType] = new WeakReference(result);
			}
			return result;
		}

		protected virtual VSCodeType FromCodeTypeInstance(IServiceProvider serviceProvider, EnvDTE.Project containingProject, EnvDTE.CodeType codeType)
		{ return FromCodeType(serviceProvider, containingProject, codeType); }

		#endregion

		#region Equals / GetHashCode

		public override string ToString()
		{ return FullName; }

		public override bool Equals(object o)
		{ return (object)this == o; }

		public override int GetHashCode()
		{
			if (hashCode == 0)
			{
				hashCode = CodeType.GetHashCode();
			}
			return hashCode;
		}
		int hashCode;

		#endregion

		#region Namespace / FullName / Name

		public override string Namespace
		{
			get
			{
				CheckThread();
				return new TypeNameHolder(FullName).Namespace;
			}
		}

		public override string Name
		{
			get
			{
				CheckThread();
				return new TypeNameHolder(FullName).Name;
			}
		}

		public override string FullName
		{
			get
			{
				CheckThread();
				if (fullName == null)
				{
					TypeNameHolder typeName = new FakeTypeConverter().ConvertFrom(CodeType.FullName) as TypeNameHolder;
					fullName = typeName.FullName;
				}
				return fullName;
			}
		}
		string fullName;

		class FakeTypeConverter : TypeTypeConverter
		{
			protected override Type ResolveTypeCore(IServiceProvider serviceProvider, string typeName)
			{
				return new TypeNameHolder(typeName);
			}
		}

		#endregion

		#region BaseType and GetInterfaces

		public override Type BaseType
		{
			get
			{
				CheckThread();
				if (baseType == null)
				{
					baseType = GetBaseType(CodeType, false);
				}
				return baseType;
			}
		}
		Type baseType;

		Type GetBaseType(EnvDTE.CodeType codeType, bool getGenericTypeDefinition)
		{
			Type result = null;
			EnvDTE.CodeType baseCodeType = null;
			try
			{
				if (codeType.Bases.Count > 0)
				{
					baseCodeType = codeType.Bases.Item(1) as EnvDTE.CodeType;
				}
			}
			catch (COMException)
			{
				// occurs intermittently, not much I can do..
			}
			if (baseCodeType != null)
			{
				TypeNameHolder baseTypeName = (TypeNameHolder)new FakeTypeConverter().ConvertFrom(baseCodeType.FullName);
				if (getGenericTypeDefinition && baseTypeName.IsGenericType)
				{
					result = ResolveTypeFromCodeType(baseTypeName.GetGenericTypeDefinition().FullName);
				}
				else
				{
					result = ResolveTypeFromCodeType(baseTypeName.FullName);
				}
				if (result == null)
				{
					result = FromCodeTypeInstance(serviceProvider, ContainingProject, baseCodeType);
				}
				if (result.IsInterface)
				{
					result = null;
				}
			}
			return result ?? typeof(object);
		}

		public override Type[] GetInterfaces()
		{
			if (interfaces == null)
			{
				List<Type> list = new List<Type>();
				foreach (EnvDTE.CodeType intf in GetCodeTypeInterfaces(CodeType))
				{
					var nextIntfType = ResolveTypeFromCodeType(intf.FullName) ?? FromCodeTypeInstance(serviceProvider, ContainingProject, intf);					
					list.Add(nextIntfType);
				}
				Type[] baseInterfaces = GetBaseType(CodeType, false).GetInterfaces();
				if (baseInterfaces.Length == 0)
				{
					baseInterfaces = GetBaseType(CodeType, true).GetInterfaces();
				}
				list.AddRange(baseInterfaces);
				interfaces = list.ToArray();
			}
			return interfaces;
		}
		Type[] interfaces;

		static IEnumerable<EnvDTE.CodeType> GetCodeTypeInterfaces(EnvDTE.CodeType codeType)
		{
			EnvDTE.CodeElements bases = codeType.Bases;
			int baseCount = 0;
			try
			{
				baseCount = bases.Count;
			}
			catch (COMException)
			{
				// occurs intermittently, not much I can do..
			}
			if (baseCount > 0)
			{
				foreach (object nextBaseObj in bases)
				{
					// not always an EnvDTE.CodeType!
					if (nextBaseObj is EnvDTE.CodeType nextBase && nextBase is EnvDTE.CodeInterface)
					{
						yield return nextBase;
					}
				}
			}
		}

		#endregion

		#region GetAttributeFlagsImpl

		protected override TypeAttributes GetAttributeFlagsImpl()
		{
			CheckThread();
			if (attrFlags == 0)
			{
				TypeAttributes result = 0;
				if (CodeType is VCCodeClass vc_class)
				{
					if (vc_class.IsAbstract)
					{
						result |= TypeAttributes.Abstract;
					}
					result |= TypeAttributes.BeforeFieldInit;
					if (vc_class.IsSealed)
					{
						result |= TypeAttributes.Sealed;
					}
				}
				if (CodeType.Kind == EnvDTE.vsCMElement.vsCMElementInterface)
				{
					result |= TypeAttributes.Interface;
					result |= TypeAttributes.Abstract;
					result |= TypeAttributes.BeforeFieldInit;
				}
				if (CodeType.Kind == EnvDTE.vsCMElement.vsCMElementStruct)
				{
					result |= TypeAttributes.BeforeFieldInit;
					result |= TypeAttributes.Sealed;
					result |= TypeAttributes.SequentialLayout;
				}
				if (CodeType.Kind == EnvDTE.vsCMElement.vsCMElementEnum)
				{
					result |= TypeAttributes.Sealed;
				}
				if (CodeType.Kind == EnvDTE.vsCMElement.vsCMElementClass ||
					CodeType.Kind == EnvDTE.vsCMElement.vsCMElementStruct)
				{
					result |= TypeAttributes.Class;
				}
				if (CodeType.Kind == EnvDTE.vsCMElement.vsCMElementDelegate)
				{
					result |= TypeAttributes.Sealed;
				}
				result |= GetVisibilityTypeAttributes(CodeType);
				attrFlags = result;
			}
			return attrFlags;
		}
		TypeAttributes attrFlags;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		TypeAttributes GetVisibilityTypeAttributes(EnvDTE.CodeType element)
		{
			TypeAttributes result = TypeAttributes.AutoLayout | TypeAttributes.AnsiClass;
			EnvDTE.vsCMAccess outerAccess = element.Access;

			if (CodeType.Parent is EnvDTE.CodeType outerClass)
			{
				EnvDTE.vsCMAccess innerAccess = element.Access;
				outerAccess = outerClass.Access;
				if (innerAccess == EnvDTE.vsCMAccess.vsCMAccessProject)
				{
					innerAccess = EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily;
				}

				if (innerAccess == EnvDTE.vsCMAccess.vsCMAccessProjectOrProtected)
				{
					innerAccess = EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily;
				}

				if (innerAccess == EnvDTE.vsCMAccess.vsCMAccessDefault)
				{
					innerAccess = EnvDTE.vsCMAccess.vsCMAccessPrivate;
				}

				if (outerAccess == EnvDTE.vsCMAccess.vsCMAccessProject)
				{
					outerAccess = EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily;
				}

				if (outerAccess == EnvDTE.vsCMAccess.vsCMAccessProjectOrProtected)
				{
					outerAccess = EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily;
				}

				if (outerAccess == EnvDTE.vsCMAccess.vsCMAccessDefault)
				{
					outerAccess = EnvDTE.vsCMAccess.vsCMAccessPrivate;
				}

				if (outerAccess == EnvDTE.vsCMAccess.vsCMAccessPublic &&
					innerAccess == EnvDTE.vsCMAccess.vsCMAccessPublic)
				{
					result |= TypeAttributes.NestedPublic;
				}
				if (outerAccess == EnvDTE.vsCMAccess.vsCMAccessPublic &&
					innerAccess == EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily)
				{
					result |= TypeAttributes.NestedFamORAssem;
				}
				if (outerAccess == EnvDTE.vsCMAccess.vsCMAccessPublic &&
					innerAccess == EnvDTE.vsCMAccess.vsCMAccessProtected)
				{
					result |= TypeAttributes.NestedFamily;
				}
				if (outerAccess == EnvDTE.vsCMAccess.vsCMAccessPublic &&
					innerAccess == EnvDTE.vsCMAccess.vsCMAccessPrivate)
				{
					result |= TypeAttributes.NestedPrivate;
				}
				if (outerAccess == EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily &&
					innerAccess == EnvDTE.vsCMAccess.vsCMAccessPublic)
				{
					result |= TypeAttributes.NestedFamORAssem;
				}
				if (outerAccess == EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily &&
					innerAccess == EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily)
				{
					result |= TypeAttributes.NestedFamORAssem;
				}
				if (outerAccess == EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily &&
					innerAccess == EnvDTE.vsCMAccess.vsCMAccessProtected)
				{
					result |= TypeAttributes.NestedFamily;
				}
				if (outerAccess == EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily &&
					innerAccess == EnvDTE.vsCMAccess.vsCMAccessPrivate)
				{
					result |= TypeAttributes.NestedPrivate;
				}
			}
			switch (outerAccess)
			{
				case EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily:
					result |= TypeAttributes.NotPublic;
					break;
				case EnvDTE.vsCMAccess.vsCMAccessDefault:
					result |= TypeAttributes.NotPublic;
					break;
				case EnvDTE.vsCMAccess.vsCMAccessPrivate:
					result |= TypeAttributes.NotPublic;
					break;
				case EnvDTE.vsCMAccess.vsCMAccessProject:
					result |= TypeAttributes.NotPublic;
					break;
				case EnvDTE.vsCMAccess.vsCMAccessProjectOrProtected:
					result |= TypeAttributes.NotPublic;
					break;
				case EnvDTE.vsCMAccess.vsCMAccessProtected:
					result |= TypeAttributes.NotPublic;
					break;
				case EnvDTE.vsCMAccess.vsCMAccessPublic:
					result |= TypeAttributes.Public;
					break;
				default:
					throw new ArgumentException("Unknown access type " + element.Access);
			}
			return result;
		}

		#endregion

		#region IsXXX

		protected internal bool ExternalIsCOMObjectImpl() => IsCOMObjectImpl();
		protected override bool IsCOMObjectImpl()
		{
			CheckThread();
			return false;
		}

		protected internal bool ExternalIsContextfulImpl() => IsContextfulImpl();
		protected override bool IsContextfulImpl()
		{
			CheckThread();
			return false;
		}

		protected internal bool ExternalIsMarshalByRefImpl() => IsMarshalByRefImpl();
		protected override bool IsMarshalByRefImpl()
		{
			CheckThread();
			return false;
		}

		protected internal bool ExternalIsPointerImpl() => IsPointerImpl();
		protected override bool IsPointerImpl()
		{
			CheckThread();
			return false;
		}

		protected internal bool ExternalIsPrimitiveImpl() => IsPrimitiveImpl();
		protected override bool IsPrimitiveImpl()
		{
			CheckThread();
			return false;
		}

		protected override bool IsArrayImpl()
		{
			CheckThread();
			return false;
		}

		protected override bool IsByRefImpl()
		{
			CheckThread();
			return false;
		}

		protected override bool HasElementTypeImpl()
		{
			CheckThread();
			return false;
		}

		protected override bool IsValueTypeImpl()
		{
			CheckThread();
			return IsSubclassOf(typeof(ValueType));
		}

		#endregion

		#region UnderlyingSystemType / IsAssignableFrom / IsInstanceOfType

		public override Type UnderlyingSystemType
		{
			get
			{
				CheckThread();
				MethodBase immediateCaller = new StackTrace(1, false).GetFrame(0).GetMethod();
				if (immediateCaller.Name == "IsAssignableFrom" &&
					immediateCaller.DeclaringType == typeof(Type))
				{
					throw new ArgumentException(
						"UnderlyingSystemType not supported so that RuntimeType.IsAssignableFrom works correctly");
				}
				return this;
			}
		}

		public override bool IsAssignableFrom(Type c)
		{
			CheckThread();

			// ripped from System.Type
			bool result = false;
			if (Equals((object)c))
			{
				result = true;
			}
			else if (IsInterface)
			{
				Type[] types2 = c.GetInterfaces();
				for (int j = 0; j < types2.Length; j++)
				{
					if (this == types2[j])
					{
						result = true;
					}
				}
			}
			else
			{
				for (c = c.BaseType; c != null; c = c.BaseType)
				{
					if (c == this)
					{
						result = true;
					}
				}
			}
			return result;
		}

		public override bool IsInstanceOfType(object o)
		{
			CheckThread();

			Type current = o.GetType();
			while (current != typeof(object) && current != null)
			{
				if (current.FullName == FullName)
				{
					return true;
				}
				current = current.BaseType;
			}
			return false;
		}

		#endregion

		#region ResolveTypeFromCodeType

		protected internal Type ResolveTypeFromCodeType(string fullName)
		{
			CheckThread();
			RuntimeTypeResolver resolver = new RuntimeTypeResolver(this);
			Type result = (Type)resolver.ConvertFrom(fullName);
			return result is TypeNameHolder ? null : result;
		}

		class RuntimeTypeResolver : TypeTypeConverter
		{
			public RuntimeTypeResolver(VSCodeType owner)
			{ this.owner = owner; }

			protected override Type ResolveTypeCore(IServiceProvider serviceProvider, string typeName)
			{
				Type result = typeof(object).Assembly.GetType(typeName);
				foreach (Assembly next in owner.ProjectAssemblyReferences)
				{
					if (result != null)
					{
						break;
					}
					result = ReflectionUtil.GetTypeFromAssemblyFixupDotsBetweenNestedType(next, typeName);
				}
				return result;
			}

			readonly VSCodeType owner;
		}

		Assembly[] ProjectAssemblyReferences
		{
			get
			{
				Assembly[] assemblies = ReferencedAssembliesCache[ContainingProject];
				if (assemblies == null)
				{
					try
					{
						assemblies = EnvDTEUtil.GetReferencedAssemblies(TypeResolutionService, ContainingProject);
						ReferencedAssembliesCache[ContainingProject] = assemblies;
					}
					catch (COMException)
					{
						assemblies = Array.Empty<Assembly>();
					}
				}
				return assemblies;
			}
		}
		static WeakReferencedKeyDictionary<EnvDTE.Project, Assembly[]> ReferencedAssembliesCache
		{ get { return referencedAssembliesCache ?? (referencedAssembliesCache = new WeakReferencedKeyDictionary<EnvDTE.Project, Assembly[]>()); } }
		[ThreadStatic]
		static WeakReferencedKeyDictionary<EnvDTE.Project, Assembly[]> referencedAssembliesCache;

		protected virtual int GetWindowThreadId(EnvDTE.Window window)
		{
			KNativeWindow mainWindowHWnd = KNativeWindow.FromHandle(new IntPtr(window.HWnd));
			return mainWindowHWnd.GetWindowThreadId();
		}

		ITypeResolutionService TypeResolutionService
		{ get { return TypeResolutionServiceLocator.Get(serviceProvider); } }

		#endregion

		#region Misc

		public override MemberTypes MemberType
		{
			get
			{
				CheckThread();
				EnvDTE.CodeElement outerElement = (EnvDTE.CodeElement)CodeType.Collection.Parent;
				if (outerElement is EnvDTE.CodeType)
				{
					return MemberTypes.NestedType;
				}
				else
				{
					return MemberTypes.TypeInfo;
				}
			}
		}

		public override int GetArrayRank()
		{
			CheckThread();
			return 0;
		}

		public override Type GetElementType()
		{
			CheckThread();
			return null;
		}

		public override Type GetInterface(string name, bool ignoreCase)
		{
			CheckThread();

			foreach (Type intf in GetInterfaces())
			{
				if ((ignoreCase && (intf.Name == name || intf.FullName == name)) ||
					(!ignoreCase && (string.Equals(intf.Name, name, StringComparison.OrdinalIgnoreCase) || string.Equals(intf.FullName, name, StringComparison.OrdinalIgnoreCase))))
				{
					return intf;
				}
			}
			return null;
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			CheckThread();

			foreach (EnvDTE.CodeAttribute attr in CodeType.Attributes)
			{
				if (attr.FullName == attributeType.FullName)
				{
					return true;
				}
			}
			if (inherit)
			{
				return BaseType.IsDefined(attributeType, true);
			}
			return false;
		}

		public override Type GetNestedType(string name, BindingFlags bindingAttr)
		{
			CheckThread();
			foreach (Type next in GetNestedTypes(bindingAttr))
			{
				if (name == next.Name)
				{
					return next;
				}
			}
			return null;
		}

		public override Type[] GetNestedTypes(BindingFlags bindingAttr)
		{
			CheckThread();
			List<Type> result = new List<Type>();
			foreach (EnvDTE.CodeElement nestedElement in CodeType.Children)
			{
				if (nestedElement is EnvDTE.CodeType nestedType)
				{
					VSCodeType toAdd = FromCodeTypeInstance(serviceProvider, ContainingProject, nestedType);
					result.Add(toAdd);
				}
			}
			return result.ToArray();
		}

		#endregion

		#region Not yet implemented

		public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
		{ throw new NotImplementedException(); }

		public override EventInfo[] GetEvents(BindingFlags bindingAttr)
		{ throw new NotImplementedException(); }

		public override FieldInfo GetField(string name, BindingFlags bindingAttr)
		{ throw new NotImplementedException(); }

		public override FieldInfo[] GetFields(BindingFlags bindingAttr)
		{ throw new NotImplementedException(); }

		public override InterfaceMapping GetInterfaceMap(Type interfaceType)
		{ throw new NotImplementedException(); }

		public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
		{ throw new NotImplementedException(); }

		public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
		{ throw new NotImplementedException(); }

		protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{ throw new NotImplementedException(); }

		public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
		{ throw new NotImplementedException(); }

		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{ throw new NotImplementedException(); }

		protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{ throw new NotImplementedException(); }

		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
		{ throw new NotImplementedException(); }

		public override object[] GetCustomAttributes(bool inherit)
		{ return Array.Empty<object>(); }

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{ return (object[])Array.CreateInstance(attributeType, 0); }

		public override bool IsGenericParameter
		{ get { return false; } }

		public override bool IsGenericType
		{ get { return false; } }

		public override bool IsGenericTypeDefinition
		{ get { return false; } }

		public override bool ContainsGenericParameters
		{ get { return false; } }

		public override Type GetGenericTypeDefinition()
		{ return this; }

		public override Type[] GetGenericArguments()
		{ return Array.Empty<Type>(); }

		public override Type[] GetGenericParameterConstraints()
		{ return Array.Empty<Type>(); }

		#endregion

		#region Not Supported Overrides

		public override Assembly Assembly
		{ get { throw new NotSupportedException(); } }

		public override string AssemblyQualifiedName
		{ get { throw new NotSupportedException(); } }

		public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
		{ throw new NotSupportedException(); }

		public override Guid GUID
		{ get { throw new NotSupportedException(); } }

		public override Module Module
		{ get { throw new NotSupportedException(); } }

		public override RuntimeTypeHandle TypeHandle
		{ get { throw new NotSupportedException(); } }

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{ throw new NotSupportedException(); }

		#endregion

		#region Implementation

		readonly IServiceProvider serviceProvider;
		readonly Thread creatingThread;

		static WeakReferencedKeyDictionary<EnvDTE.CodeType, WeakReference> Instances
		{ get { return instances ?? (instances = new WeakReferencedKeyDictionary<EnvDTE.CodeType, WeakReference>()); } }
		[ThreadStatic]
		static WeakReferencedKeyDictionary<EnvDTE.CodeType, WeakReference> instances;

		void CheckThread()
		{
			if (creatingThread != Thread.CurrentThread)
			{
				throw new InvalidOperationException("You can only invoke members of " + nameof(VSCodeType) + " on the same thread that created it.");
			}
		}

		#endregion
	}
}
