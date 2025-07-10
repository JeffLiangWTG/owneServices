using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockCodeInterface : MockCodeType, EnvDTE.CodeInterface
	{
		public MockCodeInterface(MockProjectItem projectItem, string fullName)
			: base(projectItem, fullName, EnvDTE.vsCMElement.vsCMElementInterface)
		{
			Bases.Add(new MockCodeClass(projectItem, typeof(object).FullName));
		}

		#region CodeInterface Members

		object EnvDTE.CodeInterface.Parent
		{ get { return Parent; } }

		EnvDTE.CodeElements EnvDTE.CodeInterface.Bases
		{ get { return Bases; } }

		EnvDTE.CodeElements EnvDTE.CodeInterface.Members
		{ get { return Members; } }

		public EnvDTE.CodeFunction AddFunction(string Name, EnvDTE.vsCMFunction Kind, object Type, object Position, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		#endregion
	}
}
