using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
#if DEBUG
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
#endif
	public class ZGuidSearchEdit : ZGuidDropEdit
	{
		public ZGuidSearchEdit()
		{
			ShowDescriptionBox = false;
			ShowInDropDown = ShowInDropDownList.OnlyShowCode;
			EnableShowEditOrViewForm = true;
		}

		public string SearchPhrase => CodeBox.Text;
		public ISearchBoxFilter Searcher { get; set; }

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(-1)]
		public int MaximumRows { get; set; }

		protected override ZDropCodeBox NewCodeBox()
		{
			return new ZSearchCodeBox.Bare();
		}

		protected override ZDropButton NewDropButton()
		{
			return new ZSearchButton();
		}

		protected override IEnumerable<BusinessObject> GetBizObjsToEditOrViewCore()
		{
			PullList();

			var bizObjs = new List<BusinessObject>();
			if (List.Count == 1)
			{
				bizObjs.Add(((IBusinessObjectCollection)List).ToArray()[0]);
			}
			return bizObjs;
		}

		protected override IList GetFilteredListForDropDown()
		{
			if (lastSearchPhrase != SearchPhrase)
			{
				InvalidateList();
				lastSearchPhrase = SearchPhrase;
			}
			return List;
		}

		protected override void UpdateLookups(IList lookupCollection)
		{
			if (Searcher == null)
			{
				var column = GetSchemaColumnFromLookup(lookupCollection);
				Searcher = new SearchBoxFilter((SchemaStringColumn)column);
			}
			Searcher?.ApplySearch(lookupCollection, SearchPhrase);
		}

		string lastSearchPhrase;
	}
}
