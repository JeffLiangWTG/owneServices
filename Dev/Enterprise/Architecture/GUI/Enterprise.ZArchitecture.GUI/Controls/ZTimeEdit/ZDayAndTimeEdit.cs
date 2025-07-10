using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Internal;
using static Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Supports 3 digit days (ie format ddd) or 2 digit hours (ie format hh:mm)
	/// </summary>
	[ToolboxItem(true)]
	public class ZDayAndTimeEdit : ZTimeEdit
	{
		#region Bare

		[ToolboxItem(false)]
		public new class Bare : ZDayAndTimeEdit
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		public ZDayAndTimeEdit()
		{
			TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;
		}

		[DefaultValue(ContainerPenaltyTimeUnit.Codes.Days)]
		public string TimeUnit
		{
			get
			{
				return timeUnit;
			}
			set
			{
				timeUnit = value;
				((ZDayAndTimeEditCore)Core).TimeUnit = value;
			}
		}
		string timeUnit;

		public bool IsDayUnit => TimeUnit == ContainerPenaltyTimeUnit.Codes.Days;

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToTimeUnit))]
		public string BindToTimeUnit { get; set; }

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				if (!string.IsNullOrEmpty(BindToTimeUnit))
				{
					BindTimeUnit(dataSource, BindToTimeUnit);
				}
			}
			base.SetDataBinding(dataSource, dataMember);
		}

		protected virtual void BindTimeUnit(object dataSource, string dataMember)
		{
			DataBindings.RemoveBinding(nameof(TimeUnit)); // Programmatic constant
			if (dataSource != null)
			{
				Binding binding = new KBinding(nameof(TimeUnit), dataSource, dataMember); // Programmatic constant
				DataBindings.Add(binding);
			}
		}

		#region Implementation

		internal override ZTimeEditCore GetNewCore()
		{
			return new ZDayAndTimeEditCore(TimeUnit, this);
		}

		#endregion
	}
}
