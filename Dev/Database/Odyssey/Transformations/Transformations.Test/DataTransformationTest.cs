using System;
using System.IO;
using System.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test;

public class DataTransformationTest : TestCase
{
	public void TestPreventSchemaChecksBaselineTypesExist()
	{
		// Arrange
		var baseline = GetPreventSchemaChecksBaseline();
		var allAssemblies = new[]
		{
			typeof(DataTransformation).Assembly,
			typeof(TransformationMapper).Assembly,
			typeof(DataTransformationTest).Assembly
		};

		// Act
		var errors = baseline.Where(typeName => allAssemblies.All(asm => asm.GetType(typeName) == null)).ToList();

		// Assert
		AssertContainsExactElementsInExactOrder(Array.Empty<string>(), errors);
	}

	static string[] GetPreventSchemaChecksBaseline()
	{
		var stream = typeof(DataTransformation).Assembly.GetManifestResourceStream("Enterprise.DbUpgrader.Transformation.Common.PreventSchemaChecksBaseline.txt");
		if (stream == null)
		{
			return null;
		}

		using var sr = new StreamReader(stream);
		return sr.ReadToEnd().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
	}
}
