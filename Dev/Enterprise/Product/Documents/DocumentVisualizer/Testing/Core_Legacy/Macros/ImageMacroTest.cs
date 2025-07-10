using System.Drawing;
using System.IO;
using CargoWise.IO;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Testing.Dummies;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class ImageMacroTest : DocumentMacroTest
	{
		#region TestRunMacro

		public void TestRunMacro()
		{
			var imageFilePath = Path.Combine(Temp.TempPath, @"testImage.png");

			try
			{
				new Bitmap(50, 50).Save(imageFilePath);

				Assert("prerequisite: test image exists", File.Exists(imageFilePath));

				var macro = $"\"<Image(\"{imageFilePath.Replace("\\", "\\\\")}\", 1, 1)>\"";

				var cell = new DummyCell
				{
					TopRow = 1,
					LeftColumn = 1,
					BottomRow = 1,
					RightColumn = 1
				};

				var page = new DummyPage
				{
					Range = new Range(1, 1, 10, 10)
				};

				var macroRun = new MacroRun
				{
					Data = new object(),
					ExpectedResult = "",
					Variables =
					{
						{ VariableNames.CellInternal, cell },
						{ VariableNames.PageInternal, page }
					}
				};

				AssertMacroRun(macro, macroRun);

				AssertNotNull("image has been assigned to cell", cell.Drawing);
				AssertNotNull("image has been assigned to cell", cell.Drawing.ImageData);
			}
			finally
			{
				if (File.Exists(imageFilePath))
				{
					File.Delete(imageFilePath);
				}
			}
		}

		#endregion

		#region TestRunMacro_MacroObject()

		public void TestRunMacro_MacroObject()
		{
			var data = new
			{
				Image = new Bitmap(50, 50)
			}.MakeDynamic();

			var macro = $"\"<Image(Image, 1, 1)>\"";

			var cell = new DummyCell
			{
				TopRow = 1,
				LeftColumn = 1,
				BottomRow = 1,
				RightColumn = 1
			};

			var page = new DummyPage
			{
				Range = new Range(1, 1, 10, 10)
			};

			var macroRun = new MacroRun
			{
				Data = data,
				ExpectedResult = "",
				Variables =
					{
						{ VariableNames.CellInternal, cell },
						{ VariableNames.PageInternal, page }
					}
			};

			AssertMacroRun(macro, macroRun);

			AssertNotNull("image has been assigned to cell", cell.Drawing);
			AssertNotNull("image has been assigned to cell", cell.Drawing.ImageData);
		}

		#endregion

		#region TestRunMacro_InvalidUrl

		public void TestRunMacro_InvalidUrl()
		{
			const string imageFilePath = "http://invalid.path.zzz";

			var macro = $"\"<Image(\"{imageFilePath.Replace("\\", "\\\\")}\", 1, 1)>\"";

			var cell = new DummyCell
			{
				TopRow = 1,
				LeftColumn = 1,
				BottomRow = 1,
				RightColumn = 1
			};

			var page = new DummyPage
			{
				Range = new Range(1, 1, 10, 10)
			};

			var macroRun = new MacroRun
			{
				Data = new object(),
				ExpectedResult = "",
				Variables =
					{
						{ VariableNames.CellInternal, cell },
						{ VariableNames.PageInternal, page }
					}
			};

			AssertMacroRun(macro, macroRun);

			AssertNotNull("image has been assigned to cell", cell.Drawing);
			AssertNull("there's no image", cell.Drawing.ImageData);
		}

		#endregion

		#region TestRunMacro_InvalidImageFile

		public void TestRunMacro_InvalidImageFile()
		{
			var textFilePath = Path.Combine(Temp.TempPath, @"testtext.txt");
			using (var writer = File.CreateText(textFilePath))
			{
				writer.WriteLine("I'm not an image lol");
			}

			try
			{
				Assert("prerequisite: test file exists", File.Exists(textFilePath));

				var macro = $"\"<Image(\"{textFilePath.Replace("\\", "\\\\")}\", 1, 1)>\"";

				var cell = new DummyCell
				{
					TopRow = 1,
					LeftColumn = 1,
					BottomRow = 1,
					RightColumn = 1
				};

				var page = new DummyPage
				{
					Range = new Range(1, 1, 10, 10)
				};

				var macroRun = new MacroRun
				{
					Data = new object(),
					ExpectedResult = "",
					Variables =
						{
							{ VariableNames.CellInternal, cell },
							{ VariableNames.PageInternal, page }
						}
				};

				AssertMacroRun(macro, macroRun);

				AssertNotNull("image has been assigned to cell", cell.Drawing);
				AssertNotNull("data should still be loaded", cell.Drawing.ImageData);
			}
			finally
			{
				if (File.Exists(textFilePath))
				{
					var timeWaited = 0;
					var maxWaitTime = 3000;
					while (timeWaited <= maxWaitTime)
					{
						try
						{
							File.Delete(textFilePath);
							break;
						}
						catch (IOException) when (timeWaited < maxWaitTime)
						{
							const int waitTime = 500;
							System.Threading.Thread.Sleep(waitTime);
							timeWaited += waitTime;
						}
					}
				}
			}
		}

		#endregion

		#region TestRunMacro_OverflowingPage

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunMacro_OverflowingPage()
		{
			var imageFilePath = Path.Combine(BaseSourcePath,
				@"Enterprise\Product\Documents\DocumentVisualizer\GUI\Resources\tools.png");

			Assert("prerequisite: test image exists", File.Exists(imageFilePath));

			var macro = $"\"<Image(\"{imageFilePath.Replace("\\", "\\\\")}\", 1, 1000)>\"";

			var cell = new DummyCell
			{
				TopRow = 1,
				LeftColumn = 1,
				BottomRow = 1,
				RightColumn = 1
			};

			var page = new DummyPage
			{
				Range = new Range(1, 1, 10, 10)
			};

			var macroRun = new MacroRun
			{
				Data = new object(),
				ExpectedNotifications = "Image cannot be displayed because it would overflow the right side of the page.",
				Variables =
					{
						{ VariableNames.CellInternal, cell },
						{ VariableNames.PageInternal, page }
					}
			};

			AssertMacroRun(macro, macroRun);

			AssertNull("image has not been assigned to cell", cell.Drawing);
		}

		#endregion

		#region TestRunMacro_InvalidHeight

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunMacro_InvalidHeight()
		{
			var imageFilePath = Path.Combine(BaseSourcePath,
				@"Enterprise\Product\Documents\DocumentVisualizer\GUI\Resources\tools.png");

			Assert("prerequisite: test image exists", File.Exists(imageFilePath));

			var macro = $"\"<Image(\"{imageFilePath.Replace("\\", "\\\\")}\", -1, 1)>\"";

			var cell = new DummyCell
			{
				TopRow = 1,
				LeftColumn = 1,
				BottomRow = 1,
				RightColumn = 1
			};

			var page = new DummyPage
			{
				Range = new Range(1, 1, 10, 10)
			};

			var macroRun = new MacroRun
			{
				Data = new object(),
				ExpectedNotifications = "Height has to be greater than 0.",
				Variables =
					{
						{ VariableNames.CellInternal, cell },
						{ VariableNames.PageInternal, page }
					}
			};

			AssertMacroRun(macro, macroRun);

			AssertNull("image has not been assigned to cell", cell.Drawing);
		}

		#endregion

		#region TestRunMacro_InvalidWidth

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunMacro_InvalidWidth()
		{
			var imageFilePath = Path.Combine(BaseSourcePath,
				@"Enterprise\Product\Documents\DocumentVisualizer\GUI\Resources\tools.png");

			Assert("prerequisite: test image exists", File.Exists(imageFilePath));

			var macro = $"\"<Image(\"{imageFilePath.Replace("\\", "\\\\")}\", 1, -1)>\"";

			var cell = new DummyCell
			{
				TopRow = 1,
				LeftColumn = 1,
				BottomRow = 1,
				RightColumn = 1
			};

			var page = new DummyPage
			{
				Range = new Range(1, 1, 10, 10)
			};

			var macroRun = new MacroRun
			{
				Data = new object(),
				ExpectedNotifications = "Width has to be greater than 0.",
				Variables =
					{
						{ VariableNames.CellInternal, cell },
						{ VariableNames.PageInternal, page }
					}
			};

			AssertMacroRun(macro, macroRun);

			AssertNull("image has not been assigned to cell", cell.Drawing);
		}

		#endregion
	}
}
