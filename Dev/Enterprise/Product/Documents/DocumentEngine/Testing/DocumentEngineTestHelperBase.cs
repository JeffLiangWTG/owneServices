#if DEBUG
using System.Collections.Generic;
using System.IO;
using System.Text;
using Enterprise.DocumentEngine.FlexCelInterface;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Test Class")]
	public class DocumentEngineTestHelperBase
	{
		public static byte[] CreateTemplateFromString(string contents)
		{
			byte[] result = null;

			using (var stream = new MemoryStream())
			{
				GenerateTemplateStreamFromString(stream, contents);
				result = stream.CopyToByteArray();
			}

			return result;
		}

		internal static void GenerateTemplateStreamFromString(Stream stream, string contents)
		{
			using (ExcelInterface excelInterface = new ExcelInterface())
			{
				SaveContentToStream(stream, contents, excelInterface);
			}
		}

		internal static void SaveContentToStream(Stream stream, string contents, ExcelInterface excelInterface)
		{
			excelInterface.NewExcelFile(1);
			excelInterface.ActiveWorksheet = 0;
			GenerateExcelWorkSheetFromString(excelInterface.WorkSheets[0], contents);
			excelInterface.Xls.PrintPaperSize = TPaperSize.A4;
			excelInterface.SetOrientation(Orientation.Portrait);
			excelInterface.Xls.SheetName = "Document";
			excelInterface.SaveToStream(stream);
		}

		public static void GenerateExcelWorkSheetFromString(ExcelWorkSheet workSheet, string contents)
		{
			var row = 0;

			workSheet.Clear();

			foreach (var line in contents.Split("\n".ToCharArray()))
			{
				var parser = new LineParser(row);
				var parsedCells = parser.Parse(line);

				foreach (var parsedCell in parsedCells)
				{
					workSheet[parsedCell.Row, parsedCell.Column] = parsedCell.Contents;
				}

				row++;
			}
		}

		class LineParser
		{
			internal LineParser(int row)
			{
				this.row = row;
				this.parsedCells = new List<ParsedCell>();
				this.state = new FindColumnState(this);
			}

			readonly int row;
			readonly List<ParsedCell> parsedCells;
			LineParserState state;

			internal ParsedCell[] Parse(string line)
			{
				parsedCells.Clear();

				foreach (var character in line.ToCharArray())
				{
					state.Parse(character);
				}

				return parsedCells.ToArray();
			}

			abstract class LineParserState
			{
				internal LineParserState(LineParser parser)
				{
					this.Parser = parser;
				}

				protected readonly LineParser Parser;

				internal abstract void Parse(char character);
			}

			class FindColumnState : LineParserState
			{
				internal FindColumnState(LineParser parser)
					: base(parser)
				{
				}

				internal override void Parse(char character)
				{
					switch (character)
					{
						case '{':
							Parser.state = new GetColumnState(Parser);
							break;
					}
				}
			}

			class GetColumnState : LineParserState
			{
				internal GetColumnState(LineParser parser)
					: base(parser)
				{
					columnReference = new StringBuilder();
				}

				readonly StringBuilder columnReference;

				internal override void Parse(char character)
				{
					switch (character)
					{
						case '}':
							var column = GetColumn(columnReference.ToString());
							Parser.state = new FindSeparatorState(Parser, column);
							break;

						default:
							columnReference.Append(character);
							break;
					}
				}

				int GetColumn(string columnReference)
				{
					var result = 0;
					var position = 0;
					foreach (char columnPart in columnReference.Trim().ToUpper().ToCharArray())
					{
						result += (columnPart - 'A') + (26 * position);
						position++;
					}

					return result;
				}
			}

			class FindSeparatorState : LineParserState
			{
				internal FindSeparatorState(LineParser parser, int column)
					: base(parser)
				{
					this.column = column;
				}

				readonly int column;

				internal override void Parse(char character)
				{
					switch (character)
					{
						case '-':
							Parser.state = new FindContentsState(Parser, column);
							break;
					}
				}
			}

			enum ContentType
			{
				Text,
				Formula
			}

			class FindContentsState : LineParserState
			{
				internal FindContentsState(LineParser parser, int column)
					: base(parser)
				{
					this.column = column;
					this.contentType = ContentType.Text;
				}

				readonly int column;
				ContentType contentType;

				internal override void Parse(char character)
				{
					switch (character)
					{
						case '[':
							Parser.state = new GetContentsState(Parser, column, contentType);
							break;

						case 'F':
							contentType = ContentType.Formula;
							break;
					}
				}
			}

			class GetContentsState : LineParserState
			{
				internal GetContentsState(LineParser parser, int column, ContentType contentType)
					: base(parser)
				{
					this.column = column;
					this.contentsBuilder = new StringBuilder();
					this.contentType = contentType;
					this.depth = 0;
				}

				readonly int column;
				readonly StringBuilder contentsBuilder;
				readonly ContentType contentType;
				int depth;

				internal override void Parse(char character)
				{
					switch (character)
					{
						case ']':
							depth--;
							if (depth < 0)
							{
								var parsedCell = new ParsedCell();
								parsedCell.Row = Parser.row;
								parsedCell.Column = column;

								var contents = contentsBuilder.ToString();
								switch (contentType)
								{
									case ContentType.Formula:
										parsedCell.Contents = new TFormula(contents);
										break;

									default:
										parsedCell.Contents = contents;
										break;
								}

								Parser.parsedCells.Add(parsedCell);
								Parser.state = new FindColumnState(Parser);
							}
							else
							{
								contentsBuilder.Append(character);
							}

							break;

						case '[':
							depth++;
							contentsBuilder.Append(character);
							break;

						default:
							contentsBuilder.Append(character);
							break;
					}
				}
			}
		}

		class ParsedCell
		{
			internal int Row;
			internal int Column;
			internal object Contents;
		}
	}
}
#endif
