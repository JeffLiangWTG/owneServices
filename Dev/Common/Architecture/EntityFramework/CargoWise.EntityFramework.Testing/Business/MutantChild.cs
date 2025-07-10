using System;
using System.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	class MutantChild : DummyBusinessObject
	{
		public MutantChild(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DummyBusinessObject Parent;

		protected override void MarkAsNeedingValidationCore()
		{
			base.MarkAsNeedingValidationCore();
			if (Parent != null)
			{
				Parent.MarkAsNeedingValidation();
			}
		}

		public bool IsTestingSuspendMarkingAsNeedingValidation;

		public override ZString Z0_Code
		{
			get { return base.Z0_Code; }
			set
			{
				IDisposable suspender = IsTestingSuspendMarkingAsNeedingValidation ? SuspendMarkingAsNeedingValidation() : null;
				try
				{
					base.Z0_Code = value;
					if (value == "ABC")
					{
						Z0_Description = "DEFAULT";
					}
				}
				finally
				{
					if (suspender != null)
					{
						suspender.Dispose();
					}
				}
			}
		}

		public override ZString Z0_Description
		{
			get { return base.Z0_Description; }
			set
			{
				IDisposable suspender = IsTestingSuspendMarkingAsNeedingValidation ? SuspendMarkingAsNeedingValidation() : null;
				try
				{
					base.Z0_Description = value;
				}
				finally
				{
					if (suspender != null)
					{
						suspender.Dispose();
					}
				}
			}
		}
	}
}
