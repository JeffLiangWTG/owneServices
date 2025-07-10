using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockCodeStruct : MockCodeType, EnvDTE.CodeStruct
	{
		public MockCodeStruct(MockProjectItem projectItem, string fullName)
			: base(projectItem, fullName, EnvDTE.vsCMElement.vsCMElementStruct)
		{
			Bases.Add(new MockCodeClass(projectItem, typeof(ValueType).FullName));
		}

		#region CodeStruct Members

		object EnvDTE.CodeStruct.Parent
		{ get { return Parent; } }

		EnvDTE.CodeElements EnvDTE.CodeStruct.Bases
		{ get { return Bases; } }

		EnvDTE.CodeElements EnvDTE.CodeStruct.Members
		{ get { return Members; } }

		public EnvDTE.CodeClass AddClass(string Name, object Position, object Bases, object ImplementedInterfaces, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public bool IsAbstract
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public EnvDTE.CodeDelegate AddDelegate(string Name, object Type, object Position, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeFunction AddFunction(string Name, EnvDTE.vsCMFunction Kind, object Type, object Position, EnvDTE.vsCMAccess Access, object Location)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeInterface AddImplementedInterface(object Base, object Position)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeEnum AddEnum(string Name, object Position, object Bases, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeVariable AddVariable(string Name, object Type, object Position, EnvDTE.vsCMAccess Access, object Location)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeElements ImplementedInterfaces
		{ get { throw new NotSupportedException(); } }

		public void RemoveInterface(object Element)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeStruct AddStruct(string Name, object Position, object Bases, object ImplementedInterfaces, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		#endregion
	}
}
