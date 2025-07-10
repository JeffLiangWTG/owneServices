using System;
using System.Text;
using System.Web;
using System.Web.SessionState;
using CargoWise.Data;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.WebControls.GuidTextBox.Internals;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class AutoCompleteTextBoxRequestHandler : IHttpHandler, IReadOnlySessionState
	{
		#region Constants

		public const string BaseUrl = "AutoCompleteTextBoxRequestHandler.ashx";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property Names")]
		public const string Key = "value";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property Names")]
		public const string Helper = "helper";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property Names")]
		public const string Params = "params";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property Names")]
		public const string MaxItemsCount = "count";

		#endregion

		#region IHttpHandler Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "HTML Formatting")]
		public void ProcessRequest(HttpContext context)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var key = context.Request[Key];

				var encoder = new QueryParamsEncoder();

				var helperTypeName = encoder.Decrypt(context.Request[Helper]);

				var helper = (AutoCompleteHelper)Activator.CreateInstance(Type.GetType(helperTypeName), new object[] { null });
				if (!string.IsNullOrEmpty(context.Request[Params]))
				{
					var parametersString = encoder.Decrypt(context.Request[Params]);
					helper.RestoreAdditionalParamsFromSerializedString(parametersString);
				}

				helper.MaxOptionsCount = int.Parse(encoder.Decrypt(context.Request[MaxItemsCount]));

				var list = helper.GetList(key);

				var sb = new StringBuilder();
				foreach (var option in list)
				{
					var optionLines = option.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
					var valueWithKey = ZAutoCompleteTextWithGuidValueHelper.GetValueWithHiddenPK(optionLines[0], optionLines[1]);
					if (optionLines.Length > 2)
					{
						sb.Append("<li>");
						sb.Append("<span>" + valueWithKey + "</span>");
						sb.Append("<b>");
						for (int i = 2; i < optionLines.Length; i++)
						{
							sb.Append(optionLines[i] + "<br />");
						}
						sb.Append("</b>");
						sb.Append("</li>");
					}
					else
					{
						sb.AppendFormat("<li>{0}</li>", valueWithKey);
					}
				}

				context.Response.ContentType = "text/plain";
				context.Response.Write(sb.ToString());
			}
		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}

		#endregion    

	}
}
