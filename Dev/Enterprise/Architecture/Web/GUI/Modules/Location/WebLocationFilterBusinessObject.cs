using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Modules
{
	#region Auto

	public abstract class AutoLocationFilterBusinessObject : FilterBusinessObject
	{
		#region Schema

		public abstract class Schema
		{
			public const string TableName = "LocationFilterBusinessObject";
			public const string PK = "PK";

			public const string Code = "Code";
			public const string Description = "Description";
			public const string IsCountry = "IsCountry";
			public const string IsPort = "IsPort";
			public const string IsRegion = "IsRegion";
		}

		#endregion

		protected AutoLocationFilterBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region PK

		public override string PK_ColumnName
		{
			get { return Schema.PK; }
		}

		#endregion

		#region Properties

		#region Code

		public virtual ZString Code
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(CodeInfo).ToString().TrimEnd(' ')); }
			set
			{
				CheckMaximumLength(CodeInfo, value);
				SetPropertyValue(CodeInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateCode();
				}
			}
		}

		public virtual void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();
		}

		public ZPropertyInfo CodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Code); }
		}

		#endregion

		#region Description

		public virtual ZString Description
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(DescriptionInfo).ToString().TrimEnd(' ')); }
			set
			{
				CheckMaximumLength(DescriptionInfo, value);
				SetPropertyValue(DescriptionInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateDescription();
				}
			}
		}

		public virtual void ValidateDescription()
		{
			DescriptionInfo.ClearAllNotifications();
		}

		public ZPropertyInfo DescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Description); }
		}

		#endregion

		#region IsCountry

		public virtual ZBool IsCountry
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(IsCountryInfo)); }
			set
			{
				SetPropertyValue(IsCountryInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateIsCountry();
				}
			}
		}

		public virtual void ValidateIsCountry()
		{
			IsCountryInfo.ClearAllNotifications();
		}

		public ZPropertyInfo IsCountryInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IsCountry); }
		}

		#endregion

		#region IsPort

		public virtual ZBool IsPort
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(IsPortInfo)); }
			set
			{
				SetPropertyValue(IsPortInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateIsPort();
				}
			}
		}

		public virtual void ValidateIsPort()
		{
			IsPortInfo.ClearAllNotifications();
		}

		public ZPropertyInfo IsPortInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IsPort); }
		}

		#endregion

		#region IsRegion

		public virtual ZBool IsRegion
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(IsRegionInfo)); }
			set
			{
				SetPropertyValue(IsRegionInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateIsRegion();
				}
			}
		}

		public virtual void ValidateIsRegion()
		{
			IsRegionInfo.ClearAllNotifications();
		}

		public ZPropertyInfo IsRegionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IsRegion); }
		}

		#endregion

		#endregion

		#region BusinessObject Overrides

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			((IBusinessObjectInternals)this).Row[Schema.Code] = "";
			((IBusinessObjectInternals)this).Row[Schema.Description] = "";
			((IBusinessObjectInternals)this).Row[Schema.IsCountry] = false;
			((IBusinessObjectInternals)this).Row[Schema.IsPort] = false;
			((IBusinessObjectInternals)this).Row[Schema.IsRegion] = false;
		}

		#endregion

		#region Run Pre-Save Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateCode();
			ValidateDescription();
			ValidateIsCountry();
			ValidateIsPort();
			ValidateIsRegion();

			base.RunPreSaveValidationCore();
		}

		#endregion

		#endregion

		#region Filter Methods

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				query.AddToFilter(LocationTypeGroupBoxFilter);

				return query;
			}
		}

		protected virtual ZQuery LocationTypeGroupBoxFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				return query;
			}
		}

		#endregion
	}

	#endregion

	public enum LocationType
	{
		Port,
		Zone,
		Country
	}

	public class WebLocationFilterBusinessObject : AutoLocationFilterBusinessObject
	{
		public WebLocationFilterBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Overrides

		#region Filter Override

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = new ZQuery();
				switch (LocationType)
				{
					case LocationType.Port:
						query.AddToFilter(JoinCondition.And, RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, Code.SubstringSafe(0, RefUNLOCOSchema.RL_Code.MaxLength));
						query.AddToFilter(JoinCondition.And, RefUNLOCOSchema.RL_PortName, SQLComparisonOperator.Contains, Description.SubstringSafe(0, RefUNLOCOSchema.RL_PortName.MaxLength));
						break;
					case LocationType.Zone:
						query.AddToFilter(JoinCondition.And, RefZoneHeaderSchema.FZ_Code, SQLComparisonOperator.StartsWith, Code.SubstringSafe(0, RefZoneHeaderSchema.FZ_Code.MaxLength));
						query.AddToFilter(JoinCondition.And, RefZoneHeaderSchema.FZ_Description, SQLComparisonOperator.Contains, Description.SubstringSafe(0, RefZoneHeaderSchema.FZ_Description.MaxLength));
						break;
					case LocationType.Country:
						query.AddToFilter(JoinCondition.And, RefCountrySchema.RN_Code, SQLComparisonOperator.StartsWith, Code.SubstringSafe(0, RefCountrySchema.RN_Code.MaxLength));
						query.AddToFilter(JoinCondition.And, RefCountrySchema.RN_Desc, SQLComparisonOperator.Contains, Description.SubstringSafe(0, RefCountrySchema.RN_Desc.MaxLength));
						break;
				}
				return query;
			}
		}

		#endregion Filter Override

		public override void SetInitialCodeForSearch(ZString code, Type typeOfElementsToFind)
		{
			this[Schema.Code] = code;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			IsPort = true;
		}

		#endregion Overrides

		#region LocationType

		public LocationType LocationType
		{
			get
			{
				LocationType result;
				if (IsPort)
				{
					result = LocationType.Port;
				}
				else if (IsRegion)
				{
					result = LocationType.Zone;
				}
				else if (IsCountry)
				{
					result = LocationType.Country;
				}
				else
				{
					throw new Exception("Invalid Location Type");
				}

				return result;
			}
		}

		#endregion LocationType

		#region Validation Overrides

		public override void ValidateIsCountry()
		{
			base.ValidateIsCountry();
			if (IsCountry)
			{
				IsPort = false;
				IsRegion = false;
			}
			else
			{
				if (!IsPort && !IsRegion)
				{
					IsPort = true;
				}
			}
		}

		public override void ValidateIsPort()
		{
			base.ValidateIsPort();
			if (IsPort)
			{
				IsRegion = false;
				IsCountry = false;
			}
			else
			{
				if (!IsRegion && !IsCountry)
				{
					IsCountry = true;
				}
			}
		}

		public override void ValidateIsRegion()
		{
			base.ValidateIsRegion();
			if (IsRegion)
			{
				IsPort = false;
				IsCountry = false;
			}
			else
			{
				if (!IsPort && !IsCountry)
				{
					IsPort = true;
				}
			}
		}

		#endregion Validation Overrides
	}
}
