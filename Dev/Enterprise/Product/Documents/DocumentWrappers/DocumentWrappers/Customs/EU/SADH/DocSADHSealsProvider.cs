using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using EUDocumentWrapperConstants = Enterprise.DocumentWrappers.Customs.EU.DocumentWrapperConstants;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	class DocSADHSealsProvider
	{
		public DocSADHSealsProvider(DocSADH docSADH)
		{
			this.docSADH = Argument.NotNull(docSADH, nameof(docSADH));
		}

		public ZString GetBoxS28Seals()
		{
			var seals = GetSeals()
				.Where(x => !x.IsEmpty)
				.Distinct()
				.OrderBy(x => x)
				.ToArray();

			return ZString.Join(EUDocumentWrapperConstants.Delimiters.CommaAndSpace, seals);
		}

		#region Implementation

		IEnumerable<ZString> GetSeals()
		{
			return docSADH.Pages.Count > 0
				? GetSealsFromPages()
				: GetSealsFromLines();
		}

		IEnumerable<ZString> GetSealsFromPages()
		{
			foreach (var page in docSADH.Pages.Cast<DocSADHPage>())
			{
				yield return GetSealsFromLine(page.Line1);
				yield return GetSealsFromLine(page.Line2);
				yield return GetSealsFromLine(page.Line3);
			}
		}

		IEnumerable<ZString> GetSealsFromLines()
		{
			return docSADH.Lines
				.Cast<DocSADHLine>()
				.WhereNotNull()
				.Select(line => GetSealsFromLine(line));
		}

		ZString GetSealsFromLine(DocSADHLine line)
		{
			var boxS28SealsLine = line?.BoxS28SealsLine ?? ZString.Empty;
			if (!boxS28SealsLine.IsEmpty && boxS28SealsLine != DocSADH.EadBlankBoxDashes)
			{
				return boxS28SealsLine;
			}

			return ZString.Empty;
		}

		#endregion

		readonly DocSADH docSADH;
	}
}
