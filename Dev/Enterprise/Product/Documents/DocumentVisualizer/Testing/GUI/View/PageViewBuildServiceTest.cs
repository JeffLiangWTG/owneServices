using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class PageViewBuildServiceTest : TestCaseWithFactory
	{
		#region TestBuild

		public void TestBuild()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			var child = dummy.Collection.AddNew();
			child.Z0_Description = "I am a child";

			var worksheet = CreateWorkSheet();

			IStandardTemplate template = new StandardTemplate(worksheet);

			var documentBuilder = new StandardDocumentBuilder(
				template.Name,
				"test",
				template,
				new MacroScope(dummy.MakeDynamic()),
				new IMacroLibrary[]
				{
					new StandardLibrary(),
					new DocumentLibrary()
				}.CreateContext(),
				null,
				DefaultLanguageProvider.Instance);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed without errors",
				"", string.Join("\r\n", template.ToFormatString()));

			var presenter = new DummyPageViewPresenter();

			var service = new PageViewBuildService();
			var pageView = service.CreatePageView(document.Pages.First(), presenter, false);

			try
			{
				var elementsFormatted = pageView
					.Elements
					.Select(FormatLayoutElement);

				AssertMultilineASCIIEquals("Should create all elements for the DocumentHeader and first section",
@"Text 'document header'
Line Horizontal
Line Horizontal
Line Vertical
Line Vertical
Rectangle
Editable DynamicContent '""<Modifiable(Z0_Description)>""'
Rectangle
Non editable DynamicContent '""<Z0_Description>""'
Rectangle
Text 'First section header'
Rectangle
Non editable DynamicContent '""<Z0_Description>""'
Line Vertical
Line Horizontal
Line Horizontal
Text 'First section footer'
Line Horizontal
Line Horizontal
Drawing
Line Horizontal
Line Horizontal
Line Horizontal
Line Horizontal
Line Vertical",
					string.Join("\r\n", elementsFormatted));
			}
			finally
			{
				((IDisposable)pageView).Dispose();
			}

			Assert("HorizontalPrintOffset has been calculated", document.HorizontalPrintOffset > 0d);

			var element = pageView
				.Elements
				.OfType<IDynamicContentLayoutElement>()
				.Last();

			var pageContentWidth = document.PageDimensions.Width - document.Margins.Left - document.Margins.Right;

			var left = element.Boundaries.Left;
			var right = pageContentWidth - element.Boundaries.Right;

			Assert("element is centered", Math.Abs(left - right) < 10f);

			var logo = pageView.Elements.FirstOrDefault(x => x.Element.ElementType == ElementType.Drawing);
			Assert("HorizontalPrintOffset has been included", (element.Boundaries.Right - logo.Boundaries.Right) < document.HorizontalPrintOffset);
		}

		#endregion

		#region Implementation

		IWorksheet CreateWorkSheet()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentVisualizer.Testing.Core_Legacy.Template.TestTemplate.xls", "TestTemplate.xls");
				return FlexCelWorksheet.FromFile(tempFileName);
			}
		}

		string FormatLayoutElement(ILayoutElement layoutElement)
		{
			string result = string.Empty;

			switch (layoutElement.Element.ElementType)
			{
				case ElementType.Text:
					var text = (Text)layoutElement.Element;
					result = $"{layoutElement.Element.ElementType} '{text.Content}'";
					break;

				case ElementType.DynamicContent:
					var dynammicContent = (DynamicContent)layoutElement.Element;
					result = $"{(dynammicContent.EditableData.Any() ? "Editable" : "Non editable")} {layoutElement.Element.ElementType} '{dynammicContent.MacroExpression?.Text}'";
					break;

				case ElementType.Line:
					var line = (Line)layoutElement.Element;
					result = $"{layoutElement.Element.ElementType} {GetLineDescription(line)}";
					break;

				default:
					result = $"{layoutElement.Element.ElementType}";
					break;
			}

			return result;
		}

		string GetLineDescription(Line line)
		{
			const float margin = 0.01f;

			if (line.Size.Width < margin && line.Size.Height > margin)
			{
				return "Vertical";
			}

			if (line.Size.Width > margin && line.Size.Height < margin)
			{
				return "Horizontal";
			}

			return "Angled";
		}

		#endregion
	}
}
