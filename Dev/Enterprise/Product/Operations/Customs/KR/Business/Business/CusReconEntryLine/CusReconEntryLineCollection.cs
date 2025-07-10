using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconEntryLineCollection : ActiveBusinessObjectCollection<CusReconEntryLine>, IEmbeddedModulePopupCollection
	{
		public CusReconEntryLineCollection(CusReconDeclaration declaration) : base(declaration.Factory, new AdhocCollectionRelationship(typeof(CusReconEntryLine)))
		{
			this.declaration = declaration;
		}
		readonly CusReconDeclaration declaration;

		public void Load()
		{
			((System.Collections.IList)this).Clear();
			AddRange(declaration.CusReconEntries.OfType<CusReconEntry>().WhereNotNull().SelectMany(x => x.CusReconEntryLines.OfType<CusReconEntryLine>()).WhereNotNull());
		}

		protected override void OnAdded(CusReconEntryLine line)
		{
			if (line.Header == null)
			{
				var header = declaration.CusReconEntries.AddNew();
				line.CRL_CRE = header.PK;
			}

			if (declaration.IsRefundTypeContractRevocation)
			{
				line.ContractRevocations.AddNew();
			}

			base.OnAdded(line);
		}

		public CusReconEntryLine AddNewLine(KREntryCustomsBillsView moduleBO)
		{
			var line = AddNew();
			line.Header.SaveSnapshotByCustomsBillNumber(moduleBO);
			return line;
		}

		BusinessObject IEmbeddedModulePopupCollection.AddNewLine(BusinessObject moduleBO) => AddNewLine((KREntryCustomsBillsView)moduleBO);
	}
}
