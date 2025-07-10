using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPEJobDeclarationController))]
	internal class UPEJobDeclarationControllerBasherTest : ZControllerBasherTest
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.JobDeclaration;
		}

		public override Type ControllerToBashType
		{
			get
			{
				return typeof(UPEJobDeclarationController);
			}
		}
	}
}
