using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Module.Testing
{
	[TestedType(typeof(ArchiveEDocsController))]
	internal sealed class ArchiveEDocsControllerTest : ZSingletonControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(ArchiveEDocsManager);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new ArchiveEDocsManager(new DocumentFactoryProvider().GetFactory(Factory));
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ArchiveEDocs;
		}
	}
}
