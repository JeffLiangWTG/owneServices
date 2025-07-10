namespace Enterprise.Interop.OutlookIntegration.Testing
{
	sealed class MockNativeOutlookApplication : Outlook._DApplication // earliest supported version of the interface
	{
		public object CreateItem(Outlook.OlItems itemType)
		{
			return new MockNativeMailItem();
		}

		#region _DApplication Members (all unimplemented)

		public Outlook.NameSpace GetNamespace(string type)
		{
			// TODO:  Add MockOutlookApplication.GetNamespace implementation
			return null;
		}

		public object CreateItemFromTemplate(string templatePath, object inFolder)
		{
			// TODO:  Add MockOutlookApplication.CreateItemFromTemplate implementation
			return null;
		}

		public Outlook.Inspector ActiveInspector()
		{
			// TODO:  Add MockOutlookApplication.ActiveInspector implementation
			return null;
		}

		public object CreateObject(string objectName)
		{
			// TODO:  Add MockOutlookApplication.CreateObject implementation
			return null;
		}

		public void Quit()
		{
			// TODO:  Add MockOutlookApplication.Quit implementation
		}

		public Outlook.Explorer ActiveExplorer()
		{
			// TODO:  Add MockOutlookApplication.ActiveExplorer implementation
			return null;
		}

		public Office.Assistant Assistant
		{
			get
			{
				// TODO:  Add MockOutlookApplication.Assistant getter implementation
				return null;
			}
		}

		#endregion
	}
}
