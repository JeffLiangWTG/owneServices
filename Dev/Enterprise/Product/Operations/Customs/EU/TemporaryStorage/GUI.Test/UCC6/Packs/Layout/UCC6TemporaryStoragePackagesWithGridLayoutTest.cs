using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStoragePackagesWithGridLayout))]
	public class UCC6TemporaryStoragePackagesWithGridLayoutTest : LayoutsAbstractTest
	{
		public void TestDefaultVisibilities_Transfer()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
			var layout = new UCC6TemporaryStoragePackagesWithGridLayout().Layout;

			CombineAssertions(() =>
			{
				AssertEquals("ContainerDropEdit", false, layout.IsVisible(UCC6TemporaryStoragePackageDetailsControlBag.Instance.ContainerDropEdit, header));
				AssertEquals("MarksAndNumberTextBox", false, layout.IsVisible(UCC6TemporaryStoragePackageDetailsControlBag.Instance.MarksAndNumberTextBox, header));
			});
		}

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumn;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumn
		{
			get
			{
				yield return (UCC6TemporaryStoragePackageDetailsControlBag.Instance.ContainerDropEdit, ControlWidthClass.Long);
				yield return (UCC6TemporaryStoragePackageDetailsControlBag.Instance.PackQtyCalcEdit, ControlWidthClass.Long);
				yield return (UCC6TemporaryStoragePackageDetailsControlBag.Instance.PackUQDropEdit, ControlWidthClass.Long);
				yield return (UCC6TemporaryStoragePackageDetailsControlBag.Instance.MarksAndNumberTextBox, ControlWidthClass.Long);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new UCC6TemporaryStoragePackageDetailsBuilder();

		protected override Type ExpectedGridUserControlType => typeof(UCC6TemporaryStoragePackagesGridControl);
	}
}
