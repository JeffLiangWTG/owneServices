using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeFindBoxBusinessObject))]
	sealed class CodeFindBoxBusinessObjectTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		#region Collection and Module ID

		public void TestChoicesCollectionAndModuleID()
		{
			CodeFindBoxRegistryEditorInfo editorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefUNLOCO, GetFindBoxCollection);
			CodeFindBoxBusinessObject bizO = new CodeFindBoxBusinessObject(editorInfo);

			AssertEquals("Collection Type", typeof(RefUNLOCOCollection), bizO.ChoicesCollection.GetType());
			AssertEquals("Module ID", ModuleIDs.RefUNLOCO, bizO.ModuleID);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			CodeFindBoxRegistryEditorInfo editorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefUNLOCO, GetFindBoxCollection);
			return new CodeFindBoxBusinessObject(editorInfo);
		}

		IBusinessObjectCollection GetFindBoxCollection(BusinessObjectFactory factory)
		{
			return new RefUNLOCOCollection(factory);
		}

		#endregion
	}
}
