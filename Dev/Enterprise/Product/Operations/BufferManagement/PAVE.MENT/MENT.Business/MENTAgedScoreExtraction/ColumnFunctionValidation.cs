using System;
using CargoWise.EntityFramework;
using Enterprise.PAVE.MENT.Shared;

namespace Enterprise.PAVE.MENT.Business
{
	public class ColumnFunctionValidation : ZValidation
	{
		public ColumnFunctionValidation(ColumnFunction parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly ColumnFunction parent;

		public void ValidateParameter1()
		{
			ValidateCalculatedProperty(parent.Parameter1Info);
		}

		protected void CheckParameter1()
		{
			if (parent.FunctionType != ColumnFunctionTypes.Codes.None)
			{
				CompareValidation.CheckGreaterThanOrEqualTo(parent.Parameter1Info, 0.001m);
			}
		}

		public override void ValidateAll()
		{
			ValidateParameter1();
			ValidateFunctionType();
		}

		public void ValidateFunctionType()
		{
			ValidateCalculatedProperty(parent.FunctionTypeInfo);
		}

		protected void CheckFunctionType()
		{
			MandatoryValidation.CheckEntered(parent.FunctionTypeInfo);
			ListValidation.ErrorIfInvalidCode(parent.FunctionTypeInfo);

			if (parent.FunctionType == ColumnFunctionTypes.Codes.Round && parent.ColumnType != MENTConstants.DecimalColumn)
			{
				parent.FunctionTypeInfo.AddError(Res.GetString("0af53fca-31b2-4d3d-9ce6-6cb7dede09d8", "Can only round decimal based columns"));
			}
		}

		public override Type AutoValidationType
		{
			get { return typeof(ColumnFunctionValidation); }
		}
	}
}
