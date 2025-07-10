using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockWindow : MarshalByRefObject, EnvDTE.Window
	{
		#region Window Unsupported Members

		void EnvDTE.Window.Activate()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.Window.Attach(int lWindowHandle)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE.Window.AutoHides
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

		string EnvDTE.Window.Caption
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

		void EnvDTE.Window.Close(EnvDTE.vsSaveChanges saveChanges)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.Windows EnvDTE.Window.Collection
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.ContextAttributes EnvDTE.Window.ContextAttributes
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.DTE EnvDTE.Window.DTE
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE.Window.Detach()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.Document EnvDTE.Window.Document
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		int EnvDTE.Window.HWnd
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		int EnvDTE.Window.Height
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

		bool EnvDTE.Window.IsFloating
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

		string EnvDTE.Window.Kind
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		int EnvDTE.Window.Left
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

		bool EnvDTE.Window.Linkable
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

		EnvDTE.Window EnvDTE.Window.LinkedWindowFrame
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.LinkedWindows EnvDTE.Window.LinkedWindows
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE.Window.Object
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string EnvDTE.Window.ObjectKind
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.Project EnvDTE.Window.Project
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.ProjectItem EnvDTE.Window.ProjectItem
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object EnvDTE.Window.Selection
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void EnvDTE.Window.SetFocus()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.Window.SetKind(EnvDTE.vsWindowType eKind)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.Window.SetSelectionContainer(ref object[] objects)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.Window.SetTabPicture(object picture)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		int EnvDTE.Window.Top
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

		EnvDTE.vsWindowType EnvDTE.Window.Type
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		bool EnvDTE.Window.Visible
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

		int EnvDTE.Window.Width
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

		EnvDTE.vsWindowState EnvDTE.Window.WindowState
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

		object EnvDTE.Window.get_DocumentData(string bstrWhichData)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
