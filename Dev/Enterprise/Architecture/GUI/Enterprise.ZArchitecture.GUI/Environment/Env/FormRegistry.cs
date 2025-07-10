using System;
using System.Collections.Concurrent;
using System.Data;
using System.Drawing;
using System.IO;
using CargoWise.Windows.UI;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	/// <summary>
	/// Allows storing of Form properties in Data registry
	/// </summary>
	public class FormRegistry
	{
		#region Check cache

		public bool ContainsForm(string formName)
		{
			return cache.ContainsKey(formName);
		}

		#endregion

		#region Get Form Details

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "data set column name")]
		public Rectangle GetFormLocationAndSize(string formName)
		{
			var result = Rectangle.Empty;

			if (!cache.TryGetValue(formName, out result) && FormPosition != null)
			{
				FormPosition.Name = formName;
				var currentUser = EnvProxy.Instance.CurrentUser;
				var formDataBytes = (currentUser != null) ? (byte[])FormPosition.GetValueWithoutFallback(currentUser.PK, Guid.Empty, Guid.Empty) : null;

				if (formDataBytes != null)
				{
					var xmlStream = new MemoryStream(formDataBytes);
					var formData = new DataSet();
					formData.ReadXml(xmlStream, XmlReadMode.Auto);

					if (formData.Tables.Contains("FormTable") && formData.Tables["FormTable"].Rows.Count == 1)
					{
						var formTable = formData.Tables["FormTable"];
						var formRow = formData.Tables["FormTable"].Rows[0];

						if (formTable.Columns.Contains("X"))
						{
							ControlDpiScalingHelper.SetX(ref result, int.Parse(formRow["X"].ToString()), false);
						}

						if (formTable.Columns.Contains("Y"))
						{
							ControlDpiScalingHelper.SetY(ref result, int.Parse(formRow["Y"].ToString()), false);
						}

						if (formTable.Columns.Contains("Width"))
						{
							ControlDpiScalingHelper.SetWidth(ref result, int.Parse(formRow["Width"].ToString()), false);
						}

						if (formTable.Columns.Contains("Height"))
						{
							ControlDpiScalingHelper.SetHeight(ref result, int.Parse(formRow["Height"].ToString()), false);
						}
					}

					AddOrUpdateFormInCache(formName, result);
				}
			}

			return result;
		}

		#endregion

		#region Set Form Details

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "data set column name")]
		public void SetFormLocationAndSize(string formName, Rectangle formRect)
		{
			if (FormPosition != null)
			{
				var currentUser = EnvProxy.Instance.CurrentUser;
				if (currentUser != null)
				{
					var formData = new DataSet();
					var formTable = new DataTable("FormTable");
					formData.Tables.Add(formTable);

					var xColumn = new DataColumn("X", typeof(int));
					var yColumn = new DataColumn("Y", typeof(int));
					var widthColumn = new DataColumn("Width", typeof(int));
					var heightColumn = new DataColumn("Height", typeof(int));
					formTable.Columns.Add(xColumn);
					formTable.Columns.Add(yColumn);
					formTable.Columns.Add(widthColumn);
					formTable.Columns.Add(heightColumn);

					var formRow = formTable.NewRow();
					formRow[xColumn] = formRect.X;
					formRow[yColumn] = formRect.Y;
					formRow[widthColumn] = formRect.Width;
					formRow[heightColumn] = formRect.Height;
					formTable.Rows.Add(formRow);

					FormPosition.Name = formName;

					var xmlStream = new MemoryStream();
					formData.WriteXml(xmlStream, XmlWriteMode.IgnoreSchema);

					FormPosition.SetValue(currentUser.PK, Guid.Empty, Guid.Empty, xmlStream.ToArray());

					AddOrUpdateFormInCache(formName, formRect);
				}
			}
		}

		#endregion

		#region Clear Form Details

		public void ClearFormLocationAndSize(string formName)
		{
			SetFormLocationAndSize(formName, Rectangle.Empty);
			Rectangle form;
			cache.TryRemove(formName, out form);
		}

		#endregion

		#region Implementation

		readonly ConcurrentDictionary<string, Rectangle> cache = new ConcurrentDictionary<string, Rectangle>();

		void AddOrUpdateFormInCache(string formName, Rectangle form)
		{
			cache.AddOrUpdate(formName, form, (key, oldForm) => form);
		}

		protected virtual IRegistryItem FormPosition
		{
			get
			{
				var registry = EnvProxy.Instance.Registry;
				return (registry == null) ? null : registry.RawRegistry.FormPosition;
			}
		}

		#endregion
	}
}
