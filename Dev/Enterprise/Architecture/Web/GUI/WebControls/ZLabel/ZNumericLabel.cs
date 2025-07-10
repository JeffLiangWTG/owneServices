using System;
using System.ComponentModel;
using System.Web.UI;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Label to display numeric e values
	/// </summary>
	[DefaultProperty("Text"), ToolboxData("<{0}:ZNumericLabel runat=server></{0}:ZNumericLabel>")]
	public class ZNumericLabel : ZLabelBase
	{
		#region IBindTo Members

		public override void Bind(object dataSource)
		{
			if (!string.IsNullOrEmpty(BindToDecimals))
			{
				Decimals = int.Parse(ZPropertyAccessor.Get(dataSource, BindToDecimals).ToString());
			}
			base.Bind(dataSource);
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToDecimals
		{
			get { return fBindToDecimals; }
			set
			{
				fBindToDecimals = value;

				if (!string.IsNullOrEmpty(value))
				{
					Decimals = ZCalcEditCore.DefaultDecimals;
				}
			}
		}

		protected string fBindToDecimals;

		#endregion

		#region Decimals

		[DefaultValue(2), Category("Appearance"), Description("The number of decimal places to display.")]
		public virtual int Decimals
		{
			get
			{
				object obj1 = this.ViewState["Decimals"];
				if (obj1 != null)
				{
					return (int)obj1;
				}
				return 2;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}
				this.ViewState["Decimals"] = value;
			}
		}
		protected int fDecimals;

		#endregion Decimals

		#region ShowGroupSeparators

		[DefaultValue(false)]
		public bool ShowGroupSeparators
		{
			get { return fShowGroupSeparators; }
			set { fShowGroupSeparators = value; }
		}
		bool fShowGroupSeparators;

		#endregion

		protected internal override string GetText(IZType value)
		{
			if (value is INumericZType)
			{
				return (value is ZDecimal) ?
					FormatNumber(value, Decimals) :
					FormatNumber(value, 0);
			}
			return "";
		}

		protected virtual string FormatNumber(object number, int decimals)
		{
			return ShowGroupSeparators
					? Core.Utilities.FormatNumberWithGroupSeparators(number.ToString(), decimals, Shared.WebEnvShared.ClientCulture)
					: Core.Utilities.FormatNumber(number.ToString(), decimals, Shared.WebEnvShared.ClientCulture);
		}

		#region Internal Properties

		internal string FormatNumberInternal(object number, int decimals) => FormatNumber(number, decimals);

		#endregion
	}
}
