using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZMultiColumnCheckBoxSelection : CompositeControl, ISelfBindingWebControl, IBindToList
	{
		#region Constructors

		public ZMultiColumnCheckBoxSelection()
		{
		}

		#endregion

		#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(MetadataHelper.GetListMember(this, dataSource)) && dataSource != null;
		}

		public void Bind(object dataSource)
		{
			CheckBoxes.Clear();

			string listMember = MetadataHelper.GetListMember(this, dataSource);
			object collectionObject = ZPropertyAccessor.Get(dataSource, listMember);
			if (collectionObject != null)
			{
				if (collectionObject is ICodeDescriptionBoolList)
				{
					ICodeDescriptionBoolList codeDescriptionList = collectionObject as ICodeDescriptionBoolList;
					if (codeDescriptionList != null)
					{
						foreach (ICodeDescriptionBool pair in codeDescriptionList)
						{
							AddCheckBox(pair);
						}
					}
				}
			}

			CreateChildControls();
		}

		public void UnBind()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IBindToList Members

		[DefaultValue("")]
		public string BindToList
		{
			get
			{
				return fBindToList;
			}
			set
			{
				fBindToList = value;
			}
		}
		string fBindToList;

		#endregion

		#region IBindTo Members

		[DefaultValue("")]
		public string BindTo
		{
			get
			{
				return fBindTo;
			}
			set
			{
				fBindTo = value;
			}
		}
		string fBindTo;

		#endregion

		#region Properties

		[DefaultValue("1")]
		public int Columns
		{
			get
			{
				return fColumns;
			}
			set
			{
				fColumns = value;
			}
		}
		int fColumns;

		#endregion

		#region Overrides

		protected override void CreateChildControls()
		{
			Controls.Clear();
			if (Columns > 0)
			{
				int currentColumn = 0;
				Table resultTable = new Table();
				resultTable.CssClass = CssClass;
				TableRow row = new TableRow();
				resultTable.Rows.Add(row);
				foreach (ZCheckBox checkBox in CheckBoxes)
				{
					TableCell cell = new TableCell();
					cell.Controls.Add(checkBox);
					row.Cells.Add(cell);
					currentColumn++;
					if (currentColumn >= Columns)
					{
						row = new TableRow();
						resultTable.Rows.Add(row);
						currentColumn = 0;
					}
				}

				Controls.Add(resultTable);
			}
		}

		#endregion

		#region Implementation

		protected ZCheckBox AddCheckBox(ICodeDescriptionBool codeDescription)
		{
			ZCheckBox newCheckBox = new ZCheckBox();
			newCheckBox.Text = codeDescription.Description;
			newCheckBox.ID = codeDescription.Code;
			CheckBoxes.Add(newCheckBox);
			newCheckBox.BindTo = "Bool";
			newCheckBox.Bind(codeDescription);
			return newCheckBox;
		}

		protected List<ZCheckBox> CheckBoxes
		{
			get
			{
				if (fCheckBoxes == null)
				{
					fCheckBoxes = new List<ZCheckBox>();
				}
				return fCheckBoxes;
			}
		}

		List<ZCheckBox> fCheckBoxes;

		#endregion
	}
}
