using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Base.AccStatement
{
	public class BankStatementFormatValidation : ZValidation
	{
		public BankStatementFormatValidation(BankStatementFormat parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		protected BankStatementFormat Parent;

		public override void ValidateAll()
		{
			ValidateStatementFileFormatName();
		}

		public override Type AutoValidationType
		{
			get { return typeof(BankStatementFormatValidation); }
		}

		public void ValidateStatementFileFormatName()
		{
			ValidateCalculatedProperty(Parent.StatementFileFormatNameInfo);
		}

		protected virtual void CheckStatementFileFormatName()
		{
			if (!Parent.IsValidationSuspended)
			{
				MandatoryValidation.CheckEntered(Parent.StatementFileFormatNameInfo);
				if (!Parent.StatementFileFormatNameInfo.HasErrors())
				{
					try
					{
						BankStatementFormat.StatementFileFormats dummy = Parent.StatementFileFormat;
					}
					catch (ArgumentException ex)
					{
						Parent.StatementFileFormatNameInfo.AddError(ex.Message);
					}
				}
			}
		}
	}
}
