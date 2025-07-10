namespace Enterprise.ResourceStrings.Business
{
	public class CustomizableDataTranslationEntryValidation : AutoCustomizableDataTranslationEntryValidation
	{
		public CustomizableDataTranslationEntryValidation(AutoCustomizableDataTranslationEntry entry)
			: base(entry)
		{ }

		public new CustomizableDataTranslationEntry Parent
		{
			get { return (CustomizableDataTranslationEntry)base.Parent; }
		}

		protected override void CheckTranslation()
		{
			var info = Parent.TranslationInfo;
			ParameterConsistencyChecker.Check(Parent.English, Parent.Translation, info);
			base.CheckTranslation();

			if (!Parent.OriginalTranslation.IsEmpty)
			{
				info.AddWarning(Res.GetString("fb1536ca-3ef9-43fd-9d2c-4fe331438be4", "The maximum length of '{0}' has been exceeded.\r\nThe maximum length of this property is {1} characters, but {2} were entered. It has been truncated automatically.", info.Name, info.MaxLength, Parent.OriginalTranslation.Length));
			}
		}
	}
}
