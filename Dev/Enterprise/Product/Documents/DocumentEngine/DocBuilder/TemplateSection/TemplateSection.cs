using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class TemplateSection : NonPersistentBusinessObject
	{
		public static class Schema
		{
			public const string Category = "Category";
			public const string TypeCode = "TypeCode";
			public const string SectionName = "SectionName";
			public const string Language = "Language";
			public const string StartingRowNumber = "StartingRowNumber";
			public const string RowCount = "RowCount";
			public const string TemplateName = "TemplateName";
			public const string FullLabel = "FullLabel";
		}

		public TemplateSection(TemplateSection originalSection, int startingRowNumber, int rowCount)
		{
			this.category = originalSection.category;
			this.typeCode = originalSection.typeCode;
			this.sectionName = originalSection.sectionName;
			this.startingRowNumber = startingRowNumber;
			this.rowCount = (this.typeCode == ConfigurableSectionTypeList.Codes.EndOfReport ? 0 : rowCount);
			this.templateName = originalSection.templateName;
			this.fullLabel = originalSection.fullLabel;
		}

		public TemplateSection(ZString parameterText, int startingRowNumber, int rowCount, string templateName = "", string fullLabel = "", string language = Core.Constants.Languages.English)
		{
			var parameters = ParameterSplitter.Match(parameterText);

			this.category = parameters.Groups[Schema.Category].Value.Trim();
			this.typeCode = parameters.Groups[Schema.TypeCode].Value.ToUpperInvariant();
			this.sectionName = parameters.Groups[Schema.SectionName].Value.Trim();
			this.templateName = templateName;
			this.fullLabel = fullLabel;
			this.language = language;

			if (this.sectionName.IsEmpty && !this.typeCode.IsEmpty)
			{
				this.sectionName = new ConfigurableSectionTypeList().GetDescriptionFromCode(this.typeCode);
			}

			this.startingRowNumber = startingRowNumber;
			this.rowCount = (this.typeCode == ConfigurableSectionTypeList.Codes.EndOfReport ? 0 : rowCount);
		}

		readonly ZString category;
		readonly ZString typeCode;
		readonly ZString sectionName;
		readonly ZInt rowCount;
		readonly ZString templateName;
		readonly ZString fullLabel;

		ZInt startingRowNumber;
		ZString language;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's a -ing regex.")]
		const string ParameterSplitRegex = @"^(?<TypeCode>[A-Z][A-Z][A-Z])(|\:(?<Category>[^,]+))[\s]*(|,(?<SectionName>.+))$";

		static Regex ParameterSplitter
		{
			get { return parameterSplitter ?? (parameterSplitter = new Regex(ParameterSplitRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.CultureInvariant)); }
		}

		[ThreadStatic]
		static Regex parameterSplitter;

		#region Category

		public ZString Category
		{
			get { return category; }
		}

		public ZPropertyInfo CategoryInfo
		{
			get { return GetZPropertyInfo(Schema.Category); }
		}

		#endregion

		#region TypeCode

		public ZString TypeCode
		{
			get { return typeCode; }
		}

		public ZPropertyInfo TypeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.TypeCode); }
		}

		#endregion

		#region SectionName

		public ZString SectionName
		{
			get { return sectionName; }
		}

		public ZPropertyInfo SectionNameInfo
		{
			get { return GetZPropertyInfo(Schema.SectionName); }
		}

		#endregion

		#region TemplateName

		public ZString TemplateName
		{
			get { return templateName; }
		}

		public ZPropertyInfo TemplateNameInfo
		{
			get { return GetZPropertyInfo(Schema.TemplateName); }
		}

		#endregion

		#region FullLabel

		public ZString FullLabel
		{
			get { return fullLabel; }
		}

		public ZPropertyInfo FullLabelInfo
		{
			get { return GetZPropertyInfo(Schema.FullLabel); }
		}

		#endregion

		#region Language

		[CargoWise.ComponentModel.MaxLength(7)]
		public ZString Language
		{
			get { return language; }
			set { SetNonPersistentPropertyValue<ZString>(LanguageInfo, ref language, value); }
		}

		public ZPropertyInfo LanguageInfo
		{
			get { return GetZPropertyInfo(Schema.Language); }
		}

		#endregion

		#region StartingRowNumber

		/// <summary>
		/// One based Starting Row number for this section. (matches row numbers showing in Excel)
		/// </summary>
		public ZInt StartingRowNumber
		{
			get { return startingRowNumber; }
		}

		public ZPropertyInfo StartingRowNumberInfo
		{
			get { return GetZPropertyInfo(Schema.StartingRowNumber); }
		}

		#endregion

		#region RowCount

		/// <summary>
		/// Row Count for contents of section. (count does not include the header row because it's removed)
		/// </summary>
		public ZInt RowCount
		{
			get { return rowCount; }
		}

		public ZPropertyInfo RowCountInfo
		{
			get { return GetZPropertyInfo(Schema.RowCount); }
		}

		#endregion

		internal void MoveStartingRowTo(int destinationRowNumber)
		{
			startingRowNumber = destinationRowNumber;
		}

		internal bool IsControlSection
		{
			get { return (TypeCode == ConfigurableSectionTypeList.Codes.ConfigSection) || (TypeCode == ConfigurableSectionTypeList.Codes.EndOfReport); }
		}

		internal bool IsGenericSectionType
		{
			get { return TypeCode == ConfigurableSectionTypeList.Codes.GenericSection; }
		}

		/// <summary>
		/// One based Last Row number for this section. (matches row numbers showing in Excel)
		/// </summary>
		internal int LastRowNumber
		{
			get { return StartingRowNumber + RowCount; }
		}

		public TemplateSectionValidation Validation
		{
			get { return new TemplateSectionValidation(this); }
		}

		public override string ToString()
		{
			return string.Format(
				(NoResString)"{0} / {1} - Starting: {2} Rows: {3} Ending: {4}",
				TypeCode,
				SectionName,
				StartingRowNumber,
				RowCount,
				LastRowNumber);
		}
	}
}
