using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class RepresentativeProvider : PartyProvider, IRepresentative
	{
		public static RepresentativeProvider New(JobDeclaration declaration)
			=> declaration?.Representative is OrgAddress orgAddress
				? new RepresentativeProvider(orgAddress, orgAddress.GetEORI(), declaration.JE_DeclarantType)
				: null;

		RepresentativeProvider(IDocAddress orgAddress, string id, string status) : base(orgAddress, id)
		{
			Status = MapStatusToCustomsValue(status);
		}

		public string Status { get; }

		string MapStatusToCustomsValue(string cw1Code)
		{
			switch (cw1Code)
			{
				case RepresentationTypeList.Codes._2Direct:
					return Constants.RepresentationTypeList.Direct;
				case RepresentationTypeList.Codes._3Indirect:
					return Constants.RepresentationTypeList.Indirect;
				default:
					return null;
			}
		}
	}
}
