using System;
using CargoWise.Types;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveWatermark
	{
		/// <summary>
		/// The date of the current furthest record that we have reached. Use as the foremost attribute to sort records by.
		/// </summary>
		ZDateTime WatermarkDate { get; }

		/// <summary>
		/// The natural key of the furthest record we have reached. Used to break ties between records with the same watermark date when sorting.
		/// </summary>
		string WatermarkNK { get;  }

		/// <summary>
		/// The primary key in the database of the furthest record we have reached.
		/// Used as a last-resort tiebreaker when sorting when all other watermark attributes are equal.
		/// </summary>
		Guid WatermarkPK { get; }
	}
}
