using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUACompleteImportSendMessageWrapper : DUAImportCommonSendMessageWrapper, IDUACompleteImportMessageDataProvider
	{
		public DUACompleteImportSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
			Argument.GreaterThan(entryHeader.MergedLines.Count, 0, nameof(entryHeader.MergedLines));
		}

		public ZDateTime ProcedureDate => declaration.ZG_LCPDepart;

		public IDUACompleteImportHeader Header => header ?? (header = new DUACompleteImportHeaderWrapper(entryHeader));
		DUACompleteImportHeaderWrapper header;

		public IReadOnlyCollection<IDUACompleteImportLine> Lines
		{
			get
			{
				if (lines == null)
				{
					lines = entryHeader.MergedLines
						.Cast<CusEntryLine>()
						.Select(entryLine => new DUACompleteImportLineWrapper(entryLine, IsCanary))
						.ToList().AsReadOnly();
				}
				return lines;
			}
		}
		IReadOnlyCollection<DUACompleteImportLineWrapper> lines;
	}
}
