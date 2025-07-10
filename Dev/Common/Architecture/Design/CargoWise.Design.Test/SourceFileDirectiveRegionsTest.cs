using System.IO;
using NUnit.Framework;

namespace CargoWise.Design.DTE.Testing
{
	class SourceFileDirectiveRegionsTest : TestCase
	{
		public void TestGetConditionalsSurroundingType()
		{
			MockTextPoint startPoint = new MockTextPoint(null);
			startPoint.SetLine(10);
			MockTextPoint endPoint = new MockTextPoint(null);
			endPoint.SetLine(12);
			MockCodeClass type = new MockCodeClass(ProjectItem, "MyEntityType");
			type.SetStartPoint(startPoint);
			type.SetEndPoint(endPoint);
			using (StreamWriter writer = new StreamWriter(SourceFile))
			{
				writer.WriteLine("#if DECOY");
				writer.WriteLine("using CargoWise.Business");
				writer.WriteLine("public class DecoyEntityType : DEntity");
				writer.WriteLine("{");
				writer.WriteLine("}");
				writer.WriteLine("#endif");
				writer.WriteLine("");
				writer.WriteLine("#if DEBUG");
				writer.WriteLine("#if SECOND_CONDITIONAL");
				writer.WriteLine("using CargoWise.Business;  public class MyEntityType : DEntity");
				writer.WriteLine("{");
				writer.WriteLine("}");
				writer.WriteLine("#endif");
				writer.WriteLine("#endif");
				ProjectItem.FileNames.Add(sourceFile);
			}

			DSourceFileDirectiveRegions regions = new DSourceFileDirectiveRegions(ProjectItem, "if");
			DirectiveRegion[] directives = regions.GetDirectivesSurroundingType("MyEntityType");
			AssertEquals("Should be 2 conditionals found on MyEntityType", 2, directives.Length);
			AssertEquals("DEBUG", directives[0].DirectiveText);
			AssertEquals("SECOND_CONDITIONAL", directives[1].DirectiveText);
		}

		public void TestGetConditionalsWithinType()
		{
			MockTextPoint startPoint = new MockTextPoint(null);
			startPoint.SetLine(12);
			MockTextPoint endPoint = new MockTextPoint(null);
			endPoint.SetLine(18);
			MockCodeClass type = new MockCodeClass(ProjectItem, "MyEntityType");
			type.SetStartPoint(startPoint);
			type.SetEndPoint(endPoint);
			using (StreamWriter writer = new StreamWriter(SourceFile))
			{
				writer.WriteLine("#region DECOY");
				writer.WriteLine("using CargoWise.Business");
				writer.WriteLine("public class DecoyEntityType : DEntity");
				writer.WriteLine("{");
				writer.WriteLine("#region DECOY");
				writer.WriteLine("//");
				writer.WriteLine("#endregion");
				writer.WriteLine("}");
				writer.WriteLine("#endregion");
				writer.WriteLine("");
				writer.WriteLine("#region DECOY");
				writer.WriteLine("using CargoWise.Business;  public class MyEntityType : DEntity");
				writer.WriteLine("{");
				writer.WriteLine("#if DECOY");
				writer.WriteLine("#region RegionText");
				writer.WriteLine("#region SecondRegionText");
				writer.WriteLine("#endregion");
				writer.WriteLine("#endregion");
				writer.WriteLine("#endif");
				writer.WriteLine("}");
				writer.WriteLine("#endregion");
				ProjectItem.FileNames.Add(sourceFile);
			}

			DSourceFileDirectiveRegions regions = new DSourceFileDirectiveRegions(ProjectItem, "region");
			DirectiveRegion[] directives = regions.GetDirectivesWithinType("MyEntityType");
			AssertEquals("Should be 2 regions found on MyEntityType", 2, directives.Length);
			AssertEquals("RegionText", directives[0].DirectiveText);
			AssertEquals("SecondRegionText", directives[1].DirectiveText);
		}

		string SourceFile
		{
			get
			{
				return sourceFile ?? (sourceFile = TempForTest.GetTempFileName());
			}
		}

		string sourceFile;
		MockProjectItem ProjectItem
		{
			get
			{
				return projectItem ?? (projectItem = new MockProjectItem(new MockProject()));
			}
		}

		MockProjectItem projectItem;
		protected override void TearDown()
		{
			base.TearDown();
			if (sourceFile != null)
			{
				File.Delete(sourceFile);
			}
		}
	}
}
