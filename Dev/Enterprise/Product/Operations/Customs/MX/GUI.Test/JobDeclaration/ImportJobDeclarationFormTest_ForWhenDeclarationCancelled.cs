using System.Collections.Generic;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	class ImportJobDeclarationFormTest_ForWhenDeclarationCancelled : Customs.GUI.Testing.BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<JobDeclaration>
	{
		public override CargoWise.Types.ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

		protected override IEnumerable<string> GetMessageSubTypesForFormBashingTest(CodeDescriptionPairList messageSubTypeList)
		{
			if (messageSubTypeList.Count > 0)
			{
				yield return messageSubTypeList[0].Code;
			}
		}
	}
}
