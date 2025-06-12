using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using eServices.eHubDataModel.eHubTransactions;

namespace eServices.eHubAdmin.Helpers
{
	public static class BreadcrumbsHelper
	{
		public static HtmlString Breadcrumbs<TModel>(this HtmlHelper<TModel> html)
		{
			string controller = (string)html.ViewContext.RouteData.Values["controller"];
			string action = (string)html.ViewContext.RouteData.Values["action"];
			string id = (string)html.ViewContext.RouteData.Values["id"];
			
			var url = new UrlHelper(html.ViewContext.RequestContext);
			var olTag = new TagBuilder("ol");
			olTag.AddCssClass("breadcrumb");
			var sb = new StringBuilder();
			sb.AppendLine(olTag.ToString(TagRenderMode.StartTag));

			if (controller == "Home" && action == "Index")
			{
				return new HtmlString(String.Empty);
			}
			else
			{
				sb.AppendLine("<li><a href='" + url.Action("Index", "Home") + "'><i class='glyphicon glyphicon-home'></i></a></li>");
				string controllerDefaultAction;
				switch (controller)
				{
					case "Messages":
					case "Air":
						controllerDefaultAction = "Query";
						break;
					default:
						controllerDefaultAction = "Index";
					break;
				}

				if (action == controllerDefaultAction)
				{
					sb.AppendLine(GetListItem(Constants.ContollerDisplayName[controller]));
				}
				else
				{
					sb.AppendLine(GetListItem(html.ActionLink(Constants.ContollerDisplayName[controller], controllerDefaultAction, controller)));
					sb.AppendLine(GetListItem((string)html.ViewBag.Title));
				}
			}

			sb.AppendLine(olTag.ToString(TagRenderMode.EndTag));
			return new HtmlString(sb.ToString());
		}

		static string GetListItem(object innerHtml)
		{
			var tag = new TagBuilder("li");
			tag.InnerHtml = (innerHtml ?? String.Empty).ToString();
			if (innerHtml is string) tag.AddCssClass("active");
			return tag.ToString();
		}
	}
}
