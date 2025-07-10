using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockCodeNamespace : MockCodeElement, EnvDTE.CodeNamespace
	{
		public MockCodeNamespace(MockProjectItem projectItem, string fullName)
			: base(projectItem, fullName, EnvDTE.vsCMElement.vsCMElementNamespace)
		{ }

		public MockCodeElements Members
		{ get { return members ?? (members = new MockCodeElements()); } }
		MockCodeElements members;

		EnvDTE.CodeElements EnvDTE.CodeNamespace.Members
		{ get { return Members; } }

		#region Unsupported Members

		public EnvDTE.CodeInterface AddInterface(string Name, object Position, object Bases, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeClass AddClass(string Name, object Position, object Bases, object ImplementedInterfaces, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public string Comment
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public EnvDTE.CodeDelegate AddDelegate(string Name, object Type, object Position, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public void Remove(object Element)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeEnum AddEnum(string Name, object Position, object Bases, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public string DocComment
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public EnvDTE.CodeNamespace AddNamespace(string Name, object Position)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeStruct AddStruct(string Name, object Position, object Bases, object ImplementedInterfaces, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public object Parent
		{ get { throw new NotSupportedException(); } }

		#endregion
	}
}
