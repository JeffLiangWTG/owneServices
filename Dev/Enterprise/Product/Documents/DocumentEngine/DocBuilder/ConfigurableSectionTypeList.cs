using System.Collections.ObjectModel;

namespace Enterprise.DocumentEngine.DocBuilder
{
	partial class ConfigurableSectionTypeList
	{
		public static readonly ReadOnlyCollection<string> ListInOrder = new ReadOnlyCollection<string>(new string[]
		{
			ConfigurableSectionTypeList.Codes.ConfigSection,
			ConfigurableSectionTypeList.Codes.DocumentHeader,
			ConfigurableSectionTypeList.Codes.PageHeader,
			ConfigurableSectionTypeList.Codes.GenericSection,
			ConfigurableSectionTypeList.Codes.BodySectionExpanding,
			ConfigurableSectionTypeList.Codes.BodySection,
			ConfigurableSectionTypeList.Codes.PageFooter,
			ConfigurableSectionTypeList.Codes.DocumentFooter,
			ConfigurableSectionTypeList.Codes.BackPage,
			ConfigurableSectionTypeList.Codes.EndOfReport,
		});
	}
}
