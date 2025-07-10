using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(IncotermsDropEditBusinessObject))]
	sealed class IncotermsDropEditBusinessObjectTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.Add(new CodeDescriptionPair("abcdefg", ""));
			return new IncotermsDropEditBusinessObject(list);
		}
	}
}
