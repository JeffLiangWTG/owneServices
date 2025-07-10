using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC231C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC231CProvider
	{
		Cc231CType XmlObject { get; }

		public CC231CProvider(Cc231CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString HolderIdentificationNumber => XmlObject.HolderOfTheTransitProcedure?.IdentificationNumber ?? ZString.Empty;

		public ZString GRN => XmlObject.GuaranteeReference?.Grn ?? ZString.Empty;

		public ZDate InvalidityDate => new ZDate(XmlObject.GuaranteeReference?.InvalidityDate);

		public ZString InvalidityReasonCode => XmlObject.GuaranteeReference?.InvalidityReasonCode ?? ZString.Empty;

		public ZString InvalidityReasonText => XmlObject.GuaranteeReference?.InvalidityReasonText ?? ZString.Empty;

		public ZString CustomsOfficeOfGuarantee => XmlObject.GuaranteeReference?.CustomsOfficeOfGuarantee?.ReferenceNumber ?? ZString.Empty;
	}
}
