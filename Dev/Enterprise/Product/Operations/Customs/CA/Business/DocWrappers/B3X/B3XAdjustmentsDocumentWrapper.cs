using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.CA.Messaging;

namespace Enterprise.Customs.CA.Business
{
	class B3XAdjustmentsDocumentWrapper : AdjustmentsDocumentWrapper
	{
		public B3XAdjustmentsDocumentWrapper(JobDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		protected override void CheckDeclarationType(JobDeclaration declaration)
		{
			if (!declaration.IsB3X)
			{
				throw new InvalidOperationException("Declaration must be an X Type Entry");
			}
		}

		protected override AdjustmentsDocHeader CreateNewHeader(JobDeclaration declaration = null)
		{
			if (declaration != null)
			{
				return new B3XAdjustmentsDocHeader(declaration);
			}
			else
			{
				return new B3XAdjustmentsDocHeader();
			}
		}

		protected override AdjustmentsDocFooter CreateNewFooter(JobDeclaration declaration = null, IEnumerable<AdjustmentsDocPage> pages = null)
		{
			if (declaration != null)
			{
				return new B3XAdjustmentsDocFooter(declaration);
			}
			else
			{
				return new B3XAdjustmentsDocFooter();
			}
		}

		protected override AdjustmentsDocPage CreateNewPage()
		{
			return new B3XAdjustmentsDocPage();
		}

		protected override IEnumerable<AdjustmentsDocPage> GetDocumentPages()
		{
			var result = base.GetDocumentPages();
			var lineIndex = 1;
			foreach (var docPage in result)
			{
				foreach (var docLine in ((B3XAdjustmentsDocPage)docPage).DocLines)
				{
					if (!docLine.IsEmpty)
					{
						docLine.OriginalLineNo = lineIndex.ToString();
						++lineIndex;
					}
				}
			}
			return result;
		}

		public BusinessObjectCollectionWrapper<B3BReleaseLine> B3BInputReleases
		{
			get { return new BusinessObjectCollectionWrapper<B3BReleaseLine>(GetReleaseLine()); }
		}

		IEnumerable<B3BReleaseLine> GetReleaseLine()
		{
			IEnumerable<IB3BRelease> b3bInputReleases = null;
			if (declaration.IsLVX)
			{
				b3bInputReleases = declaration.JE_EntryAuthorisationDate.IsEmpty ? Array.Empty<IB3BRelease>()
					: new IB3BRelease[] { new B3BRelease { DateOfRelease = declaration.JE_EntryAuthorisationDate } };
			}
			else
			{
				var tempNumbers = (from CargoControlNumber ccn in declaration.OriginalJobCargoControlNumbers where !ccn.CY_CargoControlNumber.IsEmpty select new B3BRelease(ccn.CY_CargoControlNumber, ccn.CY_DateOfRelease)).ToArray();
				if (!declaration.JE_EntryAuthorisationDate.IsEmpty)
				{
					if (!tempNumbers.Any())
					{
						tempNumbers = new[] { new B3BRelease() { CargoControlNumber = "2CSA1" } };
					}
					tempNumbers[0].DateOfRelease = declaration.JE_EntryAuthorisationDate;
				}
				b3bInputReleases = tempNumbers;
			}
			var linesCount = ((b3bInputReleases.Count() - 1) / 50 + 1) * 25;
			var result = new B3BReleaseLine[linesCount];
			var lineNum = 0;
			var index = 0;

			foreach (var b3BRelease in b3bInputReleases)
			{
				index = (lineNum / 50) * 25 + lineNum % 25;
				if (result[index] == null)
				{
					result[index] = new B3BReleaseLine();
				}

				result[index].SetCCN(b3BRelease.CargoControlNumber, lineNum++);
			}

			if (result[index]?.CCN2.IsEmpty ?? false) // last CNN was set into first column, so there are some null lines which should be set
			{
				var number = result[index].Number;
				for (var i = index + 1; i < linesCount; i++)
				{
					result[i] = new B3BReleaseLine { Number = ++number };
				}
			}
			return result;
		}
	}
}
