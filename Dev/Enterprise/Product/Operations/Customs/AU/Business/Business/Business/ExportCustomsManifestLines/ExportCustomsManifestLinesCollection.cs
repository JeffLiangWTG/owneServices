using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportCustomsManifestLinesCollection : Customs.Business.ExportCustomsManifestLinesCollection
	{
		public ExportCustomsManifestLinesCollection(ExportCustomsManifestHeader parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
			this.header = parent;
		}

		public new ExportCustomsManifestLines this[int index]
		{
			get { return (ExportCustomsManifestLines)Elements[index]; }
		}

		public new virtual ExportCustomsManifestLines AddNew()
		{
			return (ExportCustomsManifestLines)base.AddNew();
		}

		public new ExportCustomsManifestLines AddNew(Type bizObjType)
		{
			return (ExportCustomsManifestLines)base.AddNew(bizObjType);
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(ExportCustomsManifestLines);
		}

		public ExportCustomsManifestLines GetLine(int lineNumber)
		{
			foreach (ExportCustomsManifestLines line in this)
			{
				if (line.EL_LineNo == lineNumber)
				{
					return line;
				}
			}
			return null;
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			header.CalculateTotalContainersFromLines();
			header.CalculateTotalPackagesFromLines();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			((ExportCustomsManifestLines)child).EL_RN_NKCountryOfDestination = header.ED_RN_NKCountryOfDestination;
		}

		readonly ExportCustomsManifestHeader header;
	}
}
