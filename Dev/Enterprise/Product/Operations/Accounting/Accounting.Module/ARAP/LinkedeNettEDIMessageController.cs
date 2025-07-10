using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Environment;
using Enterprise.Messaging.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.Module
{
	public class LinkedeNettEDIMessageController : EDIMessageController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.LinkedeNettEDIMessage; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(LinkedeNettEDIMessage); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			LinkedeNettEDIMessage message = businessEntity as LinkedeNettEDIMessage;
			IZForm form = message != null ? new LinkedeNettEDIMessageForm(message) : base.GetForm(businessEntity);
			return form;
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Accounting.Module.Res.GetData("PlugInTabPage|LinkedeNettEDIMessage", "EDI Messages", "The EDI Messages tab."); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new LinkedeNettEDIMessagePlugin(businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Factory.New<InvoiceLinkedeNettEDIMessage>();
		}

		#region Security checkpoints 

		/// <summary>
		/// Security checkpoints are not required here. They are added on a relevant module at the time of adding the plugin
		/// </summary>
		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
