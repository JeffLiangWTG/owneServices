using System;
using System.ComponentModel;
using System.Globalization;

namespace Aga.Controls.Tree.NodeControls
{
	public class NodeDateBox : NodeTextBox
	{
		public NodeDateBox()
		{
		}

		[DefaultValue("T")]
		public string DateTimeFormat
		{
			get { return dateTimeFormat; }
			set { dateTimeFormat = value; }
		}
		private string dateTimeFormat;

		protected override string FormatLabel(object obj)
		{
			string result = string.Empty;
			if (obj is DateTime)
			{
				var dateTime = (DateTime)obj;
				if (dateTime == DateTime.MinValue)
				{
					return string.Empty;
				}
				else
				{
					return dateTime.ToString(DateTimeFormat, CultureInfo.CurrentCulture);
				}
			}
			else
			{
				return base.FormatLabel(obj);
			}
		}

		public override void SetValue(TreeNodeAdv node, object value)
		{
			DateTime valueAsDateTime;
			if (DateTime.TryParseExact(value.ToString(), DateTimeFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out valueAsDateTime))
			{
				base.SetValue(node, value);
			}
		}
	}
}
