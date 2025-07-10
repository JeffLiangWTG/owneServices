using System.ComponentModel;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class WebAccessControl : RegistryZUserControl
	{
		public WebAccessControl()
		{
			InitializeComponent();

			CheckBoxGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
			{
				Caption = (NoResString)"Organization Role",
				ColumnName = (NoResString)"Role",
				IsMandatory = true,
				IsReadOnly = true
			});

#if DEBUG
			TypeDescriptor.AddAttributes(CheckBoxGrid, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		public WebAccessControl(AccessRulesBase accessRules) : this()
		{
			foreach (string caption in accessRules.GetCaptions())
			{
				CheckBoxGrid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo
				{
					Caption = caption,
					ColumnName = accessRules.GetPropertyName(caption),
					IsMandatory = true,
				});
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CheckBoxGrid.ReadOnly = readOnly;
		}
	}
}
