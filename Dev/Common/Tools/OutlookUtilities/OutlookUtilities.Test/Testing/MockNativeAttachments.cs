namespace Enterprise.Interop.OutlookIntegration.Testing
{
	sealed class MockNativeAttachments : Outlook.Attachments
	{
		public bool AttachmentAdded;

		#region Attachments Members

		public Outlook.Attachment Add(object source, object type, object position, object displayName)
		{
			AttachmentAdded = true;
			return null;
		}

		public Outlook.Attachment Item(object index)
		{
			// TODO:  Add MockNativeAttachments.Item implementation
			return null;
		}

		public int Count
		{
			get
			{
				// TODO:  Add MockNativeAttachments.Count getter implementation
				return 0;
			}
		}

		public void Remove(int index)
		{
			// TODO:  Add MockNativeAttachments.Remove implementation
		}

		public object Parent
		{
			get
			{
				// TODO:  Add MockNativeAttachments.Parent getter implementation
				return null;
			}
		}

		#endregion
	}
}
