using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ImportSiscomexJobDeclarationFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		public void TestRoutingVisibilty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.Routing);

				Assert("Routing should be enabled", plugIn.Enabled);
			}
		}

		public override ZString MessageTypeForFormBashing => Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;

		protected override void SetupDeclarationForSpecificFormBashing(BaseJobDeclarationForm form)
		{
			base.SetupDeclarationForSpecificFormBashing(form);
			currentFormBashingDeclaration.FixedJobMessageType = MessageTypeForFormBashing;
		}

		protected override BaseJobDeclaration CreateDeclarationForPerformanceTest(BusinessObjectFactory factory)
		{
			var result = factory.New<JobDeclaration>();

			result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			result.JE_MessageType = MessageTypeForFormBashing;
			result.JE_TransportMode = Core.Constants.TransportModes.Sea;
			return result;
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
