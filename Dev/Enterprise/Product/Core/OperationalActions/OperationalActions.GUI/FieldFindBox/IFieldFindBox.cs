using System;

namespace Enterprise.Services.OperationalActions.GUI
{
	interface IFieldFindBox
	{
		string Value { get; set; }
		bool AllowReadOnly { get; }
		Type RootType { get; }
	}
}
