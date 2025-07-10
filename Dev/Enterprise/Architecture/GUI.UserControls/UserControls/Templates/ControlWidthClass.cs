namespace Enterprise.ZArchitecture.GUI
{
	public enum ControlWidthClass
	{
		/// <summary>
		/// Control has a predefined width that usually should not be changed.<br/>
		/// Example: <see cref="ZDateEdit"/>.
		/// </summary>
		Auto,

		/// <summary>
		/// Control should span only a part of a column width.<br/>
		/// Example: <see cref="ZTextBox"/>.
		/// </summary>
		Medium,

		/// <summary>
		/// Control should span an entire column width.<br/>
		/// Examples:<br/>
		/// - <see cref="ZCodeFindBox"/> with <see cref="ZCodeFindBox.ShowDescriptionBox"/> set to true;<br/>
		/// - <see cref="ZAddressControl"/>.
		/// </summary>
		Long,

		/// <summary>
		/// Control should span an entire column width including area that is reserved for captions.<br/>
		/// Example: Offices grid used by EU countries.
		/// </summary>
		LongNoCaption,

		/// <summary>
		/// Control should span an wide column width which for big control.<br/>
		/// Example: SupportingInformationControl.
		/// </summary>
		LongControl,

		/// <summary>
		/// Control does not follow any predefined width because the layout has its own ruler definition.<br/>
		/// Example: All controls found on ExampleLayouts.
		/// </summary>
		CustomWidth
	}
}
