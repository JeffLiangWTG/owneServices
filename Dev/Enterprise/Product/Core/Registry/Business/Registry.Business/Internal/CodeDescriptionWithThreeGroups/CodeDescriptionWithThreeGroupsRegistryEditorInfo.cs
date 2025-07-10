using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionWithThreeGroupsRegistryEditorInfo : CodeDescriptionWithGroupRegistryEditorInfo
	{
		public CodeDescriptionWithThreeGroupsRegistryEditorInfo(MultilingualString groupColumnCaption,
			MultilingualString group2ColumnCaption,
			MultilingualString group3ColumnCaption,
			MultilingualString extraDescriptionColumnCaption,
			MultilingualString mainDescriptionColumnCaption,
			bool areGroupColumnsVisible,
			bool areOnlyCodeAndGroupColumnsEditable)
			: this(groupColumnCaption, group2ColumnCaption, group3ColumnCaption, extraDescriptionColumnCaption, mainDescriptionColumnCaption, null, areGroupColumnsVisible, areOnlyCodeAndGroupColumnsEditable, false)
		{
		}

		public CodeDescriptionWithThreeGroupsRegistryEditorInfo(MultilingualString groupColumnCaption,
			MultilingualString group2ColumnCaption,
			MultilingualString group3ColumnCaption,
			MultilingualString extraDescriptionColumnCaption,
			MultilingualString mainDescriptionColumnCaption,
			ICondition areGroupColumnsVisibleCondition,
			bool areGroupColumnsVisible,
			bool areOnlyCodeAndGroupColumnsEditable,
			bool isDescriptionColumnTranslatable)
			: base(groupColumnCaption, areGroupColumnsVisible, areOnlyCodeAndGroupColumnsEditable, isDescriptionColumnTranslatable)
		{
			this.group2ColumnCaption = group2ColumnCaption;
			this.group3ColumnCaption = group3ColumnCaption;
			this.extraDescriptionColumnCaption = extraDescriptionColumnCaption;
			AreGroupColumnsVisibleCondition = areGroupColumnsVisibleCondition;
			this.areOnlyCodeAndGroupColumnsEditable = areOnlyCodeAndGroupColumnsEditable;
			AreGroupColumnsVisibleOverride = areGroupColumnsVisible;
			this.mainDescriptionColumnCaption = mainDescriptionColumnCaption;
		}

		public bool AreGroupColumnsVisible => AreGroupColumnsVisibleCondition == null ? AreGroupColumnsVisibleOverride : AreGroupColumnsVisibleCondition.IsMet;

		public string Group2ColumnCaption => group2ColumnCaption ?? string.Empty;

		public string Group3ColumnCaption => group3ColumnCaption ?? string.Empty;

		public string ExtraDescriptionColumnCaption => extraDescriptionColumnCaption ?? string.Empty;

		public string MainDescriptionColumnCaption => mainDescriptionColumnCaption ?? string.Empty;

		public bool AreOnlyCodeAndGroupColumnsEditable => areOnlyCodeAndGroupColumnsEditable && AreGroupColumnsVisible;

		readonly MultilingualString group2ColumnCaption;
		readonly MultilingualString group3ColumnCaption;
		readonly MultilingualString extraDescriptionColumnCaption;
		readonly MultilingualString mainDescriptionColumnCaption;
		internal readonly ICondition AreGroupColumnsVisibleCondition;
		readonly bool AreGroupColumnsVisibleOverride;
		readonly bool areOnlyCodeAndGroupColumnsEditable;

		#region IRegistryEditorInfo Members

		public new Type BaseDataTypeToBeEdited => typeof(CodeDescriptionWithThreeGroupsCollection);

		#endregion
	}
}
