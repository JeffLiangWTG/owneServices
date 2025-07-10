using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DummyWithCodeDescriptionPairListValidation : DummyBizoValidation
	{
		public DummyWithCodeDescriptionPairListValidation(DummyWithCodeDescriptionPairList parent)
			: base(parent)
		{
		}

		public new DummyWithCodeDescriptionPairList Parent
		{
			get { return (DummyWithCodeDescriptionPairList)base.Parent; }
		}

		protected override void CheckZ0_FK_Code()
		{
			base.CheckZ0_FK_Code();
			ListValidation.ErrorIfInvalidCode(Parent.Z0_FK_CodeInfo, Parent.DummyList);
		}
	}
}
