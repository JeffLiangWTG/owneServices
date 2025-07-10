using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
#if DEBUG
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
#endif
	public class ZGuidDropEdit : ZDropEdit
	{
		#region Format / Parse

		protected override void OnFormatValue(ConvertEventArgs e)
		{
			if (DataSource != null)
			{
				InvalidateList();
				var zGuidValue = (ZGuid)e.Value;
				if (zGuidValue.IsValid)
				{
					var selectedItem = List?.OfType<ICodeDescription>().FirstOrDefault(x => x.PK != null && x.PK.Equals(e.Value));
					if (selectedItem != null)
					{
						SetSelection(selectedItem, 0);
					}
				}
				e.Value = GetCode(zGuidValue);
			}
			else
			{
				e.Value = "";
			}
		}

		protected override void OnParseValue(ConvertEventArgs e)
		{
			var value = GetPK(e.Value.ToString());

			if (!value.IsValid)
			{
				FieldInvalidTextMemory.SetInvalidText(CurrentItem, DataPropertyName, Text);
			}

			e.Value = value;
		}

		#endregion

		#region GetPK / GetCode

		public ZGuid GetPK(string code)
		{
			return GetPK(code, List?.OfType<ICodeDescription>());
		}

		public ZGuid GetPK(string code, System.Collections.Generic.IEnumerable<ICodeDescription> list)
		{
			var result = ZGuid.Empty;

			if (!string.IsNullOrEmpty(code) && list != null)
			{
				if (LastSelectedItem != null && list.Contains(LastSelectedItem) && code.Equals(LastSelectedItem.Code, StringComparison.CurrentCultureIgnoreCase))
				{
					var identifiedItem = LastSelectedItem as IIdentified;
					if (identifiedItem != null && identifiedItem.Identifier.IsValid)
					{
						return identifiedItem.Identifier;
					}
				}

				foreach (var element in list)
				{
					if (code.Equals(element.Code, StringComparison.CurrentCultureIgnoreCase))
					{
						return (ZGuid)element.PK;
					}
				}

				result = ZGuid.Invalid; // code not found, must be invalid
			}

			return result;
		}

		public string GetCode(ZGuid pK)
		{
			var result = "";

			if (pK.IsValid)
			{
				if (List != null)
				{
					foreach (ICodeDescription element in List)
					{
						if (element.PK != null && (ZGuid)element.PK == pK)
						{
							return element.Code;
						}
					}
				}
			}
			else if (!pK.IsEmpty)
			{
				result = FieldInvalidTextMemory.GetInvalidText(CurrentItem, DataPropertyName);
			}

			return result;
		}

		#endregion

		#region UpdateSelection

		protected override void UpdateSelection()
		{
			base.UpdateSelection();
			if (string.IsNullOrEmpty(Text))
			{
				var currentItem = this.CurrentItem;
				if (currentItem != null)
				{
					var property = TypeDescriptor.GetProperties(currentItem)[DataPropertyName];
					var pK = (ZGuid)property.GetValue(currentItem);
					Text = GetCode(pK);
				}
			}
		}

		#endregion
	}
}
