using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Common;
using Dat.Integration;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using DatTestClient = Dat.Integration.TestClient;
using DatTestResult = Dat.Integration.TestResult;
using NUnitTestResult = NUnit.Framework.TestResult;

namespace Enterprise.Dat.Implementation
{
	public sealed class TestRunner : ITestRunner, IDisposable
	{
		internal TestRunner()
		{
			Initialise(UnitTestListenersFactory.GetTestListeners(), new Testing.UnitTestErrorDescriptionListFactory());
		}

		internal void Initialise(ITestListener[] listeners, ErrorDescriptionListFactory descriptionListFactory)
		{
			errorDescriptionListFactory = descriptionListFactory;
			this.listeners = listeners.ToList();
		}

		public bool IsInitialised
		{
			get { return errorDescriptionListFactory != null && listeners != null; }
		}

		public void AddTestListener(ITestListener testListener)
		{
			listeners.Add(testListener);
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public void RemoveTestListener<T>()
			where T : ITestListener
		{
			var listener = listeners.OfType<T>().FirstOrDefault();
			if (listener != null)
			{
				listeners.Remove(listener);
			}
		}

		public DatTestResult[] RunTests(TestDescriptor[] tests)
		{
			return (DatTestResult[])InvokeOnDatForm(() => DoRunTests(tests));
		}

		public object InvokeOnDatForm(Func<object> func) => DatForm.Instance.InvokeEx(func);

		internal DatTestResult[] DoRunTests(TestDescriptor[] tests)
		{
			var testListener = CreateTestRunnerTestListener();
			using (var testResult = CreateNUnitTestResult(testListener))
			{
				return DoRunTests(tests, testListener, testResult);
			}
		}

		public TestRunnerTestListener CreateTestRunnerTestListener() => new TestRunnerTestListener(errorDescriptionListFactory);

		public NUnitTestResult CreateNUnitTestResult(TestRunnerTestListener testListener)
		{
			var testListeners = new List<ITestListener>();
			testListeners.Add(testListener);
			testListeners.AddRange(listeners);
			return new NUnitTestResult(testListeners.ToArray());
		}

		public DatTestResult[] DoRunTests(TestDescriptor[] tests, TestRunnerTestListener testListener, NUnitTestResult testResult)
		{
			var tr = new DatTestResult[tests.Length];
			for (int i = 0; i < tests.Length; i++)
			{
				var td = tests[i];

				Type typeToTest = null;
				Assembly assemblyToTest = null;

				try
				{
					assemblyToTest = LoadAssembly(td.Identifier.ScopeName);
					typeToTest = assemblyToTest.GetType(td.Identifier.ElementName, true);
					testResult.BeforeTestInstantiated();
					RunTestWithTestReference(td, typeToTest, testResult);
					testResult.AfterTestSetToNull();
					tr[i] = testListener.GetTestResult(td);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					tr[i] = HandleInvokeFailure(ex, td, assemblyToTest, typeToTest);
				}
			}

			DatTestClient.CheckForEmptyResults(tr, "Enterprise.Dat.Implementation.TestRunner");

			return tr;
		}

		string NullDescription(string name, object objectToTest)
		{
			return name + " is " + ((objectToTest == null) ? "null" : "not null");
		}

		Assembly LoadAssembly(string assemblyName)
		{
			Assembly assemblyToTest = null;

			for (int i = 0; i < 5; i++)
			{
				try
				{
					assemblyToTest = Assembly.Load(assemblyName);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (i < 5 && (ex is FileLoadException || ex is FileNotFoundException))
					{
						//Dat suffers occasional unexplained failures loading DLLs due to the files being in use,
						//which shouldn't stop the DLLs loading anyway. This seems to happen most when two or more
						//client machines are loading the same assembly at very close to the same time.
						Thread.Sleep(10000);
					}
					else
					{
						throw;
					}
				}
			}

			return assemblyToTest;
		}

		void RunTestWithTestReference(TestDescriptor td, Type testType, NUnitTestResult nunitTestResult)
		{
			ITest test = new TestSuite(testType).NewTest(td.Identifier.TargetName);
			try
			{
				if (test != null)
				{
					test.Run(nunitTestResult);
				}
				else
				{
					throw new Exception("ITest is null, cannot invoke " + td.Identifier.TargetName);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				nunitTestResult.AddError(ex, test);
			}
		}

		DatTestResult HandleInvokeFailure(Exception ex, TestDescriptor td, Assembly assemblyToTest, Type typeToTest)
		{
			var message = $"Exception invoking test: <{td.Identifier}>";
			message += System.Environment.NewLine + NullDescription("AssemblyToTest", assemblyToTest);
			message += System.Environment.NewLine + NullDescription("TypeToTest", typeToTest);
			message += System.Environment.NewLine + ex.ToString();
			return new DatTestResult(td, TimeSpan.Zero, DateTime.UtcNow, message);
		}

		public void Dispose()
		{
			DatForm.Instance.Close();
		}

		List<ITestListener> listeners;
		ErrorDescriptionListFactory errorDescriptionListFactory;
	}
}
