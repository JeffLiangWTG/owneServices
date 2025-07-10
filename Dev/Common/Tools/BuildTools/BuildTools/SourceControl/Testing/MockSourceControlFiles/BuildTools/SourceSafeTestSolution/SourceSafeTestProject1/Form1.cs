using System;
using System.Windows.Forms;

#region TestCase
#if DEBUG
using NUnit.Framework;
#endif
#endregion

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace SourceSafeTestProject1
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}
	}

	#region TestCase for TestFile2
#if DEBUG
	public class Form1Test : TestCase
	{
		[ExpectNoExceptions]
		public void TestEmpty()
		{
		}
	}

#endif
	#endregion
}
