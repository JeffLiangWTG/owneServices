using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSErrorResponse
	{
		public ZString FieldInError { get; set; }

		public ZString ErrorReason { get; set; }

		public ZString DisplayError => FieldInError.IsEmpty ? ErrorReason : ZString.Format("{0} - {1}", FieldInError, ErrorReason);
	}
}
