using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.Integration;
using Enterprise.ArchiveManager.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	class DummyArchiveStorage : IArchiveAction
	{
		#region IArchiveStorage Members

		public void Store(ArchiveImage image)
		{
			if (isInTransaction)
			{
				var guidStringFile = Guid.NewGuid().ToString() + ".txt";
				var archiveFile = Path.Combine(ArchiveDirectory, guidStringFile);
				File.WriteAllBytes(archiveFile, image.Image);

				_ = indexBuilder.AppendLine("File: " + guidStringFile);
				foreach (var metaData in image.MetaData)
				{
					_ = indexBuilder.AppendLine(metaData.Key + ": " + metaData.Value);
				}

				archiveFiles.Add(archiveFile);
			}
			else
			{
				throw new InvalidOperationException("You must call Store within a transaction.");
			}
		}

		List<string> archiveFiles;

		public static string ArchiveDirectory { get; set; }

		StringBuilder indexBuilder;
		bool isInTransaction;

		#endregion

		#region IArchiveAction Members

		ITransactionManager ITransactionStarter.BeginTransactionWithManager()
		{
			isInTransaction = true;
			indexBuilder = new StringBuilder();
			archiveFiles = new List<string>();
			return new TransactionManager(this);
		}

		void IArchiveAction.Execute()
		{ }

		#endregion

		class TransactionManager : BaseTransactionManager<DummyArchiveStorage>
		{
			public TransactionManager(DummyArchiveStorage owner) : base(owner)
			{ }

			[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
			static readonly ReaderWriterLock locker = new ReaderWriterLock();
			protected override void Commit()
			{
				var indexFile = Path.Combine(ArchiveDirectory, "index.txt");

				try
				{
					locker.AcquireWriterLock(3000); // 3 seconds should be enough

					if (File.Exists(indexFile))
					{
						File.AppendAllText(indexFile, owner.indexBuilder.ToString());
					}
					else
					{
						File.WriteAllLines(indexFile, new string[] { owner.indexBuilder.ToString() });
					}
				}
				finally
				{
					locker.ReleaseWriterLock();
				}

				owner.indexBuilder = null;
				owner.archiveFiles = null;
				owner.isInTransaction = false;
			}

			protected override void Rollback()
			{
				foreach (var filepath in owner.archiveFiles)
				{
					if (File.Exists(filepath))
					{
						File.Delete(filepath);
					}
				}

				owner.indexBuilder = null;
				owner.archiveFiles = null;
				owner.isInTransaction = false;
			}
		}
	}
}
