namespace Enterprise.ZArchitecture.Business
{
	public enum ActionFieldType
	{
		/// <summary>
		/// Automaticly determine how the property should be presented based on the property type and
		/// what other information is available. (default)
		/// </summary>
		Auto = 0,

		/// <summary>
		/// Do not allow the property to be bulk updated or used in filters by operational actions.
		/// </summary>
		Hidden = 1,

		/// <summary>
		/// The property should be edited using a ZDateEdit control.
		/// </summary>
		DateTime = 2,

		/// <summary>
		/// The property should be edited using a ZTextBox control.
		/// </summary>
		Text = 3,

		/// <summary>
		/// The property should be edited using a ZDropEdit control.
		/// </summary>
		Code = 4,

		/// <summary>
		/// The property should be edited using a ZCodeFindBox control.
		/// </summary>
		NKModule = 5,

		/// <summary>
		/// The property should be edited using a ZFindBox control.
		/// </summary>
		PKModule = 6,

		/// <summary>
		/// The property should be edited using a ZAddressControl control.
		/// </summary>
		Address = 7,

		/// <summary>
		/// The property should be edited using a check box or 2 value drop edit.
		/// </summary>
		/// <remarks>
		/// If the value is mandatory then a check box should be used. If the value is optional then a drop edit should be used.
		/// </remarks>
		Boolean = 8,

		/// <summary>
		/// The property should be edited using a calc edit.
		/// </summary>
		Numeric = 9,

		/// <summary>
		/// The property should be edited using a ZDateEdit control, in long format, but treated as a ZDateTimeOffset (maintaining existing offset if one exists, else using offset from current branch UNLOCO).
		/// </summary>
		DateTimeOffset = 10,

		/// <summary>
		/// The property should be edited using a geography edit.
		/// </summary>
		Geography = 11,

		/// <summary>
		/// The property should be edited using a ZDateEdit control.
		/// </summary>
		Date = 12,

		/// <summary>
		/// The property should be edited using a ZTimeEdit control.
		/// </summary>
		Time = 13,
	}
}
