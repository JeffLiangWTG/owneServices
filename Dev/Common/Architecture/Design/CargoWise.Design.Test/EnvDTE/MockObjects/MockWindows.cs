using System;
using System.ComponentModel;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockWindows : MarshalByRefObject, EnvDTE.Windows
	{
		public EnvDTE.Window Item(object index)
		{
			if (index as string == EnvDTE.Constants.vsWindowKindMainWindow)
			{
				return MainWindow;
			}
			else
			{
				throw new ArgumentException("Could not find window '" + index + "'");
			}
		}

		public readonly MockWindow MainWindow = new MockWindow();

		#region Windows Unsupported Members

		int EnvDTE.Windows.Count
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		EnvDTE.Window EnvDTE.Windows.CreateLinkedWindowFrame(EnvDTE.Window window1, EnvDTE.Window window2, EnvDTE.vsLinkedWindowType link)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.Window EnvDTE.Windows.CreateToolWindow(EnvDTE.AddIn addInInst, string progId, string caption, string guidPosition, ref object docObj)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.DTE EnvDTE.Windows.DTE
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		System.Collections.IEnumerator EnvDTE.Windows.GetEnumerator()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.DTE EnvDTE.Windows.Parent
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
