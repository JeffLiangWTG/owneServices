
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Enterprise.Build.Database.Script
{
	abstract class DBCreateTriggerScriptTest : DbCreateScriptTest
	{
		public void TestTriggerContainsRequiredRecommendations()
		{
			// Arrange
			var message = @"All triggers should begin with the following lines: '
BEGIN
    SET NOCOUNT ON
'.
It is not recommended to use '@@ROWCOUNT' in triggers.
";

			// Act
			var isGoodTriggerOrInBaseline = IsGoodTrigger || Baseline.Contains(ScriptToTest.Name);

			// Assert
			AssertEquals(message, true, isGoodTriggerOrInBaseline);
		}

		public void TestTriggerDoesNotContainDeprecatedCodeStyle()
		{
			// Arrange
			var message = @"Outdated and imperfect approach. This is because:
1. 'SET NOCOUNT ON' only takes effect when placed on the first line.
2. It is not recommended to use 'IF (@@ROWCOUNT = 0) RETURN' in triggers. It is suggested to use 'IF NOT EXISTS(SELECT NULL FROM inserted)' or 'IF NOT EXISTS(SELECT NULL FROM deleted)' instead.
";

			// Act
			var isNotDeprecatedOrInBaseline = !IsDeprecatedCodeStyle || Baseline.Contains(ScriptToTest.Name);

			// Assert
			AssertEquals(message, true, isNotDeprecatedOrInBaseline);
		}

		public void TestGoodTriggerIsNotInBaseLine()
		{
			// Arrange
			var message = $"Good Trigger Is Not In BaseLine: {ScriptToTest.Name}.\n";

			// Act
			var goodTriggerIsNotInBaseLine = !IsGoodTrigger || !Baseline.Contains(ScriptToTest.Name);

			// Assert
			AssertEquals(message, true, goodTriggerIsNotInBaseLine);
		}

		HashSet<string> GetDBCreateTriggerScriptBaseline()
		{
			var baseline = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DBCreateTriggerScriptBaseline.txt"))
			using (var reader = new StreamReader(stream))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					baseline.Add(line);
				}
			}

			return baseline;
		}

		bool IsDeprecatedCodeStyle
		{
			get
			{
				var deprecatedCodeStyle = @"
BEGIN
	IF (@@ROWCOUNT = 0) RETURN
	SET NOCOUNT ON";

				return TriggerDefinition.Contains(deprecatedCodeStyle);
			}
		}

		bool IsGoodTrigger
		{
			get
			{
				var requiredRowCountFilter = @"
BEGIN
	SET NOCOUNT ON";
				var notRecommendedRowCountFilter = "@@ROWCOUNT";

				return TriggerDefinition.Contains(requiredRowCountFilter) && !TriggerDefinition.Contains(notRecommendedRowCountFilter);
			}
		}

		HashSet<string> Baseline => baseline ?? (baseline = GetDBCreateTriggerScriptBaseline());
		HashSet<string> baseline;

		string TriggerDefinition => triggerDefinition ?? (triggerDefinition = GetScriptFromDb());
		string triggerDefinition;
	}
}

