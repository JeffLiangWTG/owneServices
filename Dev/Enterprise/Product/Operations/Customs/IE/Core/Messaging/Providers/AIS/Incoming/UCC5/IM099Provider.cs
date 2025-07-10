using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM099;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM099Provider
	{
		public IM099Provider(Im099 xmlObject)
		{
			declaration = Argument.NotNull(xmlObject.Declaration, nameof(xmlObject.Declaration));
		}
		readonly DeclarationType declaration;

		public ZString LocalReferenceNumber => declaration.Lrn25;

		public ZDateTime DateLimitOfResponse => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(declaration.DateLimitOfResponse);

		public ZString Remarks => declaration.Remarks;

		public ZString CustomsOfficeLodgement => declaration.CustomsOffices?.CustomsOfficeLodgement;
	}
}
