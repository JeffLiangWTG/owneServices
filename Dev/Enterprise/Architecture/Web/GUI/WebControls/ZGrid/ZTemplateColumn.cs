using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Base Column for all editor-enabled controls
	/// </summary>
	public abstract class ZTemplateColumn : TemplateColumn, IBindTo, IUniqueKeyColumn, ITextTransformer
	{
		public ZTemplateColumn(string headerText, string bindTo)
		{
			IsItemTemplateOverride = false;
			this.HeaderText = headerText;
			this.BindTo = bindTo;
			this.SortExpression = bindTo;
			this.DisplayNotifications = false;
			editorWidth = 80;
		}

		[DefaultValue(TextTransformOptions.None)]
		public TextTransformOptions TextTransform { get; set; }

		public int ColumnIndex { get; set; }

		public object ColumnKey { get; set; }

		public int UniqueKey
		{
			get
			{
				if (ColumnKey == null)
				{
					return ColumnIndex;
				}
				return (int)ColumnKey;
			}
		}

		[DefaultValue("")]
		public string ID
		{
			get { return id; }
			set { id = value; }
		}
		string id;

		public string BindTo { get; set; }

		public bool DisplayNotifications
		{
			get
			{
				return displayNotifications;
			}
			set
			{
				displayNotifications = value;
			}
		}
		bool displayNotifications;

		public override void Initialize()
		{
			base.Initialize();
			if (!IsItemTemplateOverride)
			{
				base.ItemTemplate = null;
				base.EditItemTemplate = null;
				if (!ReadOnly)
				{
					base.EditItemTemplate = GetEditItemTemplate();
					base.ItemTemplate = EditItemTemplate;
				}
				if (ItemTemplate == null)
				{
					base.ItemTemplate = GetItemTemplate();
				}
			}
		}

		bool IsItemTemplateOverride;

		public override ITemplate ItemTemplate
		{
			get
			{
				return base.ItemTemplate;
			}
			set
			{
				IsItemTemplateOverride = true;
				base.ItemTemplate = value;
			}
		}

		public ITemplate GetNewItemTemplate()
		{
			return GetItemTemplate();
		}

		protected internal virtual ITemplate GetItemTemplate()
		{
			return base.ItemTemplate;
		}

		public ITemplate GetNewEditItemTemplate()
		{
			return GetEditItemTemplate();
		}

		protected internal virtual ITemplate GetEditItemTemplate()
		{
			return new ZTextEditColumnEditItemTemplate(this);
		}

		public bool AllowEdit
		{
			get
			{
				if (ZOwner != null)
				{
					return ZOwner.AllowEdit;
				}
				return true;
			}
		}

		public ZDataGrid ZOwner
		{
			get
			{
				return (ZDataGrid)this.Owner;
			}
		}

		public bool ReadOnly
		{
			get { return readOnly || !AllowEdit || (ZOwner != null && ZOwner.ReadOnly); }
			set { readOnly = value; }
		}
		bool readOnly;

		public int EditorWidth
		{
			get { return (ZOwner != null && ZOwner.AutoSizeColumns ? -1 : editorWidth); }
			set { editorWidth = value; }
		}
		int editorWidth;

		public bool NoWrap { get; set; }

		public static ZTemplateColumn GetNew(ZString caption, Type type, ZString bindTo)
		{
			if (type.IsAssignableFrom(typeof(ZDateTime)))
			{
				return new ZDateTimeColumn(caption, bindTo);
			}
			if (type.IsAssignableFrom(typeof(ZDecimal)))
			{
				return new ZCalcEditColumn(caption, bindTo);
			}
			if (type.IsAssignableFrom(typeof(ZBool)))
			{
				return new ZCheckBoxColumn(caption, bindTo);
			}

			return new ZTextEditColumn(caption, bindTo);
		}

		public static ZTemplateColumn GetNew(ZString caption, SchemaColumn column, ZString bindToPrefix)
		{
			ZString bindTo = bindToPrefix.IsEmpty ? column.Name : bindToPrefix + "+" + column.Name;

			if (column is SchemaDateTimeColumn)
			{
				return new ZDateTimeColumn(caption, bindTo);
			}
			if (column is SchemaDecimalColumn)
			{
				return new ZCalcEditColumn(caption, bindTo);
			}
			if (column is SchemaBoolColumn)
			{
				return new ZCheckBoxColumn(caption, bindTo);
			}

			return new ZTextEditColumn(caption, bindTo);
		}
	}
}
