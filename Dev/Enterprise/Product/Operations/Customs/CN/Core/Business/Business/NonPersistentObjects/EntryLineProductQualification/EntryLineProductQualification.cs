using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class EntryLineProductQualification : NonPersistentBusinessObject, ICIQProductQualification
	{
		public EntryLineProductQualification(CusEntryLine entryLine, IEnumerable<CIQProductQualification> productQualification, int sequence)
		{
			EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
			qualifications = Argument.NotNull(productQualification, nameof(productQualification));
			randomQualification = Argument.NotNull(productQualification.FirstOrDefault(), nameof(randomQualification), "productQualification should contains at least one element");
			Sequence = sequence;
		}
		public readonly CusEntryLine EntryLine;
		readonly IEnumerable<CIQProductQualification> qualifications;
		readonly CIQProductQualification randomQualification;

		public ZInt Sequence { get; private set; }
		public ZShort EntryLineNo => EntryLine.EntryLineNo;

		public ZString DocumentType => randomQualification.CSI_Code;
		public ZString DocumentNumber => randomQualification.CSI_ReferenceNumber;
		public ZInt LineNumber => randomQualification.CSI_LineNo;
		public ZDecimal Quantity => qualifications.Sum(x => x.CSI_Quantity);
		public ZString UnitOfQuantity => randomQualification.CSI_UnitOfQuantity;
		public bool SupportsVIN => randomQualification.SupportsVIN;

		public override string ToString()
		{
			return FormattableString.Invariant($"{DocumentType}:{DocumentNumber}/{LineNumber}/{Quantity} {UnitOfQuantity}");
		}

		public EntryLineVINDataCollection VINs
		{
			get
			{
				if (fVINs == null)
				{
					fVINs = new EntryLineVINDataCollection(this);
					fVINs.Load();
				}
				return fVINs;
			}
		}
		EntryLineVINDataCollection fVINs;
	}
}
