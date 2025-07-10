using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using ESCusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	public static class SeparateBoxesHelper
	{
		public static ZString GetTextTrimmedBox31ForNormalLine(ZString text, int maxLength) => GetTextTrimmedBox31(text, maxLength).Item1;

		public static (ZString, ZString) GetTextTrimmedBox31(ZString text, int maxLength) => GetTextTrimmed(text, maxLength, box31SeparatorChr);

		public static ZString GetTextTrimmedBox44ForNormalLine(ZString text, int maxLength) => GetTextTrimmedBox44(text, maxLength).Item1;

		public static (ZString, ZString) GetTextTrimmedBox44(ZString text, int maxLength) => GetTextTrimmed(text, maxLength, box44SeparatorChr);

		static (ZString, ZString) GetTextTrimmed(ZString text, int maxLength, ZString trimLimit)
		{
			if (text.Length <= maxLength)
			{
				return (text, ZString.Empty);
			}

			var textTrimmed = text.SubstringSafe(0, maxLength);
			var lastIndexOfTextTrimmed = textTrimmed.LastIndexOf(trimLimit);
			if (lastIndexOfTextTrimmed != -1)
			{
				var fixedTextTrimmed = textTrimmed.SubstringSafe(0, lastIndexOfTextTrimmed);
				var restOfText = text.SubstringSafe(lastIndexOfTextTrimmed + trimLimit.Length);
				return (fixedTextTrimmed, restOfText);
			}
			else
			{
				return (textTrimmed, text.SubstringSafe(maxLength));
			}
		}

		static bool ShouldTrimBox31OrBox44(ESCusEntryLine entryLine) => entryLine.Box31CompleteText.Length > entryLine.Box31MaxLength
																		|| entryLine.Box44CompleteText.Length > (entryLine.Header.IsImport ? entryLine.ImportBox44MaxLength : entryLine.ExportBox44MaxLength);

		static List<ExtraLineBox31AndBox44Manager> GetExtraLineBox31AndBox44(ESCusEntryLine entryLine, bool isFirstEntryLine)
		{
			var resultList = new List<ExtraLineBox31AndBox44Manager>();

			if (ShouldTrimBox31OrBox44(entryLine))
			{
				var box31MaxLength = entryLine.Box31MaxLength;
				var box44MaxLength = !isFirstEntryLine
					? entryLine.Header.IsImport ? entryLine.ImportBox44BISPageMaxLength : entryLine.ExportBox44BISPageMaxLength
					: entryLine.Header.IsImport ? entryLine.ImportBox44MaxLength : entryLine.ExportBox44MaxLength;
				var box31RemainingText = GetTextTrimmedBox31(entryLine.Box31CompleteText, box31MaxLength).Item2;
				var box44RemainingText = GetTextTrimmedBox44(entryLine.Box44CompleteText, box44MaxLength).Item2;

				while (box31RemainingText != ZString.Empty || box44RemainingText != ZString.Empty)
				{
					var (box31Text, box31LeftText) = GetTextTrimmedBox31(box31RemainingText, box31MaxLength);
					var (box44Text, box44LeftText) = GetTextTrimmedBox44(box44RemainingText, box44MaxLength);
					resultList.Add(new ExtraLineBox31AndBox44Manager()
					{
						Box31Text = box31Text,
						Box44Text = box44Text
					});

					box31RemainingText = box31LeftText;
					box44RemainingText = box44LeftText;
				}
			}

			return resultList;
		}

		public static List<ESDocSADHLineExportExtraBox31AndBox44> GetExportGetExtraLineBox31AndBox44(ESCusEntryLine entryLine, BusinessObjectFactory factory, bool isFirstEntryLine)
		{
			var resultList = new List<ESDocSADHLineExportExtraBox31AndBox44>();
			GetExtraLineBox31AndBox44(entryLine, isFirstEntryLine).ForEach(extraLine => resultList.Add(ESDocSADHLineExportExtraBox31AndBox44.New(entryLine, factory, extraLine.Box31Text, extraLine.Box44Text)));
			return resultList;
		}

		public static List<ESDocSADHLineImportExtraBox31AndBox44> GetImportGetExtraLineBox31AndBox44(ESCusEntryLine entryLine, BusinessObjectFactory factory, bool isFirstEntryLine)
		{
			var resultList = new List<ESDocSADHLineImportExtraBox31AndBox44>();
			GetExtraLineBox31AndBox44(entryLine, isFirstEntryLine).ForEach(extraLine => resultList.Add(ESDocSADHLineImportExtraBox31AndBox44.New(entryLine, factory, extraLine.Box31Text, extraLine.Box44Text)));
			return resultList;
		}

		class ExtraLineBox31AndBox44Manager()
		{
			public ZString Box31Text;
			public ZString Box44Text;
		}

		const string box31SeparatorChr = " ";

		const string box44SeparatorChr = "; ";
	}
}
