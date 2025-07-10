using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Do not access this - use MarkAsNeedingValidation() and LightValidationIsValid instead. 
	/// </summary>
	public interface ILightValidationInternals
	{
		/// <summary>
		/// Do not access this - use MarkAsNeedingValidation() and LightValidationIsValid instead.
		/// </summary>
		SchemaBoolColumn IsValidSchemaColumn { get; }
		ZBool IsValid { get; set; }
		bool IsValidHasChanges { get; }
	}
}
