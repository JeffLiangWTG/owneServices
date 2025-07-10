using System;
using System.ComponentModel;
using Microsoft.VisualStudio.VCCodeModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockCodeClass : MockCodeType, EnvDTE.CodeClass, VCCodeClass
	{
		public MockCodeClass(MockProjectItem projectItem, string fullName)
			: base(projectItem, fullName, EnvDTE.vsCMElement.vsCMElementClass)
		{
		}

		#region CodeClass Members

		object EnvDTE.CodeClass.Parent
		{ get { return Parent; } }

		EnvDTE.CodeElements EnvDTE.CodeClass.Bases
		{ get { return Bases; } }

		EnvDTE.CodeElements EnvDTE.CodeClass.Members
		{ get { return Members; } }

		#endregion

		#region VCCodeClass Members

		object VCCodeClass.Parent
		{ get { return Parent; } }

		EnvDTE.CodeElements VCCodeClass.Bases
		{ get { return Bases; } }

		EnvDTE.CodeElements VCCodeClass.Members
		{ get { return Members; } }

		public bool IsSealed
		{
			get { return isSealed; }
			set { isSealed = value; }
		}
		bool isSealed;

		public bool IsAbstract
		{
			get { return isAbstract; }
			set { isAbstract = value; }
		}
		bool isAbstract;

		#region Unsupported Members

		public bool IsReadOnly
		{ get { throw new NotSupportedException(); } }

		public bool IsManaged
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public EnvDTE.CodeElements Templatizations
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeElements Events
		{ get { throw new NotSupportedException(); } }

		public VCCodeTypedef AddTypedef(string Name, object Type, object Position, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public bool ValidateMember(string bstrName, EnvDTE.vsCMElement Kind, string bstrType)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeClass AddClass(string Name, object Position, object Bases, object ImplementedInterfaces, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public bool IsSelf(object pOther)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeElements Structs
		{ get { throw new NotSupportedException(); } }

		public VCCodeEvent AddEvent(string Name, object Type, object Position, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeInterface AddImplementedInterface(object Base, object Position)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeElements Enums
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeEnum AddEnum(string Name, object Position, object Bases, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public VCCodeModel CodeModel
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeElements Unions
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeVariable AddVariable(string Name, object Type, object Position, EnvDTE.vsCMAccess Access, object Location)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeElements Classes
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.Project Project
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeElements ImplementedInterfaces
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeStruct AddStruct(string Name, object Position, object Bases, object ImplementedInterfaces, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public string BodyText
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public EnvDTE.CodeElements Maps
		{ get { throw new NotSupportedException(); } }

		public string File
		{ get { throw new NotSupportedException(); } }

		public string DisplayName
		{ get { throw new NotSupportedException(); } }

		public object Picture
		{ get { throw new NotSupportedException(); } }

		public bool IsZombie
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeDelegate AddDelegate(string Name, object Type, object Position, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeFunction AddFunction(string Name, EnvDTE.vsCMFunction Kind, object Type, object Position, EnvDTE.vsCMAccess Access, object Location)
		{ throw new NotSupportedException(); }

		public EnvDTE.TextPoint get_EndPointOf(EnvDTE.vsCMPart Part, vsCMWhere Where)
		{ throw new NotSupportedException(); }

		public bool IsCaseSensitive
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeElements Typedefs
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeElements Functions
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.TextPoint get_StartPointOf(EnvDTE.vsCMPart Part, vsCMWhere Where)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeElements Variables
		{ get { throw new NotSupportedException(); } }

		public VCCodeUnion AddUnion(string Name, object Position, EnvDTE.vsCMAccess Access)
		{ throw new NotSupportedException(); }

		public bool IsValue
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public bool IsTemplate
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeElements Properties
		{ get { throw new NotSupportedException(); } }

		public bool IsInjected
		{ get { throw new NotSupportedException(); } }

		public string DeclarationText
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public void RemoveInterface(object Element)
		{ throw new NotSupportedException(); }

		public string get_Location(vsCMWhere Where)
		{ throw new NotSupportedException(); }

		public VCCodeMap AddMap(string Name, string ParameterText, object Position, object Location)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeElements TemplateParameters
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeElements References
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeParameter AddTemplateParameter(string Name, object Type, object Position)
		{ throw new NotSupportedException(); }

		public void RemoveTemplateParameter(object Element)
		{ throw new NotSupportedException(); }

		#endregion

		#endregion
	}
}
