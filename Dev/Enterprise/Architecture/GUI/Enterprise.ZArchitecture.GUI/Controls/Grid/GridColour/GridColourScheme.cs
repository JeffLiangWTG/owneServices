using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Controls.Grid.GridColour;
using ResString = Enterprise.ZArchitecture.GUI.ResString;

namespace Enterprise.ZArchitecture
{
	public class GridColourScheme : StmModuleFilter
	{
		public GridColourScheme(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Strip Management

		public void SetStripsFromFilter(FilterStripBusinessObject filterStripBusinessObject, Type businessEntityType, bool editable = false)
		{
			ColourStrips.Clear();
			var stripBase = new GridColourStripBusinessObject(filterStripBusinessObject, this, businessEntityType, editable);

			foreach (var rule in StripColours.Values)
			{
				var strip = (GridColourStripBusinessObject)stripBase.Clone();
				using (strip.GetValidationSuspender())
				{
					strip.RuleName = rule.RuleName;
					strip.BGColor = rule.Color;
					strip.LoadLayout(strip.StmModuleFilter, disableValidation: true, shouldReset: false);

					ColourStrips.Add(strip);
					strip.LayoutChanged += MyScheme_HasChanges;
				}
				this.Factory.ChildFactories.Add(strip.Factory);
			}
		}

		#endregion

		public void MyScheme_HasChanges(object sender, EventArgs e)
		{
			HasChanges = true;
		}

		#region Properties

		#region S9_IsPublished

		public override ZBool S9_IsPublished
		{
			get { return base.S9_IsPublished; }
			set
			{
				base.S9_IsPublished = value;

				if (!value)
				{
					PublishAcrossAllCompanies = false;
				}
			}
		}

		#endregion

		#region IsPublishedAcrossAllCompanies

		public ZBool PublishAcrossAllCompanies
		{
			get
			{
				return S9_GC.IsEmpty;
			}
			set
			{
				S9_GC = value ? ZGuid.Empty : EnvProxy.Instance.CurrentCompany.PK;
				PublishAcrossAllCompaniesInfo.RefreshBinding();
				((GridColourSchemeValidation)Validation).ValidatePublishAcrossAllCompanies();
			}
		}

		public ZPropertyInfo PublishAcrossAllCompaniesInfo
		{
			get { return GetZPropertyInfo(nameof(PublishAcrossAllCompanies)); }
		}

		protected bool PublishAcrossAllCompanies_ReadOnly
		{
			get { return !S9_IsPublished; }
		}

		#endregion

		#region S9_FilterName

		[ResourceStringData("6ED33D1B-D7BF-4845-AB5B-0D9DE64EB5B7", Caption = "Filter Name", FullDescription = "Rule name of Grid Color Scheme")]
		[GridColourSchemeTranslatableDataField(Schema.TableName, Schema.S9_FilterName, @"Database\Odyssey\Data\Public\StmModuleFilter\StmModuleFilter.xml;Database\Odyssey\Data\Public\TagRule\TagRule.xml", Schema.S9_ModuleID, MaxLength = Schema.S9_FilterNameMaxLength, Asmid = ResString.AssemblyId)]
		public override ZString S9_FilterName
		{
			get { return base.S9_FilterName; }
			set { base.S9_FilterName = value; }
		}

		#endregion

		#endregion

		#region Save / Load

		protected override void OnFactorySaving()
		{
			// insert color information into S9_FilterData
			if (ColourStrips != null && ColourStrips.Count > 0)
			{
				using (var colourStripsTable = GetRuleRelationTable())
				using (var colourStripsDataSet = new DataSet())
				using (var stream = new MemoryStream())
				{
					colourStripsDataSet.Locale = CultureInfo.InvariantCulture;
					// populate datatable
					ColourStrips.ForEach(gridColor => colourStripsTable.Rows.Add(gridColor.StmModuleFilter?.PK ?? ZGuid.Empty, gridColor.RuleName, gridColor.BGColor.ToArgb()));
					// serialize datatable through dataset
					colourStripsDataSet.Tables.Add(colourStripsTable);
					colourStripsDataSet.WriteXml(stream, XmlWriteMode.IgnoreSchema);
					stream.Position = 0;
					this.S9_FilterData = stream.ToArray();
				}
			}

			base.OnFactorySaving();
		}

		public override void OnSaving()
		{
			if (!S9_IsSystem)
			{
				S9_GC = PublishAcrossAllCompanies ? ZGuid.Empty : EnvProxy.Instance.CurrentCompany.PK;
				if (S9_RelatedEntityID.IsEmpty)
				{
					S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				}
			}

			base.OnSaving();
		}

		public override void OnLoaded()
		{
			ResetStrips();
			base.OnLoaded();
		}

		#endregion

		#region Validation

		protected override StmModuleFilterValidation GetNewValidation()
		{
			return new GridColourSchemeValidation(this);
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Content of the message in the exception., Message for inner exception")]
		public void ResetStrips()
		{
			var filterData = S9_FilterData;

			if (filterData.Length > 0)
			{
				try
				{
					using (var stream = new MemoryStream(filterData))
					using (var colourStripsDataSet = new DataSet { Locale = CultureInfo.InvariantCulture })
					{
						StripColours.Clear();

						colourStripsDataSet.ReadXml(stream, XmlReadMode.Auto);
						if (VerifyColourStripsTableIntegrity(colourStripsDataSet))
						{
							foreach (DataRow row in colourStripsDataSet.Tables[0].Rows)
							{
								var id = row[TableColumnPK].ToString();
								var rgb = Convert.ToInt32(row[TableColumnBGColor], CultureInfo.InvariantCulture);
								var bgColor = Color.FromArgb(rgb);
								var ruleName = row[TableColumnRuleName].ToString();

								if (!StripColours.ContainsKey(id))
								{
									StripColours.Add(id, new StripColourInfo(bgColor, ruleName));
								}
							}
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex is ArgumentException && ex.Message.Contains("does not belong to table ColourStrips."))
					{
						var exceptionMessageBuilder = new StringBuilder(string.Format(CultureInfo.InvariantCulture, "The Color Scheme: {0} is probably corrupted. The column names in table ColourStrips are: ", S9_FilterName), 500);
						using (var exceptionStream = new MemoryStream(filterData))
						using (var exceptionDataSet = new DataSet { Locale = CultureInfo.InvariantCulture })
						{
							exceptionDataSet.ReadXml(exceptionStream, XmlReadMode.Auto);
							foreach (var dataColumn in exceptionDataSet.Tables[0].Columns)
							{
								exceptionMessageBuilder.AppendFormat(CultureInfo.InvariantCulture, "\'{0}\' ", dataColumn.ToString());
							}
						}

						ex = new ArgumentException(ex.Message, new ArgumentException(exceptionMessageBuilder.ToString()));
					}

					Globals.Message.ShowDeveloperException(ex);
				}
			}
		}

		static bool VerifyColourStripsTableIntegrity(DataSet colourStripsDataSet)
		{
			if (colourStripsDataSet?.Tables == null)
			{
				return false;
			}

			var columns = colourStripsDataSet.Tables[0].Columns;
			return columns.Contains(TableColumnPK) && columns.Contains(TableColumnBGColor) && columns.Contains(TableColumnRuleName);
		}

		public readonly List<GridColourStripBusinessObject> ColourStrips = new List<GridColourStripBusinessObject>();

#if DEBUG
		internal
#endif
		readonly Dictionary<string, StripColourInfo> StripColours = new Dictionary<string, StripColourInfo>();

		internal struct StripColourInfo
		{
			public StripColourInfo(Color color, string ruleName)
			{
				Color = color;
				RuleName = ruleName;
			}

			public readonly Color Color;
			public readonly string RuleName;
		}

		#region XML Table

		DataTable GetRuleRelationTable()
		{
			var ruleRelationTable = new DataTable(XMLTableName);
			ruleRelationTable.Columns.Add(TableColumnPK, typeof(string));
			ruleRelationTable.Columns.Add(TableColumnRuleName, typeof(string));
			ruleRelationTable.Columns.Add(TableColumnBGColor, typeof(int));
			return ruleRelationTable;
		}

		const string TableColumnRuleName = "RuleName";
		const string TableColumnBGColor = "BGColor";
		const string TableColumnPK = "RulePK";
		const string XMLTableName = "ColourStrips";

		#endregion

		#endregion
	}
}
