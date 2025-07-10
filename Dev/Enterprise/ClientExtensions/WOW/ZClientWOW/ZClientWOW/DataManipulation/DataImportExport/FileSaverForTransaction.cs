using System;
using System.IO;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.Client.Wow
{
	public class UniqueFileNameSaver : SaveInTransactionAction, IDisposable
	{
		public UniqueFileNameSaver(StreamReader reader, string fileName)
		{
			this.reader = reader;
			this.FileName = fileName;
			this.FileNameForTransactionFail = fileName + "___failed";
			TempFileName = Temp.GetTempFileName();
			SaveReader(TempFileName);
		}

		public void SaveFile()
		{
			SaveFile(FileName);
		}

		public void SaveFile(string fileName)
		{
			string fileNameForSave = fileName;
			int fileIndex = 0;
			do
			{
				if (fileIndex == 0)
				{
					fileNameForSave = fileName;
				}
				else
				{
					fileNameForSave = fileName + "_" + fileIndex;
				}
				fileIndex++;
			}
			while (File.Exists(fileNameForSave));

			File.Move(TempFileName, fileNameForSave);
		}

		void SaveReader(string fileNameForSave)
		{
			reader.DiscardBufferedData();
			reader.BaseStream.Position = 0;

			using (FileStream fileStr = new FileStream(fileNameForSave, FileMode.Create))
			{
				StreamWriter writer = new StreamWriter(fileStr);
				writer.AddStream(reader);
				writer.Flush();
			}

			reader.Close();
		}

		protected override IChangedTableNames SaveInTransaction()
		{
			saveFailed = true;
			SaveFile(FileName);
			saveFailed = false;
			return ChangedTableNames.Empty;
		}

		protected override ITransactionManager BeginTransactionWithManager()
		{
			return new TransactionManager(this);
		}

		bool saveFailed;

		protected override bool IsInTransaction
		{
			get { return false; }
		}

		protected readonly StreamReader reader;
		protected readonly string TempFileName;
		protected readonly string FileName;
		protected readonly string FileNameForTransactionFail;

		public void Dispose()
		{
			try
			{
				File.Delete(TempFileName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}
		}

		class TransactionManager : BaseTransactionManager<UniqueFileNameSaver>
		{
			public TransactionManager(UniqueFileNameSaver owner, Action rollbackAction = null) : base(owner, rollbackAction)
			{
			}

			protected override void Commit()
			{
			}

			protected override void Rollback()
			{
				if (!owner.saveFailed)
				{
					owner.SaveFile(owner.FileNameForTransactionFail);
				}
			}
		}
	}
}
