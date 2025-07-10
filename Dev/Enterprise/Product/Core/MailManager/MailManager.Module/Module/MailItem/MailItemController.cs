using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MailManager.Module
{
	/// <summary>
	/// Module Controller for MailItem.
	/// </summary>
	public class MailItemController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public MailItemController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get	{ return ModuleIDs.MailItem; } // needed for Next/Previous buttons			
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.MailItem; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(MailItem); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new MailItemForm((MailItem)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var mailItem = (MailItem)base.GetNewBusinessEntityInLocalFactory();
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			mailItem.AddRecipientFromRecipientDef(new RecipientDef(Env.Registry.MailboxEmailAddress), MailRecipient.RecipientTypes.TO);
			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_Header += (NoResString)"X-CreatedByCargoWiseForTesting: true\r\n";
			return mailItem;
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.Emails; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Emails; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.Emails; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.Emails; }
		}

#if DEBUG

		[Serializable]
		public class MailItemNotSupportedActionException : ModuleFeatureNotSupportedException
		{
			public MailItemNotSupportedActionException() : base("")
			{
			}

#if NETFRAMEWORK
			protected MailItemNotSupportedActionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{ }
#endif
		}

#endif

	}
}
