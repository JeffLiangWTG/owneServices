using System;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	sealed class JobDeclarationControllerTest : EU.Module.Testing.JobDeclarationControllerTest
	{
		public override void TestTemplateCopyForm()
		{
			AssertControllerNotNull();
			var declaration = (JobDeclaration)GetBusinessObjectThatIsInTheDatabase();
			foreach (CusEntryInstruction instruction in declaration.CustomsEntryInstructions)
			{
				_ = instruction.GoodsLocation;
			}
			Factory.Save();
			try
			{
				Controller.ShowTemplateCopyForm(declaration);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		public override Type ControllerToBashType => typeof(JobDeclarationController);
	}
}
