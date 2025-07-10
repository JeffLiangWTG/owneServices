using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	class JobDeclarationFormTest_ForWhenDeclarationCancelled : Customs.GUI.Testing.BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<JobDeclaration>
	{
		public override void TestMinimumSizeNotTooBig()
		{
			using (var form = GetFormToBashCore())
			{
				FormHelper.AssertMinimumSizeNotTooBig(form);
			}
		}

		protected override BaseJobDeclaration CreateDeclarationForPerformanceTest(BusinessObjectFactory factory)
		{
			var result = factory.New<JobDeclaration>();
			result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			result.JE_MessageType = MessageTypeForFormBashing;
			result.JE_TransportMode = Core.Constants.TransportModes.Sea;
			return result;
		}

		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Import;

		protected override Dictionary<string, string> GetIgnoreControlForLock()
		{
			var result = base.GetIgnoreControlForLock();

			string[] codes = { Core.Constants.Customs.DeclarationTabPages.Codes.Declaration, Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationOrganizations };
			const string controls = "CIQCodeTextBox,CustomsCodeTextBox,SocialCreditCodeTextBox";
			foreach (var code in codes)
			{
				if (result.TryGetValue(code, out var oldValue))
				{
					result[code] = string.Join(",", oldValue, controls);
				}
				else
				{
					result[code] = controls;
				}
			}

			return result;
		}
	}
}
