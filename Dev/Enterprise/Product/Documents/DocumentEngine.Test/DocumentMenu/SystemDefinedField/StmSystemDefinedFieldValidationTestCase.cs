using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	abstract class StmSystemDefinedFieldValidationTestCase : BusinessObjectValidationTestCase
	{
		protected AutoStmSystemDefinedField Parent
		{
			get
			{
				if (fParent == null)
				{
					fParent = GetNewParent();
				}
				return fParent;
			}
		}

		protected abstract AutoStmSystemDefinedField GetNewParent();
		AutoStmSystemDefinedField fParent;
	}
}
