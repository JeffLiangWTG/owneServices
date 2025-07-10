using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class StmData : AutoStmData
	{
		public StmData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader

#pragma warning disable IDE0001 // Simplify Names
		public new class Loader : AutoStmData.Loader
#pragma warning restore IDE0001 // Simplify Names
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public StmData[] Load(string[] names, ZGuid owner, ZGuid departmentGuid)
			{
				return Factory.Load<StmData>(GetQuery(names, owner, departmentGuid));
			}

			public StmData LoadTop1(ZString name, ZGuid owner, ZGuid departmentGuid)
			{
				return Factory.LoadTop1<StmData>(GetQuery(name, owner, departmentGuid));
			}

			ZQuery GetQuery(object names, ZGuid owner, ZGuid departmentGuid)
			{
				ZQuery result = new ZQuery(StmDataSchema.SD_Name, names);
				result.AddToFilter(StmDataSchema.SD_Owner, owner);

				object departmentGUIDToSearchBy = departmentGuid.IsEmpty ? DBNull.Value : departmentGuid;
				result.AddToFilter(StmDataSchema.SD_DepartmentGuid, departmentGUIDToSearchBy);

				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(StmData);
			}
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion
	}
}
