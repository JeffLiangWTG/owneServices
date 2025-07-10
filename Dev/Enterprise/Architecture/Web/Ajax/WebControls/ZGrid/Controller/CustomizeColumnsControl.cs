using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.ZGridInternals
{
	static class ButtonExtensions
	{
		public static void SetAttributes(this AttributeCollection attributes, string onClickScript, string cssClass)
		{
			if (attributes != null)
			{
				attributes.Add(nameof(HtmlTextWriterAttribute.Class), cssClass);
				if (!string.IsNullOrEmpty(onClickScript))
				{
					attributes.Add("onClick", onClickScript);
				}
			}
		}
	}

	public class CustomizeColumnsControl : HtmlGenericControl
	{
		#region Constructors

		public CustomizeColumnsControl()
			: base(nameof(HtmlTextWriterTag.Div))
		{
		}

		public CustomizeColumnsControl(string tag)
			: base(nameof(HtmlTextWriterTag.Div))
		{
		}

		#endregion

		public static class Constants
		{
			public const string AcceptButtonID = "AcceptButtonn";
			public const string CancelButtonID = "CancelButtonn";
			public const string AvailableColumnsLabelID = "AvailableColumnsLabel";
			public const string SelectedColumnsLabelID = "SelectedColumnsLabel";
		}

		#region Css

		public string CssClass
		{
			get { return cssClass; }
			set { cssClass = value; }
		}
		string cssClass = "CustomizeColumnsControl";

		public string InnerDivCssClass
		{
			get { return innerDivCssClass; }
			set { innerDivCssClass = value; }
		}
		string innerDivCssClass = "CustomizeColumnsControlInner";

		public string ButtonsHolderCssClass
		{
			get { return buttonsHolderCssClass; }
			set { buttonsHolderCssClass = value; }
		}
		string buttonsHolderCssClass = "CustomizeColumnsControlButtonsHolder";

		public string ButtonCssClass
		{
			get { return buttonCssClass; }
			set { buttonCssClass = value; }
		}
		string buttonCssClass = "CustomizeColumnsButtons";

		#endregion

		public Dictionary<int, string> AvailableColumns
		{
			get { return availableColumns ?? (availableColumns = new Dictionary<int, string>()); }
		}
		Dictionary<int, string> availableColumns;

		public List<int> SelectedColumns
		{
			get { return selectedColumns ?? (selectedColumns = new List<int>()); }
		}
		List<int> selectedColumns;

		public List<int> RequiredColumns
		{
			get { return requiredColumns ?? (requiredColumns = new List<int>()); }
		}
		List<int> requiredColumns;

		protected ZListBox availableColumnsListBox;
		protected ZListBox selectedColumnsListBox;
		readonly string requiredColumnFormat = "[{0}]";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "js boolean, html tag 10chrs")]
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			this.Attributes.Add(nameof(HtmlTextWriterAttribute.Class), CssClass);

			availableColumnsListBox = GetListBox();
			availableColumnsListBox.ID = ID + Constants.AvailableColumnsLabelID;
			selectedColumnsListBox = GetListBox();
			selectedColumnsListBox.ID = ID + Constants.SelectedColumnsLabelID;

			var sortedListItems = AvailableColumns.Where(c => !SelectedColumns.Contains(c.Key) && !RequiredColumns.Contains(c.Key)).OrderBy(c => c.Value, StringComparer.CurrentCultureIgnoreCase).Select(c => new ListItem(c.Value, c.Key.ToString())).ToArray();
			availableColumnsListBox.Items.AddRange(sortedListItems);

			foreach (var colKey in RequiredColumns)
			{
				if (AvailableColumns.ContainsKey(colKey) && !SelectedColumns.Contains(colKey))
				{
					var colVal = AvailableColumns[colKey];
					ListItem item = new ListItem(string.Format(requiredColumnFormat, colVal), colKey.ToString());
					selectedColumnsListBox.Items.Add(item);
				}
			}
			foreach (var colKey in SelectedColumns)
			{
				if (AvailableColumns.ContainsKey(colKey))
				{
					if (RequiredColumns.Contains(colKey))
					{
						var colVal = AvailableColumns[colKey];
						ListItem item = new ListItem(string.Format(requiredColumnFormat, colVal), colKey.ToString());
						selectedColumnsListBox.Items.Add(item);
					}
					else
					{
						var colVal = AvailableColumns[colKey];
						ListItem item = new ListItem(colVal, colKey.ToString());
						selectedColumnsListBox.Items.Add(item);
					}
				}
			}
			var addColumnButton = GetHTMLButton(">", string.Format(CultureInfo.InvariantCulture, script_MoveSelected, GetIDForScript(availableColumnsListBox.ClientID), GetIDForScript(selectedColumnsListBox.ClientID), "false"));
			var removeColumnButton = GetHTMLButton("<", string.Format(CultureInfo.InvariantCulture, script_MoveSelected, GetIDForScript(selectedColumnsListBox.ClientID), GetIDForScript(availableColumnsListBox.ClientID), "true"));
			var addAllColumnsButton = GetHTMLButton(">>", string.Format(CultureInfo.InvariantCulture, script_MoveAll, GetIDForScript(availableColumnsListBox.ClientID), GetIDForScript(selectedColumnsListBox.ClientID), "false"));
			var removeAllColumnsButton = GetHTMLButton("<<", string.Format(CultureInfo.InvariantCulture, script_MoveAll, GetIDForScript(selectedColumnsListBox.ClientID), GetIDForScript(availableColumnsListBox.ClientID), "true"));
			var moveUpColumnButton = GetHTMLButton(Res.GetString("173768f9-67c4-4bfd-b578-be294056f658", "Up"), string.Format(script_MoveSelectedUp, GetIDForScript(selectedColumnsListBox.ClientID)));
			var moveDownColumnButton = GetHTMLButton(Res.GetString("9b3e1a95-8566-40e3-b10f-29a66d317a28", "Down"), string.Format(script_MoveSelectedDn, GetIDForScript(selectedColumnsListBox.ClientID)));

			Button okButton = GetButton(Res.GetString("4b188cf6-8f10-43af-b903-f7f41b6ca000", "OK"), string.Empty);
			okButton.ID = GetIDForScript(Constants.AcceptButtonID);
			okButton.Attributes.Add("onClick", string.Format(script_PostSelected, okButton.ClientID, GetIDForScript(selectedColumnsListBox.ClientID)));

			Button resetButton = GetButton(Res.GetString("6ee76f9f-b6de-45a9-aaba-3f36574a8312", "Reset"), string.Format(script_PostEmpty, okButton.ClientID));
			Button cancelButton = GetButton(Res.GetString("0298f5f4-f5a6-484f-b678-280d415cfa63", "Cancel"), string.Empty);
			cancelButton.ID = GetIDForScript(Constants.CancelButtonID);
			cancelButton.Attributes.Add("onClick", string.Format(script_PostEmpty, cancelButton.ClientID));
			#region ListBox

			HtmlGenericControl availableColumnsWrapper = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
			availableColumnsWrapper.Attributes.Add(nameof(HtmlTextWriterAttribute.Class), InnerDivCssClass);
			availableColumnsWrapper.Controls.Add(new Label() { Text = Res.GetString("f4aac1eb-fb7d-41fe-9640-3e8b179147a4", "Available Columns:") });
			availableColumnsWrapper.Controls.Add(new LiteralControl("<br/>"));
			availableColumnsWrapper.Controls.Add(availableColumnsListBox);
			HtmlGenericControl selectedColumnsWrapper = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
			selectedColumnsWrapper.Attributes.Add(nameof(HtmlTextWriterAttribute.Class), InnerDivCssClass);
			selectedColumnsWrapper.Controls.Add(new Label() { Text = Res.GetString("8751f9ea-c10a-4527-8c85-ed51b3e2f71a", "Selected Columns:") });
			selectedColumnsWrapper.Controls.Add(new LiteralControl("<br/>"));
			selectedColumnsWrapper.Controls.Add(selectedColumnsListBox);

			#endregion

			#region Buttons
			HtmlGenericControl selectedColumnsController = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
			selectedColumnsController.Attributes.Add(nameof(HtmlTextWriterAttribute.Class), ButtonsHolderCssClass);
			selectedColumnsController.Controls.Add(addAllColumnsButton);
			selectedColumnsController.Controls.Add(new LiteralControl("<br/>"));
			selectedColumnsController.Controls.Add(addColumnButton);
			selectedColumnsController.Controls.Add(new LiteralControl("<br/>"));
			selectedColumnsController.Controls.Add(removeColumnButton);
			selectedColumnsController.Controls.Add(new LiteralControl("<br/>"));
			selectedColumnsController.Controls.Add(removeAllColumnsButton);

			HtmlGenericControl orderController = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
			orderController.Attributes.Add(nameof(HtmlTextWriterAttribute.Class), ButtonsHolderCssClass);
			orderController.Controls.Add(moveUpColumnButton);
			orderController.Controls.Add(new LiteralControl("<br/>"));
			orderController.Controls.Add(moveDownColumnButton);
			orderController.Controls.Add(new LiteralControl("<br/>"));
			orderController.Controls.Add(new LiteralControl("<br/>"));
			orderController.Controls.Add(new LiteralControl("<br/>"));
			orderController.Controls.Add(okButton);
			orderController.Controls.Add(new LiteralControl("<br/>"));
			orderController.Controls.Add(resetButton);
			orderController.Controls.Add(new LiteralControl("<br/>"));
			orderController.Controls.Add(cancelButton);

			#endregion

			this.Controls.Add(availableColumnsWrapper);
			this.Controls.Add(selectedColumnsController);
			this.Controls.Add(selectedColumnsWrapper);
			this.Controls.Add(orderController);
		}

		string GetIDForScript(string controlID)
		{
			return this.NamingContainer == null ? controlID : string.Format("{0}_{1}", this.NamingContainer.ClientID, controlID);
		}

		Button GetButton(string text, string onClickScript)
		{
			Button result = new Button();
			result.Text = text;
			result.Attributes.SetAttributes(onClickScript, buttonCssClass);
			return result;
		}

		HtmlButton GetHTMLButton(string text, string onClickScript)
		{
			var result = new HtmlButton();
			result.InnerText = text;
			result.Attributes.SetAttributes(onClickScript, buttonCssClass);
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "css text should not translated")]
		ZListBox GetListBox()
		{
			ZListBox listBox = new ZListBox();
			listBox.Width = 200;
			listBox.Height = 136;
			listBox.Rows = 10;
			listBox.Style.Add("overflow-x", "auto");
			return listBox;
		}

		#region Button OnClicks

		readonly string script_MoveSelected = "moveSelected({0}, {1}, {2}); return false;";
		readonly string script_MoveAll = "moveAll({0}, {1}, {2}); return false;";
		readonly string script_MoveSelectedUp = "moveSelectedUp({0}); return false;";
		readonly string script_MoveSelectedDn = "moveSelectedDn({0}); return false;";
		readonly string script_PostSelected = "javascript:__doPostBack('{0}', getList({1})); return true;";
		readonly string script_PostEmpty = "javascript:__doPostBack('{0}', ''); return true;";

		#endregion
	}
}
