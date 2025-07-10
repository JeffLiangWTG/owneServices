using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockEvents : MarshalByRefObject, EnvDTE.Events, EnvDTE80.Events2
	{
		#region Events Members

		public MockSolutionEvents SolutionEvents
		{ get { return solutionEvents ?? (solutionEvents = new MockSolutionEvents()); } }
		MockSolutionEvents solutionEvents;

		EnvDTE.SolutionEvents EnvDTE.Events.SolutionEvents
		{ get { return SolutionEvents; } }

		public MockProjectItemsEvents SolutionItemsEvents
		{ get { return solutionItemsEvents ?? (solutionItemsEvents = new MockProjectItemsEvents()); } }
		MockProjectItemsEvents solutionItemsEvents;

		EnvDTE.ProjectItemsEvents EnvDTE.Events.SolutionItemsEvents
		{ get { return SolutionItemsEvents; } }

		public EnvDTE.DocumentEvents get_DocumentEvents(EnvDTE.Document document)
		{
			if (document != null)
			{
				throw new NotSupportedException("Document must be null (for this mock implementation)");
			}
			return documentEvents ?? (documentEvents = new MockDocumentEvents());
		}
		MockDocumentEvents documentEvents;

		EnvDTE.DocumentEvents EnvDTE.Events.get_DocumentEvents(EnvDTE.Document document)
		{ return get_DocumentEvents(document); }

		EnvDTE.BuildEvents EnvDTE.Events.BuildEvents
		{ get { return new MockBuildEvents(); } }

		#region Unsupported Members

		EnvDTE.DTEEvents EnvDTE.Events.DTEEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.DebuggerEvents EnvDTE.Events.DebuggerEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.FindEvents EnvDTE.Events.FindEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE.Events.GetObject(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.ProjectItemsEvents EnvDTE.Events.MiscFilesEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.SelectionEvents EnvDTE.Events.SelectionEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE.Events.get_CommandBarEvents(object commandBarControl)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.CommandEvents EnvDTE.Events.get_CommandEvents(string guid, int iD)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.OutputWindowEvents EnvDTE.Events.get_OutputWindowEvents(string pane)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.TaskListEvents EnvDTE.Events.get_TaskListEvents(string filter)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.TextEditorEvents EnvDTE.Events.get_TextEditorEvents(EnvDTE.TextDocument textDocumentFilter)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.WindowEvents EnvDTE.Events.get_WindowEvents(EnvDTE.Window windowFilter)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion

		#endregion

		#region Events2 Members

		public MockProjectItemsEvents ProjectItemsEvents
		{ get { return projectItemsEvents ?? (projectItemsEvents = new MockProjectItemsEvents()); } }
		MockProjectItemsEvents projectItemsEvents;

		EnvDTE.ProjectItemsEvents EnvDTE80.Events2.ProjectItemsEvents
		{ get { return ProjectItemsEvents; } }

		#region Unsupported Events

		EnvDTE.BuildEvents EnvDTE80.Events2.BuildEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.DTEEvents EnvDTE80.Events2.DTEEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.DebuggerEvents EnvDTE80.Events2.DebuggerEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE80.DebuggerExpressionEvaluationEvents EnvDTE80.Events2.DebuggerExpressionEvaluationEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE80.DebuggerProcessEvents EnvDTE80.Events2.DebuggerProcessEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.FindEvents EnvDTE80.Events2.FindEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE80.Events2.GetObject(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.ProjectItemsEvents EnvDTE80.Events2.MiscFilesEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.ProjectsEvents EnvDTE80.Events2.ProjectsEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE80.PublishEvents EnvDTE80.Events2.PublishEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.SelectionEvents EnvDTE80.Events2.SelectionEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.SolutionEvents EnvDTE80.Events2.SolutionEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.ProjectItemsEvents EnvDTE80.Events2.SolutionItemsEvents
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE80.CodeModelEvents EnvDTE80.Events2.get_CodeModelEvents(EnvDTE.CodeElement reserved)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		object EnvDTE80.Events2.get_CommandBarEvents(object commandBarControl)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.CommandEvents EnvDTE80.Events2.get_CommandEvents(string guid, int iD)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.DocumentEvents EnvDTE80.Events2.get_DocumentEvents(EnvDTE.Document document)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.OutputWindowEvents EnvDTE80.Events2.get_OutputWindowEvents(string pane)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.TaskListEvents EnvDTE80.Events2.get_TaskListEvents(string filter)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE80.TextDocumentKeyPressEvents EnvDTE80.Events2.get_TextDocumentKeyPressEvents(EnvDTE.TextDocument textDocument)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.TextEditorEvents EnvDTE80.Events2.get_TextEditorEvents(EnvDTE.TextDocument textDocumentFilter)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.WindowEvents EnvDTE80.Events2.get_WindowEvents(EnvDTE.Window windowFilter)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE80.WindowVisibilityEvents EnvDTE80.Events2.get_WindowVisibilityEvents(EnvDTE.Window windowFilter)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion

		#endregion
	}
}
