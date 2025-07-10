using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.ReflectiveFieldMap;
using Enterprise.DocumentVisualizer.Business.Reflection;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.Business.Reflection
{
	[TestedType(typeof(VisualizerDocDataReflector))]
	sealed class VisualizerDocDataReflectorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGenericityMembers()
		{
			var memberDescription = new VisualizerPropertyDescription(typeof(DocBindingBO).GetProperty("ReadonlyBOs"), null, "", null, new VisualizerDataReflectorFilter());
			var result = new ZStringBuilder();

			var members = new VisualizerDocDataReflector(memberDescription).Members;
			foreach (PropertyDescription propertyDescription in members)
			{
				result.Append(string.Format("Type: [{0}] Name: [{1}]", propertyDescription.Property.PropertyType.Name, propertyDescription.Property.Name));
			}
			AssertEquals("reflector.Properties", @"
Type: [ZString] Name: [ZStringPropertyInReadonlyBO1]
Type: [ZString] Name: [ZStringPropertyInReadonlyBO2]
".Trim(), result.ToStringWithNewLineBetweenAppends());
		}

		public void TestGenericityAndSelfMembers()
		{
			var memberDescription = new VisualizerPropertyDescription(typeof(DocBindingBO).GetProperty("ReadonlyCollection"), null, "", null, new VisualizerDataReflectorFilter());
			var result = new ZStringBuilder();

			var members = new VisualizerDocDataReflector(memberDescription).Members;
			foreach (PropertyDescription propertyDescription in members)
			{
				result.Append(string.Format("Type: [{0}] Name: [{1}]", propertyDescription.Property.PropertyType.Name, propertyDescription.Property.Name));
			}
			AssertEquals("reflector.Properties", @"
Type: [ZString] Name: [ZStringPropertyInReadonlyBO1]
Type: [ZString] Name: [ZStringPropertyInReadonlyBO2]
Type: [ZString] Name: [ZStringPropertyInCustomReadonlyCollection1]
Type: [ZString] Name: [ZStringPropertyInCustomReadonlyCollection2]
".Trim(), result.ToStringWithNewLineBetweenAppends());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var memberDescription = new VisualizerPropertyDescription(typeof(DocBindingBO).GetProperty("ReadonlyBOs"), null, "", null, new VisualizerDataReflectorFilter());
			return new VisualizerDocDataReflector(memberDescription);
		}
	}
}
