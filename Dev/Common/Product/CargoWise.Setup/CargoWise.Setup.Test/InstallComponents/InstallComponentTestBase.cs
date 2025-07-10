using NUnit.Framework;

namespace CargoWise.Setup.Test.InstallComponents;

internal abstract class InstallComponentTestBase
{
	protected abstract IInstallationComponent CreateComponentForIdempotencyTest();

	protected virtual void AssertSuccess()
	{
	}

	[Test]
	[Explicit]
	public void TestIsIdempotent()
	{
		var installer = CreateComponentForIdempotencyTest();
		try
		{
			installer.Install(null!, CancellationToken.None);

			// Act
			Assert.DoesNotThrow(() =>
			{
				installer.Install(null!, CancellationToken.None);
			});

			// Assert
			AssertSuccess();
		}
		finally
		{
			installer.Remove(null!, CancellationToken.None);
		}
	}

	[Test]
	[Explicit]
	public void TestIsIdempotentConcurrent()
	{
		var installer = CreateComponentForIdempotencyTest();
		try
		{
			Task.Run(() => installer.Install(null!, CancellationToken.None));

			// Act
			Assert.DoesNotThrow(() =>
			{
				installer.Install(null!, CancellationToken.None);
			});

			// Assert
			AssertSuccess();
		}
		finally
		{
			installer.Remove(null!, CancellationToken.None);
		}
	}
}
