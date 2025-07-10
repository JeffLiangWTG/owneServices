using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Registry.GUI
{
	public class DepartmentMappingRegistryItemEditor : RegistryItemEditor
	{
		public DepartmentMappingRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			MyDepartmentMappingControl result = new MyDepartmentMappingControl();
			return result;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((MyDepartmentMappingControl)editorPane).FieldValue;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((MyDepartmentMappingControl)editorPane).FieldValue = (string)value;
		}

		public override string GetCustomValidation(Control editorPane)
		{
			return ((MyDepartmentMappingControl)editorPane).GetCustomValidation();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		#region MyDepartmentMappingControl

		public class MyDepartmentMappingControl : DepartmentMappingContainer
		{
			public string FieldValue
			{
				get
				{
					return Data == null ? "" : Data.Mappings.ToString();
				}
				set
				{
					if (Data == null)
					{
						Data = new DepartmentMappingCollectionWrapper(value);
						SetDataBinding(Data, "");
					}
					else
					{
						Data.Mappings.RemoveAll();
						Data.Mappings.Load(value);
					}
				}
			}

			public string GetCustomValidation()
			{
				if (Data != null)
				{
					foreach (DepartmentMapping elem in Data.Mappings)
					{
						elem.ValidateValidDepartment(elem.Dept1CodeInfo);
						elem.ValidateValidDepartment(elem.Dept2CodeInfo);
						if (elem.Dept1CodeInfo.HasErrors())
						{
							return elem.Dept1CodeInfo.GetErrors().GetFirstMessage();
						}
						else if (elem.Dept2CodeInfo.HasErrors())
						{
							return elem.Dept2CodeInfo.GetErrors().GetFirstMessage();
						}
					}
				}
				return "";
			}

			protected DepartmentMappingCollectionWrapper Data;
		}

		#endregion
	}
}
