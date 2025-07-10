#if DEBUG

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	/// <summary>
	/// Singleton that rethrows exceptions that have been thrown by ExceptionReporter.
	/// The reason for doing this is as follows:
	/// 1. These exceptions are important, because they represent error dialogs that the user would see.
	/// 2. These exceptions are sometimes caught and discarded ... e.g., by .NET controls!
	/// 3. Rethrowing them ensures a unit test will fail whenever one happens.
	/// </summary>
	public class ExceptionReporterTestListener : BaseTestListener, IEnumerable<Exception>
	{
		public static ExceptionReporterTestListener Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new ExceptionReporterTestListener();
				}

				return fInstance;
			}
		}

		public bool Enabled;

		public override void StartTest(TestCase test, DateTime startTime)
		{
			Enabled = true;
			rememberedExceptions.Clear();
		}

		public override void EndTest(TestCase test, DateTime endTime)
		{
			UnitTestUserNotification.Instance.ClearShownErrorKeys();

			if (Count > 0)
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(Count + " exception" + (Count == 1 ? " was" : "s were") + " shown in the ExceptionReporter.");

				foreach (ExceptionWithStack ex in rememberedExceptions)
				{
					builder.Append("<br><br>".PadRight(20, '-'));
					builder.Append(ex.ToHtmlString());
				}

				rememberedExceptions.Clear();
				ErrorReporter.Clear();
				throw new AssertionFailedError(builder.ToString());
			}
			else
			{
				ErrorReporter.Clear();
			}
		}

		public void Add(Exception ex, string key = "", string message = "")
		{
			if (Enabled)
			{
				rememberedExceptions.Add(new ExceptionWithStack(ex, key, message));
			}
		}

		internal class ExceptionWithStack
		{
			public ExceptionWithStack(Exception ex, string key, string message)
			{
				Ex = ex;
				Stack = GenerateStack();
				ExceptionKey = key;
				ExceptionMessage = message;
			}

			public string ToHtmlString()
			{
				var text = ToString();
				var htmlEncoded = System.Security.SecurityElement.Escape(text);

				return htmlEncoded?.Replace(System.Environment.NewLine, "<br>");
			}

			public override string ToString()
			{
				return Ex.ToString() + System.Environment.NewLine + Stack;
			}

			internal readonly Exception Ex;
			internal readonly string Stack;
			public string ExceptionKey { get; }
			public string ExceptionMessage { get; }

			protected virtual string CurrentStackTrace()
			{
				return System.Environment.StackTrace;
			}

			string GenerateStack()
			{
				string tempStack = CurrentStackTrace();
				StringBuilder builder = new StringBuilder(tempStack.Length);

				using (StringReader reader = new StringReader(tempStack))
				{
					string line = reader.ReadLine();
					int i = 0;
					while (line != null)
					{
						if (i++ > 4)
						{
							while (line != null)
							{
								builder.Append(line + System.Environment.NewLine);
								if (line.IndexOf("NUnit.Framework.TestCase.RunTest()") >= 0)
								{
									break;
								}
								line = reader.ReadLine();
							}
							break;
						}
						line = reader.ReadLine();
					}
				}

				return TracerHelper.RemoveReportElements(builder.ToString());
			}
		}

		public void Clear()
			=> rememberedExceptions.Clear();

		public Exception this[int index]
			=> rememberedExceptions[index].Ex;

		public int Count
			=> rememberedExceptions.Count;

		public IEnumerator<Exception> GetEnumerator()
			=> rememberedExceptions.Select(r => r.Ex).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public string GetExceptionKey(int index) => rememberedExceptions[index].ExceptionKey;

		public string GetExceptionMessage(int index) => rememberedExceptions[index].ExceptionMessage;

		#region Implementation

		protected static ExceptionReporterTestListener fInstance;
		readonly List<ExceptionWithStack> rememberedExceptions = new List<ExceptionWithStack>();

		protected ExceptionReporterTestListener()
		{
		}

		#endregion
	}
}

#endif
