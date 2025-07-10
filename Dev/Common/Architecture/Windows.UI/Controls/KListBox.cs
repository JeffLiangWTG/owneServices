using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;

namespace CargoWise.Windows.UI
{
	/// <summary><see cref="KListBox"/></summary>
	[DesignTimeControlNameGenerator("lst")]
	[DefaultBindingProperty("SelectedValue")]
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KListBox : ListBox, IBindingMemberForCompileTimeCheckProvider
	{
		public KListBox()
		{
			// Need to override the assignment of the default item height, since it's a windows' hard coded constant
			var defaultItemHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(DefaultItemHeight);
			if (ItemHeight != defaultItemHeight)
			{
				ItemHeight = defaultItemHeight;
			}
		}

		#region SelectedValue / DataSource

		[BindingMetaDataProperty(MetaDataTypes.ListDataSource, "DataSource")]
		[BindingMetaDataProperty(MetaDataTypes.ListValueMember, "ValueMember")]
		[BindingMetaDataProperty(MetaDataTypes.ListDisplayMember, "DisplayMember")]
		public new object SelectedValue
		{
			get { return base.SelectedValue; }
			set { base.SelectedValue = value; }
		}

		[RefreshProperties(RefreshProperties.Repaint)]
		[AttributeProvider(typeof(IListSource))]
		public new object DataSource
		{
			get { return base.DataSource; }
			set { base.DataSource = (value == DBNull.Value) ? null : value; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DataSource = null;
			}
			base.Dispose(disposing);
		}

		#endregion

		#region IBindingMemberForCompileTimeCheckProvider Members

		CompileTimeCheckBindingMemberCollection IBindingMemberForCompileTimeCheckProvider.GetBindingMembersForCompileTimeCheck(
			Type dataSourceType, string dataMember)
		{
			return new ListValueMemberCompileTimeCheckProvider().GetBindingMembersForCompileTimeCheck(dataSourceType, dataMember);
		}

		#endregion

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override int SelectedIndex
		{
			get
			{
				try
				{
					return base.SelectedIndex;
				}
				catch (IndexOutOfRangeException)
				{
					// fixes bug reported in Issue 00851462
					return -1;
				}
			}
			set
			{
				base.SelectedIndex = value;
			}
		}
	}
}
