namespace Enterprise.Interop.OutlookIntegration.Testing
{
	sealed class MockNativeRecipients : Outlook.Recipients
	{
		public bool RecipientAdded;

		#region Recipients Members (all unimplemented)

		public Outlook.Recipient Add(string name)
		{
			RecipientAdded = true;
			return null;
		}

		public Outlook.Recipient Item(object index)
		{
			// TODO:  Add MockNativeRecipients.Item implementation
			return null;
		}

		public int Count
		{
			get
			{
				// TODO:  Add MockNativeRecipients.Count getter implementation
				return 0;
			}
		}

		public void Remove(int index)
		{
			// TODO:  Add MockNativeRecipients.Remove implementation
		}

		public bool ResolveAll()
		{
			// TODO:  Add MockNativeRecipients.ResolveAll implementation
			return false;
		}

		#endregion
	}
}
