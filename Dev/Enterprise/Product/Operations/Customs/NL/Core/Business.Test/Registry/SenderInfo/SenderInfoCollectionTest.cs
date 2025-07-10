using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(SenderInfoCollection))]
class SenderInfoCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<SenderInfoCollection>
{
	protected override bool RequiresFactory
	{
		get { return true; }
	}

	protected override bool RequiresFallbackLevel
	{
		get { return true; }
	}

	protected override SenderInfoCollection GetCollectionToTest()
	{
		return new SenderInfoCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return new SenderInfo();
	}
}
