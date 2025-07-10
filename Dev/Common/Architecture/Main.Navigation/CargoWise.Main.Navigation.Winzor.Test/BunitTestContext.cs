using Bunit;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test;

public abstract class BunitTestContext : TestContextWrapper
{
	[SetUp]
	public virtual void Setup() => TestContext = new Bunit.TestContext();

	[TearDown]
	public virtual void TearDown() => TestContext?.Dispose();
}
