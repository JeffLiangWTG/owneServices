using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class ChargeCodeRegistryItemWrapper<T> : StronglyTypedRegistryItem<T>
	{
		protected ChargeCodeRegistryItemWrapper(ChargeCodeRegistryItemImpl inner) : base(inner)
		{
		}

		public string DefaultChargeCode
		{
			get { return ((ChargeCodeRegistryItemImpl)Inner).DefaultChargeCode; }
		}

		#region class ChargeCodeRegistryItemImpl

		protected abstract class ChargeCodeRegistryItemImpl : RegistryItemImpl
		{
			public ChargeCodeRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, string defaultChargeCode)
				: base(name, category, caption, hint, dataType, RegistryStorageFlags.Company)
			{
				DefaultChargeCode = defaultChargeCode;
			}

			public ChargeCodeRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, string defaultChargeCode)
				: base(name, category, caption, hint, dataType, storage)
			{
				DefaultChargeCode = defaultChargeCode;
			}

			public ChargeCodeRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, string defaultChargeCode)
				: base(name, category, caption, hint, dataType, storage, options)
			{
				DefaultChargeCode = defaultChargeCode;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
			protected Guid GetChargeCodePK(string code, Guid companyPK)
			{
				var result = Guid.Empty;

				if (!string.IsNullOrEmpty(code))
				{
					var key = (code ?? "") + companyPK.ToString();

					if (!CachedChargeCodePKs.TryGetValue(key, out result))
					{
						using (var command = Db.Connection.Command(string.Format("SELECT {0} FROM {1} WHERE {2} = @chargeCode and {3} = @companyPK", AccChargeCodeSchema.PK.Name, AccChargeCodeSchema.Constants.TableName, AccChargeCodeSchema.AC_Code.Name, AccChargeCodeSchema.AC_GC.Name)))
						{
							command.AddParameterBasedOnDbColumn("@chargeCode", code, AccChargeCodeSchema.AC_Code);
							command.AddParameterBasedOnDbColumn("@companyPK", companyPK, AccChargeCodeSchema.AC_GC);
							var objectFromDb = command.ExecuteScalar();
							result = objectFromDb != null ? (Guid)objectFromDb : Guid.Empty;
							CachedChargeCodePKs.Add(key, result);
						}
					}
				}

				return result;
			}

			Dictionary<string, Guid> CachedChargeCodePKs
			{
				get { return cachedChargeCodePKs ?? (cachedChargeCodePKs = new Dictionary<string, Guid>()); }
			}

			Dictionary<string, Guid> cachedChargeCodePKs;

			public readonly string DefaultChargeCode;
		}

		#endregion
	}
}
