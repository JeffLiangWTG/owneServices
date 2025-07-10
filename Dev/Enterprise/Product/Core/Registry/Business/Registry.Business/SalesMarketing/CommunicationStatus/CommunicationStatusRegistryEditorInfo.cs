using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.CommunicationStatusRegistryItemEditor, Enterprise.Registry.GUI")]
	public class CommunicationStatusRegistryEditorInfo : IRegistryEditorInfo
	{
		public CommunicationStatusRegistryEditorInfo()
		{
		}

		public CommunicationStatusRegistryEditorInfo(MultilingualString boolColumnCaption)
		{
			this.boolColumnCaption = boolColumnCaption;
		}

		readonly MultilingualString boolColumnCaption;

		public string BoolColumnCaption
		{
			get { return boolColumnCaption ?? ""; }
		}

		#region IRegistryEditorInfo Members

		public Type BaseDataTypeToBeEdited
		{
			get { return typeof(CommunicationStatusCollection); }
		}

		#endregion
	}
}
