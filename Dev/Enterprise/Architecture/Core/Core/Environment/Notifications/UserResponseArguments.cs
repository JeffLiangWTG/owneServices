namespace Enterprise.ZArchitecture.Environment
{
	using Enterprise.ZArchitecture.Core;

	public class UserResponseArgument
	{
		public UserResponseArgument()
		{
			Caption = Message = "";
			Buttons = ZMessageBoxButtons.OKCancel;
			DefaultButton = ZMessageBoxDefaultButton.Button1;
			Icon = ZMessageBoxIcon.None;
			MinimumResponseLength = 20;
			UserResponseTextBoxCharactersCasing = ZCharacterCasing.Upper;
		}

		public string Caption { get; set; }
		public string Message { get; set; }
		public ZMessageBoxButtons Buttons { get; set; }
		public ZMessageBoxDefaultButton DefaultButton { get; set; }
		public ZMessageBoxIcon Icon { get; set; }
		public int MinimumResponseLength { get; set; }
		public int MaximumResponseLength { get; set; }
		public CodeDescriptionPairList AnswerList { get; set; }
		public string DefaultAnswer { get; set; }
		public ZCharacterCasing UserResponseTextBoxCharactersCasing { get; set; }
		public ZCharacterCasing UserResponseDropEditCharactersCasing { get; set; }
		public bool UserResponseDropEditOnlyShowCode { get; set; }
		public char PasswordChar { get; set; }
	}
}
