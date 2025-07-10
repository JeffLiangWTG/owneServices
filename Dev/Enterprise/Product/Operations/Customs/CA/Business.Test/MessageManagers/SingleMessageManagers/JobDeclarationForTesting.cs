using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	class JobDeclarationForTesting : JobDeclaration
	{
		public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public int RunPreSaveValidationCount;

		protected override void RunPreSaveValidationCore()
		{
			RunPreSaveValidationCount++;
			base.RunPreSaveValidationCore();
		}
	}
}
