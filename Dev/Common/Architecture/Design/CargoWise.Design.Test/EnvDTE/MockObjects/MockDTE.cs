using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockDTE : MarshalByRefObject, EnvDTE.DTE, IDisposable
	{
		public readonly MockWindows Windows = new MockWindows();

		EnvDTE.Windows EnvDTE._DTE.Windows
		{ get { return Windows; } }

		public MockSolution Solution
		{ get { return solution ?? (solution = new MockSolution(this)); } }
		MockSolution solution;

		EnvDTE.Solution EnvDTE._DTE.Solution
		{ get { return Solution; } }

		EnvDTE.Events EnvDTE._DTE.Events
		{ get { return Events; } }

		public MockEvents Events
		{ get { return events ?? (events = new MockEvents()); } }
		MockEvents events;

		public MockSourceControl SourceControl
		{ get { return sourceControl ?? (sourceControl = new MockSourceControl(this)); } }
		MockSourceControl sourceControl;

		EnvDTE.SourceControl EnvDTE._DTE.SourceControl
		{ get { return SourceControl; } }

		public MockDocuments Documents
		{ get { return documents ?? (documents = new MockDocuments()); } }
		MockDocuments documents;

		EnvDTE.Documents EnvDTE._DTE.Documents
		{ get { return Documents; } }

		#region _DTE Unsupported Members

		EnvDTE.Document EnvDTE._DTE.ActiveDocument
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE._DTE.ActiveSolutionProjects
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.Window EnvDTE._DTE.ActiveWindow
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.AddIns EnvDTE._DTE.AddIns
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.DTE EnvDTE._DTE.Application
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE._DTE.CommandBars
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE._DTE.CommandLineArguments
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.Commands EnvDTE._DTE.Commands
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.ContextAttributes EnvDTE._DTE.ContextAttributes
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.DTE EnvDTE._DTE.DTE
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.Debugger EnvDTE._DTE.Debugger
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.vsDisplay EnvDTE._DTE.DisplayMode
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		string EnvDTE._DTE.Edition
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE._DTE.ExecuteCommand(string commandName, string commandArgs)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		string EnvDTE._DTE.FileName
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.Find EnvDTE._DTE.Find
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE._DTE.FullName
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE._DTE.GetObject(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.Globals EnvDTE._DTE.Globals
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.ItemOperations EnvDTE._DTE.ItemOperations
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.wizardResult EnvDTE._DTE.LaunchWizard(string vSZFile, ref object[] contextParams)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		int EnvDTE._DTE.LocaleID
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.Macros EnvDTE._DTE.Macros
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.DTE EnvDTE._DTE.MacrosIDE
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.Window EnvDTE._DTE.MainWindow
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.vsIDEMode EnvDTE._DTE.Mode
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE._DTE.Name
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.ObjectExtenders EnvDTE._DTE.ObjectExtenders
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.Window EnvDTE._DTE.OpenFile(string viewKind, string fileName)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE._DTE.Quit()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		string EnvDTE._DTE.RegistryRoot
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE._DTE.SatelliteDllPath(string path, string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.SelectedItems EnvDTE._DTE.SelectedItems
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.StatusBar EnvDTE._DTE.StatusBar
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		bool EnvDTE._DTE.SuppressUI
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		EnvDTE.UndoContext EnvDTE._DTE.UndoContext
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		bool EnvDTE._DTE.UserControl
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		string EnvDTE._DTE.Version
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.WindowConfigurations EnvDTE._DTE.WindowConfigurations
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		bool EnvDTE._DTE.get_IsOpenFile(string viewKind, string fileName)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.Properties EnvDTE._DTE.get_Properties(string category, string page)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{ Events.SolutionEvents.FireAfterClosing(); }

		#endregion
	}
}
