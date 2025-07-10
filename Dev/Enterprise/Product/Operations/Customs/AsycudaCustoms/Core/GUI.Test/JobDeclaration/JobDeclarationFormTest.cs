using Enterprise.Customs.AsycudaCustoms.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	abstract class JobDeclarationFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashingCore();
			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			cei1.CEI_Description = "1";
			return declaration;
		}
	}

	abstract class JobDeclarationFormTest_ForWhenDeclarationCancelled : Customs.GUI.Testing.BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<JobDeclaration>
	{
		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashingCore();
			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			cei1.CEI_Description = "1";
			return declaration;
		}
	}

	[TestedType(typeof(JobDeclarationForm))]
	class ImportJobDeclarationFormTest_ForWhenDeclarationCancelled : JobDeclarationFormTest_ForWhenDeclarationCancelled
	{
		public override CargoWise.Types.ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
	}
}
