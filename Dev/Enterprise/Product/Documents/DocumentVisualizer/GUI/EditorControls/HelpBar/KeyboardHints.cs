namespace Enterprise.DocumentVisualizer.GUI
{
	static class KeyboardHints
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description strings")]
		public static class Shortcuts
		{
			public const string Enter = "Enter";
			public const string Escape = "Esc";
			public const string NewLine = "Alt+Enter";
			public const string TabForward = "Tab";
			public const string TabBackward = "Shift+Tab";
		}

		public static class Hints
		{
			public static string Enter { get { return Res.GetString("93b764fa-111a-4eec-a03e-f9f591f8947b", "Commit changes made."); } }
			public static string Escape { get { return Res.GetString("2cf6ba96-08fa-4bcd-9e3a-34fc3b34c762", "Leave control without committing changes."); } }
			public static string NewLine { get { return Res.GetString("3dec4f4c-f611-44a3-885c-e543ae0fe87d", "Enter a new line."); } }
			public static string NewLinePopup { get { return Res.GetString("0e3cda80-6589-4c53-ad60-9f5ec453fa61", "Enter a new line (if multi line text)."); } }
			public static string TabForward { get { return Res.GetString("05e8d992-d7d1-4a85-a1e9-9c96003eca6f", "Tab to next control (changes committed)."); } }
			public static string TabBackward { get { return Res.GetString("340a3104-cc74-4449-96a7-06537b508bd9", "Tab to previous control (changes committed)."); } }
		}
	}
}
