using System;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;

namespace Enterprise.Customs.CA.GUI.Testing
{
	abstract class ImportCustomsUserControlBasherTest : Customs.GUI.Testing.BaseCustomsUserControlBasherTest
	{
		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Import;

		public override Type FormToBashType => typeof(JobDeclarationForm);

		protected override Customs.Business.BaseJobDeclaration GetPopulatedDeclarationForFormBashing()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeForFormBashing;
			declaration.ApportionmentDirty = false;
			return declaration;
		}
	}
}
