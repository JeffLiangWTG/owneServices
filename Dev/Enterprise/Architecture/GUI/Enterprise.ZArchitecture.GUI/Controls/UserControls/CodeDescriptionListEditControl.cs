using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.Forms
{
	[ToolboxItem(false)]
	public partial class CodeDescriptionListEditControl : ZUserControl, ICodeDescriptionListControl
	{
		public CodeDescriptionListEditControl() : this(true, true, CharacterCasing.Upper, CharacterCasing.Upper)
		{
		}

		public CodeDescriptionListEditControl(bool showCodeColumn, bool showDescriptionColumn, CharacterCasing codeFieldCasing, CharacterCasing descriptionFieldCasing)
			: this(showCodeColumn, showDescriptionColumn, codeFieldCasing, descriptionFieldCasing, Res.GetString("CodeDescriptionListEditControl|CodeColumn", "Code"))
		{
		}

		public CodeDescriptionListEditControl(bool showCodeColumn, bool showDescriptionColumn, CharacterCasing codeFieldCasing, CharacterCasing descriptionFieldCasing, string codeColumnCaption)
			: this(showCodeColumn, showDescriptionColumn, codeFieldCasing, descriptionFieldCasing, codeColumnCaption, Res.GetString("CodeDescriptionListEditControl|DescriptionColumn", "Description"))
		{
		}

		public CodeDescriptionListEditControl(bool showCodeColumn, bool showDescriptionColumn, CharacterCasing codeFieldCasing, CharacterCasing descriptionFieldCasing, string codeColumnCaption, string descriptionColumnCaption)
			: this(showCodeColumn, showDescriptionColumn, codeFieldCasing, descriptionFieldCasing, codeColumnCaption, descriptionColumnCaption, 75, 280)
		{
		}

		public CodeDescriptionListEditControl(bool showCodeColumn, bool showDescriptionColumn, CharacterCasing codeFieldCasing, CharacterCasing descriptionFieldCasing, string codeColumnCaption,
			string descriptionColumnCaption, int codeFieldWidth, int descriptionFieldWidth)
		{
			InitializeComponent();
			InitialiseColumns(showCodeColumn, showDescriptionColumn, codeFieldCasing, descriptionFieldCasing, codeColumnCaption, descriptionColumnCaption, codeFieldWidth, descriptionFieldWidth);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non-translatable column name")]
		public const string CodeColumnName = "Code";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non-translatable column name")]
		public const string DescriptionColumnName = "Description";

		#region Multi-line

		public bool IsMultilineDescriptionColumn
		{
			set
			{
				var showDescriptionColumn = CodeDescriptionGrid.Columns.Contains(DescriptionColumnName); // Programmatic constant
				if (showDescriptionColumn)
				{
					var currentColumn = (ZTextBoxColumnStyle)CodeDescriptionGrid.Columns[DescriptionColumnName].ColumnStyle;
					var caption = currentColumn.HeaderText;
					var width = currentColumn.Width;
					var casing = currentColumn.CharacterCasing;

					CodeDescriptionGrid.Columns.Remove(DescriptionColumnName); // Programmatic constant
					currentColumn.Dispose();

					if (value)
					{
						var multiLineColumn = new ZMultiLineTextBoxColumnInfo(DescriptionColumnName, width); // Programmatic constant
						multiLineColumn.CharacterCasing = casing;
						CodeDescriptionGrid.Columns.Add(multiLineColumn);
					}
					else
					{
						CodeDescriptionGrid.Columns.AddTextColumn(DescriptionColumnName, width, casing);
					}

					CodeDescriptionGrid.Columns[DescriptionColumnName].ColumnStyle.HeaderText = caption;
				}
			}
		}

		#endregion

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				try
				{
					fReadOnly = value;
					SuspendLayout();
					CodeDescriptionGrid.SetReadOnly(value);
				}
				finally
				{
					ResumeLayout(false);
				}
			}
		}
		bool fReadOnly;

		#region Initialisation

		void InitialiseColumns(bool showCodeColumn, bool showDescriptionColumn, CharacterCasing codeFieldCasing, CharacterCasing descriptionFieldCasing, string codeColumnCaption,
			string descriptionColumnCaption, int codeFieldWidth, int descriptionFieldWidth)
		{
			if (showCodeColumn)
			{
				CodeDescriptionGrid.Columns.AddTextColumn(CodeColumnName, codeFieldWidth, codeFieldCasing); // Column identifier
				CodeDescriptionGrid.Columns[CodeColumnName].ColumnStyle.HeaderText = codeColumnCaption;
				var style = new ZTextBoxColumnStyleInfo(CodeColumnName, codeFieldWidth); // Column identifier
				style.CharacterCasing = codeFieldCasing;
				style.Caption = codeColumnCaption;
				CodeDescriptionGrid.ColumnStyles.Add(style);
			}

			if (showDescriptionColumn)
			{
				CodeDescriptionGrid.Columns.AddTextColumn(DescriptionColumnName, descriptionFieldWidth, descriptionFieldCasing); // Column identifier
				CodeDescriptionGrid.Columns[DescriptionColumnName].ColumnStyle.HeaderText = descriptionColumnCaption;
				var style = new ZTextBoxColumnStyleInfo(DescriptionColumnName, descriptionFieldWidth); // Column identifier
				style.CharacterCasing = descriptionFieldCasing;
				style.Caption = descriptionColumnCaption;
				CodeDescriptionGrid.ColumnStyles.Add(style);
			}
		}

		#endregion

		#region Code Max Length

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int CodeMaxLength
		{
			get { return ((ZTextBoxColumnStyle)CodeDescriptionGrid.Columns[CodeColumnName].ColumnStyle).TextBox.MaxLength; }
			set { ((ZTextBoxColumnStyle)CodeDescriptionGrid.Columns[CodeColumnName].ColumnStyle).TextBox.MaxLength = value; }
		}

		#endregion

		#region FieldValue

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual byte[] FieldValue
		{
			get
			{
				byte[] result = null;

				if (Data != null)
				{
					var xmlStream = new MemoryStream();
					Data.WriteXml(xmlStream, XmlWriteMode.WriteSchema);
					result = xmlStream.ToArray();
				}
				return result;
			}
			set
			{
				CodeDescriptionGrid.DataBindings.Clear();

				Data = new DataSet(); // we need a DataSet to write xml
				if (value != null)
				{
					var xmlStream = new MemoryStream(value);
					Data.ReadXml(xmlStream, XmlReadMode.Auto);
				}
				if (Data.Tables["ListTable"] == null)
				{
					var table = new DataTable("ListTable");
					var codeColumn = new DataColumn(CodeColumnName, typeof(string)); // Internal data formatting
					var descColumn = new DataColumn(DescriptionColumnName, typeof(string)); // Internal data formatting
					table.Columns.Add(codeColumn);
					table.Columns.Add(descColumn);
					Data.Tables.Add(table);
				}

				CodeDescriptionGrid.SetDataBinding(Data, Data.Tables[0].TableName);
			}
		}

		protected DataSet Data; // this is how we store the data

		#endregion

		#region Size Changed

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			CodeDescriptionGrid.PerformLayout();
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				CodeDescriptionGrid.Dispose();
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region ICodeDescriptionListControl Members

		object ICodeDescriptionListControl.Data
		{
			get { return FieldValue; }
			set { FieldValue = (byte[])value; }
		}

		ZUserControl ICodeDescriptionListControl.Control
		{
			get { return this; }
		}

		ZGrid ICodeDescriptionListControl.Grid
		{
			get { return CodeDescriptionGrid; }
		}

		#endregion
	}
}

