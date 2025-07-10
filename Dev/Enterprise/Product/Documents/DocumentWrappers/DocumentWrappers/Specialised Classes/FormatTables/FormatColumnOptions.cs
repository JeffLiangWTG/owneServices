namespace Enterprise.DocumentWrappers.FormatTables
{
	internal enum FormatColumnOptions
	{
		LeftAlign = 0x01,
		RightAlign = 0x02,
		CenterAlign = 0x03,

		AlignmentMask = 0x03,

		Default = LeftAlign,
	}
}
