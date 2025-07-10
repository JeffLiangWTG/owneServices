using System.ComponentModel;
using System.Drawing.Design;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using Enterprise.ZArchitecture.Design;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture
{
	public abstract class BindToPropertyAttributes
	{
		[SmartTagVisible]
		[Editor(DesignerTypes.BindingMemberEditor, typeof(UITypeEditor)), BindingMemberEditor("BindingSourceDataSourceType", "BindingMemberTypeFilterForEditor")]
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Category(ZGUIConstants.DesignerCategory)]
		[RefreshProperties(RefreshProperties.All)] // this is to force the .ReadOnly value to update in the designer
		[DefaultValue("")]
		public abstract string BindTo { get; }

		[SmartTagVisible]
		[Editor(DesignerTypes.BindingMemberEditor, typeof(UITypeEditor)), BindingMemberEditor("BindingSourceDataSourceType", "BindingMemberTypeFilterForEditor")]
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Category(ZGUIConstants.DesignerCategory)]
		[RefreshProperties(RefreshProperties.All)] // this is to force the .ReadOnly value to update in the designer
		[DefaultValue("")]
		public abstract string BindToList { get; }
	}

	public static class ZGUIConstants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		public const string BindToPropertyAttributes = "Enterprise.ZArchitecture.BindToPropertyAttributes, Enterprise.ZArchitecture.GUI";
		public const string KDesignerCategory = "(K-Architecture)";
		public const string DesignerCategory = "(Z-Architecture)";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		public const string FilterDesignerCategory = "(Z-Architecture Filter)";

		// if you add type names, please add them to the test below to ensure renaming of types doesn't break it
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		public const string ColumnStyleEditor = "Enterprise.ZArchitecture.GUI.Design.ZGridColumnStyleEditor, Enterprise.ZArchitecture.GUI";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		public const string GridDesigner = "Enterprise.ZArchitecture.GUI.Design.ZGridDesigner, Enterprise.ZArchitecture.GUI";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		public const string ButtonGridDesigner = "Enterprise.ZArchitecture.GUI.Design.ZButtonGridDesigner, Enterprise.ZArchitecture.GUI";
		public static string GetQueryTooComplicatedError() => Res.GetString("e9ac607e-b6b6-4d0f-a3b1-d5b1979b89a2", "Your query is too complicated, please simplify your search conditions.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		public static string GetReadOnlyError(string bindToFullPath)
		{
			return string.Format(

				"ReadOnly should not be set in the designer for bound controls. " + NewLine +
				"Please add the following to your business object's constructor instead: " + NewLine + NewLine +
				"   {0}Info.ReadOnly = true;" + NewLine + NewLine +
				"(Properties with no Set() method are automatically ReadOnly)",

				new KBindingMemberInfo(bindToFullPath).BindingField);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		public static string GetPropertyInfoDoesNotExistError(string propertyName)
		{
			return string.Format(
				"The property '{0}' does not have a corresponding ZPropertyInfo object.",
				propertyName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		public static string GetDropDownEditDoesNotHaveBindToListError(string controlName)
		{
			return string.Format(

				"Cannot bind to the list for control or grid column '{0}'. Please set the {0}.BindToList property in the designer.",
				controlName);
		}

		#region Implementation

		const string NewLine = "\r\n";

		#endregion
	}
}
