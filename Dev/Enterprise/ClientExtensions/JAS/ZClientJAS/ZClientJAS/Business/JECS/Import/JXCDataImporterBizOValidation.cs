using System;
using System.IO;
using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class JXCDataImporterBizOValidation : ZValidation
	{
		public JXCDataImporterBizOValidation(JXCDataImporterBizO parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public override Type AutoValidationType
		{
			get { return typeof(JXCDataImporterBizOValidation); }
		}

		public override void ValidateAll()
		{
			ValidateImportFilePath();
		}

		public void ValidateImportFilePath()
		{
			ValidateCalculatedProperty(Parent.ImportFilePathInfo);
		}

		protected void CheckImportFilePath()
		{
			MandatoryValidation.CheckEntered(Parent.ImportFilePathInfo);
			if (!Parent.ImportFilePath.IsEmpty && !File.Exists(Parent.ImportFilePath))
			{
				Parent.ImportFilePathInfo.AddError("File does not exist. Please choose a different file path.");
			}
		}

		public readonly JXCDataImporterBizO Parent;
	}
}
