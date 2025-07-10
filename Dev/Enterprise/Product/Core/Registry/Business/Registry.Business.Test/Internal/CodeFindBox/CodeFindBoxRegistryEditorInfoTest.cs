using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Registry.Business.Testing
{
	sealed class CodeFindBoxRegistryEditorInfoTest : TestCaseWithFactory
	{
		public void TestBaseDataTypeToBeEdited()
		{
			CodeFindBoxRegistryEditorInfo editorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefCountry, GetFindBoxCollection);
			AssertEquals("BaseDataTypeToBeEdited", typeof(StringRegistryDataType), editorInfo.BaseDataTypeToBeEdited);
		}

		public void TestModuleID()
		{
			CodeFindBoxRegistryEditorInfo editorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefCountry, GetFindBoxCollection);
			AssertEquals("ModuleID", ModuleIDs.RefCountry, editorInfo.ModuleID);
		}

		public void TestGetFindBoxCollection()
		{
			CodeFindBoxRegistryEditorInfo editorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefCountry, GetFindBoxCollection);
			AssertEquals("GetFindBoxCollection", ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IRefCountryCollection>(), editorInfo.GetFindBoxCollection(Factory).GetType());
		}

		IBusinessObjectCollection GetFindBoxCollection(BusinessObjectFactory factory)
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IRefCountryCollection>(), factory);
		}
	}
}
