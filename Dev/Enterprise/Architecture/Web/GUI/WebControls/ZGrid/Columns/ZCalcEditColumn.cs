using System;
using System.ComponentModel;
using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Column for Numberic values in the grid
	/// this is a simplistic implementation, proper implementation should involve TemplateColumn
	/// </summary>
	public class ZCalcEditColumn : ZTemplateColumn
	{
		public ZCalcEditColumn(string headerText, string bindTo) : base(headerText, bindTo)
		{
			ShowGroupSeparators = true;
		}

		public ZCalcEditColumn(string headerText, string bindTo, string bindToDecimals) : this(headerText, bindTo)
		{
			this.fBindToDecimals = bindToDecimals;
		}

		public ZCalcEditColumn(string headerText, string bindTo, string bindToDecimals, string sortExpression) : this(headerText, bindTo, bindToDecimals)
		{
			this.SortExpression = sortExpression;
		}

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZCalcEditColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return new ZCalcEditColumnEditItemTemplate(this);
		}

		#region AutoPostBack

		public bool AutoPostBack
		{
			get { return fAutoPostBack; }
			set { fAutoPostBack = value; }
		}
		bool fAutoPostBack;

		#endregion

		#region ShowGroupSeparators

		public bool ShowGroupSeparators
		{
			get { return fShowGroupSeparators; }
			set { fShowGroupSeparators = value; }
		}
		bool fShowGroupSeparators;

		#endregion

		#region BindToDecimals

		public string BindToDecimals
		{
			get { return fBindToDecimals; }
			set { fBindToDecimals = value; }
		}

		string fBindToDecimals;

		#endregion

		#region ValidateBindToDecimals

		public bool ValidateBindToDecimals
		{
			get
			{
				return validateBindToDecimals;
			}
			set
			{
				validateBindToDecimals = value;
			}
		}

		bool validateBindToDecimals;

		#endregion

		#region Decimals

		[DefaultValue(2), Category("Appearance"), Description("The number of decimal places to display.")]
		public int Decimals
		{
			get { return decimals; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				decimals = value;
			}
		}
		int decimals = ZCalcEditCore.DefaultDecimals;

		#endregion Decimals

		#region BindToCurrencySymbol

		public string BindToCurrencySymbol
		{
			get { return fBindToCurrencySymbol; }
			set { fBindToCurrencySymbol = value; }
		}

		string fBindToCurrencySymbol;

		#endregion
	}
}
