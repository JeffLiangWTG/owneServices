using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataTransfer.Native.Adapter.Utils
{
	public class PromptDialogFileLocator : ISaveFileLocator
	{
		#region IFileLocator Members
		public Stream GetFileStream()
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.CheckPathExists = true;
				dialog.Filter = Filter;
				dialog.DefaultExt = DefaultExt;
				dialog.AddExtension = AddExtension;
				dialog.FileName = DefaultFileName.IsEmpty ? String.Empty : DefaultFileName.ToString();
				dialog.InitialDirectory = InitialDirectory;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					return dialog.OpenFile();
				}
			}
			return null;
		}

		public Stream GetFileStream(IEnumerable<IBusiness> businessObject, out string displayFileName)
		{
			displayFileName = null;
			using (var dialog = new ZSaveFileDialog())
			{
				dialog.CheckPathExists = true;
				dialog.Filter = Filter;
				dialog.DefaultExt = DefaultExt;
				dialog.AddExtension = AddExtension;
				dialog.FileName = DefaultFileName.IsEmpty ? DefaultFileNameGeneratorWithBusinessObject(businessObject) : DefaultFileName.ToString();
				dialog.InitialDirectory = InitialDirectory;

				if (ShowSaveFileDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					displayFileName = dialog.UnmappedFileName;
					return dialog.OpenFile();
				}
			}
			return null;
		}

		#endregion

		protected virtual DialogResult ShowSaveFileDialogWithoutDispose(ZSaveFileDialog dialog)
		{
			return ZFormModaliser.ShowCommonDialogWithoutDispose(dialog);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Extension Filter")]
		public string Filter
		{
			get
			{
				return "Xml Files *.xml|*.xml";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Extension")]
		public string DefaultExt
		{
			get
			{
				return "xml";
			}
		}

		public bool AddExtension
		{
			get { return true; }
		}

		#region File Name

		public virtual ZString DefaultFileName
		{
			get { return defaultFileName; }
			set { defaultFileName = MakeFilenameSafe.MakeSafe(value); }
		}
		ZString defaultFileName = "";

		public virtual ZString InitialDirectory
		{
			get { return initialDirectory; }
			set { initialDirectory = value; }
		}
		ZString initialDirectory = "";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Name for Prompt")]
		public string DefaultFileNameGeneratorWithBusinessObject(IEnumerable<IBusiness> businessObjects)
		{
			var count = businessObjects.Count();

			if (count >= 1)
			{
				var first = businessObjects.First();
				if (count == 1)
				{
					return first.HumanReadableName + "_" + ZDateTime.Now.Ticks;
				}
				else
				{
					return first.TableName + " x " + count + "_" + ZDateTime.Now.Ticks;
				}
			}
			else
			{
				throw new InvalidOperationException("Should always pass in at least 1 businessObject");
			}
		}

		#endregion
	}
}
