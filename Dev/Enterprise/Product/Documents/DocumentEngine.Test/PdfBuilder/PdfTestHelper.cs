using System;
using System.Collections.Generic;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;

namespace Enterprise.DocumentEngine.PdfBuilder.Testing
{
	public sealed class PdfTestHelper
	{
		static IEnumerable<string> ExtractText(CObject cobj)
		{
			var textList = new List<string>();
			if (cobj is COperator cOperator)
			{
				if (cOperator.OpCode.Name == nameof(OpCodeName.Tj) || cOperator.OpCode.Name == nameof(OpCodeName.TJ))
				{
					foreach (var cOperand in cOperator.Operands)
					{
						textList.AddRange(ExtractText(cOperand));
					}
				}
			}
			else if (cobj is CSequence cSequence)
			{
				foreach (var element in cSequence)
				{
					textList.AddRange(ExtractText(element));
				}
			}
			else if (cobj is CString cString)
			{
				textList.Add(cString.Value);
			}
			return textList;
		}

		public static string GetTextContent(PdfPage page)
		{
			var content = ContentReader.ReadContent(page);
			var extractedText = String.Join(" ", ExtractText(content));

			return extractedText;
		}
	}
}
