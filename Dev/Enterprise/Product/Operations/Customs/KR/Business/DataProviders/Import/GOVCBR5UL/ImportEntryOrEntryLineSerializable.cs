using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlRoot("ImportEntryOrEntryLine")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ImportEntryOrEntryLineSerializable : IImportEntryOrEntryLine
	{
		public DateTime PaidDate { get; set; }

		public ChargesSerializable PaidAmounts { get; set; }

		[XmlElement("RefundAmounts")]
		public Collection<ChargesIn5WNSerializable> RefundAmounts
		{
			get
			{
				return _refundAmounts;
			}
			set
			{
				_refundAmounts = value;
			}
		}

		[XmlIgnore]
		Collection<ChargesIn5WNSerializable> _refundAmounts;

		ZDateTime IImportEntryOrEntryLine.PaidDate => PaidDate;

		ICharges IImportEntryOrEntryLine.PaidAmounts => PaidAmounts;

		IEnumerable<IChargesIn5WN> IImportEntryOrEntryLine.RefundAmounts => RefundAmounts;
	}
}
