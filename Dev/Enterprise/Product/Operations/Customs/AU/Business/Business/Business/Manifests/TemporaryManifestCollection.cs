using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TemporaryManifestsCollection : NonPersistentBusinessObjectCollection<TemporaryManifest>
	{
		public TemporaryManifestsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TemporaryManifest(Factory, new CalcExportManifestHeader(Factory, ManifestTypeList.Codes.ExportMainManifest));
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(TemporaryManifest);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
