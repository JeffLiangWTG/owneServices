using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects
{
	public class DV1DocDataObject : DocDataObject
	{
		public DV1DocDataObject(IDV1Certificate dV1)
		{
			this.dV1 = Argument.NotNull(dV1, nameof(dV1));
			BuildEntries();
		}

		readonly IDV1Certificate dV1;

		public List<EntryHeaderDataObject> Entries { get; private set; }

		void BuildEntries()
		{
			Entries = dV1.Entries.ToList();  // Entries must be evaluated before rendering in form builder, otherwise the captions won't be translated to expected language, see WI00482207
		}
	}
}
