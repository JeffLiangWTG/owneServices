using System.Data;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	public class AWBDocumentPivotCollection : NonPersistentBusinessObjectCollection<AWBDocumentPivot>
	{
		public AWBDocumentPivotCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AWBDocumentPivot(Factory);
		}

		public byte[] ToXmlArray()
		{
			if (Count > 0)
			{
				DataSet data = new DataSet();

				DataTable table = new DataTable("ListTable");
				DataColumn nameColumn = new DataColumn((NoResString)"Name", typeof(string));
				DataColumn titleColumn = new DataColumn((NoResString)"Title", typeof(string));
				DataColumn printedColumn = new DataColumn((NoResString)"Printed", typeof(bool));
				table.Columns.Add(nameColumn);
				table.Columns.Add(titleColumn);
				table.Columns.Add(printedColumn);
				data.Tables.Add(table);

				foreach (AWBDocumentPivot pivot in this)
				{
					DataRow newRow = table.NewRow();
					newRow["Name"] = pivot.Name.ToString();
					newRow["Title"] = pivot.Title.ToString();
					newRow["Printed"] = ((bool)pivot.Printed).ToString();
					table.Rows.Add(newRow);
				}

				MemoryStream xmlStream = new MemoryStream();
				data.WriteXml(xmlStream, XmlWriteMode.IgnoreSchema);
				return xmlStream.ToArray();
			}
			else
			{
				return null;
			}
		}

		public void LoadFromXmlArray(byte[] xmlArray)
		{
			RemoveAll();

			if (xmlArray != null)
			{
				DataSet data = new DataSet();

				using (MemoryStream xmlStream = new MemoryStream(xmlArray))
				{
					data.ReadXml(xmlStream, XmlReadMode.Auto);

					foreach (DataRow row in data.Tables["ListTable"].Rows)
					{
						var newPivot = AddNew();
						newPivot.Title = (string)row["Title"];
						newPivot.Name = (string)row["Name"];
						newPivot.Printed = bool.Parse((string)row["Printed"]);
					}
				}
			}
		}
	}
}
