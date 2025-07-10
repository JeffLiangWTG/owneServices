using System;

using CargoWise.Data;

using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class AccTaxRateRegistryItemWrapper<T> : StronglyTypedRegistryItem<T>
	{
		protected AccTaxRateRegistryItemWrapper(AccTaxRateRegistryItemImpl inner) : base(inner)
		{
		}

		public string DefaultTaxRate
		{
			get { return ((AccTaxRateRegistryItemImpl)Inner).DefaultTaxRate; }
		}

		#region class AccTaxRateRegistryItemImpl

		protected abstract class AccTaxRateRegistryItemImpl : RegistryItemImpl
		{
			public AccTaxRateRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, string defaultTaxRate)
				: base(name, category, caption, hint, dataType, RegistryStorageFlags.Company)
			{
				DefaultTaxRate = defaultTaxRate;
			}

			public AccTaxRateRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, string defaultTaxRate)
				: base(name, category, caption, hint, dataType, storage)
			{
				DefaultTaxRate = defaultTaxRate;
			}

			public AccTaxRateRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, string defaultTaxRate)
				: base(name, category, caption, hint, dataType, storage, options)
			{
				DefaultTaxRate = defaultTaxRate;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CW1161:Do not use constant string literals - use resource strings instead.", Justification = "Database query")]
			protected Guid GetTaxRatePK(string code, Guid companyPK)
			{
				object country = Db.Connection.ExecuteScalar(
					"SELECT " + GlbCompanySchema.GC_RN_NKCountryCode.Name +
					" FROM " + GlbCompanySchema.Constants.SqlSchemaName + "." + GlbCompanySchema.Constants.TableName +
					" WHERE " + GlbCompanySchema.PK.Name + " = @pk",
					cmd => cmd.AddParameterBasedOnDbColumn("@pk", companyPK, GlbCompanySchema.PK));
				string countryCode = country is string ? (string)country : string.Empty;

				if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(countryCode))
				{
					object result = Db.Connection.ExecuteScalar(
						"SELECT " + AccTaxRateSchema.PK.Name +
						" FROM " + AccTaxRateSchema.Constants.SqlSchemaName + "." + AccTaxRateSchema.Constants.TableName +
						" WHERE " + AccTaxRateSchema.AT_Code.Name + " = @code AND " + AccTaxRateSchema.AT_RN_NKCountry.Name + " = @country",
						cmd =>
						{
							cmd.AddParameterBasedOnDbColumn("@code", code, AccTaxRateSchema.AT_Code);
							cmd.AddParameterBasedOnDbColumn("@country", countryCode, AccTaxRateSchema.AT_RN_NKCountry);
						});
					return result != null ? (Guid)result : Guid.Empty;
				}
				return Guid.Empty;
			}

			public readonly string DefaultTaxRate;
		}

		#endregion
	}
}
