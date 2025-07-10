using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dat.Integration;
using Enterprise.ZArchitecture.Environment;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using TestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace CWNUnit.TestAdapter
{
	public static class TestUtilities
	{
		#region TestDescriptor to TestCase

		public static TestCase TestDescriptorToTestCase(TestDescriptor testDescriptor, string assemblyFileName, Uri executorUri)
		{
			var testCase = new TestCase(testDescriptor.Identifier.ElementName + "." + testDescriptor.Identifier.TargetName, executorUri, assemblyFileName)
			{
				DisplayName = testDescriptor.Identifier.TargetName,
			};

			SetTestPropertyValue(testCase, nameof(TestDescriptor.ProjectDefinedCapabilityRequirements), testDescriptor.ProjectDefinedCapabilityRequirements);
			SetTestPropertyValue(testCase, nameof(TestDescriptor.DatTestFlags), (int)testDescriptor.DatTestFlags);
			SetTestPropertyValue(testCase, nameof(TestDescriptor.CapabilityRequirements), testDescriptor.CapabilityRequirements);

			return testCase;
		}

		public static TestDescriptor TestCaseToTestDescriptor(TestCase testCase)
		{
			var dot = testCase.FullyQualifiedName.LastIndexOf('.');
			if (dot < 0)
			{
				throw new ArgumentException("Test fully qualified name does not contain class or method name.");
			}

			var scopeName = Path.GetFileNameWithoutExtension(testCase.Source);
			var elementName = testCase.FullyQualifiedName.Substring(0, dot);
			var targetName = testCase.FullyQualifiedName.Substring(dot + 1);

			var projectDefinedCapabilityRequirements = GetTestPropertyValue<int>(testCase, nameof(TestDescriptor.ProjectDefinedCapabilityRequirements));
			var datTestFlags = (DatTestFlags)GetTestPropertyValue<int>(testCase, nameof(TestDescriptor.DatTestFlags));
			var capabilityRequirements = GetTestPropertyValue<string[]>(testCase, nameof(TestDescriptor.CapabilityRequirements));

			var testDescriptor = new TestDescriptor(scopeName, elementName, targetName, projectDefinedCapabilityRequirements, datTestFlags, capabilityRequirements);

			return testDescriptor;
		}

		public static void SetTestPropertyValue<T>(TestCase testCase, string id, T value)
		{
			testCase.SetPropertyValue(GetTestProperty<T>(id), value);
		}

		static T GetTestPropertyValue<T>(TestCase testCase, string id, T defaultValue = default)
		{
			return testCase.GetPropertyValue(GetTestProperty<T>(id), defaultValue);
		}

		static TestProperty GetTestProperty<T>(string id) => TestProperty.Find(id) ?? TestProperty.Register(id, id, typeof(T), typeof(TestCase));

		#endregion

		#region Aggregate Exceptions

		public static Exception AggregateExceptions(Exception mainException, IList<Exception> otherExceptions)
		{
			mainException = StripException(mainException);

			if (otherExceptions == null || otherExceptions.Count == 0)
			{
				return mainException;
			}

			for (int i = 0; i < otherExceptions.Count; i++)
			{
				otherExceptions[i] = StripException(otherExceptions[i]);
			}
			if (mainException != null && !otherExceptions.Contains(mainException))
			{
				otherExceptions.Insert(0, mainException);
			}

			if (otherExceptions.Count > 1)
			{
				return new AggregateException(otherExceptions);
			}

			return otherExceptions.Single();
		}

		public static Exception StripException(Exception exception)
		{
			return (exception as TargetInvocationException)?.InnerException ?? exception;
		}

		public static void ExceptionToTestResult(Exception exception, TestResult testResult)
		{
			exception = StripException(exception);

			testResult.ErrorMessage = FormatErrorMessage(exception);
			testResult.ErrorStackTrace = exception.StackTrace;
			testResult.Outcome = TestOutcome.Failed;
		}

		static string FormatErrorMessage(Exception exception)
		{
			if (exception is AggregateException aggregateException)
			{
				var sb = new StringBuilder();
				sb.Append(aggregateException.Message);

				foreach (var innerException in aggregateException.InnerExceptions)
				{
					sb.Append("<br /><br />").Append(innerException.Message);
				}

				return FormatErrorMessage(sb.ToString());
			}
			else
			{
				return FormatErrorMessage(exception.Message);
			}
		}

		#endregion

		#region Format Error Message

		static string FormatErrorMessage(string message)
		{
			// VS Test Explorer does not support HTML formatting in error message - convert to plain text.
			var htmlToText = new HtmlToTextUtility();
			return htmlToText.GetPlainText(message);
		}
		#endregion
	}
}
