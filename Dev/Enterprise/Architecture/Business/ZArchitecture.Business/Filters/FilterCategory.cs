using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public static class FilterCategories
	{
		public static FilterCategory NumbersAndReferences
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.NumbersAndReferences", "Numbers and References")); }
		}

		public static FilterCategory StatusAndFlags
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.StatusAndFlags", "Status and Flags")); }
		}

		public static FilterCategory Dates
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.Dates", "Dates")); }
		}

		public static FilterCategory Times
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.Times", "Times")); }
		}

		public static FilterCategory Locations
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.Locations", "Locations")); }
		}

		public static FilterCategory Organisations
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.Organisations", "Organizations / Staff")); }
		}

		public static FilterCategory EmailAddress
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.EmailAddress", "Email Address")); }
		}

		public static FilterCategory ModesAndTypes
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.ModesAndTypes", "Modes and Types")); }
		}

		public static FilterCategory TextSearch
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.TextSearch", "Text Search")); }
		}

		public static FilterCategory AttributeSearch
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.AttributeSearch", "Attribute Search")); }
		}

		public static FilterCategory Other
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.Other", "Other")); }
		}

		public static FilterCategory AuditInformation
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.AuditInformation", "Audit Information")); }
		}

		public static FilterCategory FinancialDetails
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.FinancialDetails", "Financial Details")); }
		}

		public static FilterCategory SalesRelationActivity
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.SalesRelationActivity", "Sales Relation Activity")); }
		}

		public static FilterCategory RelationshipOrgAndStaff
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("b4458e71-7f6f-4988-b2f9-83b6b96c323e", "Relationship Org. and Staff")); }
		}

		public static FilterCategory CRMSecurity
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("014b3be5-6f15-41d6-9e1d-2c59690f7886", "CRM Security")); }
		}

		public static FilterCategory ChargeCode
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.ChargeCode", "Charge Code")); }
		}

		public static FilterCategory SupportingDocument
		{
			get { return GetOrCreateFilterCategory(ResString.GetMultilingualString("002C8B88-3CE6-4B6A-A525-C06AAB7775B9", "Supporting Document")); }
		}

		public static FilterCategory UserDefined => GetOrCreateFilterCategory(ResString.GetMultilingualString("c0cb053b-2d55-429e-837b-46148aea23c8", "User-Defined Filters"));

		public static FilterCategory DocumentTracking => GetOrCreateFilterCategory(ResString.GetMultilingualString("A10FE7AF-1F29-4EE4-B856-F7A6C3AD9C7A", "Document Tracking"));

		public static FilterCategory Execution => GetOrCreateFilterCategory(ResString.GetMultilingualString("4D001B6D-56CA-45B5-B1CA-AF30451EB839", "Execution"));

		public static FilterCategory Customs => GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.Customs", "Customs"));

		#region Implementation

		public static FilterCategory GetOrCreateFilterCategory(MultilingualString description)
		{
			FilterCategory result;
			if (Categories.ContainsKey(description.GetUnresolvedString()))
			{
				result = Categories[description.GetUnresolvedString()];
			}
			else
			{
				result = new FilterCategory(description);
				Categories.Add(description.GetUnresolvedString(), result);
			}

			return result;
		}

		static Dictionary<ZString, FilterCategory> Categories
		{
			get { return categories ?? (categories = new Dictionary<ZString, FilterCategory>()); }
		}
		[ThreadStatic]
		static Dictionary<ZString, FilterCategory> categories;

		#endregion
	}

	public class FilterCategory
	{
		public FilterCategory(MultilingualString description)
		{
			Description = description;
		}

		public override string ToString()
		{
			return Description;
		}

		public readonly MultilingualString Description;
	}
}
