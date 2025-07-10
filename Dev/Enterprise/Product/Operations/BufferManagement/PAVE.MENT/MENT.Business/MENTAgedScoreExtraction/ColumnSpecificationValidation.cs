using System;
using CargoWise.EntityFramework;

namespace Enterprise.PAVE.MENT.Business
{
	public class ColumnSpecificationValidation : ZValidation
	{
		public ColumnSpecificationValidation(ColumnSpecification parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly ColumnSpecification parent;

		public void ValidateSequence()
		{
			ValidateCalculatedProperty(parent.SequenceInfo);
		}

		protected void CheckSequence()
		{
			CompareValidation.CheckGreaterThanOrEqualTo(parent.SequenceInfo, 0);

			if (parent.Sequence > 0 && !parent.Selected)
			{
				parent.SequenceInfo.AddWarning(Res.GetString("29b81e93-a12e-4000-b155-9c27f0d38a51", "This column is currently not included in the series"));
			}
		}

		public override void ValidateAll()
		{
			ValidateSequence();
		}

		public override Type AutoValidationType
		{
			get { return typeof(ColumnSpecificationValidation); }
		}
	}
}
