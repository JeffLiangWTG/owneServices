using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockFileCodeModel : MarshalByRefObject, EnvDTE.FileCodeModel
	{
		EnvDTE.CodeElements EnvDTE.FileCodeModel.CodeElements
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

		#region FileCodeModel Unsupported Members

		EnvDTE.CodeAttribute EnvDTE.FileCodeModel.AddAttribute(string name, string value, object position)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.CodeClass EnvDTE.FileCodeModel.AddClass(string name, object position, object bases, object implementedInterfaces, EnvDTE.vsCMAccess access)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.CodeDelegate EnvDTE.FileCodeModel.AddDelegate(string name, object type, object position, EnvDTE.vsCMAccess access)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.CodeEnum EnvDTE.FileCodeModel.AddEnum(string name, object position, object bases, EnvDTE.vsCMAccess access)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.CodeFunction EnvDTE.FileCodeModel.AddFunction(string name, EnvDTE.vsCMFunction kind, object type, object position, EnvDTE.vsCMAccess access)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.CodeInterface EnvDTE.FileCodeModel.AddInterface(string name, object position, object bases, EnvDTE.vsCMAccess access)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.CodeNamespace EnvDTE.FileCodeModel.AddNamespace(string name, object position)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.CodeStruct EnvDTE.FileCodeModel.AddStruct(string name, object position, object bases, object implementedInterfaces, EnvDTE.vsCMAccess access)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.CodeVariable EnvDTE.FileCodeModel.AddVariable(string name, object type, object position, EnvDTE.vsCMAccess access)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.CodeElement EnvDTE.FileCodeModel.CodeElementFromPoint(EnvDTE.TextPoint point, EnvDTE.vsCMElement scope)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.DTE EnvDTE.FileCodeModel.DTE
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE.FileCodeModel.Language
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.ProjectItem EnvDTE.FileCodeModel.Parent
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE.FileCodeModel.Remove(object element)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
