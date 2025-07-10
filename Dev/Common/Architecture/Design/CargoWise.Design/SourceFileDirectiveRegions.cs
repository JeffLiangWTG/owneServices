using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace CargoWise.Design.DTE // update the class with the same name in CargoWise.EntityGenerator
{
	[Serializable]
	public abstract class Region : IComparable
	{
		protected Region(int startLine, int endLine)
		{
			StartLine = startLine;
			EndLine = endLine;
		}

		public int StartLine { get; set; }
		public int EndLine { get; set; }

		public bool Contains(Region region)
		{ return region.StartLine >= StartLine && region.EndLine <= EndLine; }

		#region Equals and operators

		public static bool operator ==(Region lhs, Region rhs)
		{ return object.Equals(lhs, rhs); }

		public static bool operator !=(Region lhs, Region rhs)
		{ return !(lhs == rhs); }

		public static bool operator <(Region lhs, Region rhs)
		{ return lhs.CompareTo(rhs) < 0; }

		public static bool operator >(Region lhs, Region rhs)
		{ return lhs.CompareTo(rhs) > 0; }

		public override bool Equals(object obj)
		{
			Region rhs = obj as Region;
			return
				rhs != null &&
				StartLine == rhs.StartLine &&
				EndLine == rhs.EndLine;
		}

		public override int GetHashCode()
		{ return StartLine.GetHashCode(); }

		#endregion

		#region IComparable Members

		int IComparable.CompareTo(object other)
		{ return CompareTo((Region)other); }

		public int CompareTo(Region other)
		{ return StartLine - other.StartLine; }

		#endregion
	}

	[Serializable]
	public class DirectiveRegion : Region
	{
		public DirectiveRegion(int startLine, string directiveText)
			: base(startLine, -1)
		{ DirectiveText = directiveText; }

		public string DirectiveText { get; set; }
	}

	[Serializable]
	public class DSourceFileDirectiveRegions
	{
		public DSourceFileDirectiveRegions(EnvDTE.ProjectItem sourceFile, string directiveNameFilter)
		{
			this.sourceFile = sourceFile;
			this.directiveNameFilter = directiveNameFilter;
			PopulateDirectiveRegions();
			PopulateClassRegions();
		}

		public DirectiveRegion[] GetDirectivesSurroundingType(string typeFullName)
		{
			List<DirectiveRegion> result = new List<DirectiveRegion>();
			foreach (ClassRegion typeRegion in classRegions)
			{
				if (typeRegion.TypeFullName.Replace("+", ".") == typeFullName.Replace("+", "."))
				{
					foreach (DirectiveRegion directiveRegion in directiveRegions)
					{
						if (directiveRegion.Contains(typeRegion))
						{
							result.Add(directiveRegion);
						}
					}
				}
			}
			return result.ToArray();
		}

		public DirectiveRegion[] GetDirectivesSurroundingCodeElement(EnvDTE.CodeElement codeElement)
		{
			List<DirectiveRegion> result = new List<DirectiveRegion>();
			foreach (DirectiveRegion region in directiveRegions)
			{
				if (region.StartLine < codeElement.StartPoint.Line && region.EndLine > codeElement.EndPoint.Line)
				{
					result.Add(region);
				}
			}
			return result.ToArray();
		}

		public DirectiveRegion GetDirective(string directiveText)
		{
			foreach (DirectiveRegion region in directiveRegions)
			{
				if (region.DirectiveText == directiveText)
				{
					return region;
				}
			}
			return null;
		}

		public DirectiveRegion[] GetDirectivesWithinType(string typeFullName)
		{
			List<DirectiveRegion> result = new List<DirectiveRegion>();
			foreach (ClassRegion typeRegion in classRegions)
			{
				if (typeRegion.TypeFullName.Replace("+", ".") == typeFullName.Replace("+", "."))
				{
					foreach (DirectiveRegion directiveRegion in directiveRegions)
					{
						if (typeRegion.Contains(directiveRegion))
						{
							result.Add(directiveRegion);
						}
					}
				}
			}
			return result.ToArray();
		}

		#region Helper Classes

		[Serializable]
		sealed class ClassRegion : Region
		{
			public ClassRegion(EnvDTE.CodeType type)
				: base(type.StartPoint.Line, type.EndPoint.Line)
			{
				int typeArgCount = GetTypeArgumentCount(type.FullName);
				if (typeArgCount > 0)
				{
					TypeFullName = type.FullName.Substring(0, type.FullName.IndexOf('<'));
					TypeFullName += "`" + typeArgCount;
				}
				else
				{
					TypeFullName = type.FullName;
				}
			}

			public string TypeFullName { get; private set; }

			static int GetTypeArgumentCount(string typeName)
			{
				int commaCount = Array.FindAll(typeName.ToCharArray(), delegate(char c)
				{ return c == ','; }).Length;
				return (typeName.IndexOf('<') != -1) ? commaCount + 1 : 0;
			}
		}

		#endregion

		#region Implementation

		[NonSerialized]
		readonly EnvDTE.ProjectItem sourceFile;
		readonly string directiveNameFilter;

		readonly List<DirectiveRegion> directiveRegions = new List<DirectiveRegion>();
		readonly List<ClassRegion> classRegions = new List<ClassRegion>();

		void PopulateDirectiveRegions()
		{
			EnvDTE.TextSelection selection = null;
			try
			{
				EnvDTE.Document document = sourceFile.Document;
				selection = document == null ? null : document.Selection as EnvDTE.TextSelection;
			}
			catch (COMException)
			{
			}
			if (selection != null && selection.Parent != null && sourceFile.IsDirty)
			{
				EnvDTE.EditPoint point = selection.Parent.StartPoint.CreateEditPoint();
				string text = point.GetText(selection.Parent.EndPoint);
				PopulateDirectiveRegions(new StringReader(text));
			}
			else if (sourceFile.FileCount > 0)
			{
				string filename = sourceFile.get_FileNames(1);
				using (StreamReader reader = new StreamReader(filename))
				{
					PopulateDirectiveRegions(reader);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "simple string comparison")]
		void PopulateDirectiveRegions(TextReader reader)
		{
			Stack<DirectiveRegion> pending = new Stack<DirectiveRegion>();

			int i = 1;
			string line;
			do
			{
				line = reader.ReadLine();
				if (line != null)
				{
					string trimmedLine = line.Trim();
					if (trimmedLine.StartsWith("#" + directiveNameFilter, StringComparison.Ordinal))
					{
						string directive = trimmedLine.Substring(("#" + directiveNameFilter).Length).Trim();
						pending.Push(new DirectiveRegion(i, directive));
					}
					if (trimmedLine.StartsWith("#end" + directiveNameFilter, StringComparison.Ordinal) && pending.Count > 0)
					{
						DirectiveRegion directive = pending.Pop();
						directive.EndLine = i;
						directiveRegions.Add(directive);
					}
				}
				i++;
			}
			while (line != null);
			directiveRegions.Sort();
		}

		void PopulateClassRegions()
		{ PopulateClassRegions(sourceFile.FileCodeModel.CodeElements); }

		void PopulateClassRegions(EnvDTE.CodeElements elements)
		{
			foreach (EnvDTE.CodeType codeClass in EnvDTEUtil.GetAllCodeTypes(elements))
			{
				if (codeClass.StartPoint != null && codeClass.EndPoint != null)
				{
					classRegions.Add(new ClassRegion(codeClass));
				}
			}
		}

		#endregion
	}
}
