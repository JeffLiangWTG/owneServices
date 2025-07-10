using NUnit.Framework;

namespace CargoWise.Setup.Test;
class ConfigurationModelTest
{
	[TestCase]
	public void TestPartialConfigurationModelCoversFullConfigurationModel()
	{
		var fullProperties = typeof(ConfigurationModel).GetProperties().Select(p => (p.Name, UnderlyingType(p.PropertyType)));
		var partialProperties = typeof(PartialConfigurationModel).GetProperties().Select(p => (p.Name, UnderlyingType(p.PropertyType)));
		Assert.Multiple(() =>
		{
			foreach (var property in fullProperties)
			{
				Assert.That(partialProperties, Does.Contain(property));
			}
		});
	}

	Type UnderlyingType(Type x) => Nullable.GetUnderlyingType(x) ?? x;
}
