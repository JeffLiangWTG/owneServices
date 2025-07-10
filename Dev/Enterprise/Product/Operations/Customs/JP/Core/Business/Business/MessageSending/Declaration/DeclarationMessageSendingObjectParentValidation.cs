using System;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	public class DeclarationMessageSendingObjectParentValidation : ZValidation
	{
		public DeclarationMessageSendingObjectParentValidation(DeclarationMessageSendingObjectParent parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly DeclarationMessageSendingObjectParent parent;

		public void ValidateExportPath()
		{
			ValidateCalculatedProperty(parent.ExportPathInfo);
		}

		protected void CheckExportPath()
		{
			if (parent.Context.SendTarget == SendTarget.FlatFile && (parent.ExportPath.IsEmpty || !Directory.Exists(parent.ExportPath)))
			{
				parent.ExportPathInfo.AddError(Res.GetString("89CC6C40-98E8-45D4-AF4B-07F304D0C71B", "Please select a valid directory path to export the message flat file."));
			}
		}

		public override Type AutoValidationType => typeof(DeclarationMessageSendingObjectParentValidation);

		public override void ValidateAll()
		{
			ValidateExportPath();
		}
	}
}
