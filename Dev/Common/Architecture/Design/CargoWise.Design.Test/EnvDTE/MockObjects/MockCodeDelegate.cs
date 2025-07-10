using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockCodeDelegate : MockCodeType, EnvDTE.CodeDelegate
	{
		public MockCodeDelegate(MockProjectItem projectItem, string fullName) : base(projectItem, fullName, EnvDTE.vsCMElement.vsCMElementDelegate)
		{
			Bases.Add(new MockCodeClass(projectItem, typeof(Delegate).FullName));
		}

		#region CodeDelegate Members

		object EnvDTE.CodeDelegate.Parent
		{ get { return Parent; } }

		EnvDTE.CodeElements EnvDTE.CodeDelegate.Bases
		{ get { return Bases; } }

		EnvDTE.CodeElements EnvDTE.CodeDelegate.Members
		{ get { return Members; } }

		public void RemoveParameter(object Element)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeClass BaseClass
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeTypeRef Type
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public EnvDTE.CodeParameter AddParameter(string Name, object Type, object Position)
		{ throw new NotSupportedException(); }

		public string get_Prototype(int Flags)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeElements Parameters
		{ get { throw new NotSupportedException(); } }

		#endregion
	}
}
