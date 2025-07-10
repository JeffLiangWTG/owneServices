using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	public abstract class DocumentsMenuHelperTest : TestCaseWithFactory
	{
		protected void AssertConstructorByPK(Type type)
		{
			DocumentsMenuHelper helper = (DocumentsMenuHelper)Activator.CreateInstance(type, new object[] { ZGuid.Empty });
			AssertNotNull("DocumentsMenuHelper should have constructor by PK (it is used in DocumentRequestHandler)", helper);
		}
	}
}
