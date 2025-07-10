using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.DocBuilder;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[Serializable]
	public class SortOrder : NonPersistentBusinessObject, IObsoleteValidation, IBindableBooleanItem, IJsonSerializable
	{
		/// <summary>
		/// A possible sort order
		/// </summary>
		/// <param name="displayName">The name of this sort order to show in user interfaces</param>
		/// <param name="fieldList">The field list following ORDER BY in an SQL query</param>
		public SortOrder(string displayName, string fieldList)
		{
			this.DisplayName = displayName;
			this.FieldList = fieldList;
			Selected = false;
		}

		#region Constructor For IJsonSerializable

		internal SortOrder(SortOrderJsonData data)
		{
			DisplayName = data.DisplayName;
			FieldList = data.FieldList;
			Selected = data.Selected;
		}

		#endregion

		public string DisplayName, FieldList;

		public string DisplayNameLocalized
		{
			get
			{
				return DocBuilderResourceStrings.GetTranslationString(DisplayNameLocalizedData, DisplayName);
			}
		}
		public ResourceStringData DisplayNameLocalizedData;

		public bool Selected { get; set; }

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new SortOrderJsonData
			{
				DisplayName = DisplayName,
				FieldList = FieldList,
				Selected = Selected
			};

		#endregion

		#region IBindableBooleanItem Members

		public ZBool BoolValue
		{
			get
			{
				return Selected;
			}
			set
			{
				Selected = value;
				BoolValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BoolValueInfo
		{
			get { return GetZPropertyInfo(nameof(BoolValue)); }
		}

		public ZString Text
		{
			get { return DisplayNameLocalized; }
		}

		#endregion
	}
}
