using System;
using System.Collections;
using CargoWise.Common.Design;
using CargoWise.Common.Design.Testing;
using NUnit.Framework;

namespace CargoWise.Design.DTE.Testing
{
	class DesignTimeTypeEnumerableTest : TestCase, IServiceProvider
	{
		public void TestEnumerateAcrossProjectAndDllReferences()
		{
			VSLangProj.Reference[] references = new VSLangProj.Reference[] { ProjectReference, AssemblyReference };
			MockTopLevelProject project = new MockTopLevelProject(references);
			DesignTimeTypeEnumerable e = new DesignTimeTypeEnumerable(this, project);
			ArrayList types = new ArrayList();
			foreach (Type type in e)
			{
				types.Add(type.FullName);
			}

			AssertEquals("Should contain class in project", true, types.Contains("project_class"));
			AssertEquals("Should contain struct in project", true, types.Contains("project_struct"));
			AssertEquals("Should contain intf in project", true, types.Contains("project_interface"));
			AssertEquals("Should contain enum in project", true, types.Contains("project_enum"));
			AssertEquals("Should contain delgte in project", true, types.Contains("project_delegate"));
			AssertEquals("Should contain nested class in project", true, types.Contains("nested_class"));
			// AssertEquals("Should contain type in dll", true, types.Contains(typeof(DesignTimeTypeEnumerable).FullName));
		}

		#region IServiceProvider
		object IServiceProvider.GetService(Type serviceType)
		{
			object result = null;
			if (serviceType == TypeResolutionServiceLocator.DynamicTypeServiceType)
			{
				result = new MockDynamicTypeService(new MockTypeResolutionService(GetType().Assembly));
			}

			return result;
		}
		#endregion

		#region Implementation
		MockReference ProjectReference
		{
			get
			{
				if (projectReference == null)
				{
					projectReference = new MockReference();
					projectReference.SourceProject = new MockProjectWithReferencedSourceCode();
				}

				return projectReference;
			}
		}

		MockReference projectReference;
		MockReference AssemblyReference
		{
			get
			{
				if (assemblyReference == null)
				{
					assemblyReference = new MockReference();
					assemblyReference.Path = GetType().Assembly.Location;
					assemblyReference.Name = GetType().Assembly.GetName().Name;
				}

				return assemblyReference;
			}
		}

		MockReference assemblyReference;
		#endregion

		#region Mock DTE Objects
		class MockTopLevelProject : MockProject
		{
			public MockTopLevelProject(VSLangProj.Reference[] references)
			{ this.GetVSProject().References.AddRange(references); }
		}

		class MockProjectWithReferencedSourceCode : MockProject
		{
			public MockProjectWithReferencedSourceCode()
			{
				AProjectItem.SetFileCodeModel(new MockFileCodeModelWithSourceCode(AProjectItem));
			}
		}

		class MockFileCodeModelWithSourceCode : MockFileCodeModel
		{
			public MockFileCodeModelWithSourceCode(MockProjectItem projectItem)
			{
				MockCodeNamespace emptyFile = new MockCodeNamespace(projectItem, "empty_file");
				MockCodeNamespace classFile = new MockCodeNamespace(projectItem, "class_file");

				EnvDTE.CodeElement[] classElements = new EnvDTE.CodeElement[]
				{
					NewProjectClassWithNestedClass(projectItem, "project_class"),
					new MockCodeStruct(projectItem, "project_struct"),
					new MockCodeElement(projectItem, "something_weird", EnvDTE.vsCMElement.vsCMElementOther),
					new MockCodeInterface(projectItem, "project_interface"),
					new MockCodeEnum(projectItem, "project_enum"),
					new MockCodeDelegate(projectItem, "project_delegate")
				};
				classFile.Members.AddRange(classElements);

				CodeElements.Add(emptyFile);
				CodeElements.Add(classFile);
			}

			#region Implementation
			MockCodeType NewProjectClassWithNestedClass(MockProjectItem projectItem, string fullName)
			{
				MockCodeClass result = new MockCodeClass(projectItem, fullName);
				MockCodeClass nestedClass = new MockCodeClass(projectItem, "nested_class");
				result.Members.Add(nestedClass);
				return result;
			}
			#endregion
		}
		#endregion
	}
}
