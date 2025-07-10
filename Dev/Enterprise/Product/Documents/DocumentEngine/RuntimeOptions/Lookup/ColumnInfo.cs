using System;
using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.DocBuilder;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public enum ColumnTypes
	{
		ZCalcEditColumnStyleInfo,
		ZCheckBoxColumnStyleInfo,
		ZDateEditColumnStyleInfo,
		ZMultiLineTextBoxColumnInfo,
		ZTextBoxColumnStyleInfo,
		ZGuidFindBoxColumnStyleInfo,
		ZCodeFindBoxColumnStyleInfo,
		ZDropEditColumnStyleInfo,
		ZGuidDropEditColumnStyleInfo,
		ZOrganisationFindBoxColumnStyleInfo
	}

	[Serializable]
	public class ColumnInfo
	{
		public ColumnInfo(string caption)
		{
			Caption = caption;
		}

		public string Caption { get; private set; }

		public string CaptionLocalized
		{
			get
			{
				return DocBuilderResourceStrings.GetTranslationString(CaptionLocalizedData, Caption);
			}
		}

		public ResourceStringData CaptionLocalizedData
		{
			get { return fCaptionLocalizedData; }
			set { fCaptionLocalizedData = value; }
		}
		ResourceStringData fCaptionLocalizedData;

		public ColumnTypes ColumnType { get; set; }
		public string ColumnName { get; set; }

		public void AddProperty(string name, string value)
		{
			Properties.Add(name, value);
		}

		public Dictionary<string, string> Properties = new Dictionary<string, string>();
	}
}
