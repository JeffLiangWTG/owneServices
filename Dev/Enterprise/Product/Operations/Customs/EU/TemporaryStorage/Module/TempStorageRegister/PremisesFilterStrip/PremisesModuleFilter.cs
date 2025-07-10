using System.Collections;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;
using CusTempStorageRegPremises = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises;

namespace Enterprise.Customs.EU.TemporaryStorage.Module
{
	public delegate ZQuery GetPremisesQuery(ZQuery premisesFilter);

	public class PremisesModuleFilter : ModuleTextFilter
	{
		#region Construction

		public PremisesModuleFilter(ZString description, GetPremisesQuery queryDelegate = null, IList list = null)
			: base(description, EmptyQuery, list ?? PremisesTypes)
		{
			premisesQueryDelegate = queryDelegate ?? (q => q);
		}

		#endregion

		#region Comparison Operator

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				var operators = new List<string>
				{
					string.Empty,
					ComparisonConstants.Exact,
					ComparisonConstants.NotEqual,
					ComparisonConstants.IsBlank,
					ComparisonConstants.IsNotBlank,
					ComparisonConstants.StartsWith,
					ComparisonConstants.EndsWith
				};

				return operators;
			}
		}

		protected override SQLComparisonOperator DefaultSqlComparisonOperator
		{
			get { return SQLComparisonOperator.Equal; }
		}

		public override bool HasComparisonOperator => true;

		#endregion

		#region Properties

		#region PremisesType

		[BusinessObjectTestExclude]
		[List(nameof(PremisesTypesList))]
		public ZString PremisesType
		{
			get { return premisesType; }
			set
			{
				premisesType = value;
				PremisesTypeInfo.RefreshBinding();
			}
		}
		ZString premisesType;

		public ZPropertyInfo PremisesTypeInfo
		{
			get { return GetZPropertyInfo(nameof(PremisesType)); }
		}

		public CusTempStorageRegPremisesTypeList PremisesTypesList
		{
			get
			{
				if (premisesTypeList == null)
				{
					premisesTypeList = new CusTempStorageRegPremisesTypeList();
					premisesTypeList.Sort();
				}

				return premisesTypeList;
			}
		}
		CusTempStorageRegPremisesTypeList premisesTypeList;

		static CodeDescriptionPairList PremisesTypes
		{
			get { return new CusTempStorageRegPremisesTypeList(); }
		}

		#endregion

		#region PremisesCode

		[BusinessObjectTestExclude]
		public ZString PremisesCode
		{
			get
			{
				if (ComparisonOperator == ComparisonConstants.IsBlank || ComparisonOperator == ComparisonConstants.IsNotBlank)
				{
					return ZString.Empty;
				}
				return premisesCode;
			}
			set
			{
				premisesCode = value;
				PremisesCodeInfo.RefreshBinding();
			}
		}

		protected bool PremisesCode_ReadOnly => ShouldComparisonOperatorCauseReadOnly();

		ZString premisesCode;

		public ZPropertyInfo PremisesCodeInfo
		{
			get { return GetZPropertyInfo(nameof(PremisesCode)); }
		}

		#endregion

		#region PremisesLocation

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(PremisesLookupsImplementer.CustomsLocationList))]
		public ZString PremisesLocation
		{
			get { return premisesLocation; }
			set
			{
				premisesLocation = value;
				PremisesLocationInfo.RefreshBinding();
			}
		}
		ZString premisesLocation;

		public ZPropertyInfo PremisesLocationInfo
		{
			get { return GetZPropertyInfo(nameof(PremisesLocation)); }
		}

		#endregion

		#endregion

		#region Query

		static ZQuery EmptyQuery(ZString value)
		{
			return new ZQuery();
		}

		protected override ZQuery GetQuery()
		{
			return IsEmpty ? new ZQuery() : premisesQueryDelegate(GetPremisesFilter());
		}

		ZQuery GetPremisesFilter()
		{
			var query = new ZDBOnlyQuery(typeof(CusTempStorageRegHeader));
			var premisesSubQuery = new ZDBOnlySubQuery(typeof(CusTempStorageRegPremises), CusTempStorageRegPremisesSchema.PK);

			if (!PremisesType.IsEmpty)
			{
				premisesSubQuery.AddToFilter(JoinCondition.And, CusTempStorageRegPremisesSchema.SRP_Type, PremisesType);
			}

			if (!PremisesCode.IsEmpty
				|| ComparisonOperator == ComparisonConstants.IsBlank
				|| ComparisonOperator == ComparisonConstants.IsNotBlank)
			{
				premisesSubQuery.AddToFilter(JoinCondition.And, CusTempStorageRegPremisesSchema.SRP_Code, SqlComparisonOperator, PremisesCode);
			}

			if (!PremisesLocation.IsEmpty)
			{
				premisesSubQuery.AddToFilter(JoinCondition.And, CusTempStorageRegPremisesSchema.SRP_CustomsLocation, PremisesLocation);
			}

			query.AddSubQuery(CusTempStorageRegHeaderSchema.SRH_SRP_Premises, premisesSubQuery, JoinCondition.And);

			return query;
		}

		protected readonly GetPremisesQuery premisesQueryDelegate;

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString("PremisesType", PremisesType);
			writer.WriteElementString("PremisesCode", PremisesCode);
			writer.WriteElementString("PremisesLocation", PremisesLocation);
		}

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == "PremisesType")
			{
				PremisesType = reader.ReadElementString("PremisesType");
			}

			if (reader.Name == "PremisesCode")
			{
				PremisesCode = reader.ReadElementString("PremisesCode");
			}

			if (reader.Name == "PremisesLocation")
			{
				PremisesLocation = reader.ReadElementString("PremisesLocation");
			}
		}

		#endregion

		#region Implementation

		new BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}

				return factory;
			}
		}
		BusinessObjectFactory factory;

		protected override bool IsEmptyCore => base.IsEmptyCore && PremisesType.IsEmpty && PremisesLocation.IsEmpty
			&& PremisesCode.IsEmpty && ComparisonOperator != ComparisonConstants.IsBlank && ComparisonOperator != ComparisonConstants.IsNotBlank;

		protected override void ClearCore()
		{
			base.ClearCore();
			PremisesType = ZString.Empty;
			PremisesCode = ZString.Empty;
			PremisesLocation = ZString.Empty;
		}

		#endregion

		#region Lookups

		public PremisesLookupsImplementer Lookups
		{
			get { return PremisesLookupsImplementer.Get(Factory); }
		}

		#endregion
	}
}
