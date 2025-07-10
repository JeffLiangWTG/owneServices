using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.GUI;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;
using ZClientEDI.Business;

namespace ZClientEDI.Test.Registry
{
	[TestedType(typeof(WTGActiveDirectoryCredentialsControl))]
	public class WTGActiveDirectoryCredentialsControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return WTGActiveDirectoryCredentials.DefaultValue;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((WTGActiveDirectoryCredentialsControl)control).ReadOnly;
		}
	}
}
