using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.ReflectiveFieldMap;
using Enterprise.DocumentEngine.ReflectiveFieldMap.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ValueProviders.Testing
{
	[TestedType(typeof(DataReflectorValueProviderWrapper))]
	sealed class DataReflectorValueProviderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var docDataProviderReflector = new DocDataProviderReflector(typeof(DocumentWrapperForTest));
			var valueProviderMap = new ValueProviderMap();

			var wrapper = new DataReflectorValueProviderWrapper(new DocDataProviderReflector[] { docDataProviderReflector }, valueProviderMap, DataReflectorValueProviderWrapper.Mode.Select);
			AssertEquals("wrapper.DocDataProviderReflector", docDataProviderReflector, wrapper.DocDataProviderReflectors[0]);
			AssertEquals("wrapper.ValueProviderMap", valueProviderMap, wrapper.ValueProviderMap);
		}

		public void TestSelectedPropertyInformation()
		{
			var docDataProviderReflector = new DocDataProviderReflector(new Enterprise.DocumentEngineCore.DocumentSupport.DataContextMapList.MapElement("MyDataContext", typeof(DocumentWrapperForTest)));
			var valueProviderMap = new ValueProviderMap();
			var wrapper = new DataReflectorValueProviderWrapper(new DocDataProviderReflector[] { docDataProviderReflector }, valueProviderMap, DataReflectorValueProviderWrapper.Mode.Browse);
			var docDataProviderIsInProperty = docDataProviderReflector.Members[0];
			wrapper.SetSelectedProperty(docDataProviderIsInProperty);
			AssertMultilineASCIIEquals("reflector.SelectedPropertyInformation", @"
DocDataProviderIsIn (OrgHeader)

Module ID: Organisation
Controller ID: Organisation
<DocDataProviderIsIn>
".Trim(), wrapper.SelectedMemberInformation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var docDataProviderReflector = new DocDataProviderReflector(typeof(DocumentWrapperForTest));
			var valueProviderMap = new ValueProviderMap();
			var wrapper = new DataReflectorValueProviderWrapper(new DocDataProviderReflector[] { docDataProviderReflector }, valueProviderMap, DataReflectorValueProviderWrapper.Mode.Select);

			return wrapper;
		}
	}
}
