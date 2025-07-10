using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.OrgPatternMatchRegen
{
	class WrapperFactory
	{
		internal WrapperFactory(RowFactory factory)
		{
			this.factory = Argument.NotNull(factory, "RowFactory factory");
		}

		readonly RowFactory factory;

		internal PortInfo GetPortInfo(ZString unloco)
		{
			PortInfo result;
			if (!portInfoCache.TryGetValue(unloco, out result))
			{
				if (unloco.IsEmpty)
				{
					portInfoCache[unloco] = result = new PortInfo(ZString.Empty, ZString.Empty);
				}
				else
				{
					var row = factory.LoadFromNaturalKey(RefUNLOCOSchema.Constants.TableName, RefUNLOCOSchema.RL_Code, unloco, false);
					if (row == null)
					{
						portInfoCache[unloco] = result = GetPortInfo(ZString.Empty);
					}
					else
					{
						var portName = new ZString(row[RefUNLOCOSchema.RL_PortName.Name]);
						var countryCode = new ZString(row[RefUNLOCOSchema.RL_RN_NKCountryCode.Name]);
						var countryRow = factory.LoadFromNaturalKey(RefCountrySchema.Constants.TableName, RefCountrySchema.RN_Code, countryCode, false);
						var countryName = countryRow == null ? countryCode : new ZString(countryRow[RefCountrySchema.RN_Desc.Name]);
						portInfoCache[unloco] = result = new PortInfo(countryName, portName);
					}
				}
			}

			return result;
		}

		readonly Dictionary<ZString, PortInfo> portInfoCache = new Dictionary<ZString, PortInfo>();

		internal T Load<T>(ZGuid pk) where T : Wrapper, new()
		{
			var row = factory.LoadFromPK(TableNameAttribute.GetTableName<T>(), pk);
			return row == null ? null : GetNew<T>(row);
		}

		internal IEnumerable<T> Load<T>(ZQuery query) where T : Wrapper, new()
		{
			var rows = factory.Load(TableNameAttribute.GetTableName<T>(), query);
			return rows.Select(row => GetNew<T>(row));
		}

		internal T New<T>() where T : Wrapper, new()
		{
			var row = factory.New(TableNameAttribute.GetTableName<T>());
			var result = GetNew<T>(row);
			result.PK = Guid.NewGuid();
			row.Table.Rows.Add(row);
			return result;
		}

		T GetNew<T>(DataRow row) where T : Wrapper, new()
		{
			var result = new T();
			result.Init(row, this);
			return result;
		}
	}
}
