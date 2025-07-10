using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockCodeModel : MarshalByRefObject, EnvDTE.CodeModel
	{
		readonly MockProject project;

		public MockCodeModel(MockProject project)
		{ this.project = project; }

		#region CodeModel Members

		EnvDTE.CodeElements EnvDTE.CodeModel.CodeElements
		{ get { return CodeElements; } }

		public virtual MockCodeElements CodeElements
		{
			get
			{
				if (codeElements == null)
				{
					codeElements = new MockCodeElements();
				}
				return codeElements;
			}
		}
		MockCodeElements codeElements;

		public string Language;
		string EnvDTE.CodeModel.Language
		{ get { return Language; } }

		public MockCodeClass AddClass(string name, object location, object position, object bases, object implementedInterfaces, EnvDTE.vsCMAccess access)
		{ return new MockCodeClass(project.ProjectItems.AddNew(name + ".cs"), name); }

		EnvDTE.CodeClass EnvDTE.CodeModel.AddClass(string name, object location, object position, object bases, object implementedInterfaces, EnvDTE.vsCMAccess access)
		{ return AddClass(name, location, position, bases, implementedInterfaces, access); }

		public EnvDTE.Project Parent
		{ get { return project; } }

		#region Unsupported Members

		public EnvDTE.DTE DTE
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeInterface AddInterface(string Name, object Location, object Position, object Bases, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeDelegate AddDelegate(string Name, object Location, object Type, object Position, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeFunction AddFunction(string Name, object Location, EnvDTE.vsCMFunction Kind, object Type, object Position, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public void Remove(object Element)
		{ throw new NotSupportedException(); }

		public bool IsCaseSensitive
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeEnum AddEnum(string Name, object Location, object Position, object Bases, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeTypeRef CreateCodeTypeRef(object Type)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeAttribute AddAttribute(string Name, object Location, string Value, object Position)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeVariable AddVariable(string Name, object Location, object Type, object Position, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeNamespace AddNamespace(string Name, object Location, object Position)
		{ throw new NotSupportedException(); }

		public bool IsValidID(string Name)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeStruct AddStruct(string Name, object Location, object Position, object Bases, object ImplementedInterfaces, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeType CodeTypeFromFullName(string Name)
		{
			foreach (EnvDTE.CodeElement element in EnvDTEUtil.GetAllCodeTypes(CodeElements))
			{
				if (element.FullName == Name)
				{
					if (element is MockCodeType mockCodeType)
					{
						mockCodeType.FoundFromCodeTypeFromFullName = true;
					}
					return (EnvDTE.CodeType)element;
				}
			}
			return null;
		}

		#endregion

		#endregion
	}
}
