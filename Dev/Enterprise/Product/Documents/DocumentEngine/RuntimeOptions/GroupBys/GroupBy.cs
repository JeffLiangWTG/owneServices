using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.DocBuilder;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[Serializable]
	public class GroupBy : NonPersistentBusinessObject, IObsoleteValidation, IBindableBooleanItem, IJsonSerializable
	{
		/// <summary>
		/// A possible groupby
		/// </summary>
		/// <param name="displayName">The name of this groupby to show in user interfaces</param>
		/// <param name="fieldList">The field list for groupby</param>
		public GroupBy(string displayName, string fieldList)
		{
			this.DisplayName = displayName;
			if (fieldList != null)
			{
				this.FieldList = fieldList.Replace(" ", "").Replace("\t", "");
			}
			Selected = false;
		}

		#region Constructor For IJsonSerializable

		internal GroupBy(GroupByJsonData data)
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
				if (Res.CurrentLanguage == Res.DefaultLanguage)
				{
					return DisplayName;
				}
				return DocBuilderResourceStrings.GetTranslationString(DisplayNameLocalizedData, DisplayName);
			}
		}
		public ResourceStringData DisplayNameLocalizedData;

		public bool Selected { get; set; }

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new GroupByJsonData
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
