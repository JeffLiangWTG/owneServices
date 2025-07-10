using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.NCTS.Business;
using Enterprise.Customs.Business;

namespace CargoWise.Customs.BE.NCTS.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This is currently not used but I keep it just in case it's needed")]
	public class CusAuthorizationProvider : IAuthorization
	{
		readonly CusAuthorisationHeader authorisationHeader;

		public CusAuthorizationProvider(CusAuthorisationHeader authorisationHeader, ZInt sequenceNumber)
		{
			this.authorisationHeader = Argument.NotNull(authorisationHeader, nameof(authorisationHeader));
			SequenceNumber = sequenceNumber.ToString();
		}

		public string SequenceNumber { get; }

		public string Type => CachedValueHelper.GetValue(ref typeCached, () =>
		{
			var type = authorisationHeader.CPH_Type;
			switch (type)
			{
				case Constants.CusPermitHeaderTypes.ACR:
					return Constants.AuthorisationTypes.C521;
				case Constants.CusPermitHeaderTypes.SSE:
					return Constants.AuthorisationTypes.C523;
				case Constants.CusPermitHeaderTypes.TransitOperation:
					return Constants.AuthorisationTypes.C524;
				case CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit:
					return Constants.AuthorisationTypes.C522;
			}
			return type;
		});
		CachedValue<string> typeCached;

		public string ReferenceNumber => authorisationHeader.CPH_Number;

		public string HolderOfAuthorisation => null;
	}
}
