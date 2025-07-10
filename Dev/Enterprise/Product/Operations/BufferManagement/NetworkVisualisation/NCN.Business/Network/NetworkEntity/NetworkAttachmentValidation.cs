using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class NetworkAttachmentValidation : ZValidation
	{
		public NetworkAttachmentValidation(NetworkAttachment parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly NetworkAttachment parent;
		NetworkAttachment Parent => parent;

		public override Type AutoValidationType
		{
			get { return typeof(NetworkAttachment); }
		}

		public override void ValidateAll()
		{
			ValidateBackInTimeArrows();
		}

		public void ValidateBackInTimeArrows()
		{
			Parent.ToggleRowWarningSafe(!Parent.Attachment.BNA_IsDecouple && Parent.IsBackInTime, BackInTimeArrowMessage);
		}

		static string BackInTimeArrowMessage
		{
			get { return Res.GetString("443004c6-c2fe-4e26-9acc-ab2aeb0afbea", "The post-requisite is scheduled to begin before its pre-requisite is scheduled to complete."); }
		}
	}
}
