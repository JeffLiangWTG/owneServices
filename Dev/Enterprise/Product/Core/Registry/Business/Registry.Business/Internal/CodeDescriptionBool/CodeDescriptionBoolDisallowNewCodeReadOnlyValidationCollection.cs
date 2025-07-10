using System;
using System.Xml.Serialization;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection : CodeDescriptionBoolDisallowNewCodeReadOnlyCollection
	{
		public CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection()
			: base()
		{
		}

		public CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list)
		{
		}

		protected override bool AllowNewCore => false;

		public new CodeDescriptionBoolDisallowNewCodeReadOnlyValidation AddNew() => (CodeDescriptionBoolDisallowNewCodeReadOnlyValidation)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CodeDescriptionBoolDisallowNewCodeReadOnlyValidation();

		public new CodeDescriptionBoolDisallowNewCodeReadOnlyValidation this[int i] => (CodeDescriptionBoolDisallowNewCodeReadOnlyValidation)base[i];

		protected override CodeDescriptionBoolCollection GetNewCollection() => new CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection(CurrentFallbackLevel);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolDisallowNewCodeReadOnlyValidation : CodeDescriptionBoolDisallowNewCodeReadOnly
	{
		#region Bool

		public override ZBool Bool
		{
			get => base.Bool;
			set
			{
				base.Bool = value;
				if (!IsValidationSuspended)
				{
					ValidateBool();
				}
			}
		}

		void ValidateBool()
		{
			BoolInfo.ClearAllNotifications();
			if (CheckCodeInJobBillingExchangeRateConfiguration())
			{
				var errorMessage = ResString.GetMultilingualString("bde27923-a27b-420c-9acb-dcfce8487198", "Job Invoicing/Job Billing Exchange Rate Configuration registry. Please modify Job Billing Exchange Rate Configuration, save it and then disable '{0}'", CodeInfo.Value);
				BoolInfo.AddError(ResString.GetMultilingualString("179b96e3-f2ac-4e5d-8672-9890680a0d0a", "Currency Code '{0}' is used in {1}", CodeInfo.Value, errorMessage));
			}
			if (!CheckCodeInCurrency())
			{
				return;
			}

			{
				var errorMessage = ResString.GetMultilingualString("77569df5-ee97-4686-800c-a67fbb30f239", "Currency form");
				BoolInfo.AddError(ResString.GetMultilingualString("cf97efa7-7ab1-49d7-9d84-fb9b2bb20b2d", "Currency Code '{0}' is used in {1}", CodeInfo.Value, errorMessage));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CW1161:Do not use constant string literals - use resource strings instead.", Justification = "Database query")]
		bool CheckCodeInJobBillingExchangeRateConfiguration()
		{
			if (Bool)
			{
				return false;
			}

			var result = false;
			using (var command = Db.Connection.Command("SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name LIKE @name"))
			{
				command.AddParameterBasedOnDbColumn("@name", "JobBillingExchangeRateConfiguration", StmDataSchema.SD_Name);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var dbValue = reader[StmDataSchema.Constants.SD_BinaryValue];

						if (dbValue == DBNull.Value)
						{
							continue;
						}

						var config = System.Text.Encoding.ASCII.GetString((byte[])dbValue).Replace("\0", "");
						result = config.Contains(CodeInfo.Value.ToString()) && !Bool;
						if (result)
						{
							break;
						}
					}
				}
			}
			
			return result;
		}

		bool CheckCodeInCurrency()
		{
			if (Bool)
			{
				return false;
			}

			var result = false;
			using (var command = Db.Connection.Command("SELECT DISTINCT RE_ExRateType FROM dbo.ZZRefExchangeRate"))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var dbValue = reader[ZZRefExchangeRateSchema.Constants.RE_ExRateType];
					if (dbValue == DBNull.Value)
					{
						continue;
					}

					result = dbValue.ToString().Contains(CodeInfo.Value.ToString());
					if (result)
					{
						break;
					}
				}
			}
			return result;
		}

		#endregion

		#region Clone

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			if (CallBaseCopy)
			{
				base.CopyValuesToClone(clone);
			}

			var codeDescriptionBoolDisallowNewCodeReadOnlyValid = (CodeDescriptionBoolDisallowNewCodeReadOnlyValidation)clone;
			codeDescriptionBoolDisallowNewCodeReadOnlyValid.IsCodeReadOnly = IsCodeReadOnly;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CodeDescriptionBoolDisallowNewCodeReadOnlyValidation();

		#endregion
	}
}
