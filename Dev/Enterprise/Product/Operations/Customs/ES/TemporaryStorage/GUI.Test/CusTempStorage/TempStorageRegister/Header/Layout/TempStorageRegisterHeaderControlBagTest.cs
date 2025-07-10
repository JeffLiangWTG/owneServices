using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStorageRegisterHeaderControlBag))]
	public class TempStorageRegisterHeaderControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TempStorageRegisterHeaderControlBag.InternalReferenceTextBox);
				yield return nameof(TempStorageRegisterHeaderControlBag.StatusDropEdit);
				yield return nameof(TempStorageRegisterHeaderControlBag.PreviousReferenceTypeDropEdit);
				yield return nameof(TempStorageRegisterHeaderControlBag.PresentationDateEdit);
				yield return nameof(TempStorageRegisterHeaderControlBag.ArrivalDateEdit);
				yield return nameof(TempStorageRegisterHeaderControlBag.PreviousReferenceNumberTextBox);
				yield return nameof(TempStorageRegisterHeaderControlBag.DDTNumberUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TempStorageRegisterHeaderControlBag.Instance;
	}
}
