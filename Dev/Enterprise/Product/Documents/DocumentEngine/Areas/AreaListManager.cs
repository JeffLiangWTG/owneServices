using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Enterprise.DocumentEngine.Areas
{
	public class AreaListManager : List<Type>
	{
		public AreaListManager()
			: base(new[]
				{
					typeof(ConfigArea),

					typeof(DocumentHeaderArea),

					typeof(PageHeaderArea),

					typeof(SectionHeaderArea),
					typeof(SectionPageHeaderArea),
					typeof(SectionBodyArea),
					typeof(GroupByArea),
					typeof(SectionPageFooterArea),
					typeof(SectionFooterArea),

					typeof(FirstPageFooterArea),
					typeof(PageFooterArea),
					typeof(OnlyOnePageFooterArea),
					typeof(LastPageFooterArea),

					typeof(DocumentFooterArea),

					typeof(BackPageArea),

					typeof(EndOfReportArea),
				})
		{ }

		internal bool IsAreaInCorrectOrder(Area lastArea, Area nextArea)
		{
			Type lastAreaType = lastArea.GetType();
			Type nextAreaType = nextArea.GetType();
			return IndexOf(lastAreaType) < IndexOf(nextAreaType) || (lastArea is GroupByArea && nextArea is GroupByArea);
		}

		internal bool IsSectionArea(Area area)
		{
			int indexOfAreaType = IndexOf(area.GetType());
			return indexOfAreaType >= IndexOf(typeof(SectionHeaderArea))
				&& indexOfAreaType <= IndexOf(typeof(SectionFooterArea));
		}

		public List<ValueProviderDocumenter> GetDocumenters()
		{
			var result = new List<ValueProviderDocumenter> { GetOverview() };
			var types = this.ToList();
			types.Add(typeof(SectionForeachArea));
			foreach (var areaType in types)
			{
				var ctorInfo = areaType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, Array.Empty<Type>(), null);
				var area = (Area)ctorInfo.Invoke(Array.Empty<object>());
				result.Add(area.GetDocumentation());
			}
			result.Add(GetIfElseEndifDocumentation());
			return result;
		}

		ValueProviderDocumenter GetOverview()
		{
			return new ValueProviderDocumenter("Templates in " + Core.Constants.ProductName,
				ResString.GetMultilingualString("39186863-a659-4fa8-9a69-ba6561b7ab63", @"A Document or Report Template is an Excel spreadsheet that is laid out in a special format using various commands and keywords recognized by the Document Engine in {0}, and can be rendered and merged with data from {0} to generate a Document or Report style output.

The first tab or worksheet of a Template contains the Layout comprised of Areas, Excel formatting and Macros used to produce the desired output. You should change the name of the tab (right click and rename) as the name of each tab with a Layout in it is shown on the Preview screen within {0}.

You can optionally include other tabs if you want multiple reports in the one output. You can also add specially named Interface Tabs to gain access to more advanced features of the Document Engine, but the first tab contains everything required to produce a Report or Document.

The Interface Tabs you can use on a Report are a Filters Sheet, a Sort Order Sheet and a Group Bys Sheet, and on a Document are a Fields Sheet (UDF's) and a Constants Sheet. All you need to know about them for now is that Interface Tabs are recognized by the name shown on the tab, and that the full functionality of these is covered in the Interface Tabs reference.

The first column on the first worksheet is used exclusively for Document Engine Area configurations. This is where you define the start of each Area of your template, and insert your configuration parameters for the #Config Area. Nothing entered in this column will appear in the final output as it is always hidden by the Document Engine.

Area definitions are always made up of a #, and Area Identifier, then optional parameters separated by colons. Please note, other contents on the same line as Area definitions will be removed at the end of the creation of the report/document.

Following is an explanation of the Areas currently supported by the Document Engine and and explanation of the uses of each.", Core.Constants.ProductName));
		}

		ValueProviderDocumenter GetIfElseEndifDocumentation()
		{
			return new ValueProviderDocumenter(
				@"#IF {LogicalExpression}
...
[#ELSE]
...
#ENDIF",
				ResString.GetMultilingualString("cc6dd300-255b-4337-ae4d-e65ad047d7c5", @"Conditional Areas are used within other Areas to add or remove rows from the Template before the data is merged into the Template and the Template is rendered.

The {0} {{Logical Expression}}, {1} and {2} identifiers are always entered in the first column of the spreadsheet, and anything in subsequent columns on those rows will not be included in the final output. You can also nest Conditional Areas, but for each {0} you must have a corresponding {2} or your template will not be able to be rendered.

They're processed before any of the Body Sections are rendered, which means that if you use them in a Body Section they will be processed once before the Document Engine iterates through all of the rows. This means that all rows will evaluate the same for each and every row in a Body Section regardless of the content of the individual rows.

If you want rows to be hidden conditionally depending on the data in each row, use the <{3}({{Logical Expression}})> or <{4}> macros as appropriate. You can hide the hide macros if required in a column out to the right by adding a {5} parameter in the {6} Area, and entering '1==1' in the column you want to hide on the {5} row.

You should only use {0}..{1}..{2} Areas within the other Areas. Don't use them to conditionally exclude other Areas as this will have unpredictable results.

The Document Engine uses the Microsoft JavaScript evaluator to evaluate the logical expression passed, so most logical expressions will work. E.g:

{7}",
"#IF", "#ELSE", "#ENDIF", "HideRowIf",
"HideRowIfCellIsEmpty", "HideColumnIf", "#Config",
@"#IF 1==1
#IF ""<MyField>""!=""""
#IF <CurrentPage> > 3"));
		}
	}
}
