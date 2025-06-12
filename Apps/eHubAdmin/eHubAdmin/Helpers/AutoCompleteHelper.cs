using System;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace eServices.eHubAdmin.Helpers
{
	public static class AutocompleteHelper
	{
		public static HtmlString Autocomplete<TModel>(this HtmlHelper<TModel> html, string name, string actionName, string controllerName, object htmlAttributes)
		{
			return Autocomplete<TModel>(html, name, actionName, controllerName, null, htmlAttributes);
		}

		public static HtmlString Autocomplete<TModel>(this HtmlHelper<TModel> html, string name, string actionName, string controllerName, string outputField, object htmlAttributes)
		{
			var url = new UrlHelper(html.ViewContext.RequestContext);

			var htmlAttibutesDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
			htmlAttibutesDictionary["data-autocomplete-url"] = url.Action(actionName, controllerName);
			htmlAttibutesDictionary["data-autocomplete-out"] = outputField;

			return html.TextBox(name, null, htmlAttibutesDictionary);
		}

		public static HtmlString AutocompleteFor<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string actionName, string controllerName, object htmlAttributes)
		{
			return AutocompleteFor<TModel, TProperty>(html, expression, actionName, controllerName, null, htmlAttributes);
		}

		public static HtmlString AutocompleteFor<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string actionName, string controllerName, string outputField, object htmlAttributes)
		{
			var url = new UrlHelper(html.ViewContext.RequestContext);

			var htmlAttibutesDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
			htmlAttibutesDictionary["data-autocomplete-url"] = url.Action(actionName, controllerName);
			htmlAttibutesDictionary["data-autocomplete-out"] = outputField;

			return html.TextBoxFor(expression, htmlAttibutesDictionary);
		}
	}
}