using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class ManifestMessageSendingObjectParentValidation : ZValidation
	{
		public ManifestMessageSendingObjectParentValidation(ManifestMessageSendingObjectParent parent) : base(parent)
		{
			this.parent = parent;
		}

		readonly ManifestMessageSendingObjectParent parent;

		public override Type AutoValidationType => typeof(ManifestMessageSendingObjectParentValidation);

		public override void ValidateAll()
		{
			ValidateExportPath();
			ValidateEndSendMessage();
		}

		public void ValidateExportPath()
		{
			ValidateCalculatedProperty(parent.ExportPathInfo);
		}

		protected void CheckExportPath()
		{
			if (parent.Context.SendTarget == SendTarget.FlatFile && (parent.ExportPath.IsEmpty || !Directory.Exists(parent.ExportPath)))
			{
				parent.ExportPathInfo.AddError(Res.GetString("8C383C6E-06F5-499A-860B-457DBBB772FA", "Please select a valid directory path to export the message flat file."));
			}
		}

		public void ValidateEndSendMessage()
		{
			ValidateCalculatedProperty(parent.EndSendMessageInfo);
		}

		protected void CheckEndSendMessage()
		{
			if (parent.EndSendMessage && parent.header.IsHCH && parent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().Any(x => !x.ShouldSend && x.Bill.ABL_MessageStatus != JPMessageStatusList.Codes.Acknowledged))
			{
				parent.EndSendMessageInfo.AddMessageError(Res.GetString("9CB37849-80C3-445C-8185-FDA01A6C6684", "END should only be used if all house bills under the same master bill have either been registered or included in the current message."));
			}
		}
	}
}
