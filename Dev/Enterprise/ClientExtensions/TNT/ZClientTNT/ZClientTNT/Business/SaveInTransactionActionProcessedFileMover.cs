using System.IO;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Client.TNT
{
	public class SaveInTransactionActionProcessedFileMover : SaveInTransactionAction
	{
		public SaveInTransactionActionProcessedFileMover(FileInfo fileToMove, ZString moveToDirectory)
		{
			this.FileToMove = fileToMove;
			FileMover = new ProcessedFileMover(moveToDirectory);
		}

		protected override IChangedTableNames SaveInTransaction()
		{
			FileMover.Move(FileToMove);
			return ChangedTableNames.Empty;
		}
		protected internal void SaveInTransactionInternal() => SaveInTransaction();

		internal
		readonly FileInfo FileToMove;

		internal
		readonly ProcessedFileMover FileMover;

		#region Implementation

		protected override ITransactionManager BeginTransactionWithManager()
		{
			return new StubTransactionManager();
		}

		protected override bool IsInTransaction
		{
			get { return false; }
		}

		#endregion

	}
}
