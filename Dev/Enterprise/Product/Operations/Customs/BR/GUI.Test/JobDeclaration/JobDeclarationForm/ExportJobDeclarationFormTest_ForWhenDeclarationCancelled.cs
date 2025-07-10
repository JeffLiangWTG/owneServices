using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	public class ExportJobDeclarationFormTest_ForWhenDeclarationCancelled : Customs.GUI.Testing.BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<JobDeclaration>
	{
		public override ZString MessageTypeForFormBashing => Common.BR.BRJobMessageTypeList.Codes.Export;

		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashingCore();

			declaration.JE_MergeBy = ZString.Empty;
			return declaration;
		}
	}
}
