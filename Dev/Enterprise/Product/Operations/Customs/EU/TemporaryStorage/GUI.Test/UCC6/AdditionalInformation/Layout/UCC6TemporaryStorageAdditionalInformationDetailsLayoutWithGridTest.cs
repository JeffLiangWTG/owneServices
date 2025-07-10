using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageAdditionalInformationDetailsLayoutWithGrid))]
	sealed class UCC6TemporaryStorageAdditionalInformationDetailsLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (UCC6TemporaryStorageAdditionalInformationDetailsControlBag.Instance.KindDropEdit, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageAdditionalInformationDetailsControlBag.Instance.FullTypeCodeFindBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageAdditionalInformationDetailsControlBag.Instance.ReferenceTextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageAdditionalInformationDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new UCC6TemporaryStorageAdditionalInformationDetailsLayoutBuilder();

		protected override Type ExpectedGridUserControlType => typeof(UCC6TemporaryStorageAdditionalInfosUserControlWithGrid);
	}

	sealed class UCC6TemporaryStorageAdditionalInformationDetailsLayoutWithGrid_BillLevelTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (UCC6TemporaryStorageAdditionalInformationDetailsControlBag.Instance.ReferenceTextBox, ControlWidthClass.Long);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new UCC6TemporaryStorageAdditionalInformationDetailsLayoutBuilder();

		protected override Type ExpectedGridUserControlType => typeof(UCC6TemporaryStorageAdditionalInfosUserControlWithGrid);

		protected override IPanelLayoutProvider GetNewPanelLayoutProvider() => new UCC6TemporaryStorageAdditionalInformationDetailsLayoutWithGrid(UCC6TemporaryStorageAdditionalInfosUserControlWithGrid.UCC6TemporaryStorageBillAdditionalInfoBingdingMemberName);
	}
}
