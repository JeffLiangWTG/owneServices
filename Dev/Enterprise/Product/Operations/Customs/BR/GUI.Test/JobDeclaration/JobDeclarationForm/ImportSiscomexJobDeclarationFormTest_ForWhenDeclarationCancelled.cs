using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ImportSiscomexJobDeclarationFormTest_ForWhenDeclarationCancelled : Customs.GUI.Testing.BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<JobDeclaration>
	{
		public override ZString MessageTypeForFormBashing => Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;

		protected override IEnumerable<string> GetMessageSubTypesForFormBashingTest(CodeDescriptionPairList messageSubTypeList)
		{
			if (messageSubTypeList.Count > 0)
			{
				yield return messageSubTypeList[0].Code;
			}
		}
	}
}
