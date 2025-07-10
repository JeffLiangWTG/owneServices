using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using CargoWise.Common.Design;
using CargoWise.Common.Design.Testing;
using CargoWise.Design.DTE.DVSCodeTypeTestClasses;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Design.DTE.Testing
{
	class VSCodeTypeTest : TestCase, IServiceProvider
	{
		public void TestDoesntCauseMemoryLeak()
		{
			TestDoesntCauseMemoryLeak_Setup(out var elementRef, out var typeRef);
			GC.Collect();
			AssertEquals("CodeElement should be collected", false, elementRef.IsAlive);
			AssertEquals("Fake type should be collected", false, typeRef.IsAlive);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		void TestDoesntCauseMemoryLeak_Setup(out WeakReference elementRef, out WeakReference typeRef)
		{
			MockCodeClass element = new MockCodeClass(new MockProjectItem(DTE), "my.fullname");
			TestVSCodeType type = TestVSCodeType.FromCodeType(this, element.ProjectItem.ContainingProject, element);
			elementRef = new WeakReference(element);
			typeRef = new WeakReference(type);
		}

		public void TestGetCustomAttributes()
		{
			MockCodeClass element = new MockCodeClass(new MockProjectItem(DTE), "my.fullname");
			TestVSCodeType type = TestVSCodeType.FromCodeType(this, element.ProjectItem.ContainingProject, element);
			DefaultValueAttribute[] attrs = (DefaultValueAttribute[])type.GetCustomAttributes(typeof(DefaultValueAttribute), false);
			AssertEquals("GetCustomAttributes must return the right array type of attributes", typeof(DefaultValueAttribute[]), attrs.GetType());
		}

		#region Namespace / FullName
		public void TestNamespace()
		{
			VSCodeType type = TestVSCodeType.FromCodeType(this, new MockCodeClass(new MockProjectItem(DTE), "CargoWise.Common.blah"));
			AssertEquals("Namespace should be calculated from full name", "CargoWise.Common", type.Namespace);
			VSCodeType type2 = TestVSCodeType.FromCodeType(this, new MockCodeClass(new MockProjectItem(DTE), "blah"));
			AssertEquals("Namespace when there is no namespace", "", type2.Namespace);
		}

		public void TestFullName()
		{
			MockCodeClass element = new MockCodeClass(new MockProjectItem(DTE), "my.fullname");
			VSCodeType type = TestVSCodeType.FromCodeType(this, element);
			AssertEquals("FullName correct", "my.fullname", type.FullName);
			AssertEquals("Name correct", "fullname", type.Name);
		}

		#endregion
		#region ToString / Equals / GetHashCode
		public void TestToString()
		{
			MockCodeClass element = new MockCodeClass(new MockProjectItem(DTE), "my.fullname");
			AssertEquals("ToString", "my.fullname", TestVSCodeType.FromCodeType(this, element).ToString());
		}

		public void TestEquals()
		{
			MockCodeClass element1 = new MockCodeClass(new MockProjectItem(DTE), "my.fullname");
			MockCodeClass element2 = new MockCodeClass(new MockProjectItem(DTE), "my.fullname");
			VSCodeType type1 = TestVSCodeType.FromCodeType(this, element1);
			VSCodeType type1_duplicate = TestVSCodeType.FromCodeType(this, element1);
			VSCodeType type2 = TestVSCodeType.FromCodeType(this, element2);
			AssertEquals("Equals", true, type1.Equals((object)type1_duplicate));
			AssertEquals("Equals", false, type1.Equals((object)type2));
		}

		public void TestGetHashCode()
		{
			MockCodeClass element = new MockCodeClass(new MockProjectItem(DTE), "my.fullname");
			AssertEquals("GetHashCode", TestVSCodeType.FromCodeType(this, element).GetHashCode(), element.GetHashCode());
		}

		#endregion
		#region BaseType and GetInterfaces
		public void TestGetInterfaces()
		{
			MockProjectItem project_item = new MockProjectItem(DTE);
			MockCodeInterface base_base_intf_element = new MockCodeInterface(project_item, typeof(ICloneable).FullName);
			MockCodeClass base_element = new MockCodeClass(project_item, "baseclass");
			base_element.Bases.Add(base_base_intf_element);
			MockCodeInterface base_intf_element = new MockCodeInterface(project_item, "baseinterface");
			MockCodeClass element = new MockCodeClass(project_item, "class");
			element.Bases.AddRange(base_element, base_intf_element, base_base_intf_element);
			VSCodeType type = TestVSCodeType.FromCodeType(this, element);
			VSCodeType base_type = TestVSCodeType.FromCodeType(this, base_element);
			VSCodeType base_intf = TestVSCodeType.FromCodeType(this, base_intf_element);
			Type base_base_intf = typeof(ICloneable);
			AssertEquals("The base base intf should be specified", base_base_intf, base_type.GetInterfaces()[0]);
			AssertEquals("The base type should be specified", base_type, type.BaseType);
			AssertEquals("Base intf should be specified", base_intf, type.GetInterfaces()[0]);
			AssertEquals("Base intf should be specified", base_base_intf, type.GetInterfaces()[1]);
		}

		#endregion
		#region GetAttributeFlagsImpl
		public void TestGetAttributeFlagsImpl_AndIsValueType_ForVariousTypesOfTypes()
		{
			MockCodeClass type_element = new MockCodeClass(new MockProjectItem(DTE), "class");
			AssertEquals(false, TestVSCodeType.FromCodeType(this, type_element).IsValueType);
			AssertEquals(TestVSCodeType.FromCodeType(this, type_element).Attributes, typeof(MyClass).Attributes);
			MockCodeInterface interface_element = new MockCodeInterface(new MockProjectItem(DTE), "interface");
			AssertEquals(false, TestVSCodeType.FromCodeType(this, interface_element).IsValueType);
			AssertEquals(TestVSCodeType.FromCodeType(this, interface_element).Attributes, typeof(MyInterface).Attributes);
			MockCodeStruct struct_element = new MockCodeStruct(new MockProjectItem(DTE), "struct");
			AssertEquals(true, TestVSCodeType.FromCodeType(this, struct_element).IsValueType);
			AssertEquals(TestVSCodeType.FromCodeType(this, struct_element).Attributes, typeof(MyStruct).Attributes);
			MockCodeEnum enum_element = new MockCodeEnum(new MockProjectItem(DTE), "enum");
			AssertEquals(true, TestVSCodeType.FromCodeType(this, enum_element).IsValueType);
			AssertEquals(TestVSCodeType.FromCodeType(this, enum_element).Attributes, typeof(MyEnum).Attributes);
			MockCodeDelegate delegate_element = new MockCodeDelegate(new MockProjectItem(DTE), "delegate");
			AssertEquals(false, TestVSCodeType.FromCodeType(this, delegate_element).IsValueType);
			AssertEquals(TestVSCodeType.FromCodeType(this, delegate_element).Attributes, typeof(MyDelegate).Attributes);
		}

		public void TestGetAttributeFlagsImpl_ForOtherStates()
		{
			MockCodeClass element;
			element = new MockCodeClass(new MockProjectItem(DTE), "class");
			element.IsSealed = false;
			element.IsAbstract = true;
			AssertEquals(TestVSCodeType.FromCodeType(this, element).Attributes, typeof(MyAbstractClass).Attributes);
			element = new MockCodeClass(new MockProjectItem(DTE), "class");
			element.IsSealed = true;
			element.IsAbstract = false;
			AssertEquals(TestVSCodeType.FromCodeType(this, element).Attributes, typeof(MySealedClass).Attributes);
		}

		public void TestGetAttributeFlagsImpl_AccessModifiers()
		{
			EnvDTE.vsCMAccess prot_intn = EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily;
			EnvDTE.vsCMAccess pub = EnvDTE.vsCMAccess.vsCMAccessPublic;
			EnvDTE.vsCMAccess prot = EnvDTE.vsCMAccess.vsCMAccessProtected;
			EnvDTE.vsCMAccess priv = EnvDTE.vsCMAccess.vsCMAccessPrivate;
			TypeAttributes common = TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;
			TestGetAttributeFlagsImpl_AccessModifiers(prot_intn, common | TypeAttributes.NotPublic);
			TestGetAttributeFlagsImpl_AccessModifiers(pub, common | TypeAttributes.Public);
			TestGetAttributeFlagsImpl_AccessModifiers(prot, common | TypeAttributes.NotPublic);
			TestGetAttributeFlagsImpl_AccessModifiers(priv, common | TypeAttributes.NotPublic);
			TestGetAttributeFlagsImpl_AccessModifiers_ForNested(prot_intn, prot_intn, common | TypeAttributes.NotPublic | TypeAttributes.NestedFamORAssem);
			TestGetAttributeFlagsImpl_AccessModifiers_ForNested(prot_intn, pub, common | TypeAttributes.NotPublic | TypeAttributes.NestedFamORAssem);
			TestGetAttributeFlagsImpl_AccessModifiers_ForNested(prot_intn, prot, common | TypeAttributes.NotPublic | TypeAttributes.NestedFamily);
			TestGetAttributeFlagsImpl_AccessModifiers_ForNested(prot_intn, priv, common | TypeAttributes.NotPublic | TypeAttributes.NestedPrivate);
			TestGetAttributeFlagsImpl_AccessModifiers_ForNested(pub, prot_intn, common | TypeAttributes.Public | TypeAttributes.NestedFamORAssem);
			TestGetAttributeFlagsImpl_AccessModifiers_ForNested(pub, pub, common | TypeAttributes.Public | TypeAttributes.NestedPublic);
			TestGetAttributeFlagsImpl_AccessModifiers_ForNested(pub, prot, common | TypeAttributes.Public | TypeAttributes.NestedFamily);
			TestGetAttributeFlagsImpl_AccessModifiers_ForNested(pub, priv, common | TypeAttributes.Public | TypeAttributes.NestedPrivate);
		}

		void TestGetAttributeFlagsImpl_AccessModifiers(EnvDTE.vsCMAccess access, TypeAttributes expectedTypeAttributes)
		{
			MockCodeType element = new MockCodeClass(new MockProjectItem(DTE), "class");
			element.Access = access;
			VSCodeType type = TestVSCodeType.FromCodeType(this, element);
			AssertEquals("Class access " + access, expectedTypeAttributes, type.Attributes);
		}

		void TestGetAttributeFlagsImpl_AccessModifiers_ForNested(EnvDTE.vsCMAccess outerAccess, EnvDTE.vsCMAccess nestedAccess, TypeAttributes expectedTypeAttributes)
		{
			TestGetAttributeFlagsImpl_AccessModifiers_ForNested_Core(outerAccess, nestedAccess, expectedTypeAttributes);
			if (outerAccess == EnvDTE.vsCMAccess.vsCMAccessDefault || outerAccess == EnvDTE.vsCMAccess.vsCMAccessProject)
			{
				outerAccess = EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily;
			}

			if (nestedAccess == EnvDTE.vsCMAccess.vsCMAccessDefault || nestedAccess == EnvDTE.vsCMAccess.vsCMAccessProject)
			{
				nestedAccess = EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily;
			}

			TestGetAttributeFlagsImpl_AccessModifiers_ForNested_Core(outerAccess, nestedAccess, expectedTypeAttributes);
			if (outerAccess == EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily)
			{
				outerAccess = EnvDTE.vsCMAccess.vsCMAccessProjectOrProtected;
			}

			if (nestedAccess == EnvDTE.vsCMAccess.vsCMAccessAssemblyOrFamily)
			{
				nestedAccess = EnvDTE.vsCMAccess.vsCMAccessProjectOrProtected;
			}

			TestGetAttributeFlagsImpl_AccessModifiers_ForNested_Core(outerAccess, nestedAccess, expectedTypeAttributes);
		}

		void TestGetAttributeFlagsImpl_AccessModifiers_ForNested_Core(EnvDTE.vsCMAccess outerAccess, EnvDTE.vsCMAccess nestedAccess, TypeAttributes expectedTypeAttributes)
		{
			MockProjectItem item = new MockProjectItem(DTE);
			MockCodeType element = CreateNestedTypeForAccessTest(item, outerAccess, nestedAccess);
			VSCodeType type = TestVSCodeType.FromCodeType(this, element);
			AssertEquals("Outer access " + outerAccess + "; nested access " + nestedAccess, expectedTypeAttributes, type.Attributes);
		}

		MockCodeType CreateNestedTypeForAccessTest(MockProjectItem projectItem, EnvDTE.vsCMAccess outerAccess, EnvDTE.vsCMAccess nestedAccess)
		{
			MockCodeType outer = new MockCodeClass(projectItem, "outer_class");
			outer.Access = outerAccess;
			MockCodeType nested = new MockCodeClass(projectItem, "nested_class");
			nested.Access = nestedAccess;
			nested.Parent = outer;
			outer.Members.Add(nested);
			return nested;
		}

		#endregion
		#region IsXXX
		public void TestIsCOMObjectImpl()
		{
			VSCodeType type = NewTestVSCodeType();
			AssertEquals(false, type.ExternalIsCOMObjectImpl());
		}

		public void TestIsContextfulImpl()
		{
			VSCodeType type = NewTestVSCodeType();
			AssertEquals(false, type.ExternalIsContextfulImpl());
		}

		public void TestIsPointerImpl()
		{
			VSCodeType type = NewTestVSCodeType();
			AssertEquals(false, type.ExternalIsPointerImpl());
		}

		public void TestIsPrimitiveImpl()
		{
			VSCodeType type = NewTestVSCodeType();
			AssertEquals(false, type.ExternalIsPrimitiveImpl());
		}

		public void TestIsMarshalByRefImpl()
		{
			VSCodeType type = NewTestVSCodeType();
			AssertEquals(false, type.ExternalIsMarshalByRefImpl());
		}

		public void TestIsArray()
		{
			VSCodeType new_type = NewTestVSCodeType();
			AssertEquals(false, new_type.IsArray);
		}

		public void TestIsByRef()
		{
			VSCodeType new_type = NewTestVSCodeType();
			AssertEquals(false, new_type.IsByRef);
		}

		public void TestHasElementType()
		{
			VSCodeType new_type = NewTestVSCodeType();
			AssertEquals(false, new_type.HasElementType);
		}

		#endregion
		#region UnderlyingSystemType / IsAssignableFrom / IsInstanceOfType
		//public void TestIsAssignableFrom()
		//{
		//    MockProjectItem project_item = new MockProjectItem(DTE);
		//    MockCodeClass base_base_element = new MockCodeClass(project_item, typeof(System.Exception).FullName);
		//    MockCodeInterface base_base_intf_element = new MockCodeInterface(project_item, typeof(System.ICloneable).FullName);
		//    MockCodeClass base_element = new MockCodeClass(project_item, "baseclass");
		//    base_element.Bases.AddRange(base_base_element, base_base_intf_element);
		//    MockCodeInterface base_intf_element = new MockCodeInterface(project_item, "baseinterface");
		//    MockCodeClass element = new MockCodeClass(project_item, "class");
		//    element.Bases.AddRange(base_element, base_intf_element, base_base_intf_element/*returned automatically by CodeType.Bases in real life*/);
		//    VSCodeType type = TestVSCodeType.FromCodeType(this, element);
		//    VSCodeType base_type = TestVSCodeType.FromCodeType(this, base_element);
		//    VSCodeType base_intf = TestVSCodeType.FromCodeType(this, base_intf_element);
		//    Type base_base_type = typeof(System.Exception);
		//    Type base_base_intf = typeof(System.ICloneable);
		//    AssertEquals("The base base type should be specified", base_base_type, base_type.BaseType);
		//    AssertEquals("The base base intf should be specified", base_base_intf, base_type.GetInterfaces()[0]);
		//    AssertEquals("The base type should be specified", base_type, type.BaseType);
		//    AssertEquals("Base intf should be specified", base_intf, type.GetInterfaces()[0]);
		//    AssertEquals("Base intf should be specified", base_base_intf, type.GetInterfaces()[1]);
		//    AssertEquals("Should be considered a subclass", true, base_base_type.IsAssignableFrom(type));
		//    AssertEquals("Should be considered a subclass", true, base_base_intf.IsAssignableFrom(type));
		//    AssertEquals("Should NOT be considered a subclass", false, type.IsAssignableFrom(base_base_type));
		//    AssertEquals("Should NOT be considered a subclass", false, type.IsAssignableFrom(base_base_intf));
		//    AssertEquals("Type is assignable to itself", true, type.IsAssignableFrom(type));
		//    AssertEquals("Type is assignable to itself", true, base_intf.IsAssignableFrom(base_intf));
		//}
		#endregion
		#region ResolveTypeFromCodeType
		public void TestResolveTypeFromCodeType_ForTypeInDLLReferencedAssembly()
		{
			TestVSCodeType vs_type = NewTestVSCodeTypeForTypeInReferencedAssembly(typeof(TestType).FullName);
			string test_type_full_name = typeof(TestType).FullName.Replace("+", ".");
			Type resolved_type = vs_type.ResolveTypeFromCodeType(test_type_full_name);
			AssertEquals("Should try to load the assembly from the assembly first before trying to use LoadFrom (type != type problem)", typeof(TestType), resolved_type);
		}

		public void TestResolveTypeFromCodeType_ForGenericType()
		{
			TestVSCodeType vsType = NewTestVSCodeTypeForTypeInReferencedAssembly(typeof(TestType).FullName);
			Type resolvedType = vsType.ResolveTypeFromCodeType("System.Collections.Generic.List<int>");
			AssertEquals("Generic type should be loaded", typeof(List<int>), resolvedType);
		}

		TestVSCodeType NewTestVSCodeTypeForTypeInReferencedAssembly(string typeName)
		{
			MockProjectItem projectItem = new MockProjectItem(DTE);
			projectItem.ContainingProject.GetVSProject().References.Add(AssemblyReference);
			MockCodeClass element = new MockCodeClass(projectItem, typeName);
			return TestVSCodeType.FromCodeType(this, element);
		}

		readonly MockDTE DTE = new MockDTE();
		MockReference AssemblyReference
		{
			get
			{
				if (assemblyReference == null)
				{
					assemblyReference = new MockReference();
					assemblyReference.Name = GetType().Assembly.GetName().Name;
					assemblyReference.Path = GetType().Assembly.Location;
				}

				return assemblyReference;
			}
		}

		MockReference assemblyReference;
		class TestType
		{
		}

		#endregion
		#region Misc
		public void TestGetArrayRank()
		{
			VSCodeType new_type = NewTestVSCodeType();
			AssertEquals(0, new_type.GetArrayRank());
		}

		public void TestGetElementType()
		{
			VSCodeType new_type = NewTestVSCodeType();
			AssertEquals(null, new_type.GetElementType());
		}

		#endregion
		#region Supported overrides that just call base
		//public void TestDeclaringType()
		//{
		//    VSCodeType type = NewTestVSCodeType();
		//    AssertEquals("The current type is always the declaring type", type, type.DeclaringType);
		//}
		//public void TestReflectedType()
		//{
		//    VSCodeType type = NewTestVSCodeType();
		//    AssertEquals("The current type is always the declaring type", type, type.ReflectedType);
		//}
		public void TestIsSubclassOf()
		{
			MockProjectItem project_item = new MockProjectItem(DTE);
			MockCodeClass base_base_element = new MockCodeClass(project_item, "basebaseclass");
			MockCodeClass base_element = new MockCodeClass(project_item, "baseclass");
			base_element.Bases.Add(base_base_element);
			MockCodeClass element = new MockCodeClass(project_item, "class");
			element.Bases.Add(base_element);
			VSCodeType type = TestVSCodeType.FromCodeType(this, element);
			VSCodeType base_type = TestVSCodeType.FromCodeType(this, base_element);
			VSCodeType base_base_type = TestVSCodeType.FromCodeType(this, base_base_element);
			AssertEquals("The base base type should be specified", base_base_type, base_type.BaseType);
			AssertEquals("The base type should be specified", base_type, type.BaseType);
			AssertEquals("Should be considered a subclass", true, type.IsSubclassOf(base_base_type));
			AssertEquals("Should NOT be considered a subclass", false, base_base_type.IsSubclassOf(type));
		}

		#endregion
		#region Testing Class / Interface / Struct / Enum / Delegate
		#region Testing Class
		public void TestClassWithNoBase()
		{
			MockCodeType element = new MockCodeClass(new MockProjectItem(DTE), "class");
			VSCodeType type = TestVSCodeType.FromCodeType(this, element);
			AssertEquals(typeof(object), type.BaseType);
		}

		public void TestClassWithProjectBase()
		{
			MockCodeType element = new MockCodeClass(new MockProjectItem(DTE), "class");
			MockCodeType base_element = new MockCodeClass(new MockProjectItem(DTE), "project_base");
			element.Bases.Add(base_element);
			VSCodeType type = TestVSCodeType.FromCodeType(this, element);
			AssertEquals("project_base", type.BaseType.FullName);
		}

		[RequiresSoftware(RequiredSoftware.VisualStudio)]
		public void TestClassWithExternalBase()
		{
			MockCodeType element = new MockCodeClass(new MockProjectItem(DTE), "class");
			MockCodeType base_element = new MockCodeClass(new MockProjectItem(DTE), "System.Int32");
			element.Bases.Add(base_element);
			VSCodeType type = TestVSCodeType.FromCodeType(this, element);
			AssertEquals(typeof(int), type.BaseType);
		}

		#endregion
		#region Testing Interface
		public void TestInterface()
		{
			MockCodeInterface element = new MockCodeInterface(new MockProjectItem(DTE), "class");
			VSCodeType type = TestVSCodeType.FromCodeType(this, element);
			AssertEquals(typeof(object), type.BaseType);
			AssertEquals(false, type.IsClass);
			AssertEquals(true, type.IsInterface);
			AssertEquals(false, type.IsValueType);
			AssertEquals(false, type.IsEnum);
		}

		#endregion
		#region Testing Struct
		[RequiresSoftware(RequiredSoftware.VisualStudio)]
		public void TestStruct()
		{
			MockCodeStruct element = new MockCodeStruct(new MockProjectItem(DTE), "struct");
			VSCodeType type = TestVSCodeType.FromCodeType(this, element);
			AssertEquals(typeof(ValueType), type.BaseType);
			AssertEquals(false, type.IsClass);
			AssertEquals(false, type.IsInterface);
			AssertEquals(true, type.IsValueType);
			AssertEquals(false, type.IsEnum);
		}

		#endregion
		#region Testing Enum
		public void TestEnum()
		{
			MockCodeEnum element = new MockCodeEnum(new MockProjectItem(DTE), "enum");
			VSCodeType type = TestVSCodeType.FromCodeType(this, element);
			AssertEquals(typeof(Enum), type.BaseType);
			AssertEquals(false, type.IsClass);
			AssertEquals(false, type.IsInterface);
			AssertEquals(true, type.IsValueType);
			AssertEquals(true, type.IsEnum);
		}

		#endregion
		#region Testing Delegate
		public void TestDelegate()
		{
			MockCodeDelegate element = new MockCodeDelegate(new MockProjectItem(DTE), "delegate");
			VSCodeType type = TestVSCodeType.FromCodeType(this, element);
			AssertEquals(typeof(Delegate), type.BaseType);
			AssertEquals(true, type.IsClass);
			AssertEquals(false, type.IsInterface);
			AssertEquals(false, type.IsValueType);
			AssertEquals(false, type.IsEnum);
		}

		#endregion
		#endregion
		#region IServiceProvider Members
		object IServiceProvider.GetService(Type serviceType)
		{
			object result = null;
			if (serviceType == TypeResolutionServiceLocator.DynamicTypeServiceType)
			{
				result = new MockDynamicTypeService(TypeResolutionService);
			}

			return result;
		}

		#endregion
		#region Test Classes
		class TestVSCodeType : VSCodeType
		{
			protected TestVSCodeType(IServiceProvider serviceProvider, EnvDTE.Project containingProject, EnvDTE.CodeType codeType) : base(serviceProvider, containingProject, codeType)
			{
			}

			public static TestVSCodeType FromCodeType(IServiceProvider serviceProvider, EnvDTE.CodeType codeType)
			{
				return FromCodeType(serviceProvider, codeType.ProjectItem.ContainingProject, codeType);
			}

			public new static TestVSCodeType FromCodeType(IServiceProvider serviceProvider, EnvDTE.Project containingProject, EnvDTE.CodeType codeType)
			{
				return (TestVSCodeType)TestVSCodeType.FromCodeType(codeType, delegate
				{
					return new TestVSCodeType(serviceProvider, containingProject, codeType);
				});
			}

			protected override VSCodeType FromCodeTypeInstance(IServiceProvider serviceProvider, EnvDTE.Project containingProject, EnvDTE.CodeType codeType)
			{
				return TestVSCodeType.FromCodeType(serviceProvider, containingProject, codeType);
			}
		}

		[DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		static extern int GetCurrentThreadId();
		[CodeAlive("WI00575476 Baseline")]
		class MockSynchronizationContext : SynchronizationContext
		{
			readonly VSCodeTypeTest owner;
			public MockSynchronizationContext(VSCodeTypeTest owner)
			{
				this.owner = owner;
			}

			public override void Send(SendOrPostCallback d, object state)
			{
				Thread oldThread = owner.TypeResolutionService.ThrowIfNotOnThread;
				owner.TypeResolutionService.ThrowIfNotOnThread = Thread.CurrentThread;
				try
				{
					d.DynamicInvoke(new object[] { state });
				}
				finally
				{
					owner.TypeResolutionService.ThrowIfNotOnThread = oldThread;
				}
			}

			public override void Post(SendOrPostCallback d, object state)
			{
				throw new Exception("Not supported");
			}
		}

		#endregion
		#region Implementation
		MockTypeResolutionService TypeResolutionService
		{
			get
			{
				if (typeResolutionService == null)
				{
					typeResolutionService = new MockTypeResolutionService(GetType().Assembly);
				}

				return typeResolutionService;
			}
		}

		MockTypeResolutionService typeResolutionService;
		VSCodeType NewTestVSCodeType()
		{
			MockCodeType code_type = new MockCodeClass(new MockProjectItem(DTE), "class");
			return TestVSCodeType.FromCodeType(this, code_type);
		}
		#endregion
	}
}

namespace CargoWise.Design.DTE.DVSCodeTypeTestClasses
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MyClass { }
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class MyAbstractClass { }
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class MySealedClass { }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface MyInterface { }
	[EditorBrowsable(EditorBrowsableState.Never)]
	public struct MyStruct { }
	[EditorBrowsable(EditorBrowsableState.Never)]
	public delegate void MyDelegate();
	[EditorBrowsable(EditorBrowsableState.Never)]
	public enum MyEnum { }
}
