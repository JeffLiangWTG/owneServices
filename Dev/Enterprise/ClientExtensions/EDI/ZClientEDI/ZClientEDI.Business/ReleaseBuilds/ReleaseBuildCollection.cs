using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ZClientEDI.Business.Licencing;

namespace Enterprise.Client.EDI.ReleaseBuilds.Business
{
	[ModuleID("ReleaseBuild")]
	public sealed class ReleaseBuildCollection : BusinessObjectCollection<ReleaseBuild>
	{
		public ReleaseBuildCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ReleaseBuildCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}

		protected sealed override IFindBoxListProvider FindBoxListProvider
		{
			get { return new ReleaseBuildFindBoxListProvider(this); }
		}

		public sealed override void Load(ZQuery alternativeAdditionalFilter)
		{
			licenceHeaderCounts = null;
			base.Load(alternativeAdditionalFilter);
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name is ReleaseBuild.Schema.ExeVersion)
			{
				return new ComparerForSort<ReleaseBuild, VersionNumber>(r => r.VersionNumber, direction);
			}
			else
			{
				return base.GetComparerForSort(property, direction);
			}
		}

		#region Licence Header Count

		public int GetLicenceHeaderCount(ReleaseBuild build)
		{
			int result = 0;

			DataRow[] rows = LicenceHeaderCounts.Select(new ZQuery(ReleaseBuildSchema.PK, build.PK).LiteralTextADO);
			if (rows.Length == 1)
			{
				result = (int)rows[0][0];
			}

			return result;
		}

		DataTable LicenceHeaderCounts
		{
			get
			{
				if (licenceHeaderCounts == null)
				{
					string sqlText = string.Format(CultureInfo.CurrentCulture, @"
						SELECT COUNT(*), {0}
						FROM {1}, {2}, {3}
						WHERE {4} = {5}
						AND {6} = {0}
						GROUP BY {0}",
						ReleaseBuildSchema.Constants.PK,                                                    // 0
						LicenceHeaderSchema.Constants.TableName, LicenceDatabaseSchema.Constants.TableName, // 1, 2
						ReleaseBuildSchema.Constants.TableName,                                             // 3,
						LicenceHeaderSchema.Constants.LA_LD, LicenceDatabaseSchema.Constants.PK,            // 4, 5
						LicenceDatabaseSchema.Constants.LD_HL_CurrentRunningVersion);                       // 6

					licenceHeaderCounts = ZArchitecture.Core.Utilities.GetDataTableFromQuery(sqlText);
				}

				return licenceHeaderCounts;
			}
		}

		DataTable licenceHeaderCounts;

		#endregion

		#region class ReleaseBuildFindBoxListProvider

		public class ReleaseBuildFindBoxListProvider : FindBoxListProvider
		{
			public ReleaseBuildFindBoxListProvider(BusinessObjectCollection list)
				: base(list)
			{
			}

			public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
			{
				return (code, false);
			}

			protected override void AddCodeEqualsFilter(ZQuery query, string code)
			{
				VersionNumber versionNumber;
				if (VersionNumber.TryParse(code, out versionNumber))
				{
					query.AddToFilter(ReleaseBuildSchema.HL_MajorVersion, versionNumber.Major);
					query.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, versionNumber.Minor);
					query.AddToFilter(ReleaseBuildSchema.HL_Release, versionNumber.Release);
					query.AddToFilter(ReleaseBuildSchema.HL_Patch, versionNumber.Patch);
				}
				else
				{
					base.AddCodeEqualsFilter(query, code);
				}
			}

			protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
			{
				var result = Enumerable.Empty<BusinessObject>();
				if (!string.IsNullOrEmpty(code))
				{
					var query = new ZQuery();
					AddCodeEqualsFilter(query, code);

					result = List.Factory.Load<ReleaseBuild>(query);
				}

				return result;
			}
		}

		#endregion
	}
}

