using System;
using Enterprise.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry
{
	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.IncidentClosureDispositionRegistryEditor, ZClientEDI")]
	public class IncidentClosureDispositionRegistryEditorInfo : CodeDescriptionBoolTreeRegistryEditorInfo, IRegistryEditorInfo
	{
		public IncidentClosureDispositionRegistryEditorInfo(MultilingualString[] captions, MultilingualString boolColumnCaption, ICondition isBoolColumnVisibleCondition, bool isBoolColumnVisible, bool isOnlyBoolColumnEditable, bool[] areIsResolutionColumnsVisible) : base(captions, boolColumnCaption, isBoolColumnVisibleCondition, isBoolColumnVisible, isOnlyBoolColumnEditable)
		{
			AreIsResolutionColumnsVisible = areIsResolutionColumnsVisible;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
		public readonly bool[] AreIsResolutionColumnsVisible;

		Type IRegistryEditorInfo.BaseDataTypeToBeEdited
		{
			get { return typeof(IncidentClosureDispositionCollection); }
		}
	}
}
