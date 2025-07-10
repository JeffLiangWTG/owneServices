using System;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	[DefaultBindingProperty("DocumentText")]
	public partial class HtmlInterpretationBox : ZWebBrowser, IDataBoundControl, IBindTo
	{
		public HtmlInterpretationBox()
		{
			InitializeComponent();
		}

		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(false)]
		public bool UseFixedWidthForPlainText { get; set; }

		public string FormattedDocumentText => base.DocumentText;

		#region DocumentText

		[BindingOptions(UseTypeConverters = true)]
		public new string DocumentText
		{
			get { return originalValue; }
			set
			{
				if (originalValue != value)
				{
					originalValue = value;

					if (workItem != null)
					{
						workItem.Dispose();
					}

					workItem = UserIdleWorker.QueueWorkItem(this, 0, UpdateDocumentText, value);
				}
			}
		}
		string originalValue;

		void UpdateDocumentText(string text)
		{
			var html = GetHtmlFormattedText(text);
			if (html != base.DocumentText)
			{
				UpdateDocumentTextFromHtmlText(html);
			}
			workItem = null;
		}

		protected virtual void UpdateDocumentTextFromHtmlText(string htmlText)
		{
			base.DocumentText = htmlText;
		}

		IDisposable workItem;

		#region GetHtmlFormattedText

		protected string GetHtmlFormattedText(string textToFormat)
		{
			if (textToFormat != null && !textToFormat.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase))
			{
				var isPlainText = !Regex.IsMatch(textToFormat, @"<[^>]+>");
				textToFormat = HtmlTamplate.Replace("(*HtmlStyleSheet*)", GetHtmlStyleSheet(isPlainText))
					.Replace("(*HtmlBody*)", GetHtmlIfPlainText(textToFormat, isPlainText));
			}
			return textToFormat;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be a script or markup.")]
		static string GetHtmlStyleSheet(bool isPlainText)
		{
			//This is default text box font style to make plain text looks like in text box
			const string plainTextStyleSheet = @"
body, p, td 
{ 
	font-family: Microsoft Sans Serif; 
	font-size: 8.25pt;
	margin:4pt;
}";
			return isPlainText ? plainTextStyleSheet : SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be a script or markup.")]
		string GetHtmlIfPlainText(string textToFormat, bool isPlainText)
		{
			if (isPlainText)
			{
				var builder = new StringBuilder(textToFormat);
				builder.Replace("&", "&amp;");
				builder.Replace("<", "&lt;");
				builder.Replace("\r\n", "<br/>");
				builder.Replace(" ", "&nbsp;");

				if (UseFixedWidthForPlainText)
				{
					builder.Insert(0, "<pre>");
					builder.Append("</pre>");
				}

				textToFormat = builder.ToString();
			}
			return textToFormat;
		}

		const string HtmlTamplate = @"<html>
<head>
	<style type='text/css'>
		<!--
			(*HtmlStyleSheet*)
		-->
	</style>
</head>
<body>
	<table border='0' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>
		<tr>
			<td class='content'>
				(*HtmlBody*)
			</td>
		</tr>
	</table>
</body>
</html>";

		#endregion

		#endregion

		#region IDataBoundControl Members

		string IDataBoundControl.DataMember
		{
			get { return DataBoundControl.GetDefaultImplementation(this).DataMember; }
		}

		object IDataBoundControl.DataSource
		{
			get { return DataBoundControl.GetDefaultImplementation(this).DataSource; }
		}

		Type IDataBoundControl.DataSourceType
		{
			get { return DataBoundControl.GetDefaultImplementation(this).DataSourceType; }
		}

		public virtual void SetDataBinding(object dataSource, string dataMember)
		{
			DataBoundControl.GetDefaultImplementation(this).SetDataBinding(dataSource, dataMember);
		}

		#endregion

		#region IBindTo Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindTo
		{
			get { return BindingMemberHelper.BindingMember; }
			set { BindingMemberHelper.BindingMember = value; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType; }
		}

		#endregion
	}
}
