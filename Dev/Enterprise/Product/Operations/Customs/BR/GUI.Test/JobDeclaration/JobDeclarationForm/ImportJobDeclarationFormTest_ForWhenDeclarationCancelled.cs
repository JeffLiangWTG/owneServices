using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ImportJobDeclarationFormTest_ForWhenDeclarationCancelled : Customs.GUI.Testing.BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<JobDeclaration>
	{
		public override ZString MessageTypeForFormBashing => Common.BR.BRJobMessageTypeList.Codes.Import;
	}
}
