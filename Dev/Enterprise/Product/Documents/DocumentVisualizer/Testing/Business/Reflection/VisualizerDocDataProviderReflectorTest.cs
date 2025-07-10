using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.ReflectiveFieldMap;
using Enterprise.DocumentVisualizer.Business.Reflection;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.Business.Reflection
{
	[TestedType(typeof(VisualizerDocDataProviderReflector))]
	sealed class VisualizerDocDataProviderReflectorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMembers()
		{
			var result = new ZStringBuilder();
			var members = new VisualizerDocDataProviderReflector(typeof(DocBindingBO)).Members;
			foreach (PropertyDescription propertyDescription in members)
			{
				result.Append(string.Format("Type: [{0}] Name: [{1}]", propertyDescription.Property.PropertyType.Name, propertyDescription.Property.Name));
			}
			AssertEquals("reflector.Properties", @"
Type: [IReadOnlyCollection`1] Name: [ReadonlyBOs]
Type: [CustomReadonlyCollection] Name: [ReadonlyCollection]
".Trim(), result.ToStringWithNewLineBetweenAppends());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new VisualizerDocDataProviderReflector(typeof(DocBindingBO));
		}
	}
}
