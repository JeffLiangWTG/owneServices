#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using CargoWise.Common.Design.Testing;
using CargoWise.ComponentModel.Design;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class TypeValueIntellisenseEditorSubtypeFilterAttributeTests : TestCase
	{
		public void TestGetBaseTypes()
		{
			var attr = new TypeValueIntellisenseEditorSubtypeFilterAttribute(typeof(Component), typeof(IEnumerable));
			Type[] baseTypes = attr.GetBaseTypes(TypeResolutionService);
			AssertEquals(2, baseTypes.Length);
			AssertEquals(typeof(Component), baseTypes[0]);
			AssertEquals(typeof(IEnumerable), baseTypes[1]);
		}

		public void TestGetBaseTypes_WhenTypeResolutionServiceCannotFindType()
		{
			var attr = new TypeValueIntellisenseEditorSubtypeFilterAttribute(typeof(Component), typeof(TypeNotFoundByService));
			Type[] baseTypes = attr.GetBaseTypes(TypeResolutionService);
			AssertEquals(1, baseTypes.Length);
			AssertEquals(typeof(Component), baseTypes[0]);
		}

		public void TestGetBaseTypes_WhenTypeResolutionServiceCannotFindAnyType()
		{
			var attr = new TypeValueIntellisenseEditorSubtypeFilterAttribute(new Type[] { typeof(TypeNotFoundByService), typeof(TypeNotFoundByService) });
			AssertEquals("null indicates base type filtering does not apply, an empty array indicates ITypeResolutionService found no types", 0, attr.GetBaseTypes(TypeResolutionService).Length);
		}

		public void TestGetBaseTypes_WhenNoBaseTypesSpecified()
		{
			var attr = new TypeValueIntellisenseEditorSubtypeFilterAttribute("TypeFilterMember", (Type[])null);
			var baseTypes = attr.GetBaseTypes(TypeResolutionService);
			AssertEquals("null indicates base type filtering does not apply", null, baseTypes);
		}

		#region Test Classes

		class TypeNotFoundByService { }

		#endregion

		#region Implementation

		ITypeResolutionService TypeResolutionService
		{
			get
			{
				if (typeResolutionService == null)
				{
					typeResolutionService = new MockTypeResolutionService(GetType().Assembly);
					var typesToNotFind = (List<Type>)typeResolutionService.GetType().InvokeMember("TypesToNotFind", BindingFlags.GetField, null, typeResolutionService, null);
					typesToNotFind.Add(typeof(TypeNotFoundByService));
				}
				return typeResolutionService;
			}
		}
		ITypeResolutionService typeResolutionService;

		#endregion
	}
}
#endif
