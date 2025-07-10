using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry
{
	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.ResolutionAndClosureBehaviourRegistryEditor, ZClientEDI")]
	public class ResolutionAndClosureBehaviourRegistryEditorInfo : CodeDescriptionBoolTreeRegistryEditorInfo,
		IRegistryEditorInfo,
		ICodeDescriptionBoolTreeRegistryEditorInfoExtension
	{
		public ResolutionAndClosureBehaviourRegistryEditorInfo(
			MultilingualString[] captions,
			MultilingualString boolColumnCaption,
			ICondition isBoolColumnVisibleCondition,
			bool isBoolColumnVisible,
			bool isOnlyBoolColumnEditable,
			bool[] areBoolColumnsVisible,
			bool[] areExtraColumnsVisible)
			: base(captions, boolColumnCaption, isBoolColumnVisibleCondition, isBoolColumnVisible, isOnlyBoolColumnEditable)
		{
			AreBoolColumnsVisible = areBoolColumnsVisible;
			AreCustomizedColumnsVisible = areExtraColumnsVisible;
		}

		Type IRegistryEditorInfo.BaseDataTypeToBeEdited
		{
			get { return typeof(ResolutionAndClosureBehaviourCollection); }
		}

		public IReadOnlyList<bool> AreCustomizedColumnsVisible { get; private set; }

		public IReadOnlyList<bool> AreBoolColumnsVisible { get; set; }

		public IReadOnlyList<string> CustomizedColumns => new[]
		{
			ResolutionAndClosureBehaviour.Constants.CustomizedColumns.AllowSelfResolve,
			ResolutionAndClosureBehaviour.Constants.CustomizedColumns.DaysResolvedToClosed,
			ResolutionAndClosureBehaviour.Constants.CustomizedColumns.DaysPendingCustomerToClosed,
			ResolutionAndClosureBehaviour.Constants.CustomizedColumns.ClosedReopenRule
		};
	}
}

