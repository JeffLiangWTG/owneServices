using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry
{
	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.CriticalityStageMappingRegistryEditor, ZClientEDI")]
	public class CriticalityStageMappingRegistryEditorInfo : CodeDescriptionBoolTreeRegistryEditorInfo,
		IRegistryEditorInfo,
		ICodeDescriptionBoolTreeRegistryEditorInfoExtension
	{
		public CriticalityStageMappingRegistryEditorInfo(
			MultilingualString[] captions,
			MultilingualString boolColumnCaption,
			ICondition isBoolColumnVisibleCondition,
			bool[] areBoolColumnsVisible,
			bool[] areIsDefaultColumnsVisible,
			bool isOnlyBoolColumnEditable)
			: base(captions, boolColumnCaption, isBoolColumnVisibleCondition, false, isOnlyBoolColumnEditable)
		{
			AreBoolColumnsVisible = areBoolColumnsVisible;
			AreCustomizedColumnsVisible = areIsDefaultColumnsVisible;
		}

		Type IRegistryEditorInfo.BaseDataTypeToBeEdited
		{
			get { return typeof(CriticalityStageMappingCollection); }
		}

		public IReadOnlyList<string> CustomizedColumns =>
		[
			CriticalityStageMapping.Constants.CustomizedColumns.IsDefault
		];

		public IReadOnlyList<bool> AreCustomizedColumnsVisible { get; private set; }

		public IReadOnlyList<bool> AreBoolColumnsVisible { get; private set; }
	}
}

