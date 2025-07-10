using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockCodeEnum : MockCodeType, EnvDTE.CodeEnum
	{
		public MockCodeEnum(MockProjectItem projectItem, string fullName)
			: base(projectItem, fullName, EnvDTE.vsCMElement.vsCMElementEnum)
		{
			Bases.Add(new MockCodeClass(projectItem, typeof(Enum).FullName));
		}

		#region CodeEnum Members

		public EnvDTE.CodeVariable AddMember(string Name, object Value, object Position)
		{ throw new NotSupportedException(); }

		object EnvDTE.CodeEnum.Parent
		{ get { return Parent; } }

		EnvDTE.CodeElements EnvDTE.CodeEnum.Bases
		{ get { return Bases; } }

		EnvDTE.CodeElements EnvDTE.CodeEnum.Members
		{ get { return Members; } }

		#endregion
	}
}
