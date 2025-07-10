using System;

namespace CargoWise.Data;

public record BulkCopySetting(int Threshold, int BatchSize, bool CheckRowsShouldBePersistent, SqlBulkCopyOptions Options)
{
	public BulkCopySetting(
		int threshold,
		int batchSize,
		bool checkRowsShouldBePersistent = true,
		bool keepIdentity = false,
		bool checkConstraints = false,
		bool tableLock = false,
		bool keepNulls = false,
		bool fireTriggers = false,
		bool useInternalTransaction = false,
		bool allowEncryptedValueModifications = false,
		EventHandler onBulkCopyingHandler = null) : this(threshold, batchSize, checkRowsShouldBePersistent, SqlBulkCopyOptions.Default)
	{
		if (keepIdentity)
		{
			Options |= SqlBulkCopyOptions.KeepIdentity;
		}

		if (checkConstraints)
		{
			Options |= SqlBulkCopyOptions.CheckConstraints;
		}

		if (tableLock)
		{
			Options |= SqlBulkCopyOptions.TableLock;
		}

		if (keepNulls)
		{
			Options |= SqlBulkCopyOptions.KeepNulls;
		}

		if (fireTriggers)
		{
			Options |= SqlBulkCopyOptions.FireTriggers;
		}

		if (useInternalTransaction)
		{
			Options |= SqlBulkCopyOptions.UseInternalTransaction;
		}

		if (allowEncryptedValueModifications)
		{
			Options |= SqlBulkCopyOptions.AllowEncryptedValueModifications;
		}

		if (onBulkCopyingHandler != null)
		{
			OnBulkCopying += onBulkCopyingHandler;
		}
	}

	public event EventHandler OnBulkCopying;

	public void BulkCopying()
	{
		OnBulkCopying?.Invoke(this, EventArgs.Empty);
	}
}
