using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	class ImportJobDeclarationFormTest : JobDeclarationFormTest
	{
		public override CargoWise.Types.ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
	}
}
