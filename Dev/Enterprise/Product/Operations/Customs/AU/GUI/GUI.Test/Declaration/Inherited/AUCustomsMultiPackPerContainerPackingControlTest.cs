using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUCustomsMultiPackPerContainerPackingControlTest : Customs.GUI.Testing.BasePackingControlTest
	{
		protected override Customs.Business.BaseJobDeclaration GetDeclarationRelevantForPacking()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			return declaration;
		}
	}
}
