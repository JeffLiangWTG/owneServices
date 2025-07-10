using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockCodeType : MockCodeElement, EnvDTE.CodeType
	{
		public MockCodeType(MockProjectItem projectItem, string fullName, EnvDTE.vsCMElement elementType)
			: base(projectItem, fullName, elementType)
		{
			Parent = projectItem;
			Access = EnvDTE.vsCMAccess.vsCMAccessPublic;
		}

		public bool FoundFromCodeTypeFromFullName;

		public override EnvDTE.ProjectItem ProjectItem
		{
			get
			{
				if (FoundFromCodeTypeFromFullName)
				{
					throw new COMException("DTE will throw a COMException when you retrieve ProjectItem when an EnvDTE.CodeType is retrieved from CodeTypeFromFullName.");
				}
				return base.ProjectItem;
			}
		}

		#region EnvDTE.CodeType

		public object Parent;
		object EnvDTE.CodeType.Parent
		{ get { return Parent; } }

		public MockCodeElements Bases
		{ get { return bases ?? (bases = new MockCodeElements()); } }
		MockCodeElements bases;

		EnvDTE.CodeElements EnvDTE.CodeType.Bases
		{ get { return Bases; } }

		public MockCodeElements Members
		{ get { return members ?? (members = new MockCodeElements()); } }
		MockCodeElements members;

		EnvDTE.CodeElements EnvDTE.CodeType.Members
		{ get { return Members; } }

		#region Unsupported Members

		public EnvDTE.CodeAttribute AddAttribute(string Name, string Value, object Position)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeElement AddBase(object Base, object Position)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeElements Attributes
		{ get { throw new NotSupportedException(); } }

		public string Comment
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public EnvDTE.CodeElements DerivedTypes
		{ get { throw new NotSupportedException(); } }

		public string DocComment
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public bool get_IsDerivedFrom(string FullName)
		{
			foreach (EnvDTE.CodeElement element in Bases)
			{
				EnvDTE.CodeType baseType = element as EnvDTE.CodeType;
				if (FullName == element.FullName || baseType.get_IsDerivedFrom(FullName))
				{
					return true;
				}
			}
			return false;
		}

		public EnvDTE.CodeNamespace Namespace
		{ get { throw new NotSupportedException(); } }

		public void RemoveBase(object Element)
		{ throw new NotSupportedException(); }

		public void RemoveMember(object Element)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeProperty AddProperty(string getterName, string putterName, object type, object position, EnvDTE.vsCMAccess access, object location)
		{ throw new NotSupportedException(); }

		#endregion

		#endregion
	}
}
