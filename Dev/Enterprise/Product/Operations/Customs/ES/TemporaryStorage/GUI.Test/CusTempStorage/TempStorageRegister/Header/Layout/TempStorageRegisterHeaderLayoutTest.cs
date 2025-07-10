using System.Collections.Generic;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStorageRegisterHeaderLayout))]
	public class TempStorageRegisterHeaderLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (TempStorageRegisterHeaderControlBag.Instance.InternalReferenceTextBox, ControlWidthClass.Auto);
				yield return (TempStorageRegisterHeaderControlBag.Instance.DDTNumberUserControl, ControlWidthClass.Auto);
				yield return (TempStorageRegisterHeaderControlBag.Instance.ArrivalDateEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (TempStorageRegisterHeaderControlBag.Instance.PreviousReferenceTypeDropEdit, ControlWidthClass.Auto);
				yield return (TempStorageRegisterHeaderControlBag.Instance.PreviousReferenceNumberTextBox, ControlWidthClass.Auto);
				yield return (TempStorageRegisterHeaderControlBag.Instance.PresentationDateEdit, ControlWidthClass.Auto);
				yield return (TempStorageRegisterHeaderControlBag.Instance.StatusDropEdit, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TempStorageRegisterHeaderLayoutBuilder<CusTempStorageRegHeader>();
	}
}
