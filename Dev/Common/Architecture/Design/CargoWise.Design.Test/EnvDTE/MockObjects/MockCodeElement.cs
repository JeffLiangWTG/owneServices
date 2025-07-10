using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockCodeElement : MarshalByRefObject, EnvDTE.CodeElement, EnvDTE80.CodeElement2
	{
		readonly MockProjectItem projectItem;
		string fullName;
		readonly EnvDTE.vsCMElement kind;

		public MockCodeElement(MockProjectItem projectItem, string fullName, EnvDTE.vsCMElement kind)
		{
			this.projectItem = projectItem;
			this.fullName = fullName;
			this.kind = kind;
			if (fullName != typeof(object).FullName && projectItem != null)
			{
				projectItem.ContainingProject.CodeModel.CodeElements.Add(this);
				projectItem.FileCodeModel.CodeElements.Add(this);
			}
		}

		#region CodeElement Members

		public string FullName
		{ get { return fullName; } }

		public string Name
		{
			get { return FullName.Substring(FullName.LastIndexOf(".") + 1); }
			set { fullName = fullName.Substring(0, fullName.Length - Name.Length) + value; }
		}

		public EnvDTE.vsCMElement Kind
		{ get { return kind; } }

		public virtual EnvDTE.ProjectItem ProjectItem
		{ get { return projectItem; } }

		public EnvDTE.TextPoint StartPoint
		{ get { return startPoint; } }
		EnvDTE.TextPoint startPoint;

		public void SetStartPoint(EnvDTE.TextPoint value)
		{ startPoint = value; }

		public EnvDTE.TextPoint EndPoint
		{ get { return endPoint; } }
		EnvDTE.TextPoint endPoint;

		public void SetEndPoint(EnvDTE.TextPoint value)
		{ endPoint = value; }

		#region Unsupported Members

		public EnvDTE.DTE DTE
		{ get { throw new NotSupportedException(); } }

		public object get_Extender(string ExtenderName)
		{ throw new NotSupportedException(); }

		public EnvDTE.CodeElements Collection
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.CodeElements Children
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.TextPoint GetEndPoint(EnvDTE.vsCMPart Part)
		{ throw new NotSupportedException(); }

		public object ExtenderNames
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.vsCMInfoLocation InfoLocation
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.TextPoint GetStartPoint(EnvDTE.vsCMPart Part)
		{ throw new NotSupportedException(); }

		public bool IsCodeType
		{ get { throw new NotSupportedException(); } }

		public string Language
		{ get { throw new NotSupportedException(); } }

		public string ExtenderCATID
		{ get { throw new NotSupportedException(); } }

		public EnvDTE.vsCMAccess Access
		{
			get { return access; }
			set { access = value; }
		}
		EnvDTE.vsCMAccess access;

		#endregion

		#endregion

		#region CodeElement2 Members

		void EnvDTE80.CodeElement2.RenameSymbol(string newName)
		{
			Name = newName;
		}

		string EnvDTE80.CodeElement2.ElementID
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		#endregion
	}
}
