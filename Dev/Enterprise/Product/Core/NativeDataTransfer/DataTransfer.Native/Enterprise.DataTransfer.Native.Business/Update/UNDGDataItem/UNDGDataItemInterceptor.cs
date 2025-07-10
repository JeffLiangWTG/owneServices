using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	class UNDGDataItemInterceptor : BaseInterceptor
	{
		public UNDGDataItemInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices) : base(setting, sessionServices)
		{
			factory = setting.Context.ObjectFactory;
		}

		readonly BusinessObjectFactory factory;

		public override void Invoke(IEntitySet entitySet)
		{
			var root = entitySet.Root;
			UpdateUNDGDataItem(root);
			Function(entitySet);
		}

		void UpdateUNDGDataItem(IEntity root)
		{
			var dataItems = root.ChildrenCollection.Where(x => x.EntityName == "UNDGDataItem");
			foreach (var di in dataItems)
			{
				var subsEntity = di.Parents.FirstOrDefault(x => x.EntityName == "UNDGSubstance");
				if (subsEntity == null)
				{
					continue;
				}

				subsEntity.Action = EntityAction.IGNORE;

				var subsPivotEntityExists = di.ChildrenCollection.Any(x => x.EntityName == UNDGSubstancePivotSchema.Constants.TableName);
				if (!subsPivotEntityExists && FindSubstanceInDb(subsEntity) is UNDGSubstance substanceInDb)
				{
					var pivotEntity = new Entity(di.Definition.Children.FirstOrDefault(d => d.EntityName == UNDGSubstancePivotSchema.Constants.TableName), sessionServices);

					pivotEntity.Action = EntityAction.MERGE;
					pivotEntity.Parent = di;

					pivotEntity["IsDefault"] = true;
					pivotEntity["ParentTableCode"] = UNDGDataItemSchema.Constants.Prefix;

					pivotEntity["UNNO"] = substanceInDb.DG_UNNO;
					pivotEntity["Variant"] = substanceInDb.DG_Variant;
					pivotEntity["Standard"] = substanceInDb.DG_Standard;

					subsEntity.InternalPK = substanceInDb.PK.ToGuid();

					di.ChildrenCollection.Add(pivotEntity);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		UNDGSubstance FindSubstanceInDb(IEntity substanceEntity)
		{
			var pk = substanceEntity.GetPropertyOrBlankString("PK");
			var code = substanceEntity.GetPropertyOrBlankString("Code");
			var unno = substanceEntity.GetPropertyOrBlankString("UNNO");
			var variant = substanceEntity.GetPropertyOrBlankString("Variant");

			var standard = substanceEntity.GetPropertyOrBlankString("Standard");
			standard = string.IsNullOrEmpty(standard) ? UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO : standard;

			var uniqueRecordId = substanceEntity.GetPropertyOrBlankString("UniqueRecordId");

			UNDGSubstance realSub = null;

			if (!string.IsNullOrEmpty(pk))
			{
				realSub = factory.Load<UNDGSubstance>(new ZGuid(pk));
			}

			if (realSub == null)
			{
				var query = new ZQuery(UNDGSubstanceSchema.DG_Standard, standard);
				if (!string.IsNullOrEmpty(code))
				{
					query.AddToFilter(UNDGSubstanceSchema.DG_Code, code);
				}
				else if (!string.IsNullOrEmpty(unno))
				{
					query.AddToFilter(UNDGSubstanceSchema.DG_UNNO, unno);
					query.AddToFilter(UNDGSubstanceSchema.DG_Variant, variant);
				}
				else if (string.IsNullOrEmpty(code) && standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA)
				{
					query.AddToFilter(UNDGSubstanceSchema.DG_UniqueRecordId, uniqueRecordId);
				}

				realSub = factory.LoadTop1<UNDGSubstance>(query);
			}

			return realSub;
		}
	}

	class UNDGDataItemSetting : BaseInterceptorSetting
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "IEnumerable string")]
		public override IEnumerable<string> EnableList
		{
			get { return new[] { "Product" }; }
		}

		public override IEnumerable<string> DisableList
		{
			get { return new List<string>(); }
		}
	}
}
