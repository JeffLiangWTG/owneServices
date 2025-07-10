using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.ComponentModel.Design;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZTimeZoneFindBox : ZPopupFindBox
	{
		public ZTimeZoneFindBox()
		{
			InitializeComponent();
			CodeBox.Visible = false;
			CodeBox.TextChanged += delegate
			{
				OffsetValue = Env.Time.GetUtcOffsetBasedOnUtc(Text, Env.Time.CurrentUtcDateTime);
				OnTextChanged(EventArgs.Empty);
			};
			ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
		}

		#region Properties

		[Browsable(false)]
		[DefaultValue(false)]
		public bool AllowTemplateRecords { get; set; }

		[Browsable(false)]
		public TimeSpan OffsetValue
		{
			get
			{
				TimeSpan? offset;
				var isValidOffset = TryParseUTCFormattedOffsetToTimeSpan(TimeZoneComboBox.Text, out offset);
				offsetValue = isValidOffset ? (TimeSpan)offset : TimeSpan.Zero;
				return offsetValue;
			}
			set
			{
				offsetValue = value;
				TimeZoneComboBox.Text = TimeSpanOffsetToUTCFormattedString(value); 
			}
		}
		TimeSpan offsetValue;

		#endregion

		#region BindToForDropEdit

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public virtual string BindToForDropEdit
		{
			get { return bindToForDropEdit; }
			set { bindToForDropEdit = value; }
		}
		string bindToForDropEdit = "";

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			TimeZoneComboBox.DataBindings.RemoveBinding(nameof(Text));
			DataBindings.RemoveBinding(nameof(List));

			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null && !this.IsDesignMode())
			{
				var codeBinding = new KBinding(nameof(Text), dataSource, dataMember, true);
				codeBinding.Parse += new ConvertEventHandler(OnParseValue);
				codeBinding.Format += new ConvertEventHandler(OnFormatValue);
				codeBinding.BindingComplete += new BindingCompleteEventHandler(OnBindingComplete);
				TimeZoneComboBox.DataBindings.Add(codeBinding);
			}
		}

		public override Type DataSourceType
		{
			get { return typeof(ZString); }
		}

		protected override bool RequiresListDataBinding
		{
			get { return true; }
		}

		void OnParseValue(object sender, ConvertEventArgs e)
		{
			OnParseValue(e);
		}

		void OnFormatValue(object sender, ConvertEventArgs e)
		{
			OnFormatValue(e);
		}

		void OnBindingComplete(object sender, BindingCompleteEventArgs e)
		{
			if (e.BindingCompleteContext == BindingCompleteContext.DataSourceUpdate)
			{
				e.Binding.ReadValue();
			}
		}

		protected virtual void OnParseValue(ConvertEventArgs e)
		{
		}

		protected virtual void OnFormatValue(ConvertEventArgs e)
		{
		}

		#endregion

		#region IBindingMemberForCompileTimeCheckProvider Members

		protected override CompileTimeCheckBindingMemberCollection GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			var result = new CompileTimeCheckBindingMemberCollection();
			result.AddRange(base.GetBindingMembersForCompileTimeCheck(dataSourceType, dataMember));
			if (!string.IsNullOrEmpty(BindToForDropEdit))
			{
				result.Add(new CompileTimeCheckBindingMember(dataSourceType, typeof(string), BindingHelper.GetNestedControlDataMember(BindTo, dataMember, BindToForDropEdit)));
			}
			return result;
		}

		#endregion

		#region Implementation

		#region TimeZoneOffsets

		public CodeDescriptionPairList TimeZoneOffsets
		{
			get { return timeZoneOffsets ?? (timeZoneOffsets = GetTimeZoneOffsetsList()); }
		}
		CodeDescriptionPairList timeZoneOffsets;

		CodeDescriptionPairList GetTimeZoneOffsetsList()
		{
			var result = new CodeDescriptionPairList();
			var offsets = Enterprise.Environment.Env.Time.TimeZoneOffsets;
			foreach (var offset in offsets)
			{
				result.AddPair((NoResString)TimeSpanOffsetToUTCFormattedString(offset));
			}
			return result;
		}

		string TimeSpanOffsetToUTCFormattedString(TimeSpan offset)
		{
			string sign = offset.Hours >= 0 ? "+" : "-";
			int hours = offset.Hours > 0 ? offset.Hours : -offset.Hours;
			int minutes = offset.Minutes > 0 ? offset.Minutes : -offset.Minutes;

			return $"GMT {sign}{hours:D2}:{minutes:D2}";
		}

		bool TryParseUTCFormattedOffsetToTimeSpan(string inputText, out TimeSpan? offset)
		{
			offset = null;
			string pattern = (NoResString)@"GMT\s+([+-])(\d{2}):(\d{2})";
			Match match = Regex.Match(inputText, pattern);
			if (match.Success)
			{
				int sign = match.Groups[1].Value.Equals("+") ? 1 : -1;
				int hours = int.Parse(match.Groups[2].Value) * sign;
				int minutes = int.Parse(match.Groups[3].Value) * sign;
				offset = new TimeSpan(hours, minutes, 0);
				return true;
			}
			return false;
		}

		#endregion

		protected override bool AutoCompleteText(bool explicitAutoComplete) { return false; }

		protected override EmbeddedModulePopup CreateEmbeddedPopup(ZFilterModule module)
		{
			if (module is ZFilterGridModule filterGridModule)
			{
				filterGridModule.AllowLoadTemplateRecords = AllowTemplateRecords;
			}

			return base.CreateEmbeddedPopup(module);
		}
		#endregion
	}
}
