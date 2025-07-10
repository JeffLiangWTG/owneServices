using System.Windows.Forms;
using Enterprise.Registry.Business.Internal;

namespace Enterprise.Registry.GUI
{
	public partial class StringArrayControl : RegistryZUserControl
	{
		public StringArrayControl(CharacterCasing casing)
		{
			InitializeComponent();
			DataSourceType = typeof(StringLineCollection);
			zTextBoxColumnStyleInfo1.CharacterCasing = casing;
		}

		public StringArrayControl() : this(CharacterCasing.Normal) { }

		public StringLineCollection BusinessEntity
		{
			get { return (StringLineCollection)BindingSource.Current; }
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			StringGrid.ReadOnly = readOnly;
		}
	}
}
