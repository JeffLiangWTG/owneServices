using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BatchProcessor
{
	public abstract class MultipleInstancesTest : TestCase
	{
		protected BusinessObjectFactory Factory;

		[UseSnapshotProtection]
		public void TestExecute()
		{
			Factory = new BusinessObjectFactory();

			CreateMailItemsForEMailFilter();

			var threadHammer = new ThreadHammer<EmailReaderBatchProcess>(20, i =>
			{
				var p = GetEmailReaderProcessor();
				p.ExecuteInternal();
				return p;
			});

			var processors = threadHammer.Run();
			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory { RefreshEnabled = false };
				var processed = GetEmails(factory, MailStatus.Processed, MailStatus.Failed);

				AssertEquals(NumberOfEmailsExpectedToBeProcessed, processed.Length);
				processed.ForEach(AssertEmailIncludedInFilter);
			});

			MailItem[] GetEmails(BusinessObjectFactory factory, params string[] status)
				=> factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_Status, status));
		}

		class ThreadHammer<T>
		{
			readonly Nail[] nails;

			public ThreadHammer(int numberOfThreads, Func<int, T> action)
				=> nails = Enumerable.Range(1, numberOfThreads).Select(i => new Nail(i, action)).ToArray();

			public T[] Run()
			{
				nails.ForEach(n => n.Start());
				nails.ForEach(n => n.Join());

				var exceptions = nails.Select(n => n.Ex).WhereNotNull().ToArray();
				if (exceptions.Any())
				{
					throw new AggregateException("Exception(s) occured on runner", exceptions);
				}

				return nails.Select(n => n.Result).ToArray();
			}

			class Nail
			{
				readonly Thread t;

				public Exception Ex { get; private set; }
				public T Result { get; private set; }

				[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
				public Nail(int index, Func<int, T> process)
				{
					t = new Thread(() =>
					{
						try
						{
							using (Db.DisposableActionForDbConnection())
							{
								Result = process(index);
							}
						}
						catch (Exception ex)
						{
							this.Ex = ex;
						}
					});
				}

				public void Start() => t.Start();
				public void Join() => t.Join();
			}
		}

		protected abstract int NumberOfEmailsExpectedToBeProcessed { get; }
		protected abstract void CreateMailItemsForEMailFilter();
		protected abstract EmailReaderBatchProcess GetEmailReaderProcessor();
		protected abstract void AssertEmailIncludedInFilter(MailItem mail);
	}
}
