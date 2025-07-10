using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IAuditDetails
	{
		/// <summary>
		/// System Create Time in local time
		/// </summary>
		ZDateTime SystemCreateTimeUtc { get; }

		ZString SystemCreateUser { get; }

		/// <summary>
		/// System Last Edit Time in local time
		/// </summary>
		ZDateTime SystemLastEditTimeUtc { get; }

		ZString SystemLastEditUser { get; }
	}
}
