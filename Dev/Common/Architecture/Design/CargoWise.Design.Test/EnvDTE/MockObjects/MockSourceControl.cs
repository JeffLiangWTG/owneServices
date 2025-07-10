using System;
using System.Collections;
using System.ComponentModel;
using System.IO;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MockSourceControl : MarshalByRefObject, EnvDTE.SourceControl
	{
		public MockSourceControl(EnvDTE.DTE dte)
		{ this.dte = dte; }

		public string LastSingleItemCheckedOut;
		public string[] LastMultipleItemsCheckedOut;

		public bool CheckOutItem(string ItemName)
		{
			if (File.Exists(ItemName))
			{
				File.SetAttributes(ItemName, FileAttributes.Normal);
			}
			LastSingleItemCheckedOut = ItemName;
			return true;
		}

		public bool CheckOutItems(ref object[] ItemNames)
		{
			foreach (string itemName in ItemNames)
			{
				if (File.Exists(itemName))
				{
					File.SetAttributes(itemName, FileAttributes.Normal);
				}
			}
			LastMultipleItemsCheckedOut = (string[])new ArrayList(ItemNames).ToArray(typeof(string));
			return true;
		}

		bool EnvDTE.SourceControl.IsItemUnderSCC(string itemName)
		{ return false; }

		EnvDTE.DTE EnvDTE.SourceControl.DTE
		{ get { return dte; } }

		readonly EnvDTE.DTE dte;

		#region SourceControl Members

		void EnvDTE.SourceControl.ExcludeItem(string projectFile, string itemName)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void EnvDTE.SourceControl.ExcludeItems(string projectFile, ref object[] itemNames)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool EnvDTE.SourceControl.IsItemCheckedOut(string itemName)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		EnvDTE.DTE EnvDTE.SourceControl.Parent
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		#endregion
	}
}
