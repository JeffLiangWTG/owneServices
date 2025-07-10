namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormsLocalizedTest]
	public class ProcessTemplateCustomFieldsCollectionDetailsControl : CustomPropertiesCollectionDetailsControl
	{
		#region Override

		protected override CustomPropertiesControl GetCustomPropertiesControl()
		{
			return new ProcessTemplateCustomFieldsControl();
		}

		#endregion
	}
}
