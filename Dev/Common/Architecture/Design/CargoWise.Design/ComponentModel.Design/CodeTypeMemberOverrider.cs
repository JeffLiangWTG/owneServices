using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Design.DTE;

namespace CargoWise.ComponentModel.Design
{
	public class CodeTypeMemberOverrider
	{
		public CodeTypeMemberOverrider(IServiceProvider serviceProvider, string componentTypeName)
			: this(GetDTE(serviceProvider), componentTypeName)
		{
		}

		public CodeTypeMemberOverrider(EnvDTE.DTE dte, string componentTypeName)
		{
			this.dte = dte;
			this.componentTypeName = componentTypeName;
		}

		#region NavigateToMember / OverrideOrNavigateToMember

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Required at design time only")]
		public virtual void NavigateToMember(string memberName)
		{
			if (EnsureComponentCodeTypeExists())
			{
				EnvDTE.CodeElement member = FindCodeElementIncludingInherited(ComponentCodeType, memberName);
				if (member != null)
				{
					NavigateToMember(member);
				}
				else
				{
					MessageBox.Show("Member '" + memberName + "' could not be found");
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Required at design time only")]
		public virtual void OverrideOrNavigateToMember(string memberName)
		{
			if (EnsureComponentCodeTypeExists())
			{
				OpenCodeTypeDocument();
				EnvDTE.CodeElement member = FindCodeElementIncludingInherited(ComponentCodeType, memberName);
				EnvDTE.CodeElement memberOnConcreteType = FindCodeElement(ComponentCodeType.Members, memberName);

				bool isDefinatelyNewOrOverridden = member != null && CanDetermineOverrideKind(member) && IsMemberNewOrOverride(member);
				if (member != null && (memberOnConcreteType != null || isDefinatelyNewOrOverridden))
				{
					NavigateToMember(member);
				}
				else if (member == null || (CanDetermineOverrideKind(member) && !IsMemberVirtual(member)))
				{
					MessageBox.Show("Member '" + memberName + "' could not be found or is not marked as virtual");
				}
				else
				{
					if (ComponentCodeType is EnvDTE80.CodeClass2 cls)
					{
						OverrideMember(cls, member);
					}
				}
			}
		}

		#endregion

		#region NavigateToValidationMember / OverrideOrNavigateToValidationMember

		public virtual void NavigateToValidationMember(string propertyName)
		{
			string validationMember = GetValidationMemberAndVerify(propertyName);
			if (validationMember != null)
			{
				NavigateToMember(validationMember);
			}
		}

		public virtual void OverrideOrNavigateToValidationMember(string propertyName)
		{
			string validationMember = GetValidationMemberAndVerify(propertyName);
			if (validationMember != null)
			{
				OverrideOrNavigateToMember(validationMember);
			}
		}

		#endregion

		#region Implementation

		readonly EnvDTE.DTE dte;
		readonly string componentTypeName;

		static EnvDTE.DTE GetDTE(IServiceProvider serviceProvider)
		{
			var service = serviceProvider?.GetService(typeof(EnvDTE.DTE)) ?? throw new ArgumentException("DTE object not available");
			return (EnvDTE.DTE)service;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Required at design time only")]
		bool EnsureComponentCodeTypeExists()
		{
			bool result = true;
			if (ComponentCodeType == null)
			{
				MessageBox.Show("Object type '" + componentTypeName + "' could not be found.");
				result = false;
			}
			return result;
		}

		EnvDTE.Document OpenCodeTypeDocument()
		{
			ComponentCodeType.ProjectItem.Open(EnvDTE.Constants.vsViewKindCode);
			ComponentCodeType.ProjectItem.Document.Activate();
			return ComponentCodeType.ProjectItem.Document;
		}

		#region GetValidationMemberAndVerify

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Required at design time only")]
		string GetValidationMemberAndVerify(string propertyName)
		{
			string validationMember = GetValidationMember(propertyName);
			if (string.IsNullOrEmpty(validationMember))
			{
				MessageBox.Show("Validation member not specified on property '" + propertyName + "'");
				validationMember = null;
			}
			else if (FindCodeElementIncludingInherited(ComponentCodeType, validationMember) == null)
			{
				MessageBox.Show("Could not find validation member '" + validationMember + "'");
				validationMember = null;
			}
			return validationMember;
		}

		string GetValidationMember(string propertyName)
		{ return GetValidationMember(ComponentCodeType, propertyName); }

		string GetValidationMember(EnvDTE.CodeType codeType, string propertyName)
		{
			if (FindCodeElementIncludingInherited(codeType, propertyName) is EnvDTE.CodeProperty property)
			{
				foreach (EnvDTE.CodeAttribute attr in property.Attributes)
				{
					if (attr.Name.EndsWith(nameof(NotificationsMemberAttribute), StringComparison.Ordinal) ||
							attr.Name.EndsWith(nameof(NotificationsMemberAttribute).Replace("Attribute", ""), StringComparison.Ordinal))
					{
						return attr.Value.Replace("\"", "");
					}
				}

				EnvDTE.CodeClass baseClass = codeType.Bases.Count == 0 ? null : codeType.Bases.Item(1) as EnvDTE.CodeClass;
				if (baseClass != null)
				{
					return GetValidationMember((EnvDTE.CodeType)baseClass, propertyName);
				}
			}
			return null;
		}

		#endregion

		#region NavigateToMember

		void NavigateToMember(EnvDTE.CodeElement member)
		{
			OpenCodeTypeDocument();
			EnvDTE.TextPoint point = GetMemberNavigationPoint(member);

			member.ProjectItem.Open(null);
			if (member.ProjectItem.Document != null && member.ProjectItem.Document.Selection is EnvDTE.TextSelection)
			{
				ForceMoveSelectionToPoint((EnvDTE.TextSelection)member.ProjectItem.Document.Selection, point);
				if (point.Parent.Selection.CurrentColumn == 1)
				{
					point.Parent.Selection.StartOfLine(EnvDTE.vsStartOfLineOptions.vsStartOfLineOptionsFirstText, false);
				}
				point.TryToShow(EnvDTE.vsPaneShowHow.vsPaneShowCentered, null);
			}
		}

		static EnvDTE.TextPoint GetMemberNavigationPoint(EnvDTE.CodeElement member)
		{
			EnvDTE.CodeElement memberToGetPointOn = member;
			if (member is EnvDTE.CodeProperty property && property.Getter != null)
			{
				memberToGetPointOn = (EnvDTE.CodeElement)property.Getter;
			}

			if (!(member is EnvDTE.CodeFunction))
			{
				try
				{
					return memberToGetPointOn.GetStartPoint(EnvDTE.vsCMPart.vsCMPartBodyWithDelimiter);
				}
				catch (NotImplementedException) { }
				catch (COMException) { }
				try
				{
					return memberToGetPointOn.GetStartPoint(EnvDTE.vsCMPart.vsCMPartBody);
				}
				catch (NotImplementedException) { }
				catch (COMException) { }
			}
			try
			{
				return memberToGetPointOn.GetStartPoint(EnvDTE.vsCMPart.vsCMPartNavigate);
			}
			catch (NotImplementedException) { }
			catch (COMException) { }

			return memberToGetPointOn.StartPoint;
		}

		static void ForceMoveSelectionToPoint(EnvDTE.TextSelection selection, EnvDTE.TextPoint point)
		{
			selection.MoveTo(point.Line, 1, false);
			selection.EndOfLine(false);
			if (selection.CurrentColumn < point.DisplayColumn)
			{
				selection.Insert(new string(' ', point.DisplayColumn - selection.CurrentColumn), (int)EnvDTE.vsInsertFlags.vsInsertFlagsCollapseToEnd);
			}
			else
			{
				selection.MoveTo(point.Line, point.DisplayColumn, false);
			}
		}

		#endregion

		#region OverrideMember

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		void OverrideMember(EnvDTE80.CodeClass2 cls, EnvDTE.CodeElement baseMember)
		{
			dte.UndoContext.Open("Override member " + baseMember.Name, false);
			try
			{
				OverrideMemberWithoutUndoContext(cls, baseMember);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				try
				{
					dte.UndoContext.SetAborted();
				}
				catch (Exception ex1) when (!ex1.IsCriticalException())
				{
				}
				throw;
			}
			dte.UndoContext.Close();
		}

		void OverrideMemberWithoutUndoContext(EnvDTE80.CodeClass2 cls, EnvDTE.CodeElement baseMember)
		{
			EnvDTE.CodeElement overriddenMember = null;
			if (baseMember is EnvDTE.CodeProperty baseProperty)
			{
				overriddenMember = (EnvDTE.CodeElement)OverrideProperty(cls, baseMember, baseProperty);
			}
			if (baseMember is EnvDTE.CodeFunction baseMethod)
			{
				EnvDTE.CodeFunction overriddenMethod = cls.AddFunction(baseMethod.Name, baseMethod.FunctionKind, baseMethod.Type, -1, baseMethod.Access, null);
				foreach (EnvDTE.CodeParameter param in baseMethod.Parameters)
				{
					overriddenMethod.AddParameter(param.Name, param.Type.AsFullName.Substring(param.Type.AsFullName.LastIndexOf('.') + 1), null);
				}
				overriddenMember = (EnvDTE.CodeElement)overriddenMethod;
			}

			if (overriddenMember != null)
			{
				AddOverrideKeywordToMember(overriddenMember);
				if (cls.Language == EnvDTE.CodeModelLanguageConstants.vsCMLanguageCSharp)
				{
					MoveCodeMemberIntoMatchingBaseRegionName(cls, baseMember, overriddenMember);
				}
				NavigateToMember(baseMember.Name);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		static EnvDTE.CodeProperty OverrideProperty(EnvDTE80.CodeClass2 cls, EnvDTE.CodeElement baseMember, EnvDTE.CodeProperty baseProperty)
		{
			EnvDTE.CodeProperty overriddenProperty = cls.AddProperty(baseProperty.Name, baseProperty.Name, baseProperty.Type, -1, baseProperty.Access, null);
			EnvDTE.TextSelection selection = overriddenProperty.StartPoint.Parent.Selection;
			if (baseProperty.Getter != null)
			{
				SetMethodBody(overriddenProperty.Getter, "return base." + baseMember.Name + ";");
			}
			else
			{
				selection.MoveToPoint(overriddenProperty.Getter.StartPoint, false);
				selection.StartOfLine(EnvDTE.vsStartOfLineOptions.vsStartOfLineOptionsFirstColumn, false);
				selection.MoveToPoint(overriddenProperty.Getter.EndPoint, true);
				selection.LineDown(true, 1);
				selection.StartOfLine(EnvDTE.vsStartOfLineOptions.vsStartOfLineOptionsFirstColumn, true);
				selection.Text = "";
			}
			if (baseProperty.Setter != null)
			{
				SetMethodBody(overriddenProperty.Setter, "base." + baseMember.Name + " = value;");
			}
			else
			{
				selection.MoveToPoint(overriddenProperty.Setter.StartPoint, false);
				selection.StartOfLine(EnvDTE.vsStartOfLineOptions.vsStartOfLineOptionsFirstColumn, false);
				selection.MoveToPoint(overriddenProperty.Setter.EndPoint, true);
				selection.LineDown(true, 1);
				selection.StartOfLine(EnvDTE.vsStartOfLineOptions.vsStartOfLineOptionsFirstColumn, true);
				selection.Text = "";
			}
			return overriddenProperty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		void MoveCodeMemberIntoMatchingBaseRegionName(EnvDTE80.CodeClass2 cls, EnvDTE.CodeElement baseMember, EnvDTE.CodeElement overriddenMember)
		{
			EnvDTE.Document document = OpenCodeTypeDocument();
			EnvDTE.TextSelection selection = (EnvDTE.TextSelection)document.Selection;

			DSourceFileDirectiveRegions baseTypeFileRegions = new DSourceFileDirectiveRegions(baseMember.ProjectItem, "region");
			DirectiveRegion[] baseTypeRegions = baseTypeFileRegions.GetDirectivesSurroundingCodeElement(baseMember);
			int endOfRegionLine = GetEndOfRegionLineMatchingBaseRegionName_InsertNestedRegionsIfRequired(cls, overriddenMember.StartPoint.DisplayColumn - 1, baseTypeRegions);
			MoveOverriddenMemberIntoRegion(cls, selection, endOfRegionLine, overriddenMember, baseTypeRegions);
		}

		static void MoveOverriddenMemberIntoRegion(EnvDTE80.CodeClass2 cls, EnvDTE.TextSelection selection, int endOfRegionLine, EnvDTE.CodeElement overriddenMember, DirectiveRegion[] baseTypeRegions)
		{
			int memberToDeleteStartLine = overriddenMember.StartPoint.Line;
			int memberToDeleteEndLine = overriddenMember.EndPoint.Line;
			selection.MoveTo(overriddenMember.StartPoint.Line, 1, false);
			selection.MoveTo(overriddenMember.EndPoint.Line + 1, 1, true);

			string memberText = selection.Text.TrimEnd('\n', '\r');
			if (baseTypeRegions.Length > 0)
			{
				memberText += "\r\n";
			}
			selection.MoveTo(endOfRegionLine - 1, 1, false);
			selection.EndOfLine(true);
			if (overriddenMember.EndPoint.Line > endOfRegionLine)
			{
				memberToDeleteStartLine += memberText.Split('\n').Length;
				memberToDeleteEndLine += memberText.Split('\n').Length;
			}

			selection.MoveTo(endOfRegionLine - 1, 1, false);
			selection.EndOfLine(false);
			selection.NewLine(1);
			endOfRegionLine++;
			selection.MoveTo(endOfRegionLine - 1, 1, false);
			selection.Text = "";
			selection.Insert(memberText, (int)EnvDTE.vsInsertFlags.vsInsertFlagsInsertAtStart);

			selection.MoveTo(memberToDeleteStartLine - 1, 1, false);
			selection.EndOfLine(true);
			if (selection.Text.Trim().Length == 0)
			{
				memberToDeleteStartLine--;
			}
			selection.MoveTo(memberToDeleteStartLine, 1, false);
			selection.MoveTo(memberToDeleteEndLine + 1, 1, true);
			selection.Text = "";

			InsertLeadingNewlineToMemberAndRegionIfLooksNice(selection, cls, overriddenMember.Name);
		}

		static void InsertLeadingNewlineToMemberAndRegionIfLooksNice(EnvDTE.TextSelection selection, EnvDTE80.CodeClass2 cls, string memberName)
		{
			EnvDTE.CodeElement member = FindCodeElement(cls.Members, memberName);
			if (member != null)
			{
				int memberAndRegionStartLine = LocateFirstImmediateStartRegion(selection, member);
				InsertLeadingNewlineToMemberAndRegionIfLooksNice(selection, memberAndRegionStartLine);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "simple string comparison")]
		static int LocateFirstImmediateStartRegion(EnvDTE.TextSelection selection, EnvDTE.CodeElement member)
		{
			int result = member.StartPoint.Line;
			int line = member.StartPoint.Line;
			do
			{
				line--;
				selection.MoveTo(line, 1, false);
				selection.EndOfLine(true);
				if (selection.Text.Trim().StartsWith("#region", StringComparison.Ordinal))
				{
					result = line;
				}
			}
			while (selection.Text.Trim().Length == 0 || selection.Text.Trim().StartsWith("#region", StringComparison.Ordinal));
			return result;
		}

		static void InsertLeadingNewlineToMemberAndRegionIfLooksNice(EnvDTE.TextSelection selection, int memberAndRegionStartLine)
		{
			selection.MoveTo(memberAndRegionStartLine - 1, 1, false);
			selection.EndOfLine(true);
			if (!selection.Text.Trim().StartsWith("{", StringComparison.Ordinal) && selection.Text.Trim().Length > 0)
			{
				selection.MoveTo(memberAndRegionStartLine, 1, false);
				selection.Insert("\r\n", (int)EnvDTE.vsInsertFlags.vsInsertFlagsCollapseToEnd);
			}
		}

		static void AddOverrideKeywordToMember(EnvDTE.CodeElement member)
		{
			EnvDTE.TextSelection selection = member.StartPoint.Parent.Selection;
			selection.MoveToPoint(member.StartPoint, false);
			selection.EndOfLine(true);

			#region SuppressResourceStringsCheckRegion
			TryAddOverrideKeyword(selection, "public");
			TryAddOverrideKeyword(selection, "internal");
			TryAddOverrideKeyword(selection, "protected");
			TryAddOverrideKeyword(selection, "protected internal");
			#endregion
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		static void TryAddOverrideKeyword(EnvDTE.TextSelection selection, string accessModifier)
		{
			if (selection.Text.Trim().StartsWith(accessModifier, StringComparison.Ordinal))
			{
				selection.Text = selection.Text.Insert(selection.Text.IndexOf(accessModifier, StringComparison.Ordinal) + accessModifier.Length + 1, "override ");
			}
		}

		static void SetMethodBody(EnvDTE.CodeFunction method, string bodyText)
		{
			EnvDTE.TextPoint startPoint = method.GetStartPoint(EnvDTE.vsCMPart.vsCMPartBody);
			EnvDTE.TextPoint endPoint = method.GetEndPoint(EnvDTE.vsCMPart.vsCMPartBody);
			startPoint.Parent.Selection.MoveToPoint(method.GetStartPoint(EnvDTE.vsCMPart.vsCMPartBody), false);
			startPoint.Parent.Selection.LineUp(false, 1);
			startPoint.Parent.Selection.StartOfLine(EnvDTE.vsStartOfLineOptions.vsStartOfLineOptionsFirstText, false);
			startPoint.Parent.Selection.StartOfLine(EnvDTE.vsStartOfLineOptions.vsStartOfLineOptionsFirstColumn, true);
			string indent = startPoint.Parent.Selection.Text + new string(' ', startPoint.Parent.IndentSize);

			startPoint.Parent.Selection.MoveToPoint(startPoint, false);
			endPoint.Parent.Selection.MoveToPoint(endPoint, true);
			startPoint.Parent.Selection.Text = "";
			startPoint.Parent.Selection.Insert(indent + bodyText + "\n", (int)EnvDTE.vsInsertFlags.vsInsertFlagsCollapseToEnd);
		}

		#endregion

		#region GetEndOfRegionLineMatchingBaseRegionName_InsertNestedRegionsIfRequired

		int GetEndOfRegionLineMatchingBaseRegionName_InsertNestedRegionsIfRequired(EnvDTE80.CodeClass2 cls, int indent, DirectiveRegion[] baseTypeRegions)
		{
			EnvDTE.Document document = OpenCodeTypeDocument();
			EnvDTE.TextSelection selection = (EnvDTE.TextSelection)document.Selection;

			int startLine = cls.StartPoint.Line;
			int endLine = cls.EndPoint.Line;
			for (int i = 0; i < baseTypeRegions.Length; i++)
			{
				DirectiveRegion baseTypeRegion = baseTypeRegions[i];
				if (!FindRegionWithinLines(cls, baseTypeRegion.DirectiveText, ref startLine, ref endLine))
				{
					InsertRegion(selection, baseTypeRegion.DirectiveText, endLine, indent);
					endLine += 2;
				}
			}
			return endLine;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		static bool FindRegionWithinLines(EnvDTE80.CodeClass2 cls, string regionText, ref int startLine, ref int endLine)
		{
			DSourceFileDirectiveRegions typeFileRegions = new DSourceFileDirectiveRegions(cls.ProjectItem, "region");
			foreach (DirectiveRegion region in typeFileRegions.GetDirectivesWithinType(cls.FullName))
			{
				if (region.DirectiveText == regionText && region.StartLine >= startLine && region.EndLine <= endLine)
				{
					startLine = region.StartLine;
					endLine = region.EndLine;
					return true;
				}
			}
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "just building a string for insertion, simple string comparison")]
		static void InsertRegion(EnvDTE.TextSelection selection, string regionName, int line, int indent)
		{
			selection.MoveTo(line, 1, false);
			selection.EndOfLine(true);
			string currentLine = selection.Text;

			selection.MoveTo(line, 1, false);
			string textToInsert =
					new string(' ', indent) + "#region " + regionName + "\r\n" +
					"\r\n" +
					new string(' ', indent) + "#endregion\r\n";
			if (currentLine.Trim().StartsWith("#endregion", StringComparison.Ordinal))
			{
				textToInsert += "\r\n";
			}
			selection.Insert(textToInsert, (int)EnvDTE.vsInsertFlags.vsInsertFlagsCollapseToEnd);
		}

		#endregion

		#region ComponentCodeType

		EnvDTE.CodeType ComponentCodeType
		{
			get
			{
				if (!componentCodeTypePopulated)
				{
					componentCodeType = CodeTypeFromFullName(componentTypeName);
					componentCodeTypePopulated = true;
				}
				return componentCodeType;
			}
		}
		bool componentCodeTypePopulated;
		EnvDTE.CodeType componentCodeType;

		EnvDTE.CodeType CodeTypeFromFullName(string typeName)
		{
			foreach (EnvDTE.Project project in (IEnumerable)dte.ActiveSolutionProjects)
			{
				EnvDTE.CodeType type = EnvDTEUtil.CodeTypeFromFullName(project, typeName);
				if (type != null)
				{
					return type;
				}
			}
			return null;
		}

		#endregion

		#region FindCodeElementIncludingInherited / FindCodeElement

		EnvDTE.CodeElement FindCodeElementIncludingInherited(EnvDTE.CodeType codeType, string name)
		{
			EnvDTE.CodeElement result = FindCodeElement(codeType.Members, name);
			EnvDTE.CodeClass baseClass = codeType.Bases.Count == 0 ? null : codeType.Bases.Item(1) as EnvDTE.CodeClass;
			if (result == null && baseClass != null)
			{
				result = FindCodeElementIncludingInherited((EnvDTE.CodeType)baseClass, name);
			}
			return result;
		}

		static EnvDTE.CodeElement FindCodeElement(EnvDTE.CodeElements codeElements, string name)
		{
			foreach (EnvDTE.CodeElement element in codeElements)
			{
				if (element.Name == name)
				{
					return element;
				}
			}
			return null;
		}

		#endregion

		#region CanDetermineOverrideKind / IsMemberVirtual / IsMemberNewOrOverride

		static bool CanDetermineOverrideKind(EnvDTE.CodeElement member)
		{ return member is EnvDTE80.CodeProperty2 || member is EnvDTE80.CodeFunction2; }

		static bool IsMemberVirtual(EnvDTE.CodeElement member)
		{
			EnvDTE80.vsCMOverrideKind kind = GetMemberOverrideKind(member);
			return (kind & EnvDTE80.vsCMOverrideKind.vsCMOverrideKindVirtual) == EnvDTE80.vsCMOverrideKind.vsCMOverrideKindVirtual;
		}

		static bool IsMemberNewOrOverride(EnvDTE.CodeElement member)
		{
			EnvDTE80.vsCMOverrideKind kind = GetMemberOverrideKind(member);
			return
					(kind & EnvDTE80.vsCMOverrideKind.vsCMOverrideKindNew) == EnvDTE80.vsCMOverrideKind.vsCMOverrideKindNew ||
					(kind & EnvDTE80.vsCMOverrideKind.vsCMOverrideKindOverride) == EnvDTE80.vsCMOverrideKind.vsCMOverrideKindOverride;
		}

		static EnvDTE80.vsCMOverrideKind GetMemberOverrideKind(EnvDTE.CodeElement member)
		{
			EnvDTE80.vsCMOverrideKind kind = EnvDTE80.vsCMOverrideKind.vsCMOverrideKindNone;
			if (member is EnvDTE80.CodeProperty2 property)
			{
				kind = property.OverrideKind;
			}
			if (member is EnvDTE80.CodeFunction2 method)
			{
				kind = method.OverrideKind;
			}
			return kind;
		}

		#endregion

		#endregion
	}
}
