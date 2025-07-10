using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ResString = Enterprise.ZArchitecture.Web.GUI.ResString;

namespace Enterprise.ZArchitecture.Web.Modules
{
	/// <summary>
	/// This class is manually generated but has to be prefixed with Auto in order to pass the Unit Test for FilterBusinessObject
	/// </summary>
	public abstract class AutoOrganisationFilterBusinessObject : FilterBusinessObject
	{
		#region Schema

		public abstract class Schema
		{
			public const string TableName = "OrganisationFilterBusinessObject";
			public const string PK = "PK";

			public const string ActiveStatus = "ActiveStatus";
			public const string OH_Calc_Contains = "OH_Calc_Contains";
			public const string OH_Calc_StartsWith = "OH_Calc_StartsWith";
			public const string OH_Details = "OH_Details";
			public const string OH_DetailsFilter = "OH_DetailsFilter";
			public const string OH_IsConsignee = "OH_IsConsignee";
			public const string OH_IsConsignor = "OH_IsConsignor";
			public const string OH_RelatedConsign = "OH_RelatedConsign";
			public const string OH_IsShippingProvider = "OH_IsShippingProvider";
			public const string OH_IsShippingLine = "OH_IsShippingLine";
			public const string OH_IsAirLine = "OH_IsAirLine";
			public const string OH_IsRailProvider = "OH_IsRailProvider";
			public const string OH_IsInlandWaterwayProvider = "OH_IsInlandWaterwayProvider";
			public const string OH_IsLineHaulProvider = "OH_IsLineHaulProvider";
			public const string OH_IsReceivable = "OH_IsReceivable";
		}

		#endregion Schema

		public AutoOrganisationFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			UpdateReadOnlyOnQueryDeciderParameter(Schema.OH_DetailsFilter, Schema.OH_Details);
			OH_Calc_StartsWith = true;
			OH_Calc_Contains = false;
		}

		public override void SetInitialCodeForSearch(ZString code, Type typeOfElementsToFind)
		{
			OH_DetailsFilter = OrgConstants.FilterControl.OrgDetails.All;
			OH_Details = code;
		}

		#region PK

		public override string PK_ColumnName
		{
			get { return Schema.PK; }
		}

		#endregion PK

		#region Properties

		#region ActiveStatus

		public virtual ZString ActiveStatus
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(ActiveStatusInfo).ToString().TrimEnd(' ', '\t')); }
			set
			{
				CheckMaximumLength(ActiveStatusInfo, value);
				SetPropertyValue(ActiveStatusInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateActiveStatus();
				}
			}
		}

		public virtual void ValidateActiveStatus()
		{
			ActiveStatusInfo.ClearAllNotifications();
		}

		public ZPropertyInfo ActiveStatusInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ActiveStatus); }
		}

		#endregion

		#region OH_Calc_Contains

		public virtual ZBool OH_Calc_Contains
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(OH_Calc_ContainsInfo)); }
			set
			{
				SetPropertyValue(OH_Calc_ContainsInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOH_Calc_Contains();
				}
			}
		}

		public virtual void ValidateOH_Calc_Contains()
		{
			OH_Calc_ContainsInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OH_Calc_ContainsInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OH_Calc_Contains); }
		}

		#endregion OH_Calc_Contains

		#region OH_Calc_StartsWith

		public virtual ZBool OH_Calc_StartsWith
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(OH_Calc_StartsWithInfo)); }
			set
			{
				SetPropertyValue(OH_Calc_StartsWithInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOH_Calc_StartsWith();
				}
			}
		}

		public virtual void ValidateOH_Calc_StartsWith()
		{
			OH_Calc_StartsWithInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OH_Calc_StartsWithInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OH_Calc_StartsWith); }
		}

		#endregion OH_Calc_StartsWith

		#region OH_Details

		public virtual ZString OH_Details
		{
			get
			{
				if (OH_DetailsFilter == FilterConstants.QueryDeciderNoSelectionCode)
				{
					return ZString.Empty;
				}

				return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(OH_DetailsInfo));
			}
			set
			{
				value = value.TrimEnd(' ', '\t');
				CheckMaximumLength(OH_DetailsInfo, value);
				SetPropertyValue(OH_DetailsInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOH_Details();
				}
			}
		}

		public virtual void ValidateOH_Details()
		{
			OH_DetailsInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OH_DetailsInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OH_Details); }
		}

		#endregion OH_Details

		#region OH_DetailsFilter

		public virtual ZString OH_DetailsFilter
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(OH_DetailsFilterInfo)); }
			set
			{
				value = value.TrimEnd(' ', '\t');
				CheckMaximumLength(OH_DetailsFilterInfo, value);
				SetQueryProviderParameterPropertyValue(OH_DetailsFilterInfo, value, Schema.OH_DetailsFilter, Schema.OH_Details);
				if (!IsValidationSuspended)
				{
					ValidateOH_DetailsFilter();
				}
			}
		}

		public virtual void ValidateOH_DetailsFilter()
		{
			OH_DetailsFilterInfo.ClearAllNotifications();
			System.ComponentModel.PropertyDescriptor listProperty = System.ComponentModel.TypeDescriptor.GetProperties(this)["OH_DetailsFilter_List"];		// This code is auto-generated
			if (listProperty == null)
			{
				Enterprise.ZArchitecture.Environment.Globals.Message.ShowDeveloperErrorOnce("ListPropertyNotFoundOH_DetailsFilter_List", "List property OH_DetailsFilter_List could not be found. Make sure this is declared in your Filter Business Object", "Error");
			}
			else
			{
				ICodeDescriptionPairList list = (ICodeDescriptionPairList)listProperty.GetValue(this);
				ListValidation.ErrorIfInvalidCode(OH_DetailsFilterInfo, list);
			}
		}

		public virtual ZPropertyInfo OH_DetailsFilterInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OH_DetailsFilter); }
		}

		#endregion OH_DetailsFilter

		#region OH_IsConsignee

		public virtual ZBool OH_IsConsignee
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(OH_IsConsigneeInfo)); }
			set
			{
				SetPropertyValue(OH_IsConsigneeInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOH_IsConsignee();
				}
			}
		}

		public virtual void ValidateOH_IsConsignee()
		{
			OH_IsConsigneeInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OH_IsConsigneeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OH_IsConsignee); }
		}

		#endregion

		#region OH_IsConsignor

		public virtual ZBool OH_IsConsignor
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(OH_IsConsignorInfo)); }
			set
			{
				SetPropertyValue(OH_IsConsignorInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOH_IsConsignor();
				}
			}
		}

		public virtual void ValidateOH_IsConsignor()
		{
			OH_IsConsignorInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OH_IsConsignorInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OH_IsConsignor); }
		}

		#endregion

		#region OH_RelatedConsign

		public virtual ZGuid OH_RelatedConsign
		{
			get { return new ZGuid(((IBusinessObjectInternals)this).GetValueFromRowSafely(OH_RelatedConsignInfo)); }
			set
			{
				SetPropertyValue(OH_RelatedConsignInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOH_RelatedConsign();
				}
			}
		}

		public virtual void ValidateOH_RelatedConsign()
		{
			OH_RelatedConsignInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(OH_RelatedConsignInfo);
		}

		public virtual ZPropertyInfo OH_RelatedConsignInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.OH_RelatedConsign); }
		}

		#endregion

		#region OH_DetailsFilter_List

		public ZQueryProviderCodeDescriptionList OH_DetailsFilter_List
		{
			get
			{
				ZQueryProviderCodeDescriptionList list = new ZQueryProviderCodeDescriptionList();

				list.AddEmptySelection();
				list.Add(OrgConstants.FilterControl.OrgDetails.Code, ResString.GetMultilingualString("82d4acaf-a1cb-4fc0-8679-e53520e80e9e", "Code"), new AddToQueryDelegate(AddForCode));
				list.Add(OrgConstants.FilterControl.OrgDetails.FullName, ResString.GetMultilingualString("0e76b3a0-88e1-4608-8ed6-4804d4ffdb81", "Full Name"), new AddToQueryDelegate(AddForFullName));
				list.Add(OrgConstants.FilterControl.OrgAddress.Address, ResString.GetMultilingualString("6cfaf38f-6665-494c-8959-ffeb61999144", "Address"), new AddToQueryDelegate(AddForAddress));
				list.Add(OrgConstants.FilterControl.OrgAddress.City, ResString.GetMultilingualString("ac671a40-2091-4986-890e-661dbead995c", "City"), new AddToQueryDelegate(AddForCity));
				list.Add(OrgConstants.FilterControl.OrgAddress.State, ResString.GetMultilingualString("ed7a6311-241f-49a3-8238-f6ae11b69ce7", "State"), new AddToQueryDelegate(AddForState));
				list.Add(OrgConstants.FilterControl.OrgAddress.PostCode, ResString.GetMultilingualString("a6560e23-8e80-40a6-b532-56e42c9e793e", "Post Code"), new AddToQueryDelegate(AddForPostCode));
				list.Add(OrgConstants.FilterControl.OrgAddress.Phone, ResString.GetMultilingualString("acac299f-e03d-4083-b447-c8610f881b86", "Phone"), new AddToQueryDelegate(AddForPhone));
				list.Add(OrgConstants.FilterControl.OrgAddress.Mobile, ResString.GetMultilingualString("9242ccd3-b3b1-45c8-b7a2-a98500cf55a7", "Mobile"), new AddToQueryDelegate(AddForMobile));
				list.Add(OrgConstants.FilterControl.OrgAddress.Fax, ResString.GetMultilingualString("7db27849-8efc-4e58-8f55-1e1f3dc7bae5", "Fax"), new AddToQueryDelegate(AddForFax));
				list.Add(OrgConstants.FilterControl.OrgAddress.Email, ResString.GetMultilingualString("3669eaa3-b1cc-4ca4-aab4-c7a8be728e12", "E-Mail"), new AddToQueryDelegate(AddForEmail));
				list.Add(OrgConstants.FilterControl.OrgDetails.Web, ResString.GetMultilingualString("2ddf93f8-0c9c-4c7e-91ce-87e195ecb6f0", "Web Address"), new AddToQueryDelegate(AddForWeb));

				list.AddQueryProviderCompositionForAll(1);
				list.AddQueryProviderComposition(
					OrgConstants.FilterControl.OrgDetails.Common, ResString.GetMultilingualString("55851cf9-d631-4fc8-974a-b0b6749b9f70", "Common"), 2,
					OrgConstants.FilterControl.OrgDetails.Code,
					OrgConstants.FilterControl.OrgDetails.FullName);

				return list;
			}
		}

		#region DetailsFilter delegates

		public void AddForCode(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(OrgHeaderSchema.OH_Code, SQLOperatorToUse, ((ZString)value).SubstringSafe(0, OrgHeaderSchema.OH_Code.MaxLength));
		}

		public void AddForFullName(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(OrgHeaderSchema.OH_FullName, SQLOperatorToUse, ((ZString)value).SubstringSafe(0, OrgHeaderSchema.OH_FullName.MaxLength));
		}

		public void AddForAddress(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetAddressesQuery(OrgAddressSchema.OA_Address1, ((ZString)value).SubstringSafe(0, OrgAddressSchema.OA_Address1.MaxLength)));
		}

		public void AddForCity(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetAddressesQuery(OrgAddressSchema.OA_City, ((ZString)value).SubstringSafe(0, OrgAddressSchema.OA_City.MaxLength)));
		}

		public void AddForState(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetAddressesQuery(OrgAddressSchema.OA_State, ((ZString)value).SubstringSafe(0, OrgAddressSchema.OA_State.MaxLength)));
		}

		public void AddForPostCode(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetAddressesQuery(OrgAddressSchema.OA_PostCode, ((ZString)value).SubstringSafe(0, OrgAddressSchema.OA_PostCode.MaxLength)));
		}

		public void AddForPhone(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetAddressesQuery(OrgAddressSchema.OA_Phone, ((ZString)value).SubstringSafe(0, OrgAddressSchema.OA_Phone.MaxLength)));
		}

		public void AddForMobile(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetAddressesQuery(OrgAddressSchema.OA_Mobile, ((ZString)value).SubstringSafe(0, OrgAddressSchema.OA_Mobile.MaxLength)));
		}

		public void AddForFax(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetAddressesQuery(OrgAddressSchema.OA_Fax, ((ZString)value).SubstringSafe(0, OrgAddressSchema.OA_Fax.MaxLength)));
		}

		public void AddForEmail(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetAddressesQuery(OrgAddressSchema.OA_Email, ((ZString)value).SubstringSafe(0, OrgAddressSchema.OA_Email.MaxLength)));
		}

		public void AddForWeb(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(GetWebQuery(OrgWebURLSchema.PU_URL, ((ZString)value).SubstringSafe(0, OrgWebURLSchema.PU_URL.MaxLength)));
		}

		#endregion DetailsFilter delegates

		#endregion OH_DetailsFilter_List

		#region OH_IsShippingProvider

		public virtual ZBool OH_IsShippingProvider
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(OH_IsShippingProviderInfo)); }
			set
			{
				SetPropertyValue(OH_IsShippingProviderInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOH_IsShippingProvider();
				}
			}
		}

		public virtual void ValidateOH_IsShippingProvider()
		{
			OH_IsShippingProviderInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OH_IsShippingProviderInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.OH_IsShippingProvider); }
		}

		#endregion

		#region OH_IsShippingLine

		public virtual ZBool OH_IsShippingLine
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(OH_IsShippingLineInfo)); }
			set
			{
				SetPropertyValue(OH_IsShippingLineInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOH_IsShippingLine();
				}
			}
		}

		public virtual void ValidateOH_IsShippingLine()
		{
			OH_IsShippingLineInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OH_IsShippingLineInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.OH_IsShippingLine); }
		}

		#endregion

		#region OH_IsAirLine

		public virtual ZBool OH_IsAirLine
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(OH_IsAirLineInfo)); }
			set
			{
				SetPropertyValue(OH_IsAirLineInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOH_IsAirLine();
				}
			}
		}

		public virtual void ValidateOH_IsAirLine()
		{
			OH_IsAirLineInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OH_IsAirLineInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.OH_IsAirLine); }
		}

		#endregion

		#region OH_IsRailProvider

		public virtual ZBool OH_IsRailProvider
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(OH_IsRailProviderInfo)); }
			set
			{
				SetPropertyValue(OH_IsRailProviderInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOH_IsRailProvider();
				}
			}
		}

		public virtual void ValidateOH_IsRailProvider()
		{
			OH_IsRailProviderInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OH_IsRailProviderInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.OH_IsRailProvider); }
		}

		#endregion

		#region OH_IsInlandWaterwayProvider

		public virtual ZBool OH_IsInlandWaterwayProvider
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(OH_IsInlandWaterwayProviderInfo)); }
			set
			{
				SetPropertyValue(OH_IsInlandWaterwayProviderInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOH_IsInlandWaterwayProvider();
				}
			}
		}

		public virtual void ValidateOH_IsInlandWaterwayProvider()
		{
			OH_IsInlandWaterwayProviderInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OH_IsInlandWaterwayProviderInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.OH_IsInlandWaterwayProvider); }
		}

		#endregion

		#region OH_IsLineHaulProvider

		public virtual ZBool OH_IsLineHaulProvider
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(OH_IsLineHaulProviderInfo)); }
			set
			{
				SetPropertyValue(OH_IsLineHaulProviderInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOH_IsLineHaulProvider();
				}
			}
		}

		public virtual void ValidateOH_IsLineHaulProvider()
		{
			OH_IsLineHaulProviderInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OH_IsLineHaulProviderInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.OH_IsLineHaulProvider); }
		}

		#endregion

		#region OH_IsReceivable

		public virtual ZBool OH_IsReceivable
		{
			get { return new ZBool(((IBusinessObjectInternals)this).GetValueFromRowSafely(OH_IsReceivableInfo)); }
			set
			{
				SetPropertyValue(OH_IsReceivableInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateOH_IsReceivable();
				}
			}
		}

		public virtual void ValidateOH_IsReceivable()
		{
			OH_IsReceivableInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo OH_IsReceivableInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.OH_IsReceivable); }
		}

		#endregion

		#endregion Properties

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateActiveStatus();
			ValidateOH_Calc_Contains();
			ValidateOH_Calc_StartsWith();
			ValidateOH_Details();
			ValidateOH_DetailsFilter();
			ValidateOH_IsConsignee();
			ValidateOH_IsConsignor();
			ValidateOH_RelatedConsign();
			ValidateOH_IsShippingProvider();
			ValidateOH_IsShippingLine();
			ValidateOH_IsAirLine();
			ValidateOH_IsRailProvider();
			ValidateOH_IsInlandWaterwayProvider();
			ValidateOH_IsReceivable();
			ValidateOH_IsLineHaulProvider();
			base.RunPreSaveValidationCore(); // call RunPreSaveValidation() on all children then fire OnNotificationsChanged()
		}

		#endregion

		#region Filter Methods

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;
				query.AddToFilter(OrgDetailsGroupBoxFilter);
				return query;
			}
		}

		protected virtual ZQuery OrgDetailsGroupBoxFilter
		{
			get
			{
				ZQuery result = new ZQuery();
				result.AddToFilter(StartsWithPanelFilter);
				result.AddToFilter(DetailsFilterControlFilter);

				if (OH_IsShippingProvider)
				{
					AddBoolQuerry(result, OrgHeaderSchema.OH_IsShippingProvider);
				}

				if (OH_IsShippingLine)
				{
					AddBoolQuerry(result, OrgHeaderSchema.OH_IsShippingLine);
				}

				if (OH_IsAirLine)
				{
					AddBoolQuerry(result, OrgHeaderSchema.OH_IsAirLine);
				}

				if (OH_IsRailProvider)
				{
					AddBoolQuerry(result, OrgHeaderSchema.OH_IsRailProvider);
				}

				if (OH_IsInlandWaterwayProvider)
				{
					AddBoolQuerry(result, OrgHeaderSchema.OH_IsInlandWaterwayProvider);
				}

				if (OH_IsReceivable)
				{
					ZDBOnlyQuery ohQuery = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery cdQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					cdQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					ohQuery.AddSubQuery(cdQuery, JoinCondition.And);
					result.AddToFilter(ohQuery, JoinCondition.And);
				}

				if (OH_IsLineHaulProvider)
				{
					AddBoolQuerry(result, OrgHeaderSchema.OH_IsLineHaulProvider);
				}

				if (OH_RelatedConsign.IsValid)
				{
					ZDBOnlyQuery ohQuery = new ZDBOnlyQuery(typeof(OrgHeader));
					if (OH_IsConsignee)
					{
						ZDBOnlyQuery query1 = GetSupplierBuyerLinkQuery(OrgHeaderSchema.OH_IsConsignee, OrgSupplierBuyerLinkSchema.OL_OH_Buyer, OrgSupplierBuyerLinkSchema.OL_OH_Supplier, OH_RelatedConsign, SQLComparisonOperator.Equal);
						ohQuery.AddToFilter(query1);
					}

					if (OH_IsConsignor)
					{
						ZDBOnlyQuery query2 = GetSupplierBuyerLinkQuery(OrgHeaderSchema.OH_IsConsignor, OrgSupplierBuyerLinkSchema.OL_OH_Supplier, OrgSupplierBuyerLinkSchema.OL_OH_Buyer, OH_RelatedConsign, SQLComparisonOperator.Equal);
						ohQuery.AddToFilter(query2, JoinCondition.Or);
					}

					ohQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.PK, OH_RelatedConsign);
					ohQuery.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsActive, ZBool.True);
					result.AddToFilter(ohQuery, JoinCondition.And);
				}

				return result;
			}
		}

		void AddBoolQuerry(ZQuery resultQuery, SchemaBoolColumn column)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(column, "Y");
			resultQuery.AddToFilter(query, JoinCondition.And);
		}

		protected virtual ZQuery StartsWithPanelFilter
		{
			get { return new ZQuery { DefaultJoinCondition = JoinCondition.And }; }
		}

		protected virtual ZQuery DetailsFilterControlFilter
		{
			get
			{
				ZQuery query = StartsWithPanelFilter;
				AddQueryProviderFilter(query, Schema.OH_DetailsFilter, "OH_DetailsFilter_List", SQLComparisonOperator.Contains, Schema.OH_Details, 0); // This code is auto-generated
				return query;
			}
		}

		protected internal ZQuery DetailsFilterControlFilterInternal => DetailsFilterControlFilter;

		#endregion Filter Methods

		#region OrgHeader Special

		protected ZDBOnlyQuery GetAddressesQuery(SchemaColumn orgAddressField, object value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
			subQuery.AddToFilter(orgAddressField, SQLOperatorToUse, value);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected ZDBOnlyQuery GetWebQuery(SchemaColumn field, object value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgWebURL), OrgWebURLSchema.PU_OH);
			subQuery.AddToFilter(field, SQLOperatorToUse, value);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Implementation

		#region Set Defaults

		protected override void SetDefaultValues()
		{
			((IBusinessObjectInternals)this).Row[Schema.ActiveStatus] = "";
			((IBusinessObjectInternals)this).Row[Schema.OH_Calc_Contains] = false;
			((IBusinessObjectInternals)this).Row[Schema.OH_Calc_StartsWith] = false;
			((IBusinessObjectInternals)this).Row[Schema.OH_Details] = "";
			((IBusinessObjectInternals)this).Row[Schema.OH_DetailsFilter] = "";
			((IBusinessObjectInternals)this).Row[Schema.OH_IsConsignee] = false;
			((IBusinessObjectInternals)this).Row[Schema.OH_IsConsignor] = false;
			((IBusinessObjectInternals)this).Row[Schema.OH_IsShippingProvider] = false;
			((IBusinessObjectInternals)this).Row[Schema.OH_IsShippingLine] = false;
			((IBusinessObjectInternals)this).Row[Schema.OH_IsAirLine] = false;
			((IBusinessObjectInternals)this).Row[Schema.OH_IsRailProvider] = false;
			((IBusinessObjectInternals)this).Row[Schema.OH_IsInlandWaterwayProvider] = false;
			((IBusinessObjectInternals)this).Row[Schema.OH_IsReceivable] = false;
			((IBusinessObjectInternals)this).Row[Schema.OH_IsLineHaulProvider] = false;
			((IBusinessObjectInternals)this).Row[Schema.OH_RelatedConsign] = DBNull.Value;
			ActiveStatus = OrgConstants.FilterControl.ActiveStatus.Code.ActiveClients;
			OH_Calc_StartsWith = true;
			OH_Calc_Contains = false;
		}

		#endregion

		#region Properties

		protected SQLComparisonOperator SQLOperatorToUse
		{
			get { return OH_Calc_StartsWith ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Contains; }
		}

		#endregion

		#region Queries

		#region Org Supplier Buyer Link

		protected ZDBOnlyQuery GetSupplierBuyerLinkQuery(SchemaColumn orgHeaderField, SchemaColumn foreignKey, SchemaColumn supplierBuyerField, object foreignValue, SQLComparisonOperator @operator)
		{
			return GetForeignTableQuery(orgHeaderField, typeof(OrgSupplierBuyerLink), foreignKey, supplierBuyerField, foreignValue, @operator);
		}

		#endregion

		#region Generic Table Query Methods

		protected ZDBOnlyQuery GetForeignTableQuery(SchemaColumn orgHeaderField, Type foreignType, SchemaColumn foreignKey, SchemaColumn foreignField, object foreignValue)
		{
			return GetForeignTableQuery(orgHeaderField, foreignType, foreignKey, foreignField, foreignValue, SQLComparisonOperator.Equal);
		}

		protected ZDBOnlyQuery GetForeignTableQuery(SchemaColumn orgHeaderField, Type foreignType, SchemaColumn foreignKey, SchemaColumn foreignField, object foreignValue, SQLComparisonOperator @operator)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(orgHeaderField, "Y");

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(foreignType, foreignKey);
			subQuery.AddToFilter(JoinCondition.And, foreignField, @operator, foreignValue);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#endregion

		#endregion Implementation
	}
}
