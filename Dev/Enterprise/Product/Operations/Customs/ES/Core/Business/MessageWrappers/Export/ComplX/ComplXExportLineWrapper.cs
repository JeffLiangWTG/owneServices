using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ComplXExportLineWrapper : ExportLineCommonWrapper, IComplXExportLine
	{
		public ComplXExportLineWrapper(CusEntryLine cusEntryLine) : base(cusEntryLine)
		{
		}

		const string ConcessionCode9VA = "9VA";

		protected override ZDecimal GrossWeightInKGCore
		{
			get
			{
				if (grossWeightInKGCore == null)
				{
					grossWeightInKGCore = new CachedProperty<ZDecimal>(entryLine.Factory, () =>
					{
						var concessionCode = entryLine.RandomLine.JI_FormattedProcedure.Length > 4 ? entryLine.RandomLine.JI_FormattedProcedure.SubstringSafe(4) : ZString.Empty;

						if (concessionCode == ConcessionCode9VA)
						{
							return ZDecimal.Zero;
						}
						foreach (var additionalCode in entryLine.RandomLine.AdditionalProcedureCodes.Cast<EU.Business.AdditionalProcedureCode>())
						{
							if (additionalCode.CY_Code.Right(3) == ConcessionCode9VA)
							{
								return ZDecimal.Zero;
							}
						}
						return base.GrossWeightInKGCore;
					});
				}
				return grossWeightInKGCore.Value;
			}
		}
		CachedProperty<ZDecimal> grossWeightInKGCore;

		protected override ZDecimal NetWeightInKGCore
		{
			get
			{
				if (netWeightInKGCore == null)
				{
					netWeightInKGCore = new CachedProperty<ZDecimal>(entryLine.Factory, () =>
					{
						var concessionCode = entryLine.RandomLine.JI_FormattedProcedure.Length > 4 ? entryLine.RandomLine.JI_FormattedProcedure.SubstringSafe(4) : ZString.Empty;

						if (concessionCode == ConcessionCode9VA)
						{
							return ZDecimal.Zero;
						}
						foreach (var additionalCode in entryLine.RandomLine.AdditionalProcedureCodes.Cast<EU.Business.AdditionalProcedureCode>())
						{
							if (additionalCode.CY_Code.Right(3) == ConcessionCode9VA)
							{
								return ZDecimal.Zero;
							}
						}
						return base.NetWeightInKGCore;
					});
				}
				return netWeightInKGCore.Value;
			}
		}
		CachedProperty<ZDecimal> netWeightInKGCore;

		public IReadOnlyCollection<IExportDocumentCommon> Documents => documents ?? (documents = GetDocuments());
		IReadOnlyCollection<IExportDocumentCommon> documents;

		IExportDocumentCommon[] GetDocuments()
		{
			var documents = new List<ExportDocumentCommonWrapper>();
			var supDocsInCL = entryLine.GetPreviouslySentSupportingDocuments();

			documents.AddRange(entryLine.SupportingDocuments.Cast<SupportingDocument>()
												.Where(doc => doc.DoesNotMatchAnyPreviouslySentComplXExportDocument(supDocsInCL))
													.Select(doc => new ExportDocumentCommonWrapper(doc)));

			documents.AddRange(entryLine.Header.SupportingDocuments.Cast<SupportingDocument>()
												.Where(doc => doc.DoesNotMatchAnyPreviouslySentComplXExportDocument(supDocsInCL))
													.Select(doc => new ExportDocumentCommonWrapper(doc)));

			if (documents.Count == ZInt.Zero && SupportingDocumentHelper.HasComplXExportDocuments(supDocsInCL))
			{
				documents.Add(new ExportDocumentCommonWrapper(SupportingDocumentHelper.GetFirstComplXExportDocument(supDocsInCL)));
			}

			return documents.ToArray();
		}
	}
}
