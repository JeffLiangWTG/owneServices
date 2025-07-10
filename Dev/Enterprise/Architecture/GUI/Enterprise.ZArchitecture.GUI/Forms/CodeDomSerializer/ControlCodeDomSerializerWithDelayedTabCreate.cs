#if DEBUG
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Design;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Generates methods for lazy initialization of ZTabPages for performance. To use, apply this to
	/// your form and container control classes:
	/// 
	/// [DesignerSerializer(typeof(ControlCodeDomSerializerWithDelayedTabCreate), typeof(CodeDomSerializer))]
	/// 
	/// If you need to revert form changes so that no tab methods are used (and the default designer
	/// serializer can function without this custom serializer), follow these steps:
	/// 
	/// - Go to the InitializeComponent() method of the form.
	/// - Remove all BindingOrFirstShown event attach statements.
	/// - Move the content of methods that end with _InitializeTab to the bottom of InitializeComponent().
	/// </summary>
	internal class ControlCodeDomSerializerWithDelayedTabCreate : ControlMostDerivedTypeCodeDomSerializer
	{
		#region Serialize

		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			if (QueryShouldSerializeTabPageMethods(manager, value))
			{
				EnsureWarnOfBuildDuringDesign(manager);
				lastTabPageType = TabPageType;
				manager.SerializationComplete -= new EventHandler(SerializationManager_SerializationComplete);
				manager.SerializationComplete += new EventHandler(SerializationManager_SerializationComplete);
			}
			var result = (CodeStatementCollection)base.Serialize(manager, value);
			return result;
		}

		// Must be static as Serialize() can be called on different instances of the serializer.
		static void SerializationManager_SerializationComplete(object sender, EventArgs e)
		{
			var manager = (IDesignerSerializationManager)sender;
			var typeDeclaration = (CodeTypeDeclaration)manager.GetService(typeof(CodeTypeDeclaration));
			new PostSerializationWorker(manager, typeDeclaration, lastTabPageType).DoPostSerializationWork();
		}
		[ThreadStatic]
		static Type lastTabPageType;

		protected virtual Type TabPageType
		{
			get { return typeof(ZTabPage); }
		}

		class PostSerializationWorker
		{
			public PostSerializationWorker(IServiceProvider serviceProvider, CodeTypeDeclaration typeDeclaration, Type tabPageType)
			{
				this.serviceProvider = serviceProvider;
				this.typeDeclaration = typeDeclaration;
				this.TabPageType = tabPageType;
			}

			#region DoPostSerializationWork

			public void DoPostSerializationWork()
			{
				if (InitializeComponentMethod != null)
				{
					var tabPageFieldNames = new List<string>(GetTabPageReferenceNames());
					tabPageFieldNames.Reverse();

					var controlsThatHaveIncompatibleAnchor = new List<string>();
					foreach (var tabPageFieldName in tabPageFieldNames)
					{
						if (CreateOrUpdateInitializeTabMethod(tabPageFieldName) != null)
						{
							var tabPage = InsertTabPageRunAfterBindMethod(tabPageFieldName);
							foreach (Control control in tabPage.Controls)
							{
								if (control.Dock == DockStyle.None && control.Anchor != (AnchorStyles.Top | AnchorStyles.Left))
								{
									controlsThatHaveIncompatibleAnchor.Add(control.Name);
								}
							}
						}
					}
					if (controlsThatHaveIncompatibleAnchor.Count > 0)
					{
						DesignTimeUI.ShowMessage(
							serviceProvider,
							"WARNING: All tab pages must have either:\r\n" +
							"- A single panel with DockStyle.Fill on which the controls sit.\r\n" +
							"- Otherwise all controls on the tab page must have AnchorStyle.Top | AnchorStyle.Left.\r\n" +
							"This is because, with delayed tab create and anchors other than Top|Left, resizing the\r\n" +
							"form before clicking on the tab page causes controls to end up in the wrong position\r\n" +
							"on the tab page (if you try this functionally now you may see this behaviour).\r\n" +
							"\r\n" +
							"The following controls that sit on tab pages are in error:\r\n" +
							"\r\n" +
							string.Join("\r\n", controlsThatHaveIncompatibleAnchor.ToArray()));
					}

					RemoveUnusedTabPageMethods(tabPageFieldNames.ToArray());
					CommitCodeDomChanges();
				}
			}

			void RemoveUnusedTabPageMethods(IList<string> tabPageFieldNames)
			{
				foreach (var method in new List<CodeMemberMethod>(GetTabPageCodeMethods(typeDeclaration)))
				{
					if (!tabPageFieldNames.Contains(method.Name.Replace(TabPageMethodNamePostfix, "")))
					{
						typeDeclaration.Members.Remove(method);
					}
				}
			}

			TabPage InsertTabPageRunAfterBindMethod(string tabPageFieldName)
			{
				var tabPage = ReferenceService.GetReference(tabPageFieldName) as TabPage;
				if (tabPage != null && TabPageType.IsInstanceOfType(tabPage))
				{
					var statement = CreateRunAfterBindMethodStatement(tabPageFieldName);
					InsertTabPageRunAfterBindMethod(tabPage, statement);
				}
				return tabPage;
			}

			CodeExpressionStatement CreateRunAfterBindMethodStatement(string tabPageFieldName)
			{
				var tabPageFieldReference = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), tabPageFieldName);
				var methodInvokeExpr = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(tabPageFieldReference, ZTabPage.RunWhenBindingOrFirstShownMethod));
				var delegateCreate = new CodeDelegateCreateExpression(new CodeTypeReference(typeof(EventHandler)), new CodeThisReferenceExpression(), GetTabPageMethodName(tabPageFieldName));
				methodInvokeExpr.Parameters.Add(delegateCreate);
				return new CodeExpressionStatement(methodInvokeExpr);
			}

			void InsertTabPageRunAfterBindMethod(TabPage tabPage, CodeExpressionStatement runAfterBindStatement)
			{
				var found = false;
				foreach (var method in AllCodeMethods)
				{
					if (method.Name != GetTabPageMethodName(tabPage.Name))
					{
						var i = GetTabPageFieldCodeInsertIndex(method.Statements, tabPage);
						if (i != -1)
						{
							method.Statements.Insert(i, runAfterBindStatement);
							found = true;
							break;
						}
					}
				}
				if (!found)
				{
					InitializeComponentMethod.Statements.Add(runAfterBindStatement);
				}
			}

			int GetTabPageFieldCodeInsertIndex(CodeStatementCollection collection, TabPage tabPage)
			{
				var result = GetAfterLastPropertyAssignmentOrBeforeResumeLayoutStatementIndex(collection, tabPage.Name);
				if (result == -1 && tabPage.Parent != null)
				{
					result = GetAfterLastPropertyAssignmentOrBeforeResumeLayoutStatementIndex(collection, tabPage.Parent.Name);
				}
				return result;
			}

			int GetAfterLastPropertyAssignmentOrBeforeResumeLayoutStatementIndex(CodeStatementCollection collection, string controlFieldName)
			{
				var result = -1;
				for (var i = 0; i < collection.Count; i++)
				{
					var statement = collection[i];
					var assignStatement = statement as CodeAssignStatement;
					if (assignStatement != null)
					{
						var propertyReference = assignStatement.Left as CodePropertyReferenceExpression;
						var fieldReference = propertyReference == null ? null : propertyReference.TargetObject as CodeFieldReferenceExpression;
						if (fieldReference != null)
						{
							if (fieldReference.FieldName == controlFieldName)
							{
								result = i + 1;
							}
						}
					}

					if (result == -1)
					{
						result = GetIndexBeforeStatementIfResumeLayout(collection, i, controlFieldName);
					}
				}
				return result;
			}

			int GetIndexBeforeStatementIfResumeLayout(CodeStatementCollection statements, int i, string controlFieldName)
			{
				var exprStatement = statements[i] as CodeExpressionStatement;
				var methodInvoke = exprStatement == null ? null : exprStatement.Expression as CodeMethodInvokeExpression;
				if (methodInvoke != null && methodInvoke.Method.MethodName == "ResumeLayout")
				{
					foreach (var next in GetObjectNamesExpressionAppliesTo(statements, i, methodInvoke))
					{
						if (next == controlFieldName)
						{
							return i;
						}
					}
				}
				return -1;
			}

			#endregion

			#region CreateOrUpdateInitializeTabMethod

			CodeMemberMethod CreateOrUpdateInitializeTabMethod(string tabPageFieldName)
			{
				var method = FindOrCreateInitializeTabMethod(tabPageFieldName);
				method.Statements.Clear();
				var tabPage = (TabPage)ReferenceService.GetReference(tabPageFieldName);

				MoveStatementsIntoTabPageMethod(tabPage, method.Statements);
				CodeMemberMethod result = null;
				if (method.Statements.Count > 0)
				{
					var comment = nameof(ControlCodeDomSerializerWithDelayedTabCreate) + " Designer generated code";
					method.Statements.Insert(0, new CodeCommentStatement(""));
					method.Statements.Insert(1, new CodeCommentStatement(comment));
					method.Statements.Insert(2, new CodeCommentStatement(""));

					var i = typeDeclaration.Members.IndexOf(InitializeComponentMethod);
					typeDeclaration.Members.Insert(i + 1, method);
					result = method;
				}
				return result;
			}

			CodeMemberMethod FindOrCreateInitializeTabMethod(string tabPageFieldName)
			{
				foreach (CodeTypeMember member in typeDeclaration.Members)
				{
					var method = member as CodeMemberMethod;
					if (method != null && member.Name == tabPageFieldName + TabPageMethodNamePostfix)
					{
						return method;
					}
				}
				return CreateNewInitializeTabMethod(tabPageFieldName);
			}

			CodeMemberMethod CreateNewInitializeTabMethod(string tabPageFieldName)
			{
				var result = new CodeMemberMethod();
				result.Name = GetTabPageMethodName(tabPageFieldName);

				result.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "sender"));
				result.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(EventArgs).FullName), "e"));
				return result;
			}

			void MoveStatementsIntoTabPageMethod(TabPage tabPage, CodeStatementCollection methodStatements)
			{
				CodeStatement tabPageResumeLayoutStatement = null;
				var statementsToDefer = new List<CodeStatement>();
				for (var i = 0; i < InitializeComponentMethod.Statements.Count; i++)
				{
					if (MustDeferStatementForTabPageOrChild(tabPage, InitializeComponentMethod.Statements, i))
					{
						var statement = InitializeComponentMethod.Statements[i];
						statementsToDefer.Add(statement);
						if (IsTabPageResumeLayout(InitializeComponentMethod.Statements, i, tabPage))
						{
							tabPageResumeLayoutStatement = statement;
						}
					}
				}
				MoveStatements(InitializeComponentMethod.Statements, methodStatements, statementsToDefer);
				if (tabPageResumeLayoutStatement != null)
				{
					ChangeResumeLayoutArgumentToTrueAndMoveToEnd(tabPageResumeLayoutStatement, methodStatements);
				}
			}

			void MoveStatements(CodeStatementCollection from, CodeStatementCollection to, IEnumerable<CodeStatement> statementsToMove)
			{
				foreach (var statement in statementsToMove)
				{
					from.Remove(statement);
					to.Add(statement);
				}
			}

			bool IsTabPageResumeLayout(CodeStatementCollection collection, int i, TabPage tabPage)
			{
				var exprStatement = collection[i] as CodeExpressionStatement;
				var methodInvoke = exprStatement == null ? null : exprStatement.Expression as CodeMethodInvokeExpression;
				if (methodInvoke != null && methodInvoke.Method.MethodName == "ResumeLayout")
				{
					foreach (var name in GetObjectNamesStatementAppliesTo(collection, i, collection[i]))
					{
						if (name == tabPage.Name)
						{
							return true;
						}
					}
				}
				return false;
			}

			void ChangeResumeLayoutArgumentToTrueAndMoveToEnd(CodeStatement statement, CodeStatementCollection methodStatements)
			{
				var exprStatement = (CodeExpressionStatement)statement;
				var methodInvoke = (CodeMethodInvokeExpression)exprStatement.Expression;
				if (methodInvoke.Parameters.Count == 1)
				{
					methodInvoke.Parameters[0] = new CodePrimitiveExpression(true);
				}
				methodStatements.Remove(statement);
				methodStatements.Add(statement);
			}

			string GetTabPageMethodName(string fieldName)
			{
				return fieldName + TabPageMethodNamePostfix;
			}

			CodeMemberMethod InitializeComponentMethod
			{
				get
				{
					if (initializeComponentMethod == null)
					{
						foreach (CodeTypeMember member in typeDeclaration.Members)
						{
							var method = member as CodeMemberMethod;
							if (method != null && method.Name == "InitializeComponent")
							{
								initializeComponentMethod = method;
								break;
							}
						}
					}
					return initializeComponentMethod;
				}
			}
			CodeMemberMethod initializeComponentMethod;

			#endregion

			#region MustDeferStatementForTabPageOrChildStatement

			bool MustDeferStatementForTabPageOrChild(TabPage tabPage, CodeStatementCollection statements, int i)
			{
				var statement = statements[i];
				var statementComponent = GetComponentStatementAppliesTo(statements, i, statement);
				return IsChildOfTabPage(tabPage, statementComponent) || MustDeferStatementOnTabPage(tabPage, statement, statementComponent);
			}

			bool IsChildOfTabPage(TabPage tabPage, IComponent component)
			{
				var control = component as Control;
				var parentStatementTabPage = control == null ? null : GetTabPage(control.Parent);
				return parentStatementTabPage == tabPage;
			}

			bool MustDeferStatementOnTabPage(TabPage tabPage, CodeStatement statement, IComponent statementComponent)
			{
				var statementTabPage = GetTabPage(statementComponent);
				var result = false;
				if (statementTabPage != null)
				{
					var isStatementDirectlyForTabPage = statementTabPage == tabPage;
					result =
						isStatementDirectlyForTabPage &&
						(!TabPageType.IsInstanceOfType(statementComponent) || IsControlAddOrSetChildIndexMethodInvoke(statement) || IsControlSuspendResumeOrPerformLayout(statement));
				}
				return result;
			}

			TabPage GetTabPage(IComponent component)
			{
				var control = component as Control;
				while (control != null)
				{
					var tabPage = control as TabPage;
					if (tabPage != null && TabPageType.IsInstanceOfType(tabPage))
					{
						return tabPage;
					}
					control = control.Parent;
				}
				return null;
			}

			#endregion

			#region CommitCodeDomChanges

			void CommitCodeDomChanges()
			{
				PreventMembersBeingDeletedIncorrectlyByVS(serviceProvider, typeDeclaration);
				CodeDomAdapter.GetType().InvokeMember("Generate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, CodeDomAdapter, null);
			}

			#endregion

			#region GetComponentStatementAppliesTo

			IComponent GetComponentStatementAppliesTo(CodeStatementCollection collection, int i, CodeStatement statement)
			{
				IComponent result = null;
				foreach (var name in GetObjectNamesStatementAppliesTo(collection, i, statement))
				{
					result = GetComponentFromName(collection, i, statement, name) ?? result;
					if (result != null && !(result is IExtenderProvider))
					{
						break;
					}
				}
				return result;
			}

			IComponent GetComponentFromName(CodeStatementCollection collection, int i, CodeStatement statement, string name)
			{
				var reference = ReferenceService.GetReference(name);
				if (reference == null)
				{
					for (var relatedStatementIndex = i + 1; relatedStatementIndex < collection.Count; relatedStatementIndex++)
					{
						var nextStatement = collection[relatedStatementIndex];
						var objectNames = new List<string>(GetObjectNamesStatementAppliesTo(collection, i, nextStatement));
						if (objectNames.Contains(name))
						{
							foreach (var nextName in objectNames)
							{
								reference = ReferenceService.GetReference(nextName);
								var isExtenderProviderMethodInvoke = objectNames.Count == 2 && typeof(IExtenderProvider).IsInstanceOfType(reference);
								if (!isExtenderProviderMethodInvoke && reference != null)
								{
									break;
								}
							}
						}
					}
				}
				return reference == null ? null : ReferenceService.GetComponent(reference);
			}

			#endregion

			#region GetObjectNamesStatementAppliesTo

			IEnumerable<string> GetObjectNamesStatementAppliesTo(CodeStatementCollection collection, int i, CodeStatement statement)
			{
				var assignStatement = statement as CodeAssignStatement;
				if (assignStatement != null)
				{
					foreach (var name in GetObjectNamesStatementAppliesTo(collection, i, assignStatement))
					{
						yield return name;
					}
				}

				var exprStatement = statement as CodeExpressionStatement;
				if (exprStatement != null)
				{
					foreach (var name in GetObjectNamesExpressionAppliesTo(collection, i, exprStatement.Expression))
					{
						yield return name;
					}
				}

				var attachEventStatement = statement as CodeAttachEventStatement;
				if (attachEventStatement != null)
				{
					foreach (var name in GetObjectNamesExpressionAppliesTo(collection, i, attachEventStatement.Event.TargetObject))
					{
						yield return name;
					}
				}

				var commentStatement = statement as CodeCommentStatement;
				if (commentStatement != null)
				{
					var name = GetObjectNameStatementAppliesTo(collection, i, commentStatement);
					if (name != null)
					{
						yield return name;
					}
				}

				var variableStatement = statement as CodeVariableDeclarationStatement;
				if (variableStatement != null)
				{
					yield return variableStatement.Name;
				}
			}

			string GetObjectNameStatementAppliesTo(CodeStatementCollection collection, int i, CodeCommentStatement statement)
			{
				var fieldName = statement.Comment.Text.Trim();
				if (string.IsNullOrEmpty(fieldName))
				{
					var commentBefore = i >= 1 ? collection[i - 1] as CodeCommentStatement : null;
					var commentAfter = i + 1 < collection.Count ? collection[i + 1] as CodeCommentStatement : null;
					if (commentBefore != null)
					{
						fieldName = commentBefore.Comment.Text.Trim();
					}
					if (string.IsNullOrEmpty(fieldName) && commentAfter != null)
					{
						fieldName = commentAfter.Comment.Text.Trim();
					}
				}
				return string.IsNullOrEmpty(fieldName) ? null : fieldName;
			}

			IEnumerable<string> GetObjectNamesStatementAppliesTo(CodeStatementCollection collection, int i, CodeAssignStatement statement)
			{
				var assignStatement = statement;
				if (assignStatement != null)
				{
					foreach (var name in GetObjectNamesExpressionAppliesTo(collection, i, assignStatement.Left))
					{
						yield return name;
					}
					foreach (var name in GetObjectNamesExpressionAppliesTo(collection, i, assignStatement.Right))
					{
						yield return name;
					}
				}
			}

			IEnumerable<string> GetObjectNamesExpressionAppliesTo(CodeStatementCollection collection, int i, CodeExpression expr)
			{
				var field = expr as CodeFieldReferenceExpression;
				if (field != null)
				{
					yield return field.FieldName;
				}

				var variable = expr as CodeVariableReferenceExpression;
				if (variable != null)
				{
					yield return variable.VariableName;
				}

				var property = expr as CodePropertyReferenceExpression;
				if (property != null)
				{
					foreach (var name in GetObjectNamesExpressionAppliesTo(collection, i, property.TargetObject))
					{
						yield return name;
					}
				}

				var cast = expr as CodeCastExpression;
				if (cast != null)
				{
					foreach (var name in GetObjectNamesExpressionAppliesTo(collection, i, cast.Expression))
					{
						yield return name;
					}
				}

				var methodInvoke = expr as CodeMethodInvokeExpression;
				if (methodInvoke != null)
				{
					foreach (var name in GetObjectNamesExpressionAppliesTo(collection, i, methodInvoke))
					{
						yield return name;
					}
				}
			}

			IEnumerable<string> GetObjectNamesExpressionAppliesTo(CodeStatementCollection collection, int i, CodeMethodInvokeExpression expr)
			{
				foreach (var name in GetObjectNamesExpressionAppliesTo(collection, i, expr.Method.TargetObject))
				{
					yield return name;
				}
				foreach (CodeExpression parameter in expr.Parameters)
				{
					foreach (var name in GetObjectNamesExpressionAppliesTo(collection, i, parameter))
					{
						yield return name;
					}
				}
			}

			#endregion

			#region Services

			IReferenceService ReferenceService
			{
				get { return (IReferenceService)serviceProvider.GetService(typeof(IReferenceService)); }
			}

			IEventBindingService EventBindingService
			{
				get { return (IEventBindingService)serviceProvider.GetService(typeof(IEventBindingService)); }
			}

			object CodeDomAdapter
			{
				get { return EventBindingService.GetType().InvokeMember("CodeDomAdapter", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, EventBindingService, null); }
			}

			#endregion

			#region GetTabPageReferenceNames / AllCodeMethods

			IEnumerable<string> GetTabPageReferenceNames()
			{
				var tabNamesSoFar = new List<string>();
				for (var i = 0; i < InitializeComponentMethod.Statements.Count; i++)
				{
					var statement = InitializeComponentMethod.Statements[i];
					foreach (var name in GetObjectNamesStatementAppliesTo(InitializeComponentMethod.Statements, 0, statement))
					{
						var tabPage = GetComponentFromName(InitializeComponentMethod.Statements, i, statement, name);
						if (tabPage != null && TabPageType.IsAssignableFrom(tabPage.GetType()))
						{
							if (!tabNamesSoFar.Contains(name))
							{
								tabNamesSoFar.Add(name);
								yield return name;
							}
						}
						break;
					}
				}
			}

			IEnumerable<CodeMemberMethod> AllCodeMethods
			{
				get
				{
					yield return InitializeComponentMethod;
					foreach (var tabPageMethod in GetTabPageCodeMethods(typeDeclaration))
					{
						yield return tabPageMethod;
					}
				}
			}

			#endregion

			#region Implementation

			readonly CodeTypeDeclaration typeDeclaration;
			readonly IServiceProvider serviceProvider;
			readonly Type TabPageType;

			bool IsControlAddOrSetChildIndexMethodInvoke(CodeStatement statement)
			{
				var exprStatement = statement as CodeExpressionStatement;
				var methodInvoke = exprStatement == null ? null : exprStatement.Expression as CodeMethodInvokeExpression;
				var controlsCollectionExpr = methodInvoke == null ? null : methodInvoke.Method.TargetObject as CodePropertyReferenceExpression;
				return
					controlsCollectionExpr != null &&
					(methodInvoke.Method.MethodName == "Add" || methodInvoke.Method.MethodName == "SetChildIndex") &&
					controlsCollectionExpr.PropertyName == "Controls";
			}

			bool IsControlSuspendResumeOrPerformLayout(CodeStatement statement)
			{
				var exprStatement = statement as CodeExpressionStatement;
				var invokeMethod = exprStatement == null ? null : exprStatement.Expression as CodeMethodInvokeExpression;
				return invokeMethod != null && (invokeMethod.Method.MethodName == "SuspendLayout" || invokeMethod.Method.MethodName == "ResumeLayout" || invokeMethod.Method.MethodName == "PerformLayout");
			}

			#endregion
		}

		#region PreventMembersBeingDeletedIncorrectlyByVS

		static void PreventMembersBeingDeletedIncorrectlyByVS(IServiceProvider serviceProvider, CodeTypeDeclaration typeDeclaration)
		{
			var cls = (EnvDTE.CodeClass)typeDeclaration.UserData[typeof(EnvDTE.CodeElement)];
			var cls2 = cls as EnvDTE80.CodeClass2;
			if (cls != null && !(cls is PreventMembersBeingDeletedIncorrectlyByVSCodeClass))
			{
				if (cls2 != null)
				{
					typeDeclaration.UserData[typeof(EnvDTE.CodeElement)] = new PreventMembersBeingDeletedIncorrectlyByVSCodeClass2(typeDeclaration, cls2);
				}
				else
				{
					typeDeclaration.UserData[typeof(EnvDTE.CodeElement)] = new PreventMembersBeingDeletedIncorrectlyByVSCodeClass(typeDeclaration, cls);
				}
			}
		}

		class PreventMembersBeingDeletedIncorrectlyByVSCodeClass : EnvDTE.CodeClass
		{
			public PreventMembersBeingDeletedIncorrectlyByVSCodeClass(CodeTypeDeclaration typeDeclaration, EnvDTE.CodeClass inner)
			{
				this.typeDeclaration = typeDeclaration;
				this.inner = inner;
			}

			void EnvDTE.CodeClass.RemoveMember(object element)
			{
				RemoveMember(element);
			}

			protected void RemoveMember(object element)
			{
				var codeElement = element as EnvDTE.CodeElement;
				if (codeElement != null)
				{
					if (!TypeDeclarationContainsMember(codeElement.Name) &&
						(codeElement.Name.EndsWith(TabPageMethodNamePostfix) || ((IList<string>)UsedFieldsLocator.LocateUsedFields(typeDeclaration)).Contains(codeElement.Name)))
					{
						inner.RemoveMember(element);
					}
				}
			}

			bool TypeDeclarationContainsMember(string memberName)
			{
				foreach (CodeTypeMember codeMember in typeDeclaration.Members)
				{
					if (codeMember.Name == memberName)
					{
						return true;
					}
				}
				return false;
			}

			#region Proxying EnvDTE.CodeClass calls

			EnvDTE.vsCMAccess EnvDTE.CodeClass.Access
			{
				get { return inner.Access; }
				set { inner.Access = value; }
			}

			EnvDTE.CodeAttribute EnvDTE.CodeClass.AddAttribute(string name, string value, object position)
			{
				return inner.AddAttribute(name, value, position);
			}

			EnvDTE.CodeElement EnvDTE.CodeClass.AddBase(object @base, object position)
			{
				return inner.AddBase(@base, position);
			}

			EnvDTE.CodeClass EnvDTE.CodeClass.AddClass(string name, object position, object bases, object implementedInterfaces, EnvDTE.vsCMAccess access)
			{
				return inner.AddClass(name, position, bases, implementedInterfaces, access);
			}

			EnvDTE.CodeDelegate EnvDTE.CodeClass.AddDelegate(string name, object type, object position, EnvDTE.vsCMAccess access)
			{
				return inner.AddDelegate(name, type, position, access);
			}

			EnvDTE.CodeEnum EnvDTE.CodeClass.AddEnum(string name, object position, object bases, EnvDTE.vsCMAccess access)
			{
				return inner.AddEnum(name, position, bases, access);
			}

			EnvDTE.CodeFunction EnvDTE.CodeClass.AddFunction(string name, EnvDTE.vsCMFunction kind, object type, object position, EnvDTE.vsCMAccess access, object location)
			{
				return inner.AddFunction(name, kind, type, position, access, location);
			}

			EnvDTE.CodeInterface EnvDTE.CodeClass.AddImplementedInterface(object @base, object position)
			{
				return inner.AddImplementedInterface(@base, position);
			}

			EnvDTE.CodeProperty EnvDTE.CodeClass.AddProperty(string getterName, string putterName, object type, object position, EnvDTE.vsCMAccess access, object location)
			{
				return inner.AddProperty(getterName, putterName, type, position, access, location);
			}

			EnvDTE.CodeStruct EnvDTE.CodeClass.AddStruct(string name, object position, object bases, object implementedInterfaces, EnvDTE.vsCMAccess access)
			{
				return inner.AddStruct(name, position, bases, implementedInterfaces, access);
			}

			EnvDTE.CodeVariable EnvDTE.CodeClass.AddVariable(string name, object type, object position, EnvDTE.vsCMAccess access, object location)
			{
				return inner.AddVariable(name, type, position, access, location);
			}

			EnvDTE.CodeElements EnvDTE.CodeClass.Attributes
			{
				get { return inner.Attributes; }
			}

			EnvDTE.CodeElements EnvDTE.CodeClass.Bases
			{
				get { return inner.Bases; }
			}

			EnvDTE.CodeElements EnvDTE.CodeClass.Children
			{
				get { return inner.Children; }
			}

			EnvDTE.CodeElements EnvDTE.CodeClass.Collection
			{
				get { return inner.Collection; }
			}

			string EnvDTE.CodeClass.Comment
			{
				get { return inner.Comment; }
				set { inner.Comment = value; }
			}

			EnvDTE.DTE EnvDTE.CodeClass.DTE
			{
				get { return inner.DTE; }
			}

			EnvDTE.CodeElements EnvDTE.CodeClass.DerivedTypes
			{
				get { return inner.DerivedTypes; }
			}

			string EnvDTE.CodeClass.DocComment
			{
				get { return inner.DocComment; }
				set { inner.DocComment = value; }
			}

			EnvDTE.TextPoint EnvDTE.CodeClass.EndPoint
			{
				get { return inner.EndPoint; }
			}

			string EnvDTE.CodeClass.ExtenderCATID
			{
				get { return inner.ExtenderCATID; }
			}

			object EnvDTE.CodeClass.ExtenderNames
			{
				get { return inner.ExtenderNames; }
			}

			string EnvDTE.CodeClass.FullName
			{
				get { return inner.FullName; }
			}

			EnvDTE.TextPoint EnvDTE.CodeClass.GetEndPoint(EnvDTE.vsCMPart part)
			{
				return inner.GetEndPoint(part);
			}

			EnvDTE.TextPoint EnvDTE.CodeClass.GetStartPoint(EnvDTE.vsCMPart part)
			{
				return inner.GetStartPoint(part);
			}

			EnvDTE.CodeElements EnvDTE.CodeClass.ImplementedInterfaces
			{
				get { return inner.ImplementedInterfaces; }
			}

			EnvDTE.vsCMInfoLocation EnvDTE.CodeClass.InfoLocation
			{
				get { return inner.InfoLocation; }
			}

			bool EnvDTE.CodeClass.IsAbstract
			{
				get { return inner.IsAbstract; }
				set { inner.IsAbstract = value; }
			}

			bool EnvDTE.CodeClass.IsCodeType
			{
				get { return inner.IsCodeType; }
			}

			EnvDTE.vsCMElement EnvDTE.CodeClass.Kind
			{
				get { return inner.Kind; }
			}

			string EnvDTE.CodeClass.Language
			{
				get { return inner.Language; }
			}

			EnvDTE.CodeElements EnvDTE.CodeClass.Members
			{
				get { return inner.Members; }
			}

			string EnvDTE.CodeClass.Name
			{
				get { return inner.Name; }
				set { inner.Name = value; }
			}

			EnvDTE.CodeNamespace EnvDTE.CodeClass.Namespace
			{
				get { return inner.Namespace; }
			}

			object EnvDTE.CodeClass.Parent
			{
				get { return inner.Parent; }
			}

			EnvDTE.ProjectItem EnvDTE.CodeClass.ProjectItem
			{
				get { return inner.ProjectItem; }
			}

			void EnvDTE.CodeClass.RemoveBase(object element)
			{
				inner.RemoveBase(element);
			}

			void EnvDTE.CodeClass.RemoveInterface(object element)
			{
				inner.RemoveInterface(element);
			}

			EnvDTE.TextPoint EnvDTE.CodeClass.StartPoint
			{
				get { return inner.StartPoint; }
			}

			object EnvDTE.CodeClass.get_Extender(string extenderName)
			{
				return inner.get_Extender(extenderName);
			}

			bool EnvDTE.CodeClass.get_IsDerivedFrom(string fullName)
			{
				return inner.get_IsDerivedFrom(fullName);
			}

			#endregion

			readonly CodeTypeDeclaration typeDeclaration;
			readonly EnvDTE.CodeClass inner;
		}

		class PreventMembersBeingDeletedIncorrectlyByVSCodeClass2 : PreventMembersBeingDeletedIncorrectlyByVSCodeClass, EnvDTE80.CodeClass2
		{
			public PreventMembersBeingDeletedIncorrectlyByVSCodeClass2(CodeTypeDeclaration typeDeclaration, EnvDTE80.CodeClass2 inner)
				: base(typeDeclaration, inner)
			{
				this.inner = inner;
			}

			void EnvDTE80.CodeClass2.RemoveMember(object element)
			{
				RemoveMember(element);
			}

			#region Proxying EnvDTE80.CodeClass2 calls

			EnvDTE.vsCMAccess EnvDTE80.CodeClass2.Access
			{
				get { return inner.Access; }
				set { inner.Access = value; }
			}

			EnvDTE.CodeAttribute EnvDTE80.CodeClass2.AddAttribute(string name, string value, object position)
			{
				return inner.AddAttribute(name, value, position);
			}

			EnvDTE.CodeElement EnvDTE80.CodeClass2.AddBase(object @base, object position)
			{
				return inner.AddBase(@base, position);
			}

			EnvDTE.CodeClass EnvDTE80.CodeClass2.AddClass(string name, object position, object bases, object implementedInterfaces, EnvDTE.vsCMAccess access)
			{
				return inner.AddClass(name, position, bases, implementedInterfaces, access);
			}

			EnvDTE.CodeDelegate EnvDTE80.CodeClass2.AddDelegate(string name, object type, object position, EnvDTE.vsCMAccess access)
			{
				return inner.AddDelegate(name, type, position, access);
			}

			EnvDTE.CodeEnum EnvDTE80.CodeClass2.AddEnum(string name, object position, object bases, EnvDTE.vsCMAccess access)
			{
				return inner.AddEnum(name, position, bases, access);
			}

			EnvDTE80.CodeEvent EnvDTE80.CodeClass2.AddEvent(string name, string fullDelegateName, bool createPropertyStyleEvent, object location, EnvDTE.vsCMAccess access)
			{
				return inner.AddEvent(name, fullDelegateName, createPropertyStyleEvent, location, access);
			}

			EnvDTE.CodeFunction EnvDTE80.CodeClass2.AddFunction(string name, EnvDTE.vsCMFunction kind, object type, object position, EnvDTE.vsCMAccess access, object location)
			{
				return inner.AddFunction(name, kind, type, position, access, location);
			}

			EnvDTE.CodeInterface EnvDTE80.CodeClass2.AddImplementedInterface(object @base, object position)
			{
				return inner.AddImplementedInterface(@base, position);
			}

			EnvDTE.CodeProperty EnvDTE80.CodeClass2.AddProperty(string getterName, string putterName, object type, object position, EnvDTE.vsCMAccess access, object location)
			{
				return inner.AddProperty(getterName, putterName, type, position, access, location);
			}

			EnvDTE.CodeStruct EnvDTE80.CodeClass2.AddStruct(string name, object position, object bases, object implementedInterfaces, EnvDTE.vsCMAccess access)
			{
				return inner.AddStruct(name, position, bases, implementedInterfaces, access);
			}

			EnvDTE.CodeVariable EnvDTE80.CodeClass2.AddVariable(string name, object type, object position, EnvDTE.vsCMAccess access, object location)
			{
				return inner.AddVariable(name, type, position, access, location);
			}

			EnvDTE.CodeElements EnvDTE80.CodeClass2.Attributes
			{
				get { return inner.Attributes; }
			}

			EnvDTE.CodeElements EnvDTE80.CodeClass2.Bases
			{
				get { return inner.Bases; }
			}

			EnvDTE.CodeElements EnvDTE80.CodeClass2.Children
			{
				get { return inner.Children; }
			}

			EnvDTE80.vsCMClassKind EnvDTE80.CodeClass2.ClassKind
			{
				get { return inner.ClassKind; }
				set { inner.ClassKind = value; }
			}

			EnvDTE.CodeElements EnvDTE80.CodeClass2.Collection
			{
				get { return inner.Collection; }
			}

			string EnvDTE80.CodeClass2.Comment
			{
				get { return inner.Comment; }
				set { inner.Comment = value; }
			}

			EnvDTE.DTE EnvDTE80.CodeClass2.DTE
			{
				get { return inner.DTE; }
			}

			EnvDTE80.vsCMDataTypeKind EnvDTE80.CodeClass2.DataTypeKind
			{
				get { return inner.DataTypeKind; }
				set { inner.DataTypeKind = value; }
			}

			EnvDTE.CodeElements EnvDTE80.CodeClass2.DerivedTypes
			{
				get { return inner.DerivedTypes; }
			}

			string EnvDTE80.CodeClass2.DocComment
			{
				get { return inner.DocComment; }
				set { inner.DocComment = value; }
			}

			EnvDTE.TextPoint EnvDTE80.CodeClass2.EndPoint
			{
				get { return inner.EndPoint; }
			}

			string EnvDTE80.CodeClass2.ExtenderCATID
			{
				get { return inner.ExtenderCATID; }
			}

			object EnvDTE80.CodeClass2.ExtenderNames
			{
				get { return inner.ExtenderNames; }
			}

			string EnvDTE80.CodeClass2.FullName
			{
				get { return inner.FullName; }
			}

			EnvDTE.TextPoint EnvDTE80.CodeClass2.GetEndPoint(EnvDTE.vsCMPart part)
			{
				return inner.GetEndPoint(part);
			}

			EnvDTE.TextPoint EnvDTE80.CodeClass2.GetStartPoint(EnvDTE.vsCMPart part)
			{
				return inner.GetStartPoint(part);
			}

			EnvDTE.CodeElements EnvDTE80.CodeClass2.ImplementedInterfaces
			{
				get { return inner.ImplementedInterfaces; }
			}

			EnvDTE.vsCMInfoLocation EnvDTE80.CodeClass2.InfoLocation
			{
				get { return inner.InfoLocation; }
			}

			EnvDTE80.vsCMInheritanceKind EnvDTE80.CodeClass2.InheritanceKind
			{
				get { return inner.InheritanceKind; }
				set { inner.InheritanceKind = value; }
			}

			bool EnvDTE80.CodeClass2.IsAbstract
			{
				get { return inner.IsAbstract; }
				set { inner.IsAbstract = value; }
			}

			bool EnvDTE80.CodeClass2.IsCodeType
			{
				get { return inner.IsCodeType; }
			}

			bool EnvDTE80.CodeClass2.IsGeneric
			{
				get { return inner.IsGeneric; }
			}

			bool EnvDTE80.CodeClass2.IsShared
			{
				get { return inner.IsShared; }
				set { inner.IsShared = value; }
			}

			EnvDTE.vsCMElement EnvDTE80.CodeClass2.Kind
			{
				get { return inner.Kind; }
			}

			string EnvDTE80.CodeClass2.Language
			{
				get { return inner.Language; }
			}

			EnvDTE.CodeElements EnvDTE80.CodeClass2.Members
			{
				get { return inner.Members; }
			}

			string EnvDTE80.CodeClass2.Name
			{
				get { return inner.Name; }
				set { inner.Name = value; }
			}

			EnvDTE.CodeNamespace EnvDTE80.CodeClass2.Namespace
			{
				get { return inner.Namespace; }
			}

			object EnvDTE80.CodeClass2.Parent
			{
				get { return inner.Parent; }
			}

			EnvDTE.CodeElements EnvDTE80.CodeClass2.PartialClasses
			{
				get { return inner.PartialClasses; }
			}

			EnvDTE.CodeElements EnvDTE80.CodeClass2.Parts
			{
				get { return inner.Parts; }
			}

			EnvDTE.ProjectItem EnvDTE80.CodeClass2.ProjectItem
			{
				get { return inner.ProjectItem; }
			}

			void EnvDTE80.CodeClass2.RemoveBase(object element)
			{
				inner.RemoveBase(element);
			}

			void EnvDTE80.CodeClass2.RemoveInterface(object element)
			{
				inner.RemoveInterface(element);
			}

			EnvDTE.TextPoint EnvDTE80.CodeClass2.StartPoint
			{
				get { return inner.StartPoint; }
			}

			object EnvDTE80.CodeClass2.get_Extender(string extenderName)
			{
				return inner.get_Extender(extenderName);
			}

			bool EnvDTE80.CodeClass2.get_IsDerivedFrom(string fullName)
			{
				return inner.get_IsDerivedFrom(fullName);
			}

			#endregion

			readonly EnvDTE80.CodeClass2 inner;
		}

		sealed class UsedFieldsLocator : CodeDomVisitor
		{
			UsedFieldsLocator()
			{
			}

			public static string[] LocateUsedFields(CodeTypeDeclaration codeType)
			{
				var locator = new UsedFieldsLocator();
				locator.VisitMember(codeType);
				return locator.fieldNames.ToArray();
			}

			protected override void VisitMember(CodeTypeMember member)
			{
				if (member is CodeTypeDeclaration ||
					(member is CodeMemberMethod &&
					 (member.Name == "InitializeComponent" || member.Name.EndsWith(TabPageMethodNamePostfix))))
				{
					base.VisitMember(member);
				}
			}

			protected override void VisitExpression(CodeExpression expr)
			{
				base.VisitExpression(expr);
				var fieldReference = expr as CodeFieldReferenceExpression;
				if (fieldReference != null)
				{
					if (fieldReference.TargetObject is CodeThisReferenceExpression)
					{
						fieldNames.Add(fieldReference.FieldName);
					}
				}
			}

			readonly List<string> fieldNames = new List<string>();
		}

		#endregion

		#endregion

		#region Deserialize

		public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
		{
			var statements = new CodeStatementCollection((CodeStatementCollection)codeObject);
			var typeDeclaration = (CodeTypeDeclaration)manager.GetService(typeof(CodeTypeDeclaration));

			CodeTypeMemberCollectionForFieldVisibilityFix.Install(typeDeclaration);
			ExpandTabPageMethodStatements(typeDeclaration, statements);

			var result = base.Deserialize(manager, statements);
			return result;
		}

		void ExpandTabPageMethodStatements(CodeTypeDeclaration typeDeclaration, CodeStatementCollection collection)
		{
			for (var i = 0; i < collection.Count; i++)
			{
				var statement = collection[i];
				ExpandTabPageMethodStatement(typeDeclaration, statement, collection, i);
			}
		}

		void ExpandTabPageMethodStatement(CodeTypeDeclaration typeDeclaration, CodeStatement statement, CodeStatementCollection collection, int i)
		{
			var exprStatement = statement as CodeExpressionStatement;
			var methodInvoke = exprStatement == null ? null : exprStatement.Expression as CodeMethodInvokeExpression;
			if (methodInvoke != null && methodInvoke.Method.MethodName == ZTabPage.RunWhenBindingOrFirstShownMethod)
			{
				var delegateCreate = methodInvoke.Parameters.Count == 0 ? null : methodInvoke.Parameters[0] as CodeObjectCreateExpression;
				var delegateMethod = (delegateCreate == null || delegateCreate.Parameters.Count == 0) ? null : delegateCreate.Parameters[0] as CodeMethodReferenceExpression;
				var tabPageMethod = delegateMethod == null ? null : FindCodeMethod(typeDeclaration, delegateMethod.MethodName);
				if (tabPageMethod != null)
				{
					collection.Remove(statement);
					foreach (CodeStatement methodStatement in tabPageMethod.Statements)
					{
						collection.Insert(i++, methodStatement);
					}
				}
			}
			ExpandTabPageMethodStatement_Legacy(typeDeclaration, statement, collection, i);
		}

		void ExpandTabPageMethodStatement_Legacy(CodeTypeDeclaration typeDeclaration, CodeStatement statement, CodeStatementCollection collection, int i)
		{
			var attachEvent = statement as CodeAttachEventStatement;
			if (attachEvent != null)
			{
				if (attachEvent.Event.EventName == ZTabPage.BindingOrFirstShownPropertyName)
				{
					var delegateCreate = attachEvent.Listener as CodeObjectCreateExpression;
					var delegateMethod = (delegateCreate == null || delegateCreate.Parameters.Count == 0) ? null : delegateCreate.Parameters[0] as CodeMethodReferenceExpression;
					var tabPageMethod = delegateMethod == null ? null : FindCodeMethod(typeDeclaration, delegateMethod.MethodName);
					if (tabPageMethod != null)
					{
						collection.Remove(statement);
						foreach (CodeStatement methodStatement in tabPageMethod.Statements)
						{
							collection.Insert(i++, methodStatement);
						}
					}
				}
			}
		}

		CodeMemberMethod FindCodeMethod(CodeTypeDeclaration typeDeclaration, string methodName)
		{
			foreach (CodeTypeMember member in typeDeclaration.Members)
			{
				var method = member as CodeMemberMethod;
				if (method != null && method.Name == methodName)
				{
					return method;
				}
			}
			return null;
		}

		bool HasTabControls(object component)
		{
			var control = component as Control;
			if (control != null)
			{
				foreach (Control childControl in control.Controls)
				{
					var tabControl = childControl as TabControl;
					if (tabControl != null)
					{
						return true;
					}
					else if (childControl is GroupBox || childControl is Panel)
					{
						return HasTabControls(childControl);
					}
				}
			}
			return false;
		}

		#endregion

		#region QueryShouldSerializeTabPageMethods / EnsureWarnOfBuildDuringDesign

		bool QueryShouldSerializeTabPageMethods(IServiceProvider serviceProvider, object component)
		{
			if (HasTabControls(component) &&
				!IsShouldSerializeTabPageMethodsSpecified(serviceProvider) &&
				!HasUndoEngineOnComponentChangingInCallStack()) // if UndoEngine is processing the OnComponentChanging event, _MessageBox.Show will cause a BehaviourAdorner will cause a TransactionClosed event to raise causing an enumeration failed exception over _unitStack
			{
				var response = DialogResult.Yes;
				var typeDeclaration = (CodeTypeDeclaration)serviceProvider.GetService(typeof(CodeTypeDeclaration));
				if (!HasInitializeTabMethods(typeDeclaration))
				{
					var message = @"
Do you wish to serialize the tab pages to methods?

If you answer yes, " + nameof(ControlCodeDomSerializerWithDelayedTabCreate) + @" will serialize
ZTabPage controls to methods to make forms load faster at runtime.

You should chooose yes when there may be performance improvements, for example:
- You are designing a form with at least 4 tab pages.
- You are designing a user control with at least 2 tab pages, and the user control is immediately
  visible on at least 1 form when shown.

If the form or user control is subclassed, each subclass will need to be converted as well.
";
					var uiservice = (IUIService)serviceProvider.GetService(typeof(IUIService));
					if (uiservice != null)
					{
						response = uiservice.ShowMessage(message, nameof(ControlCodeDomSerializerWithDelayedTabCreate), MessageBoxButtons.YesNo);
					}
				}
				SetShouldSerializeTabPageMethodSerialization(serviceProvider, response == DialogResult.Yes);
			}
			return GetShouldSerializeTabPageMethods(serviceProvider);
		}

		static bool HasUndoEngineOnComponentChangingInCallStack()
		{
			var stack = new StackTrace();
			foreach (var frame in stack.GetFrames())
			{
				if (frame.GetMethod().DeclaringType == typeof(UndoEngine) &&
					frame.GetMethod().Name == "OnComponentChanging")
				{
					return true;
				}
			}
			return false;
		}

		bool HasInitializeTabMethods(CodeTypeDeclaration typeDeclaration)
		{
			foreach (CodeTypeMember member in typeDeclaration.Members)
			{
				if (member.Name.Contains(TabPageMethodNamePostfix))
				{
					return true;
				}
			}
			return false;
		}

		bool IsShouldSerializeTabPageMethodsSpecified(IServiceProvider serviceProvider)
		{
			var host = (IDesignerHost)serviceProvider.GetService(typeof(IDesignerHost));
			return host != null && IsShouldSerializeTabPageMethodsSpecified(host.RootComponent);
		}

		bool IsShouldSerializeTabPageMethodsSpecified(object component)
		{
			var property = component == null ? null : TypeDescriptor.GetProperties(component)[ZUserControl.IsShouldSerializeTabPageMethodsSpecifiedPropertyName];
			return property != null && (bool)property.GetValue(component);
		}

		bool GetShouldSerializeTabPageMethods(IServiceProvider serviceProvider)
		{
			var host = (IDesignerHost)serviceProvider.GetService(typeof(IDesignerHost));
			return host != null && GetShouldSerializeTabPageMethods(host.RootComponent);
		}

		bool GetShouldSerializeTabPageMethods(object component)
		{
			var property = component == null ? null : TypeDescriptor.GetProperties(component)[ZUserControl.ShouldSerializeTabPageMethodsPropertyName];
			return property != null && (bool)property.GetValue(component);
		}

		void SetShouldSerializeTabPageMethodSerialization(IServiceProvider serviceProvider, bool value)
		{
			var host = (IDesignerHost)serviceProvider.GetService(typeof(IDesignerHost));
			SetShouldSerializeTabPageMethodSerialization(host.RootComponent, value);
		}

		void SetShouldSerializeTabPageMethodSerialization(object component, bool value)
		{
			var property = component == null ? null : TypeDescriptor.GetProperties(component)[ZUserControl.ShouldSerializeTabPageMethodsPropertyName];
			if (property != null)
			{
				property.SetValue(component, value);
			}
		}

		void EnsureWarnOfBuildDuringDesign(IServiceProvider serviceProvider)
		{
			if (buildWarner == null)
			{
				buildWarner = new BuildDuringControlDesignWarner(serviceProvider);
			}
			buildWarner.EnsureWarnOfBuildDuringDesign();
		}
		[ThreadStatic]
		static BuildDuringControlDesignWarner buildWarner;

		#endregion

		#region CodeTypeMemberCollectionForFieldVisibilityFix

				class CodeTypeMemberCollectionForFieldVisibilityFix : CodeTypeMemberCollection
				{
					protected CodeTypeMemberCollectionForFieldVisibilityFix(CodeTypeMemberCollection value)
						: base(value)
					{
					}

					public static void Install(CodeTypeDeclaration codeTypeDeclaration)
					{
						typeof(CodeTypeDeclaration).InvokeMember("members", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, codeTypeDeclaration, new object[] { new CodeTypeMemberCollectionForFieldVisibilityFix(codeTypeDeclaration.Members) });
					}

					protected override void OnSet(int index, object oldValue, object newValue)
					{
						base.OnSet(index, oldValue, newValue);
						var oldField = oldValue as CodeMemberField;
						var newField = newValue as CodeMemberField;
						if (oldField != null &&
							newField != null &&
							oldField.Name == newField.Name &&
							oldField.Type.BaseType == newField.Type.BaseType)
						{
							newField.Attributes = oldField.Attributes;
						}
					}
				}

				#endregion

		#region Implementation

				internal const string TabPageMethodNamePostfix = "_InitializeTab";

				static IEnumerable<CodeMemberMethod> GetTabPageCodeMethods(CodeTypeDeclaration typeDeclaration)
				{
					foreach (CodeTypeMember member in typeDeclaration.Members)
					{
						var method = member as CodeMemberMethod;
						if (method != null && method.Name.EndsWith(TabPageMethodNamePostfix))
						{
							yield return method;
						}
					}
				}

				#endregion
	}
}
#endif

// To functionally test, open ControlCodeDomSerializerWithDelayedTabCreate_TestForm in the designer and follow the instructions.
