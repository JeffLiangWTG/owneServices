using CargoWise.Types;

namespace Enterprise.ZArchitecture.Core
{
	/// <summary>
	/// Represents a result from updating a phone numeber in a database row.
	/// </summary>
	public class PhoneNumberUpdateResult
	{
		#region Constructor

		PhoneNumberUpdateResult(ZGuid rowPk, ZBool isSuccessful, ZString originalNumber, ZString newNumber)
		{
			RowPk = rowPk;
			IsSuccessful = isSuccessful;
			OriginalNumber = originalNumber;
			NewNumber = newNumber;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the primary key of the row.
		/// </summary>
		public ZGuid RowPk { get; private set; }

		/// <summary>
		/// Gets whether the update succeeds.
		/// </summary>
		public ZBool IsSuccessful { get; private set; }

		/// <summary>
		/// Gets the original phone number before the update.
		/// </summary>
		public ZString OriginalNumber { get; private set; }

		/// <summary>
		/// Gets the new phone number after the update.
		/// </summary>
		public ZString NewNumber { get; private set; }

		#endregion

		#region Factory Methods

		public static PhoneNumberUpdateResult CreateSuccess(ZGuid rowPk, ZString originalNumber, ZString newNumber)
		{
			return new PhoneNumberUpdateResult(rowPk, ZBool.True, originalNumber, newNumber);
		}

		public static PhoneNumberUpdateResult CreateFailure(ZGuid rowPk, ZString originalNumber)
		{
			return new PhoneNumberUpdateResult(rowPk, ZBool.False, originalNumber, ZString.Empty);
		}

		#endregion
	}
}
