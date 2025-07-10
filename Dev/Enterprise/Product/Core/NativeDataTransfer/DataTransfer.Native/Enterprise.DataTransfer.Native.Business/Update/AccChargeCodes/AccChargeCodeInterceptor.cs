using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Utils.Models;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.AccChargeCodes
{
	class AccChargeCodeInterceptor : BaseInterceptor
	{
		public AccChargeCodeInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
			factory = setting.Context.ObjectFactory;
		}
		readonly BusinessObjectFactory factory;

		public override void Invoke(IEntitySet entitySet)
		{
			var root = entitySet.Root;
			UpdateGlbCompany(root);
			Function(entitySet);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void UpdateGlbCompany(IEntity root)
		{
			foreach (var entity in root.Relatives())
			{
				if (entity.TableName == "AccChargeCode" && entity.Definition.IsExternal && entity.HasProperty("Code"))
				{
					var companyEntity = entity.Parents.FirstOrDefault(e => e.EntityName == "GlbCompany");
					if (companyEntity == null)
					{
						var companyDefinition = entity.Definition.Parents.First(e => e.EntityName == "GlbCompany");
						companyEntity = new Entity(companyDefinition, sessionServices);
						((Entity)entity).ParentCollection.Add(companyEntity);
					}

					ZGuid companyPK = ZGuid.Empty;
					if (companyEntity.HasProperty("Code"))
					{
						var companyCode = (companyEntity["Code"] ?? "").ToString();
						if (!string.IsNullOrEmpty(companyCode))
						{
							var company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);
							if (company != null)
							{
								companyPK = company.PK;
							}
						}
					}

					if (companyPK.IsEmpty)
					{
						companyPK = companyEntity.InternalPK;
					}

					if (!companyPK.IsEmpty)
					{
						var query = new ZQuery();
						query.AddToFilter(AccChargeCodeSchema.AC_Code, entity["Code"]);
						query.AddToFilter(AccChargeCodeSchema.AC_GC, companyPK);
						var chargeCodes = factory.Load<AccChargeCode>(query);
						if (chargeCodes.Length != 0)
						{
							return;
						}
					}

					companyEntity["PK"] = Env.CurrentCompany.PK;
					companyEntity.InternalPK = Env.CurrentCompany.PK;
				}
			}
		}
	}

	class AccChargeCodeSetting : BaseInterceptorSetting
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "IEnumerable string")]
		public override IEnumerable<string> EnableList
		{
			get { return new[] { "Order" }; }
		}

		public override IEnumerable<string> DisableList
		{
			get { return new List<string>(); }
		}
	}
}
