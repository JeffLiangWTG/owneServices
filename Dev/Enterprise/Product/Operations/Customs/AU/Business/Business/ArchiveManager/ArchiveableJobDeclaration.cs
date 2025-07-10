using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.ArchiveManager;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class ArchiveableJobDeclaration : ArchiveableJobDeclaration<Integration.Customs.AU.IJobDeclaration, JobDeclaration>
	{
		public ArchiveableJobDeclaration(Integration.Customs.AU.IJobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void AddAnyAdditionalParents(List<ZGuid> cusEntryNumberParents)
		{
			base.AddAnyAdditionalParents(cusEntryNumberParents);

			foreach (JobComInvoiceHeader invoice in Declaration.Invoices)
			{
				QuarantineExDocHeader cachedQuarantineExDocHeader = invoice.QuarantineExDocHeader;
				if (cachedQuarantineExDocHeader != null)
				{
					cusEntryNumberParents.Add(cachedQuarantineExDocHeader.PK);
				}
			}
		}
	}
}
