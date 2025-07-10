using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class AutoRatingRequiredFieldsRegistryEditorInfo : RegistryEditorInfo
	{
		public AutoRatingRequiredFieldsRegistryEditorInfo(bool showIncoterm)
		{
			ShowIncoterm = showIncoterm;
		}

		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(BinaryRegistryDataType); }
		}

		public readonly bool ShowIncoterm;
	}
}
