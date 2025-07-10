using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	public class LanguageSelectionControl : ZTextLabel
	{
		public LanguageSelectionControl()
		{
			ID = "LanguageSelection";
		}

		protected override void Render(HtmlTextWriter writer)
		{
			try
			{
				if (WebDataRegistry.Instance.AllowedLanguages.Value.Count > 1)
				{
					base.Render(writer);
				}
			}
			catch (Exception e) when (!e.IsCriticalException()) { }
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			string currentLanguage = Res.CurrentLanguage;
			if (Res.IsEnglish(currentLanguage))
			{
				currentLanguage = Res.DefaultLanguage;
			}
			writer.Write(Res.GetString("74BBE5DC-D47D-466A-AAC4-7FB2418371BA", "Language:") + " ");
			var list = new DropDownList();
			list.DataSource = GetAllowedLanguagesList();
			list.DataTextField = "Description";
			list.DataValueField = "Code";
			list.SelectedValue = currentLanguage;
			list.DataBind();
			list.Attributes.Add("onchange", "changeLanguage(this.options[this.selectedIndex].value);");
			list.ID = "LanguageList";
			list.RenderControl(writer);
		}

		CodeDescriptionPairList GetAllowedLanguagesList()
		{
			var list = new CodeDescriptionPairList();
			foreach (CodeSelection selection in WebDataRegistry.Instance.AllowedLanguages.Value)
			{
				list.AddPair(selection.Code, GetLocalLanguageDescription(selection.Code));
			}
			return list;
		}

		string GetLocalLanguageDescription(string code)
		{
			string value;
			localLanguageDescriptionCache.TryGetValue(code, out value);
			if (string.IsNullOrEmpty(value))
			{
				value = new CodeDescriptionPairList(OLookUpEditType.Language).GetMultilingualDescriptionFromCode(code).ToString(code);
				localLanguageDescriptionCache[code] = value;
			}
			return value;
		}

		static readonly Dictionary<string, string> localLanguageDescriptionCache = new Dictionary<string, string>();
	}

	#endregion
}
