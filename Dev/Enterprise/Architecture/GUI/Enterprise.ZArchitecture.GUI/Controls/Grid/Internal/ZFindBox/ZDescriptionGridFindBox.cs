using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	internal class ZDescriptionGridFindBox : ZGridFindBox, ICustomizableFindBoxPopup, IFindBoxPopupWithAdvancedCodeStore
	{
		#region Overrides

		protected override string CodeForFinding => FindBoxListProviderDescriptionEx?.CodeFromDescription(Description) ?? ZDescriptionCodeFindBoxColumnStyle.InvalidCode;

		protected override string Description
		{
			get { return Code; }
			set { Code = value; }
		}

		protected override string AutoCompleteTextCore(string text, bool explicitAutoComplete, int cursor)
		{
			var findBoxListProviderDescriptionEx = FindBoxListProviderDescriptionEx;
			return findBoxListProviderDescriptionEx != null
				? findBoxListProviderDescriptionEx.NearestDescriptionMatch(text, explicitAutoComplete)
				: base.AutoCompleteTextCore(text, explicitAutoComplete, cursor);
		}

		#endregion

		#region Implementation

		internal void PullList(object currentItem, string propertyName)
		{
			CurrentItem = currentItem;
			DataPropertyName = propertyName;
			PullList();
		}

		internal IFindBoxListProviderDescriptionEx FindBoxListProviderDescriptionEx
		{
			get
			{
				var result = IFindBox.ListProvider as IFindBoxListProviderDescriptionEx;
				if (result == null && IFindBox.ListProvider != null)
				{
					result = IFindBox.ListProvider.List as IFindBoxListProviderDescriptionEx;
				}
				return result;
			}
		}

		#endregion

		#region ICustomizableFindBoxPopup Members

		string ICustomizableFindBoxPopup.CodeForPopup
		{
			get { return Text; }
		}

		string ICustomizableFindBoxPopup.PropertyNameForPopup
		{
			get { return ((ZDescriptionCodeFindBoxColumnStyle)ColumnStyle).DescriptionColumnName; }
		}

		string IFindBoxPopupWithAdvancedCodeStore.CodeFromPopup { get; set; }

		#endregion
	}
}
