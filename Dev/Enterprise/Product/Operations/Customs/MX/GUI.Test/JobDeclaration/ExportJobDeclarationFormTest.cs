using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.MX.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ExportJobDeclarationFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		public override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Export;

		public override void TestFormIsFullyTranslatable()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				base.TestFormIsFullyTranslatable();
			}
		}

		protected override IEnumerable<string> GetMessageSubTypesForFormBashingTest(CodeDescriptionPairList messageSubTypeList)
		{
			if (messageSubTypeList.Count > 0)
			{
				yield return messageSubTypeList[0].Code;
			}
		}
	}
}
