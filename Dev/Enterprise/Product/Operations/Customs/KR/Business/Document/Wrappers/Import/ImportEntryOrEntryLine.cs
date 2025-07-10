using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("Soon to be used")]
	public class ImportEntryOrEntryLine : NonPersistentBusinessObject, IImportEntryOrEntryLine
	{
		public ImportEntryOrEntryLine(ImportEntryOrEntryLineSerializable importEntryOrEntryLineSerializable, BusinessObjectFactory factory) : base(factory)
		{
			this.importEntryOrEntryLineSerializable = importEntryOrEntryLineSerializable;
		}

		readonly IImportEntryOrEntryLine importEntryOrEntryLineSerializable;

		public ZDateTime PaidDate => importEntryOrEntryLineSerializable.PaidDate;

		public ICharges PaidAmounts => importEntryOrEntryLineSerializable.PaidAmounts;

		public IEnumerable<IChargesIn5WN> RefundAmounts => importEntryOrEntryLineSerializable.RefundAmounts;
	}
}
