using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.WCB.Module;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.WCB.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class WCBClientOverrideTest : ClientOverrideTest
	{
		public void TestBizOverrides()
		{
			ClientOverride @override = ClientOverride.Instance;
			AssertEquals("Client", Clients.WCB, @override.Client);
			AssertEquals("Client Display Name", "Watson Curro Burke Menta", @override.ClientDisplayName);
			AssertEquals("Help Web Page", ZString.Empty, @override.HelpWebPage);
			AssertEquals("AdditionalRegistryItemSet", WCBDataRegistry.Instance, @override.AdditionalRegistryItemSet);
			AssertNotNull("ModuleOverrides should contain JobDeclaration module", @override.ModuleOverrides[ModuleIDs.Customs.JobDeclaration, Core.Constants.CountryCodes.Australia]);
			AssertEquals("ModuleOverride should be DeclarationModuleOverride", typeof(DeclarationModuleOverride).FullName, @override.ModuleOverrides[ModuleIDs.Customs.JobDeclaration, Core.Constants.CountryCodes.Australia].TypePath.Split(',')[0]);
		}

		public static void TestTypeDecider()
		{
			var factory = new BusinessObjectFactory();
			var entryLine = factory.New<CusEntryLine>();
			AssertEquals(typeof(WCBCusEntryLine).FullName, typeof(WCBCusEntryLine), entryLine.GetType());
			var declaration = factory.New<JobDeclaration>();
			AssertEquals(typeof(JobDeclarationWithFixedInvHeads).FullName, typeof(JobDeclarationWithFixedInvHeads), declaration.GetType());
			var invoice = factory.New<JobComInvoiceHeader>();
			AssertEquals(typeof(InvHeadWithFixedInvLines).FullName, typeof(InvHeadWithFixedInvLines), invoice.GetType());
		}

#region Implementation
		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		class ClientOverrideTestClass : ClientOverride
		{
		}
#endregion
	}
}
